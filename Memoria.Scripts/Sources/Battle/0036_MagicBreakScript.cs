using System;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Magic Break
    /// </summary>
    [BattleScript(Id)]
    public sealed class MagicBreakScript : IBattleScript
    {
        public const Int32 Id = 0036;

        private readonly BattleCalculator _v;

        public MagicBreakScript(BattleCalculator v)
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
                if (TranceSeekAPI.SteinerPassive[_v.Caster.Data][1] > 0)
                {
                    _v.Command.HitRate += 10 * TranceSeekAPI.SteinerPassive[_v.Caster.Data][1];
                    TranceSeekAPI.ResetSteinerPassive(_v.Caster);
                }

                TranceSeekAPI.MagicAccuracy(_v);
                TranceSeekAPI.EnemyTranceBonusAttack(_v);
                TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                TranceSeekAPI.BonusBackstabAndPenaltyLongDistance(_v);
                TranceSeekAPI.BonusElement(_v);
                _v.CalcHpDamage();
                _v.Command.AbilityStatus |= TranceSeekStatus.MagicBreak;
                TranceSeekAPI.TryAlterMagicStatuses(_v);
            }
        }
    }
}
