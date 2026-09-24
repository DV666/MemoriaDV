using System;
using System.Collections.Generic;
using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Data;

namespace Memoria.Scripts.TranceSeek
{
    public class BestiaryDisplayEntry
    {
        public int BattleId;
        public int MonsterIndex;

        public int? MaxHP;
        public int? MaxMP;
        public int? Speed;
        public int? Strength;
        public int? Magic;
        public int? Spirit;
        public int? PhysicalDefense;
        public int? PhysicalEvade;
        public int? MagicalDefense;
        public int? MagicalEvade;
        public int? WinGil;
        public int? WinExp;

        public int? RewardGils;
        public RegularItem[] RewardItems;
        public Boolean IsBoss = false;

        public BestiaryDisplayEntry(int battleId, int monsterIndex)
        {
            BattleId = battleId;
            MonsterIndex = monsterIndex;
        }
    }

    public struct MonsterEncounter
    {
        public int BattleId;
        public int MonsterIndex;

        public MonsterEncounter(int battleId, int monsterIndex)
        {
            BattleId = battleId;
            MonsterIndex = monsterIndex;
        }
    }

    public static class TranceSeekBestiaryDB
    {
        public const int MemoriaDictStatusKey = 1010;
        public const int MemoriaDictKillCountKey = 1011;
        public const int MemoriaDictRewardKey = 1012;

        public const int StatusUndiscovered = 0;
        public const int StatusDiscovered = 1;
        public const int StatusScanned = 2;
        public const int StatusScannedPlus = 3;
        public const int StatusMastered = 4;

        public const int RequiredKillsForMastery = 10;

        public static List<string> PendingRewardMessages = new List<string>();
        private static bool _isRewardUiInitialized = false;

        public static readonly Dictionary<int, BestiaryDisplayEntry> DisplayDatabase = new Dictionary<int, BestiaryDisplayEntry>
        {
            { 1, new BestiaryDisplayEntry(305, 0) {
                RewardGils = 500,
                RewardItems = new RegularItem[] { RegularItem.Potion, RegularItem.Potion, RegularItem.HiPotion }
            }},
            { 2, new BestiaryDisplayEntry(305, 1) },
            { 3, new BestiaryDisplayEntry(310, 0) },
            { 4, new BestiaryDisplayEntry(302, 0) { IsBoss = true } },
            { 5, new BestiaryDisplayEntry(301, 0) { IsBoss = true } },
            { 6, new BestiaryDisplayEntry(302, 1) },
            { 7, new BestiaryDisplayEntry(301, 1) },
            { 8, new BestiaryDisplayEntry(303, 0) { IsBoss = true } },
            { 9, new BestiaryDisplayEntry(303, 2) },
            { 10, new BestiaryDisplayEntry(304, 0) }
        };

        public static readonly Dictionary<MonsterEncounter, int> EncounterToBestiaryId = new Dictionary<MonsterEncounter, int>
        {
            { new MonsterEncounter(305, 0), 1 },
            { new MonsterEncounter(306, 0), 1 },
            { new MonsterEncounter(67, 0), 1 },

            { new MonsterEncounter(305, 1), 2 },
            { new MonsterEncounter(306, 1), 2 },
            { new MonsterEncounter(67, 1), 2 },

            { new MonsterEncounter(309, 0), 3 },
            { new MonsterEncounter(310, 0), 3 },
        };

        private static void EnsureRewardUiInitialized()
        {
            if (!_isRewardUiInitialized)
            {
                UnityEngine.GameObject go = new UnityEngine.GameObject("TranceSeekBestiaryRewardSystem");
                UnityEngine.Object.DontDestroyOnLoad(go);
                go.AddComponent<TranceSeekBestiaryRewardUI>();
                _isRewardUiInitialized = true;
            }
        }

        public static bool TryGetDisplayEntry(int bestiaryId, out BestiaryDisplayEntry entry)
        {
            return DisplayDatabase.TryGetValue(bestiaryId, out entry);
        }

        public static bool TryGetBestiaryId(int battleId, int monsterIndex, out int bestiaryId)
        {
            return EncounterToBestiaryId.TryGetValue(new MonsterEncounter(battleId, monsterIndex), out bestiaryId);
        }

        public static int GetMonsterStatus(int bestiaryId)
        {
            if (FF9StateSystem.EventState != null)
            {
                if (FF9StateSystem.EventState.gScriptDictionary.TryGetValue(MemoriaDictStatusKey, out Dictionary<Int32, Int32> dict))
                {
                    if (dict.TryGetValue(bestiaryId, out int status))
                        return status;
                }
            }
            return StatusUndiscovered;
        }

        public static int GetMonsterKills(int bestiaryId)
        {
            if (FF9StateSystem.EventState != null)
            {
                if (FF9StateSystem.EventState.gScriptDictionary.TryGetValue(MemoriaDictKillCountKey, out Dictionary<Int32, Int32> dict))
                {
                    if (dict.TryGetValue(bestiaryId, out int kills))
                        return kills;
                }
            }
            return 0;
        }

        public static void UpdateMonsterStatus(int battleId, int monsterIndex, int newStatus)
        {
            if (!TryGetBestiaryId(battleId, monsterIndex, out int bestiaryId))
                return;

            if (FF9StateSystem.EventState == null)
                return;

            if (!FF9StateSystem.EventState.gScriptDictionary.TryGetValue(MemoriaDictStatusKey, out Dictionary<Int32, Int32> dict))
            {
                dict = new Dictionary<Int32, Int32>();
                FF9StateSystem.EventState.gScriptDictionary.Add(MemoriaDictStatusKey, dict);
            }

            if (!dict.TryGetValue(bestiaryId, out int currentStatus) || newStatus > currentStatus)
                dict[bestiaryId] = newStatus;
        }

        public static void IncrementMonsterKills(int battleId, int monsterIndex)
        {
            if (!TryGetBestiaryId(battleId, monsterIndex, out int bestiaryId))
                return;

            if (FF9StateSystem.EventState == null)
                return;

            if (!FF9StateSystem.EventState.gScriptDictionary.TryGetValue(MemoriaDictKillCountKey, out Dictionary<Int32, Int32> dict))
            {
                dict = new Dictionary<Int32, Int32>();
                FF9StateSystem.EventState.gScriptDictionary.Add(MemoriaDictKillCountKey, dict);
            }

            if (dict.TryGetValue(bestiaryId, out int kills))
                dict[bestiaryId] = kills + 1;
            else
                dict[bestiaryId] = 1;
        }

        public static bool IsRewardUnlocked(int bestiaryId)
        {
            if (!TryGetDisplayEntry(bestiaryId, out BestiaryDisplayEntry entry))
                return false;

            if (entry.IsBoss)
                return false;

            int kills = GetMonsterKills(bestiaryId);
            return kills >= RequiredKillsForMastery;
        }

        public static bool ClaimReward(int bestiaryId)
        {
            if (FF9StateSystem.EventState == null)
                return false;

            if (!FF9StateSystem.EventState.gScriptDictionary.TryGetValue(MemoriaDictRewardKey, out Dictionary<Int32, Int32> dict))
            {
                dict = new Dictionary<Int32, Int32>();
                FF9StateSystem.EventState.gScriptDictionary.Add(MemoriaDictRewardKey, dict);
            }

            if (dict.ContainsKey(bestiaryId))
                return false;

            dict.Add(bestiaryId, 1);
            return true;
        }

        public static void ProcessMonsterDeath(int battleId, int monsterIndex)
        {
            EnsureRewardUiInitialized();
            IncrementMonsterKills(battleId, monsterIndex);

            if (!TryGetBestiaryId(battleId, monsterIndex, out int bestiaryId))
                return;

            if (IsRewardUnlocked(bestiaryId))
            {
                if (ClaimReward(bestiaryId))
                {
                    if (TryGetDisplayEntry(bestiaryId, out BestiaryDisplayEntry entry))
                    {
                        UpdateMonsterStatus(battleId, monsterIndex, StatusMastered);

                        if (entry.RewardGils.HasValue)
                            battle.btl_bonus.gil += entry.RewardGils.Value;

                        Dictionary<RegularItem, int> rewardCounts = new Dictionary<RegularItem, int>();
                        if (entry.RewardItems != null)
                        {
                            foreach (RegularItem it in entry.RewardItems)
                            {
                                if (it != RegularItem.NoItem)
                                {
                                    battle.btl_bonus.item.Add(it);
                                    if (rewardCounts.ContainsKey(it))
                                        rewardCounts[it]++;
                                    else
                                        rewardCounts[it] = 1;
                                }
                            }
                        }

                        string monsterName = "???";
                        try
                        {
                            string battleSceneName = "";
                            FF9BattleDB.SceneData.TryGetKey(entry.BattleId, out battleSceneName);
                            battleSceneName = battleSceneName.Substring(4);
                            int textId = FF9BattleDB.SceneData["BSC_" + battleSceneName];
                            string[] texts = FF9TextTool.GetBattleText(textId);

                            if (texts != null && entry.MonsterIndex < texts.Length)
                            {
                                monsterName = FF9TextTool.RemoveOpCode(texts[entry.MonsterIndex]);
                            }
                        }
                        catch { }

                        string animatedRewardStr =
                            "[ANIM=TextRGBA,Loop,Linear,0.2,Linear,0.4,Linear,0.6,Linear,0.8,Linear,1.0,Linear,1.2,Linear,1.4,Linear,1.6, 0.87,0,0, 0,0.56,0.56, 0.84,0.84,0.18, 0.36,0.36,1, 0.17,0.38,0.23, 0.47,0.25,0.02, 1,1,1, 0.5,0,0.5, 0.87,0,0]r" +
                            "[ANIM=TextRGBA,Loop,Linear,0.2,Linear,0.4,Linear,0.6,Linear,0.8,Linear,1.0,Linear,1.2,Linear,1.4,Linear,1.6, 0,0.56,0.56, 0.84,0.84,0.18, 0.36,0.36,1, 0.17,0.38,0.23, 0.47,0.25,0.02, 1,1,1, 0.5,0,0.5, 0.87,0,0, 0,0.56,0.56]é" +
                            "[ANIM=TextRGBA,Loop,Linear,0.2,Linear,0.4,Linear,0.6,Linear,0.8,Linear,1.0,Linear,1.2,Linear,1.4,Linear,1.6, 0.84,0.84,0.18, 0.36,0.36,1, 0.17,0.38,0.23, 0.47,0.25,0.02, 1,1,1, 0.5,0,0.5, 0.87,0,0, 0,0.56,0.56, 0.84,0.84,0.18]c" +
                            "[ANIM=TextRGBA,Loop,Linear,0.2,Linear,0.4,Linear,0.6,Linear,0.8,Linear,1.0,Linear,1.2,Linear,1.4,Linear,1.6, 0.36,0.36,1, 0.17,0.38,0.23, 0.47,0.25,0.02, 1,1,1, 0.5,0,0.5, 0.87,0,0, 0,0.56,0.56, 0.84,0.84,0.18, 0.36,0.36,1]o" +
                            "[ANIM=TextRGBA,Loop,Linear,0.2,Linear,0.4,Linear,0.6,Linear,0.8,Linear,1.0,Linear,1.2,Linear,1.4,Linear,1.6, 0.17,0.38,0.23, 0.47,0.25,0.02, 1,1,1, 0.5,0,0.5, 0.87,0,0, 0,0.56,0.56, 0.84,0.84,0.18, 0.36,0.36,1, 0.17,0.38,0.23]m" +
                            "[ANIM=TextRGBA,Loop,Linear,0.2,Linear,0.4,Linear,0.6,Linear,0.8,Linear,1.0,Linear,1.2,Linear,1.4,Linear,1.6, 0.47,0.25,0.02, 1,1,1, 0.5,0,0.5, 0.87,0,0, 0,0.56,0.56, 0.84,0.84,0.18, 0.36,0.36,1, 0.17,0.38,0.23, 0.47,0.25,0.02]p" +
                            "[ANIM=TextRGBA,Loop,Linear,0.2,Linear,0.4,Linear,0.6,Linear,0.8,Linear,1.0,Linear,1.2,Linear,1.4,Linear,1.6, 1,1,1, 0.5,0,0.5, 0.87,0,0, 0,0.56,0.56, 0.84,0.84,0.18, 0.36,0.36,1, 0.17,0.38,0.23, 0.47,0.25,0.02, 1,1,1]e" +
                            "[ANIM=TextRGBA,Loop,Linear,0.2,Linear,0.4,Linear,0.6,Linear,0.8,Linear,1.0,Linear,1.2,Linear,1.4,Linear,1.6, 0.5,0,0.5, 0.87,0,0, 0,0.56,0.56, 0.84,0.84,0.18, 0.36,0.36,1, 0.17,0.38,0.23, 0.47,0.25,0.02, 1,1,1, 0.5,0,0.5]n" +
                            "[ANIM=TextRGBA,Loop,Linear,0.2,Linear,0.4,Linear,0.6,Linear,0.8,Linear,1.0,Linear,1.2,Linear,1.4,Linear,1.6, 0.87,0,0, 0,0.56,0.56, 0.84,0.84,0.18, 0.36,0.36,1, 0.17,0.38,0.23, 0.47,0.25,0.02, 1,1,1, 0.5,0,0.5, 0.87,0,0]s" +
                            "[ANIM=TextRGBA,Loop,Linear,0.2,Linear,0.4,Linear,0.6,Linear,0.8,Linear,1.0,Linear,1.2,Linear,1.4,Linear,1.6, 0,0.56,0.56, 0.84,0.84,0.18, 0.36,0.36,1, 0.17,0.38,0.23, 0.47,0.25,0.02, 1,1,1, 0.5,0,0.5, 0.87,0,0, 0,0.56,0.56]e" +
                            "[ANIM=TextRGBA,Loop,Linear,0.2,Linear,0.4,Linear,0.6,Linear,0.8,Linear,1.0,Linear,1.2,Linear,1.4,Linear,1.6, 0.84,0.84,0.18, 0.36,0.36,1, 0.17,0.38,0.23, 0.47,0.25,0.02, 1,1,1, 0.5,0,0.5, 0.87,0,0, 0,0.56,0.56, 0.84,0.84,0.18]s" +
                            "[C8C8C8]";

                        string msg = $"[FFCC00]Vous avez débloqué le bestiaire de[-] {monsterName} [FFCC00]![-]\n";
                        msg += $"Vous gagnez les {animatedRewardStr} suivantes :";

                        if (entry.RewardGils.HasValue)
                            msg += $"\n[SPRT=IconAtlas,icon_gil,48,48]  {entry.RewardGils.Value} Gils";

                        foreach (KeyValuePair<RegularItem, int> kvp in rewardCounts)
                        {
                            string itemName = FF9TextTool.ItemName(kvp.Key);
                            FF9ITEM_DATA itemData = ff9item._FF9Item_Data[kvp.Key];
                            string spriteName = $"item{itemData.shape:0#}_{itemData.color:0#}";
                            string MultipleItems = kvp.Value > 1 ? $"{kvp.Value}x" : "";
                            msg += $"\n[SPRT={spriteName},48,48]  {MultipleItems} {itemName}";
                        }

                        PendingRewardMessages.Add(msg);
                    }
                }
            }
        }
    }
}
