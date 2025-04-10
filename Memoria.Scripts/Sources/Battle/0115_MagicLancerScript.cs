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
                TranceSeekCustomAPI.CasterPenaltyMini(_v);
                TranceSeekCustomAPI.EnemyTranceBonusAttack(_v);
                TranceSeekCustomAPI.PenaltyShellAttack(_v);
                TranceSeekCustomAPI.PenaltyCommandDividedAttack(_v);
                TranceSeekCustomAPI.BonusElement(_v);
                if (TranceSeekCustomAPI.CanAttackMagic(_v))
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
                TranceSeekCustomAPI.TryAlterMagicStatuses(_v);
            }
        }
    }
}
