using Memoria.Data;
using System;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// LV3 Def-less
    /// </summary>
    [BattleScript(Id)]
    public sealed class LvReduceDefenceScript : IBattleScript
    {
        public const Int32 Id = 0024;

        private readonly BattleCalculator _v;

        public LvReduceDefenceScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            if (_v.Command.Power == 0)
            {
                if (_v.IsTargetLevelMultipleOfCommandRate() && _v.Target.CanBeAttacked() && !_v.Target.Data.special_status_old)
                {
                    if (_v.Target.PhysicalDefence > 0)
                    {
                        _v.Target.PhysicalDefence = _v.Target.PhysicalDefence / 2;
                    }
                    if (_v.Target.MagicDefence > 0)
                    {
                        _v.Target.MagicDefence = _v.Target.MagicDefence / 2;
                    }
                    if (_v.Target.Strength > 0)
                    {
                        _v.Target.Strength = (byte)(_v.Target.Strength / 2);
                    }
                    if (_v.Target.Magic > 0)
                    {
                        _v.Target.Magic = (byte)(_v.Target.Magic / 2);
                    }
                    if (_v.Target.Will > 0)
                    {
                        _v.Target.Will = (byte)(_v.Target.Will / 2);
                    }
                    if (_v.Target.PhysicalEvade > 0)
                    {
                        _v.Target.PhysicalEvade = _v.Target.PhysicalEvade / 2;
                    }
                    if (_v.Target.MagicEvade > 0)
                    {
                        _v.Target.MagicEvade = _v.Target.MagicEvade / 2;
                    }
                    _v.Target.Data.special_status_old = true;
                }
                else
                {
                    _v.Context.Flags |= BattleCalcFlags.Miss;
                }
            }
            else
            {
                if (_v.Caster.IsPlayer)
                {
                    _v.OriginalMagicParams();
                }
                else
                {
                    _v.NormalMagicParams();
                }
                _v.Caster.PenaltyMini();
                _v.Target.PenaltyShellAttack();
                _v.PenaltyCommandDividedAttack();
                _v.BonusElement();
                if (TranceSeekCustomAPI.CanAttackMagic(_v))
                {
                    _v.CalcHpDamage();
                }
                _v.TryAlterMagicStatuses();
                _v.Target.PhysicalDefence = _v.Target.PhysicalDefence / 2;
                _v.Target.MagicDefence = _v.Target.MagicDefence / 2;
            }
            TranceSeekCustomAPI.SpecialSA(_v);
        }
    }
}