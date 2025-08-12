using System;
using System.Runtime.Remoting.Contexts;
using FF9;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Spear
    /// </summary>
    [BattleScript(Id)]
    public sealed class SpearScript : IBattleScript
    {
        public const Int32 Id = 0048;

        private readonly BattleCalculator _v;

        public SpearScript(BattleCalculator v)
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

                if (_v.Caster.HasSupportAbility(SupportAbility1.HighJump) && GameRandom.Next8() % 2 == 0 || _v.Caster.HasSupportAbilityByIndex((SupportAbility)1021))
                {
                    _v.Target.AlterStatus(TranceSeekStatus.Dragon, _v.Caster);
                }
                if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)217)) // SA Skydive
                {
                    int num = Comn.random16() % (1 + (_v.Caster.Level + _v.Caster.Strength >> 3));
                    _v.Context.AttackPower = _v.Caster.WeaponPower;
                    _v.Context.Attack = ((short)(_v.Caster.Strength + num));
                    _v.Context.DefensePower = _v.Target.MagicDefence / 2;
                    TranceSeekAPI.PenaltyShellAttack(_v);
                    if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1217)) // SA Skydive+
                        _v.Caster.AlterStatus(TranceSeekStatus.MagicUp, _v.Caster);
                }
                else
                {
                    int num = Comn.random16() % (1 + (_v.Caster.Level + _v.Caster.Strength >> 3));
                    _v.Context.AttackPower = _v.Caster.WeaponPower;
                    _v.Context.Attack = ((short)(_v.Caster.Strength + num));
                    _v.Context.DefensePower = _v.Target.PhysicalDefence / 2; // [TODO] Change maybe with this formula ? => Math.Max(1, (_v.Target.PhysicalDefence / 2) - _v.Caster.Level + _v.Target.Level)
                    TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                }

                _v.BonusKillerAbilities();
                TranceSeekAPI.CasterPenaltyMini(_v);
                TranceSeekAPI.EnemyTranceBonusAttack(_v);
                TranceSeekAPI.BonusWeaponElement(_v);
                if (TranceSeekAPI.CanAttackWeaponElementalCommand(_v))
                {
                    TranceSeekAPI.IpsenCastleMalus(_v);
                    TranceSeekAPI.RaiseTrouble(_v);
                    _v.CalcPhysicalHpDamage();
                }

                if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)216)) // SA Sky Attack 
                {
                    _v.Caster.Flags |= CalcFlag.HpDamageOrHeal;
                    _v.Caster.HpDamage = _v.Target.HpDamage / (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1216) ? 4 : 8);
                }
            }
        }
    }
}
