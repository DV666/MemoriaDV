using FF9;
using Memoria.Data;
using System;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Weapon: Blood Sword
    /// </summary>
    [BattleScript(Id)]
    public sealed class EnemyPhysicalDispelAttackScript : IBattleScript
    {
        public const Int32 Id = 0128;

        private readonly BattleCalculator _v;

    public EnemyPhysicalDispelAttackScript(BattleCalculator v)
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

                btl_stat.RemoveStatuses(_v.Target, _v.Command.AbilityStatus);
                _v.NormalPhysicalParams();
                TranceSeekAPI.CharacterBonusPassive(_v, "PhysicalAttack");
                TranceSeekAPI.EnemyTranceBonusAttack(_v);
                TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                if (_v.Command.HitRate != 101)
                {
                    TranceSeekAPI.BonusBackstabAndPenaltyLongDistance(_v);
                }
                TranceSeekAPI.BonusElement(_v);
                if (_v.CanAttackElementalCommand())
                {
                    TranceSeekAPI.TryCriticalHit(_v);
                    _v.CalcPhysicalHpDamage();
                    TranceSeekAPI.RaiseTrouble(_v);
                    TranceSeekAPI.InfusedWeaponStatus(_v);
                }                    
            }
        }
    }
}
