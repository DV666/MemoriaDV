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
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            if (_v.Caster.PlayerIndex == CharacterId.Freya && _v.Command.AbilityId == BattleAbilityId.Luna)
            {
                if (_v.Target.IsUnderAnyStatus(TranceSeekCustomAPI.CustomStatus.Dragon))
                {
                    _v.Context.Flags |= BattleCalcFlags.Miss;
                }
                else
                {
                    _v.Target.AlterStatus(TranceSeekCustomAPI.CustomStatus.Dragon);
                }
                return;
            }
            else
            {
                if (_v.Command.AbilityId == BattleAbilityId.MatraMagic || _v.Command.AbilityId == (BattleAbilityId)1030 || _v.Command.HitRate == 255) // Matra Magic
                {
                    _v.Context.Attack = (int)((short)(GameRandom.Next16() % (int)(_v.Caster.Magic + _v.Caster.Level)));
                    _v.SetCommandPower();
                    _v.Caster.PenaltyMini();
                    TranceSeekCustomAPI.PenaltyShellAttack(_v);
                    _v.PenaltyCommandDividedAttack();
                    _v.BonusElement();
                    if (TranceSeekCustomAPI.CanAttackMagic(_v))
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
                    _v.TryAlterMagicStatuses();
                }
                else
                {
                    if (_v.Target.CheckUnsafetyOrMiss() && _v.Target.CanBeAttacked())
                    {
                        _v.MagicAccuracy();
                        _v.Target.PenaltyShellHitRate();
                        if (_v.TryMagicHit())
                        {
                            _v.TryDirectHPDamage();
                        }
                    }
                }
            }
            TranceSeekCustomAPI.SpecialSA(_v);
        }
    }
}
