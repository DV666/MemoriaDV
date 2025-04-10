﻿using System;
using System.Collections.Generic;
using FF9;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Chakra
    /// </summary>
    [BattleScript(Id)]
    public sealed class ChakraScript : IBattleScript
    {
        public const Int32 Id = 0037;

        private readonly BattleCalculator _v;

        public ChakraScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Command.Power == 111 && _v.Command.HitRate == 111)
            {
                if (_v.Caster.PlayerIndex == CharacterId.Amarant) // Amarant - Plenitude
                {
                    if (_v.Caster.IsUnderStatus(BattleStatus.Trance))
                    {
                        _v.Target.Flags |= (CalcFlag.MpAlteration | CalcFlag.MpRecovery);
                        _v.Target.MpDamage = (int)(_v.Target.MaximumMp / 2U);
                    }
                    _v.Target.TryAlterSingleStatus(BattleStatusId.ChangeStat, true, _v.Caster, "Magic", Math.Min(99, _v.Target.Magic + (_v.Target.Magic / 10)));
                    Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                                {
                                    { "US", "Magic ↑" },
                                    { "UK", "Magic ↑" },
                                    { "JP", "まりょく ↑" },
                                    { "ES", "POT magico ↑" },
                                    { "FR", "Magie ↑" },
                                    { "GR", "Magia ↑" },
                                    { "IT", "Zauber ↑" },
                                };
                    btl2d.Btl2dReqSymbolMessage(_v.Caster.Data, "[F9FF39]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 0);
                    _v.Target.TryAlterSingleStatus(BattleStatusId.ChangeStat, true, _v.Caster, "Will", Math.Min(50, _v.Target.Will + (_v.Target.Will / 10)));
                    Dictionary<String, String> localizedMessage2 = new Dictionary<String, String>
                                {
                                    { "US", "Spirit ↑" },
                                    { "UK", "Spirit ↑" },
                                    { "JP", "きりょく ↑" },
                                    { "ES", "POT spirito ↑" },
                                    { "FR", "Esprit ↑" },
                                    { "GR", "Espíritu ↑" },
                                    { "IT", "Wille ↑" },
                                };
                    btl2d.Btl2dReqSymbolMessage(_v.Caster.Data, "[F9FF39]", localizedMessage2, HUDMessage.MessageStyle.DAMAGE, 5);
                    TranceSeekCustomAPI.TryAlterCommandStatuses(_v);
                }
                else // Ogre - Zenitude
                {
                    TranceSeekCustomAPI.TryAlterCommandStatuses(_v);
                    _v.Target.Will = 99;
                }
            }
            else if (_v.Caster.Data.dms_geo_id == 401) // Friendly Feather Circle - Angel Whisper + End
            {
                if (_v.Command.Power == 75 && _v.Command.HitRate == 75)
                {
                    _v.Target.Flags |= CalcFlag.HpAlteration;
                    if (!_v.Target.IsZombie)
                    {
                        _v.Target.Flags |= CalcFlag.HpRecovery;
                    }
                    _v.Target.HpDamage = (int)(_v.Target.MaximumHp * 3UL / 4UL);
                }
                else if (_v.Command.Power == 100 && _v.Command.HitRate == 100)
                {
                    _v.Target.Flags |= (CalcFlag.HpDamageOrHeal | CalcFlag.MpDamageOrHeal);
                    btl_stat.MakeStatusesPermanent(_v.Target, BattleStatus.Zombie, false);
                    _v.Target.RemoveStatus(BattleStatus.Zombie);
                    _v.Target.RemoveStatus(BattleStatus.Death);
                    _v.Target.HpDamage = (int)_v.Target.MaximumHp;
                    _v.Target.MpDamage = (int)_v.Target.MaximumMp;
                }
                return;
            }
            else
            {
                _v.Target.Flags |= (CalcFlag.HpDamageOrHeal | CalcFlag.MpDamageOrHeal);
                if (_v.Caster.HasSupportAbilityByIndex(SupportAbility.PowerUp)) // PowerUp
                {
                    if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1029)) // PowerUp+
                    {
                        _v.Target.HpDamage = (int)(_v.Target.MaximumHp * (uint)_v.Command.Power / 100U);
                        _v.Target.MpDamage = (int)(_v.Target.MaximumMp * (uint)_v.Command.Power / 100U);
                    }
                    _v.Target.Trance = (byte)(_v.Target.Trance + (Comn.random16() % _v.Caster.Will));
                }
                else
                {
                    _v.Target.HpDamage = (int)(_v.Target.MaximumHp * (uint)_v.Command.Power / 100U);
                    _v.Target.MpDamage = (int)(_v.Target.MaximumMp * (uint)_v.Command.Power / 100U);
                }
                if (_v.Caster.PlayerIndex == CharacterId.Amarant && _v.Caster.IsUnderStatus(BattleStatus.Trance))
                {
                    _v.Target.RemoveStatus(BattleStatus.Poison);
                    _v.Target.RemoveStatus(BattleStatus.Silence);
                    _v.Target.RemoveStatus(BattleStatus.Blind);
                }
                TranceSeekCustomAPI.TryAlterMagicStatuses(_v);
            }
        }
    }
}
