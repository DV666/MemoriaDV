using System;
using System.Collections.Generic;
using FF9;
using Memoria.Data;

namespace Memoria.Scripts.TranceSeek
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
            if (_v.Command.AbilityId == BattleAbilityId.Aura1 || _v.Command.AbilityId == BattleAbilityId.Aura2) // Amarant - Plenitude
            {
                if (_v.Caster.IsUnderStatus(BattleStatus.Trance))
                {
                    _v.Target.Flags |= (CalcFlag.MpAlteration | CalcFlag.MpRecovery);
                    _v.Target.MpDamage = (int)(_v.Target.MaximumMp / 2U);
                }
                _v.Target.TryAlterSingleStatus(BattleStatusId.ChangeStat, true, _v.Caster, "Magic", Math.Min(99, _v.Target.Magic + (_v.Target.Magic / _v.Command.Power)));
                _v.Target.TryAlterSingleStatus(BattleStatusId.ChangeStat, true, _v.Caster, "Will", Math.Min(50, _v.Target.Will + (_v.Target.Will / _v.Command.Power)));
                TranceSeekAPI.TryAlterCommandStatuses(_v);
            }
            else if (_v.Command.Power == 111 && _v.Command.HitRate == 111) // Ogre - Zenitude
            {
                btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.PerfectCrit, parameters: $"+9");
                TranceSeekAPI.TryAlterCommandStatuses(_v);
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
                    _v.Target.RemoveStatus(BattleStatus.Death);
                    _v.Target.RemoveStatus(BattleStatusConst.AnyNegative);
                    _v.Target.HpDamage = (int)_v.Target.MaximumHp;
                    _v.Target.MpDamage = (int)_v.Target.MaximumMp;
                }
                return;
            }
            else
            {
                if (_v.Caster.HasSupportAbilityByIndex(SupportAbility.PowerUp)) // PowerUp
                    TranceSeekAPI.IncreaseTrance(_v.Target.Data, Comn.random16() % (_v.Caster.Will / 2));

                if (_v.Caster.HasSupportAbilityByIndex(SupportAbility.PowerUp) && !_v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.PowerUp_Boosted))
                {
                    _v.Target.Flags |= (CalcFlag.HpDamageOrHeal);
                    _v.Target.HpDamage = (int)(_v.Target.MaximumHp * (uint)_v.Command.Power / 100U);
                }
                else
                {
                    _v.Target.Flags |= (CalcFlag.HpDamageOrHeal | CalcFlag.MpDamageOrHeal);
                    _v.Target.HpDamage = (int)(_v.Target.MaximumHp * (uint)_v.Command.Power / 100U);
                    _v.Target.MpDamage = (int)(_v.Target.MaximumMp * (uint)_v.Command.Power / 100U);
                }

                if (_v.Command.AbilityId == BattleAbilityId.Chakra2)
                    _v.Target.RemoveStatus(BattleStatus.Poison | BattleStatus.Silence | BattleStatus.Blind);
                else if (_v.Command.HitRate == 222)
                    _v.Target.TryRemoveStatuses(BattleStatusConst.AnyNegative);
                else
                    TranceSeekAPI.TryAlterCommandStatuses(_v);
            }
        }
    }
}
