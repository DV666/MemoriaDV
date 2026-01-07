using Memoria.Data;
using Memoria.Prime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Memoria.EchoS
{
    public static class BattleScriptParser
    {
        public static FileSystemWatcher watcher;
        public static bool Loading;
        private const String StuffListedPath = "[Tsunamods] Echo-S 9/BattleLines.tsv";

        public static IEnumerable<LineEntry> LoadLines()
        {
            Loading = true;
            List<LineEntry> lines = new List<LineEntry>();
            List<LineEntry> customLines = new List<LineEntry>();
            Dictionary<int, string> chainLinks = new Dictionary<int, string>();
            Dictionary<int, string> customChainLinks = new Dictionary<int, string>();

            Stream stream = null;
            string fullPath = null;

            string assetPath = AssetManager.SearchAssetOnDisc(StuffListedPath, false, false);

            if (!string.IsNullOrEmpty(assetPath))
            {
                fullPath = Path.Combine(Environment.CurrentDirectory, assetPath);
            }
            else
            {
                string directPath = Path.Combine(Environment.CurrentDirectory, StuffListedPath);
                if (File.Exists(directPath))
                {
                    fullPath = directPath;
                }
            }

            if (!string.IsNullOrEmpty(fullPath) && File.Exists(fullPath))
            {
                Log.Message($"Loading external database '{fullPath}'");
                try
                {
                    stream = File.OpenRead(fullPath);

                    if (watcher == null)
                    {
                        watcher = new FileSystemWatcher(Path.GetDirectoryName(fullPath), Path.GetFileName(fullPath))
                        {
                            NotifyFilter = NotifyFilters.LastWrite,
                            IncludeSubdirectories = false
                        };
                        watcher.Changed += (sender, e) =>
                        {
                            if (e.ChangeType != WatcherChangeTypes.Changed || Loading) return;
                            BattleSystem.Lines = LoadLines().ToArray();
                        };
                        watcher.EnableRaisingEvents = true;
                    }
                }
                catch (Exception ex)
                {
                    Log.Message($"Failed to open file '{fullPath}': {ex.Message}");
                    stream = null;
                }
            }
            else
            {
                Log.Message($"[BattleScriptParser] File not found: '{StuffListedPath}'. Please check the path or mod installation.");
            }

            if (stream == null)
            {
                Loading = false;
                yield break;
            }

            using (StreamReader reader = new StreamReader(stream))
            {
                reader.ReadLine();
                int lineNumber = 1;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lineNumber++;
                    if (string.IsNullOrEmpty(line) || line.Trim().Length == 0) continue;

                    string[] columns = line.Split('\t');
                    if (columns.Length < 36) continue;

                    BattleSpeakerEx speaker = ParseSpeaker(columns[0]);
                    if (speaker == null)
                    {
                        Log.Message($"Speaker missing or invalid at line {lineNumber}");
                        continue;
                    }

                    if (string.IsNullOrEmpty(columns[2]))
                    {
                        Log.Message($"Path missing {lineNumber}");
                        continue;
                    }

                    string path = columns[2];
                    if (!path.Contains("/") && speaker.playerId != CharacterId.NONE)
                    {
                        path = $"{speaker.playerId}/{columns[2]}";
                    }

                    ParseBattleIds(columns[13], out int[] bIds, out bool bBlacklist);

                    LineEntry entry = new LineEntry
                    {
                        Path = path,
                        ChainId = -1,
                        When = ParseMoments(columns[5]),
                        Speaker = speaker,
                        Target = ParseSpeaker(columns[9]),
                        Priority = ParseInt32(columns[6], 0),
                        Weight = ParseInt32(columns[7], 100) / 100f,
                        BattleIds = bIds,
                        BattleIdIsBlacklist = bBlacklist,
                        ScenarioMin = ParseInt32(columns[14], 0),
                        ScenarioMax = ParseInt32(columns[15], 0),
                        Statuses = ParseEnumMulti<BattleStatusId>(columns[10])
                    };

                    string commandIdRaw = columns[8].Trim();
                    if (!string.IsNullOrEmpty(commandIdRaw))
                    {
                        if (commandIdRaw.StartsWith("!"))
                        {
                            entry.CommandIdIsBlacklist = true;
                            commandIdRaw = commandIdRaw.Substring(1).Trim();
                        }
                        entry.CommandId = ParseEnumMulti<BattleCommandId>(commandIdRaw);
                    }

                    string abilitiesRaw = columns[12].Trim();
                    if (!string.IsNullOrEmpty(abilitiesRaw))
                    {
                        if (abilitiesRaw.StartsWith("!"))
                        {
                            entry.AbilitiesIsBlacklist = true;
                            abilitiesRaw = abilitiesRaw.Substring(1).Trim();
                        }
                        entry.Abilities = ParseAbilities(abilitiesRaw);
                    }

                    if (entry.When == null)
                    {
                        Log.Message($"Moment missing or invalid at line {lineNumber}");
                        continue;
                    }

                    if (columns[1].Length > 0)
                    {
                        List<BattleSpeakerEx> withList = new List<BattleSpeakerEx>();
                        string[] withParts = columns[1].Split(',');
                        foreach (string part in withParts)
                        {
                            BattleSpeakerEx withSpeaker = ParseSpeaker(part.Trim());
                            if (withSpeaker != null) withList.Add(withSpeaker);
                        }
                        if (withList.Count > 0) entry.With = withList.ToArray();
                    }

                    if (columns[16].Length > 0)
                    {
                        BattleCalcFlags[] ctxFlags = ParseEnumMulti<BattleCalcFlags>(columns[16]);
                        entry.ContextFlags = 0;
                        if (ctxFlags != null)
                        {
                            foreach (BattleCalcFlags f in ctxFlags) entry.ContextFlags |= f;
                        }
                        else
                        {
                            Log.Message($"Couldn't parse Context Flags '{columns[16]}' at line {lineNumber}");
                        }
                    }

                    for (ushort i = 0; i < 22; i++)
                    {
                        if (columns[17 + i].Length == 0)
                        {
                            entry.Flags |= (LineEntryFlag)(1U << i);
                        }
                    }

                    if (columns[11].Length > 0)
                    {
                        entry.Items = ParseEnumMulti<RegularItem>(columns[11]);
                        if (entry.Items == null) Log.Message($"Couldn't parse items '{columns[11]}' at line {lineNumber}");
                    }

                    string textVal = columns[3].Trim();
                    if (textVal.Length > 0) entry.Text = textVal;

                    if (entry.When.Contains((BattleVoice.BattleMoment)BattleMomentEx.Custom))
                    {
                        if (columns[4].Length > 0) customChainLinks.Add(customLines.Count, columns[4]);
                        customLines.Add(entry);
                    }
                    else
                    {
                        if (columns[4].Length > 0) chainLinks.Add(lines.Count, columns[4]);
                        lines.Add(entry);
                    }
                }
            }

            foreach (var link in chainLinks)
            {
                string targetPath = link.Value;
                for (int k = 0; k < lines.Count; k++)
                {
                    if (lines[k].Path == targetPath)
                    {
                        LineEntry update = lines[link.Key];
                        update.ChainId = k;
                        lines[link.Key] = update;
                        break;
                    }
                }
                if (lines[link.Key].ChainId < 0) Log.Message($"Couldn't find next line in the chain '{targetPath}'");
            }

            foreach (var link in customChainLinks)
            {
                string targetPath = link.Value;
                for (int l = 0; l < customLines.Count; l++)
                {
                    if (customLines[l].Path == targetPath)
                    {
                        LineEntry update = customLines[link.Key];
                        update.ChainId = l;
                        customLines[link.Key] = update;
                        break;
                    }
                }
                if (customLines[link.Key].ChainId < 0) Log.Message($"Couldn't find next line in the chain '{targetPath}'");
            }

            Log.Message($"Total lines successfully loaded '{lines.Count + customLines.Count}'");
            BattleSystem.CustomLineStart = lines.Count;

            foreach (var lineItem in lines) yield return lineItem;
            foreach (var customItem in customLines) yield return customItem;

            Loading = false;
        }

        private static int ParseInt32(string value, int defaultValue)
        {
            if (value.Length == 0) return defaultValue;
            if (!int.TryParse(value, out int result)) LogEchoS.Debug($"Couldn't parse '{value}'");
            return result;
        }

        private static T ParseEnum<T>(string value, T defaultValue) where T : Enum
        {
            if (value.Length == 0) return defaultValue;
            try
            {
                return (T)Enum.Parse(typeof(T), value);
            }
            catch
            {
                Log.Message($"Couldn't parse {typeof(T).Name} '{value}'");
                return defaultValue;
            }
        }

        private static T[] ParseEnumMulti<T>(string value) where T : Enum
        {
            if (value.Length == 0) return null;
            string[] parts = value.Split(',');
            List<T> list = new List<T>();
            try
            {
                foreach (string s in parts)
                {
                    list.Add((T)Enum.Parse(typeof(T), s.Trim()));
                }
                return list.ToArray();
            }
            catch
            {
                Log.Message($"Couldn't parse {typeof(T).Name} multi '{value}'");
                return null;
            }
        }

        private static void ParseBattleIds(string value, out int[] ids, out bool isBlacklist)
        {
            ids = null;
            isBlacklist = false;

            if (string.IsNullOrEmpty(value) || value.Trim().Length == 0) return;

            string content = value.Trim();

            if (content.StartsWith("!"))
            {
                isBlacklist = true;
                content = content.Substring(1);
            }

            string[] parts = content.Split(',');
            List<int> list = new List<int>();

            foreach (string s in parts)
            {
                if (int.TryParse(s.Trim(), out int id))
                {
                    list.Add(id);
                }
            }

            if (list.Count > 0) ids = list.ToArray();
        }

        private static BattleAbilityId[] ParseAbilities(string value)
        {
            if (value.Length == 0) return null;
            string[] parts = value.Split(',');
            List<BattleAbilityId> list = new List<BattleAbilityId>();
            try
            {
                foreach (string s in parts)
                {
                    string trimmed = s.Trim();
                    if (int.TryParse(trimmed, out int id)) list.Add((BattleAbilityId)id);
                    else list.Add((BattleAbilityId)Enum.Parse(typeof(BattleAbilityId), trimmed));
                }
                return list.ToArray();
            }
            catch
            {
                Log.Message($"Couldn't parse BattleAbilityId '{value}'");
                return null;
            }
        }

        private static BattleVoice.BattleMoment[] ParseMoments(string value)
        {
            if (value.Length == 0) return null;
            List<BattleVoice.BattleMoment> list = new List<BattleVoice.BattleMoment>();
            string[] parts = value.Split(',');
            foreach (string p in parts)
            {
                BattleVoice.BattleMoment m = ParseMoment(p.Trim());
                if ((int)m == 0 && p.Trim() != "0")
                {
                    Log.Message($"Couldn't parse BattleMoment '{p.Trim()}'");
                    return null;
                }
                list.Add(m);
            }
            return list.Count > 0 ? list.ToArray() : null;
        }

        private static BattleVoice.BattleMoment ParseMoment(string value)
        {
            try
            {
                return (BattleVoice.BattleMoment)Enum.Parse(typeof(BattleVoice.BattleMoment), value);
            }
            catch
            {
                string lower = value.ToLower();
                foreach (PropertyInfo prop in typeof(BattleMomentEx).GetProperties())
                {
                    if (prop.Name.ToLower() == lower)
                    {
                        return (BattleVoice.BattleMoment)prop.GetValue(null, null);
                    }
                }
            }
            return 0;
        }

        private static BattleSpeakerEx ParseSpeaker(string value)
        {
            if (value.Length == 0) return null;
            BattleSpeakerEx speaker = new BattleSpeakerEx();

            if (value.StartsWith("$"))
            {
                speaker.CheckCanTalk = false;
                value = value.Substring(1);
            }
            if (value.StartsWith("!"))
            {
                speaker.CheckIsPlayer = false;
                value = value.Substring(1);
            }
            if (value.StartsWith("\\"))
            {
                speaker.Without = true;
                value = value.Substring(1);
            }

            string[] parts = value.Trim().Split(':');
            if (parts.Length == 1)
            {
                speaker.playerId = ParseEnum(parts[0], CharacterId.NONE);
                if (speaker.playerId == CharacterId.NONE) return null;
            }
            else if (parts.Length > 1)
            {
                string statusPart = null;
                if (parts[0].Length > 0 && !int.TryParse(parts[0], out speaker.enemyBattleId))
                {
                    speaker.playerId = ParseEnum(parts[0], CharacterId.NONE);
                    if (speaker.playerId == CharacterId.NONE) return null;
                    statusPart = parts[1];
                }

                if (statusPart == null && parts[1].Length > 0)
                {
                    if (int.TryParse(parts[1], out int modelId))
                    {
                        if (!FF9BattleDB.GEO.ContainsKey(modelId))
                        {
                            Log.Message($"Invalid model id '{modelId}'");
                            return null;
                        }
                        speaker.enemyModelId = modelId;
                    }
                    else if (!FF9BattleDB.GEO.TryGetKey(parts[1], out speaker.enemyModelId))
                    {
                        Log.Message($"Invalid model name '{parts[1]}'");
                        return null;
                    }
                }

                if (parts.Length > 2) statusPart = parts[2];
                if (statusPart != null)
                {
                    speaker.Status = ParseEnum(statusPart, (BattleStatusId)(-1));
                }
            }
            return speaker;
        }
    }
}
