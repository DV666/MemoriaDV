using System;
using FF9;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Spear (mass)
    /// </summary>
    [BattleScript(Id)]
    public sealed class MassSpearScript : IBattleScript
    {
        public const Int32 Id = 0083;

        private readonly BattleCalculator _v;

        public MassSpearScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Target.PhysicalDefence == 255)
            {
                _v.Context.Flags |= BattleCalcFlags.Guard;
            }
            else if (_v.Target.IsUnderAnyStatus(BattleStatus.Vanish) || _v.Target.PhysicalEvade == 255)
            {
                _v.Context.Flags |= BattleCalcFlags.Miss;
                return;
            }
            else
            {
                int num = Comn.random16() % (1 + (_v.Caster.Level + _v.Caster.Strength >> 3));
                _v.Context.AttackPower = _v.Caster.WeaponPower;
                _v.Context.Attack = ((short)(_v.Caster.Strength + num));
                if (_v.Caster.HasSupportAbility(SupportAbility1.HighJump) && GameRandom.Next8() % 2 == 0 || _v.Caster.HasSupportAbilityByIndex((SupportAbility)1021))
                {
                    _v.Target.AlterStatus(TranceSeekCustomAPI.CustomStatus.Dragon, _v.Caster);
                }
                byte PhysicalDefence = (byte)_v.Target.PhysicalDefence;
                _v.Target.PhysicalDefence = (byte)(_v.Target.PhysicalDefence * 60 / 100);
                _v.Target.SetPhysicalDefense();
                _v.BonusSupportAbilitiesAttack();
                _v.Caster.PenaltyMini();
                TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                _v.Caster.BonusWeaponElement();
                if (_v.CanAttackWeaponElementalCommand())
                {
                    TranceSeekCustomAPI.IpsenCastleMalus(_v);
                    TranceSeekCustomAPI.RaiseTrouble(_v);
                    _v.CalcPhysicalHpDamage();
                }
                _v.Target.PhysicalDefence = PhysicalDefence;
            }
        }
    }
}
