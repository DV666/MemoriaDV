using FF9;
<<<<<<< HEAD
using System;
=======
using UnityEngine;
>>>>>>> origin/TranceSeekCurrent

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
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            _v.PhysicalAccuracy();
            if (TranceSeekCustomAPI.TryPhysicalHit(_v))
            {
                _v.NormalPhysicalParams();
                TranceSeekCustomAPI.CharacterBonusPassive(_v, "PhysicalAttack");
                _v.Caster.PhysicalPenaltyAndBonusAttack();
                _v.Caster.EnemyTranceBonusAttack();
                TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                if (Mathf.Abs((_v.Caster.Row - _v.Target.Row)) > 1)
                {
                    _v.Context.Attack = _v.Context.Attack * 3 >> 1;
                }
                _v.BonusElement();
                if (_v.CanAttackElementalCommand())
                {
                    _v.CalcPhysicalHpDamage();
                    TranceSeekCustomAPI.RaiseTrouble(_v);
                    _v.TryAlterMagicStatuses();
                }
            }
            TranceSeekCustomAPI.SpecialSA(_v);
        }
    }
}
