﻿using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Data;
using UnityEngine;
using static TitleUI;

namespace Memoria.Scripts.Battle
{
    public static class TranceSeekCustomAPI
    {
        public static Dictionary<BTL_DATA, Boolean> InitBTL = new Dictionary<BTL_DATA, Boolean>();

        public static Dictionary<BTL_DATA, Int32[]> ZidanePassive = new Dictionary<BTL_DATA, Int32[]>();
        // [0] => Dodge ; [1] => Critical ; [2] => Eye of the thief ; [3] => Master Thief ; [4] => Dagger Attack ; [5] => FirstItemMug ; [6] => SecondItemMug ; [7] => Mug+ ; [8] => Steal Gil ; [9] => Flexible

        public static Dictionary<BTL_DATA, Int32[]> ViviPassive = new Dictionary<BTL_DATA, Int32[]>(); // [0] => Focus ; [1] => NumberTargets ; [2] => TriggerOneTime
        public static Dictionary<BTL_DATA, BattleAbilityId> ViviPreviousSpell = new Dictionary<BTL_DATA, BattleAbilityId>();

        public static Dictionary<BTL_DATA, Int32[]> BeatrixPassive = new Dictionary<BTL_DATA, Int32[]>(); // [0] => Strength ; [1] => Magic ; [2] => Bravoure ; [3] => TargetCount

        public static Dictionary<BTL_DATA, Dictionary<BattleStatus, Int32>> ProtectStatus = new Dictionary<BTL_DATA, Dictionary<BattleStatus, Int32>>();
        public static Dictionary<BTL_DATA, Int32> AbsorbElement = new Dictionary<BTL_DATA, Int32>();

        public static Dictionary<BTL_DATA, Int32> StateMoug = new Dictionary<BTL_DATA, Int32>();
        public static Dictionary<BTL_DATA, GameObject> ModelMoug = new Dictionary<BTL_DATA, GameObject>();

        public static Dictionary<BTL_DATA, Int32[]> StackBreakOrUpStatus = new Dictionary<BTL_DATA, Int32[]>();  // [0] => StackStrength ; [1] => StackMagic ; [2] => StackArmor ; [3] => StackMental

        public static Dictionary<BTL_DATA, Int32[]> MonsterMechanic = new Dictionary<BTL_DATA, Int32[]>(); // [0] => Trance Activated ; [1] => Special1 ; [2] => Special2 ; [3] => HPBoss10000? ; [4] => ResistStatusEasyKill ; [5] => NerfGravity

        public static Dictionary<BTL_DATA, Int32[]> SpecialSAEffect = new Dictionary<BTL_DATA, Int32[]>();
        // [0] => Sentinel/Duel ; [1] => LastStand ; [2] => Instinct ; [3] => PreventTranceSFX ; [4] => Mode EX ; [5] => HealHP ; [6] => HealMP ; [7] => TargetCount ; [8] => SpringBoots ; [9] => CriticalHit100 ;
        // [10] => SteinerEnchantedBlade ; [11] => Peuh! ; [12] => That's all ; [13] => In top form!
        public static Dictionary<BTL_DATA, Int32[]> RollBackStats = new Dictionary<BTL_DATA, Int32[]>();
        public static Dictionary<BTL_DATA, BattleStatus> RollBackBattleStatus = new Dictionary<BTL_DATA, BattleStatus>();
        public static Dictionary<BTL_DATA, EffectElement> WeaponNewElement = new Dictionary<BTL_DATA, EffectElement>();
        public static Dictionary<BTL_DATA, BattleStatus> WeaponNewStatus = new Dictionary<BTL_DATA, BattleStatus>();

        public static class CustomStatus
        {
            public const BattleStatus PowerBreak = BattleStatus.CustomStatus1;
            public const BattleStatus MagicBreak = BattleStatus.CustomStatus2;
            public const BattleStatus ArmorBreak = BattleStatus.CustomStatus3;
            public const BattleStatus MentalBreak = BattleStatus.CustomStatus4;
            public const BattleStatus PowerUp = BattleStatus.CustomStatus5;
            public const BattleStatus MagicUp = BattleStatus.CustomStatus6;
            public const BattleStatus ArmorUp = BattleStatus.CustomStatus7;
            public const BattleStatus MentalUp = BattleStatus.CustomStatus8;
            public const BattleStatus Dragon = BattleStatus.CustomStatus9;
            public const BattleStatus ZombieArmor = BattleStatus.CustomStatus10;
            public const BattleStatus MechanicalArmor = BattleStatus.CustomStatus11;
            public const BattleStatus Redemption = BattleStatus.CustomStatus12;
            public const BattleStatus Bulwark = BattleStatus.CustomStatus13;
            public const BattleStatus PerfectDodge = BattleStatus.CustomStatus14;
            public const BattleStatus PerfectCrit = BattleStatus.CustomStatus15;
            public const BattleStatus Vieillissement = BattleStatus.CustomStatus16;
            public const BattleStatus SleepEasyKill = BattleStatus.CustomStatus17;
            public const BattleStatus SilenceEasyKill = BattleStatus.CustomStatus18;
            public const BattleStatus Rage = BattleStatus.CustomStatus19;
            public const BattleStatus Runic = BattleStatus.CustomStatus20;
            public const BattleStatus Special = BattleStatus.CustomStatus21;
            public const BattleStatus Provok = BattleStatus.CustomStatus22;
        }

        public static class CustomStatusId
        {
            public const BattleStatusId PowerBreak = BattleStatusId.CustomStatus1;
            public const BattleStatusId MagicBreak = BattleStatusId.CustomStatus2;
            public const BattleStatusId ArmorBreak = BattleStatusId.CustomStatus3;
            public const BattleStatusId MentalBreak = BattleStatusId.CustomStatus4;
            public const BattleStatusId PowerUp = BattleStatusId.CustomStatus5;
            public const BattleStatusId MagicUp = BattleStatusId.CustomStatus6;
            public const BattleStatusId ArmorUp = BattleStatusId.CustomStatus7;
            public const BattleStatusId MentalUp = BattleStatusId.CustomStatus8;
            public const BattleStatusId Dragon = BattleStatusId.CustomStatus9;
            public const BattleStatusId ZombieArmor = BattleStatusId.CustomStatus10;
            public const BattleStatusId MechanicalArmor = BattleStatusId.CustomStatus11;
            public const BattleStatusId Redemption = BattleStatusId.CustomStatus12;
            public const BattleStatusId Bulwark = BattleStatusId.CustomStatus13;
            public const BattleStatusId PerfectDodge = BattleStatusId.CustomStatus14;
            public const BattleStatusId PerfectCrit = BattleStatusId.CustomStatus15;
            public const BattleStatusId Vieillissement = BattleStatusId.CustomStatus16;
            public const BattleStatusId SleepEasyKill = BattleStatusId.CustomStatus17;
            public const BattleStatusId SilenceEasyKill = BattleStatusId.CustomStatus18;
            public const BattleStatusId Rage = BattleStatusId.CustomStatus19;
            public const BattleStatusId Runic = BattleStatusId.CustomStatus20;
            public const BattleStatusId Special = BattleStatusId.CustomStatus21;
            public const BattleStatusId Provok = BattleStatusId.CustomStatus22;
        }

        public static void WeaponPhysicalParams(CalcAttackBonus bonus, BattleCalculator v)
        {
            Int32 baseDamage = Comn.random16() % (1 + (v.Caster.Level + v.Caster.Strength >> 3));
            v.Context.AttackPower = v.Caster.GetWeaponPower(v.Command);
            v.Target.SetPhysicalDefense();
            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)222) & bonus == CalcAttackBonus.Random) // SA Sharpening
            {
                if (v.Caster.HasSupportAbilityByIndex((SupportAbility)1222)) // SA Sharpening +
                    v.Context.Attack = v.Caster.Strength + BeatrixPassive[v.Caster.Data][0] + baseDamage;
                else
                    v.Context.Attack = UnityEngine.Random.Range(v.Caster.Strength / 2, v.Caster.Strength) + BeatrixPassive[v.Caster.Data][0] + baseDamage;
            }
            else
            {
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
        }

        public static void TryApplyDragon(this BattleCalculator v)
        {
            if (v.Caster.PlayerIndex == CharacterId.Freya && !v.Target.IsUnderAnyStatus(CustomStatus.Dragon))
            {
                Int32 quarterWill = v.Caster.Data.elem.wpr >> 2;
                Int32 bonus = 0;
                switch (v.Caster.Weapon)
                {
                    case RegularItem.MythrilSpear:
                    case RegularItem.Partisan:
                        bonus += 5;
                        break;
                    case RegularItem.IceLance:
                    case RegularItem.Trident:
                        bonus += 8;
                        break;
                    case RegularItem.HeavyLance:
                    case RegularItem.Obelisk:
                        bonus += 10;
                        break;
                    case RegularItem.HolyLance:
                        bonus += 15;
                        break;
                    case RegularItem.KainLance:
                        bonus += 20;
                        break;
                    case RegularItem.DragonHair:
                        bonus += 25;
                        break;
                }
                if (v.Command.AbilityId == BattleAbilityId.CherryBlossom)
                    bonus += 25;

                if (quarterWill != 0 && (((Comn.random16() % quarterWill) + bonus) > Comn.random16() % 100))
                {
                    v.Target.AlterStatus(CustomStatus.Dragon, v.Caster);
                }
            }
        }

        public static void TryCriticalHit(this BattleCalculator v)
        {
            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)1102)) // Archimage+ (10% crit en bonus)
                ZidanePassive[v.Caster.Data][1] = 40;
            Int32 quarterWill = (v.Caster.Data.elem.wpr + ZidanePassive[v.Caster.Data][1]) >> 2;
            BonusCriticalFromWeapon(v.Caster.Weapon, out Int32 BonusWeaponCritical);
            if (quarterWill != 0 && ((Comn.random16() % quarterWill) + v.Caster.Data.critical_rate_deal_bonus + v.Target.Data.critical_rate_receive_resistance + BonusWeaponCritical > Comn.random16() % 100) || v.Target.IsUnderAnyStatus(CustomStatus.PerfectCrit) || SpecialSAEffect[v.Target.Data][9] > 0)
            {
                if (SpecialSAEffect[v.Target.Data][9] > 0)
                    SpecialSAEffect[v.Target.Data][9]--;
                if (v.Target.IsUnderAnyStatus(CustomStatus.PerfectCrit)) // Perfect Crit
                    btl_stat.AlterStatus(v.Target, CustomStatusId.PerfectCrit, parameters: "-1");
                else
                    ZidanePassive[v.Caster.Data][1] = 0;
                v.Context.Attack *= 2;
                v.Target.HpDamage *= 2;
                v.Target.MpDamage *= 2;
                v.Target.Flags |= CalcFlag.Critical;
                if (v.Caster.PlayerIndex == CharacterId.Freya)
                    v.Target.AlterStatus(CustomStatus.Dragon, v.Caster);
            }
            else if (v.Caster.PlayerIndex == CharacterId.Zidane && btl_util.getSerialNumber(v.Caster.Data) == CharacterSerialNumber.ZIDANE_SWORD)
            {
                ZidanePassive[v.Caster.Data][1] += 5;
                Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                    {
                        { "US", "↑ Critical ↑" },
                        { "UK", "↑ Critical ↑" },
                        { "JP", "↑ Critical ↑" },
                        { "ES", "↑ Letal ↑" },
                        { "FR", "↑ Critique ↑" },
                        { "GR", "↑ KRITISCH ↑" },
                        { "IT", "↑ Letale ↑" },
                    };
                btl2d.Btl2dReqSymbolMessage(v.Caster.Data, "[FFFF00]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 15);
            }
        }

        public static void BonusCriticalFromWeapon(RegularItem Weapon, out Int32 BonusWeaponCritical)
        {
            BonusWeaponCritical = 0;
            switch (Weapon)
            {
                case RegularItem.ButterflySword:
                    BonusWeaponCritical += 1;
                    break;
                case RegularItem.TheOgre:
                    BonusWeaponCritical += 2;
                    break;
                case (RegularItem)1004: // Gladius Sword
                    BonusWeaponCritical += 2;
                    break;
                case RegularItem.Exploda:
                    BonusWeaponCritical += 3;
                    break;
                case RegularItem.RuneTooth:
                    BonusWeaponCritical += 3;
                    break;
                case (RegularItem)1005: // Zorlin Sword
                    BonusWeaponCritical += 4;
                    break;
                case RegularItem.AngelBless:
                    BonusWeaponCritical += 5;
                    break;
                case RegularItem.Sargatanas:
                    BonusWeaponCritical += 6;
                    break;
                case RegularItem.Masamune:
                    BonusWeaponCritical += 7;
                    break;
                case (RegularItem)1006: // Orichalcon Sword
                    BonusWeaponCritical += 8;
                    break;
                case RegularItem.TheTower:
                    BonusWeaponCritical += 9;
                    break;
                case RegularItem.UltimaWeapon:
                    BonusWeaponCritical += 10;
                    break;
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

            if (BeatrixPassive[v.Caster.Data][2] == 2) // Héroïsme de Beatrix => [TODO] Change to Context.Evade ?
                v.Context.HitRate += 25;

            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)235) && v.Command.Id == BattleCommandId.Attack) // SA Fencing
                v.Context.Evade = Math.Min(0, v.Context.Evade - (v.Caster.HasSupportAbilityByIndex((SupportAbility)1235) ? v.Context.Evade / 4 : v.Context.Evade / 8));

            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)201) && v.Caster.PlayerIndex == CharacterId.Zidane) // SA Gorilla
                v.Context.HitRate += ZidanePassive[v.Caster.Data][1];

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

            if (v.Target.IsUnderAnyStatus(CustomStatus.PerfectDodge) && !v.Caster.HasSupportAbility(SupportAbility1.Healer)) // Perfect Dodge
            {
                v.Context.Flags |= BattleCalcFlags.Miss | BattleCalcFlags.Dodge;
                btl_stat.AlterStatus(v.Target, CustomStatusId.PerfectDodge, parameters: "Remove");
                //if (v.Target.IsUnderAnyStatus(CustomStatus.PerfectDodge)) // Didn't work when Stack > 1.... ?
                //    btl2d.Btl2dReqSymbolMessage(v.Target.Data, "[FFFFFF]", Localization.Get("Miss"), HUDMessage.MessageStyle.DAMAGE, 0);

                return false;
            }

            foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(v.Caster.Data.saExtended))
                saFeature.TriggerOnAbility(v, "HitRateSetup", false);
            foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(v.Target.Data.saExtended))
                saFeature.TriggerOnAbility(v, "HitRateSetup", true);

            if ((v.Context.HitRate <= Comn.random16() % 100) || v.Target.PhysicalEvade == 255 || v.Target.IsUnderAnyStatus(BattleStatus.Vanish))
            {
                v.Context.Flags |= BattleCalcFlags.Miss;
                return false;
            }

            if (v.Target.PhysicalDefence == 255 || (v.Target.IsUnderAnyStatus(CustomStatus.Bulwark) && !v.Caster.HasSupportAbility(SupportAbility1.Healer))) // Bulwark
            {
                v.Target.RemoveStatus(CustomStatus.Bulwark);
                v.Context.Flags |= BattleCalcFlags.Guard;
                return false;
            }
            if ((v.Target.Data == v.Caster.Data || (v.Context.Evade + (v.Target.PlayerIndex == CharacterId.Zidane ? ZidanePassive[v.Target.Data][0] : 0)) <= Comn.random16() % 100 || v.Context.Evade == 0))
            {
                if (v.Target.PlayerIndex == CharacterId.Zidane && btl_util.getSerialNumber(v.Target.Data) == CharacterSerialNumber.ZIDANE_DAGGER && !v.Target.IsUnderAnyStatus(BattleStatusConst.BattleEndFull) && !v.Caster.HasSupportAbility(SupportAbility1.Healer))
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
            if (v.Target.PhysicalDefence == 255)
            {
                v.Context.Flags |= BattleCalcFlags.Guard;
                return;
            }
            if (v.Target.PlayerIndex == CharacterId.Beatrix)
            {
                v.Context.DefensePower += BeatrixPassive[v.Caster.Data][0];
            }

            if (v.Target.IsUnderAnyStatus(BattleStatus.Defend))
                v.Context.Attack >>= 1;

            if (v.Target.PlayerIndex == CharacterId.Steiner && v.Target.IsUnderAnyStatus(BattleStatus.Trance)) // Steiner Trance => 25% reduce physical damage
            {
                v.Context.Attack = (3 * v.Context.Attack) / 4;
            }

            if (v.Target.IsUnderAnyStatus(BattleStatus.Protect))
                v.Context.Attack >>= 1;

            if (v.Target.IsUnderAnyStatus(BattleStatus.Mini) || v.Target.IsUnderAnyStatus(BattleStatus.Sleep) && !v.Target.IsUnderAnyStatus(BattleStatus.EasyKill) || v.Target.IsUnderAnyStatus(BattleStatus.Freeze))
                v.Context.Attack = (Int16)(v.Context.Attack * 3 >> 1);

            if (StackBreakOrUpStatus[v.Caster.Data][0] != 0)
                v.Context.Attack += ((StackBreakOrUpStatus[v.Caster.Data][0] * v.Context.Attack) / 100);
            if (StackBreakOrUpStatus[v.Target.Data][2] != 0)
                v.Context.Attack -= ((StackBreakOrUpStatus[v.Target.Data][2] * v.Context.Attack) / 100);

            if (v.Context.Attack < 1)
                v.Context.Attack = 1;
        }

        public static void CasterPhysicalPenaltyAndBonusAttack(BattleCalculator v)
        {
            if (v.Caster.IsUnderAnyStatus(BattleStatus.Mini))
                v.Context.DecreaseAttackDrastically();
            if (v.Caster.IsUnderAnyStatus(BattleStatus.Berserk) || v.Caster.IsPlayer && v.Caster.IsUnderAnyStatus(BattleStatus.Trance))
                v.Context.Attack = (Int16)(v.Context.Attack * 3 >> 1);
        }

        public static void EnemyTranceBonusAttack(BattleCalculator v)
        {
            if (!v.Caster.IsPlayer && v.Caster.IsUnderAnyStatus(BattleStatus.Trance))
                v.Context.Attack = (Int16)(v.Context.Attack * 3 >> 1);
        }

        public static void BonusBackstabAndPenaltyLongDistance(this BattleCalculator v)
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

                        if (v.Target.Row != unit.Row && !unit.IsUnderAnyStatus(BattleStatusConst.Immobilized) && (unit.Position == v.Target.Position + 1 || unit.Position == v.Target.Position - 1))
                            longDistance = true;
                    }
                }
                if (longDistance)
                    v.Context.Attack /= 2;
            }
        }

        public static void MagicAccuracy(this BattleCalculator v)
        {
            v.Context.HitRate = (Int16)(v.Command.HitRate + (v.Caster.Magic >> 2) + v.Caster.Level - v.Target.Level);

            //if (v.Context.HitRate > 100)
            //    v.Context.HitRate = 100;

            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)230))
                AmarantPassive(v);

            if (v.Context.HitRate < 1)
                v.Context.HitRate = 1;

            v.Context.Evade = v.Target.MagicEvade;

            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)1200) && v.Caster.PlayerIndex == CharacterId.Zidane) // SA Knavery+
                v.Context.Evade += ZidanePassive[v.Caster.Data][0];
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

            if (v.Caster.IsUnderAnyStatus(BattleStatus.CustomStatus18)) // Silence Easy Kill - 10% magic attack malus for bosses with Silence.
                v.Context.Attack = (9 * v.Context.Attack) / 10;

            Int32 ReduceWeaponDamage = 0;
            switch (v.Target.Weapon)
            {
                case RegularItem.IronSword:
                    ReduceWeaponDamage += 10;
                    break;
                case RegularItem.MythrilSword:
                    ReduceWeaponDamage += 15;
                    break;
                case RegularItem.IceBrand:
                    ReduceWeaponDamage += 20;
                    break;
                case RegularItem.CoralSword:
                    ReduceWeaponDamage += 20;
                    break;
                case RegularItem.DiamondSword:
                    ReduceWeaponDamage += 25;
                    break;
                case RegularItem.FlameSaber:
                    ReduceWeaponDamage += 25;
                    break;
                case RegularItem.RuneBlade:
                    ReduceWeaponDamage += 30;
                    break;
                case RegularItem.Defender:
                    ReduceWeaponDamage += 30;
                    break;
            }
            if (ReduceWeaponDamage > 0 && v.Target.IsUnderAnyStatus(BattleStatus.Defend))
                v.Context.Attack -= (ReduceWeaponDamage * v.Context.Attack) / 100;

            ViviFocus(v);

            if (StackBreakOrUpStatus[v.Caster.Data][1] != 0)
                v.Context.Attack += ((StackBreakOrUpStatus[v.Caster.Data][1] * v.Context.Attack) / 100);
            if (StackBreakOrUpStatus[v.Target.Data][3] != 0)
                v.Context.Attack -= ((StackBreakOrUpStatus[v.Target.Data][3] * v.Context.Attack) / 100);

            if (v.Context.Attack < 1)
                v.Context.Attack = 1;
        }

        public static void PenaltyCommandDividedAttack(this BattleCalculator v)
        {
            if (v.Command.IsDevided)
                v.Context.Attack /= 2;
        }

        public static void CasterPenaltyMini(this BattleCalculator v)
        {
            if (v.Caster.IsUnderAnyStatus(BattleStatus.Mini))
                v.Context.Attack /= 2;
        }

        public static void PrepareHpDraining(this BattleCalculator v)
        {
            v.Target.Flags |= CalcFlag.HpAlteration;
            v.Caster.Flags |= CalcFlag.HpAlteration;

            if (v.Target.IsZombie || v.Context.IsAbsorb || (Int32)v.Target.GetPropertyByName("StatusProperty CustomStatus21 CursedBlood") != 0)
                v.Target.Flags |= CalcFlag.HpRecovery;
            else
                v.Caster.Flags |= CalcFlag.HpRecovery;

            v.Context.IsDrain = true;
        }

        public static void BonusWeaponElement(this BattleCalculator v)
        {
            if ((v.Caster.WeaponElement & v.Caster.BonusElement) != 0)
                v.Context.Attack = (Int16)(v.Context.Attack * 3 >> 1);

            if ((WeaponNewElement[v.Caster.Data] & v.Caster.BonusElement) != 0)
                v.Context.Attack = (Int16)(v.Context.Attack * 3 >> 1);
        }

        public static void BonusElement(this BattleCalculator v)
        {
            if ((v.Command.ElementForBonus & v.Caster.BonusElement) != 0)
                v.Context.Attack = (Int16)(v.Context.Attack * 3 >> 1);
        }

        public static Boolean CanAttackWeaponElementalCommand(this BattleCalculator v)
        {
            EffectElement WeaponElement = v.Caster.WeaponElement;

            if (WeaponNewElement[v.Caster.Data] != 0)
                WeaponElement |= WeaponNewElement[v.Caster.Data];

            if (v.Target.CanGuardElement(WeaponElement))
                return false;

            v.Target.PenaltyHalfElement(WeaponElement);
            v.Target.PenaltyAbsorbElement(WeaponElement);
            v.Target.BonusWeakElement(WeaponElement);
            v.Target.AlterStatuses(WeaponElement);

            if (WeaponNewElement[v.Caster.Data] != 0 & v.Target.IsWeakElement(WeaponElement))
            {
                if (v.Caster.PlayerIndex == (CharacterId)12) // SA Maximum infusion
                {
                    BattleAbilityId InfusedAA = ViviPreviousSpell[v.Caster.Data];
                    if (InfusedAA == (BattleAbilityId)1095 || InfusedAA == (BattleAbilityId)1096 || InfusedAA == (BattleAbilityId)1097 || InfusedAA == (BattleAbilityId)1098)
                    {
                        v.Context.Attack = (Int16)(v.Context.Attack * 3 >> 1);
                    }
                    else if (InfusedAA == (BattleAbilityId)1091 || InfusedAA == (BattleAbilityId)1092 || InfusedAA == (BattleAbilityId)1093 || InfusedAA == (BattleAbilityId)1094)
                    {
                        ++v.Context.DamageModifierCount;
                    }
                }
            }

            return true;
        }

        public static Boolean CanAttackMagic(this BattleCalculator v)
        {
            if (v.Target.IsUnderAnyStatus(CustomStatus.Runic))
            {
                v.CalcHpDamage();
                v.Target.Flags = (CalcFlag.HpDamageOrHeal | CalcFlag.MpDamageOrHeal);
                v.Target.MpDamage = Math.Max(1, v.Target.HpDamage / 40);
                v.Target.HpDamage = Math.Max(1, v.Target.HpDamage / 2);
                v.Command.AbilityStatus = 0;
                return false;
            }

            if (v.Target.IsLevitate && v.Command.IsGround)
            {
                v.Context.Flags |= BattleCalcFlags.Miss;
                return false;
            }

            if (v.Target.CanGuardElement(v.Command.Element))
                return false;

            if (v.Target.IsHalfElement(v.Command.Element))
                v.Context.Attack >>= 1;

            if (v.Target.IsWeakElement(v.Command.Element))
                v.Context.Attack = (Int16)(v.Context.Attack * 3 >> 1);

            if (v.Target.CanAbsorbElement(v.Command.Element))
            {
                if (v.Target.HasSupportAbilityByIndex((SupportAbility)241) && (v.Command.Element & EffectElement.Darkness) != 0) // SA Dark side
                {
                    v.Context.DefensePower = 0;
                    if (v.Target.HasSupportAbilityByIndex((SupportAbility)1241))
                    {
                        v.Target.Trance = (byte)Math.Min(v.Target.Trance + (Comn.random16() % v.Target.Will), Byte.MaxValue);
                        if (v.Target.Trance >= Byte.MaxValue)
                            v.Target.AlterStatus(BattleStatus.Trance);
                    }
                }
            }
            if (AbsorbElement.TryGetValue(v.Target.Data, out Int32 elementprotect))
                if ((v.Command.Element & (EffectElement)elementprotect) != 0 && elementprotect != -1)
                    v.Context.Flags |= BattleCalcFlags.Absorb;

            v.Target.AlterStatuses(v.Command.Element);

            if (v.Target.PlayerIndex == CharacterId.Beatrix)
            {
                v.Context.DefensePower += BeatrixPassive[v.Caster.Data][1];
            }

            return true;
        }

        public static void InfusedWeaponStatus(this BattleCalculator v)
        {
            if (WeaponNewStatus[v.Caster.Data] != 0 && WeaponNewStatus[v.Caster.Data] != BattleStatus.Protect && WeaponNewStatus[v.Caster.Data] != BattleStatus.Shell)
            {
                foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(v.Caster))
                    saFeature.TriggerOnAbility(v, "HitRateSetup", false);
                foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(v.Target))
                    saFeature.TriggerOnAbility(v, "HitRateSetup", true);

                if (v.Caster.IsPlayer)
                {
                    if (v.Caster.WeaponRate > Comn.random16() % 100)
                        v.Target.TryAlterStatuses(WeaponNewStatus[v.Caster.Data], false, v.Caster);
                }
                else
                {
                    if (v.Command.HitRate > Comn.random16() % 100)
                    {
                        v.Target.TryAlterStatuses(WeaponNewStatus[v.Caster.Data], false, v.Caster);
                    }
                }
            }
        }

        public static void ViviFocus(this BattleCalculator v)
        {
            if (ViviPassive[v.Caster.Data][2] == 0)
            {
                ViviPassive[v.Caster.Data][2] = 1;
                v.Caster.AddDelayedModifier(
                    caster => caster.CurrentAtb >= caster.MaximumAtb,
                    caster =>
                    {
                        ViviPassive[v.Caster.Data][1] = 0;
                        ViviPassive[v.Caster.Data][2] = 0;
                    }
                );
                if (v.Caster.PlayerIndex == CharacterId.Vivi)
                {
                    if ((v.Command.Id == BattleCommandId.BlackMagic || v.Command.Id == BattleCommandId.DoubleBlackMagic))
                    {
                        if (ViviPassive[v.Caster.Data][1] == 0)
                        {
                            ViviPassive[v.Caster.Data][1] = (ushort)(v.Command.TargetCount);
                            if (FF9TextTool.ActionAbilityName(ViviPreviousSpell[v.Caster.Data]) != v.Command.AbilityName)
                            {
                                Int32 BonusFocusMax = 0;
                                switch (v.Caster.Weapon)
                                {
                                    case RegularItem.FlameStaff:
                                    case RegularItem.IceStaff:
                                    case RegularItem.LightningStaff:
                                        BonusFocusMax += 5;
                                        break;
                                    case RegularItem.OakStaff:
                                        BonusFocusMax += 10;
                                        break;
                                    case RegularItem.CypressPile:
                                        BonusFocusMax += 10;
                                        break;
                                    case RegularItem.OctagonRod:
                                        BonusFocusMax += 15;
                                        break;
                                    case RegularItem.HighMageStaff:
                                        BonusFocusMax += 25;
                                        break;
                                    case RegularItem.MaceOfZeus:
                                        BonusFocusMax += 50;
                                        break;
                                }
                                if (ViviPassive[v.Caster.Data][0] < (50 + BonusFocusMax))
                                {
                                    ViviPassive[v.Caster.Data][0] += v.Caster.HasSupportAbilityByIndex((SupportAbility)207) ? 10 : 5; // SA Bobbin
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
                                if (v.Caster.HasSupportAbilityByIndex((SupportAbility)1207)) // SA Bobbin+
                                {
                                    ViviPassive[v.Caster.Data][0] /= 2;
                                    if (ViviPassive[v.Caster.Data][0] % 10U == 5)
                                        ViviPassive[v.Caster.Data][0] += 5;

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
                            }
                            ViviPreviousSpell[v.Caster.Data] = v.Command.AbilityId;
                        }
                        ViviPassive[v.Caster.Data][1]--;
                        if (ViviPassive[v.Caster.Data][1] < 0)
                            ViviPassive[v.Caster.Data][1] = 0;

                        if (v.Command.ScriptId != 17) // Magic Gravity didn't get damage boost.
                        {
                            v.Context.Attack += (v.Context.Attack * ViviPassive[v.Caster.Data][0]) / 100;
                            v.Command.HitRate += (v.Command.HitRate * ViviPassive[v.Caster.Data][0]) / 100;
                        }
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
                                    Int32 BonusFocusMax = 0;
                                    switch (Vivi.Weapon)
                                    {
                                        case RegularItem.FlameStaff:
                                        case RegularItem.IceStaff:
                                        case RegularItem.LightningStaff:
                                            BonusFocusMax += 5;
                                            break;
                                        case RegularItem.OakStaff:
                                            BonusFocusMax += 10;
                                            break;
                                        case RegularItem.CypressPile:
                                            BonusFocusMax += 10;
                                            break;
                                        case RegularItem.OctagonRod:
                                            BonusFocusMax += 15;
                                            break;
                                        case RegularItem.HighMageStaff:
                                            BonusFocusMax += 25;
                                            break;
                                        case RegularItem.MaceOfZeus:
                                            BonusFocusMax += 50;
                                            break;
                                    }
                                    if (ViviPassive[Vivi.Data][0] < (50 + BonusFocusMax))
                                    {
                                        ViviPassive[Vivi.Data][0] += Vivi.HasSupportAbilityByIndex((SupportAbility)207) ? 10 : 5; // SA Bobbin;
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
                                    if (v.Caster.HasSupportAbilityByIndex((SupportAbility)1207)) // SA Bobbin+
                                    {
                                        ViviPassive[Vivi.Data][0] /= 2;
                                        if (ViviPassive[Vivi.Data][0] % 10U == 5)
                                            ViviPassive[Vivi.Data][0] += 5;

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
                                }
                                ViviPreviousSpell[Vivi.Data] = v.Command.AbilityId;
                            }
                        }
                    }
                    ViviPassive[v.Caster.Data][1]--;
                    if (ViviPassive[v.Caster.Data][1] < 0)
                        ViviPassive[v.Caster.Data][1] = 0;
                }
            }
        }

        public static void AmarantPassive(this BattleCalculator v)
        {
            Int32 factor = 0;
            List<BattleStatusId> statuschoosen = new List<BattleStatusId>{ BattleStatusId.Poison, BattleStatusId.Venom, BattleStatusId.Blind, BattleStatusId.Silence, BattleStatusId.Trouble,
                    BattleStatusId.Sleep, BattleStatusId.Freeze, BattleStatusId.Heat, BattleStatusId.Doom, BattleStatusId.Mini, BattleStatusId.Petrify, BattleStatusId.GradualPetrify,
                    BattleStatusId.Berserk, BattleStatusId.Confuse, BattleStatusId.Stop, BattleStatusId.Zombie, BattleStatusId.Slow, CustomStatusId.Vieillissement,
                    CustomStatusId.ArmorBreak, CustomStatusId.MagicBreak, CustomStatusId.MentalBreak, CustomStatusId.PowerBreak};

            for (Int32 i = 0; i < (statuschoosen.Count - 1); i++)
            {
                if (v.Target.IsUnderAnyStatus(statuschoosen[i].ToBattleStatus()))
                {
                    factor++;
                }
            }

            int bonus = v.Caster.HasSupportAbilityByIndex((SupportAbility)1228) ? 12 : (v.Caster.HasSupportAbilityByIndex((SupportAbility)228) ? 10 : 8);

            if (factor > 0)
            {
                if (v.Caster.HasSupportAbilityByIndex((SupportAbility)230))
                {
                    if (v.Caster.HasSupportAbilityByIndex((SupportAbility)1230))
                        v.Context.Attack += (v.Context.Attack * factor * bonus) / 100;

                    v.Context.HitRate += (v.Context.HitRate * factor * bonus) / 100;
                }
                else
                    v.Context.Attack += (v.Context.Attack * factor * bonus) / 100;
            }
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
            v.Target.TryAlterStatuses(v.Command.AbilityStatus, true, v.Caster);
        }

        public static void TryRemoveAbilityStatuses(this BattleCalculator v)
        {
            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)249) && v.Command.AbilityStatus > 0) // SA Assistance
            {
                foreach (BattleStatusId status in v.Command.AbilityStatus.ToStatusList())
                {
                    if (status == BattleStatusId.Death && !v.Caster.HasSupportAbilityByIndex((SupportAbility)1249))
                        continue;

                    if (v.Target.IsUnderAnyStatus(status))
                        v.Caster.Trance = (byte)Math.Min(v.Caster.Trance + (Comn.random16() % v.Caster.Will), Byte.MaxValue);
                }
            }

            if (!v.Target.TryRemoveStatuses(v.Command.AbilityStatus))
                v.Context.Flags |= BattleCalcFlags.Miss;
        }

        public static void TryRemoveItemStatuses(this BattleCalculator v)
        {
            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)249) && v.Command.Item.Status > 0) // SA Assistance
            {
                foreach (BattleStatusId status in v.Command.Item.Status.ToStatusList())
                {
                    if (status == BattleStatusId.Death && !v.Caster.HasSupportAbilityByIndex((SupportAbility)1249))
                        continue;

                    if (v.Target.IsUnderAnyStatus(status))
                        v.Caster.Trance = (byte)Math.Min(v.Caster.Trance + (Comn.random16() % v.Caster.Will), Byte.MaxValue);
                }
            }

            if (!v.Target.TryRemoveStatuses(v.Command.ItemStatus))
                v.Context.Flags |= BattleCalcFlags.Miss;
        }

        public static void RaiseTrouble(this BattleCalculator v)
        {
            if (v.Target.PhysicalDefence != 255 || v.Target.PhysicalDefence != 255 || v.Target.MagicDefence != 255 || v.Target.MagicEvade != 255 && !v.Command.IsManyTarget)
                v.RaiseTrouble();
        }

        public static void SA_StatusApply(BattleUnit inflicter, Boolean Positive)
        {
            if (inflicter != null)
            {
                if (inflicter.HasSupportAbilityByIndex((SupportAbility)128) && inflicter.CurrentMp < inflicter.MaximumMp) // SA Strategist
                {
                    int factor = inflicter.HasSupportAbilityByIndex((SupportAbility)1128) ? 2 : 1;
                    inflicter.CurrentMp = (uint)Math.Min(inflicter.CurrentMp + factor * (inflicter.MaximumMp / 100), inflicter.MaximumMp);
                }
                if (inflicter.HasSupportAbilityByIndex((SupportAbility)131) && !inflicter.InTrance && inflicter.Trance < byte.MaxValue && Positive) // SA Altruistic
                {
                    byte bonusTrance = (byte)(inflicter.Will / 10);
                    if (inflicter.Trance + bonusTrance < Byte.MaxValue)
                        inflicter.Trance += bonusTrance;
                    else
                        inflicter.Trance = Byte.MaxValue;
                }
            }
        }

        public static void SOS_SA(this BattleCalculator v)
        {
            if (v.Target.HasSupportAbilityByIndex((SupportAbility)1103)) // SOS Carapace+
            {
                v.Target.AddDelayedModifier(
                    target => target.CurrentHp > (target.MaximumHp / 2),
                    target =>
                    {
                        target.AlterStatus(BattleStatus.Protect, target);
                    }
                );
            }
            else if (v.Target.HasSupportAbilityByIndex((SupportAbility)103)) // SOS Carapace
            {
                v.Target.AddDelayedModifier(
                    target => !target.IsUnderAnyStatus(BattleStatus.LowHP),
                    target =>
                    {
                        target.AlterStatus(BattleStatus.Protect, target);
                    }
                );
            }

            if (v.Target.HasSupportAbilityByIndex((SupportAbility)1104)) // SOS Blindage+
            {
                v.Target.AddDelayedModifier(
                    target => target.CurrentHp > (target.MaximumHp / 2),
                    target =>
                    {
                        target.AlterStatus(BattleStatus.Shell, target);
                    }
                );
            }
            else if (v.Target.HasSupportAbilityByIndex((SupportAbility)104)) // SOS Blindage
            {
                v.Target.AddDelayedModifier(
                    target => !target.IsUnderAnyStatus(BattleStatus.LowHP),
                    target =>
                    {
                        target.AlterStatus(BattleStatus.Shell, target);
                    }
                );
            }

            if (v.Target.HasSupportAbilityByIndex((SupportAbility)1105)) // SOS Regen+
            {
                v.Target.AddDelayedModifier(
                    target => target.CurrentHp > (target.MaximumHp / 2),
                    target =>
                    {
                        target.AlterStatus(BattleStatus.Regen, target);
                    }
                );
            }
            else if (v.Target.HasSupportAbilityByIndex((SupportAbility)105)) // SOS Regen
            {
                v.Target.AddDelayedModifier(
                    target => !target.IsUnderAnyStatus(BattleStatus.LowHP),
                    target =>
                    {
                        target.AlterStatus(BattleStatus.Regen, target);
                    }
                );
            }

            if (v.Target.HasSupportAbilityByIndex((SupportAbility)1106)) // SOS Booster+
            {
                v.Target.AddDelayedModifier(
                    target => target.CurrentHp > (target.MaximumHp / 2),
                    target =>
                    {
                        target.AlterStatus(BattleStatus.Haste, target);
                    }
                );
            }
            else if (v.Target.HasSupportAbilityByIndex((SupportAbility)106)) // SOS Booster
            {
                v.Target.AddDelayedModifier(
                    target => !target.IsUnderAnyStatus(BattleStatus.LowHP),
                    target =>
                    {
                        target.AlterStatus(BattleStatus.Haste, target);
                    }
                );
            }
        }

        public static void SpecialSA(this BattleCalculator v)
        {
            if (v.Target.HpDamage > 0 && v.Target.IsUnderAnyStatus(CustomStatus.MechanicalArmor) && MonsterMechanic[v.Target.Data][1] > 0 && (v.Target.Flags & CalcFlag.HpRecovery) == 0) // Armor Mechanical
            {
                Int32 DamageReduction = MonsterMechanic[v.Target.Data][1] * 10;
                if (DamageReduction < 100)
                v.Target.HpDamage = ((100 - DamageReduction) * v.Target.HpDamage) / 100;
                else
                {
                    v.Target.Flags = 0;
                    v.Context.Flags |= BattleCalcFlags.Guard;
                }
                MonsterMechanic[v.Target.Data][1]--;
                if (MonsterMechanic[v.Target.Data][1] < 4 && MonsterMechanic[v.Target.Data][2] == 0 && v.Target.Data.dms_geo_id == 446) // Refresh Garland stand animation
                    v.Target.Data.mot[2] = "ANH_MON_B3_185_003";
                if (MonsterMechanic[v.Target.Data][1] < 0)
                {
                    MonsterMechanic[v.Target.Data][1] = 0;
                    v.Target.RemoveStatus(CustomStatusId.MechanicalArmor);
                }
                else
                    v.Target.TryAlterSingleStatus(CustomStatusId.MechanicalArmor, true, v.Caster, MonsterMechanic[v.Target.Data][1]);
            }
            if (v.Context.IsAbsorb)
            {
                v.Target.Flags |= CalcFlag.HpDamageOrHeal;

            }
            if (v.Target.HasSupportAbilityByIndex((SupportAbility)234) && (int)v.Target.GetPropertyByName("StatusProperty CustomStatus12 Stack") >= 2 && v.Target.Will < Comn.random16() % 100 && !v.Caster.IsPlayer) // SA Dominance
            {
                List<BattleAbilityId> Counter_AA = new List<BattleAbilityId>{ BattleAbilityId.ThunderSlash, BattleAbilityId.StockBreak, BattleAbilityId.Climhazzard, BattleAbilityId.Shock,
                BattleAbilityId.Protect, BattleAbilityId.Shell, BattleAbilityId.Cura, BattleAbilityId.Berserk, BattleAbilityId.Reflect, BattleAbilityId.Regen, BattleAbilityId.Holy};

                for (Int32 i = 0; i < Counter_AA.Count; i++)
                {
                    if (!ff9abil.FF9Abil_IsMaster(v.Target.Player, (int)Counter_AA[i]))
                    {
                        Counter_AA.Remove(Counter_AA[i]);
                    }
                }

                BattleAbilityId Counter_AA_Selected = Counter_AA[GameRandom.Next16() % Counter_AA.Count];
                if (Counter_AA_Selected == BattleAbilityId.Protect || Counter_AA_Selected == BattleAbilityId.Shell || Counter_AA_Selected == BattleAbilityId.Reflect || Counter_AA_Selected == BattleAbilityId.Cura || Counter_AA_Selected == BattleAbilityId.Regen)
                {
                    btl_cmd.SetCounter(v.Target, BattleCommandId.Counter, (Int32)Counter_AA_Selected, v.Target.Id);
                }
                else
                {
                    if (Counter_AA_Selected == BattleAbilityId.StockBreak || Counter_AA_Selected == BattleAbilityId.Climhazzard)
                        btl_cmd.SetCounter(v.Target, BattleCommandId.Counter, (Int32)Counter_AA_Selected, 240);
                    else
                        btl_cmd.SetCounter(v.Target, BattleCommandId.Counter, (Int32)Counter_AA_Selected, v.Caster.Id);
                }
            }
            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)110) && !v.Command.IsManyTarget && v.Command.Id != BattleCommandId.Attack && v.Target.HpDamage > 0 && 
                (v.Command.ScriptId == 9 || v.Command.ScriptId == 10 || v.Command.ScriptId == 17 || v.Command.ScriptId == 18 || v.Command.ScriptId == 116 || v.Command.ScriptId == 118
                || v.Command.AbilityId == BattleAbilityId.PumpkinHead || v.Command.AbilityId == BattleAbilityId.ThousandNeedles || v.Command.AbilityId == BattleAbilityId.GoblinPunch
                || v.Command.AbilityId == BattleAbilityId.AutoLife)) // Prolifération
            {
                int basedamage = v.Target.HpDamage;
                BTL_DATA targetdefault = v.Target.Data;
                foreach (BattleUnit unit in BattleState.EnumerateUnits())
                {
                    if ((unit.IsPlayer && !v.Target.IsPlayer || !unit.IsPlayer && v.Target.IsPlayer) || unit.MagicDefence == 255 || unit.PhysicalEvade == 255 || unit.Data == targetdefault)
                        continue;

                    if (unit.Data != targetdefault) 
                    {
                        if (v.Caster.HasSupportAbilityByIndex((SupportAbility)1110))
                        {
                            basedamage = v.Target.HpDamage / 2;
                        }
                        else
                        {
                            basedamage = v.Target.HpDamage / 4;
                        }
                        btl2d.Btl2dStatReq(unit, basedamage, 0);
                    }                   
                }
            }
            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)119) && v.Command.Id == BattleCommandId.MagicSword) // Entente
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

            if (v.Caster.HasSupportAbility(SupportAbility1.ReflectNull) && v.Target.IsUnderAnyStatus(BattleStatus.Reflect) && !v.Caster.HasSupportAbilityByIndex((SupportAbility)1030))
                v.Target.HpDamage >>= 1;

            if (v.Target.IsUnderAnyStatus(CustomStatus.Dragon) && !v.Caster.IsUnderStatus(BattleStatus.Trance) && v.Command.Id == BattleCommandId.DragonAct) // Trigger Dragon status
            {
                float DragonRemove = v.Caster.HasSupportAbilityByIndex((SupportAbility)1122) ? 25 : (v.Caster.HasSupportAbilityByIndex((SupportAbility)122) ? 12.5f : 0); // Eye of the dragon

                if (DragonRemove < Comn.random16() % 100)
                    btl_stat.AlterStatus(v.Target, CustomStatusId.Dragon, v.Caster, parameters: "Remove");
            }

            if (v.Command.Id == (BattleCommandId)10032) // SA Witchcraft
            {
                v.Target.HpDamage /= 2;
                v.Target.MpDamage /= 2;
            }

            if (v.Caster.PlayerIndex == CharacterId.Zidane && (v.Command.Id == BattleCommandId.Attack || v.Command.Id == BattleCommandId.Counter) && ff9item._FF9Item_Data[FF9StateSystem.Common.FF9.player[(CharacterId)v.Caster.Data.bi.slot_no].equip[0]].shape == 1)
            { // Zidane - Dagger double hits
                v.Target.HpDamage /= 2;
                if (ZidanePassive[v.Caster.Data][4] == 2)
                {
                    ZidanePassive[v.Caster.Data][4] = 0;
                    if (v.Target.CurrentHp > v.Target.HpDamage)
                        v.Command.AbilityCategory += 64;
                }
            }

            int HealHP = SpecialSAEffect[v.Caster.Data][5];
            int HealMP = SpecialSAEffect[v.Caster.Data][6];
            if (SpecialSAEffect[v.Caster.Data][7] == 0)
                SpecialSAEffect[v.Caster.Data][7] = v.Command.TargetCount;
            SpecialSAEffect[v.Caster.Data][7]--;

            if (v.Command.AbilityId == BattleAbilityId.DemiShock2) // Tobigeri+
            {
                HealHP = v.Target.HpDamage / 2;
            }
            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)1061) && (v.Command.Id == BattleCommandId.Attack || v.Command.Id == BattleCommandId.Counter) && v.Target.Data != v.Caster.Data) // Mug+
            {
                if (ff9item._FF9Item_Data[FF9StateSystem.Common.FF9.player[(CharacterId)v.Caster.Data.bi.slot_no].equip[0]].shape == 1)
                {
                    if (v.Command.Data.info.effect_counter == 2)
                    {
                        HealHP += (v.Target.HpDamage / 4 + ZidanePassive[v.Caster.Data][7]);
                        ZidanePassive[v.Caster.Data][7] = 0;
                    }
                    else
                    {
                        ZidanePassive[v.Caster.Data][7] = v.Target.HpDamage / 4;
                    }
                }
                else
                {
                    HealHP += v.Target.HpDamage / 4;
                }
            }
            if (v.Caster.Accessory == (RegularItem)1208 && (v.Target.Flags & CalcFlag.HpRecovery) == 0 && v.Target.Data != v.Caster.Data) // Materia Support
            {
                HealHP += v.Target.HpDamage / 10;
            }
            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)115) && (v.Target.WeakElement & v.Command.Element) != 0) // Soul Drain
            {
                HealHP += v.Target.HpDamage / (v.Caster.HasSupportAbilityByIndex((SupportAbility)1115) ? 2 : 4);
            }
            if (WeaponNewStatus[v.Caster.Data] == BattleStatus.Protect && (v.Command.Id == BattleCommandId.Attack || v.Command.Id == BattleCommandId.Counter)) // Drain MagiLame
            {
                HealHP += v.Target.HpDamage / 4;
            }
            if (WeaponNewStatus[v.Caster.Data] == BattleStatus.Shell && (v.Command.Id == BattleCommandId.Attack || v.Command.Id == BattleCommandId.Counter)) // Osmose MagiLame
            {
                HealMP += v.Target.MpDamage / 80;
            }
            if (v.Target.HasSupportAbilityByIndex((SupportAbility)118) && v.Target.IsCovering) // Flawless
            {
                HealMP += (int)(v.Target.HasSupportAbilityByIndex((SupportAbility)1118) ? (v.Target.MaximumMp / 25) : (v.Target.MaximumMp / 50));
            }
            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)202) && (v.Command.AbilityId == BattleAbilityId.Steal || v.Command.AbilityId == BattleAbilityId.Attack && v.Caster.HasSupportAbility(SupportAbility2.Bandit))) // SA Phantom hand
            {
                HealMP += (int)(v.Target.HasSupportAbilityByIndex((SupportAbility)1202) ? (v.Target.MaximumMp / 25) : (v.Target.MaximumMp / 50));
            }

            if ((HealHP > 0 || HealMP > 0) && !v.Caster.IsUnderAnyStatus(BattleStatus.Death) && SpecialSAEffect[v.Caster.Data][7] <= 0)
            {
                if (HealHP > 0)
                {
                    v.Caster.CurrentHp = Math.Min(v.Caster.CurrentHp + (uint)HealHP, v.Caster.MaximumHp);
                }
                if (HealMP > 0)
                {
                    v.Caster.CurrentMp = Math.Min(v.Caster.CurrentMp + (uint)HealMP, v.Caster.MaximumMp);
                }
                btl2d.Btl2dStatReq(v.Caster, -HealHP, -HealMP);
                SpecialSAEffect[v.Caster.Data][5] = 0;
                SpecialSAEffect[v.Caster.Data][6] = 0;
            }
            else
            {
                SpecialSAEffect[v.Caster.Data][5] += HealHP;
                SpecialSAEffect[v.Caster.Data][6] += HealMP;
            }

            if (v.Command.ScriptId != 88 && v.Command.Power != 99 & v.Command.HitRate != 99) // Prevent to trigger Last Stand on Game Over type attack.
            {
                if (v.Target.HasSupportAbilityByIndex((SupportAbility)52) && SpecialSAEffect[v.Target.Data][1] > 0 && v.Target.HpDamage > v.Target.CurrentHp && v.Target.CurrentMp > 0 && (v.Target.Flags & CalcFlag.HpRecovery) == 0) // Last Stand
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
            }

            if (v.Caster.Weapon == (RegularItem)1100 && (v.Target.Flags & CalcFlag.HpRecovery) == 0)
            {
                v.Target.HpDamage = 1;
            }

            if (v.Target.HasSupportAbilityByIndex((SupportAbility)123) && (v.Command.AbilityCategory & 8) != 0 && v.Command.TargetCount == 1) // Assistance
            {
                int HPAssistanceDamage = 0;
                int MPAssistanceDamage = 0;

                if ((v.Target.Flags & CalcFlag.HpAlteration) != 0)
                {
                    v.Target.HpDamage = Math.Max((v.Target.HpDamage * (v.Target.HasSupportAbilityByIndex((SupportAbility)1123) ? 70 : 85)) / 100, 1);
                    HPAssistanceDamage = Math.Max((v.Target.HpDamage * (v.Target.HasSupportAbilityByIndex((SupportAbility)1123) ? 30 : 15)) / 100, 1);
                }
                if ((v.Target.Flags & CalcFlag.MpAlteration) != 0)
                {
                    v.Target.MpDamage = Math.Max((v.Target.MpDamage * (v.Target.HasSupportAbilityByIndex((SupportAbility)1123) ? 70 : 85)) / 100, 1);
                    MPAssistanceDamage = Math.Max((v.Target.MpDamage * (v.Target.HasSupportAbilityByIndex((SupportAbility)1123) ? 30 : 15)) / 100, 1);
                }

                foreach (BattleUnit unit in BattleState.EnumerateUnits())
                {
                    if (!unit.IsPlayer || !unit.IsTargetable || unit.Data == v.Target.Data || unit.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify))
                        continue;

                    if (HPAssistanceDamage > 0)
                    {
                        unit.CurrentHp = Math.Max(unit.CurrentHp - (uint)HPAssistanceDamage, 0);
                    }
                    if (MPAssistanceDamage > 0)
                    {
                        unit.CurrentMp = Math.Max(unit.CurrentMp - (uint)MPAssistanceDamage, 0);
                    }
                    btl2d.Btl2dStatReq(unit.Data, HPAssistanceDamage, MPAssistanceDamage);
                }
            }

            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)1131) && !v.Caster.InTrance && v.Caster.Trance < byte.MaxValue && (v.Target.Flags & CalcFlag.HpRecovery) != 0) // SA Altruistic+
            {
                byte bonusTrance = (byte)(v.Caster.Will / 10);
                if (v.Caster.Trance + bonusTrance < Byte.MaxValue)
                    v.Caster.Trance += bonusTrance;
                else
                    v.Caster.Trance = Byte.MaxValue;
            }

            if (v.Target.HasSupportAbilityByIndex((SupportAbility)1201) && (v.Command.AbilityCategory & 8) != 0 && ZidanePassive[v.Target.Data][1] > Comn.random16() % 100) // SA Gorilla+
                BattleState.EnqueueCounter(v.Target, BattleCommandId.Counter, BattleAbilityId.Attack, v.Caster.Id);

            if (v.Target.HasSupportAbilityByIndex((SupportAbility)211) && (v.Target.Flags & CalcFlag.HpRecovery) == 0 && !v.Target.IsUnderAnyStatus(BattleStatus.Heat) && v.Target.CurrentHp < (v.Target.MaximumHp / (v.Target.HasSupportAbilityByIndex((SupportAbility)1211) ? 1 : 2)) && !v.Caster.IsPlayer) // SA Auto Gem
            {
                List <RegularItem> GemList = new List<RegularItem>{ RegularItem.Garnet, RegularItem.Amethyst, RegularItem.Aquamarine, RegularItem.Diamond, RegularItem.Emerald, RegularItem.Moonstone,
                    RegularItem.Ruby, RegularItem.Peridot, RegularItem.Sapphire, RegularItem.Opal, RegularItem.Topaz, RegularItem.LapisLazuli};

                List<BattleStatusId> statuschoosen = new List<BattleStatusId>();

                for (Int32 i = 0; i < GemList.Count; i++)
                {
                    if (ff9item.FF9Item_GetCount(GemList[i]) <= 0)
                    {
                        GemList.Remove(GemList[i]);
                    }
                }
                RegularItem GemSelected = GemList[GameRandom.Next16() % GemList.Count];
                UIManager.Battle.ItemRequest(GemSelected);
                btl_cmd.SetCounter(v.Target, BattleCommandId.AutoPotion, (Int32)GemSelected, v.Target.Id);
            }

            if (v.Target.HasSupportAbilityByIndex((SupportAbility)225) && (v.Target.Flags & CalcFlag.HpRecovery) == 0 && v.Target.HpDamage > 0 && Comn.random16() % 100 < 10) // SA Bodyguard (10%)
            {
                if (v.Target.HasSupportAbilityByIndex((SupportAbility)1225))
                {
                    v.Target.HpDamage = 0;
                }
                else
                {
                    v.Target.HpDamage /= 2;
                }
            }

            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)229) && (v.Target.Flags & CalcFlag.Critical) != 0) // SA Lethality
            {
                v.Target.AlterStatus(v.Caster.WeaponStatus, v.Caster);
                if (v.Caster.HasSupportAbilityByIndex((SupportAbility)1229))
                    v.Target.AlterStatus(BattleStatus.Doom, v.Caster);
            }

            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)1238) && (v.Target.Flags & CalcFlag.HpRecovery) == 0 && v.Target.HpDamage > 0) // SA Crisis level+
            {
                float RatioCrisisLevel = (v.Caster.CurrentHp * 100) / v.Caster.MaximumHp;
                v.Target.HpDamage += (short)((v.Target.HpDamage * (100 - RatioCrisisLevel)) / 200);
            }

            if (v.Target.IsUnderAnyStatus(BattleStatus.AutoLife) && (v.Target.Flags & CalcFlag.HpRecovery) == 0 && !v.Target.IsPlayer && v.Target.HpDamage >= v.Target.CurrentHp)
            {
                v.Target.HpDamage = (int)(v.Target.CurrentHp - 1);
                Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                            {
                                { "US", "Auto-Life!" },
                                { "UK", "Auto-Life!" },
                                { "JP", "リレイズ!" },
                                { "ES", "¡AutoLázaro!" },
                                { "FR", "Auréole !" },
                                { "GR", "Reinkarnat!" },
                                { "IT", "Risveglio!" },
                            };
                btl2d.Btl2dReqSymbolMessage(v.Target.Data, "[FF99FD]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 20);
                v.Target.RemoveStatus(BattleStatus.AutoLife);
            }

            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)235) && v.Command.Id == BattleCommandId.Attack) // SA Fencing
                v.Target.HpDamage += v.Caster.HasSupportAbilityByIndex((SupportAbility)1235) ? v.Target.HpDamage / 4 : v.Target.HpDamage / 8;
        }
    }
}
