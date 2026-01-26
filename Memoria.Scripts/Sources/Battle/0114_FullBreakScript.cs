using System;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Armour Break
    /// </summary>
    [BattleScript(Id)]
    public sealed class FullBreakScript : IBattleScript
    {
        public const Int32 Id = 0114;

        private readonly BattleCalculator _v;

        public FullBreakScript(BattleCalculator v)
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

                if (_v.Caster.IsPlayer)
                {
                    _v.WeaponPhysicalParams();
                    TranceSeekAPI.CharacterBonusPassive(_v, "LowPhysicalAttack");
                }
                else
                {
                    _v.NormalPhysicalParams();
                }
                TranceSeekAPI.MagicAccuracy(_v);
                TranceSeekAPI.EnemyTranceBonusAttack(_v);
                TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                TranceSeekAPI.BonusBackstabAndPenaltyLongDistance(_v);
                TranceSeekAPI.BonusElement(_v);
                _v.CalcHpDamage();
                _v.Command.AbilityStatus |= TranceSeekStatus.PowerBreak;
                _v.Command.AbilityStatus |= TranceSeekStatus.MagicBreak;
                _v.Command.AbilityStatus |= TranceSeekStatus.ArmorBreak;
                _v.Command.AbilityStatus |= TranceSeekStatus.MentalBreak;
                TranceSeekAPI.TryAlterMagicStatuses(_v);
            }
        }
    }
}
