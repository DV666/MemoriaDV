using System;
using UnityEngine;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Cannon
    /// </summary>
    [BattleScript(Id)]
    public sealed class CannonScript : IBattleScript
    {
        public const Int32 Id = 0091;

        private readonly BattleCalculator _v;

        public CannonScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            _v.PhysicalAccuracy();
            if (TranceSeekCustomAPI.TryPhysicalHit(_v))
            {
                _v.NormalPhysicalParams();
                TranceSeekCustomAPI.CharacterBonusPassive(_v, "PhysicalAttack");
                TranceSeekCustomAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                TranceSeekCustomAPI.EnemyTranceBonusAttack(_v);
                TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                if (Mathf.Abs((_v.Caster.Row - _v.Target.Row)) > 1)
                    ++_v.Context.DamageModifierCount;
                TranceSeekCustomAPI.BonusElement(_v);
                if (_v.CanAttackElementalCommand())
                {
                    _v.CalcPhysicalHpDamage();
                    TranceSeekCustomAPI.RaiseTrouble(_v);
                    TranceSeekCustomAPI.TryAlterMagicStatuses(_v);
                }
            }
        }
    }
}
