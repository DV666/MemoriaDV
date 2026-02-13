using Memoria.Prime;
using NCalc;
using NCalc.Domain;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Global.Sound.SaXAudio
{
    public class AudioEffectManager
    {
        private const String FILENAME = "AudioEffects.txt";

        private static Dictionary<Int32, EffectPreset> fieldIDPresets = new Dictionary<Int32, EffectPreset>();
        private static Dictionary<Int32, EffectPreset> battleIDPresets = new Dictionary<Int32, EffectPreset>();
        private static Dictionary<Int32, EffectPreset> battleBgIDPresets = new Dictionary<Int32, EffectPreset>();
        private static Dictionary<String, EffectPreset> resourceIDPresets = new Dictionary<String, EffectPreset>();
        private static List<EffectPreset> conditionalPresets = new List<EffectPreset>();
        private static Dictionary<String, EffectPreset> unlistedPresets = new Dictionary<String, EffectPreset>();
        private static EffectPreset? currentPreset = null;

        // Cache the structure (LogicalExpression) instead of the Expression object itself.
        // This is Thread-Safe because the structure is read-only.
        private static Dictionary<String, LogicalExpression> parsedCache = new Dictionary<String, LogicalExpression>();

        // Simple lock for list access
        private static readonly object listLock = new object();

        private static Boolean initialized = false;
        private static FileSystemWatcher watcher = null;

        public static Boolean IsSaXAudio { get; private set; } = ISdLibAPIProxy.Instance is SdLibAPIWithSaXAudio;

        // Helper for old .NET versions lacking String.IsNullOrWhiteSpace
        private static bool IsNullOrWhiteSpace(String value)
        {
            if (String.IsNullOrEmpty(value)) return true;
            return value.Trim().Length == 0;
        }

        public static void Initialize()
        {
            new Thread(() =>
            {
                lock (listLock)
                {
                    if (!initialized) Init();
                }
            }).Start();
        }

        public static void ApplyFieldEffects(Int32 fieldID)
        {
            if (!IsSaXAudio) return;

            lock (listLock)
            {
                if (!initialized) Init();
            }

            if (fieldIDPresets.TryGetValue(fieldID, out EffectPreset preset) && ApplyPreset(ref preset))
            {
                currentPreset = preset;
                Log.Message($"[AudioEffectManager] Applied preset '{preset.Name}' to field {fieldID}");
                return;
            }
            ResetEffects();
        }

        public static void ApplyBattleEffects(Int32 battleID, Int32 battleBgID)
        {
            if (!IsSaXAudio) return;

            lock (listLock)
            {
                if (!initialized) Init();
            }

            if (battleIDPresets.TryGetValue(battleID, out EffectPreset preset) && ApplyPreset(ref preset))
            {
                currentPreset = preset;
                Log.Message($"[AudioEffectManager] Applied preset '{preset.Name}' to battle {battleID}");
                return;
            }

            if (battleBgIDPresets.TryGetValue(battleBgID, out preset) && ApplyPreset(ref preset))
            {
                currentPreset = preset;
                Log.Message($"[AudioEffectManager] Applied preset '{preset.Name}' to battle background {battleBgID}");
                return;
            }
            ResetEffects();
        }

        public static EffectPreset? GetPreset(SoundProfile profile, Int32 bus)
        {
            if (!IsSaXAudio || !initialized)
                return null;

            EffectPreset? preset = null;

            // 1. Search by ResourceID (Fast)
            if (resourceIDPresets.TryGetValue(profile.ResourceID, out EffectPreset foundPreset))
            {
                preset = foundPreset;
                if (IsNullOrWhiteSpace(preset.Value.Condition) || EvaluatePresetCondition(profile, preset.Value.Condition, preset.Value.Name))
                    return preset;
            }

            // 2. Search by Conditions
            // Lock list access to prevent concurrent modification issues during Init
            lock (listLock)
            {
                if (conditionalPresets.Count > 0)
                {
                    for (Int32 i = 0; i < conditionalPresets.Count; i++)
                    {
                        preset = conditionalPresets[i];

                        // Check Layers BEFORE NCalc
                        if (preset.Value.Layers != EffectPreset.Layer.None && !preset.Value.IsBusInLayers(bus))
                            continue;

                        if (EvaluatePresetCondition(profile, preset.Value.Condition, preset.Value.Name))
                            return preset;
                    }
                }
            }

            // 3. Current Preset
            preset = currentPreset;
            if (preset != null && preset.Value.IsBusInLayers(bus) && !IsNullOrWhiteSpace(preset.Value.Condition))
            {
                if (!EvaluatePresetCondition(profile, preset.Value.Condition, preset.Value.Name))
                    return new EffectPreset();
            }

            return null;
        }

        public static EffectPreset? GetUnlistedPreset(String presetName)
        {
            lock (listLock)
            {
                if (!IsSaXAudio || !initialized || !unlistedPresets.ContainsKey(presetName)) return null;
                return unlistedPresets[presetName];
            }
        }

        public static EffectPreset? FindPreset(String presetName)
        {
            if (!IsSaXAudio || !initialized) return null;

            lock (listLock)
            {
                if (unlistedPresets.ContainsKey(presetName)) return unlistedPresets[presetName];
                foreach (var kvp in fieldIDPresets) if (kvp.Value.Name == presetName) return kvp.Value;
                foreach (var kvp in battleBgIDPresets) if (kvp.Value.Name == presetName) return kvp.Value;
                foreach (var kvp in battleIDPresets) if (kvp.Value.Name == presetName) return kvp.Value;
                foreach (var kvp in resourceIDPresets) if (kvp.Value.Name == presetName) return kvp.Value;

                for (Int32 i = 0; i < conditionalPresets.Count; i++)
                {
                    if (conditionalPresets[i].Name == presetName)
                        return conditionalPresets[i];
                }
            }
            return null;
        }

        // Secure compilation of the structure
        private static void PrecacheExpression(String condition)
        {
            if (IsNullOrWhiteSpace(condition)) return;
            if (parsedCache.ContainsKey(condition)) return;

            try
            {
                // Expression.Compile analyzes text and returns a logical tree (LogicalExpression)
                // This is the heavy operation we only want to do once.
                LogicalExpression logicalExp = Expression.Compile(condition, false);
                parsedCache[condition] = logicalExp;
            }
            catch (Exception e)
            {
                Log.Error($"[AudioEffectManager] Failed to compile condition: '{condition}'. Error: {e.Message}");
            }
        }

        public static Boolean EvaluatePresetCondition(SoundProfile profile, String condition, String presetName)
        {
            // SECURITY 1: If empty, exit IMMEDIATELY. No NCalc.
            if (IsNullOrWhiteSpace(condition)) return true;

            LogicalExpression logicalExp;

            // Get compiled structure
            if (!parsedCache.TryGetValue(condition, out logicalExp))
            {
                // Fallback
                PrecacheExpression(condition);
                if (!parsedCache.TryGetValue(condition, out logicalExp))
                    return true;
            }

            try
            {
                // SECURITY 2: Create a NEW Expression instance for each evaluation
                // using the compiled structure (logicalExp).
                // This guarantees parameters of THIS sound do not pollute other sounds (Thread Isolation).
                Expression c = new Expression(logicalExp);

                c.EvaluateFunction += NCalcUtility.commonNCalcFunctions;
                c.EvaluateParameter += NCalcUtility.commonNCalcParameters;
                c.EvaluateParameter += NCalcUtility.worldNCalcParameters;

                // Define parameters ONLY for this isolated instance
                c.Parameters["SoundIndex"] = profile.SoundIndex;
                c.Parameters["ResourceID"] = profile.ResourceID;
                c.Parameters["SoundProfileType"] = profile.SoundProfileType;
                c.Parameters["SoundProfileType_Default"] = SoundProfileType.Default;
                c.Parameters["SoundProfileType_Music"] = SoundProfileType.Music;
                c.Parameters["SoundProfileType_SoundEffect"] = (Byte)SoundProfileType.SoundEffect;
                c.Parameters["SoundProfileType_MovieAudio"] = SoundProfileType.MovieAudio;
                c.Parameters["SoundProfileType_Song"] = SoundProfileType.Song;
                c.Parameters["SoundProfileType_Sfx"] = SoundProfileType.Sfx;
                c.Parameters["SoundProfileType_Voice"] = SoundProfileType.Voice;

                // Isolated evaluation
                var result = c.Evaluate();
                if (NCalcUtility.EvaluateNCalcCondition(result))
                    return true;
            }
            catch (Exception e)
            {
                // SECURITY 3: If NCalc crashes, catch the error to avoid crashing the game
                Log.Error($"[AudioEffectManager] Error evaluating '{condition}' in preset '{presetName}': {e.Message}");
                return false;
            }

            return false;
        }

        public static void ApplyPresetOnSound(EffectPreset preset, Int32 soundID, String soundName, Single fade = 0)
        {
            if (!IsSaXAudio) return;

            // Double check safe
            if (!initialized)
            {
                lock (listLock)
                {
                    if (!initialized) Init();
                }
            }

            if (preset.Effects == EffectPreset.Effect.None) return;

            Boolean reverb = (preset.Effects & EffectPreset.Effect.Reverb) != 0;
            Boolean eq = (preset.Effects & EffectPreset.Effect.Eq) != 0;
            Boolean echo = (preset.Effects & EffectPreset.Effect.Echo) != 0;
            Boolean volume = (preset.Effects & EffectPreset.Effect.Volume) != 0;

            if (reverb) SaXAudio.SetReverb(soundID, preset.Reverb, fade);
            else SaXAudio.RemoveReverb(soundID, fade);
            if (eq) SaXAudio.SetEq(soundID, preset.Eq, fade);
            else SaXAudio.RemoveEq(soundID, fade);
            if (echo) SaXAudio.SetEcho(soundID, preset.Echo, fade);
            else SaXAudio.RemoveEcho(soundID, fade);

            if (volume) SaXAudio.SetVolume(soundID, SaXAudio.GetVolume(soundID) * preset.Volume, fade);
        }

        public static void ResetEffects()
        {
            SdLibAPIWithSaXAudio saXAudio = ISdLibAPIProxy.Instance as SdLibAPIWithSaXAudio;
            if (saXAudio == null) return;

            SaXAudio.RemoveReverb(saXAudio.BusMusic, 0, true);
            SaXAudio.RemoveEq(saXAudio.BusMusic, 0, true);
            SaXAudio.RemoveEcho(saXAudio.BusMusic, 0, true);
            SaXAudio.SetVolume(saXAudio.BusMusic, 1f, 0, true);

            SaXAudio.RemoveReverb(saXAudio.BusAmbient, 0, true);
            SaXAudio.RemoveEq(saXAudio.BusAmbient, 0, true);
            SaXAudio.RemoveEcho(saXAudio.BusAmbient, 0, true);
            SaXAudio.SetVolume(saXAudio.BusAmbient, 1f, 0, true);

            SaXAudio.RemoveReverb(saXAudio.BusSoundEffect, 0, true);
            SaXAudio.RemoveEq(saXAudio.BusSoundEffect, 0, true);
            SaXAudio.RemoveEcho(saXAudio.BusSoundEffect, 0, true);
            SaXAudio.SetVolume(saXAudio.BusSoundEffect, 1f, 0, true);

            SaXAudio.RemoveReverb(saXAudio.BusVoice, 0, true);
            SaXAudio.RemoveEq(saXAudio.BusVoice, 0, true);
            SaXAudio.RemoveEcho(saXAudio.BusVoice, 0, true);
            SaXAudio.SetVolume(saXAudio.BusVoice, 1f, 0, true);
        }

        private static void Init()
        {
            fieldIDPresets.Clear();
            battleIDPresets.Clear();
            battleBgIDPresets.Clear();
            conditionalPresets.Clear();
            unlistedPresets.Clear();

            lock (parsedCache)
            {
                parsedCache.Clear();
            }

            foreach (AssetManager.AssetFolder folder in AssetManager.FolderLowToHigh)
            {
                if (folder.TryFindAssetInModOnDisc(FILENAME, out String fullPath))
                {
                    var presets = LoadPresets($"{Path.GetDirectoryName(fullPath)}\\");
                    foreach (var preset in presets.Values)
                    {
                        // Pre-compile logical structure
                        if (!IsNullOrWhiteSpace(preset.Condition))
                        {
                            PrecacheExpression(preset.Condition);
                        }

                        Boolean listed = false;
                        foreach (Int32 fieldID in preset.FieldIDs)
                        {
                            fieldIDPresets[fieldID] = preset;
                            fieldIDPresets[fieldID].RemoveIDs();
                            listed = true;
                        }
                        foreach (Int32 battleID in preset.BattleIDs)
                        {
                            battleIDPresets[battleID] = preset;
                            battleIDPresets[battleID].RemoveIDs();
                            listed = true;
                        }
                        foreach (Int32 battleBgID in preset.BattleBgIDs)
                        {
                            battleBgIDPresets[battleBgID] = preset;
                            battleBgIDPresets[battleBgID].RemoveIDs();
                            listed = true;
                        }
                        foreach (String resourceID in preset.ResourceIDs)
                        {
                            resourceIDPresets[resourceID] = preset;
                            resourceIDPresets[resourceID].RemoveIDs();
                            listed = true;
                        }
                        if (!listed)
                        {
                            if (!IsNullOrWhiteSpace(preset.Condition))
                                conditionalPresets.Add(preset);
                            else
                                unlistedPresets[preset.Name] = preset;
                        }
                    }
                }
            }

            if (watcher == null)
            {
                watcher = new FileSystemWatcher("./", $"*{FILENAME}");
                watcher.IncludeSubdirectories = true;
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                watcher.Changed += (sender, e) =>
                {
                    if (e.ChangeType != WatcherChangeTypes.Changed) return;
                    initialized = false;
                    Log.Message($"'{e.FullPath}' changed");
                };
                watcher.EnableRaisingEvents = true;
            }
            initialized = true;
        }

        private static Boolean ApplyPreset(ref EffectPreset preset)
        {
            SdLibAPIWithSaXAudio saXAudio = ISdLibAPIProxy.Instance as SdLibAPIWithSaXAudio;
            if (saXAudio == null) return false;

            if ((preset.Layers & EffectPreset.Layer.Music) != 0) ApplyPresetOnBus(ref preset, saXAudio.BusMusic);
            if ((preset.Layers & EffectPreset.Layer.Ambient) != 0) ApplyPresetOnBus(ref preset, saXAudio.BusAmbient);
            if ((preset.Layers & EffectPreset.Layer.SoundEffect) != 0) ApplyPresetOnBus(ref preset, saXAudio.BusSoundEffect);
            if ((preset.Layers & EffectPreset.Layer.Voice) != 0) ApplyPresetOnBus(ref preset, saXAudio.BusVoice);

            return true;
        }

        private static void ApplyPresetOnBus(ref EffectPreset preset, Int32 bus)
        {
            Boolean reverb = (preset.Effects & EffectPreset.Effect.Reverb) != 0;
            Boolean eq = (preset.Effects & EffectPreset.Effect.Eq) != 0;
            Boolean echo = (preset.Effects & EffectPreset.Effect.Echo) != 0;
            Boolean volume = (preset.Effects & EffectPreset.Effect.Volume) != 0;

            if (reverb) SaXAudio.SetReverb(bus, preset.Reverb, 0, true);
            else SaXAudio.RemoveReverb(bus, 0, true);
            if (eq) SaXAudio.SetEq(bus, preset.Eq, 0, true);
            else SaXAudio.RemoveEq(bus, 0, true);
            if (echo) SaXAudio.SetEcho(bus, preset.Echo, 0, true);
            else SaXAudio.RemoveEcho(bus, 0, true);
            if (volume) SaXAudio.SetVolume(bus, preset.Volume, 0, true);
            else SaXAudio.SetVolume(bus, 1f, 0, true);
        }

        public struct EffectPreset
        {
            public enum Effect : Byte
            {
                None = 0,
                Reverb = 1,
                Eq = 1 << 1,
                Echo = 1 << 2,
                Volume = 1 << 3,
                All = Reverb | Eq | Echo | Volume
            }

            public enum Layer : Byte
            {
                None = 0,
                Music = 1,
                Ambient = 1 << 1,
                SoundEffect = 1 << 2,
                Voice = 1 << 3,
                All = Music | Ambient | SoundEffect | Voice
            }

            public Boolean IsBusInLayers(Int32 bus)
            {
                if (!IsSaXAudio || Layers == Layer.None)
                    return false;

                SdLibAPIWithSaXAudio saXAudio = ISdLibAPIProxy.Instance as SdLibAPIWithSaXAudio;
                if (saXAudio == null) return false;

                if ((Layers & Layer.Music) != 0 && bus == saXAudio.BusMusic)
                    return true;
                if ((Layers & Layer.Ambient) != 0 && bus == saXAudio.BusAmbient)
                    return true;
                if ((Layers & Layer.SoundEffect) != 0 && bus == saXAudio.BusSoundEffect)
                    return true;
                if ((Layers & Layer.Voice) != 0 && bus == saXAudio.BusVoice)
                    return true;
                return false;
            }

            public String Name = "";
            public Effect Effects = Effect.None;
            public SaXAudio.ReverbParameters Reverb = new SaXAudio.ReverbParameters();
            public SaXAudio.EqParameters Eq = new SaXAudio.EqParameters();
            public SaXAudio.EchoParameters Echo = new SaXAudio.EchoParameters();
            public Single Volume = 1f;

            public Layer Layers = Layer.None;
            public HashSet<Int32> FieldIDs = new HashSet<Int32>();
            public HashSet<String> ResourceIDs = new HashSet<String>();
            public HashSet<Int32> BattleIDs = new HashSet<Int32>();
            public HashSet<Int32> BattleBgIDs = new HashSet<Int32>();

            public String Condition = "";

            public EffectPreset() { }
            public EffectPreset(String str)
            {
                String[] tokens = str.Split(';');
                Int32 i = 0;
                Name = tokens[i++];
                Effects = (Effect)Byte.Parse(tokens[i++]);

                FieldInfo[] fields = typeof(SaXAudio.ReverbParameters).GetFields();
                object reverb = Reverb;
                foreach (FieldInfo field in fields)
                {
                    field.SetValue(reverb, Convert.ChangeType(tokens[i++], field.FieldType, CultureInfo.InvariantCulture));
                }
                Reverb = (SaXAudio.ReverbParameters)reverb;

                fields = typeof(SaXAudio.EqParameters).GetFields();
                object eq = Eq;
                foreach (FieldInfo field in fields)
                {
                    field.SetValue(eq, Convert.ChangeType(tokens[i++], field.FieldType, CultureInfo.InvariantCulture));
                }
                Eq = (SaXAudio.EqParameters)eq;

                fields = typeof(SaXAudio.EchoParameters).GetFields();
                object echo = Echo;
                foreach (FieldInfo field in fields)
                {
                    field.SetValue(echo, Convert.ChangeType(tokens[i++], field.FieldType, CultureInfo.InvariantCulture));
                }
                Echo = (SaXAudio.EchoParameters)echo;

                Volume = Single.Parse(tokens[i++]);

                if (i == tokens.Length) return;

                Layers = (Layer)Byte.Parse(tokens[i++]);

                String[] ids = tokens[i++].Split('|');
                if (ids.Length > 0)
                {
                    FieldIDs.Clear();
                    foreach (String id in ids)
                    {
                        if (id.Length > 0)
                            FieldIDs.Add(Int32.Parse(id));
                    }
                }

                ids = tokens[i++].Split('|');
                if (ids.Length > 0)
                {
                    BattleIDs.Clear();
                    foreach (String id in ids)
                    {
                        if (id.Length > 0)
                            BattleIDs.Add(Int32.Parse(id));
                    }
                }

                ids = tokens[i++].Split('|');
                if (ids.Length > 0)
                {
                    BattleBgIDs.Clear();
                    foreach (String id in ids)
                    {
                        if (id.Length > 0)
                            BattleBgIDs.Add(Int32.Parse(id));
                    }
                }

                ids = tokens[i++].Split('|');
                if (ids.Length > 0)
                {
                    ResourceIDs.Clear();
                    foreach (String id in ids)
                    {
                        if (id.Length > 0)
                            ResourceIDs.Add(id);
                    }
                }

                Condition = tokens[i++].Replace('\x0A', ';');
            }

            public override readonly String ToString()
            {
                StringBuilder builder = new StringBuilder();
                const Char separator = ';';
                builder.Append(Name);
                builder.Append(separator);
                builder.Append((Byte)Effects);

                FieldInfo[] fields = typeof(SaXAudio.ReverbParameters).GetFields();
                foreach (FieldInfo field in fields)
                {
                    builder.Append(separator);
                    builder.Append((String)Convert.ChangeType(field.GetValue(Reverb), typeof(String), CultureInfo.InvariantCulture));
                }

                fields = typeof(SaXAudio.EqParameters).GetFields();
                foreach (FieldInfo field in fields)
                {
                    builder.Append(separator);
                    builder.Append((String)Convert.ChangeType(field.GetValue(Eq), typeof(String), CultureInfo.InvariantCulture));
                }

                fields = typeof(SaXAudio.EchoParameters).GetFields();
                foreach (FieldInfo field in fields)
                {
                    builder.Append(separator);
                    builder.Append((String)Convert.ChangeType(field.GetValue(Echo), typeof(String), CultureInfo.InvariantCulture));
                }

                builder.Append(separator);
                builder.Append(Volume);

                builder.Append(separator);
                builder.Append((Byte)Layers);

                builder.Append(separator);
                builder.Append(String.Join("|", FieldIDs.Select(x => x.ToString()).ToArray()));

                builder.Append(separator);
                builder.Append(String.Join("|", BattleIDs.Select(x => x.ToString()).ToArray()));

                builder.Append(separator);
                builder.Append(String.Join("|", BattleBgIDs.Select(x => x.ToString()).ToArray()));

                builder.Append(separator);
                builder.Append(String.Join("|", ResourceIDs.ToArray()));

                builder.Append(separator);
                builder.Append(Condition.Trim().Replace(';', '\x0A'));

                return builder.ToString();
            }

            public void RemoveIDs()
            {
                FieldIDs = null;
                BattleIDs = null;
                BattleBgIDs = null;
                ResourceIDs = null;
            }
        }

        public static SortedDictionary<String, EffectPreset> LoadPresets(String modLocation, Boolean backup = false)
        {
            SortedDictionary<String, EffectPreset> presets = new SortedDictionary<String, EffectPreset>();
            String path = Path.Combine(modLocation, $"{FILENAME}{(backup ? ".bak" : "")}");
            if (File.Exists(path))
            {
                try
                {
                    String mod = Path.GetDirectoryName(modLocation);
                    String[] lines = File.ReadAllLines(path);
                    foreach (String line in lines)
                    {
                        if (line.Length == 0 || line.StartsWith("#"))
                            continue;
                        EffectPreset preset = new EffectPreset(line);
                        presets.Add(preset.Name, preset);
                    }
                    Log.Message($"[AudioEffectManager] Loaded {mod} presets");
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
            return presets;
        }

        public static void SavePresets(SortedDictionary<String, EffectPreset> presets, String modLocation, Boolean backup = false)
        {
            try
            {
                String mod = Path.GetDirectoryName(modLocation);
                List<String> lines = new List<String>();
                foreach (EffectPreset effectPreset in presets.Values)
                {
                    lines.Add(effectPreset.ToString());
                }
                String path = Path.Combine(modLocation, $"{FILENAME}{(backup ? ".bak" : "")}");
                File.WriteAllLines(path, lines.ToArray());
                Log.Message($"[AudioEffectManager] Saved {mod} presets");
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}
