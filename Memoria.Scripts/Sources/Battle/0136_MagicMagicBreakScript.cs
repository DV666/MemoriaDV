using System;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Magic Break
    /// </summary>
    [BattleScript(Id)]
    public sealed class MagicMagicBreakScript : IBattleScript, IEstimateBattleScript
    {
        public const Int32 Id = 0136;

        private readonly BattleCalculator _v;

        public MagicMagicBreakScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
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
                TranceSeekAPI.RaiseTrouble(_v);
            }
            _v.Command.AbilityStatus |= TranceSeekStatus.MagicBreak;
            TranceSeekAPI.TryAlterMagicStatuses(_v);
        }

        public Single RateTarget()
        {
            _v.NormalMagicParams();
            TranceSeekAPI.CharacterBonusPassive(_v, "MagicAttack");
            TranceSeekAPI.CasterPenaltyMini(_v);
            TranceSeekAPI.PenaltyShellAttack(_v);
            TranceSeekAPI.PenaltyCommandDividedAttack(_v);
            TranceSeekAPI.BonusElement(_v);

            if (!TranceSeekAPI.CanAttackMagic(_v))
                return 0;

            if (_v.Target.IsUnderAnyStatus(BattleStatus.Reflect) && !_v.Command.IsReflectNull)
                return 0;

            _v.CalcHpDamage();

            Single rate = Math.Min(_v.Target.HpDamage, _v.Target.CurrentHp);

            if ((_v.Target.Flags & CalcFlag.HpRecovery) == CalcFlag.HpRecovery)
                rate *= -1;
            if (_v.Target.IsPlayer)
                rate *= -1;

            return rate;
        }
    }
}
