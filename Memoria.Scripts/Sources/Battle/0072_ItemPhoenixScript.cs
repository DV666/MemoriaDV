using System;
using Memoria.Data;
using static SiliconStudio.Social.ResponseData;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    ///  Phoenix Down, Phoenix Pinion
    /// </summary>
    [BattleScript(Id)]
    public sealed class ItemPhoenixScript : IBattleScript, IEstimateBattleScript
    {
        public const Int32 Id = 0072;

        private readonly BattleCalculator _v;

        public ItemPhoenixScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Target.Data.dms_geo_id == 401 && (FF9StateSystem.Battle.battleMapIndex == 631 || FF9StateSystem.Battle.battleMapIndex == 632)) // Friendly Feather Circle
            {
                _v.Target.Flags |= CalcFlag.HpAlteration | CalcFlag.HpRecovery;
                if (_v.Command.ItemId == RegularItem.PhoenixDown)
                {
                    _v.Target.HpDamage = 2500;
                }
                else if (_v.Command.ItemId == RegularItem.PhoenixPinion)
                {
                    _v.Target.HpDamage = 9999;
                }
                else
                {
                    _v.Context.Flags |= BattleCalcFlags.Miss;
                }
            }
            else
            {
                if (!_v.Target.CanBeRevived())
                    return;

                if (_v.Target.Accessory == (RegularItem)1213) // Anneau Maudit
                {
                    if (_v.Command.ItemId == RegularItem.PhoenixPinion && (_v.Target.Data.stat.permanent & BattleStatus.Doom) != 0 && !_v.Target.IsUnderAnyStatus(BattleStatus.Death))
                    {
                        _v.Context.Flags |= BattleCalcFlags.Miss;
                        return;
                    }
                }

                if (_v.Target.IsZombie && !_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill))
                {
                    if ((_v.Target.CurrentHp = (UInt32)(GameRandom.Next8() % 10)) == 0)
                        _v.Target.Kill();
                }
                else if (_v.Target.CheckIsPlayer())
                {
                    if (_v.Target.IsUnderStatus(BattleStatus.Death))
                        if (_v.Target.HasSupportAbilityByIndex((SupportAbility)1004)) // Invincible+
                        {
                            _v.Target.Flags |= CalcFlag.HpAlteration | CalcFlag.HpRecovery | CalcFlag.MpAlteration | CalcFlag.MpRecovery;
                            _v.Target.HpDamage = (int)_v.Target.MaximumHp;
                            _v.Target.MpDamage = (int)_v.Target.MaximumMp;
                        }
                        else
                        {
                            _v.Target.CurrentHp = (UInt32)(1 + GameRandom.Next8() % 10);
                        }
                    TranceSeekAPI.TryRemoveItemStatuses(_v);
                }
            }

            if (_v.Caster.PlayerIndex == CharacterId.Blank && _v.Command.Id == BattleCommandId.Item)
                btl_stat.AlterStatus(_v.Caster, TranceSeekStatusId.Special, _v.Caster, true, "SoakedBlade", _v.Command.ItemId);
        }

        public Single RateTarget()
        {
            if (!_v.Target.CanBeRevived())
                return 0;

            if (_v.Target.IsZombie && !_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill))
            {
                Single result = BattleScriptStatusEstimate.RateStatus(BattleStatusId.Death) * 0.1f;
                if (!_v.Target.IsPlayer)
                    result *= -1;
                return result;
            }

            if (!_v.Target.IsPlayer)
                return 0;

            BattleStatus playerStatus = _v.Target.CurrentStatus;
            BattleStatus removeStatus = _v.Command.ItemStatus;
            BattleStatus removedStatus = playerStatus & removeStatus;
            Int32 rating = BattleScriptStatusEstimate.RateStatuses(removedStatus);

            return -1 * rating;
        }
    }
}
