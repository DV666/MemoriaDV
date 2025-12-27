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
    public sealed class SpecialApplyPositiveStatusScript : IBattleScript
    {
        public const Int32 Id = 0163;

        private readonly BattleCalculator _v;

        public SpecialApplyPositiveStatusScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Command.Power == 11 && _v.Command.HitRate == 11 && _v.Caster.Data.dms_geo_id == 556)
            {
                _v.Command.AbilityStatus |= BattleStatus.Reflect;
                btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.PerfectDodge, parameters: $"+2");
                TranceSeekAPI.TryAlterCommandStatuses(_v);
            }
        }
    }
}
