using System;
using FF9;

namespace Memoria.Scripts.Battle
{
    [BattleScript(Id)]
    public sealed class FearScript : IBattleScript
    {
        public const Int32 Id = 0110;

        private readonly BattleCalculator _v;

        public FearScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            byte trancefear = (byte)(Comn.random16() % ((_v.Caster.Will * 2) - _v.Target.Will));
            if (trancefear < _v.Target.Trance)
            {
                _v.Target.Trance -= trancefear;
            }
            else
            {
                _v.Target.Trance = 0;
            }
            if (_v.Command.Power > 0)
            {
                _v.NormalMagicParams();
                TranceSeekAPI.CharacterBonusPassive(_v, "MagicAttack");
                TranceSeekAPI.CasterPenaltyMini(_v);
                TranceSeekAPI.EnemyTranceBonusAttack(_v);
                TranceSeekAPI.PenaltyShellAttack(_v);
                TranceSeekAPI.PenaltyCommandDividedAttack(_v);
                TranceSeekAPI.BonusElement(_v);
                if (TranceSeekAPI.CanAttackMagic(_v))
                {
                    _v.CalcHpDamage();
                }
                TranceSeekAPI.TryAlterMagicStatuses(_v);
            }
            else if (_v.Command.HitRate > 0 && _v.Command.Power == 0)
            {
                TranceSeekAPI.MagicAccuracy(_v);
                _v.Target.PenaltyShellHitRate();
                _v.PenaltyCommandDividedHitRate();
                if (TranceSeekAPI.TryMagicHit(_v))
                {
                    TranceSeekAPI.TryAlterCommandStatuses(_v);
                }
            }
        }
    }
}
