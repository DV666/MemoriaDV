using System;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Iai Strike
    /// </summary>
    [BattleScript(Id)]
    public sealed class IaiStrikeScript : IBattleScript
    {
        public const Int32 Id = 0108;

        private readonly BattleCalculator _v;

        public IaiStrikeScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            _v.Command.Power = _v.Command.Power * (_v.Target.PhysicalDefence / 100);
            if (_v.Command.Power < 1)
            {
                _v.Command.Power = 1;
            }
            _v.WeaponPhysicalParams();
            _v.Context.DefensePower = _v.Context.DefensePower / _v.Command.Power;
            _v.Caster.EnemyTranceBonusAttack();
            _v.Caster.PhysicalPenaltyAndBonusAttack();
            TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
            TranceSeekCustomAPI.BonusElement(_v);
            if (_v.CanAttackWeaponElementalCommand())
            {
                _v.CalcHpDamage();
                _v.TryAlterMagicStatuses();
            }
        }
    }
}
