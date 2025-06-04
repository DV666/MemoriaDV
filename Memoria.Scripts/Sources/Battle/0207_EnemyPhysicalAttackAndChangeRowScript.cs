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
            TranceSeekAPI.CharacterBonusPassive(_v, "PhysicalAttack");
            TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
            TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
            TranceSeekAPI.BonusBackstabAndPenaltyLongDistance(_v);
            TranceSeekAPI.BonusElement(_v);
            if (_v.CanAttackElementalCommand() && TranceSeekAPI.TryPhysicalHit(_v))
            {
                _v.CalcHpDamage();
                TranceSeekAPI.InfusedWeaponStatus(_v);
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
                        if (_v.Target.Row == 1)
                            btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.Special, parameters: "CanCover1");
                        else
                            btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.Special, parameters: "CanCover0");
                    }
                }
                TranceSeekAPI.TryAlterMagicStatuses(_v);
            }
        }
    }
}
