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
                byte OldDefenceValue = _v.Caster.HasSupportAbilityByIndex((SupportAbility)217) ? (byte)_v.Target.MagicDefence : (byte)_v.Target.PhysicalDefence;
                if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)217)) // SA Skydive
                {
                    _v.Target.MagicDefence = (byte)(_v.Target.MagicDefence / (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1217) ? 2 : 1));
                    _v.Context.DefensePower = _v.Target.MagicDefence;
                }
                else
                {
                    _v.Target.PhysicalDefence = (byte)(_v.Target.PhysicalDefence / 2);
                    _v.Context.DefensePower = _v.Target.PhysicalDefence;
                }

                _v.BonusKillerAbilities();
                TranceSeekCustomAPI.CasterPenaltyMini(_v);
                TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                TranceSeekCustomAPI.BonusWeaponElement(_v);
                if (TranceSeekCustomAPI.CanAttackWeaponElementalCommand(_v))
                {
                    TranceSeekCustomAPI.IpsenCastleMalus(_v);
                    TranceSeekCustomAPI.RaiseTrouble(_v);
                    _v.CalcPhysicalHpDamage();
                    if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)216)) // SA Sky Attack 
                    {
                        _v.Caster.Flags |= CalcFlag.HpDamageOrHeal;
                        _v.Caster.HpDamage = _v.Target.HpDamage / (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1216) ? 2 : 4);
                    }
                }
                if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)217)) // SA Skydive
                    _v.Target.MagicDefence = OldDefenceValue;
                else
                    _v.Target.PhysicalDefence = OldDefenceValue;
            }
        }
    }
}
