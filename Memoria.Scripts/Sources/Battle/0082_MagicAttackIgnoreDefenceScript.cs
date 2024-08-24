using System;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    [BattleScript(Id)]
    public sealed class MagicAttackIgnoreDefenceScript : IBattleScript, IEstimateBattleScript
    {
        public const Int32 Id = 0082;

        private readonly BattleCalculator _v;

        public MagicAttackIgnoreDefenceScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Target.MagicDefence == 255)
            {
                _v.Context.Flags |= BattleCalcFlags.Guard;
                return;
            }
            _v.SetCommandPower();
            _v.Caster.SetMagicAttack();
            _v.Caster.PenaltyMini();
            _v.Caster.EnemyTranceBonusAttack();
            TranceSeekCustomAPI.PenaltyShellAttack(_v);
            TranceSeekCustomAPI.PenaltyCommandDividedAttack(_v);
            _v.BonusElement();
            _v.CalcHpDamage();
            _v.TryAlterMagicStatuses();
        }

        public Single RateTarget()
        {
            _v.NormalMagicParams();
            TranceSeekCustomAPI.CharacterBonusPassive(_v, "MagicAttack");
            _v.Caster.PenaltyMini();
            TranceSeekCustomAPI.PenaltyShellAttack(_v);
            TranceSeekCustomAPI.PenaltyCommandDividedAttack(_v);
            _v.BonusElement();

            if (!TranceSeekCustomAPI.CanAttackMagic(_v))
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
