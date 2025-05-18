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
            if (TranceSeekAPI.TryPhysicalHit(_v))
            {
                _v.NormalPhysicalParams();
                TranceSeekAPI.CharacterBonusPassive(_v, "PhysicalAttack");
                TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                TranceSeekAPI.EnemyTranceBonusAttack(_v);
                TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                if (Mathf.Abs((_v.Caster.Row - _v.Target.Row)) > 1)
                    ++_v.Context.DamageModifierCount;
                TranceSeekAPI.BonusElement(_v);
                if (_v.CanAttackElementalCommand())
                {
                    _v.CalcPhysicalHpDamage();
                    TranceSeekAPI.RaiseTrouble(_v);
                    TranceSeekAPI.TryAlterMagicStatuses(_v);
                }
            }
        }
    }
}
