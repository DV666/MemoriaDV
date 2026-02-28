using Memoria.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static BTL_DATA;

namespace Memoria.Scripts.Battle
{
    public static class TranceSeekBattleDictionary
    {
        public static Dictionary<BTL_DATA, TranceSeekFighterState> States = new Dictionary<BTL_DATA, TranceSeekFighterState>();

        public static TranceSeekFighterState GetState(BTL_DATA btl)
        {
            if (!States.TryGetValue(btl, out var state))
            {
                state = new TranceSeekFighterState();
                States[btl] = state;
            }
            return state;
        }

        public static bool ImmuneStatusPlayer = Configuration.Mod.FolderNames.Contains("TranceSeek/ImmuneStatusPlayer");
    }

    public static class TranceSeekExtensions
    {
        public static TranceSeekFighterState State(this BattleUnit unit)
        {
            return TranceSeekBattleDictionary.GetState(unit.Data);
        }

        public static TranceSeekFighterState State(this BTL_DATA btl)
        {
            return TranceSeekBattleDictionary.GetState(btl);
        }

        public static TranceSeekFighterState CasterState(this BattleCalculator calc)
        {
            return TranceSeekBattleDictionary.GetState(calc.Caster.Data);
        }

        public static TranceSeekFighterState TargetState(this BattleCalculator calc)
        {
            return TranceSeekBattleDictionary.GetState(calc.Target.Data);
        }
    }

    public class ZidanePassives
    {
        public int Dodge { get; set; }
        public int Critical { get; set; }

        public Boolean EyeOfTheThief { get; set; }
        public int MasterThief { get; set; }
        public int DaggerAttack { get; set; }
        public RegularItem FirstItemMug { get; set; }
        public RegularItem SecondItemMug { get; set; }
        public int ItemMugMasterThief { get; set; }
        public int MugPlus { get; set; }
        public int StealGil { get; set; }
        public int Flexible { get; set; }
    }

    public class ViviPassives
    {
        public int Focus { get; set; }
        public int NumberTargets { get; set; }
        public Boolean TriggerOneTime { get; set; }
        public BattleAbilityId PreviousSpell { get; set; }
    }

    public class SteinerPassives
    {
        public int StackCMD1 { get; set; }
        public int StackCMD2 { get; set; }
        public Boolean TriggerOneTime { get; set; }
        public int SteinerEnchantedBlade { get; set; } // Need to be replace with infused weapon ?
        public Boolean Sentinel { get; set; }
    }

    public class BeatrixPassives
    {
        public int StackCMD { get; set; }
        public int MagicDummied { get; set; }
        public int Braver { get; set; }
        public Boolean RedemptionTrigger { get; set; }
    }

    public class AmarantPassives
    {
        public Boolean Duel { get; set; }
    }

    public class CinnaPassives
    {
        public int InventionCoolDown { get; set; }
        public int SpringBoots { get; set; }
    }

    public class BakuPassives
    {
        public Boolean Peuh { get; set; }
        public Boolean ThatsAll { get; set; }
        public int InTopForm { get; set; }
    }


    public class GeneralPassives
    {
        public int Critical { get; set; }
        public int TargetCount { get; set; }
    }

    public class MonsterMechanics
    {
        public int Special1 { get; set; }
        public int Special2 { get; set; }
        public Boolean HPBoss10000 { get; set; }
        public int DurationDeadlyStatus { get; set; }
        public int NerfGravity { get; set; }
        public Boolean NoDodge { get; set; }
    }

    public class StackBreakOrUpStatuses
    {
        public int Strength { get; set; }
        public int Magic { get; set; }
        public int PDefence { get; set; }
        public int MDefence { get; set; }
    }

    public class SpecialSAEffects
    {
        public int SentinelDuel { get; set; }
        public int LastStand { get; set; }
        public int Instinct { get; set; }
        public Boolean ModeEX { get; set; }
        public int HealHP { get; set; }
        public int HealMP { get; set; }
        public int CriticalHit100 { get; set; }
        public int OneTriggerSOS { get; set; }
        public int NewMaximumHP { get; set; }
        public int NewMaximumMP { get; set; }
    }

    public class SpecialItemEffects
    {
        public int EmergencySatchel { get; set; }
        public int MagicalSatchel { get; set; }
        public int MechanicalArmor { get; set; }
    }

    public class NewEffectElements
    {
        public int Poison { get; set; }
        public int Gravity { get; set; }
    }

    public class RollbackData
    {
        public Boolean IsSaved { get; set; }
        public uint CurrentHp { get; set; }
        public uint CurrentMp { get; set; }
        public byte Strength { get; set; }
        public byte Magic { get; set; }
        public byte Will { get; set; }
        public int PhysicalDefence { get; set; }
        public int PhysicalEvade { get; set; }
        public int MagicDefence { get; set; }
        public int MagicEvade { get; set; }
        public byte Trance { get; set; }

        public BattleStatus SavedStatus { get; set; }
    }

    public class TranceSeekFighterState
    {
        public ZidanePassives Zidane { get; set; } = new ZidanePassives();
        public ViviPassives Vivi { get; set; } = new ViviPassives();
        public SteinerPassives Steiner { get; set; } = new SteinerPassives();
        public AmarantPassives Amarant { get; set; } = new AmarantPassives();
        public BeatrixPassives Beatrix { get; set; } = new BeatrixPassives();
        public CinnaPassives Cinna { get; set; } = new CinnaPassives();
        public BakuPassives Baku { get; set; } = new BakuPassives();

        public GeneralPassives General { get; set; } = new GeneralPassives();

        public MonsterMechanics Monster { get; set; } = new MonsterMechanics();
        public StackBreakOrUpStatuses StackStatus { get; set; } = new StackBreakOrUpStatuses();
        public SpecialSAEffects SpecialSA { get; set; } = new SpecialSAEffects();
        public SpecialItemEffects SpecialItem { get; set; } = new SpecialItemEffects();
        public NewEffectElements EffectElement { get; set; } = new NewEffectElements();

        public Dictionary<BattleStatus, int> ProtectStatus { get; set; } = new Dictionary<BattleStatus, int>();
        public int AbsorbElement { get; set; }
        public List<WEAPON_MODEL> AdditionalModel { get; set; } = new List<WEAPON_MODEL>();

        public RollbackData Rollback { get; set; } = new RollbackData();

        public Boolean TriggerSPSResistStatus { get; set; }
        public Boolean IsBackAttack { get; set; }
        public Boolean PreventTranceSFX { get; set; }
        public int DragonChanceProc { get; set; }
    }
        /*
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
        */
}
