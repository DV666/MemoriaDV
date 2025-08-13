using System;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Life, Full-Life, Rebirth Flame, Revive
    /// </summary>
    [BattleScript(Id)]
    public sealed class ReviveScript : IBattleScript, IEstimateBattleScript
    {
        public const Int32 Id = 0013;

        private readonly BattleCalculator _v;

        public ReviveScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (!_v.Target.CanBeRevived())
            {
                _v.Context.Flags |= BattleCalcFlags.Miss;
                return;
            }

            if (_v.Caster.PlayerIndex == CharacterId.Quina && (_v.Command.AbilityId == BattleAbilityId.AutoLife || _v.Command.AbilityId == (BattleAbilityId)1526))
            {
                if (_v.Target.CurrentHp == _v.Target.MaximumHp)
                {
                    _v.Target.AlterStatus(BattleStatus.AutoLife, _v.Caster);
                }
                else
                {
                    _v.Target.Flags |= CalcFlag.HpAlteration;
                    if (!_v.Target.IsZombie)
                    {
                        _v.Target.Flags |= CalcFlag.HpRecovery;
                    }

                    if ((_v.Target.CanBeRevived() || _v.Target.Accessory != (RegularItem)1213) && _v.Target.CheckIsPlayer() && _v.Target.CurrentHp == 0U)
                    {
                        _v.Target.HpDamage = (int)(_v.Target.MaximumHp * 3UL / 4UL);
                        TranceSeekAPI.TryRemoveAbilityStatuses(_v);
                    }
                    else
                    {
                        _v.Target.HpDamage = (int)(_v.Target.MaximumHp * 3UL / 4UL);
                    }

                    if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)100)) // Medecin
                        _v.Target.HpDamage += _v.Caster.HpDamage / (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1100) ? 2 : 4);


                    if (_v.Command.IsManyTarget)
                    {
                        if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1126))
                            _v.Target.HpDamage = (_v.Target.HpDamage * 3) / 4;
                        else
                            _v.Target.HpDamage /= 2;
                    }
                }
                return;
            }

            if (HitRateForZombie() && !TranceSeekAPI.TryMagicHit(_v))
                return;

            if (_v.Target.IsZombie && !_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill))
            {
                _v.Target.Kill();
                return;
            }

            if (!_v.Target.CheckIsPlayer())
                return;

            _v.Target.Flags |= CalcFlag.HpAlteration | CalcFlag.HpRecovery;
            if (_v.Target.HasSupportAbilityByIndex((SupportAbility)1004)) // Invincible+
            {
                _v.Target.Flags |= CalcFlag.MpAlteration | CalcFlag.MpRecovery;
                _v.Target.HpDamage = (int)_v.Target.MaximumHp;
                _v.Target.MpDamage = (int)_v.Target.MaximumMp;
            }
            else
            {
                if (_v.Command.AbilityId == BattleAbilityId.Phoenix || _v.Command.AbilityId == BattleAbilityId.RebirthFlame)
                {
                    _v.Target.HpDamage = (Int32)((_v.Target.MaximumHp * (ff9item.FF9Item_GetCount(RegularItem.PhoenixPinion)) / 100));
                    _v.Target.HpDamage /= !_v.Caster.HasSupportAbilityByIndex(SupportAbility.Boost) ? 4 : 1;

                    if (_v.Command.IsShortSummon)
                        _v.Target.HpDamage = _v.Target.HpDamage / 2;

                    _v.Target.HpDamage = Math.Max(1, _v.Target.HpDamage);
                }
                else
                {
                    _v.Target.HpDamage = (Int32)(_v.Target.MaximumHp * (_v.Target.Will + _v.Command.Power) / 100);
                    if (_v.Command.IsManyTarget)
                    {
                        if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1126))
                            _v.Target.HpDamage = (_v.Target.HpDamage * 3) / 4;
                        else
                            _v.Target.HpDamage /= 2;
                    }
                }

                if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)100)) // Medecin
                    _v.Target.HpDamage += _v.Caster.HpDamage / (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1100) ? 2 : 4);
            }
            TranceSeekAPI.TryRemoveAbilityStatuses(_v);
        }

        private Boolean HitRateForZombie()
        {
            if (_v.Target.IsZombie)
            {
                TranceSeekAPI.MagicAccuracy(_v);
                return true;
            }
            return false;
        }

        public Single RateTarget()
        {
            if (!_v.Target.CanBeRevived())
                return 0;

            if (_v.Target.IsZombie)
            {
                TranceSeekAPI.MagicAccuracy(_v);

                Single hitRate = BattleScriptAccuracyEstimate.RatePlayerAttackHit(_v.Context.HitRate);
                Single evaRate = BattleScriptAccuracyEstimate.RatePlayerAttackEvade(_v.Context.Evade);

                Single result = BattleScriptStatusEstimate.RateStatus(BattleStatusId.Death) * hitRate * evaRate;
                if (!_v.Target.IsPlayer)
                    result *= -1;
                return result;
            }

            if (!_v.Target.IsPlayer)
                return 0;

            BattleStatus playerStatus = _v.Target.CurrentStatus;
            BattleStatus removeStatus = _v.Command.AbilityStatus;
            BattleStatus removedStatus = playerStatus & removeStatus;
            Int32 rating = BattleScriptStatusEstimate.RateStatuses(removedStatus);

            return -1 * rating;
        }
    }
}
