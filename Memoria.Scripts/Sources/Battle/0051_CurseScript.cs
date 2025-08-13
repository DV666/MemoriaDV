using Memoria.Data;
using Memoria.Prime;
using System.Collections.Generic;
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
                TranceSeekAPI.AlterStatusDuration(_v, _v.Target.Data.stat.cur & (BattleStatusConst.ContiCount & BattleStatusConst.AnyNegative));

                if (_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill)) 
                {
                    List<BattleStatus> statuschoosen = new List<BattleStatus>{ BattleStatus.Poison, BattleStatus.Venom, BattleStatus.Blind,
                        BattleStatus.Trouble, BattleStatus.Zombie, TranceSeekStatus.SilenceEasyKill };

                    foreach (BattleStatus status in statuschoosen)
                    {
                        if (_v.Target.IsUnderAnyStatus(status))
                        {
                            _v.Target.RemoveStatus(status);
                            _v.Command.AbilityStatus |= status;
                        }
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
