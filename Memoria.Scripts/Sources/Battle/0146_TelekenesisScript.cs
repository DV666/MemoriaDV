using Memoria.Data;
using Memoria.Prime;
using System;
using System.Runtime.Remoting.Contexts;

namespace Memoria.Scripts.TranceSeek
{
    /// <summary>
    /// Demi, Aqua Breath, Demi Shock, Worm Hole
    /// </summary>
    [BattleScript(Id)]
    public sealed class TelekenesisScript : IBattleScript
    {
        public const Int32 Id = 0146;

        private readonly BattleCalculator _v;

    public TelekenesisScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            _v.NormalMagicParams();

            TranceSeekAPI.CasterPenaltyMini(_v);
            TranceSeekAPI.PenaltyShellAttack(_v);
            TranceSeekAPI.PenaltyCommandDividedAttack(_v);
            TranceSeekAPI.BonusElement(_v);
            if (TranceSeekAPI.CanAttackMagic(_v))
            {
                if (_v.Target.IsLevitate) // (x 3 against Flying)
                    _v.Context.DamageModifierCount += 8;
                _v.CalcHpDamage();
                TranceSeekAPI.RaiseTrouble(_v);
            }
            TranceSeekAPI.TryAlterMagicStatuses(_v);
        }
    }
}
