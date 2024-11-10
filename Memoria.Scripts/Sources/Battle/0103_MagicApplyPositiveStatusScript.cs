using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Regen, Shell, Protect, Haste, Reflect, Float, Carbuncle, Mighty Guard, Vanish, Auto-Life, Reis’s Wind, Luna, Aura, Defend
    /// </summary>
    [BattleScript(Id)]
    public sealed class MagicApplyPositiveStatusScript : IBattleScript
    {
        public const Int32 Id = 0103;

        private readonly BattleCalculator _v;

        public MagicApplyPositiveStatusScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Command.AbilityId == BattleAbilityId.Defend) // Defense
            {
                if (_v.Caster.PlayerIndex == CharacterId.Beatrix)
                    _v.Caster.AlterStatus(TranceSeekCustomAPI.CustomStatus.Redemption);
            }

            if ( _v.Command.AbilityId == (BattleAbilityId)1104 && (_v.Caster.ResistStatus & BattleStatus.Doom) == 0) // Sang Maudit
            {
                btl_stat.AlterStatus(_v.Target, TranceSeekCustomAPI.CustomStatusId.PowerUp, parameters: $"+{_v.Command.Power}");
                btl_stat.AlterStatus(_v.Target, TranceSeekCustomAPI.CustomStatusId.MagicUp, parameters: $"+{_v.Command.Power}");
                btl_stat.AlterStatus(_v.Target, TranceSeekCustomAPI.CustomStatusId.ArmorUp, parameters: $"+{_v.Command.Power}");
                btl_stat.AlterStatus(_v.Target, TranceSeekCustomAPI.CustomStatusId.MentalUp, parameters: $"+{_v.Command.Power}");
                btl_stat.AlterStatus(_v.Target, TranceSeekCustomAPI.CustomStatusId.Special, parameters: "LifeorDeath++");
                btl_stat.MakeStatusesPermanent(_v.Target, BattleStatus.Doom, true);
                _v.Target.AddDelayedModifier(
                    target => !target.IsUnderAnyStatus(BattleStatus.Death),
                    target =>
                    {
                        btl_stat.MakeStatusesPermanent(target, BattleStatus.Doom, false);
                        btl_stat.AlterStatus(target, TranceSeekCustomAPI.CustomStatusId.Special, parameters: "LifeorDeath--");
                        btl_stat.RemoveStatus(target, BattleStatusId.Doom);
                    }
                );
                return;
            }

            if (_v.Command.AbilityId == (BattleAbilityId)1099) // Iron Clast
            {
                _v.Command.AbilityStatus |= TranceSeekCustomAPI.CustomStatus.ArmorUp;
            }
            else if (_v.Command.AbilityId == (BattleAbilityId)1007) // Rempart
            {
                _v.Command.AbilityStatus |= TranceSeekCustomAPI.CustomStatus.Bulwark;
            }
            else if (_v.Command.AbilityId == (BattleAbilityId)1059) // Runic
            {
                _v.Command.AbilityStatus |= TranceSeekCustomAPI.CustomStatus.Runic;
            }
            if (!_v.Target.IsPlayer)
            {
                if (_v.Command.Element == EffectElement.Earth && _v.Command.Power == 1)
                {
                    _v.Target.Flags |= (CalcFlag.HpAlteration | CalcFlag.HpRecovery);
                    _v.Target.HpDamage = (int)_v.Target.MaximumHp;
                    return;
                }
            }
            if (_v.Caster.Data.dms_geo_id == 125 && _v.Command.Power == 1) // Valseur 2 - Stasis
            {
                _v.Target.PhysicalDefence = 255;
                _v.Target.MagicDefence = 255;
                btl_stat.MakeStatusesPermanent(_v.Target, BattleStatus.Stop, true);
            }
            else if (_v.Caster.Data.dms_geo_id == 36 && _v.Command.Power == 1) // Silver Dragon - Shinryu's dance
            {
                _v.Caster.AlterStatus(BattleStatus.Regen);
                _v.Caster.AlterStatus(BattleStatus.Haste);
                if (_v.Caster.PhysicalEvade < 255)
                    _v.Caster.PhysicalEvade += 10;
                else
                    _v.Caster.PhysicalEvade = 255;
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
                btl2d.Btl2dReqSymbolMessage(_v.Caster.Data, "[FFC0CB]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 5);
            }
            else if (_v.Caster.Data.dms_geo_id == 412 && _v.Command.Power == 99) // Vin de Bacchus
            {
                btl_stat.AlterStatus(_v.Target, TranceSeekCustomAPI.CustomStatusId.PowerUp, parameters: "4");
            }
            TranceSeekCustomAPI.TryAlterCommandStatuses(_v);
            if (_v.Command.AbilityId == (BattleAbilityId)1137) // Spring Boots
            {
                TranceSeekCustomAPI.SpecialSAEffect[_v.Target.Data][8] = 1;
                _v.Context.Flags = 0;
            }
            if (_v.Command.AbilityId == (BattleAbilityId)1139) // Spring Boots
            {
                TranceSeekCustomAPI.SpecialSAEffect[_v.Target.Data][9] = 3;
                _v.Context.Flags = 0;
            }
        }
    }
}
