using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Assets;
using Memoria.Data;
using Memoria.Database;
using Memoria.Prime;
using System;
using System.Collections.Generic;
using static Memoria.Scripts.Battle.TranceSeekBattleDictionary;
using static SFX;

namespace Memoria.Scripts.Battle
{
    public static class TranceSeekCharacterMechanic
    {
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

        public static void GarnetGemMechanic(this BattleCalculator v, GarnetGemMechanic_Type type = GarnetGemMechanic_Type.ElementalAndHeal)
        {
            if (v.Target.PlayerIndex == CharacterId.Garnet)
            {
                Boolean GarnetInTrance = v.Target.InTrance;
                RegularItem GarnetAccessory = v.Target.Accessory;
                int number_element = 0;
                int number_gem = 0;
                float BonusSA = v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.Gemologist_Boosted) ? 1.5f : (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.Gemologist) ? 1.25f : 1);

                if (type == GarnetGemMechanic_Type.BoostPhysicalEvade)
                {
                    if ((GarnetInTrance || GarnetAccessory == RegularItem.Amethyst))
                        v.Context.Evade += (Int16)(v.Context.Evade * ((1 + ff9item.FF9Item_GetCount(RegularItem.Amethyst) * BonusSA) / 400));
                }
                else if (type == GarnetGemMechanic_Type.BoostMagicalEvade)
                {
                    if ((GarnetInTrance || GarnetAccessory == RegularItem.LapisLazuli))
                        v.Context.Evade += (Int16)(v.Context.Evade * ((1 + ff9item.FF9Item_GetCount(RegularItem.LapisLazuli) * BonusSA) / 400));
                }
                else
                {
                    if (v.Command.Element > 0 && (v.Target.Flags & CalcFlag.HpAlteration) != 0) // Elemental Gems
                    {
                        if ((GarnetInTrance || GarnetAccessory == RegularItem.Topaz) && (v.Command.Element & EffectElement.Fire) != 0)
                        {
                            number_element++;
                            number_gem += ff9item.FF9Item_GetCount(RegularItem.Topaz);
                        }
                        if ((GarnetInTrance || GarnetAccessory == RegularItem.Opal) && (v.Command.Element & EffectElement.Cold) != 0)
                        {
                            number_element++;
                            number_gem += ff9item.FF9Item_GetCount(RegularItem.Opal);
                        }
                        if ((GarnetInTrance || GarnetAccessory == RegularItem.Peridot) && (v.Command.Element & EffectElement.Thunder) != 0)
                        {
                            number_element++;
                            number_gem += ff9item.FF9Item_GetCount(RegularItem.Peridot);
                        }
                        if ((GarnetInTrance || GarnetAccessory == RegularItem.Sapphire) && (v.Command.Element & EffectElement.Earth) != 0)
                        {
                            number_element++;
                            number_gem += ff9item.FF9Item_GetCount(RegularItem.Sapphire);
                        }
                        if ((GarnetInTrance || GarnetAccessory == RegularItem.Aquamarine) && (v.Command.Element & EffectElement.Aqua) != 0)
                        {
                            number_element++;
                            number_gem += ff9item.FF9Item_GetCount(RegularItem.Aquamarine);
                        }
                        if (number_element > 0)
                        {
                            v.Target.Flags |= CalcFlag.HpRecovery;
                            v.Target.HpDamage += (Int16)(v.Target.HpDamage * ((1 + (number_gem / number_element) * BonusSA) / 100));
                        }
                    }
                    if ((GarnetInTrance || GarnetAccessory == RegularItem.Diamond) && (v.Command.AbilityCategory & 8) != 0) // "Physical" damage reduction
                        v.Target.HpDamage -= (Int16)(v.Target.HpDamage * ((1 + ff9item.FF9Item_GetCount(RegularItem.Diamond) * BonusSA) / 400));
                    if ((GarnetInTrance || GarnetAccessory == RegularItem.Garnet) && (v.Command.AbilityCategory & 16) != 0) // "Magical" damage reduction
                        v.Target.HpDamage -= (Int16)(v.Target.HpDamage * ((1 + ff9item.FF9Item_GetCount(RegularItem.Garnet) * BonusSA) / 400));
                }
            }
            else if (v.Caster.PlayerIndex == CharacterId.Garnet && v.Command.ScriptId == 10 && (v.Caster.InTrance || v.Caster.Accessory == RegularItem.Moonstone)) // Only for White Magic Spell
            {
                float BonusSA = v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.Gemologist_Boosted) ? 1.5f : (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.Gemologist) ? 1.25f : 1);
                v.Target.HpDamage += (Int16)(v.Target.HpDamage * ((1 + ff9item.FF9Item_GetCount(RegularItem.Moonstone) * BonusSA) / 200));
            }
        }

        public enum GarnetGemMechanic_Type
        {
            ElementalAndHeal,
            BoostPhysicalEvade,
            BoostMagicalEvade
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

        public static void EikoMougMechanic(BattleCalculator v)
        {
            if (FF9StateSystem.Common.FF9.party.IsInParty(CharacterId.Eiko) && v.Command.ScriptId != 64 && v.Command.ScriptId != 164 && v.Command.Id != BattleCommandId.Counter && v.Command.Data.info.effect_counter == 1)
            {
                if (v.Caster.IsPlayer && v.Target.IsUnderAnyStatus(BattleStatus.Death) && (v.Command.ScriptId == 13 || v.Command.ScriptId == 72)) // Don't trigger if a player revive someone.
                    return;

                BattleUnit Eiko = BattleState.GetPlayerUnit(CharacterId.Eiko);

                if (Eiko == null)
                    return;

                if (PassiveEikoScript.StateMoug[Eiko.Data] == 0 && !Eiko.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump | BattleStatus.Heat) && v.Caster.Data != Eiko.Data)
                {
                    float ChanceMoug = (float)(Eiko.HasSupportAbilityByIndex((SupportAbility)1224) ? 12 : (Eiko.HasSupportAbilityByIndex((SupportAbility)224) ? 10 : 8));

                    if (Comn.random16() % 100 > ChanceMoug || Eiko.HasSupportAbilityByIndex((SupportAbility)226)) // Synergy
                        return;

                    ushort TargetId = v.Caster.Id;
                    PassiveEikoScript.StateMoug[Eiko.Data] = 1;
                    Boolean TargetReflect = false;
                    List<BattleAbilityId> ClassicMougAAList = new List<BattleAbilityId>();
                    List<BattleAbilityId> SuperMougAAList = new List<BattleAbilityId>();
                    foreach (BattleAbilityId abilId in CharacterCommands.Commands[(BattleCommandId)1049].EnumerateAbilities()) // CMD Kupo (not used for Eiko)
                    {
                        Boolean AddAA = false;
                        switch (abilId)
                        {
                            case (BattleAbilityId)2000: // Mog Cure
                            {
                                if (Eiko.CurrentHp <= Eiko.MaximumHp / 2)
                                    AddAA = true;
                                break;
                            }
                            case (BattleAbilityId)2001: // Mog Hug
                            {
                                if (Eiko.CurrentMp <= Eiko.MaximumMp / 2)
                                    AddAA = true;
                                break;
                            }
                            case (BattleAbilityId)2002: // Mog Regen
                            {
                                if (!Eiko.IsUnderAnyStatus(BattleStatus.Regen))
                                    AddAA = true;
                                break;
                            }
                            case (BattleAbilityId)2004: // Mog Mirror
                            {
                                if (!Eiko.IsUnderAnyStatus(BattleStatus.Vanish))
                                    AddAA = true;
                                break;
                            }
                            case (BattleAbilityId)2005: // Mog AutoLife
                            {
                                if (Eiko.Level >= 35 && !Eiko.IsUnderAnyStatus(BattleStatus.AutoLife))
                                    AddAA = true;
                                break;
                            }
                            case (BattleAbilityId)2006: // Mog Esuna
                            {
                                if (Eiko.IsUnderAnyStatus(BattleStatus.Poison | BattleStatus.Venom | BattleStatus.Silence | BattleStatus.Blind | BattleStatus.Trouble | BattleStatus.Berserk | BattleStatus.Mini | TranceSeekStatus.Vieillissement))
                                    AddAA = true;
                                break;
                            }
                            case (BattleAbilityId)2007: // Mog Support
                            {
                                if (Eiko.Level >= 30 && (StackBreakOrUpStatus[Eiko.Data][1] < 50 || StackBreakOrUpStatus[Eiko.Data][3] < 50))
                                    AddAA = true;
                                break;
                            }
                            case (BattleAbilityId)2009: // Mog Flare
                            case (BattleAbilityId)2010: // Mog Holy
                            {
                                if (FF9StateSystem.EventState.ScenarioCounter >= 9990) // The party finds Hilda
                                {
                                    Boolean TargetAvailable = true;
                                    foreach (BattleUnit monster in BattleState.EnumerateUnits())
                                    {
                                        if (!monster.IsPlayer && monster.IsTargetable && !monster.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump | BattleStatus.Reflect))
                                            TargetAvailable = true;

                                        if (monster.IsUnderAnyStatus(BattleStatus.Reflect))
                                            TargetReflect = true;
                                    }


                                    if (TargetAvailable)
                                        AddAA = true;
                                }
                                break;
                            }
                            case (BattleAbilityId)2011: // Moga Cure
                            {
                                if (Eiko.Level >= 40)
                                {
                                    uint CurrentHPTeam = 0;
                                    uint CurrentMaxHPTeam = 0;
                                    Boolean ZombiePresent = false;
                                    foreach (BattleUnit unit in BattleState.EnumerateUnits())
                                        if (unit.IsPlayer && unit.IsTargetable && !unit.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump))
                                        {
                                            CurrentHPTeam += unit.CurrentHp;
                                            CurrentMaxHPTeam += unit.MaximumHp;
                                            if (unit.IsZombie)
                                                ZombiePresent = true;
                                        }
                                    if (CurrentHPTeam <= (CurrentMaxHPTeam / 2) && !ZombiePresent)
                                        AddAA = true;
                                }
                                break;
                            }
                            case (BattleAbilityId)2012: // Moga Hug
                            {
                                if (Eiko.Level >= 50)
                                {
                                    uint CurrentMPTeam = 0;
                                    uint CurrentMaxMPTeam = 0;
                                    Boolean ZombiePresent = false;
                                    foreach (BattleUnit unit in BattleState.EnumerateUnits())
                                        if (unit.IsPlayer && unit.IsTargetable && !unit.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump))
                                        {
                                            CurrentMPTeam += unit.CurrentMp;
                                            CurrentMaxMPTeam += unit.MaximumMp;
                                            if (unit.IsZombie)
                                                ZombiePresent = true;
                                        }

                                    if (CurrentMPTeam <= (CurrentMaxMPTeam / 4) && !ZombiePresent)
                                        AddAA = true;
                                }
                                break;
                            }
                            case (BattleAbilityId)2013: // Moga Regen
                            {
                                if (Eiko.Level >= 70)
                                {
                                    Boolean StatusToApply = false;
                                    foreach (BattleUnit unit in BattleState.EnumerateUnits())
                                        if (unit.IsPlayer && unit.IsTargetable && !unit.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump | BattleStatus.Regen))
                                            StatusToApply = true;

                                    if (StatusToApply)
                                        AddAA = true;
                                }
                                break;
                            }
                            case (BattleAbilityId)2014: // Moga Shield
                            {
                                if (Eiko.Level >= 60)
                                    AddAA = true;
                                break;
                            }
                            case (BattleAbilityId)2015: // Moga Mirror
                            {
                                if (Eiko.Level >= 80)
                                {
                                    Boolean StatusToApply = false;
                                    foreach (BattleUnit unit in BattleState.EnumerateUnits())
                                        if (unit.IsPlayer && unit.IsTargetable && !unit.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump | BattleStatus.Vanish))
                                            StatusToApply = true;

                                    if (StatusToApply)
                                        AddAA = true;
                                }
                                break;
                            }
                            case (BattleAbilityId)2017: // Moga Esuna
                            {
                                if (Eiko.Level >= 75)
                                {
                                    Boolean StatusToCure = false;
                                    foreach (BattleUnit unit in BattleState.EnumerateUnits())
                                        if (unit.IsPlayer && unit.IsTargetable && !unit.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump) && unit.IsUnderAnyStatus(BattleStatus.Poison | BattleStatus.Venom | BattleStatus.Silence | BattleStatus.Blind | BattleStatus.Trouble | BattleStatus.Mini | BattleStatus.Berserk | TranceSeekStatus.Vieillissement))
                                            StatusToCure = true;

                                    if (StatusToCure)
                                        AddAA = true;
                                }
                                break;
                            }
                            case (BattleAbilityId)2018: // Moga Support
                            {
                                if (Eiko.Level >= 85)
                                {
                                    Boolean StatusToApply = false;
                                    foreach (BattleUnit unit in BattleState.EnumerateUnits())
                                        if (unit.IsPlayer && unit.IsTargetable && !unit.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump) && (StackBreakOrUpStatus[unit.Data][1] < 50 || StackBreakOrUpStatus[unit.Data][3] < 50))
                                            StatusToApply = true;

                                    if (StatusToApply)
                                        AddAA = true;
                                }
                                break;
                            }
                        }
                        if (AddAA)
                        {
                            if ((int)abilId <= 2010)
                                ClassicMougAAList.Add(abilId);
                            else
                                SuperMougAAList.Add(abilId);
                        }

                    }

                    if (ClassicMougAAList.Count == 0)
                        return;

                    BattleAbilityId MougAAChoosen = ClassicMougAAList[GameRandom.Next16() % ClassicMougAAList.Count]; // Classic Mog spell
                    if (GameRandom.Next16() % 100 < 20 && SuperMougAAList.Count > 0)
                        MougAAChoosen = SuperMougAAList[GameRandom.Next16() % SuperMougAAList.Count];
                    if (GameRandom.Next16() % 100 < 5 && Eiko.Level >= 90) // Moga Autolife spell
                    {
                        Boolean StatusToApply = false;
                        foreach (BattleUnit unit in BattleState.EnumerateUnits())
                            if (unit.IsPlayer && unit.IsTargetable && !unit.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump | BattleStatus.AutoLife))
                                StatusToApply = true;

                        if (!StatusToApply)
                            MougAAChoosen = TranceSeekBattleAbility.MogAutoLife2;
                    }
                    if (GameRandom.Next16() % 100 == 0 && Eiko.Level >= 99) // MougaHoming
                        MougAAChoosen = TranceSeekBattleAbility.MougaHoming;

                    TargetType TargetAA = FF9StateSystem.Battle.FF9Battle.aa_data[MougAAChoosen].Info.Target;
                    Boolean TargetDefaultAlly = FF9StateSystem.Battle.FF9Battle.aa_data[MougAAChoosen].Info.DefaultAlly;

                    if (TargetDefaultAlly)
                    {
                        if (TargetAA == TargetType.Self)
                            TargetId = Eiko.Id;
                        else
                            TargetId = 15;
                    }
                    else
                    {
                        if (TargetAA == TargetType.AllEnemy)
                            TargetId = 240;
                        else
                            TargetId = BattleState.GetRandomUnitId(isPlayer: false);
                    }

                    if (Comn.random16() % 100 <= ChanceMoug) // Mog Life
                    {
                        List<UInt16> candidates = new List<UInt16>(4);
                        for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next)
                            if (next.bi.player == 1 && btl_stat.CheckStatus(next, BattleStatus.Death) && !btl_stat.CheckStatus(next, BattleStatus.Zombie) && next.bi.target != 0)
                                candidates.Add(next.btl_id);

                        if (candidates.Count > 0)
                        {
                            TargetId = candidates[UnityEngine.Random.Range(0, candidates.Count)];
                            MougAAChoosen = (BattleAbilityId)2008;
                        }
                    }

                    if (TargetReflect && (MougAAChoosen == (BattleAbilityId)2009 || MougAAChoosen == (BattleAbilityId)2010)) // Prevent Moug use Mog Flare / Mog Holy on target under Reflect
                    {
                        List<UInt16> TargetsAvailable = new List<UInt16>(4);
                        for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next)
                            if (next.bi.player == 0 && (!btl_stat.CheckStatus(next, BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Reflect)) && next.bi.target != 0)
                                TargetsAvailable.Add(next.btl_id);

                        if (TargetsAvailable.Count > 0)
                            TargetId = TargetsAvailable[UnityEngine.Random.Range(0, TargetsAvailable.Count)];
                    }
                    btl_cmd.SetCounter(Eiko, BattleCommandId.Counter, (int)MougAAChoosen, TargetId);
                }
            }
        }
    }
}
