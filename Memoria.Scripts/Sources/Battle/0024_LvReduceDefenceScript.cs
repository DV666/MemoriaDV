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
                    _v.Command.AbilityStatus |= TranceSeekStatus.Vieillissement;
                    TranceSeekAPI.TryAlterCommandStatuses(_v);
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
                TranceSeekAPI.CasterPenaltyMini(_v);
                _v.Target.PenaltyShellAttack();
                TranceSeekAPI.PenaltyCommandDividedAttack(_v);
                TranceSeekAPI.BonusElement(_v);
                if (TranceSeekAPI.CanAttackMagic(_v))
                {
                    _v.CalcHpDamage();
                }
                TranceSeekAPI.TryAlterMagicStatuses(_v);
                btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.ArmorBreak, parameters: "+2");
                btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.MentalBreak, parameters: "+2");
            }
        }
    }
}
