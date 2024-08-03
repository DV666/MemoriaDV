using Memoria.Data;
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
            if (_v.Caster.PlayerIndex == CharacterId.Amarant)
            {
                if (_v.Command.AbilityId == BattleAbilityId.NoMercy1 || _v.Command.AbilityId == BattleAbilityId.NoMercy2) // Fragilité
                {
                    _v.MagicAccuracy();
                    _v.Target.PenaltyShellHitRate();
                    _v.PenaltyCommandDividedHitRate();
                    //foreach (BattleStatusId statusId in _v.Target.CurrentStatus.ToStatusList()) [!!!TODO!!!]
                    //{
                    //    if ((statusId.ToBattleStatus() & BattleStatusConst.OprCount) == 0)
                    //        continue;
                    //    _v.Target.Data.stat.opr[statusId] += _v.Target.Data.stat.opr[statusId];
                    //}
                    if (_v.Command.AbilityId == BattleAbilityId.NoMercy2)
                    {
                        _v.Command.AbilityStatus |= (BattleStatus.Poison | BattleStatus.Venom);
                    }
                    _v.Command.AbilityStatus = BattleStatus.Confuse;
                    //if (_v.TryMagicHit())
                    TranceSeekCustomAPI.TryAlterCommandStatuses(_v);            
                }
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
                TranceSeekCustomAPI.CharacterBonusPassive(_v, "MagicAttack");
                _v.Caster.PenaltyMini();
                TranceSeekCustomAPI.PenaltyShellAttack(_v);
                _v.PenaltyCommandDividedAttack();
                _v.CalcHpDamage();
                _v.TryAlterMagicStatuses();
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
            if ((_v.Command.Element & _v.Target.AbsorbElement) > 0)
            {
                _v.Target.AbsorbElement &= ~_v.Command.Element;
                _v.Target.GuardElement |= _v.Command.Element;
            }
            else
            {
                if ((_v.Command.Element & _v.Target.GuardElement) > 0)
                {
                    _v.Target.GuardElement &= ~_v.Command.Element;
                    _v.Target.HalfElement |= _v.Command.Element;
                }
                else
                {
                    if ((_v.Command.Element & _v.Target.HalfElement) > 0)
                    {
                        _v.Target.HalfElement &= ~_v.Command.Element;
                    }
                    else
                    {
                        _v.Target.WeakElement |= _v.Command.Element;
                        if (_v.Command.HitRate == 255)
                        {
                            _v.TryRemoveAbilityStatuses();
                        }
                        else
                        {
                            _v.TryAlterMagicStatuses();
                        }
                    }
                }
            }
            if (_v.Caster.PlayerIndex == CharacterId.Amarant && _v.Command.AbilityId == BattleAbilityId.Curse2)
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
