using Memoria.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Memoria.Scripts.Battle
{
    public static class TranceSeekBattleDictionary
    {
        public static Dictionary<BTL_DATA, Int32[]> ZidanePassive = new Dictionary<BTL_DATA, Int32[]>();
        // [0] => Dodge ; [1] => Critical ; [2] => Eye of the thief ; [3] => Master Thief ; [4] => Dagger Attack ; [5] => FirstItemMug ; [6] => SecondItemMug ; [7] => Mug+ ; [8] => Steal Gil ; [9] => Flexible
        // [10] => FirstItemMugMT ; [11] => SecondItemMugMT

        public static Dictionary<BTL_DATA, Int32[]> ViviPassive = new Dictionary<BTL_DATA, Int32[]>(); // [0] => Focus ; [1] => NumberTargets ; [2] => TriggerOneTime
        public static Dictionary<BTL_DATA, BattleAbilityId> ViviPreviousSpell = new Dictionary<BTL_DATA, BattleAbilityId>();

        public static Dictionary<BTL_DATA, Int32[]> FreyaPassive = new Dictionary<BTL_DATA, Int32[]>(); // [0] => DragonChanceStack
        public static Dictionary<BTL_DATA, Int32[]> SteinerPassive = new Dictionary<BTL_DATA, Int32[]>(); // [0] => StackCMD ; [1] => StackCMD ; [2] => TriggerOneTime
        public static Dictionary<BTL_DATA, Int32[]> BeatrixPassive = new Dictionary<BTL_DATA, Int32[]>(); // [0] => StackCMD ; [1] => Magic (Dummied) ; [2] => Bravoure ; [3] => TargetCount

        public static Dictionary<BTL_DATA, Dictionary<BattleStatus, Int32>> ProtectStatus = new Dictionary<BTL_DATA, Dictionary<BattleStatus, Int32>>();
        public static Dictionary<BTL_DATA, Int32> AbsorbElement = new Dictionary<BTL_DATA, Int32>();
        public static Dictionary<BTL_DATA, GameObject> AdditionalModel = new Dictionary<BTL_DATA, GameObject>();
        public static Dictionary<BTL_DATA, Int32[]> StackBreakOrUpStatus = new Dictionary<BTL_DATA, Int32[]>();
        // [0] => StackStrength ; [1] => StackMagic ; [2] => StackArmor ; [3] => StackMental

        public static Dictionary<BTL_DATA, Int32[]> MonsterMechanic = new Dictionary<BTL_DATA, Int32[]>();
        // [0] => Trance Activated ; [1] => Special1 ; [2] => Special2 ; [3] => HPBoss10000? ; [4] => ResistStatusEasyKill ; [5] => NerfGravity ; [6] => NoDodge

        public static Dictionary<BTL_DATA, Int32[]> SpecialSAEffect = new Dictionary<BTL_DATA, Int32[]>();
        // [0] => Sentinel/Duel ; [1] => LastStand ; [2] => Instinct ; [3] => PreventTranceSFX ; [4] => Mode EX ; [5] => HealHP ; [6] => HealMP ; [7] => TargetCount ; [8] => SpringBoots ; [9] => CriticalHit100 ;
        // [10] => SteinerEnchantedBlade ; [11] => Peuh! ; [12] => That's all ; [13] => In top form! ; [14] => OneTriggerSOS ; [15] => NewMaximumHP ; [16] => NewMaximumMP

        public static Dictionary<BTL_DATA, Int32[]> SpecialItemEffect = new Dictionary<BTL_DATA, Int32[]>();
        // [0] => Emergency Satchel ; [1] => Magical Satchel

        public static Dictionary<BTL_DATA, Int32[]> NewEffectElement = new Dictionary<BTL_DATA, Int32[]>(); // 0 = None, 1 = Weak, 2 = Half, 4 = Immune, 8 = Absorb
        // [0] => Poison ; [1] => Gravity

        public static Dictionary<BTL_DATA, Int32[]> RollBackStats = new Dictionary<BTL_DATA, Int32[]>();
        public static Dictionary<BTL_DATA, Boolean> TriggerSPSResistStatus = new Dictionary<BTL_DATA, Boolean>();
        public static Dictionary<BTL_DATA, BattleStatus> RollBackBattleStatus = new Dictionary<BTL_DATA, BattleStatus>();

        public static Dictionary<BTL_DATA, Boolean> IsBackAttack = new Dictionary<BTL_DATA, Boolean>(); // [TODO] To delete when get fixed on Memoria.

        public static Boolean ImmuneStatusPlayer = Configuration.Mod.FolderNames.Contains("TranceSeek/ImmuneStatusPlayer");
    }
}
