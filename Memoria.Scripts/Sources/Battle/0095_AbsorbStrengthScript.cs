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
            _v.Caster.AlterStatus(TranceSeekStatus.PowerUp, _v.Caster);
            _v.Target.AlterStatus(TranceSeekStatus.PowerBreak, _v.Caster);
        }
    }
}
