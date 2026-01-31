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
            if (!_v.Target.TryKillFrozen())
            {
                if (_v.Target.PhysicalDefence == 255)
                {
                    _v.Context.Flags |= BattleCalcFlags.Guard;
                    return;
                }
                if (_v.Target.IsUnderAnyStatus(BattleStatus.Vanish) || _v.Target.PhysicalEvade == 255)
                {
                    _v.Context.Flags |= BattleCalcFlags.Miss;
                    return;
                }
                _v.PhysicalAccuracy();
                if (_v.Caster.IsPlayer)
                {
                    _v.WeaponPhysicalParams();
                }
                else
                {
                    _v.NormalPhysicalParams();
                }
                TranceSeekAPI.CharacterBonusPassive(_v, "PhysicalAttack");
                TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                TranceSeekAPI.BonusBackstabAndPenaltyLongDistance(_v);
                TranceSeekAPI.EnemyTranceBonusAttack(_v);
                TranceSeekAPI.BonusElement(_v);
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
                    TranceSeekAPI.RaiseTrouble(_v);
                }
            }
        }
    }
}
