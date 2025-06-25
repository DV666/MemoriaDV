using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text.RegularExpressions;
using FF9;
using Memoria.Assets;
using Memoria.Data;

namespace Assets.Sources.Scripts.UI.Common
{
    public static class DescriptionBuilder
    {
        // Maybe doing a Dictionary for that ? To add custom script.
        public static Dictionary<string, HashSet<int>> DescriptionTextfromScriptId = new Dictionary<string, HashSet<int>>
        {
            { "ClassicDamageScript", new HashSet<int> { 1, 2, 3, 4, 5, 7, 8, 9, 18, 19, 20, 21, 28, 39, 48, 49, 63, 67, 68, 75, 77, 83, 85, 99, 100, 107 } }, // 77 => DarkMatterScript ?
            { "MPAttackScript", new HashSet<int> { 31 } },
            { "DarksideScript", new HashSet<int> { 32 } },
            { "HealScript", new HashSet<int> { 10, 30, 37, 41, 69, 70, 71, 74, 76 } },
            { "ApplyStatusScript", new HashSet<int> { 11, 14, 46, 87, 103, 105, 108, 109 } }, // 87 : Odin
            { "RemoveStatusScript", new HashSet<int> { 12, 62, 73 } },
            { "DrainScript", new HashSet<int> { 6, 15, 16 } },
            { "GravityScript", new HashSet<int> { 17 } },
            { "ReviveScript", new HashSet<int> { 13, 72 } },
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
            { "EatScript", new HashSet<int> { 65 } },
            { "FrogDropScript", new HashSet<int> { 66 } },
            // { "ThieveryScript", new HashSet<int> { 67 } }, Present in ClassicDamageScript but maybe we can give a more detailed one.
            // { "DragonCrestScript", new HashSet<int> { 68 } }, Present in ClassicDamageScript but maybe we can give a more detailed one.
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
            { "GeneralScript", new HashSet<int> { 0 } } // Description by "default" if the script ID is not found here.
        };

        public static string BuildDescriptionAA(AA_DATA ability, Boolean aa_stat = false)
        {
            string Description = "";
            if (aa_stat)
            {
                if ((ability.Category & 8) != 0 || (ability.Category & 8) != 0)
                {
                    if ((ability.Category & 8) != 0) // Physical
                        Description += "[ICON=95] ";
                    if ((ability.Category & 16) != 0) // Magical
                        Description += "[ICON=102] ";

                    Description += " / ";
                }
                if (ability.Ref.Power > 0)
                {
                    Description += $"{Localization.GetWithDefault("AADesc_Power")} : {ability.Ref.Power}";
                }
                if (ability.Ref.Rate > 0 && false)
                {
                    if (ability.Ref.Power > 0)
                        Description += " / ";
                    Description += $"{Localization.GetWithDefault("AADesc_HitRate")} : {ability.Ref.Rate}%\n";
                }
            }
            if (!String.IsNullOrEmpty(Description))
                Description += "\n";

            var result = DescriptionTextfromScriptId.FirstOrDefault(kvp => kvp.Value.Contains(ability.Ref.ScriptId)); // Search the good description...
            if (result.Key == "MugScript")
            {
                Description += $"{Localization.GetWithDefault("ClassicDamageScript")}";
                Description += $"\n{Localization.GetWithDefault("StealScript")}";
            }
            else if (!string.IsNullOrEmpty(result.Key))
                Description += $"{Localization.GetWithDefault(result.Key)}";
            else //... or else, use a "generic" description
            {
                Description += $"{Localization.GetWithDefault("GeneralScript")}";
                Description = Regex.Replace(Description, "=AA=", ability.Name);
            }

            if (ability.Ref.ScriptId == 10 || ability.Ref.ScriptId == 30 || ability.Ref.ScriptId == 69) // Magic Heal, Potion, White Wind
                Description = Regex.Replace(Description, "=FLAGS=", "HP");
            else if (ability.Ref.ScriptId == 70) // Ether
                Description = Regex.Replace(Description, "=FLAGS=", "MP");
            else if (ability.Ref.ScriptId == 37 || ability.Ref.ScriptId == 71) // Chakra, Elixir
                Description = Regex.Replace(Description, "=FLAGS=", "HP/MP");
            else if (ability.Ref.ScriptId == 6 || ability.Ref.ScriptId == 16) // Drain HP
                Description = Regex.Replace(Description, "=FLAGS=", Localization.GetWithDefault("AADesc_HP"));
            else if (ability.Ref.ScriptId == 15) // Drain MP
                Description = Regex.Replace(Description, "=FLAGS=", Localization.GetWithDefault("AADesc_MP"));

            if (Description.Contains("=ELEMENT="))
            {
                string ElementText = "";
                if (ability.Ref.Elements == 0)
                    ElementText += $"{Localization.GetWithDefault("AADesc_NonElemental")}";
                else
                {
                    if (((EffectElement)ability.Ref.Elements & EffectElement.Fire) != 0)
                        ElementText += $"[DF0000][HSHD]{FF9TextTool.BattleFollowText(0)}[383838][HSHD]";
                    if (((EffectElement)ability.Ref.Elements & EffectElement.Cold) != 0)
                        ElementText += (!String.IsNullOrEmpty(ElementText) ? " / " : "") + $"[028E8E][HSHD]{FF9TextTool.BattleFollowText(1)}[383838][HSHD]";
                    if (((EffectElement)ability.Ref.Elements & EffectElement.Thunder) != 0)
                        ElementText += (!String.IsNullOrEmpty(ElementText) ? " / " : "") + $"[D6D62D][HSHD]{FF9TextTool.BattleFollowText(2)}[383838][HSHD]";
                    if (((EffectElement)ability.Ref.Elements & EffectElement.Earth) != 0)
                        ElementText += (!String.IsNullOrEmpty(ElementText) ? " / " : "") + $"[783F04][HSHD]{FF9TextTool.BattleFollowText(3)}[383838][HSHD]";
                    if (((EffectElement)ability.Ref.Elements & EffectElement.Aqua) != 0)
                        ElementText += (!String.IsNullOrEmpty(ElementText) ? " / " : "") + $"[5C5CFF][HSHD]{FF9TextTool.BattleFollowText(4)}[383838][HSHD]";
                    if (((EffectElement)ability.Ref.Elements & EffectElement.Wind) != 0)
                        ElementText += (!String.IsNullOrEmpty(ElementText) ? " / " : "") + $"[2C623A][HSHD]{FF9TextTool.BattleFollowText(5)}[383838][HSHD]";
                    if (((EffectElement)ability.Ref.Elements & EffectElement.Holy) != 0)
                        ElementText += (!String.IsNullOrEmpty(ElementText) ? " / " : "") + $"[FFFFFF][HSHD]{FF9TextTool.BattleFollowText(6)}[383838][HSHD]";
                    if (((EffectElement)ability.Ref.Elements & EffectElement.Darkness) != 0)
                        ElementText += (!String.IsNullOrEmpty(ElementText) ? " / " : "") + $"[000000][HSHD]{FF9TextTool.BattleFollowText(7)}[383838][HSHD]";
                }
                Description = Regex.Replace(Description, "=ELEMENT=", ElementText);
            }
            if ((FF9BattleDB.StatusSets.TryGetValue(ability.AddStatusNo, out BattleStatusEntry stat) ? stat.Value : 0) > 0 && result.Key != "ApplyStatusScript")
            {
                if (!String.IsNullOrEmpty(Description))
                    Description += " ";

                Description += $"{Localization.GetWithDefault("ApplyStatusBisScript")}";
            }
            if (Description.Contains("=STATUS="))
            {
                string StatusText = "";
                BattleStatus status = stat.Value;
                foreach (BattleStatusId statusId in status.ToStatusList())
                {
                    StatusText += ($"[A85038][HSHD]{Localization.GetWithDefault("Status"+statusId.ToString())}[383838][HSHD]");
                    if (BattleHUD.BuffIconNames.TryGetValue(statusId, out String buffspriteName))
                        StatusText += ($" [SPRT={buffspriteName},48,48] ");
                    if (BattleHUD.DebuffIconNames.TryGetValue(statusId, out String debuffspriteName))
                        StatusText += ($" [SPRT={debuffspriteName},48,48] ");
                }
                if (ability.Ref.Rate > 0)
                    StatusText += $" ({ability.Ref.Rate}%)";
                Description = Regex.Replace(Description, "=STATUS=", StatusText);
            }
            if (Description.Contains("=TYPE="))
            {
                string TypeText = "";
                if ((ability.Category & 8) != 0) // Physical
                {
                    TypeText += Localization.GetWithDefault("AADesc_Physical");
                    //TypeText += "[ICON=95] ";
                }
                if ((ability.Category & 16) != 0) // Magical
                {
                    TypeText += Localization.GetWithDefault("AADesc_Magical");
                    //TypeText += "[ICON=102] ";
                }
                Description = Regex.Replace(Description, "=TYPE=", TypeText);
            }
            if (Description.Contains("=TARGET="))
            {
                string TargetText = "";
                TargetText = Localization.GetWithDefault($"AADesc_{ability.Info.Target}");
                if (string.IsNullOrEmpty(TargetText)) // By Default
                    TargetText = Localization.GetWithDefault("AADesc_SingleAny");

                Description = Regex.Replace(Description, "=TARGET=", TargetText);
            }
            if (Description.Contains("=DAMAGE="))
            {
                if (result.Key == "ThousandNeedlesScript")
                    Description = Regex.Replace(Description, "=DAMAGE=", (ability.Ref.Power * 100 + ability.Ref.Rate).ToString());
            }
            if (Description.Contains("=STRENGTH="))
                Description = Regex.Replace(Description, "=STRENGTH=", Localization.Get("Strength"));
            if (Description.Contains("=MAGIC="))
                Description = Regex.Replace(Description, "=MAGIC=", Localization.Get("Magic"));
            if (Description.Contains("=ITEM="))
                Description = Regex.Replace(Description, "=ITEM=", FF9TextTool.ItemName((RegularItem)ability.Ref.Power));

            if (result.Key == "DoubleCastScript")
            {
                Description = Regex.Replace(Description, "=SPELL1=", FF9TextTool.ActionAbilityName((BattleAbilityId)ability.Ref.Power));
                Description = Regex.Replace(Description, "=SPELL2=", FF9TextTool.ActionAbilityName((BattleAbilityId)ability.Ref.Rate));
            }

            return Description;
        }
    }
}
