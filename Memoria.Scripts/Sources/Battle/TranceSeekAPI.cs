using System;
using System.Collections.Generic;
using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Assets;
using Memoria.Data;
using Memoria.Database;
using UnityEngine;
using static SiliconStudio.Social.ResponseData;

namespace Memoria.Scripts.Battle
{
    public static class TranceSeekAPI
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
        public static Dictionary<BTL_DATA, Int32> StateMoug = new Dictionary<BTL_DATA, Int32>();
        public static Dictionary<BTL_DATA, GameObject> ModelMoug = new Dictionary<BTL_DATA, GameObject>();
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

        public static Boolean EliteMonster(BTL_DATA Monster)
        {
            if (Monster.bi.player != 0)
                return false;

            return (btl_util.getEnemyPtr(Monster).info.flags & 128) != 0; // Unused (8)
        }

        public static void WeaponPhysicalParams(CalcAttackBonus bonus, BattleCalculator v)
        {
            Int32 baseDamage = Comn.random16() % (1 + (v.Caster.Level + v.Caster.Strength >> 3));
            v.Context.AttackPower = v.Caster.GetWeaponPower(v.Command);
            v.Target.SetPhysicalDefense();
            switch (bonus)
            {
                case CalcAttackBonus.Simple:
                    v.Context.Attack = v.Caster.Strength + baseDamage;
                    break;
                case CalcAttackBonus.WillPower:
                    v.Context.Attack = (v.Caster.Strength + v.Caster.Will >> 1) + baseDamage;
                    break;
                case CalcAttackBonus.Dexterity:
                    v.Context.Attack = (v.Caster.Strength + v.Caster.Data.elem.dex >> 1) + baseDamage;
                    break;
                case CalcAttackBonus.Magic:
                    v.Context.Attack = (v.Caster.Strength + v.Caster.Data.elem.mgc) + baseDamage;
                    break;
                case CalcAttackBonus.Random:
                {
                    if (v.Caster.HasSupportAbilityByIndex((SupportAbility)222)) // SA Sharpening
                    {
                        if (v.Caster.HasSupportAbilityByIndex((SupportAbility)1222)) // SA Sharpening +
                            v.Context.Attack = v.Caster.Strength + baseDamage;
                        else
                            v.Context.Attack = UnityEngine.Random.Range(v.Caster.Strength / 2, v.Caster.Strength) + baseDamage;
                    }
                    else
                    {
                        v.Context.Attack = Comn.random16() % v.Caster.Strength + baseDamage;
                    }
                    break;
                }
                case CalcAttackBonus.Level:
                    v.Context.AttackPower += v.Caster.Data.level;
                    v.Context.Attack = v.Caster.Strength + baseDamage;
                    break;
            }
        }

        public static void TryCriticalHit(this BattleCalculator v, int BonusCrit = 0)
        {
            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)1102)) // Archimage+ (10% crit en bonus)
                ZidanePassive[v.Caster.Data][1] = 40;
            Int32 quarterWill = (v.Caster.Data.elem.wpr + ZidanePassive[v.Caster.Data][1]) >> 2;
            BonusCriticalFromWeapon(v.Caster.Weapon, out Int32 BonusWeaponCritical);
            if (quarterWill != 0 && (((Comn.random16() % quarterWill) + v.Caster.Data.critical_rate_deal_bonus + v.Target.Data.critical_rate_receive_resistance + BonusWeaponCritical + BonusCrit) > Comn.random16() % 100) || v.Caster.IsUnderAnyStatus(TranceSeekStatus.PerfectCrit) || SpecialSAEffect[v.Target.Data][9] > 0)
            {
                if (SpecialSAEffect[v.Target.Data][9] > 0)
                    SpecialSAEffect[v.Target.Data][9]--;
                if (v.Caster.IsUnderAnyStatus(TranceSeekStatus.PerfectCrit)) // Perfect Crit
                    btl_stat.AlterStatus(v.Caster, TranceSeekStatusId.PerfectCrit, parameters: "-1");
                else
                    ZidanePassive[v.Caster.Data][1] = 0;
                v.Context.Attack *= 2;
                v.Target.HpDamage *= 2;
                v.Target.MpDamage *= 2;
                v.Target.Flags |= CalcFlag.Critical;
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

        public static void TryApplyDragon(this BattleCalculator v)
        {
            if (v.Caster.PlayerIndex == CharacterId.Freya)
            {
                Int32 quarterWill = v.Caster.Data.elem.wpr >> 2;
                Int32 bonusdragon = 2;
                switch (v.Caster.Weapon)
                {
                    case RegularItem.MythrilSpear:
                        bonusdragon = 4;
                        break;
                    case RegularItem.Partisan:
                        bonusdragon = 5;
                        break;
                    case RegularItem.IceLance:
                        bonusdragon = 6;
                        break;
                    case RegularItem.Trident:
                        bonusdragon = 7;
                        break;
                    case RegularItem.HeavyLance:
                        bonusdragon = 8;
                        break;
                    case RegularItem.Obelisk:
                        bonusdragon = 9;
                        break;
                    case RegularItem.HolyLance:
                        bonusdragon = 10;
                        break;
                    case RegularItem.KainLance:
                        bonusdragon = 12;
                        break;
                    case RegularItem.DragonHair:
                        bonusdragon = 15;
                        break;
                }

                FreyaPassive[v.Target.Data][0] += bonusdragon;

                if (quarterWill != 0)
                {
                    if ((((Comn.random16() % quarterWill) + FreyaPassive[v.Target.Data][0]) > Comn.random16() % 100) || ((v.Target.Flags & CalcFlag.Critical) != 0 && v.Command.Id == BattleCommandId.Attack))
                    {
                        v.Target.AlterStatus(TranceSeekStatus.Dragon, v.Caster);
                        FreyaPassive[v.Target.Data][0] = 0;
                    }
                }
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

        public static Boolean TryKillFrozen(this BattleCalculator v)
        {
            if (!v.Target.IsUnderAnyStatus(BattleStatus.Freeze) || v.Target.IsUnderAnyStatus(BattleStatus.Petrify))
                return false;
            if (v.Target.IsUnderAnyStatus(BattleStatus.EasyKill) && !EliteMonster(v.Target.Data))
                return false;

            BattleVoice.TriggerOnStatusChange(v.Target.Data, BattleVoice.BattleMoment.Used, BattleStatusId.Freeze);
            btl_cmd.KillSpecificCommand(v.Target.Data, BattleCommandId.SysStone);
            v.Target.Kill();
            UIManager.Battle.SetBattleFollowMessage(BattleMesages.ImpactCrushes);
            return true;
        }

        public static Boolean TryPhysicalHit(this BattleCalculator v)
        {
            if (MonsterMechanic[v.Caster.Data][6] == 1)
            {
                MonsterMechanic[v.Caster.Data][6] = 0;
                return true;
            }
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

            if (v.Caster.PlayerIndex == CharacterId.Zidane && v.Command.Id == BattleCommandId.Attack && ff9item._FF9Item_Data[v.Caster.Weapon].shape == 1) // Zidane - Dagger double hits
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

            if (v.Caster.IsUnderAnyStatus(BattleStatus.Trance | BattleStatus.Vanish) || v.Target.IsUnderAnyStatus(BattleStatusConst.PenaltyEvade))
                v.Context.Evade = 0;

            if (v.Target.IsUnderAnyStatus(BattleStatus.Defend))
            {
                if (v.Target.HasSupportAbilityByIndex(SupportAbility.AutoFloat)) // Pas Léger
                    v.Context.Evade /= v.Target.HasSupportAbilityByIndex((SupportAbility)1001) ? 1 : 2;
                else
                    v.Context.Evade = 0;
            }

            v.Target.PenaltyBanishHitRate();

            if (v.Target.IsUnderAnyStatus(TranceSeekStatus.PerfectDodge) && !v.Caster.HasSupportAbility(SupportAbility1.Healer)) // Perfect Dodge
            {
                v.Context.Flags |= BattleCalcFlags.Miss | BattleCalcFlags.Dodge;
                btl_stat.AlterStatus(v.Target, TranceSeekStatusId.PerfectDodge, parameters: "Remove");
                //if (v.Target.IsUnderAnyStatus(TranceSeekCustomStatus.PerfectDodge)) // Didn't work when Stack > 1.... ?
                //    btl2d.Btl2dReqSymbolMessage(v.Target.Data, "[FFFFFF]", Localization.Get("Miss"), HUDMessage.MessageStyle.DAMAGE, 0);

                return false;
            }

            foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(v.Caster.Data.saExtended))
                saFeature.TriggerOnAbility(v, "HitRateSetup", false);
            foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(v.Target.Data.saExtended))
                saFeature.TriggerOnAbility(v, "HitRateSetup", true);

            if (v.Caster.HasSupportAbility(SupportAbility1.Healer) && v.Target.IsPlayer) // SA Healer never miss on Player.
                v.Context.Evade = 0;

            if ((v.Context.HitRate <= Comn.random16() % 100) || v.Target.PhysicalEvade == 255 || v.Target.IsUnderAnyStatus(BattleStatus.Vanish))
            {
                v.Context.Flags |= BattleCalcFlags.Miss;
                return false;
            }

            if (v.Target.PhysicalDefence == 255 || (v.Target.IsUnderAnyStatus(TranceSeekStatus.Bulwark) && !v.Caster.HasSupportAbility(SupportAbility1.Healer))) // Bulwark
            {
                v.Target.RemoveStatus(TranceSeekStatus.Bulwark);
                v.Context.Flags |= BattleCalcFlags.Guard;
                return false;
            }
            if ((v.Target.Data == v.Caster.Data || (v.Context.Evade + (v.Target.PlayerIndex == CharacterId.Zidane ? ZidanePassive[v.Target.Data][0] : 0)) <= Comn.random16() % 100 || v.Context.Evade == 0))
            {
                if (v.Target.PlayerIndex == CharacterId.Zidane && v.Target.Data != v.Caster.Data && btl_util.getSerialNumber(v.Target.Data) == CharacterSerialNumber.ZIDANE_DAGGER && !v.Target.IsUnderAnyStatus(BattleStatusConst.BattleEndFull) && !v.Caster.HasSupportAbility(SupportAbility1.Healer))
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

        public static void ViviFocus(BattleCalculator v)
        {
            if (v.Command.Id != BattleCommandId.BlackMagic && v.Command.Id != BattleCommandId.DoubleBlackMagic && v.Command.Id != BattleCommandId.MagicSword && v.Command.Id != TranceSeekBattleCommand.Witchcraft)
                return;

            if (ViviPassive[v.Caster.Data][2] == 0)
            {
                ViviPassive[v.Caster.Data][2] = 1;
                Int32 counter = 25;
                v.Caster.AddDelayedModifier(
                    caster => (counter -= 1) > 0,
                    caster =>
                    {
                        ViviPassive[v.Caster.Data][1] = 0;
                        ViviPassive[v.Caster.Data][2] = 0;
                    }
                );
                if (v.Caster.PlayerIndex == CharacterId.Vivi)
                {
                    if ((v.Command.Id == BattleCommandId.BlackMagic || v.Command.Id == BattleCommandId.DoubleBlackMagic) || v.Command.Id == TranceSeekBattleCommand.Witchcraft)
                    {
                        if (ViviPassive[v.Caster.Data][1] == 0)
                        {
                            ViviPassive[v.Caster.Data][1] = (ushort)(v.Command.TargetCount);
                            if (FF9TextTool.ActionAbilityName(ViviPreviousSpell[v.Caster.Data]) != v.Command.AbilityName)
                            {
                                Int32 BonusFocusMax = 0;
                                switch (v.Caster.Weapon)
                                {
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

                            if (v.Caster.Weapon == (RegularItem)1163 && !v.Target.IsPlayer)
                            {
                                if (v.Command.TargetCount > 1)
                                    WhatIsThatScript.MultipleSteal(v, false);
                                else
                                    StealScript.ClassicSteal(v, false);
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
                else if (v.Caster.PlayerIndex == CharacterId.Steiner && v.Command.Id == BattleCommandId.MagicSword && v.Command.AbilityId != TranceSeekBattleAbility.MagicSword_Aero &&
                v.Command.AbilityId != TranceSeekBattleAbility.MagicSword_Aera && v.Command.AbilityId != TranceSeekBattleAbility.MagicSword_Aeraga && v.Command.AbilityId != TranceSeekBattleAbility.MagicSword_Holy)
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

        public static Boolean TryMagicHit(this BattleCalculator v)
        {
            ViviFocus(v);

            foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(v.Caster))
                saFeature.TriggerOnAbility(v, "HitRateSetup", false);
            foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(v.Target))
                saFeature.TriggerOnAbility(v, "HitRateSetup", true);

            if (v.Context.HitRate <= Comn.random16() % 100)
            {
                v.Context.Flags |= BattleCalcFlags.Miss;
                SPS_GuardStatus(v);
                return false;
            }

            if (v.Context.Evade > Comn.random16() % 100)
            {
                v.Context.Flags |= BattleCalcFlags.Miss;
                SPS_GuardStatus(v);
                return false;
            }

            return true;
        }

        public static Boolean TryMagicHitWithoutBattleCalcFlag(this BattleCalculator v)
        {
            ViviFocus(v);

            foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(v.Caster))
                saFeature.TriggerOnAbility(v, "HitRateSetup", false);
            foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(v.Target))
                saFeature.TriggerOnAbility(v, "HitRateSetup", true);

            if (v.Context.HitRate <= Comn.random16() % 100)
            {
                SPS_GuardStatus(v);
                return false;
            }

            if (v.Context.Evade > Comn.random16() % 100)
            {
                SPS_GuardStatus(v);
                return false;
            }

            return true;
        }

        public static void TryAlterMagicStatuses(this BattleCalculator v)
        {
            ViviFocus(v);

            foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(v.Caster))
                saFeature.TriggerOnAbility(v, "HitRateSetup", false);
            foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(v.Target))
                saFeature.TriggerOnAbility(v, "HitRateSetup", true);

            ReduceAccuracyEliteMonsters(v);

            if (v.Command.HitRate > Comn.random16() % 100)
            {
                v.Target.TryAlterStatuses(v.Command.AbilityStatus, false, v.Caster);
                AlterStatusDurationFromSA(v, v.Command.AbilityStatus);
            }

            SPS_GuardStatus(v);
        }

        public static void TargetPhysicalPenaltyAndBonusAttack(this BattleCalculator v)
        {
            if (v.Target.PhysicalDefence == 255)
            {
                v.Context.Flags |= BattleCalcFlags.Guard;
                return;
            }

            if (v.Target.IsUnderAnyStatus(BattleStatus.Defend))
            {
                v.Context.DamageModifierCount -= 2;
                SoundLib.PlaySoundEffect(356); //se050010
            }

            if (v.Target.PlayerIndex == CharacterId.Steiner && v.Target.IsUnderAnyStatus(BattleStatus.Trance)) // Steiner Trance => 25% reduce physical damage
                --v.Context.DamageModifierCount;
            if (v.Target.IsUnderAnyStatus(BattleStatus.Protect))
                v.Context.DamageModifierCount -= 2;
            if (v.Target.IsUnderAnyStatus(BattleStatus.Mini) || v.Target.IsUnderAnyStatus(BattleStatus.Sleep) && !v.Target.IsUnderAnyStatus(BattleStatus.EasyKill) || v.Target.IsUnderAnyStatus(BattleStatus.Freeze))
                ++v.Context.DamageModifierCount;

            if (StackBreakOrUpStatus[v.Caster.Data][0] != 0)
                v.Context.Attack += ((StackBreakOrUpStatus[v.Caster.Data][0] * v.Context.Attack) / 100);
            if (StackBreakOrUpStatus[v.Target.Data][2] != 0)
                v.Context.Attack -= ((StackBreakOrUpStatus[v.Target.Data][2] * v.Context.Attack) / 100);

            if (v.Target.Weapon == (RegularItem)1156 && v.Target.IsUnderAnyStatus(BattleStatus.Defend)) // Sea Spear
                v.Context.Attack = 1;

            if (v.Context.Attack < 1)
                v.Context.Attack = 1;
        }

        public static void CasterPhysicalPenaltyAndBonusAttack(BattleCalculator v)
        {
            if (v.Caster.IsUnderAnyStatus(BattleStatus.Mini))
                v.Context.DecreaseAttackDrastically();
            if (v.Caster.IsUnderAnyStatus(BattleStatus.Berserk) || v.Caster.IsPlayer && v.Caster.IsUnderAnyStatus(BattleStatus.Trance))
                ++v.Context.DamageModifierCount;
        }

        public static void EnemyTranceBonusAttack(BattleCalculator v)
        {
            if (!v.Caster.IsPlayer && v.Caster.IsUnderAnyStatus(BattleStatus.Trance))
                ++v.Context.DamageModifierCount;
        }

        public static void BonusBackstabAndPenaltyLongDistance(this BattleCalculator v)
        {
            if ((Math.Abs(v.Caster.Data.evt.rotBattle.eulerAngles.y - v.Target.Data.evt.rotBattle.eulerAngles.y) < 0.1) || v.Target.IsRunningAway())
                ++v.Context.DamageModifierCount;

            if (Mathf.Abs(v.Caster.Row - v.Target.Row) > 1 && !v.Caster.HasLongRangeWeapon && v.Command.IsShortRange && v.Caster.IsPlayer)
            {
                v.Context.DamageModifierCount -= 2;
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
                    v.Context.DamageModifierCount -= 2;
            }
        }

        public static void MagicAccuracy(this BattleCalculator v)
        {
            v.Context.HitRate = (Int16)(v.Command.HitRate + (v.Caster.Magic >> 2) + v.Caster.Level - v.Target.Level);

            //if (v.Context.HitRate > 100)
            //    v.Context.HitRate = 100;

            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)230))
                AmarantPassive(v);

            ReduceAccuracyEliteMonsters(v);

            if (v.Context.HitRate < 1)
                v.Context.HitRate = 1;

            v.Context.Evade = v.Target.MagicEvade;

            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)1200) && v.Caster.PlayerIndex == CharacterId.Zidane) // SA Knavery+
                v.Context.Evade += ZidanePassive[v.Caster.Data][0];
        }

        public static void ReduceAccuracyEliteMonsters(this BattleCalculator v, Boolean MalusForced = false)
        {
            BattleStatus LethalStatus = BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Death | BattleStatus.Mini | BattleStatus.Heat |
                                        BattleStatus.Freeze | BattleStatus.Zombie | BattleStatus.Stop | TranceSeekStatus.Vieillissement;
            if (EliteMonster(v.Target.Data))
            {            
                if ((v.Command.AbilityStatus & LethalStatus) != 0 || MalusForced)
                {
                    v.Context.HitRate /= 2;
                    v.Command.HitRate /= 2;
                }
            }
            else if (v.Target.IsUnderAnyStatus(BattleStatus.EasyKill)) // Security for special cases... maybe useless.
            {
                if ((v.Command.AbilityStatus & BattleStatus.Death) != 0)
                {
                    v.Command.AbilityStatus &= ~BattleStatus.Death;
                    TriggerSPSResistStatus[v.Target] = true;
                }
            }

        }

        public static void PenaltyShellAttack(this BattleCalculator v)
        {
            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)1257)) // SA Mania+ [TODO] Need a new function for that ? Like HitRateBonus()
            {
                if (FF9StateSystem.EventState.gScriptDictionary.TryGetValue(1030, out Dictionary<Int32, Int32> dict))
                {
                    int MpCost = FF9StateSystem.Battle.FF9Battle.aa_data[v.Command.AbilityId].MP;
                    int IdAA = (int)v.Command.AbilityId;
                    if (IdAA % 2 == 0)
                        MpCost *= Math.Max(1, (dict[IdAA] + dict[IdAA + 1] - 2));
                    else
                        MpCost *= Math.Max(1, (dict[IdAA] + dict[IdAA - 1] - 2));

                    v.Context.HitRate += (v.Context.HitRate * MpCost) / 100;
                }
            }

            if (v.Target.MagicDefence == 255)
            {
                v.Context.Attack = 0;
                v.Context.Flags |= BattleCalcFlags.Guard;
                return;
            }

            if (v.Target.IsUnderAnyStatus(BattleStatus.Shell))
                v.Context.Attack >>= 1;

            if (v.Caster.IsUnderAnyStatus(BattleStatus.CustomStatus18)) // Silence Easy Kill - 25% magic attack malus for bosses with Silence.
                v.Context.DamageModifierCount--;

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
                v.Context.DamageModifierCount -= 2;
        }

        public static void CasterPenaltyMini(this BattleCalculator v)
        {
            if (v.Caster.IsUnderAnyStatus(BattleStatus.Mini))
                v.Context.DamageModifierCount -= 2;
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
                ++v.Context.DamageModifierCount;

            if ((InfusedWeaponScript.WeaponNewElement[v.Caster.Data] & v.Caster.BonusElement) != 0)
                ++v.Context.DamageModifierCount;
        }

        public static void BonusElement(this BattleCalculator v)
        {
            if ((v.Command.ElementForBonus & v.Caster.BonusElement) != 0)
                v.Context.DamageModifierCount += 2;
        }

        public static Boolean CanAttackWeaponElementalCommand(this BattleCalculator v)
        {
            EffectElement WeaponElement = v.Caster.WeaponElement;

            if (InfusedWeaponScript.WeaponNewElement[v.Caster.Data] != 0)
                WeaponElement |= InfusedWeaponScript.WeaponNewElement[v.Caster.Data];

            CanAttackElement(v, WeaponElement);

            if (InfusedWeaponScript.WeaponNewElement[v.Caster.Data] != 0)
            {
                if (v.Caster.PlayerIndex == (CharacterId)12 & v.Target.IsWeakElement(WeaponElement)) // SA Maximum infusion
                {
                    BattleAbilityId InfusedAA = ViviPreviousSpell[v.Caster.Data];
                    if (InfusedAA == (BattleAbilityId)1091 || InfusedAA == (BattleAbilityId)1092 || InfusedAA == (BattleAbilityId)1093 || InfusedAA == (BattleAbilityId)1094)
                    {
                        v.Context.DamageModifierCount++;
                    }
                    if (InfusedAA == (BattleAbilityId)1095 || InfusedAA == (BattleAbilityId)1096 || InfusedAA == (BattleAbilityId)1097 || InfusedAA == (BattleAbilityId)1098)
                    {
                        v.Context.DamageModifierCount += 2;
                    }
                }
            }

            if (((InfusedWeaponScript.WeaponNewCustomElement[v.Caster.Data] & 1) != 0) && v.Target.HasCategory(EnemyCategory.Humanoid)) // Poison
                v.Context.DamageModifierCount++;

            if (((InfusedWeaponScript.WeaponNewCustomElement[v.Caster.Data] & 2) != 0) && v.Target.HasCategory(EnemyCategory.Stone)) // Gravity
                v.Context.DamageModifierCount++;

            return true;
        }

        public static Boolean CanAttackMagic(this BattleCalculator v)
        {
            ViviFocus(v);

            if ((v.Context.Flags & TranceSeekBattleCalcFlags.PropagationFail) != 0)
            {
                v.Context.Flags |= BattleCalcFlags.Miss;
                return false;
            }
            if (v.Target.IsUnderAnyStatus(TranceSeekStatus.Runic))
            {
                v.CalcHpDamage();
                v.Target.Flags = (CalcFlag.HpDamageOrHeal | CalcFlag.MpDamageOrHeal);
                v.Target.MpDamage = Math.Max(1, v.Target.HpDamage / 40);
                v.Target.HpDamage = Math.Max(1, v.Target.HpDamage / 2);
                v.Target.AlterStatus(TranceSeekStatus.Rage, v.Caster);
                v.Command.AbilityStatus = 0;
                return false;
            }

            CanAttackElement(v);
         
            return true;
        }

        public static Boolean CanAttackElement(this BattleCalculator v, EffectElement Element = 0)
        {
            if (Element == 0)
                Element = v.Command.Element;

            if (v.Target.IsLevitate && v.Command.IsGround)
            {
                v.Context.Flags |= BattleCalcFlags.Miss;
                return false;
            }

            if (v.Target.IsGuardElement(Element) || ((v.Command.ScriptId == 118 || v.Command.ScriptId == 119) && (NewEffectElement[v.Target.Data][0] & 4) != 0) || ((v.Command.ScriptId == 17 || v.Command.ScriptId == 86) && (NewEffectElement[v.Target.Data][1] & 4) != 0))
            {
                v.Context.Flags |= BattleCalcFlags.Guard;
                return false;
            }

            if (v.Target.IsHalfElement(Element) || ((v.Command.ScriptId == 118 || v.Command.ScriptId == 119) && (NewEffectElement[v.Target.Data][0] & 2) != 0) || ((v.Command.ScriptId == 17 || v.Command.ScriptId == 86) && (NewEffectElement[v.Target.Data][1] & 2) != 0))
                v.Context.DamageModifierCount -= 2;

            if (v.Target.IsWeakElement(Element) || ((v.Command.ScriptId == 118 || v.Command.ScriptId == 119) && (NewEffectElement[v.Target.Data][0] & 1) != 0) || ((v.Command.ScriptId == 17 || v.Command.ScriptId == 86) && (NewEffectElement[v.Target.Data][1] & 1) != 0))
                v.Context.DamageModifierCount += 2;

            if (v.Target.CanAbsorbElement(Element))
            {
                if (v.Target.HasSupportAbilityByIndex((SupportAbility)241) && (Element & EffectElement.Darkness) != 0) // SA Dark side
                {
                    v.Context.DefensePower = 0;
                    if (v.Target.HasSupportAbilityByIndex((SupportAbility)1241))
                        IncreaseTrance(v.Caster.Data, Comn.random16() % (v.Target.Will / 2));
                }
            }
            if (AbsorbElement.TryGetValue(v.Target.Data, out Int32 elementprotect))
                if ((Element & (EffectElement)elementprotect) != 0 && elementprotect != -1)
                    v.Context.Flags |= BattleCalcFlags.Absorb;
            else if (((v.Command.ScriptId == 118 || v.Command.ScriptId == 119) && (NewEffectElement[v.Target.Data][0] & 8) != 0) || ((v.Command.ScriptId == 17 || v.Command.ScriptId == 86) && (NewEffectElement[v.Target.Data][1] & 8) != 0))
                v.Context.Flags |= BattleCalcFlags.Absorb;

            v.Target.AlterStatuses(Element);
            return true;
        }

        public static void InfusedWeaponStatus(this BattleCalculator v)
        {
            if (InfusedWeaponScript.WeaponNewStatus[v.Caster.Data] != 0 && InfusedWeaponScript.WeaponNewStatus[v.Caster.Data] != BattleStatus.Protect && InfusedWeaponScript.WeaponNewStatus[v.Caster.Data] != BattleStatus.Shell)
            {
                foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(v.Caster))
                    saFeature.TriggerOnAbility(v, "HitRateSetup", false);
                foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(v.Target))
                    saFeature.TriggerOnAbility(v, "HitRateSetup", true);

                if (v.Caster.IsPlayer)
                {
                    if (v.Caster.WeaponRate > Comn.random16() % 100)
                        v.Command.AbilityStatus |= InfusedWeaponScript.WeaponNewStatus[v.Caster.Data];
                }
                else
                    v.Command.AbilityStatus |= InfusedWeaponScript.WeaponNewStatus[v.Caster.Data];

                if ((v.Target.ResistStatus & InfusedWeaponScript.WeaponNewStatus[v.Caster.Data]) != 0 && !v.Target.IsPlayer)
                    TriggerSPSResistStatus[v.Target] = true;
            }
        } 

        public static void AmarantPassive(this BattleCalculator v)
        {
            Int32 factor = 0;
            List<BattleStatusId> statuschoosen = new List<BattleStatusId>{ BattleStatusId.Poison, BattleStatusId.Venom, BattleStatusId.Blind, BattleStatusId.Silence, BattleStatusId.Trouble,
                    BattleStatusId.Sleep, BattleStatusId.Freeze, BattleStatusId.Heat, BattleStatusId.Doom, BattleStatusId.Mini, BattleStatusId.Petrify, BattleStatusId.GradualPetrify,
                    BattleStatusId.Berserk, BattleStatusId.Confuse, BattleStatusId.Stop, BattleStatusId.Zombie, BattleStatusId.Slow, TranceSeekStatusId.Vieillissement,
                    TranceSeekStatusId.ArmorBreak, TranceSeekStatusId.MagicBreak, TranceSeekStatusId.MentalBreak, TranceSeekStatusId.PowerBreak};

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
                if (v.Caster.HasSupportAbilityByIndex((SupportAbility)230)) // Venefic
                {
                    if (v.Caster.HasSupportAbilityByIndex((SupportAbility)1230))
                        v.Context.Attack += (v.Context.Attack * factor * bonus) / 100;

                    v.Context.HitRate += (v.Context.HitRate * factor * bonus) / 100;
                }
                else
                    v.Context.Attack += (v.Context.Attack * factor * bonus) / 100;
            }
        }

        public static void DragonMechanic(this BattleCalculator v)
        {
            if (v.Caster.PlayerIndex == CharacterId.Freya)
            {
                if (v.Command.AbilityId == BattleAbilityId.Luna) // Luna effect handle in 0079_DragonSkillScript.cs
                    return;

                if (v.Target.IsUnderAnyStatus(TranceSeekStatus.Dragon) && !v.Caster.IsUnderStatus(BattleStatus.Trance) && v.Command.Id == BattleCommandId.DragonAct)
                {
                    float DragonRemove = v.Caster.HasSupportAbilityByIndex((SupportAbility)1122) ? 25 : (v.Caster.HasSupportAbilityByIndex((SupportAbility)122) ? 12.5f : 0); // Eye of the dragon
                    if (DragonRemove < Comn.random16() % 100)
                        btl_stat.AlterStatus(v.Target, TranceSeekStatusId.Dragon, v.Caster, parameters: "Remove");
                }
                else if (v.Command.Id == BattleCommandId.Attack || v.Command.Id == BattleCommandId.Spear || v.Command.Id == BattleCommandId.SpearInTrance || (v.Command.Id == BattleCommandId.DragonAct && !v.Target.IsUnderAnyStatus(TranceSeekStatus.Dragon)))
                {
                    TryApplyDragon(v);
                }
            }
        }
        public static void CharacterBonusPassive(this BattleCalculator v, string mode = "") // [TODO] Rename + delete this from most functions (deprecaticed old Beatrix passive)
        {
            if (v.Caster.PlayerIndex == CharacterId.Marcus)
            {
                if (mode == "MagicAttack")
                {
                    v.Context.Attack = (Int16)(v.Caster.Strength + Comn.random16() % (1 + v.Caster.Level));
                }
            }
        }

        public static void TryAlterCommandStatuses(this BattleCalculator v, Boolean ChangeContext = true)
        {
            if (v.Command.AbilityStatus != 0 && (v.Context.Flags & TranceSeekBattleCalcFlags.PropagationFail) == 0)
            {
                v.Target.TryAlterStatuses(v.Command.AbilityStatus, ChangeContext, v.Caster);
                AlterStatusDurationFromSA(v, v.Command.AbilityStatus);
            }

            SPS_GuardStatus(v);
        }

        public static void TryRemoveAbilityStatuses(this BattleCalculator v)
        {
            if ((v.Context.Flags & TranceSeekBattleCalcFlags.PropagationFail) != 0)
            {
                v.Context.Flags |= BattleCalcFlags.Miss;
                return;
            }

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

        public static Boolean CheckUnsafetyOrGuard(this BattleCalculator v)
        {
            if (!v.Target.IsUnderAnyStatus(BattleStatus.EasyKill) || EliteMonster(v.Target.Data))
                return true;

            v.Context.Flags |= BattleCalcFlags.Guard;
            TriggerSPSResistStatus[v.Target] = true;
            SPS_GuardStatus(v);
            return false;
        }

        public static void RaiseTrouble(this BattleCalculator v)
        {
            if (v.Target.PhysicalDefence != 255 || v.Target.PhysicalDefence != 255 || v.Target.MagicDefence != 255 || v.Target.MagicEvade != 255 && !v.Command.IsManyTarget)
                v.RaiseTrouble();
        }

        public static void ChangeRow(BattleUnit unit)
        {
            btl_para.SwitchPlayerRow(unit.Data);
            if (unit.Row == 1)
                btl_stat.AlterStatus(unit, TranceSeekStatusId.Special, parameters: "CanCover1");
            else
                btl_stat.AlterStatus(unit, TranceSeekStatusId.Special, parameters: "CanCover0");
        }

        public static void SA_StatusApply(BattleUnit inflicter, Boolean StatusIsPositive)
        {
            if (inflicter != null)
            {
                if (inflicter.HasSupportAbilityByIndex((SupportAbility)128) && inflicter.CurrentMp < inflicter.MaximumMp) // SA Strategist
                {
                    int factor = inflicter.HasSupportAbilityByIndex((SupportAbility)1128) ? 2 : 1;
                    inflicter.CurrentMp = (uint)Math.Min(inflicter.CurrentMp + factor * (inflicter.MaximumMp / 100), inflicter.MaximumMp);
                }
                if (inflicter.HasSupportAbilityByIndex((SupportAbility)131) && !inflicter.InTrance && inflicter.Trance < byte.MaxValue && StatusIsPositive) // SA Altruistic
                    IncreaseTrance(inflicter.Data, inflicter.Will / 10);

                if (inflicter.HasSupportAbilityByIndex((SupportAbility)1258) && inflicter.CurrentMp < inflicter.MaximumMp) // SA Reward+
                    inflicter.CurrentMp = Math.Min(inflicter.CurrentMp + (inflicter.MaximumMp / 25), inflicter.MaximumMp);
            }
        }

        public static void SPS_GuardStatus(this BattleCalculator v)
        {
            //if (v.Command.Id == BattleCommandId.Attack && v.Caster.PlayerIndex == CharacterId.Zidane && (v.Context.Flags & BattleCalcFlags.Miss) != 0) // Don't trigger on miss and second hit dagger from Zidane
            //return;

            Boolean CommandAttack = v.Command.Id == BattleCommandId.Attack || (v.Command.Id == TranceSeekBattleCommand.MagicWeapon_Weak || v.Command.Id == TranceSeekBattleCommand.MagicWeapon_Normal || v.Command.Id == TranceSeekBattleCommand.MagicWeapon_Strong) && v.Command.Data.info.effect_counter == 1;
            if ((((v.Target.ResistStatus & v.Command.AbilityStatus) != 0 || ((v.Target.ResistStatus & v.Caster.WeaponStatus) != 0 && v.Caster.HasSupportAbility(SupportAbility1.AddStatus) && CommandAttack)) && !v.Target.IsPlayer) || TriggerSPSResistStatus[v.Target]) // SPS immune status.
            {
                SPSEffect sps = HonoluluBattleMain.battleSPS.AddSequenceSPS(13, -1, 1);
                TriggerSPSResistStatus[v.Target] = false;
                if (sps == null)
                    return;
                btl2d.GetIconPosition(v.Target, btl2d.ICON_POS_ROOT, out Transform attachTransf, out Vector3 iconOff);
                sps.charTran = v.Target.Data.gameObject.transform;
                sps.boneTran = attachTransf;
                sps.posOffset = Vector3.zero;
                //sps.scale *= 1;
                SoundLib.PlaySoundEffect(1314); // se000046, se060146, se070003
                if (v.Target.HpDamage == 0 && v.Target.MpDamage == 0 && (v.Command.AbilityStatus - (v.Target.ResistStatus & v.Command.AbilityStatus)) == 0)
                {
                    v.Context.Flags = 0;
                    Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                    {
                        { "US", "Immune!" },
                        { "UK", "Immune!" },
                        { "JP", "免疫！" },
                        { "ES", "¡Inmune!" },
                        { "FR", "Immunisé !" },
                        { "GR", "Immun!" },
                        { "IT", "Immunità!" },
                    };
                    btl2d.Btl2dReqSymbolMessage(v.Target.Data, "[FF00FF]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 10);
                }
            }
        }

        public static void PhantomHandSA(this BattleCalculator v)
        {
            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)202)) // SA Phantom hand
            {
                v.Caster.Flags |= CalcFlag.MpDamageOrHeal;
                v.Caster.MpDamage = (int)(v.Caster.MaximumMp / UnityEngine.Random.Range(5, 20));
                if (v.Caster.HasSupportAbilityByIndex((SupportAbility)1202) && !v.Caster.InTrance)
                    IncreaseTrance(v.Caster.Data, Comn.random16() % v.Caster.Will);
            }
        }

        public static void IncreaseTrance(BTL_DATA btl, int trance_added, int MaxValueTrance = Byte.MaxValue)
        {
            BattleUnit unit = new BattleUnit(btl);
            btl.trance = (byte)Math.Min(btl.trance + trance_added, MaxValueTrance);
            if (btl.trance >= Byte.MaxValue)
                btl_stat.AlterStatus(unit, BattleStatusId.Trance);
        }

        public static void SpecialItems(this BattleCalculator v)
        {
            if (ff9item._FF9Item_Data[v.Caster.Weapon].shape == 56 && v.Target.HpDamage > 0 && v.Command.Id != BattleCommandId.Item && v.Command.Id != BattleCommandId.AutoPotion) // Axe
            {
                v.Target.HpDamage = UnityEngine.Random.Range(v.Target.HpDamage / 10, v.Target.HpDamage);
            }

            switch (v.Target.Accessory)
            {
                case TranceSeekRegularItem.EmergencySatchel:
                {
                    Boolean TargetPreventStatus = v.Target.IsUnderAnyStatus(BattleStatusConst.PreventCounter | BattleStatus.Heat | BattleStatus.Zombie);
                    int PotionHeal = v.Target.HasSupportAbility(SupportAbility1.Chemist) ? 400 : 200;
                    if (!TargetPreventStatus && v.Caster.IsPlayer != v.Target.IsPlayer && SpecialItemEffect[v.Target.Data][0] > 0 && (v.Target.MaximumHp - v.Target.CurrentHp + v.Target.HpDamage) > PotionHeal && v.Command.Id <= BattleCommandId.BoundaryCheck)
                    {
                        btl_cmd.SetCounter(v.Target.Data, BattleCommandId.AutoPotion, (int)RegularItem.Potion, v.Target.Id);
                        SpecialItemEffect[v.Target.Data][0]--;
                    }
                    break;
                }
                case TranceSeekRegularItem.MagicalSatchel:
                {
                    Boolean TargetPreventStatus = v.Target.IsUnderAnyStatus(BattleStatusConst.PreventCounter | BattleStatus.Heat);
                    if (!TargetPreventStatus && v.Caster.IsPlayer != v.Target.IsPlayer && SpecialItemEffect[v.Target.Data][1] > 0 && v.Target.IsUnderAnyStatus(BattleStatusConst.AnyNegative))
                    {
                        RegularItem PotionChoosen = RegularItem.NoItem;
                        if (v.Target.IsUnderAnyStatus(BattleStatus.GradualPetrify))
                            PotionChoosen = RegularItem.Soft;
                        else if (v.Target.IsUnderAnyStatus(BattleStatus.Poison | BattleStatus.Venom))
                            PotionChoosen = RegularItem.Antidote;
                        else if (v.Target.IsUnderAnyStatus(BattleStatus.Zombie))
                            PotionChoosen = RegularItem.MagicTag;
                        else if (v.Target.IsUnderAnyStatus(BattleStatus.Silence))
                            PotionChoosen = RegularItem.EchoScreen;
                        else if (v.Target.IsUnderAnyStatus(BattleStatus.Virus))
                            PotionChoosen = RegularItem.Vaccine;
                        else if (v.Target.IsUnderAnyStatus(BattleStatus.Blind))
                            PotionChoosen = RegularItem.EyeDrops;
                        else if (v.Target.IsUnderAnyStatus(BattleStatus.Trouble | TranceSeekStatus.Vieillissement))
                            PotionChoosen = RegularItem.Annoyntment;

                        if (PotionChoosen != RegularItem.NoItem)
                        {
                            ff9item.FF9Item_Add(PotionChoosen, 1);
                            btl_cmd.SetCounter(v.Target.Data, BattleCommandId.AutoPotion, (int)PotionChoosen, v.Target.Id);
                            SpecialItemEffect[v.Target.Data][1]--;
                        }
                    }
                    break;
                }
            }
        }

        public static void AlterStatusDuration(this BattleCalculator v, BattleStatus targetstatus, int NewFormula = 0, Boolean Add = true)
        {
            int Formula = 400 + v.Caster.Will * 2 - v.Target.Will;
            if (Formula != 0)
                Formula = NewFormula;

            foreach (BattleStatusId statusId in targetstatus.ToStatusList())
            {
                BattleStatusDataEntry statusData = FF9StateSystem.Battle.FF9Battle.status_data[statusId];
                if (Add)
                    v.Target.Data.stat.conti[statusId] += (Int16)((statusData.ContiCnt * Formula) * v.Target.Data.stat.duration_factor[statusId]);
                else
                    v.Target.Data.stat.conti[statusId] -= (Int16)((statusData.ContiCnt * Formula) * v.Target.Data.stat.duration_factor[statusId]);
            }
        }

        public static void AlterStatusDurationFromSA(this BattleCalculator v, BattleStatus cmd_status)
        {
            foreach (BattleStatusId statusId in (BattleStatusConst.ContiCount & v.Command.AbilityStatus).ToStatusList())
            {
                BattleStatus status = statusId.ToBattleStatus();
                BattleStatusDataEntry statusData = FF9StateSystem.Battle.FF9Battle.status_data[statusId];

                Boolean PositiveStatus = (BattleStatusConst.AnyPositive & status) != 0;
                Boolean NegativeStatus = (BattleStatusConst.AnyNegative & status) != 0;

                int durationfactor = NegativeStatus ? (400 + v.Caster.Will * 2 - v.Target.Will) : (PositiveStatus ? (400 + v.Caster.Will * 3) : 200);

                if (PositiveStatus)
                {
                    if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.Blessing_Boosted))
                        durationfactor *= 2;
                    else if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.Blessing))
                        durationfactor = (3 * durationfactor) / 2;

                    if (v.Caster.PlayerIndex == CharacterId.Garnet && (v.Caster.Accessory == RegularItem.Ruby || v.Caster.InTrance))
                        durationfactor = durationfactor * (1 + ((ff9item.FF9Item_GetCount(RegularItem.Ruby) + 1) / 200));
                }
                if (NegativeStatus)
                {
                    if (v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Persistence_Boosted))
                        durationfactor = (3 * durationfactor) / 2;
                    else if (v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Persistence))
                        durationfactor = (5 * durationfactor) / 4;
                }

                if (v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Propagation) && !v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Propagation_Boosted)
                    && v.Command.IsManyTarget && v.Command.AbilityId >= (BattleAbilityId)1499 && v.Command.AbilityId <= (BattleAbilityId)1526)
                    durationfactor /= 2;

                v.Target.Data.stat.conti[statusId] = (Int16)((statusData.ContiCnt * durationfactor) * v.Target.Data.stat.duration_factor[statusId]);
            }
        }

        public static void ResetSteinerPassive(BattleUnit unit)
        {
            FF9TextTool.SetCommandName(BattleCommandId.SwordAct, TranceSeekBattleCommand.SwdArtCMDNameVanilla[Localization.CurrentDisplaySymbol]);
            unit.UILabelHP = unit.CurrentHp.ToString();

            unit.AddDelayedModifier(
            caster => caster.CurrentAtb >= caster.MaximumAtb,
            caster =>
            {
                SteinerPassive[unit.Data][1] = 0;
                if (FF9StateSystem.EventState.gScriptDictionary.TryGetValue(1000, out Dictionary<Int32, Int32> dictbattle)) 
                    dictbattle[1] = 0;
            }
            );
        }

        public static void UpdateRedemptionHUD(BattleUnit unit)
        {
            int RedemptionStack = (int)unit.GetPropertyByName("StatusProperty CustomStatus12 Stack");
            if (RedemptionStack > 0 && BeatrixPassive[unit.Data][0] != RedemptionStack)
            {
                BeatrixPassive[unit.Data][0] = RedemptionStack;
                FF9TextTool.SetCommandName(BattleCommandId.HolySword1, TranceSeekBattleCommand.SeikenCMDNameVanilla[Localization.CurrentDisplaySymbol] + " (" + RedemptionStack + " [SPRT=IconAtlas,item200_01,40,40] )");
                FF9TextTool.SetCommandName(BattleCommandId.HolySword2, TranceSeekBattleCommand.SeikenPlusCMDNameVanilla[Localization.CurrentDisplaySymbol] + " (" + RedemptionStack + " [SPRT=IconAtlas,item200_01,40,40] )");
                Dictionary<String, String> BeatrixPassiveMessage = new Dictionary<String, String>
                    {
                        { "US", "[SPRT=IconAtlas,item200_01] Redemption!" },
                        { "UK", "[SPRT=IconAtlas,item200_01] Redemption!" },
                        { "JP", "[SPRT=IconAtlas,item200_01] Redemption!" },
                        { "ES", "[SPRT=IconAtlas,item200_01] Redemption!" },
                        { "FR", "[SPRT=IconAtlas,item200_01] Redemption !" },
                        { "GR", "[SPRT=IconAtlas,item200_01] Redemption!" },
                        { "IT", "[SPRT=IconAtlas,item200_01] Redemption!" },
                    };
                btl2d.Btl2dReqSymbolMessage(unit.Data, "[FFFFFF]", BeatrixPassiveMessage, HUDMessage.MessageStyle.DAMAGE, 30);
            }
            else if (RedemptionStack == 0)
            {
                BeatrixPassive[unit.Data][0] = RedemptionStack;
                FF9TextTool.SetCommandName(BattleCommandId.HolySword1, TranceSeekBattleCommand.SeikenCMDNameVanilla[Localization.CurrentDisplaySymbol]);
                FF9TextTool.SetCommandName(BattleCommandId.HolySword2, TranceSeekBattleCommand.SeikenPlusCMDNameVanilla[Localization.CurrentDisplaySymbol]);
            }

        }

        public static void SpecialEffect(this BattleCalculator v)
        {
            if (v.Target.HpDamage > 0 && v.Target.IsUnderAnyStatus(TranceSeekStatus.MechanicalArmor) && MonsterMechanic[v.Target.Data][1] > 0 && (v.Target.Flags & CalcFlag.HpRecovery) == 0) // Armor Mechanical
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
                    v.Target.RemoveStatus(TranceSeekStatusId.MechanicalArmor);
                }
                else
                    v.Target.TryAlterSingleStatus(TranceSeekStatusId.MechanicalArmor, true, v.Caster, MonsterMechanic[v.Target.Data][1]);
            }
            if (v.Context.IsAbsorb)
            {
                v.Target.Flags |= CalcFlag.HpDamageOrHeal;

            }
            if (v.Target.HasSupportAbilityByIndex(SupportAbility.Counter) && v.Target.HasSupportAbilityByIndex((SupportAbility)234) &&
                (int)v.Target.GetPropertyByName("StatusProperty CustomStatus12 Stack") >= 2 && v.Target.Will < Comn.random16() % 100 && !v.Caster.IsPlayer) // SA Dominance
            {
                List<BattleAbilityId> Counter_AA = new List<BattleAbilityId>{ BattleAbilityId.ThunderSlash, BattleAbilityId.StockBreak, BattleAbilityId.Climhazzard, BattleAbilityId.Shock,
                BattleAbilityId.Protect, BattleAbilityId.Shell, BattleAbilityId.Cura, BattleAbilityId.Berserk, BattleAbilityId.Reflect, BattleAbilityId.Regen};

                int RedemptionStack = (int)v.Target.GetPropertyByName("StatusProperty CustomStatus12 Stack");
               
                for (Int32 i = 0; i < Counter_AA.Count; i++)
                {
                    if (!ff9abil.FF9Abil_IsMaster(v.Target.Player, (int)Counter_AA[i]))
                    {
                        int MPCost = FF9StateSystem.Battle.FF9Battle.aa_data[Counter_AA[i]].MP;
                        if (Counter_AA[i] == BattleAbilityId.ThunderSlash || Counter_AA[i] == BattleAbilityId.StockBreak || Counter_AA[i] == BattleAbilityId.Climhazzard || Counter_AA[i] == BattleAbilityId.Shock)
                            MPCost = ((4 - RedemptionStack) * (FF9StateSystem.Battle.FF9Battle.aa_data[Counter_AA[i]].MP)) / 4;

                        if (MPCost > v.Target.CurrentMp)
                            Counter_AA.Remove(Counter_AA[i]);

                        if (v.Target.IsUnderAnyStatus(BattleStatus.Reflect) && (Counter_AA[i] == BattleAbilityId.Protect || Counter_AA[i] == BattleAbilityId.Shell
                            || Counter_AA[i] == BattleAbilityId.Cura || Counter_AA[i] == BattleAbilityId.Reflect || Counter_AA[i] == BattleAbilityId.Regen)) // [TODO] To improve, don't work as intended...
                            Counter_AA.Remove(Counter_AA[i]);
                    }
                }

                if (Counter_AA.Count > 0)
                {
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
                else
                {
                    btl_cmd.SetCounter(v.Target, BattleCommandId.Counter, (int)BattleAbilityId.Attack, v.Caster.Id);
                }

            }
            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)110) && !v.Command.IsManyTarget && v.Command.Id != BattleCommandId.Attack && v.Target.HpDamage > 0 && 
                (v.Command.ScriptId == 9 || v.Command.ScriptId == 10 || v.Command.ScriptId == 17 || v.Command.ScriptId == 18 || v.Command.ScriptId == 116 || v.Command.ScriptId == 118
                || v.Command.AbilityId == BattleAbilityId.PumpkinHead || v.Command.AbilityId == BattleAbilityId.ThousandNeedles || v.Command.AbilityId == BattleAbilityId.GoblinPunch
                || v.Command.AbilityId == BattleAbilityId.AutoLife)) // Prolifération
            {
                int basedamage = v.Target.HpDamage;
                BTL_DATA targetdefault = v.Target.Data;
                Boolean Healing = (v.Target.Flags & CalcFlag.HpRecovery) != 0;
                foreach (BattleUnit unit in BattleState.EnumerateUnits())
                {
                    if ((unit.IsPlayer && !v.Target.IsPlayer || !unit.IsPlayer && v.Target.IsPlayer) || unit.MagicDefence == 255 || unit.PhysicalEvade == 255 || unit.Data == targetdefault)
                        continue;

                    if (unit.Data != targetdefault) 
                    {
                        if (v.Caster.HasSupportAbilityByIndex((SupportAbility)1110))
                            basedamage = v.Target.HpDamage / 2;
                        else
                            basedamage = v.Target.HpDamage / 4;

                        btl2d.Btl2dStatReq(unit, Healing ? -basedamage : basedamage, 0);

                        if (Healing)
                            unit.CurrentHp = (uint)Math.Min(unit.CurrentHp + basedamage, unit.MaximumHp);
                        else
                        {
                            if (unit.CurrentHp > basedamage)
                                unit.CurrentHp -= (uint)basedamage;
                            else
                                unit.Kill(v.Caster);
                        }
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
                            unit.CurrentMp = (uint)(unit.CurrentMp - (v.Command.Data.aa.MP / (v.Caster.HasSupportAbilityByIndex((SupportAbility)1119) ? 2 : 4)));
                        else
                            unit.CurrentMp = 0;
                    }
                }
                v.Caster.CurrentMp = (uint)(v.Caster.CurrentMp + (v.Command.Data.aa.MP / (v.Caster.HasSupportAbilityByIndex((SupportAbility)1119) ? 2 : 4)));
            }

            if (v.Caster.HasSupportAbility(SupportAbility1.ReflectNull) && v.Target.IsUnderAnyStatus(BattleStatus.Reflect) && !v.Caster.HasSupportAbilityByIndex((SupportAbility)1030))
                v.Target.HpDamage >>= 1;

            if (v.Command.Id == (BattleCommandId)1032) // SA Witchcraft
            {
                v.Target.HpDamage /= 2;
                v.Target.MpDamage /= 2;
            }

            if (v.Caster.PlayerIndex == CharacterId.Zidane && (v.Command.Id == BattleCommandId.Attack || v.Command.Id == BattleCommandId.Counter) && ff9item._FF9Item_Data[v.Caster.Weapon].shape == 1)
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
                if (ff9item._FF9Item_Data[v.Caster.Weapon].shape == 1)
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
            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)258) && (v.Target.WeakElement & v.Command.Element) != 0) // Reward
            {
                HealMP += (int)(v.Caster.MaximumMp / 25);
            }
            if (InfusedWeaponScript.WeaponNewStatus[v.Caster.Data] == BattleStatus.Protect && (v.Command.Id == BattleCommandId.Attack || v.Command.Id == BattleCommandId.Counter)) // Drain MagiLame
            {
                HealHP += v.Target.HpDamage / 4;
            }
            if (InfusedWeaponScript.WeaponNewStatus[v.Caster.Data] == BattleStatus.Shell && (v.Command.Id == BattleCommandId.Attack || v.Command.Id == BattleCommandId.Counter)) // Osmose MagiLame
            {
                HealMP += v.Target.MpDamage / 80;
            }
            if (v.Target.HasSupportAbilityByIndex((SupportAbility)118) && v.Target.IsCovering) // Flawless
            {
                int TargetHealMP = (int)(v.Target.HasSupportAbilityByIndex((SupportAbility)1118) ? (v.Target.MaximumMp / 25) : (v.Target.MaximumMp / 50));
                if (TargetHealMP > 0)
                {
                    v.Target.CurrentMp = Math.Min(v.Target.CurrentMp + (uint)TargetHealMP, v.Target.MaximumMp);
                }
                btl2d.Btl2dStatReq(v.Target, 0, -TargetHealMP);
            }

            if ((HealHP > 0 || HealMP > 0) && !v.Caster.IsUnderAnyStatus(BattleStatus.Death) && SpecialSAEffect[v.Caster.Data][7] == 0)
            {
                if (HealHP > 0)
                    v.Caster.CurrentHp = Math.Min(v.Caster.CurrentHp + (uint)HealHP, v.Caster.MaximumHp);
                if (HealMP > 0)
                    v.Caster.CurrentMp = Math.Min(v.Caster.CurrentMp + (uint)HealMP, v.Caster.MaximumMp);

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
                    HPAssistanceDamage = Math.Max((v.Target.HpDamage * (v.Target.HasSupportAbilityByIndex((SupportAbility)1123) ? 30 : 15)) / 100, 1);
                    v.Target.HpDamage = Math.Max((v.Target.HpDamage * (v.Target.HasSupportAbilityByIndex((SupportAbility)1123) ? 70 : 85)) / 100, 1);
                    
                }
                if ((v.Target.Flags & CalcFlag.MpAlteration) != 0)
                {
                    MPAssistanceDamage = Math.Max((v.Target.MpDamage * (v.Target.HasSupportAbilityByIndex((SupportAbility)1123) ? 30 : 15)) / 100, 1);
                    v.Target.MpDamage = Math.Max((v.Target.MpDamage * (v.Target.HasSupportAbilityByIndex((SupportAbility)1123) ? 70 : 85)) / 100, 1);
                }

                foreach (BattleUnit unit in BattleState.EnumerateUnits())
                {
                    if (!unit.IsPlayer || !unit.IsTargetable || unit.Data == v.Target.Data || unit.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify))
                        continue;

                    if (HPAssistanceDamage > 0)
                    {
                        if (unit.CurrentHp - HPAssistanceDamage > 0)
                            unit.CurrentHp = Math.Max(unit.CurrentHp - (uint)HPAssistanceDamage, 0);
                        else
                            unit.CurrentHp = 0;
                    }
                    if (MPAssistanceDamage > 0)
                    {
                        if (unit.CurrentMp - HPAssistanceDamage > 0)
                            unit.CurrentMp = Math.Max(unit.CurrentMp - (uint)HPAssistanceDamage, 0);
                        else
                            unit.CurrentMp = 0;
                    }
                    btl2d.Btl2dStatReq(unit.Data, HPAssistanceDamage, MPAssistanceDamage);
                }
            }

            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)1131) && !v.Caster.InTrance && v.Target.CurrentHp < v.Target.MaximumHp && (v.Target.Flags & CalcFlag.HpRecovery) != 0) // SA Altruistic+
                IncreaseTrance(v.Caster.Data, v.Caster.Will / 10);

            if (v.Target.HasSupportAbilityByIndex((SupportAbility)1201) && (v.Command.AbilityCategory & 8) != 0 && ZidanePassive[v.Target.Data][1] > Comn.random16() % 100) // SA Gorilla+
                BattleState.EnqueueCounter(v.Target, BattleCommandId.Counter, BattleAbilityId.Attack, v.Caster.Id);

            if (v.Target.HasSupportAbilityByIndex((SupportAbility)211) && (v.Target.Flags & CalcFlag.HpRecovery) == 0 && !v.Target.IsUnderAnyStatus(BattleStatus.Heat) && v.Target.CurrentHp < (v.Target.MaximumHp / (v.Target.HasSupportAbilityByIndex((SupportAbility)1211) ? 2 : 4)) && !v.Caster.IsPlayer) // SA Auto Gem
            {
                List <RegularItem> GemList = new List<RegularItem>{ RegularItem.Garnet, RegularItem.Amethyst, RegularItem.Aquamarine, RegularItem.Diamond, RegularItem.Emerald, RegularItem.Moonstone,
                    RegularItem.Ruby, RegularItem.Peridot, RegularItem.Sapphire, RegularItem.Opal, RegularItem.Topaz, RegularItem.LapisLazuli, RegularItem.Ore};

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
                    v.Target.HpDamage = 0;
                else
                    v.Target.HpDamage /= 2;

                Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                {
                    { "US", "Bodyguard!" },
                    { "UK", "Bodyguard!" },
                    { "JP", "Bodyguard!" },
                    { "ES", "Bodyguard!" },
                    { "FR", "Garde du corps !" },
                    { "GR", "Bodyguard!" },
                    { "IT", "Bodyguard!" },
                };
                btl2d.Btl2dReqSymbolMessage(v.Target.Data, "[FF00EA]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 8);
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

            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)235) && v.Command.Id == BattleCommandId.Attack && v.Caster.IsUnderAnyStatus(TranceSeekStatus.Redemption)) // SA Fencing
                v.Target.HpDamage += v.Caster.HasSupportAbilityByIndex((SupportAbility)1235) ? v.Target.HpDamage / 4 : v.Target.HpDamage / 8;

            if ((v.Target.Flags & CalcFlag.HpRecovery) != 0 && v.Caster.HasSupportAbilityByIndex((SupportAbility)127) && v.Target.HpDamage > (v.Target.MaximumHp - v.Target.CurrentHp) && v.Target.IsPlayer) // SA Invigorating
            {
                if (v.Target.MaximumHp == SpecialSAEffect[v.Target.Data][15])
                {
                    uint OldMaximumHP = v.Target.MaximumHp;
                    uint factor = (uint)(v.Caster.HasSupportAbilityByIndex((SupportAbility)1127) ? 20 : 10);
                    uint LimitMaxHP = v.Target.MaximumHp + ((v.Target.MaximumHp * factor) / 100);

                    v.Target.MaximumHp = (uint)Math.Min(v.Target.CurrentHp + v.Target.HpDamage, LimitMaxHP);
                    v.Target.CurrentHp = v.Target.MaximumHp;
                    v.Target.AddDelayedModifier(
                        target => v.Target.CurrentHp > OldMaximumHP,
                        target =>
                        {
                            v.Target.MaximumHp = OldMaximumHP;
                        }
                    );
                }
            }

            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)257)) // SA Mania
            {
                if (FF9StateSystem.EventState.gScriptDictionary.TryGetValue(1030, out Dictionary<Int32, Int32> dict))
                {
                    int MpCost = FF9StateSystem.Battle.FF9Battle.aa_data[v.Command.AbilityId].MP;
                    int IdAA = (int)v.Command.AbilityId;
                    if (IdAA % 2 == 0)
                        MpCost *= Math.Max(1, (dict[IdAA] + dict[IdAA + 1] - 2));
                    else
                        MpCost *= Math.Max(1, (dict[IdAA] + dict[IdAA - 1] - 2));

                    v.Target.HpDamage += (v.Target.HpDamage * MpCost) / 100;
                }
            }

            if (v.Target.IsCovering && v.Target.Data.bi.cover_unit.dms_geo_id == 220)
            {
                BattleUnit Mog = new BattleUnit(v.Target.Data.bi.cover_unit);
                if (Mog.HasSupportAbilityByIndex((SupportAbility)259)) // SA Mog Kiss
                    v.Target.AlterStatus(BattleStatus.Regen);
                if (Mog.HasSupportAbilityByIndex((SupportAbility)1259))
                    v.Target.RemoveStatus(BattleStatusConst.AnyNegative &~BattleStatus.Death);
            }

            if (v.Target.PlayerIndex == CharacterId.Steiner && v.Target.IsCovering && (SteinerPassive[v.Target.Data][0] + SteinerPassive[v.Target.Data][1]) < 5)
            {
                SteinerPassive[v.Target.Data][0]++;
                FF9TextTool.SetCommandName(BattleCommandId.SwordAct, TranceSeekBattleCommand.SwdArtCMDNameVanilla[Localization.CurrentSymbol] + " (" + SteinerPassive[v.Target.Data][0] + "/" + (SteinerPassive[v.Target.Data][0] + SteinerPassive[v.Target.Data][1]) + ")");
                Dictionary<String, String> SteinerPassiveMessage = new Dictionary<String, String>
                {
                    { "US", "[SPRT=IconAtlas,item200_00] Pluto!" },
                    { "UK", "[SPRT=IconAtlas,item200_00] Pluto!" },
                    { "JP", "[SPRT=IconAtlas,item200_00] Pluto!" },
                    { "ES", "[SPRT=IconAtlas,item200_00] Pluto!" },
                    { "FR", "[SPRT=IconAtlas,item200_00] Brutos !" },
                    { "GR", "[SPRT=IconAtlas,item200_00] Pluto!" },
                    { "IT", "[SPRT=IconAtlas,item200_00] Pluto!" },
                };
                btl2d.Btl2dReqSymbolMessage(v.Target.Data, "[5C5C5C]", SteinerPassiveMessage, HUDMessage.MessageStyle.DAMAGE, 30);
            }

            if (v.Caster.Weapon == (RegularItem)1152 && v.Caster.Level == v.Target.Level && v.Command.AbilityId == BattleAbilityId.Attack) // Goblin Sword
                v.Target.HpDamage = v.Target.HpDamage * 3;
        }
    }
}
