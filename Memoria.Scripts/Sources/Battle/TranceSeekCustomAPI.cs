using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Data;
using UnityEngine;
using static SFX;
using static SiliconStudio.Social.ResponseData;

namespace Memoria.Scripts.Battle
{
    public static class TranceSeekCustomAPI
    {
        public static Dictionary<BTL_DATA, Boolean> InitBTL = new Dictionary<BTL_DATA, Boolean>();

        public static Dictionary<BTL_DATA, Int32[]> ZidanePassive = new Dictionary<BTL_DATA, Int32[]>(); // [0] => Dodge ; [1] => Critical ; [2] => Eye of the thief ; [3] => Master Thief ; [4] => Dagger Attack

        public static Dictionary<BTL_DATA, Int32[]> ViviPassive = new Dictionary<BTL_DATA, Int32[]>(); // [0] => Focus ; [1] => NumberTargets
        public static Dictionary<BTL_DATA, BattleAbilityId> ViviPreviousSpell = new Dictionary<BTL_DATA, BattleAbilityId>();

        public static Dictionary<BTL_DATA, Int32[]> SteinerPassive = new Dictionary<BTL_DATA, Int32[]>(); // [0] => Flawless
        public static Dictionary<BTL_DATA, Int32[]> BeatrixPassive = new Dictionary<BTL_DATA, Int32[]>(); // [0] => Strength ; [1] => Magic ; [2] => Bravoure ; [3] => TargetCount

        public static Dictionary<BTL_DATA, String> ModelEiko = new Dictionary<BTL_DATA, String>();

        public static Dictionary<BTL_DATA, Int32[]> MonsterMechanic = new Dictionary<BTL_DATA, Int32[]>(); // [0] => Trance Activated ; [1] => Special1 ; [2] => Special2 ; [3] => HPBoss10000? ; [4] => ResistStatusEasyKill ; [5] => Dragon

        public static Dictionary<BTL_DATA, Int32[]> SpecialSAEffect = new Dictionary<BTL_DATA, Int32[]>(); // [0] => Millionaire (not used anymore) ; [1] => LastStand ; [2] => Instinct ; [3] => ResetOnDeath
        public static Dictionary<BTL_DATA, Int32[]> PerfectBonus = new Dictionary<BTL_DATA, Int32[]>(); // [0] => Dodge ; [1] => Crit ; [2] => Not used anymore
        public static Dictionary<BTL_DATA, Int32[]> RollBackStats = new Dictionary<BTL_DATA, Int32[]>();
        public static Dictionary<BTL_DATA, BattleStatus> RollBackBattleStatus = new Dictionary<BTL_DATA, BattleStatus>();
        public static Dictionary<BTL_DATA, BattleStatus> StatusBeforeScript = new Dictionary<BTL_DATA, BattleStatus>(); // Lani's mechanic

        public static Dictionary<BTL_DATA, Int32[]> SPSSpecialStatus = new Dictionary<BTL_DATA, Int32[]>();
        // [0] => PowerBreak ; [1] => MagicBreak ; [2] => ArmorBreak ; [3] => MentalBreak
        // [4] => PowerBreak2 ; [5] => MagicBreak2 ; [6] => ArmorBreak2 ; [7] => MentalBreak2
        // [8] => PowerBreak3 ; [9] => MagicBreak3 ; [10] => ArmorBreak3 ; [11] => MentalBreak3
        // [12] => PowerBreak4 ; [13] => MagicBreak4 ; [14] => ArmorBreak4 ; [15] => MentalBreak4
        // [16] => PowerUp ; [17] => MagicUp ; [18] => ArmorUp ; [19] => MentalUp
        // [20] => PowerUp2 ; [21] => MagicUp2 ; [22] => ArmorUp2 ; [23] => MentalUp2
        // [24] => PowerUp3 ; [25] => MagicUp3 ; [26] => ArmorUp3 ; [27] => MentalUp3
        // [28] => PowerUp4 ; [29] => MagicUp4 ; [30] => ArmorUp4 ; [31] => MentalUp4
        // [32] => Virus ; [33] => Dragon
        // [34] => ZombieArmor ; [35] => ZombieArmor2 ; [36] => ZombieArmor3 ; [37] => ZombieArmor4 ; [38] => ZombieArmor5 ; [39] => ZombieArmor6 ; [40] => ZombieArmor7 ; [41] => ZombieArmor8 ; [42] => ZombieArmor9 ; [43] => ZombieArmor10
        // [44] => MechanicalArmor ; [45] => MechanicalArmor2 ; [46] => MechanicalArmor3 ; [47] => MechanicalArmor4 ; [48] => MechanicalArmor5 ; [49] => MechanicalArmor6 ; [50] => MechanicalArmor7 ; [51] => MechanicalArmor8 ; [52] => MechanicalArmor9 ; [53] => MechanicalArmor10
        // [54] => MechanicalArmor11 ; [55] => MechanicalArmor12 ; [56] => MechanicalArmor13 ; [57] => MechanicalArmor14 ; [58] => MechanicalArmor15 ; [59] => MechanicalArmor16 ; [60] => MechanicalArmor17 ; [61] => MechanicalArmor18 ; [62] => MechanicalArmor19 ; [63] => MechanicalArmor20
        // [64] => Redemption ; [65] => Redemption2
        // [66] => Bulwark
        // [67] => PerfectDodge ; [68] => PerfectDodge1 ; [69] => PerfectDodge2 ; [70] => PerfectDodge3 ; [71] => PerfectDodge4 ; [72] => PerfectDodge5 ; [73] => PerfectDodge6 ; [74] => PerfectDodge7 ; [75] => PerfectDodge8 ; [76] => PerfectDodge9
        // [77] => PerfectCrit

        public static void InitCustomBTLDATA(this BattleCalculator v)
        {
            foreach (BattleUnit unit in BattleState.EnumerateUnits())
            {
                if (!InitBTL.TryGetValue(unit.Data, out Boolean init))
                    InitBTL[unit.Data] = false;

                if (!InitBTL[unit.Data])
                {
                    SB2_PATTERN sb2Pattern = FF9StateSystem.Battle.FF9Battle.btl_scene.PatAddr[FF9StateSystem.Battle.FF9Battle.btl_scene.PatNum];
                    foreach (BattleUnit player in BattleState.EnumerateUnits())
                    {
                        if (!player.IsPlayer)
                            continue;

                        if (player.HasSupportAbilityByIndex((SupportAbility)1045)) // Pluriche+
                        {
                            foreach (BattleUnit monster in BattleState.EnumerateUnits())
                            {
                                if (!monster.IsPlayer)
                                {
                                    BattleEnemy battleEnemy = BattleEnemy.Find(monster);
                                    battleEnemy.Data.bonus_item_rate[3] = 16;
                                    battleEnemy.Data.bonus_item_rate[2] = 96;
                                    battleEnemy.Data.bonus_item_rate[1] = 192;
                                }
                            }
                            break;
                        }
                    }

                    if (!ZidanePassive.TryGetValue(unit.Data, out Int32[] zidanepassive))
                        ZidanePassive[unit.Data] = new Int32[] { 0, 0, 0, 0, 0 };
                    if (!ViviPreviousSpell.TryGetValue(unit.Data, out BattleAbilityId e))
                        ViviPreviousSpell[unit.Data] = BattleAbilityId.Void;
                    if (!ViviPassive.TryGetValue(unit.Data, out Int32[] vivipassive))
                        ViviPassive[unit.Data] = new Int32[] { 0, 0 };
                    if (!SteinerPassive.TryGetValue(unit.Data, out Int32[] steinerpassive))
                        SteinerPassive[unit.Data] = new Int32[] { 0, 0 };
                    if (!BeatrixPassive.TryGetValue(unit.Data, out Int32[] beatrixpassive))
                        BeatrixPassive[unit.Data] = new Int32[] { 0, 0, 0, 0 };
                    if (!PerfectBonus.TryGetValue(unit.Data, out Int32[] perfectbonus))
                        PerfectBonus[unit.Data] = new Int32[] { 0, 0, 0 };
                    if (!MonsterMechanic.TryGetValue(unit.Data, out Int32[] monstermechanic))
                        MonsterMechanic[unit.Data] = new Int32[] { 0, 0, 0, 0, 120, 0 };
                    if (!SpecialSAEffect.TryGetValue(unit.Data, out Int32[] specialSAeffect))
                        SpecialSAEffect[unit.Data] = new Int32[] { 0, 0, 2, 0 };
                    if (!RollBackStats.TryGetValue(unit.Data, out Int32[] rb))
                        RollBackStats[unit.Data] = new Int32[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                    if (!RollBackBattleStatus.TryGetValue(unit.Data, out BattleStatus rs))
                        RollBackBattleStatus[unit.Data] = 0;
                    if (!StatusBeforeScript.TryGetValue(unit.Data, out BattleStatus sbs))
                        StatusBeforeScript[unit.Data] = 0;
                    if (!SPSSpecialStatus.TryGetValue(unit.Data, out Int32[] sps))
                        SPSSpecialStatus[unit.Data] = new Int32[] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };


                    if (unit.HasSupportAbilityByIndex((SupportAbility)1041)) // Alert+
                    {
                        PerfectBonus[unit.Data][0] += 2;
                    }
                    if (unit.HasSupportAbilityByIndex((SupportAbility)52)) // Last Stand
                    {
                        SpecialSAEffect[unit.Data][1] = unit.HasSupportAbilityByIndex((SupportAbility)1052) ? 2 : 1;
                    }

                    if (!unit.IsPlayer) // Check if boss have +10000 HP for scripts
                    {
                        for (Int32 i = 0; i < BossBattleBonusHP.GetLength(0); i++)
                        {
                            if (BossBattleBonusHP[i, 0] == FF9StateSystem.Battle.battleMapIndex && BossBattleBonusHP[i, 1] == sb2Pattern.Monster[unit.Data.bi.slot_no].TypeNo)
                            {
                                MonsterMechanic[unit.Data][3] = 1;
                                break;
                            }
                        }
                    }

                    InitBTL[unit.Data] = true;
                }
                if (!unit.IsPlayer && unit.IsUnderAnyStatus(BattleStatus.Trance) && MonsterMechanic[unit.Data][0] == 0 && !unit.IsUnderAnyStatus(BattleStatus.EasyKill)) // +50% HP/MP Max if monster get under Trance
                {
                    MonsterMechanic[unit.Data][0] = 1;
                    unit.MaximumHp += (unit.MaximumHp / 2);
                    unit.MaximumMp += (unit.MaximumMp / 2);
                    unit.CurrentHp = unit.MaximumHp;
                }
            }
            if (v.Caster.Data.dms_geo_id == 410 && MonsterMechanic[v.Caster.Data][2] > 0 && v.Command.ScriptId != 12) // Lamie
            {
                v.Command.AbilityStatus = (BattleStatus)MonsterMechanic[v.Caster.Data][2];
            }
            if (!v.Target.IsPlayer && ((v.Command.AbilityStatus & (BattleStatus.Stop | BattleStatus.Sleep | BattleStatus.Freeze)) != 0))
            {
                if (MonsterMechanic[v.Target.Data][4] <= 20)
                    v.Target.ResistStatus |= (BattleStatus.Stop | BattleStatus.Sleep | BattleStatus.Freeze);
                else
                    StatusBeforeScript[v.Target.Data] = v.Target.CurrentStatus;
            }
        }

        public static void WeaponPhysicalParams(CalcAttackBonus bonus, BattleCalculator v)
        {
            Int32 baseDamage = Comn.random16() % (1 + (v.Caster.Level + v.Caster.Strength >> 3));
            v.Context.AttackPower = v.Caster.GetWeaponPower(v.Command);
            v.Target.SetPhysicalDefense();
            switch (bonus)
            {
                case CalcAttackBonus.Simple:
                    v.Context.Attack = v.Caster.Strength + BeatrixPassive[v.Caster.Data][0] + baseDamage;
                    break;
                case CalcAttackBonus.WillPower:
                    v.Context.Attack = (v.Caster.Strength + BeatrixPassive[v.Caster.Data][0] + v.Caster.Will >> 1) + baseDamage;
                    break;
                case CalcAttackBonus.Dexterity:
                    v.Context.Attack = (v.Caster.Strength + BeatrixPassive[v.Caster.Data][0] + v.Caster.Data.elem.dex >> 1) + baseDamage;
                    break;
                case CalcAttackBonus.Magic:
                    v.Context.Attack = (v.Caster.Strength + BeatrixPassive[v.Caster.Data][0] + v.Caster.Data.elem.mgc + BeatrixPassive[v.Caster.Data][1] >> 1) + baseDamage;
                    break;
                case CalcAttackBonus.Random:
                    v.Context.Attack = Comn.random16() % v.Caster.Strength + BeatrixPassive[v.Caster.Data][0] + baseDamage;
                    break;
                case CalcAttackBonus.Level:
                    v.Context.AttackPower += v.Caster.Data.level;
                    v.Context.Attack = v.Caster.Strength + BeatrixPassive[v.Caster.Data][0] + baseDamage;
                    break;
            }
        }

        public static void TryCriticalHitDragon(this BattleCalculator v)
        {
            Int32 quarterWill = v.Caster.Data.elem.wpr >> 2;
            if (quarterWill != 0 && (Comn.random16() % quarterWill) + v.Caster.Data.critical_rate_deal_bonus + v.Target.Data.critical_rate_receive_bonus > Comn.random16() % 100 || SPSSpecialStatus[v.Target.Data][33] >= 0)
            {
                MonsterMechanic[v.Target.Data][5] = 1;
                Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                    {
                        { "US", "Dragon!" },
                        { "UK", "Dragon!" },
                        { "JP", "Dragon!" },
                        { "ES", "Dragon!" },
                        { "FR", "Dragon!" },
                        { "GR", "Dragon!" },
                        { "IT", "Dragon!" },
                    };
                btl2d.Btl2dReqSymbolMessage(v.Target.Data, "[DC143C]", localizedMessage, HUDMessage.MessageStyle.CRITICAL, 5);
                RemoveSpecialSPS(v.Target.Data, 33);
            }
        }

        public static void TryCriticalHit(this BattleCalculator v)
        {
            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)1102)) // Archimage+ (10% crit en bonus)
                ZidanePassive[v.Caster.Data][1] = 40;
            Int32 quarterWill = (v.Caster.Data.elem.wpr + ZidanePassive[v.Caster.Data][1]) >> 2;
            if (quarterWill != 0 && ((Comn.random16() % quarterWill) + v.Caster.Data.critical_rate_deal_bonus + v.Target.Data.critical_rate_receive_bonus > Comn.random16() % 100) || PerfectBonus[v.Caster.Data][1] > 0)
            {
                if (PerfectBonus[v.Caster.Data][1] > 0)
                    PerfectBonus[v.Caster.Data][1]--;
                else
                    ZidanePassive[v.Caster.Data][1] = 0;
                v.Context.Attack *= 2;
                v.Target.HpDamage *= 2;
                v.Target.MpDamage *= 2;
                v.Target.Flags |= CalcFlag.Critical;
                if (v.Caster.PlayerIndex == CharacterId.Freya)
                    AddSpecialSPS(v.Target.Data, 33, -1, 1.0f);
            }
            else if (v.Caster.PlayerIndex == CharacterId.Zidane && btl_util.getSerialNumber(v.Caster.Data) == CharacterSerialNumber.ZIDANE_SWORD)
            {
                ZidanePassive[v.Caster.Data][1] += 5;
                Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                    {
                        { "US", "↑ Critical ↑" },
                        { "UK", "↑ Critical ↑" },
                        { "JP", "↑ Critical ↑" },
                        { "ES", "↑ Letale ↑" },
                        { "FR", "↑ Critique ↑" },
                        { "GR", "↑ Letal ↑" },
                        { "IT", "↑ KRITISCH ↑" },
                    };
                btl2d.Btl2dReqSymbolMessage(v.Caster.Data, "[FFFF00]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 15);
            }
        }

        public static void IpsenCastleMalus(this BattleCalculator v)
        {
            if (!FF9StateSystem.Battle.FF9Battle.btl_scene.Info.ReverseAttack || !v.Caster.IsPlayer)
                return;

            v.Context.AttackPower = 60 - v.Context.AttackPower;
            if (v.Context.AttackPower < 1)
                v.Context.AttackPower = 1;
        }

        public static Boolean TryPhysicalHit(this BattleCalculator v)
        {
            if (v.Target.Data != v.Caster.Data) {
                foreach (BattleUnit unit in FF9StateSystem.Battle.FF9Battle.EnumerateBattleUnits())
                {
                    if (v.Target.Row != unit.Row && unit.IsUnderAnyStatus(BattleStatus.Death) && (unit.Position == v.Target.Position + 1 || unit.Position == v.Target.Position - 1) && v.Target.HasSupportAbilityByIndex((SupportAbility)109) && SpecialSAEffect[v.Target.Data][2] > 0) // Instinct
                    {
                        if (v.Target.HasSupportAbilityByIndex((SupportAbility)1109)) // Instinct+
                        {
                            SpecialSAEffect[v.Target.Data][2]--;
                        }
                        else
                        {
                            SpecialSAEffect[v.Target.Data][2] = 0;
                        }
                        v.Context.HitRate = 0;
                        Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                        {
                            { "US", "Instinct!" },
                            { "UK", "Instinct!" },
                            { "JP", "Instinct!" },
                            { "ES", "Instinct!" },
                            { "FR", "Instinct !" },
                            { "GR", "Instinct!" },
                            { "IT", "Instinct!" },
                        };
                        btl2d.Btl2dReqSymbolMessage(v.Target.Data, "[FDEE00]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 10);
                    }
                }
            }

            if (v.Caster.PlayerIndex == CharacterId.Zidane && v.Command.Id == BattleCommandId.Attack && ff9item._FF9Item_Data[FF9StateSystem.Common.FF9.player[(CharacterId)v.Caster.Data.bi.slot_no].equip[0]].shape == 1) // Zidane - Dagger double hits
            {
                ZidanePassive[v.Caster.Data][4] = ZidanePassive[v.Caster.Data][4] == 0 ? 1 : 2;
                if ((v.Command.AbilityCategory & 64) != 0)
                    v.Command.AbilityCategory -= 64;
            }

            if (BeatrixPassive[v.Caster.Data][2] == 2) // Héroïsme de Beatrix
                v.Context.HitRate += 25;

            if (v.Target.PlayerIndex == CharacterId.Zidane)
                v.Context.Evade += ZidanePassive[v.Target.Data][0];

            if (v.Caster.IsUnderAnyStatus(BattleStatus.Blind))
                v.Context.HitRate /= v.Caster.IsUnderAnyStatus(BattleStatus.EasyKill) ? (4 / 3) : 2;

            if (v.Caster.IsUnderAnyStatus(BattleStatus.Trance | BattleStatus.Vanish))
                v.Context.Evade = 0;

            if (v.Target.IsUnderAnyStatus(BattleStatusConst.PenaltyEvade))
                v.Context.Evade = 0;

            if (v.Target.IsUnderAnyStatus(BattleStatus.Defend))
            {
                if (v.Target.HasSupportAbilityByIndex(SupportAbility.AutoFloat)) // Pas Léger
                {
                    v.Context.Evade /= 2;
                }
                else if (!v.Target.HasSupportAbilityByIndex((SupportAbility)1001))
                {
                    v.Context.Evade = 0;
                }
            }

            v.Target.PenaltyBanishHitRate();

            if (PerfectBonus[v.Target.Data][0] > 0 && !v.Caster.HasSupportAbility(SupportAbility1.Healer))
            {
                SPSCumulative(v.Target.Data, SPSStackable.SuperDodge, false);
                v.Context.Flags |= BattleCalcFlags.Miss;
                return false;
            }

            foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(v.Caster.Data.saExtended))
                saFeature.TriggerOnAbility(v, "HitRateSetup", false);
            foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(v.Target.Data.saExtended))
                saFeature.TriggerOnAbility(v, "HitRateSetup", true);

            if ((v.Context.HitRate <= Comn.random16() % 100) || v.Target.PhysicalEvade == 255)
            {
                v.Context.Flags |= BattleCalcFlags.Miss;
                return false;
            }

            if (v.Target.PhysicalDefence == 255 || (SPSSpecialStatus[v.Target.Data][66] != -1 && !v.Caster.HasSupportAbility(SupportAbility1.Healer)))
            {
                RemoveSpecialSPS(v.Target, 66);
                v.Context.Flags |= BattleCalcFlags.Guard;
                return false;
            }
            if (v.Target.Data == v.Caster.Data || (v.Context.Evade + (v.Target.PlayerIndex == CharacterId.Zidane ? ZidanePassive[v.Target.Data][0] : 0)) <= Comn.random16() % 100 || v.Context.Evade == 0)
            {
                if (v.Target.PlayerIndex == CharacterId.Zidane && btl_util.getSerialNumber(v.Target.Data) == CharacterSerialNumber.ZIDANE_DAGGER && !v.Target.IsUnderAnyStatus(BattleStatusConst.BattleEndFull))
                {
                    ZidanePassive[v.Target.Data][0] += 5;
                    Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                    {
                        { "US", "↑ Dodge ↑" },
                        { "UK", "↑ Dodge ↑" },
                        { "JP", "↑ かいひりつ ↑" },
                        { "ES", "↑ DST fisica ↑" },
                        { "FR", "↑ Esquive ↑" },
                        { "GR", "↑ Evasión F ↑" },
                        { "IT", "↑ Reflex ↑" },
                    };
                    btl2d.Btl2dReqSymbolMessage(v.Target.Data, "[FFFF00]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 20);
                }    
                return true;
            }
            if (v.Target.PlayerIndex == CharacterId.Zidane)
                ZidanePassive[v.Target.Data][0] = 0;

            v.Context.Flags |= BattleCalcFlags.Miss;

            if (!v.Target.IsCovering)
                v.Context.Flags |= BattleCalcFlags.Dodge;

            return false;
        }

        public static Boolean CanAttackMagic(this BattleCalculator v)
        {
            if (v.Target.IsLevitate && v.Command.IsGround)
            {
                v.Context.Flags |= BattleCalcFlags.Miss;
                return false;
            }

            if (v.Target.CanGuardElement(v.Command.Element))
                return false;

            v.Target.PenaltyHalfElement(v.Command.Element);
            v.Target.BonusWeakElement(v.Command.Element);
            if (v.Target.CanAbsorbElement(v.Command.Element))
            {
                // v.Context.DefensePower = 0;
            }
            v.Target.AlterStatuses(v.Command.Element);

            if (v.Target.PlayerIndex == CharacterId.Beatrix)
            {
                v.Context.DefensePower += BeatrixPassive[v.Caster.Data][1];
            }

            return true;
        }

        public static Boolean TryMagicHitWithoutBattleCalcFlag(this BattleCalculator v)
        {
            foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(v.Caster))
                saFeature.TriggerOnAbility(v, "HitRateSetup", false);
            foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(v.Target))
                saFeature.TriggerOnAbility(v, "HitRateSetup", true);

            if (v.Context.HitRate <= Comn.random16() % 100)
            {
                return false;
            }

            if (v.Context.Evade > Comn.random16() % 100)
            {
                return false;
            }

            return true;
        }

        public static void TargetPhysicalPenaltyAndBonusAttack(this BattleCalculator v)
        {
            if (v.Target.PlayerIndex == CharacterId.Beatrix)
            {
                v.Context.DefensePower += BeatrixPassive[v.Caster.Data][0];
            }

            if (v.Target.PlayerIndex == CharacterId.Steiner && v.Target.IsUnderAnyStatus(BattleStatus.Defend) && SteinerPassive[v.Target.Data][0] > 0 && v.Target.HasSupportAbilityByIndex((SupportAbility)118)) // Flawless Steiner
            {
                SteinerPassive[v.Target.Data][0] = 0;
                if (v.Target.HasSupportAbilityByIndex((SupportAbility)1118))
                {
                    v.Context.Flags |= BattleCalcFlags.Guard;
                    return;
                }
                else if (GameRandom.Next8() % 2 != 0)
                {
                    v.Context.Flags |= BattleCalcFlags.Guard;
                    return;
                }
            }

            if (v.Target.PlayerIndex == CharacterId.Steiner && v.Target.IsUnderAnyStatus(BattleStatus.Defend)) // Gardien Steiner
                v.Context.Attack >>= 2;
            else if (v.Target.IsUnderAnyStatus(BattleStatus.Defend))
                v.Context.Attack >>= 1;

            if (v.Target.PlayerIndex == CharacterId.Steiner && v.Target.IsUnderAnyStatus(BattleStatus.Trance)) // Steiner Trance => 25% reduce physical damage
            {
                v.Context.Attack = (3 * v.Context.Attack) / 4;
            }

            if (v.Target.IsUnderAnyStatus(BattleStatus.Protect))
                v.Context.Attack >>= 1;

            if (v.Target.IsUnderAnyStatus(BattleStatus.Mini) || v.Target.IsUnderAnyStatus(BattleStatus.Sleep) && !v.Target.IsUnderAnyStatus(BattleStatus.EasyKill) || v.Target.IsUnderAnyStatus(BattleStatus.Freeze))
                v.Context.Attack = (Int16)(v.Context.Attack * 3 >> 1);

            MalusOrBonusStats(v, true);
        }

        public static void BonusBackstabAndPenaltyLongDistanceTranceSeek(this BattleCalculator v)
        {
            if ((Math.Abs(v.Caster.Data.evt.rotBattle.eulerAngles.y - v.Target.Data.evt.rotBattle.eulerAngles.y) < 0.1) || v.Target.IsRunningAway())
                v.Context.Attack = v.Context.Attack * 3 >> 1;

            if (Mathf.Abs(v.Caster.Row - v.Target.Row) > 1 && !v.Caster.HasLongRangeWeapon && v.Command.IsShortRange && v.Caster.IsPlayer)
            {
                v.Context.Attack /= 2;
                return;
            }
            if (!v.Caster.IsPlayer)
            {
                Boolean longDistance = false;
                if (v.Target.IsPlayer && Mathf.Abs(v.Caster.Row - v.Target.Row) > 1)
                {
                    foreach (BattleUnit unit in FF9StateSystem.Battle.FF9Battle.EnumerateBattleUnits())
                    {
                        if (!unit.IsPlayer || unit.Position == v.Target.Position)
                            continue;

                        if (v.Target.Row != unit.Row && !unit.IsUnderAnyStatus(BattleStatusConst.NoReaction) && (unit.Position == v.Target.Position + 1 || unit.Position == v.Target.Position - 1))
                            longDistance = true;
                    }
                }
                if (longDistance)
                    v.Context.Attack /= 2;
            }
        }

        public static void PenaltyShellAttack(this BattleCalculator v)
        {
            if (v.Target.MagicDefence == 255)
            {
                v.Context.Attack = 0;
                v.Context.Flags |= BattleCalcFlags.Guard;
                return;
            }

            if (v.Target.IsUnderAnyStatus(BattleStatus.Shell))
                v.Context.Attack >>= 1;

            if (v.Caster.IsUnderAnyStatus(BattleStatus.Silence) && v.Caster.IsUnderAnyStatus(BattleStatus.EasyKill)) // 10% magic attack malus for bosses with Silence.
                v.Context.Attack = (9 * v.Context.Attack) / 10;

            ViviFocus(v);
            MalusOrBonusStats(v, false);
        }

        public static void MalusOrBonusStats(this BattleCalculator v, Boolean IsPhysical)
        {
            if (IsPhysical)
            {

                if (SPSSpecialStatus[v.Caster.Data][0] != -1) // PowerBreak
                {
                    v.Context.Attack -= v.Caster.IsUnderAnyStatus(BattleStatus.EasyKill) ? (1 * v.Context.Attack) / 10 : v.Context.Attack / 4;
                }
                else if (SPSSpecialStatus[v.Caster.Data][4] != -1) // PowerBreak2
                {
                    v.Context.Attack -= v.Caster.IsUnderAnyStatus(BattleStatus.EasyKill) ? (15 * v.Context.Attack) / 100 : v.Context.Attack / 2;
                }
                else if (SPSSpecialStatus[v.Caster.Data][8] != -1) // PowerBreak3
                {
                    v.Context.Attack -= v.Caster.IsUnderAnyStatus(BattleStatus.EasyKill) ? (2 * v.Context.Attack) / 10 : (3 * v.Context.Attack) / 4;
                }
                else if (SPSSpecialStatus[v.Caster.Data][12] != -1) // PowerBreak4
                {
                    v.Context.Attack -= v.Caster.IsUnderAnyStatus(BattleStatus.EasyKill) ? (25 * v.Context.Attack) / 100 : v.Context.Attack;
                }
                if (SPSSpecialStatus[v.Target.Data][2] != -1) // ArmorBreak
                {
                    v.Context.Attack += v.Target.IsUnderAnyStatus(BattleStatus.EasyKill) ? (1 * v.Context.Attack) / 10 : v.Context.Attack / 4;
                }
                else if (SPSSpecialStatus[v.Target.Data][6] != -1) // ArmorBreak2
                {
                    v.Context.Attack += v.Target.IsUnderAnyStatus(BattleStatus.EasyKill) ? (15 * v.Context.Attack) / 100 : v.Context.Attack / 2;
                }
                else if (SPSSpecialStatus[v.Target.Data][10] != -1) // ArmorBreak3
                {
                    v.Context.Attack += v.Target.IsUnderAnyStatus(BattleStatus.EasyKill) ? (2 * v.Context.Attack) / 10 : (3 * v.Context.Attack) / 4;
                }
                else if (SPSSpecialStatus[v.Target.Data][14] != -1) // ArmorBreak4
                {
                    v.Context.Attack += v.Target.IsUnderAnyStatus(BattleStatus.EasyKill) ? (25 * v.Context.Attack) / 100 : v.Context.Attack;
                }
                if (SPSSpecialStatus[v.Caster.Data][16] != -1) // PowerUp
                {
                    v.Context.Attack += v.Context.Attack / 4;
                }
                else if (SPSSpecialStatus[v.Caster.Data][20] != -1) // PowerUp2
                {
                    v.Context.Attack += v.Context.Attack / 2;
                }
                else if (SPSSpecialStatus[v.Caster.Data][24] != -1) // PowerUp3
                {
                    v.Context.Attack += (3 * v.Context.Attack) / 4;
                }
                else if (SPSSpecialStatus[v.Caster.Data][28] != -1) // PowerUp4
                {
                    v.Context.Attack += v.Context.Attack;
                }
                if (SPSSpecialStatus[v.Target.Data][18] != -1) // ArmorUp
                {
                    v.Context.Attack -= v.Context.Attack / 4;
                }
                else if (SPSSpecialStatus[v.Target.Data][22] != -1) // ArmorUp2
                {
                    v.Context.Attack -= v.Context.Attack / 2;
                }
                else if (SPSSpecialStatus[v.Target.Data][26] != -1) // ArmorUp3
                {
                    v.Context.Attack -= (3 * v.Context.Attack) / 4;
                }
                else if (SPSSpecialStatus[v.Target.Data][30] != -1) // ArmorUp4
                {
                    v.Context.Attack -= v.Context.Attack;
                }
            }
            else
            {
                if (SPSSpecialStatus[v.Caster.Data][1] != -1) // MagicBreak
                {
                    v.Context.Attack -= v.Caster.IsUnderAnyStatus(BattleStatus.EasyKill) ? (1 * v.Context.Attack) / 10 : v.Context.Attack / 4;
                }
                else if (SPSSpecialStatus[v.Caster.Data][5] != -1) // MagicBreak2
                {
                    v.Context.Attack -= v.Caster.IsUnderAnyStatus(BattleStatus.EasyKill) ? (15 * v.Context.Attack) / 100 : v.Context.Attack / 2;
                }
                else if (SPSSpecialStatus[v.Caster.Data][9] != -1) // MagicBreak3
                {
                    v.Context.Attack -= v.Caster.IsUnderAnyStatus(BattleStatus.EasyKill) ? (2 * v.Context.Attack) / 10 : (3 * v.Context.Attack) / 4;
                }
                else if (SPSSpecialStatus[v.Caster.Data][13] != -1) // MagicBreak4
                {
                    v.Context.Attack -= v.Caster.IsUnderAnyStatus(BattleStatus.EasyKill) ? (25 * v.Context.Attack) / 100 : v.Context.Attack;
                }
                if (SPSSpecialStatus[v.Target.Data][3] != -1) // MentalBreak
                {
                    v.Context.Attack += v.Target.IsUnderAnyStatus(BattleStatus.EasyKill) ? (1 * v.Context.Attack) / 10 : v.Context.Attack / 4;
                }
                else if (SPSSpecialStatus[v.Target.Data][7] != -1) // MentalBreak2
                {
                    v.Context.Attack += v.Target.IsUnderAnyStatus(BattleStatus.EasyKill) ? (15 * v.Context.Attack) / 100 : v.Context.Attack / 2;
                }
                else if (SPSSpecialStatus[v.Target.Data][11] != -1) // MentalBreak3
                {
                    v.Context.Attack += v.Target.IsUnderAnyStatus(BattleStatus.EasyKill) ? (2 * v.Context.Attack) / 10 : (3 * v.Context.Attack) / 4;
                }
                else if (SPSSpecialStatus[v.Target.Data][15] != -1) // MentalBreak4
                {
                    v.Context.Attack += v.Target.IsUnderAnyStatus(BattleStatus.EasyKill) ? (25 * v.Context.Attack) / 100 : v.Context.Attack;
                }
                if (SPSSpecialStatus[v.Caster.Data][17] != -1) // MagicUp
                {
                    v.Context.Attack += v.Context.Attack / 4;
                }
                else if (SPSSpecialStatus[v.Caster.Data][21] != -1) // MagicUp2
                {
                    v.Context.Attack += v.Context.Attack / 2;
                }
                else if (SPSSpecialStatus[v.Caster.Data][25] != -1) // MagicUp3
                {
                    v.Context.Attack += (3 * v.Context.Attack) / 4;
                }
                else if (SPSSpecialStatus[v.Caster.Data][29] != -1) // MagicUp4
                {
                    v.Context.Attack += v.Context.Attack;
                }
                if (SPSSpecialStatus[v.Target.Data][19] != -1) // MentalUp
                {
                    v.Context.Attack -= v.Context.Attack / 4;
                }
                else if (SPSSpecialStatus[v.Target.Data][23] != -1) // MentalUp2
                {
                    v.Context.Attack -= v.Context.Attack / 2;
                }
                else if (SPSSpecialStatus[v.Target.Data][27] != -1) // MentalUp3
                {
                    v.Context.Attack -= (3 * v.Context.Attack) / 4;
                }
                else if (SPSSpecialStatus[v.Target.Data][31] != -1) // MentalUp4
                {
                    v.Context.Attack -= v.Context.Attack;
                }
            }
        }

        public static void ViviFocus(this BattleCalculator v)
        {
            if (v.Caster.PlayerIndex == CharacterId.Vivi)
            {
                if ((v.Command.Id == BattleCommandId.BlackMagic || v.Command.Id == BattleCommandId.DoubleBlackMagic))
                {
                    if (ViviPassive[v.Caster.Data][1] == 0)
                    {
                        ViviPassive[v.Caster.Data][1] = (ushort)(v.Command.TargetCount);
                        if (FF9TextTool.ActionAbilityName(ViviPreviousSpell[v.Caster.Data]) != v.Command.AbilityName)
                        {
                            if (ViviPassive[v.Caster.Data][0] < 50)
                            {
                                ViviPassive[v.Caster.Data][0] += 5;
                            }
                            Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                                {
                                    { "US", $"Focus +{ViviPassive[v.Caster.Data][0]}%!" },
                                    { "UK", $"Focus +{ViviPassive[v.Caster.Data][0]}%!" },
                                    { "JP", $"フォーカス +{ViviPassive[v.Caster.Data][0]}%!" },
                                    { "ES", $"¡Focus +{ViviPassive[v.Caster.Data][0]}%!" },
                                    { "FR", $"Focus +{ViviPassive[v.Caster.Data][0]}% !" },
                                    { "GR", $"Focus +{ViviPassive[v.Caster.Data][0]}%!" },
                                    { "IT", $"Focus +{ViviPassive[v.Caster.Data][0]}%!" },
                                };
                            btl2d.Btl2dReqSymbolMessage(v.Caster.Data, "[BA55D3]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 40);
                        }
                        else
                        {
                            ViviPassive[v.Caster.Data][0] = 0;
                            Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                            {
                                { "US", "- Focus!" },
                                { "UK", "- Focus!" },
                                { "JP", "- フォーカス!" },
                                { "ES", "¡- Focus!" },
                                { "FR", "- Focus !" },
                                { "GR", "- Focus!" },
                                { "IT", "- Focus!" },
                            };
                            btl2d.Btl2dReqSymbolMessage(v.Caster.Data, "[DC143C]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 40);
                        }
                        ViviPreviousSpell[v.Caster.Data] = v.Command.AbilityId;
                    }
                    ViviPassive[v.Caster.Data][1]--;
                    if (ViviPassive[v.Caster.Data][1] < 0)
                        ViviPassive[v.Caster.Data][1] = 0;

                    v.Context.Attack += (v.Context.Attack * ViviPassive[v.Caster.Data][0]) / 100;
                    v.Command.HitRate += (v.Command.HitRate * ViviPassive[v.Caster.Data][0]) / 100;
                }
            }
            else if (v.Caster.PlayerIndex == CharacterId.Steiner && v.Command.Id == BattleCommandId.MagicSword)
            {
                if (ViviPassive[v.Caster.Data][1] == 0)
                {
                    ViviPassive[v.Caster.Data][1] = (ushort)(v.Command.TargetCount);
                    foreach (BattleUnit Vivi in BattleState.EnumerateUnits())
                    {
                        if (Vivi.IsPlayer && Vivi.PlayerIndex == CharacterId.Vivi)
                        {
                            if (FF9TextTool.ActionAbilityName(ViviPreviousSpell[Vivi.Data]) != v.Command.AbilityName)
                            {
                                if (ViviPassive[Vivi.Data][0] < 50)
                                {
                                    ViviPassive[Vivi.Data][0] += 5;
                                }
                                Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                                {
                                { "US", $"Focus +{ViviPassive[Vivi.Data][0]}%!" },
                                { "UK", $"Focus +{ViviPassive[Vivi.Data][0]}%!" },
                                { "JP", $"フォーカス +{ViviPassive[Vivi.Data][0]}%!" },
                                { "ES", $"¡Focus +{ViviPassive[Vivi.Data][0]}%!" },
                                { "FR", $"Focus +{ViviPassive[Vivi.Data][0]}% !" },
                                { "GR", $"Focus +{ViviPassive[Vivi.Data][0]}%!" },
                                { "IT", $"Focus +{ViviPassive[Vivi.Data][0]}%!" },
                                };
                                btl2d.Btl2dReqSymbolMessage(Vivi.Data, "[BA55D3]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 40);
                                v.Context.Attack += v.Context.Attack * (ViviPassive[Vivi.Data][0] / 100);
                                v.Command.HitRate += v.Command.HitRate * (ViviPassive[Vivi.Data][0] / 100);
                            }
                            else
                            {
                                ViviPassive[Vivi.Data][0] = 0;
                                Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                                {
                                { "US", "- Focus!" },
                                { "UK", "- Focus!" },
                                { "JP", "- フォーカス!" },
                                { "ES", "¡- Focus!" },
                                { "FR", "- Focus !" },
                                { "GR", "- Focus!" },
                                { "IT", "- Focus!" },
                                };
                                btl2d.Btl2dReqSymbolMessage(Vivi.Data, "[DC143C]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 40);
                            }
                            ViviPreviousSpell[Vivi.Data] = v.Command.AbilityId;
                        }
                        ViviPassive[v.Caster.Data][1]--;
                        if (ViviPassive[v.Caster.Data][1] < 0)
                            ViviPassive[v.Caster.Data][1] = 0;
                    }
                }  
            }
        }

        public static void AmarantPassive(this BattleCalculator v)
        {
            const UInt32 goodstatus = (UInt32)
            (BattleStatus.AutoLife | BattleStatus.Trance | BattleStatus.Defend | BattleStatus.Regen | BattleStatus.Haste 
            | BattleStatus.Float | BattleStatus.Shell | BattleStatus.Protect | BattleStatus.Vanish | BattleStatus.Reflect);

            Int32 factor = 0;
            UInt32 status = 1;
            for (Int32 i = 0; i < 32; i++, status <<= 1)
            {
                if ((status & goodstatus) != 0 || !v.Target.IsUnderStatus((BattleStatus)status))
                    continue;

                factor++;
                if (status == 32 && v.Target.IsUnderStatus(BattleStatus.EasyKill)) // Trouble++ on Bosses.
                    factor++;
            }

            if (v.Target.Data.special_status_old)
                factor++;

            if (factor > 0)
                v.Context.Attack += (v.Context.Attack * factor * 8) / 100;
        }

        public static void CharacterBonusPassive(this BattleCalculator v, string mode = "")
        {
            if (v.Caster.PlayerIndex == CharacterId.Beatrix)
            {
                switch (mode)
                {
                    case "MagicAttack":
                    {
                        v.Context.Attack = (Int16)(v.Caster.Magic + Comn.random16() % (1 + (v.Caster.Level + v.Caster.Magic + BeatrixPassive[v.Caster.Data][1] >> 3)));
                        break;
                    }
                    case "PhysicalAttack":
                    {
                        v.Context.Attack = (Int16)(v.Caster.Strength + Comn.random16() % (1 + (v.Caster.Level + v.Caster.Strength + BeatrixPassive[v.Caster.Data][0] >> 2)));
                        break;
                    }
                    case "LowPhysicalAttack":
                    {
                        v.Context.Attack = (Int16)(v.Caster.Strength + Comn.random16() % (1 + (v.Caster.Level + v.Caster.Strength + BeatrixPassive[v.Caster.Data][0] >> 3)));
                        break;
                    }
                }
            }
            else if (v.Caster.PlayerIndex == CharacterId.Marcus)
            {
                if (mode == "MagicAttack")
                {
                    v.Context.Attack = (Int16)(v.Caster.Strength + Comn.random16() % (1 + (v.Caster.Level + v.Caster.Strength >> 3)));
                }
            }
        }

        public static void TryAlterCommandStatuses(this BattleCalculator v)
        {
            byte CasterWill = v.Caster.Will;
            byte CasterNewWill = v.Caster.Will;
            if (v.Target.HasSupportAbilityByIndex((SupportAbility)1000))  // Bénédiction+
            {
                CasterNewWill += CasterNewWill;
            }
            else if (v.Target.HasSupportAbilityByIndex(SupportAbility.AutoReflect)) // Bénédiction
            {
                CasterNewWill += (byte)(CasterNewWill / 2);
            }
            if (v.Caster.PlayerIndex == CharacterId.Garnet && (v.Caster.Accessory == RegularItem.Ruby || v.Caster.IsUnderAnyStatus(BattleStatus.Trance)))  // Bonus Ruby Dagga
            {
                CasterNewWill += (byte)(CasterNewWill * ff9item.FF9Item_GetCount(RegularItem.Ruby) / 200);
            }
            v.Caster.Will = (byte)(CasterNewWill < 100 ? CasterNewWill : 99);
            BattleStatus status = v.Command.AbilityStatus;
            v.Target.TryAlterStatuses(status, true, v.Caster);
            v.Caster.Will = CasterWill;
        }

        public static void RaiseTrouble(this BattleCalculator v)
        {
            if (v.Target.PhysicalDefence == 255 || v.Target.PhysicalDefence == 255 || v.Target.MagicDefence == 255 || v.Target.MagicEvade == 255)
                if (v.Command.Data.tar_id == v.Target.Id && v.Target.IsUnderAnyStatus(BattleStatus.Trouble) && (v.Context.AddedStatuses & BattleStatus.Trouble) == 0 && (v.Target.Flags & CalcFlag.HpRecovery) == 0)
                    v.Target.Data.fig_info |= Param.FIG_INFO_TROUBLE;
        }

        public static void AddSpecialSPS(BTL_DATA btl, uint spstype, int bone, float scale, bool rotate = false)
        {
            for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next)
            {
                if ((next.btl_id & btl.btl_id) != 0 && next.bi.disappear == 0 && SPSSpecialStatus[next][spstype] == -1)
                {
                    HonoluluBattleMain.battleSPS.AddSpecialSPSObj(-1, (spstype + 15), next, bone, scale, out int SPSindex, rotate);
                    SPSSpecialStatus[next][spstype] = SPSindex;
                }
            }
        }

        public static void RemoveSpecialSPS(BTL_DATA btl, uint spstype)
        {
            for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next)
            {
                if ((next.btl_id & btl.btl_id) != 0 && next.bi.disappear == 0 && SPSSpecialStatus[next][spstype] != -1)
                {
                    HonoluluBattleMain.battleSPS.RemoveSpecialSPSObj(SPSSpecialStatus[next][spstype]);
                    SPSSpecialStatus[next][spstype] = -1;
                }
            }

        }

        public static void SPSCumulative(BTL_DATA btl, SPSStackable sps, Boolean increase = true)
        {
            switch (sps)
            {
                case SPSStackable.SuperDodge:
                    {
                        RemoveSpecialSPS(btl, (uint)(66 + PerfectBonus[btl][0]));
                        if (increase)                         
                            PerfectBonus[btl][0]++;                         
                        else
                            PerfectBonus[btl][0]--;

                        if (PerfectBonus[btl][0] > 0)
                            AddSpecialSPS(btl, (uint)(66 + PerfectBonus[btl][0]), -1, 1.0f);
                        else
                            PerfectBonus[btl][0] = 0;
                        break;
                    }
                case SPSStackable.ZombieArmor:
                    {
                        RemoveSpecialSPS(btl, 42);
                        RemoveSpecialSPS(btl, 41);
                        RemoveSpecialSPS(btl, 40);
                        RemoveSpecialSPS(btl, 39);
                        RemoveSpecialSPS(btl, 38);
                        RemoveSpecialSPS(btl, 37);
                        RemoveSpecialSPS(btl, 36);
                        RemoveSpecialSPS(btl, 35);
                        RemoveSpecialSPS(btl, 34);

                        if (SPSSpecialStatus[btl][43] == -1 && btl.defence.PhysicalDefence > 46) // ZombieArmor * 10
                        {
                            AddSpecialSPS(btl, 43, -1, 1.0f);
                        }
                        else if (SPSSpecialStatus[btl][42] == -1 && btl.defence.PhysicalDefence > 42) // ZombieArmor * 9
                        {
                            AddSpecialSPS(btl, 42, -1, 1.0f);
                        }
                        else if (SPSSpecialStatus[btl][41] == -1 && btl.defence.PhysicalDefence > 38) // ZombieArmor * 8
                        {
                            AddSpecialSPS(btl, 41, -1, 1.0f);
                        }
                        else if (SPSSpecialStatus[btl][40] == -1 && btl.defence.PhysicalDefence > 34) // ZombieArmor * 7
                        {
                            AddSpecialSPS(btl, 40, -1, 1.0f);
                        }
                        else if (SPSSpecialStatus[btl][39] == -1 && btl.defence.PhysicalDefence > 30) // ZombieArmor * 6
                        {
                            AddSpecialSPS(btl, 39, -1, 1.0f);
                        }
                        else if (SPSSpecialStatus[btl][38] == -1 && btl.defence.PhysicalDefence > 26) // ZombieArmor * 5
                        {
                            AddSpecialSPS(btl, 38, -1, 1.0f);
                        }
                        else if (SPSSpecialStatus[btl][37] == -1 && btl.defence.PhysicalDefence > 22) // ZombieArmor * 4
                        {
                            AddSpecialSPS(btl, 37, -1, 1.0f);
                        }
                        else if (SPSSpecialStatus[btl][36] == -1 && btl.defence.PhysicalDefence > 18) // ZombieArmor * 3
                        {
                            AddSpecialSPS(btl, 36, -1, 1.0f);
                        }
                        else if (SPSSpecialStatus[btl][35] == -1 && btl.defence.PhysicalDefence > 14) // ZombieArmor * 2
                        {
                            AddSpecialSPS(btl, 35, -1, 1.0f);
                        }
                        else if (SPSSpecialStatus[btl][34] == -1 && btl.defence.PhysicalDefence > 10) // ZombieArmor
                        {
                            AddSpecialSPS(btl, 34, -1, 1.0f);
                        }                    
                        break;
                    }
                case SPSStackable.MechanicalArmor:
                {
                    for (Int32 i = 0; i < 20; i++)
                    {
                        RemoveSpecialSPS(btl, 44 + (uint)i);
                    }
                    if (MonsterMechanic[btl][1] > 0)
                    {
                        uint indexsps = (uint)(MonsterMechanic[btl][1] / 10);
                        AddSpecialSPS(btl, 43 + indexsps, -1, 1.0f);
                    }
                    break;
                }
                case SPSStackable.Redemption:
                    {
                        BattleUnit unit = new BattleUnit(btl);
                        if (SPSSpecialStatus[btl][64] == -1 && SPSSpecialStatus[btl][65] == -1)
                        {
                            unit.SummonCount = 1;
                            AddSpecialSPS(btl, 64, -1, 1.0f);
                        }
                        else if (SPSSpecialStatus[btl][64] != -1 && SPSSpecialStatus[btl][65] == -1)
                        {
                            unit.SummonCount = 2;
                            RemoveSpecialSPS(btl, 64);
                            AddSpecialSPS(btl, 65, -1, 1.0f);
                        }
                        break;
                    }
            }
        }

        public static void BonusOrMalusSPS(BTL_DATA btl, string type, Boolean buff)
        {
            switch (type)
            {
                case "Strength":
                    {
                        if (buff && SPSSpecialStatus[btl][28] == -1)
                        {
                            if (SPSSpecialStatus[btl][24] != -1)
                            {
                                RemoveSpecialSPS(btl, 24);
                                AddSpecialSPS(btl, 28, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][20] != -1)
                            {
                                RemoveSpecialSPS(btl, 20);
                                AddSpecialSPS(btl, 24, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][16] != -1)
                            {
                                RemoveSpecialSPS(btl, 16);
                                AddSpecialSPS(btl, 20, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][12] != -1)
                            {
                                RemoveSpecialSPS(btl, 12);
                                AddSpecialSPS(btl, 8, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][8] != -1)
                            {
                                RemoveSpecialSPS(btl, 8);
                                AddSpecialSPS(btl, 4, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][4] != -1)
                            {
                                RemoveSpecialSPS(btl, 4);
                                AddSpecialSPS(btl, 0, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][0] != -1)
                            {
                                RemoveSpecialSPS(btl, 0);
                            }
                            else
                            {
                                AddSpecialSPS(btl, 16, -1, 1.0f);
                            }
                        }
                        else if (!buff && SPSSpecialStatus[btl][12] == -1)
                        {
                            if (SPSSpecialStatus[btl][28] != -1)
                            {
                                RemoveSpecialSPS(btl, 28);
                                AddSpecialSPS(btl, 24, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][24] != -1)
                            {
                                RemoveSpecialSPS(btl, 24);
                                AddSpecialSPS(btl, 20, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][20] != -1)
                            {
                                RemoveSpecialSPS(btl, 20);
                                AddSpecialSPS(btl, 16, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][16] != -1)
                            {
                                RemoveSpecialSPS(btl, 16);
                            }
                            else if (SPSSpecialStatus[btl][0] != -1)
                            {
                                RemoveSpecialSPS(btl, 0);
                                AddSpecialSPS(btl, 4, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][4] != -1)
                            {
                                RemoveSpecialSPS(btl, 4);
                                AddSpecialSPS(btl, 8, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][8] != -1)
                            {
                                RemoveSpecialSPS(btl, 8);
                                AddSpecialSPS(btl, 12, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][12] != -1)
                            {
                                RemoveSpecialSPS(btl, 12);
                                AddSpecialSPS(btl, 16, -1, 1.0f);
                            }
                            else
                            {
                                AddSpecialSPS(btl, 0, -1, 1.0f);
                            }
                        }
                    }
                    break;
                case "Magic":
                    {
                        if (buff && SPSSpecialStatus[btl][29] == -1)
                        {
                            if (SPSSpecialStatus[btl][25] != -1)
                            {
                                RemoveSpecialSPS(btl, 25);
                                AddSpecialSPS(btl, 29, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][21] != -1)
                            {
                                RemoveSpecialSPS(btl, 21);
                                AddSpecialSPS(btl, 25, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][17] != -1)
                            {
                                RemoveSpecialSPS(btl, 17);
                                AddSpecialSPS(btl, 21, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][13] != -1)
                            {
                                RemoveSpecialSPS(btl, 13);
                                AddSpecialSPS(btl, 9, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][9] != -1)
                            {
                                RemoveSpecialSPS(btl, 9);
                                AddSpecialSPS(btl, 5, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][5] != -1)
                            {
                                RemoveSpecialSPS(btl, 5);
                                AddSpecialSPS(btl, 1, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][1] != -1)
                            {
                                RemoveSpecialSPS(btl, 1);
                            }
                            else
                            {
                                AddSpecialSPS(btl, 17, -1, 1.0f);
                            }
                        }
                        else if (!buff && SPSSpecialStatus[btl][13] == -1)
                        {
                            if (SPSSpecialStatus[btl][29] != -1)
                            {
                                RemoveSpecialSPS(btl, 29);
                                AddSpecialSPS(btl, 25, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][25] != -1)
                            {
                                RemoveSpecialSPS(btl, 25);
                                AddSpecialSPS(btl, 21, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][21] != -1)
                            {
                                RemoveSpecialSPS(btl, 21);
                                AddSpecialSPS(btl, 17, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][17] != -1)
                            {
                                RemoveSpecialSPS(btl, 17);
                            }
                            else if (SPSSpecialStatus[btl][1] != -1)
                            {
                                RemoveSpecialSPS(btl, 1);
                                AddSpecialSPS(btl, 5, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][5] != -1)
                            {
                                RemoveSpecialSPS(btl, 5);
                                AddSpecialSPS(btl, 9, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][9] != -1)
                            {
                                RemoveSpecialSPS(btl, 9);
                                AddSpecialSPS(btl, 13, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][1] != -1)
                            {
                                RemoveSpecialSPS(btl, 13);
                                AddSpecialSPS(btl, 17, -1, 1.0f);
                            }
                            else
                            {
                                AddSpecialSPS(btl, 1, -1, 1.0f);
                            }
                        }
                    }
                    break;
                case "Defence":
                    {
                        if (buff && SPSSpecialStatus[btl][30] == -1)
                        {
                            if (SPSSpecialStatus[btl][26] != -1)
                            {
                                RemoveSpecialSPS(btl, 26);
                                AddSpecialSPS(btl, 30, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][22] != -1)
                            {
                                RemoveSpecialSPS(btl, 22);
                                AddSpecialSPS(btl, 26, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][18] != -1)
                            {
                                RemoveSpecialSPS(btl, 18);
                                AddSpecialSPS(btl, 22, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][14] != -1)
                            {
                                RemoveSpecialSPS(btl, 14);
                                AddSpecialSPS(btl, 10, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][10] != -1)
                            {
                                RemoveSpecialSPS(btl, 10);
                                AddSpecialSPS(btl, 6, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][6] != -1)
                            {
                                RemoveSpecialSPS(btl, 6);
                                AddSpecialSPS(btl, 0, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][2] != -1)
                            {
                                RemoveSpecialSPS(btl, 2);
                            }
                            else
                            {
                                AddSpecialSPS(btl, 18, -1, 1.0f);
                            }
                        }
                        else if (!buff && SPSSpecialStatus[btl][14] == -1)
                        {
                            if (SPSSpecialStatus[btl][30] != -1)
                            {
                                RemoveSpecialSPS(btl, 30);
                                AddSpecialSPS(btl, 26, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][26] != -1)
                            {
                                RemoveSpecialSPS(btl, 26);
                                AddSpecialSPS(btl, 22, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][22] != -1)
                            {
                                RemoveSpecialSPS(btl, 22);
                                AddSpecialSPS(btl, 18, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][18] != -1)
                            {
                                RemoveSpecialSPS(btl, 18);
                            }
                            else if (SPSSpecialStatus[btl][2] != -1)
                            {
                                RemoveSpecialSPS(btl, 2);
                                AddSpecialSPS(btl, 6, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][6] != -1)
                            {
                                RemoveSpecialSPS(btl, 6);
                                AddSpecialSPS(btl, 10, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][10] != -1)
                            {
                                RemoveSpecialSPS(btl, 10);
                                AddSpecialSPS(btl, 14, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][14] != -1)
                            {
                                RemoveSpecialSPS(btl, 14);
                                AddSpecialSPS(btl, 18, -1, 1.0f);
                            }
                            else
                            {
                                AddSpecialSPS(btl, 2, -1, 1.0f);
                            }
                        }
                    }
                    break;
                case "MagicDefence":
                    {
                        if (buff && SPSSpecialStatus[btl][31] == -1)
                        {
                            if (SPSSpecialStatus[btl][27] != -1)
                            {
                                RemoveSpecialSPS(btl, 27);
                                AddSpecialSPS(btl, 31, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][23] != -1)
                            {
                                RemoveSpecialSPS(btl, 23);
                                AddSpecialSPS(btl, 27, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][19] != -1)
                            {
                                RemoveSpecialSPS(btl, 19);
                                AddSpecialSPS(btl, 23, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][15] != -1)
                            {
                                RemoveSpecialSPS(btl, 15);
                                AddSpecialSPS(btl, 11, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][11] != -1)
                            {
                                RemoveSpecialSPS(btl, 11);
                                AddSpecialSPS(btl, 7, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][7] != -1)
                            {
                                RemoveSpecialSPS(btl, 7);
                                AddSpecialSPS(btl, 3, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][3] != -1)
                            {
                                RemoveSpecialSPS(btl, 3);
                            }
                            else
                            {
                                AddSpecialSPS(btl, 19, -1, 1.0f);
                            }
                        }
                        else if (!buff && SPSSpecialStatus[btl][15] == -1)
                        {
                            if (SPSSpecialStatus[btl][31] != -1)
                            {
                                RemoveSpecialSPS(btl, 31);
                                AddSpecialSPS(btl, 27, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][27] != -1)
                            {
                                RemoveSpecialSPS(btl, 27);
                                AddSpecialSPS(btl, 23, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][23] != -1)
                            {
                                RemoveSpecialSPS(btl, 23);
                                AddSpecialSPS(btl, 19, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][19] != -1)
                            {
                                RemoveSpecialSPS(btl, 19);
                            }
                            else if (SPSSpecialStatus[btl][3] != -1)
                            {
                                RemoveSpecialSPS(btl, 3);
                                AddSpecialSPS(btl, 7, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][7] != -1)
                            {
                                RemoveSpecialSPS(btl, 7);
                                AddSpecialSPS(btl, 11, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][11] != -1)
                            {
                                RemoveSpecialSPS(btl, 11);
                                AddSpecialSPS(btl, 15, -1, 1.0f);
                            }
                            else if (SPSSpecialStatus[btl][15] != -1)
                            {
                                RemoveSpecialSPS(btl, 15);
                                AddSpecialSPS(btl, 19, -1, 1.0f);
                            }
                            else
                            {
                                AddSpecialSPS(btl, 3, -1, 1.0f);
                            }
                        }
                    }
                    break;
            }

        }

        public static void SpecialSA(this BattleCalculator v)
        {
            if (MonsterMechanic[v.Target.Data][5] > 0)
                MonsterMechanic[v.Target.Data][5] = 0;
            if (v.Target.HpDamage > 0 && v.Target.Data.dms_geo_id == 446 && MonsterMechanic[v.Target.Data][1] > 0 && v.Target.Data != v.Caster.Data && (v.Target.Flags & CalcFlag.HpRecovery) == 0) // Armor Mechanical
            {
                if (MonsterMechanic[v.Target.Data][1] < 100)
                v.Target.HpDamage = ((100 - MonsterMechanic[v.Target.Data][1]) * v.Target.HpDamage) / 100;
                else
                {
                    v.Target.Flags = 0;
                    v.Context.Flags |= BattleCalcFlags.Guard;
                }
                MonsterMechanic[v.Target.Data][1] -= 10;
                if (MonsterMechanic[v.Target.Data][1] < 40 && MonsterMechanic[v.Target.Data][2] == 0)
                    v.Target.Data.mot[2] = "ANH_MON_B3_185_003";
                if (MonsterMechanic[v.Target.Data][1] < 0)
                    MonsterMechanic[v.Target.Data][1] = 0;

                SPSCumulative(v.Target.Data, SPSStackable.MechanicalArmor);
            }
            if (v.Context.IsAbsorb)
            {
                v.Target.Flags |= CalcFlag.HpDamageOrHeal;

            }
            if (v.Caster.PlayerIndex == CharacterId.Beatrix || v.Target.PlayerIndex == CharacterId.Beatrix && v.Command.Data.info.cover == 1) // Redemption mechanic
            {
                if (BeatrixPassive[v.Caster.Data][3] == 0)
                {
                    BeatrixPassive[v.Caster.Data][3] = (ushort)(v.Command.TargetCount);
                    if ((v.Command.AbilityId == (BattleAbilityId)1012 || v.Command.AbilityId == (BattleAbilityId)1054)) // Bravoure & Héroïsme
                    {
                        v.Target.SummonCount = 2;
                        RemoveSpecialSPS(v.Caster.Data, 64);
                        AddSpecialSPS(v.Caster.Data, 16, -1, 1.0f);
                        AddSpecialSPS(v.Caster.Data, 17, -1, 1.0f);
                        AddSpecialSPS(v.Caster.Data, 65, -1, 1.0f);
                    }
                    else if (v.Command.Id == BattleCommandId.Attack || v.Command.Id == BattleCommandId.Defend || v.Command.Id == BattleCommandId.Counter ||
                        v.Command.Id == BattleCommandId.HolyWhiteMagic || v.Caster.IsUnderAnyStatus(BattleStatus.Trance))
                    {
                        SPSCumulative(v.Caster.Data, SPSStackable.Redemption);
                    }
                    else if (v.Command.Data.info.cover == 1 && v.Target.HasSupportAbility(SupportAbility2.Cover))
                    {
                        SPSCumulative(v.Target.Data, SPSStackable.Redemption);
                    }
                    else if (v.Command.Id == BattleCommandId.HolySword1)
                    {
                        v.Caster.SummonCount = 0;
                        RemoveSpecialSPS(v.Caster.Data, 64);
                        RemoveSpecialSPS(v.Caster.Data, 65);
                    }
                }
                BeatrixPassive[v.Caster.Data][3]--;
                if (BeatrixPassive[v.Caster.Data][3] < 0)
                    BeatrixPassive[v.Caster.Data][3] = 0;
            }
            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)110) && !v.Command.IsManyTarget && v.Command.Id != BattleCommandId.Attack && v.Target.HpDamage > 0 && 
                (v.Command.ScriptId == 9 || v.Command.ScriptId == 10 || v.Command.ScriptId == 17 || v.Command.ScriptId == 18 
                || v.Command.AbilityId == BattleAbilityId.PumpkinHead || v.Command.AbilityId == BattleAbilityId.ThousandNeedles || v.Command.AbilityId == BattleAbilityId.GoblinPunch
                || v.Command.AbilityId == BattleAbilityId.AutoLife)) // Prolifération
            {
                int basedamage = v.Target.HpDamage;
                BTL_DATA targetdefault = v.Target.Data;
                foreach (BattleUnit unit in BattleState.EnumerateUnits())
                {
                    if ((unit.IsPlayer && !v.Target.IsPlayer || !unit.IsPlayer && v.Target.IsPlayer) || unit.MagicDefence == 255 || unit.PhysicalEvade == 255)
                        continue;

                    if (unit.Data != targetdefault) 
                    {
                        if (v.Caster.HasSupportAbilityByIndex((SupportAbility)1110))
                        {
                            v.Target.HpDamage = basedamage / 2;
                        }
                        else
                        {
                            v.Target.HpDamage = basedamage / 4;
                        }
                    }
                    else
                    {
                        v.Target.HpDamage = basedamage;
                    }
                    v.Target.Change(unit);
                    SBattleCalculator.CalcResult(v);
                    BattleState.Unit2DReq(unit);
                }
                v.Target.Flags = 0;
                v.Target.HpDamage = 0;
                v.PerformCalcResult = false;
            }
            if (!v.Target.IsPlayer && v.Target.IsUnderAnyStatus(BattleStatus.EasyKill))
            {
                if (((StatusBeforeScript[v.Target.Data] & (BattleStatus.Freeze | BattleStatus.Stop | BattleStatus.Sleep)) == 0) && v.Target.IsUnderAnyStatus(BattleStatus.Freeze | BattleStatus.Stop | BattleStatus.Sleep) && MonsterMechanic[v.Target.Data][4] > 20)
                {
                    if (MonsterMechanic[v.Target.Data][4] > 0)
                        MonsterMechanic[v.Target.Data][4] -= 20;
                    if ((StatusBeforeScript[v.Target.Data] & (BattleStatus.Freeze)) == 0)
                    {
                        v.Target.Data.stat.cnt.conti[25] = (short)((v.Target.Data.stat.cnt.conti[25] * MonsterMechanic[v.Target.Data][4]) / 100);
                    }
                    if ((StatusBeforeScript[v.Target.Data] & (BattleStatus.Stop)) == 0)
                    {
                        v.Target.Data.stat.cnt.conti[12] = (short)((v.Target.Data.stat.cnt.conti[12] * MonsterMechanic[v.Target.Data][4]) / 100);
                    }
                    if ((StatusBeforeScript[v.Target.Data] & (BattleStatus.Sleep)) == 0)
                    {
                        v.Target.Data.stat.cnt.conti[17] = (short)((v.Target.Data.stat.cnt.conti[17] * MonsterMechanic[v.Target.Data][4]) / 100);
                    }
                }
                if (v.Target.IsUnderAnyStatus(BattleStatus.Venom) && v.Target.Data.stat.cnt.conti[1] <= 0)
                {
                    v.Target.Data.stat.cnt.conti[1] = (short)((400 + (v.Caster.Will * 2) - v.Target.Will) * FF9StateSystem.Battle.FF9Battle.status_data[16].conti_cnt);
                    v.Target.AddDelayedModifier(
                    target => target.Data.stat.cnt.conti[1] > 0,
                    target =>
                    {
                        v.Target.RemoveStatus(BattleStatus.Venom);
                        v.Target.Data.stat.cnt.conti[1] = 0;
                    }
                    );
                }
                if (v.Target.IsUnderAnyStatus(BattleStatus.Silence) && v.Target.Data.stat.cnt.conti[3] <= 0)
                {
                    v.Target.Data.stat.cnt.conti[3] = (short)((400 + (v.Caster.Will * 2) - v.Target.Will) * FF9StateSystem.Battle.FF9Battle.status_data[16].conti_cnt);
                    v.Target.AddDelayedModifier(
                    target => target.Data.stat.cnt.conti[3] > 0,
                    target =>
                    {
                        v.Target.RemoveStatus(BattleStatus.Silence);
                        v.Target.Data.stat.cnt.conti[3] = 0;
                    }
                    );
                }
                if (v.Target.IsUnderAnyStatus(BattleStatus.Blind) && v.Target.Data.stat.cnt.conti[4] <= 0)
                {
                    v.Target.Data.stat.cnt.conti[4] = (short)((400 + (v.Caster.Will * 2) - v.Target.Will) * FF9StateSystem.Battle.FF9Battle.status_data[16].conti_cnt);
                    v.Target.AddDelayedModifier(
                    target => target.Data.stat.cnt.conti[4] > 0,
                    target =>
                    {
                        v.Target.RemoveStatus(BattleStatus.Blind);
                        v.Target.Data.stat.cnt.conti[4] = 0;
                    }
                    );
                }
                if (v.Target.IsUnderAnyStatus(BattleStatus.Trouble) && v.Target.Data.stat.cnt.conti[5] <= 0)
                {
                    v.Target.Data.stat.cnt.conti[5] = (short)((400 + (v.Caster.Will * 2) - v.Target.Will) * FF9StateSystem.Battle.FF9Battle.status_data[16].conti_cnt);
                    v.Target.AddDelayedModifier(
                    target => target.Data.stat.cnt.conti[5] > 0,
                    target =>
                    {
                        v.Target.RemoveStatus(BattleStatus.Trouble);
                        v.Target.Data.stat.cnt.conti[5] = 0;
                    }
                    );
                }
                if (v.Target.IsUnderAnyStatus(BattleStatus.Zombie) && v.Target.Data.stat.cnt.conti[6] <= 0)
                {
                    v.Target.Data.stat.cnt.conti[6] = (short)((400 + (v.Caster.Will * 2) - v.Target.Will) * FF9StateSystem.Battle.FF9Battle.status_data[16].conti_cnt);
                    v.Target.AddDelayedModifier(
                    target => target.Data.stat.cnt.conti[6] > 0,
                    target =>
                    {
                        v.Target.RemoveStatus(BattleStatus.Zombie);
                        v.Target.Data.stat.cnt.conti[6] = 0;
                    }
                    );
                }
                if (v.Target.IsUnderAnyStatus(BattleStatus.Poison) && v.Target.Data.stat.cnt.conti[16] <= 0)
                {
                    v.Target.Data.stat.cnt.conti[16] = (short)((400 + (v.Caster.Will * 2) - v.Target.Will) * FF9StateSystem.Battle.FF9Battle.status_data[16].conti_cnt);
                    v.Target.AddDelayedModifier(
                    target => target.Data.stat.cnt.conti[16] > 0,
                    target =>
                    {
                        v.Target.RemoveStatus(BattleStatus.Poison);
                        v.Target.Data.stat.cnt.conti[16] = 0;
                    }
                    );
                }
            }
            if (!v.Caster.IsPlayer && v.Caster.IsUnderAnyStatus(BattleStatus.EasyKill)) 
            {
                if (v.Caster.IsUnderAnyStatus(BattleStatus.Heat)) // Heat Damage
                {
                    Int32 heat_damage = (int)(v.Caster.MaximumHp / 64);
                    if (MonsterMechanic[v.Caster.Data][3] > 0)
                        heat_damage = (int)(v.Caster.MaximumHp - 10000) / 64;
                    if (heat_damage > Math.Max(v.Caster.CurrentHp - 1, 9999))
                        heat_damage = (Int32)v.Caster.CurrentHp - 1;
                    if (heat_damage > 0)
                    {
                        v.Caster.AddDelayedModifier(
                        caster => caster.CurrentAtb >= caster.MaximumAtb,
                        caster =>
                        {
                            if ((EffectElement.Fire & v.Caster.AbsorbElement) != 0)
                                v.Caster.Data.fig_info = Param.FIG_INFO_HP_RECOVER;
                            else
                                v.Caster.Data.fig_info = Param.FIG_INFO_DISP_HP;
                            btl_para.SetDamage(new BattleUnit(v.Caster.Data), heat_damage, (Byte)(btl_mot.checkMotion(v.Caster.Data, v.Caster.Data.bi.def_idle) ? 1 : 0));
                            btl2d.Btl2dReq(v.Caster.Data);
                        }
                    );
                    }
                }
            }
            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)119) && v.Command.Id == BattleCommandId.MagicSword)
            {
                foreach (BattleUnit unit in FF9StateSystem.Battle.FF9Battle.EnumerateBattleUnits())
                {
                    if (btl_util.getSerialNumber(unit.Data) == CharacterSerialNumber.VIVI && (unit.CurrentMp >= v.Command.Data.aa.MP / (v.Caster.HasSupportAbilityByIndex((SupportAbility)1119) ? 2 : 4)))
                    {
                        if (unit.CurrentMp > v.Command.Data.aa.MP)
                        {
                            unit.CurrentMp = (uint)(unit.CurrentMp - (v.Command.Data.aa.MP / (v.Caster.HasSupportAbilityByIndex((SupportAbility)1119) ? 2 : 4)));
                        }
                        else
                        {
                            unit.CurrentMp = 0;
                        }                      
                    }
                }
                v.Caster.CurrentMp = (uint)(v.Caster.CurrentMp + (v.Command.Data.aa.MP / (v.Caster.HasSupportAbilityByIndex((SupportAbility)1119) ? 2 : 4)));
            }

            if (!v.Target.IsUnderAnyStatus(BattleStatus.Virus) && SPSSpecialStatus[v.Target.Data][32] >= 0)
            {
                RemoveSpecialSPS(v.Target.Data, 32);
            }

            if (v.Caster.HasSupportAbility(SupportAbility1.ReflectNull) && v.Target.IsUnderAnyStatus(BattleStatus.Reflect) && !v.Caster.HasSupportAbilityByIndex((SupportAbility)1030))
                v.Target.HpDamage >>= 1;

            int HealHPSAOrItem = 0;
            int HealMPSAOrItem = 0;
            if (v.Command.AbilityId == BattleAbilityId.DemiShock2) // Tobigeri+
            {
                HealHPSAOrItem = v.Target.HpDamage / 2;
            }
            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)1061) && v.Command.Id == BattleCommandId.Attack) // Mug+
            {
                HealHPSAOrItem += v.Target.HpDamage / 4;
            }
            if (v.Caster.Accessory == (RegularItem)1208 && (v.Target.Flags & CalcFlag.HpRecovery) == 0 && v.Target.Data != v.Caster.Data) // Materia Support
            {
                HealHPSAOrItem += v.Target.HpDamage / 10;
            }
            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)115) && (v.Target.WeakElement & v.Command.Element) != 0) // Soul Drain
            {
                HealHPSAOrItem += v.Target.HpDamage / (v.Caster.HasSupportAbilityByIndex((SupportAbility)1115) ? 2 : 4);
            }
            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)117)) // Mode EX
            {
                HealHPSAOrItem += (int)(v.Caster.MaximumHp * (v.Caster.HasSupportAbilityByIndex((SupportAbility)1117) ? 16 : 8) / 100);
                HealMPSAOrItem += (int)(v.Caster.MaximumMp * (v.Caster.HasSupportAbilityByIndex((SupportAbility)1117) ? 16 : 8) / 100);
            }
            if (HealHPSAOrItem > 0 || HealMPSAOrItem > 0)
            {
                if (HealHPSAOrItem > 0)
                    v.Caster.Flags |= CalcFlag.HpDamageOrHeal;
                if (HealMPSAOrItem > 0)
                    v.Caster.Flags |= CalcFlag.MpDamageOrHeal;
                v.Caster.HpDamage = HealHPSAOrItem;
                if (HealMPSAOrItem > 0)
                    v.Caster.MpDamage = HealMPSAOrItem;
            }
            if (v.Target.IsPlayer && SpecialSAEffect[v.Target][3] == 0) // Reset stats and others things when player die.
            {
                SpecialSAEffect[v.Target][3] = 1;
                v.Target.AddDelayedModifier(
                    target => !target.IsUnderAnyStatus(BattleStatus.Death),
                    target =>
                    {
                        v.Target.Data.elem.str = v.Target.Player.Data.elem.str;
                        v.Target.Data.elem.wpr = v.Target.Player.Data.elem.wpr;
                        v.Target.Data.elem.mgc = v.Target.Player.Data.elem.mgc;
                        v.Target.Data.defence.PhysicalDefence = v.Target.Player.Data.defence.PhysicalDefence;
                        v.Target.Data.defence.PhysicalEvade = v.Target.Player.Data.defence.PhysicalEvade;
                        v.Target.Data.defence.MagicalDefence = v.Target.Player.Data.defence.MagicalDefence;
                        v.Target.Data.defence.MagicalEvade = v.Target.Player.Data.defence.MagicalEvade;
                        if (v.Target.PlayerIndex == CharacterId.Beatrix) 
                        {
                            if (!v.Target.InTrance)
                                v.Target.SummonCount = 0;

                            BeatrixPassive[v.Target.Data][2] = 0; // Retire Bravoure/Héroïsme de Beatrix.
                        }
                        for (Int32 i = 0; i < (SPSSpecialStatus[v.Target.Data].Length - 1); i++)
                        {
                            if (v.Target.PlayerIndex == CharacterId.Beatrix && (i == 64 || i == 65) && v.Target.InTrance)
                            {
                                continue;
                            }
                            if (SPSSpecialStatus[v.Target.Data][i] >= 0)
                            {
                                RemoveSpecialSPS(v.Target.Data, (uint)i);
                            }
                        }
                        SpecialSAEffect[v.Target][3] = 0;
                    }
                );
            }
            if ((v.Command.AbilityStatus & BattleStatus.Petrify) != 0 && !v.Caster.IsPlayer) // Petrify fix after Death/Trance
            {
                v.Target.AddDelayedModifier(
                    caster => caster.CurrentAtb >= 1,
                    target =>
                    {
                        if ((target.CurrentHp == 0 || target.IsUnderAnyStatus(BattleStatus.Death)))
                        {
                            target.Trance = 254;
                            target.RemoveStatus(BattleStatus.Petrify);
                        }
                        else if (target.Trance == 255 && !target.IsUnderStatus(BattleStatus.Death))
                        {
                            target.Trance = 254;
                            target.RemoveStatus(BattleStatus.Trance);
                        }
                    }
                );
            }

            if (v.Target.HasSupportAbilityByIndex((SupportAbility)52) && SpecialSAEffect[v.Target.Data][1] > 0 && v.Target.HpDamage > v.Target.CurrentHp && v.Target.CurrentMp > 0) // Last Stand
            {
                SpecialSAEffect[v.Target.Data][1]--;
                v.Target.HpDamage = (int)v.Target.CurrentHp - 1;
                v.Target.CurrentMp = v.Target.HasSupportAbilityByIndex((SupportAbility)1052) ? (v.Target.CurrentMp / 2) : 0;
                Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                            {
                                { "US", "Last Stand!" },
                                { "UK", "Last Stand!" },
                                { "JP", "Last Stand!" },
                                { "ES", "Last Stand!" },
                                { "FR", "Echapée belle !" },
                                { "GR", "Last Stand!" },
                                { "IT", "Last Stand!" },
                            };
                btl2d.Btl2dReqSymbolMessage(v.Target.Data, "[FDEE00]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 10);
            }

            if (v.Caster.PlayerIndex == CharacterId.Zidane && v.Command.Id == BattleCommandId.Attack && ff9item._FF9Item_Data[FF9StateSystem.Common.FF9.player[(CharacterId)v.Caster.Data.bi.slot_no].equip[0]].shape == 1) // Zidane - Dagger double hits
            {
                v.Target.HpDamage /= 2;
                if (ZidanePassive[v.Caster.Data][4] == 2)
                {
                    ZidanePassive[v.Caster.Data][4] = 0;
                    if (v.Target.CurrentHp > v.Target.HpDamage)
                        v.Command.AbilityCategory += 64;
                }
            }

            if (v.Caster.Weapon == (RegularItem)1100)
            {
                v.Target.HpDamage = 1;
            }
        }

        public enum SPSStackable
        {
            SuperDodge = 1,
            ZombieArmor,
            MechanicalArmor,
            Redemption
        }

        public static Int32[,] BossBattleBonusHP = new Int32[,]
        {
            { 73, 0 }, // Beatrix 3rd
		    { 299, 0 }, // Beatrix 2nd
		    { 4, 0 }, // Beatrix 1st
            { 4, 1 }, // Dark Beatrix
            { 295, 0 }, // Bach
		    { 115, 1 },	// Kuja 1st (Ifa)
		    { 920, 0 }, { 921, 0 }, // Friendly Belhamel
            { 303, 0 }, // Bamblourine
            { 938, 0 }, // Necron
            { 251, 0 }, { 363, 0 }, { 364, 0 }, { 838, 0 }, // Friendly Eskuriax
            { 192, 0 }, { 193, 0 }, { 196, 0 }, { 197, 0 }, { 199, 0 }, // Friendly Fantôme
            { 300, 0 }, // Fourmillion
            { 2, 2 }, // Gardienne du feu
            { 107, 0 }, // Gargantua
            { 890, 0 }, // Garland
            { 723, 0 }, // Friendly Garuda
            { 57, 0 }, // Ozma
            { 211, 0 }, // Ozma
            { 365, 0 }, { 367, 0 }, { 368, 0 }, { 595, 0 }, { 605, 0 }, { 606, 0 }, // Friendly Jabah
            { 891, 0 }, // Kuja 2nd
            { 891, 1 }, // Trance Kuja 1st
            { 937, 0 }, // Trance Kuja 2nd (Crystal World)
            { 330, 0 }, // Kwell
            { 75, 0 }, // Larvalar
            { 76, 0 }, // Larvalar Junior
            { 132, 0 }, // Amarant Enemy
            { 631, 0 }, { 632, 0 },  // Friendly Manta
            { 336, 0 }, // Maskedefer
            { 302, 0 }, // Maton + Grenat
            { 301, 0 }, // Maton + Vivi
            { 682, 0 }, { 686, 0 }, { 687, 0 }, { 689, 0 }, { 270, 0 }, { 235, 0 }, { 841, 0 }, { 239, 0 }, // Friendly Miskoxy
            { 112, 2 }, // Lamie 2nd
            { 636, 0 }, { 637, 0 }, { 641, 0 }, { 268, 0 }, { 647, 0 }, { 188, 0 }, { 189, 0 }, // Friendly Nymphe
            { 525, 0 }, // Obélisk
            { 191, 0 }, // Pluton
            { 338, 0 }, // Roi Lear
            { 931, 0 }, // Shinryu
            { 889, 0 }, // Silver Dragon
            { 337, 0 }, // Steiner 1st
            { 335, 0 }, // Steiner 2nd
            { 337, 0 }, { 337, 1 }, // Steiner 3rd + Bombo
            { 936, 0 }, // Sulfura
            { 294, 0 }, // Valseur 2
            { 296, 0 }, // Valseur 3
            { 668, 0 },  { 217, 0 }, { 670, 0 }, { 751, 0 }, { 652, 0 }, { 664, 0 }, { 216, 0 } // Friendly Yeti
        };
    }
}
