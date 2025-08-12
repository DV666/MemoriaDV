using Memoria.Data;
using Memoria.Prime;
using System;﻿

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Curse
    /// </summary>
    [BattleScript(Id)]
    public sealed class CurseScript : IBattleScript
    {
        public const Int32 Id = 0051;

        private readonly BattleCalculator _v;

        public CurseScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {

            if (_v.Command.AbilityId == BattleAbilityId.NoMercy1 || _v.Command.AbilityId == BattleAbilityId.NoMercy2) // Fragilité
            {
                TranceSeekAPI.MagicAccuracy(_v);
                _v.Target.PenaltyShellHitRate();
                _v.PenaltyCommandDividedHitRate();
                foreach (BattleStatusId statusId in (_v.Target.Data.stat.cur & (BattleStatusConst.ContiCount & BattleStatusConst.AnyNegative)).ToStatusList())
                {
                    BattleStatusDataEntry statusData = FF9StateSystem.Battle.FF9Battle.status_data[statusId];
                    _v.Target.Data.stat.conti[statusId] += (Int16)((statusData.ContiCnt * (400 + _v.Caster.Will * 2 - _v.Target.Will)) * _v.Target.Data.stat.duration_factor[statusId]);
                }
                if (_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill)) // !!!TODO!!! Need to improve
                {
                    if (_v.Target.IsUnderAnyStatus(BattleStatus.Poison))
                    {
                        _v.Target.RemoveStatus(BattleStatus.Poison);
                        _v.Command.AbilityStatus |= BattleStatus.Poison;
                    }
                    if (_v.Target.IsUnderAnyStatus(BattleStatus.Blind))
                    {
                        _v.Target.RemoveStatus(BattleStatus.Blind);
                        _v.Command.AbilityStatus |= BattleStatus.Blind;
                    }
                    if (_v.Target.IsUnderAnyStatus(BattleStatus.Trouble))
                    {
                        _v.Target.RemoveStatus(BattleStatus.Trouble);
                        _v.Command.AbilityStatus |= BattleStatus.Trouble;
                    }
                    if (_v.Target.IsUnderAnyStatus(BattleStatus.Venom))
                    {
                        _v.Target.RemoveStatus(BattleStatus.Venom);
                        _v.Command.AbilityStatus |= BattleStatus.Venom;
                    }
                    if (_v.Target.IsUnderAnyStatus(BattleStatus.Zombie))
                    {
                        _v.Target.RemoveStatus(BattleStatus.Zombie);
                        _v.Command.AbilityStatus |= BattleStatus.Zombie;
                    }
                    if (_v.Target.IsUnderAnyStatus(TranceSeekStatus.SilenceEasyKill))
                    {
                        _v.Target.RemoveStatus(TranceSeekStatus.SilenceEasyKill);
                        _v.Command.AbilityStatus |= TranceSeekStatus.SilenceEasyKill;
                    }
                    TranceSeekAPI.TryAlterCommandStatuses(_v);
                }
                if (_v.Command.AbilityId == BattleAbilityId.NoMercy2)
                {
                    _v.Command.AbilityStatus |= (BattleStatus.Poison | BattleStatus.Venom);
                }
                if (TranceSeekAPI.TryMagicHit(_v))
                    TranceSeekAPI.TryAlterCommandStatuses(_v);
                return;
            }               
            
            if (_v.Command.Power > 0)
            {
                if (_v.Target.MagicDefence == 255)
                {
                    _v.Context.Flags |= BattleCalcFlags.Guard;
                    return;
                }
                _v.NormalMagicParams();
                TranceSeekAPI.CharacterBonusPassive(_v, "MagicAttack");
                TranceSeekAPI.CasterPenaltyMini(_v);
                TranceSeekAPI.PenaltyShellAttack(_v);
                TranceSeekAPI.PenaltyCommandDividedAttack(_v);
                _v.CalcHpDamage();
                TranceSeekAPI.TryAlterMagicStatuses(_v);
            }
            if (_v.Caster.Data.dms_geo_id == 328) // Fandalf Curse
            {
                int num4 = GameRandom.Next16() % 3;
                if (num4 == 0)
                {
                    _v.Command.Element = EffectElement.Fire;
                }
                else if (num4 == 1)
                {
                    _v.Command.Element = EffectElement.Cold;
                }
                else if (num4 == 2)
                {
                    _v.Command.Element = EffectElement.Thunder;
                }
            }
            if (_v.Command.Element == 0)
            {
                _v.Command.Element = (EffectElement)(1 << (GameRandom.Next16() % 8));
            }
            UiState.SetBattleFollowMessage(BattleMesages.BecameWeakAgainst, _v.Command.Element);
            _v.Context.Flags = 0;
            if ((_v.Command.Element & _v.Target.AbsorbElement) > 0)
            {
                _v.Target.AbsorbElement &= ~_v.Command.Element;
                _v.Target.GuardElement |= _v.Command.Element;
            }
            else if ((_v.Command.Element & _v.Target.GuardElement) > 0)
            {
                _v.Target.GuardElement &= ~_v.Command.Element;
                _v.Target.HalfElement |= _v.Command.Element;
            }
            else if ((_v.Command.Element & _v.Target.HalfElement) > 0)
            {
                _v.Target.HalfElement &= ~_v.Command.Element;
            }
            else
            {
                _v.Target.WeakElement |= _v.Command.Element;
                if (_v.Command.AbilityStatus != 0)
                {
                    if (_v.Command.HitRate == 255)
                    {
                        TranceSeekAPI.TryRemoveAbilityStatuses(_v);
                    }
                    else
                    {
                        TranceSeekAPI.TryAlterMagicStatuses(_v);
                    }
                }
            }
            if (_v.Caster.PlayerIndex == CharacterId.Amarant && _v.Command.AbilityId == BattleAbilityId.Curse2) // Sort+
            {
                if (_v.Command.Element == EffectElement.Fire)
                {
                    _v.Target.TryAlterStatuses(BattleStatus.Heat, true, _v.Caster);
                }
                else if (_v.Command.Element == EffectElement.Cold)
                {
                    _v.Target.TryAlterStatuses(BattleStatus.Freeze, true, _v.Caster);
                }
                else if (_v.Command.Element == EffectElement.Thunder)
                {
                    _v.Target.TryAlterStatuses(BattleStatus.Slow, true, _v.Caster);
                }
                else if (_v.Command.Element == EffectElement.Aqua)
                {
                    _v.Target.TryAlterStatuses(BattleStatus.Silence, true, _v.Caster);
                }
                else if (_v.Command.Element == EffectElement.Wind)
                {
                    _v.Target.TryAlterStatuses(BattleStatus.Confuse, true, _v.Caster);
                }
                else if (_v.Command.Element == EffectElement.Earth)
                {
                    _v.Target.TryAlterStatuses(BattleStatus.Berserk, true, _v.Caster);
                }
                else if (_v.Command.Element == EffectElement.Holy)
                {
                    _v.Target.TryAlterStatuses(BattleStatus.Blind, true, _v.Caster);
                }
                else if (_v.Command.Element == EffectElement.Darkness)
                {
                    _v.Target.TryAlterStatuses(BattleStatus.Doom, true, _v.Caster);
                }
            }
        }
    }
}
