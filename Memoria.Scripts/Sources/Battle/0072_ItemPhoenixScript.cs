<<<<<<< HEAD
=======
using System;
using System.Runtime.Remoting.Contexts;
>>>>>>> origin/TranceSeekCurrent
using Memoria.Data;
using System;

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
<<<<<<< HEAD
            if (!_v.Target.CanBeRevived())
                return;

            if (_v.Target.IsZombie)
=======
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            if (_v.Target.Data.dms_geo_id == 401 && (FF9StateSystem.Battle.battleMapIndex == 631 || FF9StateSystem.Battle.battleMapIndex == 632)) // Friendly Feather Circle
>>>>>>> origin/TranceSeekCurrent
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
                {
                    _v.Context.Flags |= BattleCalcFlags.Miss;
                    return;
                }

                if (_v.Target.Accessory == (RegularItem)1213)
                {
                    _v.Target.PermanentStatus &= ~BattleStatus.Doom;
                    btl_stat.RemoveStatus(_v.Target, BattleStatus.Doom);
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
                    _v.TryRemoveItemStatuses();
                }
            }
            TranceSeekCustomAPI.SpecialSA(_v);
        }

        public Single RateTarget()
        {
            if (!_v.Target.CanBeRevived() || _v.Target.Accessory == (RegularItem)1213)
                return 0;

            if (_v.Target.IsZombie && !_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill))
            {
                Single result = BattleScriptStatusEstimate.RateStatus(BattleStatus.Death) * 0.1f;
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
