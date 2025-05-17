using Memoria.Data;
using System;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Soft (Item)
    /// </summary>
    [BattleScript(Id)]
    public sealed class ItemSoftScript : IBattleScript, IEstimateBattleScript
    {
        public const Int32 Id = 0062;

        private readonly BattleCalculator _v;

        public ItemSoftScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            BTL_DATA data = _v.Target.Data;
            if (data.dms_geo_id == 221 || data.dms_geo_id == 83)
            {
                if (TranceSeekAPI.CheckUnsafetyOrGuard(_v))
                {
                    _v.Target.Flags |= CalcFlag.HpAlteration;
                    _v.Target.HpDamage = (int)(_v.Target.MaximumHp / 2U);
                }
                else
                {
                    _v.Context.Flags |= BattleCalcFlags.Miss;
                }
            }
            else
            {
                TranceSeekAPI.TryRemoveItemStatuses(_v);
            }

            if (_v.Caster.PlayerIndex == CharacterId.Blank && _v.Command.Id == BattleCommandId.Item)
                btl_stat.AlterStatus(_v.Caster, TranceSeekStatusId.Special, _v.Caster, true, "SoakedBlade", _v.Command.ItemId);
        }

        public Single RateTarget()
        {
            BattleStatus playerStatus = _v.Target.CurrentStatus;
            BattleStatus removeStatus = _v.Command.ItemStatus;
            BattleStatus removedStatus = playerStatus & removeStatus;
            Int32 rating = BattleScriptStatusEstimate.RateStatuses(removedStatus);

            if (_v.Target.IsPlayer)
                return -1 * rating;

            return rating;
        }
    }
}
