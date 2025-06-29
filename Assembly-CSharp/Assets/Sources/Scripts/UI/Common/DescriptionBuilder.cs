using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FF9;
using Memoria;
using Memoria.Assets;
using Memoria.Data;
using Memoria.Prime;

namespace Assets.Sources.Scripts.UI.Common
{
    public static class DescriptionBuilder
    {
        public static Dictionary<string, HashSet<int>> DescriptionTextfromScriptId = new Dictionary<string, HashSet<int>>
        {
            { "ClassicDamageScript", new HashSet<int> { 1, 2, 3, 4, 5, 7, 8, 9, 18, 19, 20, 21, 28, 48, 49, 63, 67, 68, 75, 77, 83, 85, 99, 100, 107 } }, // 77 => DarkMatterScript ?
            { "MPAttackScript", new HashSet<int> { 31 } },
            { "DarksideScript", new HashSet<int> { 32 } },
            { "HealScript", new HashSet<int> { 10, 30, 37, 41, 74, 76 } },
            { "ApplyStatusScript", new HashSet<int> { 11, 14, 46, 87, 103, 105, 108, 109 } }, // 87 : Odin
            { "RemoveStatusScript", new HashSet<int> { 12, 62, 73 } },
            { "DrainScript", new HashSet<int> { 6, 15, 16 } },
            { "GravityScript", new HashSet<int> { 17 } },
            { "ReviveScript", new HashSet<int> { 13 } },
            { "LvDirectHPDamageScript", new HashSet<int> { 22 } },
            { "LVHolyScript", new HashSet<int> { 23 } },
            { "LvReduceDefence", new HashSet<int> { 24 } },
            { "PreciseDirectHPDamageScript", new HashSet<int> { 25, 27 } },
            { "ThousandNeedlesScript", new HashSet<int> { 26 } },
            { "DifferentCasterHPScript", new HashSet<int> { 29, 40 } },
            { "ArmourBreakScript", new HashSet<int> { 33 } },
            { "PowerBreakScript", new HashSet<int> { 34 } },
            { "MentalBreakScript", new HashSet<int> { 35 } },
            { "MagicBreakScript", new HashSet<int> { 36 } },
            { "SpareChangeScript", new HashSet<int> { 38 } },
            { "LancerScript", new HashSet<int> { 39 } },
            { "MightScript", new HashSet<int> { 43 } },
            { "FocusScript", new HashSet<int> { 44 } },
            { "SacrificeScript", new HashSet<int> { 45 } },
            { "SixDragonsScript", new HashSet<int> { 50 } },
            { "CurseScript", new HashSet<int> { 51 } },
            { "AngelSnackScript", new HashSet<int> { 52 } },
            { "LuckySevenScript", new HashSet<int> { 53 } },
            { "WhatIsThatScript", new HashSet<int> { 54 } },
            { "ChangeRowScript", new HashSet<int> { 55 } },
            { "FleeScript", new HashSet<int> { 56, 57 } }, // I think 56 is not used (specific)
            { "StealScript", new HashSet<int> { 58, 101 } },
            { "MugScript", new HashSet<int> { 102 } }, // No description on this one, i mix ClassicDamageScript + StealScript
            { "ScanScript", new HashSet<int> { 59 } },
            { "DetectScript", new HashSet<int> { 60 } },
            { "ChargeScript", new HashSet<int> { 61 } },
            { "SpecialScript", new HashSet<int> { 64 } },
            { "EatScript", new HashSet<int> { 65 } },
            { "FrogDropScript", new HashSet<int> { 66 } },
            // { "ThieveryScript", new HashSet<int> { 67 } }, Present in ClassicDamageScript but maybe we can give a more detailed one.
            // { "DragonCrestScript", new HashSet<int> { 68 } }, Present in ClassicDamageScript but maybe we can give a more detailed one.
            { "ItemHealScript", new HashSet<int> { 69, 70 } },
            { "ItemElixirScript", new HashSet<int> { 71 } },
            { "ItemReviveScript", new HashSet<int> { 72 } },
            { "DoubleCastScript", new HashSet<int> { 80, 84 } },
            { "KamikazeScript", new HashSet<int> { 88 } }, // Melt
            { "HPSwitchingScript", new HashSet<int> { 89 } },
            { "HalfDefenceScript", new HashSet<int> { 90 } },
            { "CannonScript", new HashSet<int> { 91 } },
            { "ItemAddScript", new HashSet<int> { 92 } },
            { "MaelstromScript", new HashSet<int> { 93 } },
            { "AbsorbMagicScript", new HashSet<int> { 94 } },
            { "AbsorbStrengthScript", new HashSet<int> { 95 } },
            { "TranceFullScript", new HashSet<int> { 96 } },
            { "EnticeScript", new HashSet<int> { 97 } },
            { "SimpleAttackGaiaScript", new HashSet<int> { 98 } },
            { "TonberryKarmaScript", new HashSet<int> { 104 } },
            { "SwallowScript", new HashSet<int> { 106 } },
            { "GeneralScript", new HashSet<int> { 0 } }, // Description by "default" if the script ID is not found here.

            // Not used to build a description
            { "PhysicalTypeScript", new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 19, 39, 42, 48, 83, 100, 102, 107 } }, // Based on "PhysicalPenaltyAndBonusAttack"
            { "MagicalTypeScript", new HashSet<int> { 9, 11, 15, 16, 17, 18, 20, 21, 23, 27, 33, 34, 35, 36, 84, 85, 91, 99, 108, 109 } }, // Based on "PenaltyShellAttack" and "PenaltyShellHitRate"
            { "NoApplyStatusScript", new HashSet<int> { 11, 12, 13, 14, 22, 46, 62, 72, 73, 87, 103, 105, 108, 109 } }, // Prevent ApplyStatusScript on specific ScriptId.
            { "UseSpellPowerScript", new HashSet<int> { 9, 10 } } // Prevent ApplyStatusScript on specific ScriptId.
        };

        public static string BuildDescription(DESCRIPTION_INFO desc_info, DescriptionCategory aa_category = 0, string desc_base = "", string lang = "")
        {
            if (string.IsNullOrEmpty(lang))
                lang = Localization.CurrentDisplaySymbol;

            string Description_stats = "";
            string Description = "";
            int ScriptId = desc_info.scriptid;
            Boolean AACategoryPhysical = (desc_info.category & 8) != 0;
            Boolean AACategoryMagical = (desc_info.category & 16) != 0;
            if (((aa_category & DescriptionCategory.ADDSTATS) != 0 || (aa_category & DescriptionCategory.ONLYSTATS) != 0) && ScriptId != 52)
            {
                if ((AACategoryPhysical || AACategoryMagical) && (aa_category & DescriptionCategory.NOCATEGORYSTAT) == 0)
                {
                    if (AACategoryPhysical) // Physical
                        Description_stats += "[ICON=95] ";
                    if (AACategoryMagical) // Magical
                        Description_stats += "[ICON=102] ";

                    Description_stats += " ";
                }
                if (desc_info.power > 0 && (aa_category & DescriptionCategory.NOPOWERSTAT) == 0)
                {
                    Description_stats += $"{Localization.GetWithDefaultAndLang("AADesc_Power", lang)} : {desc_info.power}";
                }
                if (desc_info.hitrate > 0 && (aa_category & DescriptionCategory.NOHITRATESTAT) == 0)
                {
                    if (desc_info.power > 0)
                        Description_stats += " / ";
                    Description_stats += $"{Localization.GetWithDefaultAndLang("AADesc_HitRate", lang)} : {desc_info.hitrate}%";
                }

                if (!String.IsNullOrEmpty(Description_stats))
                    Description_stats += "\n";

                if ((aa_category & DescriptionCategory.ONLYSTATS) != 0)
                    return Description_stats + desc_base;
            }

            var result = DescriptionTextfromScriptId.FirstOrDefault(kvp => kvp.Value.Contains(ScriptId)); // Search the good description...
            if (result.Key == "MugScript")
            {
                Description += $"{Localization.GetWithDefaultAndLang("ClassicDamageScript", lang)}";
                Description += $"\n{Localization.GetWithDefaultAndLang("StealScript", lang)}";
            }
            else if (!string.IsNullOrEmpty(result.Key))
                Description += $"{Localization.GetWithDefaultAndLang(result.Key, lang)}";
            else //... or else, use a "generic" description
            {
                Description += $"{Localization.GetWithDefaultAndLang("GeneralScript", lang)}";
                Description = Regex.Replace(Description, "=AA=", desc_info.name);
            }

            if (Description.Contains("=ELEMENT="))
            {
                string ElementText = "";
                if (desc_info.element == 0)
                    ElementText += $"{Localization.GetWithDefaultAndLang("AADesc_NonElemental", lang)}";
                else
                {
                    if ((desc_info.element & EffectElement.Fire) != 0)
                        ElementText += $"{Localization.GetWithDefaultAndLang("AADesc_Element_Fire", lang)}";
                    if ((desc_info.element & EffectElement.Cold) != 0)
                        ElementText += (!String.IsNullOrEmpty(ElementText) ? " / " : "") + $"{Localization.GetWithDefaultAndLang("AADesc_Element_Cold", lang)}";
                    if ((desc_info.element & EffectElement.Thunder) != 0)
                        ElementText += (!String.IsNullOrEmpty(ElementText) ? " / " : "") + $"{Localization.GetWithDefaultAndLang("AADesc_Element_Thunder", lang)}";
                    if ((desc_info.element & EffectElement.Earth) != 0)
                        ElementText += (!String.IsNullOrEmpty(ElementText) ? " / " : "") + $"{Localization.GetWithDefaultAndLang("AADesc_Element_Earth", lang)}";
                    if ((desc_info.element & EffectElement.Aqua) != 0)
                        ElementText += (!String.IsNullOrEmpty(ElementText) ? " / " : "") + $"{Localization.GetWithDefaultAndLang("AADesc_Element_Aqua", lang)}";
                    if ((desc_info.element & EffectElement.Wind) != 0)
                        ElementText += (!String.IsNullOrEmpty(ElementText) ? " / " : "") + $"{Localization.GetWithDefaultAndLang("AADesc_Element_Wind", lang)}";
                    if ((desc_info.element & EffectElement.Holy) != 0)
                        ElementText += (!String.IsNullOrEmpty(ElementText) ? " / " : "") + $"{Localization.GetWithDefaultAndLang("AADesc_Element_Holy", lang)}";
                    if ((desc_info.element & EffectElement.Darkness) != 0)
                        ElementText += (!String.IsNullOrEmpty(ElementText) ? " / " : "") + $"{Localization.GetWithDefaultAndLang("AADesc_Element_Darkness", lang)}";
                }
                Description = Regex.Replace(Description, "=ELEMENT=", ElementText);
            }
            if ((FF9BattleDB.StatusSets.TryGetValue(desc_info.statusset, out BattleStatusEntry stat) ? stat.Value : 0) > 0 && !DescriptionTextfromScriptId["NoApplyStatusScript"].Contains(ScriptId))
            {
                if (!String.IsNullOrEmpty(Description))
                    Description += " ";

                Description += $"\n{Localization.GetWithDefaultAndLang("ApplyStatusBisScript", lang)}";
            }
            if (Description.Contains("=STATUS=") && (stat.Value > 0 || desc_info.status > 0))
            {
                string StatusText = "";
                if (stat.Value > 0)
                {
                    BattleStatus status = stat.Value;
                    foreach (BattleStatusId statusId in status.ToStatusList())
                    {
                        if (!String.IsNullOrEmpty(StatusText))
                            StatusText += " / ";
                        StatusText += ($"[A85038][HSHD]{Localization.GetWithDefaultAndLang("Status" + statusId.ToString(), lang)}[383838][HSHD]");
                        if (BattleHUD.BuffIconNames.TryGetValue(statusId, out String buffspriteName))
                            StatusText += ($" [SPRT={buffspriteName},48,48] ");
                        if (BattleHUD.DebuffIconNames.TryGetValue(statusId, out String debuffspriteName))
                            StatusText += ($" [SPRT={debuffspriteName},48,48] ");
                    }
                }
                else
                {
                    foreach (BattleStatusId statusId in desc_info.status.ToStatusList())
                    {
                        if (!String.IsNullOrEmpty(StatusText))
                            StatusText += " / ";
                        StatusText += ($"[A85038][HSHD]{Localization.GetWithDefaultAndLang("Status" + statusId.ToString(), lang)}[383838][HSHD]");
                        if (BattleHUD.BuffIconNames.TryGetValue(statusId, out String buffspriteName))
                            StatusText += ($" [SPRT={buffspriteName},48,48] ");
                        if (BattleHUD.DebuffIconNames.TryGetValue(statusId, out String debuffspriteName))
                            StatusText += ($" [SPRT={debuffspriteName},48,48] ");
                    }
                }

                if (desc_info.hitrate > 0 && desc_info.hitrate < 255)
                    StatusText += $" ({desc_info.hitrate}%)";
                Description = Regex.Replace(Description, "=STATUS=", StatusText);
            }
            if (Description.Contains("=TYPE="))
            {
                string TypeText = "";              
                if (DescriptionTextfromScriptId["PhysicalTypeScript"].Contains(ScriptId) && DescriptionTextfromScriptId["MagicalTypeScript"].Contains(ScriptId))
                    TypeText += Localization.GetWithDefaultAndLang("AADesc_Physical", lang) + " and " + Localization.GetWithDefaultAndLang("AADesc_Magical", lang);
                else if (DescriptionTextfromScriptId["PhysicalTypeScript"].Contains(ScriptId))
                    TypeText += Localization.GetWithDefaultAndLang("AADesc_Physical", lang);
                else if (DescriptionTextfromScriptId["MagicalTypeScript"].Contains(ScriptId)) 
                    TypeText += Localization.GetWithDefaultAndLang("AADesc_Magical", lang);
                else if (AACategoryPhysical)
                    TypeText += Localization.GetWithDefaultAndLang("AADesc_Physical", lang);
                else if (AACategoryMagical)
                    TypeText += Localization.GetWithDefaultAndLang("AADesc_Magical", lang);
                Description = Regex.Replace(Description, "=TYPE=", TypeText);
            }
            if (Description.Contains("=TARGET="))
            {
                string TargetText = "";
                TargetText = Localization.GetWithDefaultAndLang($"AADesc_{desc_info.target}", lang);
                if (ScriptId == 41) // Edit target for WhiteDraw (the true target is the player team)
                    TargetText = Localization.GetWithDefaultAndLang("AADesc_AllAlly", lang);
                else if (string.IsNullOrEmpty(TargetText)) // By Default
                    TargetText = Localization.GetWithDefaultAndLang("AADesc_SingleAny", lang);

                Description = Regex.Replace(Description, "=TARGET=", TargetText);
            }
            if (Description.Contains("=SPELLPWR="))
            {
                string SpellPwrText = "";
                if (DescriptionTextfromScriptId["UseSpellPowerScript"].Contains(ScriptId))
                {
                    if (desc_info.power >= 70)
                        SpellPwrText = Localization.GetWithDefaultAndLang("AADesc_StrongSpell", lang);
                    else if (desc_info.power >= 25)
                        SpellPwrText = Localization.GetWithDefaultAndLang("AADesc_NormalSpell", lang);
                    else
                        SpellPwrText = Localization.GetWithDefaultAndLang("AADesc_WeakSpell", lang);
                }
                Description = Regex.Replace(Description, "=SPELLPWR=", SpellPwrText);
            }
            if (Description.Contains("=DAMAGE=") || Description.Contains("=FLAGS="))
            {
                if (ScriptId == 40 || ScriptId == 41 || ScriptId == 61) // Exception like WhiteDraw
                {
                    Description = Regex.Replace(Description, "=FLAGS=", Localization.GetWithDefaultAndLang("AADesc_MP", lang));
                }
                else
                {
                    string DamageText = "";
                    int recoverHPvalue = 0;
                    int recoverMPvalue = 0;
                    try
                    {
                        BTL_DATA casterdummy = new BTL_DATA();
                        BTL_DATA targetdummy = new BTL_DATA();
                        casterdummy.saExtended = new HashSet<SupportAbility>();
                        targetdummy.saExtended = new HashSet<SupportAbility>();
                        CMD_DATA testCommand = new CMD_DATA
                        {
                            regist = casterdummy,
                            //cmd_no = desc_info.itemid > 0 ? BattleCommandId.Item : BattleCommandId.Attack,
                            cmd_no = BattleCommandId.Attack,
                            tar_id = 0,
                            ScriptId = ScriptId
                        };
                        BattleCommand testBattleCMD = new BattleCommand(testCommand);
                        testBattleCMD.Power = desc_info.power;
                        BattleCalculator v = new BattleCalculator(casterdummy, targetdummy, testBattleCMD);
                        BattleScriptFactory factory = SBattleCalculator.FindScriptFactory(ScriptId);
                        if (factory != null)
                        {
                            IBattleScript script = factory(v);
                            script.Perform();
                        }
                        recoverHPvalue = v.Target.HpDamage;
                        recoverMPvalue = v.Target.MpDamage;
                        Boolean targetHPflag = (v.Target.Flags & CalcFlag.HpRecovery) != 0 || (v.Caster.Flags & CalcFlag.HpRecovery) != 0;
                        Boolean targetMPflag = (v.Target.Flags & CalcFlag.MpRecovery) != 0 || (v.Caster.Flags & CalcFlag.MpRecovery) != 0;
                        if (recoverHPvalue > 0 && recoverMPvalue > 0 || result.Key == "ItemElixirScript" || targetHPflag && targetMPflag)
                        {
                            DamageText = recoverHPvalue.ToString() + " " + Localization.GetWithDefaultAndLang("AADesc_HP", lang) + "/" + recoverMPvalue.ToString() + " " + Localization.GetWithDefaultAndLang("AADesc_MP", lang);
                            Description = Regex.Replace(Description, "=FLAGS=", Localization.GetWithDefaultAndLang("AADesc_HP", lang) + "/" + Localization.GetWithDefaultAndLang("AADesc_MP", lang));
                        }
                        else if (recoverHPvalue > 0 || ScriptId == 30 || targetHPflag)
                        {
                            DamageText = recoverHPvalue.ToString() + " " + Localization.GetWithDefaultAndLang("AADesc_HP", lang);
                            Description = Regex.Replace(Description, "=FLAGS=", Localization.GetWithDefaultAndLang("AADesc_HP", lang));
                        }
                        else if (recoverMPvalue > 0 || targetMPflag)
                        {
                            DamageText = recoverMPvalue.ToString() + " " + Localization.GetWithDefaultAndLang("AADesc_MP", lang);
                            Description = Regex.Replace(Description, "=FLAGS=", Localization.GetWithDefaultAndLang("AADesc_MP", lang));
                        }
                    }
                    catch (Exception err)
                    {
                        Log.Error(err);
                    }
                    Description = Regex.Replace(Description, "=DAMAGE=", DamageText);
                }
            }
            if (Description.Contains("=STRENGTH="))
                Description = Regex.Replace(Description, "=STRENGTH=", Localization.GetWithDefaultAndLang("Strength", lang));
            if (Description.Contains("=MAGIC="))
                Description = Regex.Replace(Description, "=MAGIC=", Localization.GetWithDefaultAndLang("Magic", lang));
            if (Description.Contains("=POWER="))
                Description = Regex.Replace(Description, "=POWER=", desc_info.power.ToString());
            if (Description.Contains("=HITRATE="))
                Description = Regex.Replace(Description, "=HITRATE=", desc_info.hitrate.ToString());
            if (Description.Contains("=ITEM="))
                Description = Regex.Replace(Description, "=ITEM=", FF9TextTool.ItemName((RegularItem)desc_info.power));
            if (Description.Contains("=CUREKO="))
                Description = Regex.Replace(Description, "=CUREKO=", Localization.GetWithDefaultAndLang("CureDeath", lang));
            
            if (result.Key == "DoubleCastScript")
            {
                Description = Regex.Replace(Description, "=SPELL1=", FF9TextTool.ActionAbilityName((BattleAbilityId)desc_info.power));
                Description = Regex.Replace(Description, "=SPELL2=", FF9TextTool.ActionAbilityName((BattleAbilityId)desc_info.hitrate));
            }

            if ((aa_category & DescriptionCategory.CONCACTBEFORE) != 0)
                Description = Description_stats + Description + (String.IsNullOrEmpty(Description) ? "" : "\n") + desc_base;
            else if ((aa_category & DescriptionCategory.CONCACTAFTER) != 0)
                Description = Description_stats + desc_base + (String.IsNullOrEmpty(desc_base) ? "" : "\n") + Description;

            return Description;
        }

        public static void BuildAndAddDescriptionInDict(BattleAbilityId ability, DescriptionCategory aa_stat = 0) // [DV] Seen with Tir, not really necessary.
        {
            string MainDescription = "";
            string SecondDescription = "";
            Dictionary<String, String> descriptioncreated = new Dictionary<String, String>();

            if (!AADescriptionBuildedFromBuilder.ContainsKey(ability))
            {
                MainDescription = BuildDescriptionAA(FF9StateSystem.Battle.FF9Battle.aa_data[ability], aa_stat, Localization.CurrentSymbol);
                if (!String.IsNullOrEmpty(MainDescription))
                    descriptioncreated.Add(Localization.CurrentDisplaySymbol, MainDescription);

                if (Configuration.Lang.DualLanguageMode != 0)
                {
                    SecondDescription = BuildDescriptionAA(FF9StateSystem.Battle.FF9Battle.aa_data[ability], aa_stat, Configuration.Lang.DualLanguage);
                    if (!String.IsNullOrEmpty(SecondDescription))
                        descriptioncreated.Add(Configuration.Lang.DualLanguage, SecondDescription);
                }
                if (descriptioncreated.Count > 0)
                    AADescriptionBuildedFromBuilder.Add(ability, descriptioncreated);
            }
        }

        public static string BuildDescriptionAA(AA_DATA ability, DescriptionCategory aa_category = 0, string desc_base = "", string lang = "")
        {
            DESCRIPTION_INFO desc_info_aa = new DESCRIPTION_INFO
            {
                name = ability.Name,
                power = ability.Ref.Power,
                hitrate = ability.Ref.Rate,
                scriptid = ability.Ref.ScriptId,
                category = ability.Category,
                element = (EffectElement)ability.Ref.Elements,
                target = ability.Info.Target,
                statusset = ability.AddStatusNo
            };
            return BuildDescription(desc_info_aa, aa_category, desc_base, lang);
        }

        public static string BuildDescriptionItem(RegularItem itemId, DescriptionCategory aa_category = 0, string desc_base = "", string lang = "")
        {
            FF9ITEM_DATA itemData = ff9item._FF9Item_Data[itemId];

            if (itemData == null || itemId == RegularItem.NoItem)
                return "";

            int itempower = 0;
            int itemhitrate = 0;
            int itemscriptid = 0;
            TargetType itemtarget = 0;
            BattleStatus itemstatus = 0;
            StatusSetId itemstatusset = 0;
            WeaponCategory weaponcategory = 0;
            if (itemData.effect_id >= 0 && ff9item._FF9Item_Info.TryGetValue(itemData.effect_id, out ITEM_DATA item_data))
            {
                itempower = item_data.Ref.Power;
                itemhitrate = item_data.Ref.Rate;
                itemscriptid = item_data.Ref.ScriptId;
                itemtarget = item_data.info.Target;
                itemstatus = item_data.status;
            }
            // if (itemData.armor_id >= 0 && ff9armor.ArmorData.TryGetValue(itemData.armor_id, out ItemDefence armor_data)) [DV] Not useful

            if (itemData.weapon_id >= 0 && ff9weap.WeaponData.TryGetValue(itemData.weapon_id, out ItemAttack weapon_data))
            {
                itempower = weapon_data.Ref.Power;
                itemhitrate = weapon_data.Ref.Rate;
                itemscriptid = weapon_data.Ref.ScriptId;
                weaponcategory = weapon_data.Category;
                itemstatusset = weapon_data.StatusIndex;
            }

            DESCRIPTION_INFO desc_info_item = new DESCRIPTION_INFO
            {
                itemid = itemId,
                name = FF9TextTool.ItemName(itemId),
                power = itempower,
                hitrate = itemhitrate,
                scriptid = itemscriptid,
                target = itemtarget,
                status = itemstatus,
                statusset = itemstatusset,
                weapon_category = weaponcategory,
                item_effect = itemData.effect_id >= 0,
                item_weapon = itemData.weapon_id >= 0
            };

            return BuildDescription(desc_info_item, aa_category, desc_base, lang);
        }

        public class DESCRIPTION_INFO
        {
            public string name = "";
            public Int32 power = 0;
            public Int32 hitrate = 0;
            public Int32 scriptid = 0;
            public Int32 category = 0;
            public EffectElement element = 0;
            public TargetType target = 0;
            public BattleStatus status = 0;
            public StatusSetId statusset = 0;

            public RegularItem itemid = 0;
            public WeaponCategory weapon_category = 0;
            public Boolean item_effect = false;
            public Boolean item_weapon = false;
            //public Boolean item_armor = false;
        }

        public enum DescriptionCategory
        {
            ADDSTATS = 1,
            CONCACTBEFORE = 2,
            CONCACTAFTER = 4,
            ONLYSTATS = 8,
            NOPOWERSTAT = 16,
            NOHITRATESTAT = 32,
            NOCATEGORYSTAT = 64
        }

        public static Dictionary<BattleAbilityId, DescriptionCategory> AADescriptionFromBuilder = new Dictionary<BattleAbilityId, DescriptionCategory>();
        public static Dictionary<RegularItem, DescriptionCategory> ItemDescriptionFromBuilder = new Dictionary<RegularItem, DescriptionCategory>();

        public static Dictionary<BattleAbilityId, Dictionary<String, String>> AADescriptionBuildedFromBuilder = new Dictionary<BattleAbilityId, Dictionary<String, String>>();
        public static Dictionary<KeyValuePair<BattleCommandId, int>, Dictionary<String, String>> MonsterAADescriptionBuildedFromBuilder = new Dictionary<KeyValuePair<BattleCommandId, int>, Dictionary<String, String>>();
    }
}
