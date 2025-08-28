using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Assets;
using Memoria.Data;
using Memoria.Prime;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Special
    /// </summary>
    [BattleScript(Id)]
    public sealed class InfusedWeaponScript : IBattleScript
    {
        public const Int32 Id = 0123;

        private readonly BattleCalculator _v;

        public InfusedWeaponScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            TranceSeekAPI.WeaponNewElement[_v.Target.Data] = _v.Command.Element;
            TranceSeekAPI.WeaponNewStatus[_v.Target.Data] = _v.Command.AbilityStatus;
            TranceSeekAPI.ViviPreviousSpell[_v.Target.Data] = _v.Command.AbilityId; // SA Maximum infusion for Lani/Vivi

            int Element = (int)_v.Command.Element;
            if (_v.Command.Element == 0 && _v.Command.AbilityId != TranceSeekBattleAbility.SFlare)
                Element = -1;

            if (_v.Command.AbilityId == TranceSeekBattleAbility.SPoison || _v.Command.AbilityId == TranceSeekBattleAbility.SBio)
                Element = 256;

            InfuseWeapon(_v, _v.Target.Data, Element, (int)_v.Command.AbilityStatus);
        }

        public static void InfuseWeapon(BattleCalculator v, BTL_DATA btl, int NewElement = -1, int NewStatus = -1)
        {
            if (NewElement == -1 && NewStatus == -1)
                return;

            EffectElement InfusedElement = 0;
            BattleStatus InfusedStatus = 0;

            if (NewElement != -1)
            {
                InfusedElement = (EffectElement)NewElement;
                TranceSeekAPI.WeaponNewElement[btl] = InfusedElement;
            }

            if (NewStatus != -1)
            {
                InfusedStatus = (BattleStatus)NewStatus;
                TranceSeekAPI.WeaponNewStatus[btl] = InfusedStatus;
            }

            if (btl.bi.player != 0)
            {
                int ID = 2000 + btl.bi.slot_no;
                if (!FF9StateSystem.EventState.gScriptDictionary.TryGetValue(ID, out Dictionary<Int32, Int32> dict))
                {
                    dict = new Dictionary<Int32, Int32>();
                    FF9StateSystem.EventState.gScriptDictionary.Add(ID, dict);
                }
                dict[0] = 1;

                string ElementText = "";
                string StatusText = "";
                string DescText = VanillaAttackCMDDesc[Localization.CurrentSymbol];
                string StatusDescText = "";

                if (InfusedElement > 0)
                    ElementText = InfusedElementCMD[InfusedElement];

                if (InfusedElement > 0 && String.IsNullOrEmpty(ElementText) && NewElement != 256 && NewElement != 512)
                {
                    ElementText = "[FF16F4]";
                    DescText += EffectElementCMDDesc[Localization.CurrentSymbol];
                    Regex.Replace(DescText, "=ELEMENT=", "[FF16F4][HSHD]Multi[383838][HSHD]");
                }
                else
                {
                    DescText += EffectElementCMDDesc[Localization.CurrentSymbol];
                    string ReplaceElementText = "";
                    if ((InfusedElement & EffectElement.Fire) != 0)
                        ReplaceElementText = $"{ElementText}[HSHD]{FF9TextTool.BattleFollowText(0)}[383838][HSHD]";
                    if ((InfusedElement & EffectElement.Cold) != 0)
                        ReplaceElementText = $"{ElementText}[HSHD]{FF9TextTool.BattleFollowText(1)}[383838][HSHD]";
                    if ((InfusedElement & EffectElement.Thunder) != 0)
                        ReplaceElementText = $"{ElementText}[HSHD]{FF9TextTool.BattleFollowText(2)}[383838][HSHD]";
                    if ((InfusedElement & EffectElement.Earth) != 0)
                        ReplaceElementText = $"{ElementText}[HSHD]{FF9TextTool.BattleFollowText(3)}[383838][HSHD]";
                    if ((InfusedElement & EffectElement.Aqua) != 0)
                        ReplaceElementText = $"{ElementText}[HSHD]{FF9TextTool.BattleFollowText(4)}[383838][HSHD]";
                    if ((InfusedElement & EffectElement.Wind) != 0)
                        ReplaceElementText = $"{ElementText}[HSHD]{FF9TextTool.BattleFollowText(5)}[383838][HSHD]";
                    if ((InfusedElement & EffectElement.Holy) != 0)
                        ReplaceElementText = $"{ElementText}[HSHD]{FF9TextTool.BattleFollowText(6)}[383838][HSHD]";
                    if ((InfusedElement & EffectElement.Darkness) != 0)
                        ReplaceElementText = $"{ElementText}[HSHD]{FF9TextTool.BattleFollowText(7)}[383838][HSHD]";
                    if ((InfusedElement & EffectElement.None) != 0)
                        ReplaceElementText = $"[A85038][HSHD]{NeutralEffectElementCMDDesc[Localization.CurrentSymbol]}[383838][HSHD]";

                    if (NewElement == 256) // Poison
                    {
                        TranceSeekAPI.WeaponNewCustomElement[btl] = 1;
                        ElementText = "[800080]";
                        DescText = Regex.Replace(DescText, "=ELEMENT=", PoisonEffectElementCMDDesc[Localization.CurrentSymbol]);
                    }
                    else if (NewElement == 512) // Gravity
                    {
                        TranceSeekAPI.WeaponNewCustomElement[btl] = 2;
                        ElementText = "[6E0C5C]";
                        DescText = Regex.Replace(DescText, "=ELEMENT=", GravityEffectElementCMDDesc[Localization.CurrentSymbol]);
                    }
                    else
                        DescText = Regex.Replace(DescText, "=ELEMENT=", ReplaceElementText);
                }

                if (InfusedStatus > 0)
                {
                    String spriteName;
                    foreach (BattleStatusId infusedstatus in InfusedStatus.ToStatusList())
                        if (BattleHUD.BuffIconNames.TryGetValue(infusedstatus, out spriteName) || BattleHUD.DebuffIconNames.TryGetValue(infusedstatus, out spriteName))
                        {
                            StatusText += $" [SPRT={spriteName},48,48]";
                            StatusDescText += $" [SPRT={spriteName},48,48]";
                        }

                    DescText += BattleStatusCMDDesc[Localization.CurrentSymbol];
                    DescText = Regex.Replace(DescText, "=STATUS=", StatusDescText);
                }

                FF9TextTool.SetCommandName((BattleCommandId)ID, ElementText + InfusedAttackCMDName[Localization.CurrentSymbol] + StatusText);
                FF9TextTool.SetCommandHelpDesc((BattleCommandId)ID, DescText);
            }
        }

        public static void ClearInfuseWeapon(BTL_DATA btl)
        {
            if (btl.bi.player != 0)
            {
                int ID = 2000 + btl.bi.slot_no;
                if (!FF9StateSystem.EventState.gScriptDictionary.TryGetValue(ID, out Dictionary<Int32, Int32> dict))
                {
                    dict = new Dictionary<Int32, Int32>();
                    FF9StateSystem.EventState.gScriptDictionary.Add(ID, dict);
                }
                dict[0] = 0;
            }
        }

        public static Dictionary<String, String> InfusedAttackCMDName = new Dictionary<String, String>
        {
            { "US", "Attack" },
            { "UK", "Attack" },
            { "JP", "付与された攻撃" },
            { "ES", "Atacar" },
            { "FR", "Attaquer" },
            { "GR", "Angriff" },
            { "IT", "Attacca" },
        };

        public static Dictionary<String, String> VanillaAttackCMDDesc = new Dictionary<String, String>
        {
            { "US", "Attack with equipped weapon." },
            { "UK", "Attack with equipped weapon." },
            { "JP", "装備している武器で攻撃します。" },
            { "ES", "Ataca con un arma." },
            { "FR", "Attaque basique." },
            { "GR", "Benutzt getragene\r\nWaffe und greift an." },
            { "IT", "Attacca con un’arma." },
        };

        public static Dictionary<String, String> EffectElementCMDDesc = new Dictionary<String, String>
        {
            { "US", "\r\nInfused with the =ELEMENT= element." },
            { "UK", "\r\nInfused with the =ELEMENT= element." },
            { "JP", "\r\n=ELEMENT=属性で付与されている" },
            { "ES", "\r\nInfundida con el elemento =ELEMENT=." },
            { "FR", "\r\nInfusée par l'élément =ELEMENT=." },
            { "GR", "\r\nDurchdrungen vom =ELEMENT=-Element." },
            { "IT", "\r\nInfusa con l'elemento =ELEMENT=." }
        };

        public static Dictionary<String, String> BattleStatusCMDDesc = new Dictionary<String, String>
        {
            { "US", "\r\nHas a chance to inflict =STATUS=." },
            { "UK", "\r\nHas a chance to inflict =STATUS=." },
            { "JP", "\r\n=STATUS=を付与する可能性がある" },
            { "ES", "\r\nTiene una probabilidad de infligir =STATUS=." },
            { "FR", "\r\nA une chance d'infliger =STATUS=." },
            { "GR", "\r\nHat eine Chance, =STATUS= zuzufügen." },
            { "IT", "\r\nHa una probabilità di infliggere =STATUS=." }
        };

        public static Dictionary<String, String> NeutralEffectElementCMDDesc = new Dictionary<String, String>
        {
            { "US", "Non-elemental" },
            { "UK", "Non-elemental" },
            { "JP", "無" },
            { "ES", "no elemental" },
            { "FR", "Neutre" },
            { "GR", "Neutral" },
            { "IT", "Non-elementali" }
        };

        public static Dictionary<String, String> PoisonEffectElementCMDDesc = new Dictionary<String, String>
        {
            { "US", "[800080][HSHD]Poison[383838][HSHD]" },
            { "UK", "[800080][HSHD]Poison[383838][HSHD]" },
            { "JP", "[800080][HSHD]毒[383838][HSHD]" },
            { "ES", "[800080][HSHD]Veneno[383838][HSHD]" },
            { "FR", "[800080][HSHD]Poison[383838][HSHD]" },
            { "GR", "[800080][HSHD]Gift[383838][HSHD]" },
            { "IT", "[800080][HSHD]Veleno[383838][HSHD]" }
        };

        public static Dictionary<String, String> GravityEffectElementCMDDesc = new Dictionary<String, String>
        {
            { "US", "[6E0C5C][HSHD]Gravity[383838][HSHD]" },
            { "UK", "[6E0C5C][HSHD]Gravity[383838][HSHD]" },
            { "JP", "[6E0C5C][HSHD]重力[383838][HSHD]" },
            { "ES", "[6E0C5C][HSHD]Gravedad[383838][HSHD]" },
            { "FR", "[6E0C5C][HSHD]Gravité[383838][HSHD]" },
            { "GR", "[6E0C5C][HSHD]Gravitation[383838][HSHD]" },
            { "IT", "[6E0C5C][HSHD]Gravità[383838][HSHD]" }
        };

        public static Dictionary<EffectElement, String> InfusedElementCMD = new Dictionary<EffectElement, String>
        {
            { EffectElement.Fire, "[DF0000]" },
            { EffectElement.Cold, "[1CFFFF]" },
            { EffectElement.Thunder, "[D6D62D]" },
            { EffectElement.Aqua, "[4445FF]" },
            { EffectElement.Wind, "[0AFF0E]" },
            { EffectElement.Earth, "[783F04]" },
            { EffectElement.Holy, "[FFFFB8]" },
            { EffectElement.Darkness, "[000000]" },
            { EffectElement.None, "[A85038]" }
        };
    }
}
