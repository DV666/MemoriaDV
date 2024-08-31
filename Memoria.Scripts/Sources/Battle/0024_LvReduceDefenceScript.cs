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
            if (_v.Command.Power == 0)
            {
                if (_v.IsTargetLevelMultipleOfCommandRate() && _v.Target.CanBeAttacked())
                {
                    _v.Target.AlterStatus(TranceSeekCustomAPI.CustomStatus.Vieillissement);
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
                TranceSeekCustomAPI.PenaltyCommandDividedAttack(_v);
                TranceSeekCustomAPI.BonusElement(_v);
                if (TranceSeekCustomAPI.CanAttackMagic(_v))
                {
                    _v.CalcHpDamage();
                }
                _v.TryAlterMagicStatuses();
                _v.Target.PhysicalDefence = _v.Target.PhysicalDefence / 2;
                _v.Target.MagicDefence = _v.Target.MagicDefence / 2;
            }
        }
    }
}
