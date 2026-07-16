using System;
using Memoria.Data;

namespace Memoria.Scripts.TranceSeek
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
            if (_v.Command.AbilityId == TranceSeekBattleAbility.Judgement || _v.Command.HitRate == 255)
            {
                _v.Target.RemoveStatus(BattleStatus.Protect);
                _v.Target.RemoveStatus(BattleStatus.Shell);
                _v.Target.RemoveStatus(BattleStatus.Vanish);
                _v.Target.RemoveStatus(BattleStatus.Reflect);
            }

            if (_v.Caster.IsPlayer)
                _v.WeaponPhysicalParams();
            else
                _v.NormalPhysicalParams();

            if (!TranceSeekAPI.TryKillFrozen(_v))
            {               
                TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
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
