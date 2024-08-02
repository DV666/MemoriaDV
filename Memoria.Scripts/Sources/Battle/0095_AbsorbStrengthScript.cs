using System;
using System.Collections.Generic;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Absorb Strength
    /// </summary>
    [BattleScript(Id)]
    public sealed class AbsorbStrengthScript : IBattleScript
    {
        public const Int32 Id = 0095;

        private readonly BattleCalculator _v;

        public AbsorbStrengthScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            _v.Caster.AlterStatus(TranceSeekCustomAPI.CustomStatus.PowerUp, _v.Caster);
            _v.Target.AlterStatus(TranceSeekCustomAPI.CustomStatus.PowerBreak, _v.Caster);
            TranceSeekCustomAPI.SpecialSA(_v);
        }
    }
}
