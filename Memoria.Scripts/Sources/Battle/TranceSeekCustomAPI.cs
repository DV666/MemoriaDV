using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Assets;
using Memoria.Data;
using Memoria.Prime;
using UnityEngine;

namespace Memoria.Scripts.Battle
{
    public static class TranceSeekCustomAPI
    {
        public static Dictionary<BTL_DATA, Boolean> InitBTL = new Dictionary<BTL_DATA, Boolean>();

        public static Dictionary<BTL_DATA, Int32[]> ZidanePassive = new Dictionary<BTL_DATA, Int32[]>(); // [0] => Dodge ; [1] => Critical ; [2] => Eye of the thief ; [3] => Master Thief ; [4] => Dagger Attack ; [5] => FirstItemMug ; [6] => SecondItemMug

        public static Dictionary<BTL_DATA, Int32[]> ViviPassive = new Dictionary<BTL_DATA, Int32[]>(); // [0] => Focus ; [1] => NumberTargets
        public static Dictionary<BTL_DATA, BattleAbilityId> ViviPreviousSpell = new Dictionary<BTL_DATA, BattleAbilityId>();

        public static Dictionary<BTL_DATA, Int32[]> BeatrixPassive = new Dictionary<BTL_DATA, Int32[]>(); // [0] => Strength ; [1] => Magic ; [2] => Bravoure ; [3] => TargetCount

        public static Dictionary<BTL_DATA, Int32> StateMoug = new Dictionary<BTL_DATA, Int32>();
        public static Dictionary<BTL_DATA, GameObject> ModelMoug = new Dictionary<BTL_DATA, GameObject>();

        public static Dictionary<BTL_DATA, Int32[]> MonsterMechanic = new Dictionary<BTL_DATA, Int32[]>(); // [0] => Trance Activated ; [1] => Special1 ; [2] => Special2 ; [3] => HPBoss10000? ; [4] => ResistStatusEasyKill ; [5] => Dragon

        public static Dictionary<BTL_DATA, Int32[]> SpecialSAEffect = new Dictionary<BTL_DATA, Int32[]>(); // [0] => Millionaire (not used anymore) ; [1] => LastStand ; [2] => Instinct ; [3] => PreventTranceSFX ; [4] => Mode EX

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
            if (quarterWill != 0 && ((Comn.random16() % quarterWill) + v.Caster.Data.critical_rate_deal_bonus + v.Target.Data.critical_rate_receive_resistance + BonusWeaponCritical > Comn.random16() % 100) || v.Target.IsUnderAnyStatus(CustomStatus.PerfectCrit))
            {
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
                        { "ES", "↑ Letale ↑" },
                        { "FR", "↑ Critique ↑" },
                        { "GR", "↑ Letal ↑" },
                        { "IT", "↑ KRITISCH ↑" },
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

            if ((v.Context.HitRate <= Comn.random16() % 100) || v.Target.PhysicalEvade == 255)
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
            if (v.Target.PhysicalDefence == 255)
            {
                v.Context.Flags |= BattleCalcFlags.Guard;
                return;
            }
            if (v.Target.PlayerIndex == CharacterId.Beatrix)
            {
                v.Context.DefensePower += BeatrixPassive[v.Caster.Data][0];
            }

            if (v.Target.PlayerIndex == CharacterId.Steiner && v.Target.IsUnderAnyStatus(BattleStatus.Defend) && v.Target.IsCovering && v.Target.HasSupportAbilityByIndex((SupportAbility)118)) // Flawless Steiner
            {
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

                        if (v.Target.Row != unit.Row && !unit.IsUnderAnyStatus(BattleStatusConst.Immobilized) && (unit.Position == v.Target.Position + 1 || unit.Position == v.Target.Position - 1))
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
        }

        public static void PrepareHpDraining(this BattleCalculator v)
        {
            v.Target.Flags |= CalcFlag.HpAlteration;
            v.Caster.Flags |= CalcFlag.HpAlteration;

            if (v.Target.IsZombie || v.Context.IsAbsorb)
                v.Target.Flags |= CalcFlag.HpRecovery;
            else
                v.Caster.Flags |= CalcFlag.HpRecovery;

            v.Context.IsDrain = true;
        }

        public static void BonusWeaponElement(this BattleCalculator v)
        {
            if ((v.Caster.WeaponElement & v.Caster.BonusElement) != 0)
                ++v.Context.DamageModifierCount;

            if ((WeaponNewElement[v.Caster.Data] & v.Caster.BonusElement) != 0)
                ++v.Context.DamageModifierCount;
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
            v.Target.TryAlterStatuses(v.Command.AbilityStatus, true, v.Caster);
            v.Caster.Will = CasterWill;
        }

        public static void RaiseTrouble(this BattleCalculator v)
        {
            if (v.Target.PhysicalDefence != 255 || v.Target.PhysicalDefence != 255 || v.Target.MagicDefence != 255 || v.Target.MagicEvade != 255 && !v.Command.IsManyTarget)
                v.RaiseTrouble();
        }
        public static void SOS_SA(this BattleCalculator v)
        {
            if (v.Target.HasSupportAbilityByIndex((SupportAbility)103)) // SOS Carapace
            {
                v.Target.AddDelayedModifier(
                    target => !target.IsUnderAnyStatus(BattleStatus.LowHP),
                    target =>
                    {
                        target.AlterStatus(BattleStatus.Protect, target);
                    }
                );
            }

            if (v.Target.HasSupportAbilityByIndex((SupportAbility)104)) // SOS Blindage
            {
                v.Target.AddDelayedModifier(
                    target => !target.IsUnderAnyStatus(BattleStatus.LowHP),
                    target =>
                    {
                        target.AlterStatus(BattleStatus.Shell, target);
                    }
                );
            }

            if (v.Target.HasSupportAbilityByIndex((SupportAbility)105)) // SOS Regen
            {
                v.Target.AddDelayedModifier(
                    target => !target.IsUnderAnyStatus(BattleStatus.LowHP),
                    target =>
                    {
                        target.AlterStatus(BattleStatus.Regen, target);
                    }
                );
            }

            if (v.Target.HasSupportAbilityByIndex((SupportAbility)106)) // SOS Booster
            {
                v.Target.AddDelayedModifier(
                    target => !target.IsUnderAnyStatus(BattleStatus.LowHP),
                    target =>
                    {
                        target.AlterStatus(BattleStatus.Haste, target);
                    }
                );
            }

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
        }

        public static void SpecialSA(this BattleCalculator v)
        {
            if (MonsterMechanic[v.Target.Data][5] > 0)
                MonsterMechanic[v.Target.Data][5] = 0;
            if (v.Target.HpDamage > 0 && v.Target.IsUnderAnyStatus(CustomStatus.MechanicalArmor) && MonsterMechanic[v.Target.Data][1] > 0 && v.Target.Data != v.Caster.Data && (v.Target.Flags & CalcFlag.HpRecovery) == 0) // Armor Mechanical
            {
                Int32 DamageReduction = MonsterMechanic[v.Target.Data][1] * 10;
                if (DamageReduction < 100)
                v.Target.HpDamage = ((100 - DamageReduction) * v.Target.HpDamage) / 100;
                else
                {
                    v.Target.Flags = 0;
                    v.Context.Flags |= BattleCalcFlags.Guard;
                }
                MonsterMechanic[v.Target.Data][1] -= 1;
                if (MonsterMechanic[v.Target.Data][1] < 4 && MonsterMechanic[v.Target.Data][2] == 0 && v.Target.Data.dms_geo_id == 446) // Refresh Garland stand animation
                    v.Target.Data.mot[2] = "ANH_MON_B3_185_003";
                if (MonsterMechanic[v.Target.Data][1] < 0)
                    MonsterMechanic[v.Target.Data][1] = 0;

                v.Target.TryAlterSingleStatus(CustomStatusId.MechanicalArmor, true, v.Caster, MonsterMechanic[v.Target.Data][1]);
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
                    if (v.Command.Id == BattleCommandId.Attack || v.Command.Id == BattleCommandId.Defend || v.Command.Id == BattleCommandId.Counter ||
                        v.Command.Id == BattleCommandId.HolyWhiteMagic || v.Caster.IsUnderAnyStatus(BattleStatus.Trance))
                    {
                        v.Caster.AlterStatus(CustomStatus.Redemption, v.Caster);
                    }
                    else if (v.Command.Data.info.cover == 1 && v.Target.HasSupportAbility(SupportAbility2.Cover))
                    {
                        v.Target.AlterStatus(CustomStatus.Redemption, v.Caster);
                    }
                    else if (v.Command.Id == BattleCommandId.HolySword1)
                    {
                        v.Caster.RemoveStatus(CustomStatus.Redemption);
                    }
                }
                BeatrixPassive[v.Caster.Data][3]--;
                if (BeatrixPassive[v.Caster.Data][3] < 0)
                    BeatrixPassive[v.Caster.Data][3] = 0;
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
                    v.Target.RemoveStatus(CustomStatus.Dragon);
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

            int HealHPSAOrItem = 0;
            int HealMPSAOrItem = 0;
            if (v.Command.AbilityId == BattleAbilityId.DemiShock2) // Tobigeri+
            {
                HealHPSAOrItem = v.Target.HpDamage / 2;
            }
            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)1061) && (v.Command.Id == BattleCommandId.Attack || v.Command.Id == BattleCommandId.Counter)) // Mug+
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
            if (WeaponNewStatus[v.Caster.Data] == BattleStatus.Protect && (v.Command.Id == BattleCommandId.Attack || v.Command.Id == BattleCommandId.Counter)) // Drain MagiLame
            {
                HealHPSAOrItem += v.Target.HpDamage / 4;
            }
            if (WeaponNewStatus[v.Caster.Data] == BattleStatus.Shell && (v.Command.Id == BattleCommandId.Attack || v.Command.Id == BattleCommandId.Counter)) // Osmose MagiLame
            {
                HealMPSAOrItem += v.Target.MpDamage / 80;
            }
            if (HealHPSAOrItem > 0 || HealMPSAOrItem > 0)
            {
                v.Caster.AddDelayedModifier(
                    caster => caster.CurrentAtb >= caster.MaximumAtb,
                    caster =>
                    {
                        if (HealHPSAOrItem > 0)
                        {
                            caster.CurrentHp = Math.Min(caster.CurrentHp + (uint)HealHPSAOrItem, caster.MaximumHp);
                        }
                        if (HealMPSAOrItem > 0)
                        {
                            caster.CurrentMp = Math.Min(caster.CurrentMp + (uint)HealMPSAOrItem, caster.MaximumMp);
                        }
                        btl2d.Btl2dStatReq(caster, -HealHPSAOrItem, -HealMPSAOrItem);
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

            if (v.Caster.Weapon == (RegularItem)1100 && (v.Target.Flags & CalcFlag.HpRecovery) == 0)
            {
                v.Target.HpDamage = 1;
            }
        }
    }
}
