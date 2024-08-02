using System;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Darkside
    /// </summary>
    [BattleScript(Id)]
    public sealed class DarksideScript : IBattleScript
    {
        public const Int32 Id = 0032;

        private readonly BattleCalculator _v;

        public DarksideScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            _v.PhysicalAccuracy();
            if (_v.Command.AbilityId != BattleAbilityId.Darkside)
            {
                if (TranceSeekCustomAPI.TryPhysicalHit(_v))
                {
                    _v.Context.Flags |= BattleCalcFlags.Miss;
                    TranceSeekCustomAPI.SpecialSA(_v);
                    return;
                }
            }

            if (_v.Caster.IsPlayer)
            {
                _v.WeaponPhysicalParams();
            }
            else
            {
                _v.NormalPhysicalParams();
            }
            TranceSeekCustomAPI.CharacterBonusPassive(_v, "PhysicalAttack");
            _v.Caster.PhysicalPenaltyAndBonusAttack();
            TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
            TranceSeekCustomAPI.BonusBackstabAndPenaltyLongDistanceTranceSeek(_v);
            _v.Caster.EnemyTranceBonusAttack();
            _v.BonusElement();
            if (_v.CanAttackElementalCommand())
            {
                _v.CalcHpDamage();
                _v.Caster.Flags |= CalcFlag.HpAlteration;
                if (_v.Caster.IsPlayer)
                {
                    _v.Caster.HpDamage = (Int32)(_v.Caster.MaximumHp / 4U);
                }
                else
                {
                    _v.Caster.HpDamage = (Int32)(_v.Caster.MaximumHp >> 3);
                }
                TranceSeekCustomAPI.RaiseTrouble(_v);
            }           
            TranceSeekCustomAPI.SpecialSA(_v);
        }
    }
}
