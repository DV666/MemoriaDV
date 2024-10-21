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
            _v.PhysicalAccuracy();
            if (_v.Caster.IsPlayer)
            {
                _v.WeaponPhysicalParams();
            }
            else
            {
                _v.NormalPhysicalParams();
            }
            TranceSeekCustomAPI.CharacterBonusPassive(_v, "PhysicalAttack");
            TranceSeekCustomAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
            TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
            TranceSeekCustomAPI.BonusBackstabAndPenaltyLongDistance(_v);
            TranceSeekCustomAPI.EnemyTranceBonusAttack(_v);
            TranceSeekCustomAPI.BonusElement(_v);
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
        }
    }
}
