using Assets.Sources.Scripts.UI.Common;
using Memoria.Assets;
using Memoria.Data;
using Memoria.Prime;
using System;
using System.Collections.Generic;
using static Memoria.IOverloadPlayerUIScript;

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
            TranceSeekAPI.ViviPreviousSpell[_v.Target.Data] = _v.Command.AbilityId;

            if (_v.Target.IsPlayer)
            {
                int ID = 2000 + (int)_v.Target.PlayerIndex;
                if (!FF9StateSystem.EventState.gScriptDictionary.TryGetValue(ID, out Dictionary<Int32, Int32> dict))
                {
                    dict = new Dictionary<Int32, Int32>();
                    FF9StateSystem.EventState.gScriptDictionary.Add(ID, dict);
                }
                dict[0] = 1;
                dict[1] = (int)_v.Command.AbilityId;

                string ElementText = "";
                string StatusText = "";
                if (_v.Command.AbilityId == (BattleAbilityId)1074 || _v.Command.Element > 0)
                    ElementText = InfusedElementCMD[_v.Command.Element];

                if (_v.Command.Element > 0 && String.IsNullOrEmpty(ElementText))
                    ElementText = "[FF16F4]";

                String spriteName;
                foreach (BattleStatusId infusedstatus in _v.Command.AbilityStatus.ToStatusList())
                    if (BattleHUD.BuffIconNames.TryGetValue(infusedstatus, out spriteName) || BattleHUD.DebuffIconNames.TryGetValue(infusedstatus, out spriteName))
                        StatusText += $" [SPRT={spriteName},48,48]";
              
                FF9TextTool.SetCommandName((BattleCommandId)ID, ElementText + InfusedAttackCMDName[Localization.CurrentSymbol] + StatusText);
            }
        }

        public Dictionary<String, String> InfusedAttackCMDName = new Dictionary<String, String>
        {
            { "US", "Attack" },
            { "UK", "Attack" },
            { "JP", "付与された攻撃" },
            { "ES", "Atacar" },
            { "FR", "Attaquer" },
            { "GR", "Angriff" },
            { "IT", "Attacca" },
        };

        public Dictionary<EffectElement, String> InfusedElementCMD = new Dictionary<EffectElement, String>
        {
            { EffectElement.Fire, "[DF0000]" },
            { EffectElement.Cold, "[028E8E]" },
            { EffectElement.Thunder, "[D6D62D]" },
            { EffectElement.Aqua, "[5C5CFF]" },
            { EffectElement.Wind, "[2C623A]" },
            { EffectElement.Earth, "[783F04]" },
            { EffectElement.Holy, "[FFFFB8]" },
            { EffectElement.Darkness, "[000000]" },
            { EffectElement.None, "[A85038]" }
        };
    }
}
