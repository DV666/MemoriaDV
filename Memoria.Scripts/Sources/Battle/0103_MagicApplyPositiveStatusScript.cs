using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using System.Security.Policy;
using Memoria.Data;
using Memoria.Prime;

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
            if (_v.Command.AbilityId == (BattleAbilityId)1538 || _v.Command.AbilityId == (BattleAbilityId)1539) // AA Idea + Eureka
            {
                if (TranceSeekAPI.ViviPassive[_v.Caster.Data][0] == 0)
                {
                    _v.Context.Flags |= BattleCalcFlags.Miss;
                    return;
                }

                List<BattleStatusId> statuslist = new List<BattleStatusId>{ BattleStatusId.Protect, BattleStatusId.Shell, BattleStatusId.Haste, BattleStatusId.Reflect, BattleStatusId.Regen,
                    BattleStatusId.AutoLife, BattleStatusId.Float, BattleStatusId.Vanish, TranceSeekStatusId.ArmorUp,
                    TranceSeekStatusId.MagicUp, TranceSeekStatusId.MentalUp, TranceSeekStatusId.PowerUp};

                BattleStatusId statusselected;
                while (TranceSeekAPI.ViviPassive[_v.Caster.Data][0] > 0)
                {
                    statusselected = statuslist[GameRandom.Next16() % statuslist.Count];
                    btl_stat.AlterStatus(_v.Target, statusselected, _v.Caster);
                    statuslist.Remove(statusselected);
                    TranceSeekAPI.ViviPassive[_v.Caster.Data][0]--;
                }
                return;
            }

            if (_v.Command.AbilityId == BattleAbilityId.Luna)
            {
                btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.Dragon, _v.Caster, parameters: "Add");
                return;
            }

            if ( _v.Command.AbilityId == (BattleAbilityId)1104 && (_v.Caster.ResistStatus & BattleStatus.Doom) == 0) // Sang Maudit
            {
                btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.PowerUp, parameters: $"+{_v.Command.Power}");
                btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.MagicUp, parameters: $"+{_v.Command.Power}");
                btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.ArmorUp, parameters: $"+{_v.Command.Power}");
                btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.MentalUp, parameters: $"+{_v.Command.Power}");
                btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.Special, parameters: "LifeorDeath++");
                btl_stat.MakeStatusesPermanent(_v.Target, BattleStatus.Doom, true);
                _v.Target.AddDelayedModifier(
                    target => !target.IsUnderAnyStatus(BattleStatus.Death),
                    target =>
                    {
                        btl_stat.MakeStatusesPermanent(target, BattleStatus.Doom, false);
                        btl_stat.AlterStatus(target, TranceSeekStatusId.Special, parameters: "LifeorDeath--");
                        btl_stat.RemoveStatus(target, BattleStatusId.Doom);
                    }
                );
                return;
            }

            if (_v.Command.AbilityId == (BattleAbilityId)1099) // Iron Clast
            {
                _v.Command.AbilityStatus |= TranceSeekStatus.ArmorUp;
            }
            else if (_v.Command.AbilityId == (BattleAbilityId)1007) // Rempart
            {
                _v.Command.AbilityStatus |= TranceSeekStatus.Bulwark;
            }
            else if (_v.Command.AbilityId == (BattleAbilityId)1059) // Runic
            {
                _v.Command.AbilityStatus |= TranceSeekStatus.Runic;
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
                return;
            }
            else if (_v.Caster.Data.dms_geo_id == 36 && _v.Command.Power == 1) // Silver Dragon - Shinryu's dance
            {
                _v.Command.AbilityStatus |= (BattleStatus.Regen | BattleStatus.Haste);
                TranceSeekAPI.TryAlterCommandStatuses(_v);
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
                btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.PowerUp, parameters: "4");
            }
            TranceSeekAPI.TryAlterCommandStatuses(_v);
            if (_v.Command.AbilityId == (BattleAbilityId)1137) // Spring Boots
            {
                TranceSeekAPI.SpecialSAEffect[_v.Target.Data][8] = 1;
                _v.Context.Flags = 0;
            }
            if (_v.Command.AbilityId == (BattleAbilityId)1139) // Spring Boots
            {
                TranceSeekAPI.SpecialSAEffect[_v.Target.Data][9] = 3;
                _v.Context.Flags = 0;
            }
        }
    }
}
