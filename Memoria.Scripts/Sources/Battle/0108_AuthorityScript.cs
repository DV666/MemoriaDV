using Memoria.Prime;
using System;
using System.Runtime.Remoting.Contexts;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Iai Strike
    /// </summary>
    [BattleScript(Id)]
    public sealed class AuthorityScript : IBattleScript
    {
        public const Int32 Id = 0108;

        private readonly BattleCalculator _v;

        public AuthorityScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            float RatioDamage = (float)(_v.Command.Power + _v.Caster.Level - _v.Target.PhysicalDefence - _v.Target.Level) / 5;
            TranceSeekAPI.WeaponPhysicalParams(CalcAttackBonus.Simple, _v);
            _v.Caster.SetLowPhysicalAttack();
            if (RatioDamage < 0)
            {
                RatioDamage = -RatioDamage;
                _v.Context.DefensePower = (_v.Target.PhysicalDefence / 10);
                float NewContextAttack = (float)(Math.Min(2, RatioDamage) * _v.Context.Attack);
                _v.Context.Attack = (int)(NewContextAttack);
            }
            else
            {
                _v.Context.DamageModifierCount--; // Little Malus if condition is not respected.
            }
            TranceSeekAPI.EnemyTranceBonusAttack(_v);
            TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
            TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
            TranceSeekAPI.BonusElement(_v);
            if (TranceSeekAPI.CanAttackWeaponElementalCommand(_v))
            {
                _v.CalcHpDamage();
                TranceSeekAPI.TryAlterMagicStatuses(_v);
            }
        }
    }
}
