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
                    _v.Command.AbilityStatus |= TranceSeekCustomAPI.CustomStatus.Vieillissement;
                    TranceSeekCustomAPI.TryAlterCommandStatuses(_v);
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
                TranceSeekCustomAPI.CasterPenaltyMini(_v);
                _v.Target.PenaltyShellAttack();
                TranceSeekCustomAPI.PenaltyCommandDividedAttack(_v);
                TranceSeekCustomAPI.BonusElement(_v);
                if (TranceSeekCustomAPI.CanAttackMagic(_v))
                {
                    _v.CalcHpDamage();
                }
                TranceSeekCustomAPI.TryAlterMagicStatuses(_v);
                btl_stat.AlterStatus(_v.Target, TranceSeekCustomAPI.CustomStatusId.ArmorBreak, parameters: "+2");
                btl_stat.AlterStatus(_v.Target, TranceSeekCustomAPI.CustomStatusId.MentalBreak, parameters: "+2");
            }
        }
    }
}
