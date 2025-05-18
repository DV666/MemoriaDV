using System;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Lancer
    /// </summary>
    [BattleScript(Id)]
    public sealed class MagicLancerScript : IBattleScript
    {
        public const Int32 Id = 0115;

        private readonly BattleCalculator _v;

        public MagicLancerScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Target.MagicDefence == 255)
            {
                _v.Context.Flags |= BattleCalcFlags.Guard;
            }
            else
            {
                _v.NormalMagicParams();
                TranceSeekAPI.CasterPenaltyMini(_v);
                TranceSeekAPI.EnemyTranceBonusAttack(_v);
                TranceSeekAPI.PenaltyShellAttack(_v);
                TranceSeekAPI.PenaltyCommandDividedAttack(_v);
                TranceSeekAPI.BonusElement(_v);
                if (TranceSeekAPI.CanAttackMagic(_v))
                {
                    _v.Target.Flags |= (CalcFlag.HpAlteration | CalcFlag.MpAlteration);
                    if (_v.Context.IsAbsorb)
                    {
                        _v.Target.Flags = (CalcFlag.HpDamageOrHeal);
                    }
                    _v.CalcHpDamage();
                    int hpDamage2 = _v.Target.HpDamage;
                    if ((_v.Target.Flags & CalcFlag.HpRecovery) != 0)
                    {
                        _v.Target.FaceTheEnemy();
                    }
                    _v.Target.MpDamage = hpDamage2 >> 4;
                    if (!_v.Target.IsZombie && !_v.Context.IsAbsorb)
                        _v.Target.MpDamage = hpDamage2 >> 4;
                }
                TranceSeekAPI.TryAlterMagicStatuses(_v);
            }
        }
    }
}
