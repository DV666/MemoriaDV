using System;
using Memoria.Data;
using Memoria.Prime;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Matra Magic, Blue Shockwave, Judgment Sword, Helm Divide
    /// </summary>
    [BattleScript(Id)]
    public sealed class DirectHPDamageScript : IBattleScript
    {
        public const Int32 Id = 0027;

        private readonly BattleCalculator _v;

        public DirectHPDamageScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Command.AbilityId == BattleAbilityId.MatraMagic || _v.Command.AbilityId == (BattleAbilityId)1030 || _v.Command.HitRate == 255) // Matra Magic
            {
                _v.Context.Attack = (short)(GameRandom.Next16() % (_v.Caster.Magic + _v.Caster.Level));
                _v.SetCommandPower();
                _v.Command.Element = (EffectElement)(1 << GameRandom.Next16() % 8);
                TranceSeekAPI.CasterPenaltyMini(_v);
                TranceSeekAPI.PenaltyShellAttack(_v);
                TranceSeekAPI.PenaltyCommandDividedAttack(_v);
                TranceSeekAPI.BonusElement(_v);
                if (TranceSeekAPI.CanAttackMagic(_v))
                {
                    _v.CalcHpDamage();
                }
                TranceSeekAPI.TryAlterMagicStatuses(_v);
            }
            else
            {
                if (TranceSeekAPI.CheckUnsafetyOrGuard(_v) && _v.Target.CanBeAttacked())
                {
                    TranceSeekAPI.MagicAccuracy(_v);
                    _v.Target.PenaltyShellHitRate();
                    if (TranceSeekAPI.TryMagicHit(_v))
                    {
                        _v.TryDirectHPDamage();
                    }
                }
            }
        }
    }
}
