using System;
using Memoria.Data;

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
            if (_v.Command.AbilityId == BattleAbilityId.Luna)
            {
                btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.Dragon, _v.Caster);
                return;
            }
            else
            {
                if (_v.Command.AbilityId == BattleAbilityId.MatraMagic || _v.Command.AbilityId == (BattleAbilityId)1030 || _v.Command.HitRate == 255) // Matra Magic
                {
                    _v.Context.Attack = (short)(GameRandom.Next16() % (_v.Caster.Magic + _v.Caster.Level));
                    _v.SetCommandPower();
                    TranceSeekAPI.CasterPenaltyMini(_v);
                    TranceSeekAPI.PenaltyShellAttack(_v);
                    TranceSeekAPI.PenaltyCommandDividedAttack(_v);
                    TranceSeekAPI.BonusElement(_v);
                    if (TranceSeekAPI.CanAttackMagic(_v))
                    {
                        int num = GameRandom.Next16() % 8;
                        if (num == 0)
                        {
                            _v.Target.CanAttackElement(EffectElement.Cold);
                        }
                        else if (num == 1)
                        {
                            _v.Target.CanAttackElement(EffectElement.Fire);
                        }
                        else if (num == 2)
                        {
                            _v.Target.CanAttackElement(EffectElement.Thunder);
                        }
                        else if (num == 3)
                        {
                            if (_v.Target.IsLevitate)
                            {
                                _v.Context.Flags |= BattleCalcFlags.Miss;
                                return;
                            }
                            _v.Target.CanAttackElement(EffectElement.Earth);
                        }
                        else if (num == 4)
                        {
                            _v.Target.CanAttackElement(EffectElement.Aqua);
                        }
                        else if (num == 5)
                        {
                            _v.Target.CanAttackElement(EffectElement.Wind);
                        }
                        else if (num == 6)
                        {
                            _v.Target.CanAttackElement(EffectElement.Holy);
                        }
                        else
                        {
                            _v.Target.CanAttackElement(EffectElement.Darkness);
                        }
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
}
