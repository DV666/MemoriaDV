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
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            _v.Caster.AlterStatus(TranceSeekCustomAPI.CustomStatus.MagicUp, _v.Caster);
            _v.Target.AlterStatus(TranceSeekCustomAPI.CustomStatus.MagicBreak, _v.Caster);
            TranceSeekCustomAPI.SpecialSA(_v);
        }
    }
}
