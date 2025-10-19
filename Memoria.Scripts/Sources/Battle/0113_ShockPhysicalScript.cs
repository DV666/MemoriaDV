using System;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Dark Matter
    /// </summary>
    [BattleScript(Id)]
    public sealed class ShockMagicalScript : IBattleScript
    {
        public const Int32 Id = 00113;

        private readonly BattleCalculator _v;

        public ShockMagicalScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Caster.IsPlayer)
            {
                if (_v.Command.AbilityId == TranceSeekBattleAbility.Judgement)
                {
                    _v.Target.RemoveStatus(BattleStatus.Protect);
                    _v.Target.RemoveStatus(BattleStatus.Shell);
                    _v.Target.RemoveStatus(BattleStatus.Vanish);
                    _v.Target.RemoveStatus(BattleStatus.Reflect);
                }

                _v.WeaponPhysicalParams();
                TranceSeekAPI.CharacterBonusPassive(_v, "PhysicalAttack");
                TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                if (_v.Target.IsUnderAnyStatus(BattleStatus.Protect))
                    _v.Context.DamageModifierCount += 2; // Ignore Protect reduction.
                TranceSeekAPI.BonusElement(_v);
                if (TranceSeekAPI.CanAttackWeaponElementalCommand(_v))
                {
                    _v.CalcHpDamage();
                    TranceSeekAPI.TryAlterMagicStatuses(_v);
                }
                return;
            }
            _v.PhysicalAccuracy();
            if (!_v.Target.TryKillFrozen())
            {
                if (_v.Command.HitRate == 255)
                {
                    _v.Target.RemoveStatus(BattleStatus.Protect);
                    _v.Target.RemoveStatus(BattleStatus.Shell);
                    _v.Target.RemoveStatus(BattleStatus.Vanish);
                    _v.Target.RemoveStatus(BattleStatus.Reflect);
                }
                _v.NormalPhysicalParams();
                TranceSeekAPI.CharacterBonusPassive(_v, "PhysicalAttack");
                TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                if (_v.Target.IsUnderAnyStatus(BattleStatus.Protect))
                    _v.Context.DamageModifierCount += 2; // Ignore Protect reduction.
                TranceSeekAPI.BonusElement(_v);
                if (_v.CanAttackElementalCommand())
                {
                    _v.CalcPhysicalHpDamage();

                    TranceSeekAPI.TryAlterMagicStatuses(_v);
                }
            }
        }
    }
}
