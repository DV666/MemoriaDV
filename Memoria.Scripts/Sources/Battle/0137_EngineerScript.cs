using System;
using System.Collections.Generic;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Magic Break
    /// </summary>
    [BattleScript(Id)]
    public sealed class EngineerScript : IBattleScript
    {
        public const Int32 Id = 0137;

        private readonly BattleCalculator _v;

        public EngineerScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Command.AbilityId == (BattleAbilityId)1140) // Electroshock
            {
                _v.NormalMagicParams();
                TranceSeekAPI.CharacterBonusPassive(_v, "MagicAttack");
                TranceSeekAPI.CasterPenaltyMini(_v);
                TranceSeekAPI.EnemyTranceBonusAttack(_v);
                TranceSeekAPI.PenaltyCommandDividedAttack(_v);
                _v.CalcHpMagicRecovery();
                _v.Target.Flags |= (CalcFlag.MpDamageOrHeal);
                _v.Target.MpDamage = Math.Max(1, _v.Target.HpDamage >> 5);
                if (_v.Caster.Position == _v.Target.Position + 1 || _v.Caster.Position == _v.Target.Position - 1)
                {
                    _v.Target.HpDamage *= 2;
                    _v.Target.MpDamage *= 2;
                }
                _v.Target.TryRemoveStatuses(_v.Command.AbilityStatus);
            }
            else if (_v.Command.AbilityId == (BattleAbilityId)1142) // Adjustable Wrench
            {
                for (Int32 i = 0; i < 8; i++)
                {
                    int idAA = 1136 + i;
                    if (idAA == 1142)
                        continue;
                    if (FF9StateSystem.Battle.FF9Battle.aa_data[(BattleAbilityId)idAA].MP > 0)
                        FF9StateSystem.Battle.FF9Battle.aa_data[(BattleAbilityId)idAA].MP--;

                    if (FF9StateSystem.Battle.FF9Battle.aa_data[(BattleAbilityId)idAA].MP > 0)
                        FF9StateSystem.Battle.FF9Battle.aa_data[(BattleAbilityId)idAA].MP--;
                }
            }
            else if (_v.Command.AbilityId == (BattleAbilityId)1143) // Hymn of the Tantalas
            {
                if (_v.Target.InTrance || !_v.Target.HasTrance)
                {
                    _v.Context.Flags = BattleCalcFlags.Miss;
                    return;
                }
                _v.Target.Trance = (byte)Math.Min(_v.Target.Trance + ((_v.Target.Trance * 25) / 100), byte.MaxValue);
                if (_v.Target.Trance == byte.MaxValue)
                    _v.Target.AlterStatus(BattleStatus.Trance);
            }
        }
    }
}
