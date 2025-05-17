using System;
using System.Collections.Generic;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Absorb Magic
    /// </summary>
    [BattleScript(Id)]
    public sealed class AbsorbMagicScript : IBattleScript
    {
        public const Int32 Id = 0094;

        private readonly BattleCalculator _v;

        public AbsorbMagicScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            _v.Caster.AlterStatus(TranceSeekStatus.MagicUp, _v.Caster);
            _v.Target.AlterStatus(TranceSeekStatus.MagicBreak, _v.Caster);
        }
    }
}
