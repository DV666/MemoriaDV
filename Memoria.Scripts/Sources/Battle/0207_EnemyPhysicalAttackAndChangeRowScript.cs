using Memoria.Data;
using System;
using UnityEngine;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// ???
    /// </summary>
    [BattleScript(Id)]
    public sealed class EnemyPhysicalAttackAndChangeRowScript : IBattleScript
    {
        public const Int32 Id = 0207;

        private readonly BattleCalculator _v;

        public EnemyPhysicalAttackAndChangeRowScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            _v.NormalPhysicalParams();
            TranceSeekCustomAPI.CharacterBonusPassive(_v, "PhysicalAttack");
            TranceSeekCustomAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
            TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
            TranceSeekCustomAPI.BonusBackstabAndPenaltyLongDistance(_v);
            TranceSeekCustomAPI.BonusElement(_v);
            if (_v.CanAttackElementalCommand() && TranceSeekCustomAPI.TryPhysicalHit(_v))
            {
                _v.CalcHpDamage();
                TranceSeekCustomAPI.InfusedWeaponStatus(_v);
                if (_v.Command.HitRate == 255)
                {
                    if ((Mathf.Abs((_v.Caster.Row - _v.Target.Row)) <= 1) && (!_v.Target.HasSupportAbilityByIndex((SupportAbility)1026))) // Stone Skin+
                    {
                        _v.Target.ChangeRow();
                    }
                }
                else
                {
                    if ((_v.Target.Row > 0) && (!_v.Target.HasSupportAbilityByIndex((SupportAbility)1026))) // Stone Skin+
                    {
                        _v.Target.ChangeRow();
                    }
                }
                _v.TryAlterMagicStatuses();
            }
        }
    }
}