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
        public Single AdjustScale = 1f;
        public String AnimationIdle;
        public String CustomBBG;

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
#if DEV_TS
            { 1, new BestiaryDisplayEntry(336, 0) { IsBoss = true } }, // Masked Man
            { 2, new BestiaryDisplayEntry(337, 0) { IsBoss = true } }, // Steiner 1
            { 3, new BestiaryDisplayEntry(335, 0) { IsBoss = true } }, // Steiner 2
            { 4, new BestiaryDisplayEntry(335, 1) { IsBoss = true } }, // Haagen
            { 5, new BestiaryDisplayEntry(335, 2) { IsBoss = true } }, // Weimar
            { 6, new BestiaryDisplayEntry(334, 1) { IsBoss = true } }, // Steiner 3
            { 7, new BestiaryDisplayEntry(305, 0) }, // Goblin
            { 8, new BestiaryDisplayEntry(305, 1) }, // Fang
            { 9, new BestiaryDisplayEntry(310, 0) }, // Dendrobium
            { 10, new BestiaryDisplayEntry(302, 0) { IsBoss = true } }, // Prison Cage
            { 11, new BestiaryDisplayEntry(301, 0) { IsBoss = true } }, // Prison Cage
            { 12, new BestiaryDisplayEntry(302, 1) { IsBoss = true } }, // Vivi
            { 13, new BestiaryDisplayEntry(301, 1) { IsBoss = true } }, // Garnet
            { 14, new BestiaryDisplayEntry(303, 0) { IsBoss = true } }, // Plant Brain
            { 15, new BestiaryDisplayEntry(303, 2) }, // Plant Spider
            { 16, new BestiaryDisplayEntry(304, 0) }, // Plant Spider
            { 17, new BestiaryDisplayEntry(357, 0) }, // Python
            { 18, new BestiaryDisplayEntry(362, 0) }, // Mu
            { 19, new BestiaryDisplayEntry(22, 0) }, // Flan
            { 20, new BestiaryDisplayEntry(22, 1) }, // Cave Imp
            { 21, new BestiaryDisplayEntry(24, 0) }, // Wyerd
            { 22, new BestiaryDisplayEntry(21, 1) { IsBoss = true } }, // Black Waltz 1
            { 23, new BestiaryDisplayEntry(21, 0) { IsBoss = true } }, // Sealion 
            { 24, new BestiaryDisplayEntry(244, 1) }, // Carve Spider
            { 25, new BestiaryDisplayEntry(343, 1) }, // Vice
            { 26, new BestiaryDisplayEntry(139, 0) }, // Ghost
            { 27, new BestiaryDisplayEntry(6, 1) { IsBoss = true } }, // Ghost King
            { 28, new BestiaryDisplayEntry(294, 0) { IsBoss = true } }, // Black Waltz 2
            { 29, new BestiaryDisplayEntry(296, 0) { IsBoss = true } }, // Black Waltz 3
            { 30, new BestiaryDisplayEntry(30, 0) }, // Fang (Lindblum)
            { 31, new BestiaryDisplayEntry(20, 0) }, // Mu (Lindblum)
            { 32, new BestiaryDisplayEntry(34, 0) }, // Trick Sparrow (Lindblum)
            { 33, new BestiaryDisplayEntry(14, 0) }, // Zaghnol (Lindblum)
            { 34, new BestiaryDisplayEntry(161, 1) }, // Axe Beak
            { 35, new BestiaryDisplayEntry(165, 0) }, // Bomb
            { 36, new BestiaryDisplayEntry(243, 0) }, // Vice
            { 37, new BestiaryDisplayEntry(243, 1) }, // Hedgehog Pie
            { 38, new BestiaryDisplayEntry(221, 1) }, // Ladybug
            { 39, new BestiaryDisplayEntry(231, 0) }, // Serpion
            { 40, new BestiaryDisplayEntry(146, 0) }, // Ironite
            { 41, new BestiaryDisplayEntry(331, 0) }, // Axolotl
            { 42, new BestiaryDisplayEntry(135, 0) }, // Gigan Toad
            { 43, new BestiaryDisplayEntry(101, 0) }, // Skeleton
            { 44, new BestiaryDisplayEntry(101, 1) }, // Hornet
            { 45, new BestiaryDisplayEntry(327, 0) }, // Lamia
            { 46, new BestiaryDisplayEntry(325, 0) }, // Type A
            { 47, new BestiaryDisplayEntry(325, 1) { IsBoss = true } }, // Zorn
            { 48, new BestiaryDisplayEntry(325, 2) { IsBoss = true } }, // Thorn
            { 49, new BestiaryDisplayEntry(326, 0) { IsBoss = true, AdjustScale = 0.65f, AnimationIdle = "ANH_MON_B3_114_000" } }, // Gizamaluke
            { 50, new BestiaryDisplayEntry(204, 0) }, // Lizard Man
            { 51, new BestiaryDisplayEntry(187, 0) }, // Nymph
            { 52, new BestiaryDisplayEntry(215, 0) }, // Yeti
            { 53, new BestiaryDisplayEntry(39, 0) }, // Basilisk
            { 54, new BestiaryDisplayEntry(71, 0) }, // Magic Vice
            { 55, new BestiaryDisplayEntry(51, 0) }, // Mimic
            { 56, new BestiaryDisplayEntry(36, 0) }, // Type A
            { 57, new BestiaryDisplayEntry(36, 1) }, // Type B
            { 58, new BestiaryDisplayEntry(4, 0) { IsBoss = true } }, // Beatrix 1
            { 59, new BestiaryDisplayEntry(52, 0) { IsBoss = true } }, // Black Waltz 3 (broken)
            { 60, new BestiaryDisplayEntry(353, 1) }, // Trick Sparrow
            { 61, new BestiaryDisplayEntry(182, 0) }, // Mandragora
            { 62, new BestiaryDisplayEntry(100, 0) }, // Dragonfly
            { 63, new BestiaryDisplayEntry(77, 0) }, // Crawler
            { 64, new BestiaryDisplayEntry(342, 0) }, // Zemzelett (Arena)
            { 65, new BestiaryDisplayEntry(76, 0) { IsBoss = true } }, // Ralvurahva
            { 66, new BestiaryDisplayEntry(53, 2) }, // Sand Scorpion
            { 67, new BestiaryDisplayEntry(60, 0) }, // Carrion Worm
            { 68, new BestiaryDisplayEntry(317, 0) { AdjustScale = 0.60f } }, // Sand Golem
            { 69, new BestiaryDisplayEntry(317, 1) { AdjustScale = 0.60f } }, // Core
            { 70, new BestiaryDisplayEntry(320, 0) { AdjustScale = 0.65f } }, // Zuu
            { 71, new BestiaryDisplayEntry(300, 0) { IsBoss = true } }, // Antlion
            { 72, new BestiaryDisplayEntry(298, 0) }, // Soldier
            { 73, new BestiaryDisplayEntry(298, 1) }, // Type B
            { 74, new BestiaryDisplayEntry(299, 0) { IsBoss = true } }, // Beatrix 2
            { 75, new BestiaryDisplayEntry(271, 0) }, // Soldier (dungeon)
            { 76, new BestiaryDisplayEntry(65, 0) }, // Type C
            { 77, new BestiaryDisplayEntry(65, 1) }, // Bandersnatch
            { 78, new BestiaryDisplayEntry(74, 0) { IsBoss = true } }, // Zorn
            { 79, new BestiaryDisplayEntry(74, 1) { IsBoss = true } }, // Thorn
            { 80, new BestiaryDisplayEntry(73, 0) { IsBoss = true } }, // Beatrix 3
            { 81, new BestiaryDisplayEntry(75, 0) { IsBoss = true } }, // Ralvuimago
            { 82, new BestiaryDisplayEntry(81, 0) }, // Zaghnol (Pinnacle Rock)
            { 83, new BestiaryDisplayEntry(82, 1) }, // Seeker Bat
            { 84, new BestiaryDisplayEntry(84, 0) { IsBoss = true } }, // Armodullahan
            { 85, new BestiaryDisplayEntry(83, 0) { IsBoss = true } }, // Lani 1
            { 86, new BestiaryDisplayEntry(102, 0) }, // Feather Circle
            { 87, new BestiaryDisplayEntry(86, 0) }, // Abomination
            { 88, new BestiaryDisplayEntry(98, 0) }, // Griffin
            { 89, new BestiaryDisplayEntry(218, 0) }, // Goblin Mage
            { 90, new BestiaryDisplayEntry(218, 1) }, // Goblin
            { 91, new BestiaryDisplayEntry(70, 0) { AnimationIdle = "ANH_MON_B3_061_001" } }, // Cactuar
            { 92, new BestiaryDisplayEntry(332, 0) }, // Sahagin
            { 93, new BestiaryDisplayEntry(104, 0) }, // Myconid
            { 94, new BestiaryDisplayEntry(826, 0) }, // Zemzelett
            { 95, new BestiaryDisplayEntry(826, 1) }, // Magedragora
            { 96, new BestiaryDisplayEntry(887, 0) }, // Troll
            { 97, new BestiaryDisplayEntry(109, 0) }, // Gnoll
            { 98, new BestiaryDisplayEntry(110, 0) { AdjustScale = 0.75f } }, // Ochu
            { 99, new BestiaryDisplayEntry(107, 0) { IsBoss = true, AdjustScale = 0.75f } }, // Hilgigars 1
            { 100, new BestiaryDisplayEntry(658, 0) }, // Blazer Beetle
            { 101, new BestiaryDisplayEntry(131, 1) }, // Zombie
            { 102, new BestiaryDisplayEntry(119, 0) { AdjustScale = 0.70f } }, // Stroper
            { 103, new BestiaryDisplayEntry(124, 0) }, // Dracozombie
            { 104, new BestiaryDisplayEntry(116, 0) { IsBoss = true, AdjustScale = 0.70f } }, // Soulcage
            { 105, new BestiaryDisplayEntry(132, 0) { IsBoss = true } }, // Scarlet Hair
            { 106, new BestiaryDisplayEntry(112, 2) { IsBoss = true } }, // Lani 2
            { 107, new BestiaryDisplayEntry(107, 1) { IsBoss = true, AdjustScale = 0.75f } }, // Hilgigars 2
            { 108, new BestiaryDisplayEntry(115, 0) }, // Mistodon
            { 109, new BestiaryDisplayEntry(115, 1) { IsBoss = true } }, // Kuja 1
            { 110, new BestiaryDisplayEntry(340, 0) }, // Epitaph (arena)
            { 111, new BestiaryDisplayEntry(340, 1) }, // Dagger (arena)
            { 112, new BestiaryDisplayEntry(340, 2) }, // Amarant (arena)
            { 113, new BestiaryDisplayEntry(340, 3) }, // Quina (arena)
            { 114, new BestiaryDisplayEntry(930, 0) }, // Tantarian
            { 115, new BestiaryDisplayEntry(930, 1) { IsBoss = true, AdjustScale = 0.60f } }, // Stroper (Tantarian)
            { 116, new BestiaryDisplayEntry(930, 2) { IsBoss = true} }, // Armstrong (Tantarian)
            { 117, new BestiaryDisplayEntry(930, 3) { IsBoss = true} }, // Hecteyes (Tantarian)
            { 118, new BestiaryDisplayEntry(915, 0) { IsBoss = true } }, // Mistodon (Alexandria)
            { 119, new BestiaryDisplayEntry(144, 1) { AdjustScale = 0.60f } }, // Land Worm
            { 120, new BestiaryDisplayEntry(144, 0) }, // Antlion (WM)
            { 121, new BestiaryDisplayEntry(144, 2) { IsBoss = true } }, // King Antlion
            { 122, new BestiaryDisplayEntry(144, 3) { IsBoss = true, AdjustScale = 0.60f } }, // Sand Worm
            { 123, new BestiaryDisplayEntry(173, 0) }, // Anemone
            { 124, new BestiaryDisplayEntry(582, 0) }, // Armstrong
            { 125, new BestiaryDisplayEntry(576, 0) }, // Catoblepas
            { 126, new BestiaryDisplayEntry(172, 0) }, // Jabberwock
            { 127, new BestiaryDisplayEntry(3, 0) }, // Ogre
            { 128, new BestiaryDisplayEntry(912, 0) }, // Garuda
            { 129, new BestiaryDisplayEntry(3, 1) }, // Toadulent
            { 130, new BestiaryDisplayEntry(499, 0) }, // Epitaph
            { 131, new BestiaryDisplayEntry(499, 1) }, // Amarant
            { 132, new BestiaryDisplayEntry(468, 1) }, // Dagger
            { 133, new BestiaryDisplayEntry(465, 1) }, // Eiko
            { 134, new BestiaryDisplayEntry(467, 1) }, // Freya
            { 135, new BestiaryDisplayEntry(519, 1) }, // Quina
            { 136, new BestiaryDisplayEntry(464, 1) }, // Steiner
            { 137, new BestiaryDisplayEntry(475, 1) }, // Vivi
            { 138, new BestiaryDisplayEntry(502, 1) }, // Zidane
            { 139, new BestiaryDisplayEntry(468, 2) }, // Beatrix
            { 140, new BestiaryDisplayEntry(465, 2) }, // Lani
            { 141, new BestiaryDisplayEntry(475, 2) }, // Baku
            { 142, new BestiaryDisplayEntry(464, 2) }, // King Leo
            { 143, new BestiaryDisplayEntry(1, 1) { IsBoss = true, AdjustScale = 0.80f } }, // Oeilvert Guardian
            { 144, new BestiaryDisplayEntry(0, 0) { IsBoss = true, AdjustScale = 0.50f } }, // Arkh
            { 145, new BestiaryDisplayEntry(851, 0) }, // Drakan
            { 146, new BestiaryDisplayEntry(856, 0) }, // Torama
            { 147, new BestiaryDisplayEntry(857, 0) }, // Grimlock
            { 148, new BestiaryDisplayEntry(857, 1) }, // Grimlock
            { 149, new BestiaryDisplayEntry(849, 1) { IsBoss = true } }, // The Brother
            { 150, new BestiaryDisplayEntry(849, 2) { IsBoss = true } }, // The Sister
            { 151, new BestiaryDisplayEntry(850, 1) { IsBoss = true, AdjustScale = 0.65f, AnimationIdle = "ANH_MON_B3_114_000" } }, // Salamander
            { 152, new BestiaryDisplayEntry(849, 4) { IsBoss = true } }, // Onyx
            { 153, new BestiaryDisplayEntry(851, 1) { IsBoss = true } }, // Kelgar
            { 154, new BestiaryDisplayEntry(849, 5) { IsBoss = true } }, // Obscurbo
            { 155, new BestiaryDisplayEntry(849, 3) { IsBoss = true } }, // Mad Alchemist
            { 156, new BestiaryDisplayEntry(525, 0) { IsBoss = true, AdjustScale = 0.50f } }, // Valia Pira
            { 157, new BestiaryDisplayEntry(759, 0) }, // Vepal
            { 158, new BestiaryDisplayEntry(257, 1) }, // Cave Imp (WM)
            { 159, new BestiaryDisplayEntry(758, 0) { AdjustScale = 0.65f } }, // Whale Zombie
            { 160, new BestiaryDisplayEntry(741, 0) }, // Gigan Octopus
            { 161, new BestiaryDisplayEntry(308, 0) }, // Vepal (Gulg)
            { 162, new BestiaryDisplayEntry(308, 1) }, // Grenade
            { 163, new BestiaryDisplayEntry(267, 0) }, // Wraith
            { 164, new BestiaryDisplayEntry(267, 1) }, // Wraith
            { 165, new BestiaryDisplayEntry(210, 0) }, // Worm Hydra
            { 166, new BestiaryDisplayEntry(195, 0) { AdjustScale = 0.75f } }, // Red Dragon
            { 167, new BestiaryDisplayEntry(835, 0) { IsBoss = true } }, // Zorn
            { 168, new BestiaryDisplayEntry(835, 1) { IsBoss = true } }, // Thorn
            { 169, new BestiaryDisplayEntry(200, 0) { IsBoss = true, AdjustScale = 0.75f } }, // Meltigemini
            { 170, new BestiaryDisplayEntry(829, 0) }, // Gimme Cat
            { 171, new BestiaryDisplayEntry(262, 0) }, // Grand Dragon
            { 172, new BestiaryDisplayEntry(246, 0) }, // Cerberus
            { 173, new BestiaryDisplayEntry(881, 0) }, // Gargoyle
            { 174, new BestiaryDisplayEntry(881, 1) }, // Agares
            { 175, new BestiaryDisplayEntry(878, 0) { AdjustScale = 0.75f } }, // Veteran
            { 176, new BestiaryDisplayEntry(872, 0) }, // Tonberry
            { 177, new BestiaryDisplayEntry(871, 0) { IsBoss = true, AdjustScale = 0.85f } }, // Taharka
            { 178, new BestiaryDisplayEntry(2, 2) { IsBoss = true } }, // Fire Guardian
            { 179, new BestiaryDisplayEntry(2, 4) { IsBoss = true } }, // Water Guardian
            { 180, new BestiaryDisplayEntry(2, 3) { IsBoss = true, AdjustScale = 0.85f } }, // Wind Guardian
            { 181, new BestiaryDisplayEntry(2, 0) { IsBoss = true, AdjustScale = 0.65f } }, // Earth Guardian
            { 182, new BestiaryDisplayEntry(341, 0) }, // Malboro (arena)
            { 183, new BestiaryDisplayEntry(4, 1) { IsBoss = true } }, // Dark Beatrix
            { 184, new BestiaryDisplayEntry(4, 2) { IsBoss = true, AdjustScale = 0.70f } }, // Nightmare
            { 185, new BestiaryDisplayEntry(4, 3) { IsBoss = true } }, // Thousand Fears
            { 186, new BestiaryDisplayEntry(128, 2) { IsBoss = true } }, // Zombdance
            { 187, new BestiaryDisplayEntry(128, 3) { IsBoss = true, AdjustScale = 0.75f } }, // Mickson & Jackael
            { 188, new BestiaryDisplayEntry(871, 1) { IsBoss = true, AdjustScale = 0.50f, CustomBBG = "BBG_B042" } }, // Mysterious Girl
            { 189, new BestiaryDisplayEntry(333, 0) }, // Hecteyes
            { 190, new BestiaryDisplayEntry(868, 0) }, // Ring Leader
            { 191, new BestiaryDisplayEntry(865, 0) }, // Mover
            { 192, new BestiaryDisplayEntry(155, 1) { IsBoss = true } }, // Poichi Sacré
            { 193, new BestiaryDisplayEntry(160, 2) { IsBoss = true } }, // Toxicochère
            { 194, new BestiaryDisplayEntry(163, 1) { IsBoss = true } }, // Volcalion
            { 195, new BestiaryDisplayEntry(903, 0) }, // Abadon
            { 196, new BestiaryDisplayEntry(899, 0) }, // Malboro
            { 197, new BestiaryDisplayEntry(917, 0) { AdjustScale = 0.60f } }, // Shell Dragon
            { 198, new BestiaryDisplayEntry(899, 1) }, // Amdusias
            { 199, new BestiaryDisplayEntry(917, 1) }, // Poichi Sacré
            { 200, new BestiaryDisplayEntry(903, 2) }, // Toxicochère
            { 201, new BestiaryDisplayEntry(917, 2) }, // Volcalion
            { 202, new BestiaryDisplayEntry(903, 1) { AdjustScale = 2.5f }}, // Dark Flan
            { 203, new BestiaryDisplayEntry(889, 0) { IsBoss = true, AdjustScale = 0.85f } }, // Silver Dragon
            { 204, new BestiaryDisplayEntry(890, 0) { IsBoss = true } }, // Garland
            { 205, new BestiaryDisplayEntry(891, 0) { IsBoss = true } }, // Kuja 2
            { 206, new BestiaryDisplayEntry(363, 0) { IsBoss = true } }, // Mu (friendly)
            { 207, new BestiaryDisplayEntry(192, 0) { IsBoss = true } }, // Ghost (friendly)
            { 208, new BestiaryDisplayEntry(235, 0) { IsBoss = true } }, // Ladybug (friendly)
            { 209, new BestiaryDisplayEntry(668, 0) { IsBoss = true } }, // Yeti (friendly)
            { 210, new BestiaryDisplayEntry(637, 0) { IsBoss = true } }, // Nymph (friendly)
            { 211, new BestiaryDisplayEntry(365, 0) { IsBoss = true } }, // Jabberwock (friendly)
            { 212, new BestiaryDisplayEntry(632, 0) { IsBoss = true } }, // Feater Circle (friendly)
            { 213, new BestiaryDisplayEntry(723, 0) { IsBoss = true } }, // Garuda (friendly)
            { 214, new BestiaryDisplayEntry(920, 0) { IsBoss = true } }, // Yan (friendly)
            { 215, new BestiaryDisplayEntry(600, 1) { IsBoss = true } }, // Fandalf
            { 216, new BestiaryDisplayEntry(600, 2) { IsBoss = true, AdjustScale = 0.60f } }, // Frimoire (Fire)
            { 217, new BestiaryDisplayEntry(600, 3) { IsBoss = true, AdjustScale = 0.60f } }, // Frimoire (Ice)
            { 218, new BestiaryDisplayEntry(600, 4) { IsBoss = true, AdjustScale = 0.60f } }, // Frimoire (Thunder)
            { 219, new BestiaryDisplayEntry(838, 1) { IsBoss = true, CustomBBG = "BBG_B042" } } // Golden Pidove
#endif
        };

        public static readonly Dictionary<MonsterEncounter, int> EncounterToBestiaryId = new Dictionary<MonsterEncounter, int>
        {
#if DEV_TS
            { new MonsterEncounter(305, 0), 7 }, // Goblin
            { new MonsterEncounter(306, 0), 7 },
            { new MonsterEncounter(67, 0), 7 },

            { new MonsterEncounter(305, 1), 8 }, // Fang
            { new MonsterEncounter(306, 1), 8 },
            { new MonsterEncounter(67, 1), 8 },

            { new MonsterEncounter(309, 0), 9 }, // Dendrobium
            { new MonsterEncounter(310, 0), 9 },

            { new MonsterEncounter(799, 0), 127 }, // Ogre
            { new MonsterEncounter(800, 0), 127 },
            { new MonsterEncounter(203, 0), 127 },
            { new MonsterEncounter(3, 0), 127 },
            { new MonsterEncounter(457, 0), 127 },
            { new MonsterEncounter(1, 0), 127 }
#endif
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

        public static void ProcessMonsterDeath(int battleId, int monsterIndex, Boolean Mastering = false)
        {
            EnsureRewardUiInitialized();
            IncrementMonsterKills(battleId, monsterIndex);

            if (!TryGetBestiaryId(battleId, monsterIndex, out int bestiaryId))
                return;

            if (Mastering)
                UpdateMonsterStatus(battleId, monsterIndex, StatusMastered);

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
