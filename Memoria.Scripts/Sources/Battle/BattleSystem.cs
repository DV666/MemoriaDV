using Global.Sound.SaXAudio;
using Memoria.Assets;
using Memoria.Data;
using Memoria.Prime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Memoria.EchoS
{
    public static class BattleSystem
    {
        public static bool IsPreemptive => FF9StateSystem.Battle.FF9Battle.btl_scene.Info.StartType == battle_start_type_tags.BTL_START_FIRST_ATTACK;

        public static bool IsBackAttack => FF9StateSystem.Battle.FF9Battle.btl_scene.Info.StartType == battle_start_type_tags.BTL_START_BACK_ATTACK;

        public static bool CanPlayMoreLines => CurrentPlayingDialog < 0;

        public static Dictionary<int, AudioEffectManager.EffectPreset> PresetCache = new Dictionary<int, AudioEffectManager.EffectPreset>();
        private static bool _cacheInitialized = false;

        private static void InitializePresetCache()
        {
            if (_cacheInitialized) return;
            PresetCache.Clear();

            for (int i = 0; i < 4; i++)
            {
                foreach (CharacterId charId in new CharacterId[] { CharacterId.Zidane, CharacterId.Vivi, CharacterId.Garnet, CharacterId.Steiner, CharacterId.Freya, CharacterId.Quina, CharacterId.Eiko, CharacterId.Amarant, CharacterId.Beatrix })
                {
                    string charName = charId.ToString();
                    foreach (BattleStatus status in Enum.GetValues(typeof(BattleStatus)))
                    {
                        string key = status.ToString() + charName;
                        var preset = AudioEffectManager.GetUnlistedPreset(key);
                        if (preset != null)
                        {
                            int cacheKey = (int)status | ((int)charId << 16);
                            if (!PresetCache.ContainsKey(cacheKey))
                                PresetCache.Add(cacheKey, preset.Value);
                        }
                    }
                }
            }
            _cacheInitialized = true;
        }

        public static LineEntryFlag GetFlags(BattleCalculator calc)
        {
            LineEntryFlag flags = (LineEntryFlag)Flags;

            if (calc.Command.IsManyTarget)
                flags |= LineEntryFlag.Multi;
            else
                flags |= LineEntryFlag.Single;

            if (calc.Target.Id == calc.Caster.Id)
                flags |= LineEntryFlag.Self;

            CalcFlag targetFlags = (CalcFlag)calc.Target.Flags;
            BattleCalcFlags contextFlags = (BattleCalcFlags)calc.Context.Flags;

            if ((targetFlags & CalcFlag.Critical) != 0)
                flags |= LineEntryFlag.Crit;

            if ((targetFlags & CalcFlag.HpAlteration) != 0)
                flags |= LineEntryFlag.Hp;

            if ((targetFlags & CalcFlag.MpAlteration) != 0)
                flags |= LineEntryFlag.Mp;

            if ((contextFlags & BattleCalcFlags.Dodge) != 0)
                flags |= LineEntryFlag.Dodge;
            else if ((contextFlags & BattleCalcFlags.Miss) != 0)
                flags |= LineEntryFlag.Miss;
            else
                flags |= LineEntryFlag.Hit;

            return flags;
        }

        public static bool CommonChecks(int i, BattleVoice.BattleMoment when, uint flags, BattleUnit speaker, BattleStatusId statusException = (BattleStatusId)30)
        {
            LineEntry currentLine = Lines[i];

            if (FF9StateSystem.Battle.battleMapIndex == 296 && (CharacterId)currentLine.Speaker.playerId == CharacterId.Vivi)
            {
                bool explicitlyAllowed = false;
                if (currentLine.BattleIds != null)
                {
                    foreach (int id in currentLine.BattleIds)
                    {
                        if (id == 296)
                        {
                            explicitlyAllowed = true;
                            break;
                        }
                    }
                }
                if (!explicitlyAllowed || currentLine.BattleIdIsBlacklist)
                    return false;
            }

            if (!currentLine.When.Contains(when))
                return false;

            if (currentLine.Flags != LineEntryFlag.None && ((uint)currentLine.Flags & flags) == 0U)
                return false;

            if (currentLine.BattleIds != null && currentLine.BattleIds.Length > 0)
            {
                int currentMapId = FF9StateSystem.Battle.battleMapIndex;
                bool matchFound = false;
                foreach (int id in currentLine.BattleIds)
                {
                    if (id == currentMapId)
                    {
                        matchFound = true;
                        break;
                    }
                }

                if (currentLine.BattleIdIsBlacklist)
                {
                    if (matchFound) return false;
                }
                else
                {
                    if (!matchFound) return false;
                }
            }

            if (currentLine.ScenarioMin > 0 && GameState.ScenarioCounter < currentLine.ScenarioMin)
                return false;

            if (currentLine.ScenarioMax > 0 && GameState.ScenarioCounter > currentLine.ScenarioMax)
                return false;

            if (speaker != null)
            {
                if (currentLine.Speaker.CheckIsPlayer)
                {
                    if (!currentLine.Speaker.CheckIsCharacter(speaker))
                        return false;
                }
                else if (currentLine.With != null && currentLine.With.Length > 0 && currentLine.With[0].CheckIsPlayer && !currentLine.With[0].CheckIsCharacter(speaker))
                {
                    return false;
                }
            }

            int priority = AdjustPriority(when, currentLine.Priority);

            if (!CanTalk(currentLine.Speaker, priority, statusException))
                return false;

            for (int j = currentLine.ChainId; j >= 0; j = Lines[j].ChainId)
            {
                if (FF9StateSystem.Battle.battleMapIndex == 296 && (CharacterId)Lines[j].Speaker.playerId == CharacterId.Vivi)
                {
                    bool explicitlyAllowed = false;
                    if (Lines[j].BattleIds != null)
                    {
                        foreach (int id in Lines[j].BattleIds)
                        {
                            if (id == 296)
                            {
                                explicitlyAllowed = true;
                                break;
                            }
                        }
                    }
                    if (!explicitlyAllowed || Lines[j].BattleIdIsBlacklist)
                        return false;
                }

                if (!CanTalk(Lines[j].Speaker, priority, statusException))
                    return false;
            }

            if (currentLine.With != null)
            {
                bool hasRequiredPartner = false;
                foreach (BattleSpeakerEx withSpeaker in currentLine.With)
                {
                    BattleUnit partnerUnit = withSpeaker.FindBattleUnit();
                    if ((!withSpeaker.Without || partnerUnit == null) && partnerUnit != null && !partnerUnit.IsUnderStatus(BattleStatus.Death) && (withSpeaker.Status == BattleStatusId.None || partnerUnit.IsUnderStatus((BattleStatus)withSpeaker.Status)) && (!withSpeaker.CheckCanTalk || BattleVoice.BattleSpeaker.CheckCanSpeak(partnerUnit, priority, statusException)))
                    {
                        hasRequiredPartner = true;
                        break;
                    }
                }
                if (!hasRequiredPartner)
                    return false;
            }

            return true;
        }

        public static int GetRandomLine(BattleVoice.BattleMoment moment, LineEntryPredicate filter)
        {
            int maxPriority = int.MinValue;
            Dictionary<int, float> weights = new Dictionary<int, float>();
            float totalWeight = 0f;
            string debugInfo = "";

            for (int i = 0; i < CustomLineStart; i++)
            {
                if (CurrentPlayingChain < 0 || !Lines[CurrentPlayingChain].Speaker.Equals(Lines[i].Speaker))
                {
                    int priority = AdjustPriority(moment, Lines[i].Priority);
                    if (priority >= maxPriority && filter(i, moment))
                    {
                        float weight = Lines[i].Weight;
                        if (weight >= 0f)
                        {
                            for (int j = 0; j < PlayedLinesCount; j++)
                            {
                                int idx = (PlayedLinesPos - 1 - j + PlayedLines.Length) % PlayedLines.Length;
                                if (PlayedLines[idx] == i)
                                {
                                    float penalty = Mathf.Min(0.5f, (float)j / 20f);
                                    weight *= penalty;
                                }
                            }
                        }
                        else
                        {
                            weight = -weight;
                        }

                        if (priority > maxPriority)
                        {
                            maxPriority = priority;
                            weights.Clear();
                            totalWeight = 0f;
                            debugInfo = "";
                        }

                        weights[i] = weight;
                        totalWeight += weight;
                        debugInfo += string.Format("['{0}'({1})] ", Lines[i].Path, weight);
                    }
                }
            }

            if (totalWeight == 0f)
                return -1;

            if (weights.Count == 1 && totalWeight < 1f)
                totalWeight *= 3f;

            float roll = UnityEngine.Random.Range(0f, totalWeight);
            Log.Message(string.Format("Selected lines ({0}): {1}RNG: {2}/{3}", weights.Count, debugInfo, roll, totalWeight));

            float currentRange = 0f;
            foreach (var entry in weights)
            {
                currentRange += entry.Value;
                if (roll <= currentRange)
                    return entry.Key;
            }

            return -1;
        }

        public static void QueueLine(int i, BattleVoice.BattleMoment when)
        {
            if (i < 0) return;

            bool startPlaying = LinesQueue.Count == 0;

            if (!Lines[i].IsVerbal)
            {
                for (int j = Lines[i].ChainId; j >= 0; j = Lines[j].ChainId)
                    LinesQueue.Enqueue(new KeyValuePair<int, BattleVoice.BattleMoment>(j, (BattleVoice.BattleMoment)BattleMomentEx.Chain));

                if (startPlaying)
                {
                    LinesQueue.Enqueue(new KeyValuePair<int, BattleVoice.BattleMoment>(i, when));
                    PlayLine(i, when, PlayNextLine);
                }
                else
                {
                    PlayLine(i, when, null);
                }
            }
            else
            {
                if (LinesQueue.Count > 2)
                {
                    Log.Message("Line '" + Lines[i].Path + "' ignored. Queue full");
                    return;
                }

                LinesQueue.Enqueue(new KeyValuePair<int, BattleVoice.BattleMoment>(i, when));
                for (int j = Lines[i].ChainId; j >= 0; j = Lines[j].ChainId)
                    LinesQueue.Enqueue(new KeyValuePair<int, BattleVoice.BattleMoment>(j, (BattleVoice.BattleMoment)BattleMomentEx.Chain));

                if (startPlaying)
                    PlayLine(i, when, PlayNextLine);
                else
                    Log.Message("Queueing '" + Lines[i].Path + "'");
            }
        }

        private static void PlayNextLine()
        {
            if (LinesQueue.Count == 0) return;
            LinesQueue.Dequeue();

            if (LinesQueue.Count == 0)
            {
                Log.Message("Chain ended");
                return;
            }

            KeyValuePair<int, BattleVoice.BattleMoment> next = LinesQueue.Peek();
            Log.Message("Playing next line: " + Lines[next.Key].Path);
            PlayLine(next.Key, next.Value, PlayNextLine);
        }

        private static IEnumerator WaitForVoiceRoutine(int i, BattleVoice.BattleMoment when, Action onFinishedPlaying)
        {
            yield return new WaitForSeconds(0.1f);
            float timeout = 3.0f;

            while (timeout > 0)
            {
                yield return new WaitForSeconds(0.05f);
                timeout -= 0.05f;

                if (CurrentPlayingDialog >= 0) break;

                if (BattleVoice.GetPlayingVoicesCount() == 0)
                {
                    PlayLineNow(i, when, onFinishedPlaying);
                    yield break;
                }
            }

            Log.Message(string.Format("Cancelled queued line '{0}' ({1})", Lines[i].Path, (timeout <= 0) ? "timeout" : "dialog"));
            onFinishedPlaying?.Invoke();
        }

        private static void PlayLine(int i, BattleVoice.BattleMoment when, Action onFinishedPlaying = null)
        {
            if (Lines[i].IsVerbal && (CurrentPlayingDialog >= 0 || BattleVoice.GetPlayingVoicesCount() > 0))
            {
                if (PersistenSingleton<BattleSubtitles>.Instance != null)
                {
                    PersistenSingleton<BattleSubtitles>.Instance.StartCoroutine(WaitForVoiceRoutine(i, when, onFinishedPlaying));
                }
                else
                {
                    onFinishedPlaying?.Invoke();
                }
                return;
            }
            PlayLineNow(i, when, onFinishedPlaying);
        }

        private static IEnumerator WaitAndHideRoutine(int speakerId, string text, float seconds, Action onFinished)
        {
            yield return new WaitForSeconds(seconds);
            if (PersistenSingleton<BattleSubtitles>.Instance != null)
            {
                string displayText = text != null ? "“" + text + "”" : "";
                PersistenSingleton<BattleSubtitles>.Instance.Hide((ushort)speakerId, displayText);
            }
            onFinished?.Invoke();
        }

        private static void PlayLineNow(int i, BattleVoice.BattleMoment when, Action onFinishedPlaying)
        {
            if (!_cacheInitialized) InitializePresetCache();

            BattleUnit speaker = null;
            if (!Lines[i].Speaker.CheckCanTalk && Lines[i].With != null)
            {
                foreach (BattleSpeakerEx withSpeaker in Lines[i].With)
                {
                    if (withSpeaker.CheckCanTalk)
                    {
                        speaker = withSpeaker.FindBattleUnit();
                        if (speaker != null) break;
                    }
                }
            }
            else
            {
                speaker = Lines[i].Speaker.FindBattleUnit();
            }

            if (speaker != null)
            {
                string path = Lines[i].Path;
                bool isTrance = InTranceCharacters.Contains(speaker.Data);

                if (isTrance && when != (BattleVoice.BattleMoment)BattleMomentEx.TranceEnter && when != (BattleVoice.BattleMoment)BattleMomentEx.TranceLeave)
                {
                    string trancePath = path.Replace("/", "(Trance)/");
                    string fullPath = "Voices/" + Localization.CurrentSymbol + "/Battle/" + trancePath;
                    if (AssetManager.HasAssetOnDisc("Sounds/" + fullPath + ".akb", true, true) || AssetManager.HasAssetOnDisc("Sounds/" + fullPath + ".ogg", true, false))
                        path = trancePath;
                }

                Log.Message("Starting '" + path + "'" + ((onFinishedPlaying != null) ? " with a chain" : ""));
                AddToPlayedLines(i);

                bool soundStarted = false;
                string displayText = Lines[i].Text != null ? "“" + Lines[i].Text + "”" : "";

                int soundId = BattleVoice.PlayVoice(speaker, "Battle/" + path, AdjustPriority(when, Lines[i].Priority), delegate ()
                {
                    if (soundStarted)
                    {
                        onFinishedPlaying?.Invoke();
                        if (PersistenSingleton<BattleSubtitles>.Instance != null)
                            PersistenSingleton<BattleSubtitles>.Instance.Hide(speaker.Id, displayText);
                    }
                });

                if (soundId != -1)
                {
                    soundStarted = true;

                    if (speaker.IsPlayer)
                    {
                        BattleStatus currentStatuses = speaker.CurrentStatus;
                        int speakerIdShifted = (int)speaker.PlayerIndex << 16;

                        foreach (var kvp in PresetCache)
                        {
                            if ((kvp.Key & 0xFFFF0000) != speakerIdShifted) continue;

                            int statusInt = kvp.Key & 0xFFFF;
                            if (((int)currentStatuses & statusInt) != 0)
                            {
                                AudioEffectManager.ApplyPresetOnSound(kvp.Value, soundId, path, 0f);
                                break;
                            }
                        }
                    }

                    if (PersistenSingleton<BattleSubtitles>.Instance != null && !string.IsNullOrEmpty(Lines[i].Text))
                        PersistenSingleton<BattleSubtitles>.Instance.Show(speaker, displayText);
                }
                else
                {
                    Log.Message($"[Echo-S] Audio file missing for {speaker.Name} ! => Battle/{path}");

                    if (PersistenSingleton<BattleSubtitles>.Instance != null && !string.IsNullOrEmpty(Lines[i].Text))
                        PersistenSingleton<BattleSubtitles>.Instance.Show(speaker, displayText);

                    float textLength = Lines[i].Text != null ? Lines[i].Text.Length : 0;
                    float waitTime = 2.0f + (textLength * 0.05f);

                    if (PersistenSingleton<BattleSubtitles>.Instance != null)
                    {
                        PersistenSingleton<BattleSubtitles>.Instance.StartCoroutine(WaitAndHideRoutine(speaker.Id, Lines[i].Text, waitTime, onFinishedPlaying));
                    }
                    else
                    {
                        onFinishedPlaying?.Invoke();
                    }
                }
                return;
            }

            Log.Message(string.Format("Couldn't find battle unit '{0}'", Lines[i].Speaker));
            onFinishedPlaying?.Invoke();
        }

        private static bool CanTalk(BattleSpeakerEx speaker, int priority, BattleStatusId statusException)
        {
            BattleUnit unit = speaker.FindBattleUnit();
            return unit != null && (speaker.Status == BattleStatusId.None || unit.IsUnderStatus((BattleStatus)speaker.Status)) && (!speaker.CheckCanTalk || BattleVoice.BattleSpeaker.CheckCanSpeak(unit, priority, statusException));
        }

        private static int AdjustPriority(BattleVoice.BattleMoment moment, int priority)
        {
            if (moment == BattleVoice.BattleMoment.VictoryPose)
                return priority + 2000;

            if ((moment >= BattleVoice.BattleMoment.BattleStart && moment <= BattleVoice.BattleMoment.EnemyEscape) ||
                moment == (BattleVoice.BattleMoment)BattleMomentEx.VictoryPoseSurvivor ||
                moment == (BattleVoice.BattleMoment)BattleMomentEx.TranceEnter ||
                moment == (BattleVoice.BattleMoment)BattleMomentEx.TranceLeave)
            {
                return priority + 1000;
            }

            if (moment == (BattleVoice.BattleMoment)BattleMomentEx.KillEffect ||
                moment == (BattleVoice.BattleMoment)BattleMomentEx.MissEffect ||
                moment == (BattleVoice.BattleMoment)BattleMomentEx.DodgeEffect)
            {
                return priority + 100;
            }

            if (moment == BattleVoice.BattleMoment.Added || moment == BattleVoice.BattleMoment.Removed)
                return priority + 100;

            if (moment >= BattleVoice.BattleMoment.Damaged && moment <= BattleVoice.BattleMoment.Missed)
                return priority - 100;

            return priority;
        }

        private static void AddToPlayedLines(int i)
        {
            PlayedLines[PlayedLinesPos] = i;
            PlayedLinesPos = (PlayedLinesPos + 1) % PlayedLines.Length;
            if (PlayedLinesCount < PlayedLines.Length)
                PlayedLinesCount++;
        }

        public static uint Flags = 0U;
        public static LineEntry[] Lines = null;
        public static int CustomLineStart;
        public static Queue<KeyValuePair<int, BattleVoice.BattleMoment>> LinesQueue = new Queue<KeyValuePair<int, BattleVoice.BattleMoment>>();
        public static int[] PlayedLines = new int[512];
        public static int PlayedLinesPos;
        public static int PlayedLinesCount;
        public static BattleCalculator OnDeathCalc;
        public static BattleCalculator PerformingCalc;
        public static Dictionary<BattleCommand, List<StatusEventData>> StatusEvents = new Dictionary<BattleCommand, List<StatusEventData>>();
        public static int CurrentPlayingDialog = -1;
        public static int CurrentPlayingChain = -1;
        public static bool HasFirstActHappened = false;
        public static HashSet<BTL_DATA> InTranceCharacters = new HashSet<BTL_DATA>();

        public delegate bool LineEntryPredicate(int lineId, BattleVoice.BattleMoment when);
    }
}
