using FF9;
using System;
using System.Collections.Generic;
using UnityEngine;

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

        public const int StatusUndiscovered = 0;
        public const int StatusDiscovered = 1;
        public const int StatusScanned = 2;
        public const int StatusScannedPlus = 3;
        public const int StatusMastered = 4;

        public const int RequiredKillsForMastery = 10;

        //{ 1, new BestiaryDisplayEntry(305, 0) { MaxHP = 900, WinGil = 15000 }}

        public static readonly Dictionary<int, BestiaryDisplayEntry> DisplayDatabase = new Dictionary<int, BestiaryDisplayEntry>
        {
            { 1, new BestiaryDisplayEntry(305, 0) }, // Goblin
            { 2, new BestiaryDisplayEntry(305, 1) }, // Fang
            { 3, new BestiaryDisplayEntry(310, 0) }, // Dendrobium
            { 4, new BestiaryDisplayEntry(302, 0) }, // Prison Cage
            { 5, new BestiaryDisplayEntry(301, 0) }, // Prison Cage
            { 6, new BestiaryDisplayEntry(302, 1) }, // Vivi
            { 7, new BestiaryDisplayEntry(301, 1) }, // Garnet
            { 8, new BestiaryDisplayEntry(303, 0) }, // Plant Brain
            { 9, new BestiaryDisplayEntry(303, 2) }, // Plant Spider (Plant Brain adds)
            { 10, new BestiaryDisplayEntry(304, 0) } // Plant Spider
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

        public static void UpdateMonsterStatus(int bestiaryId, int newStatus)
        {
            if (FF9StateSystem.EventState == null) return;
            if (!FF9StateSystem.EventState.gScriptDictionary.TryGetValue(MemoriaDictStatusKey, out Dictionary<Int32, Int32> dict))
            {
                dict = new Dictionary<Int32, Int32>();
                FF9StateSystem.EventState.gScriptDictionary.Add(MemoriaDictStatusKey, dict);
            }

            if (!dict.TryGetValue(bestiaryId, out int currentStatus) || newStatus > currentStatus)
                dict[bestiaryId] = newStatus;
        }

        public static void IncrementMonsterKills(int bestiaryId)
        {
            if (FF9StateSystem.EventState == null) return;
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
    }
}
