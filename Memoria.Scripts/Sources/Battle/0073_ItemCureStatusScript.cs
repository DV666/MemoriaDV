using System;
using Memoria.Data;
using Memoria.Prime;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Echo Screen, Antidote, Eye Drops, Magic Tag, Vaccine, Remedy, Annoyntment, Gysahl Greens
    /// </summary>
    [BattleScript(Id)]
    public sealed class ItemCureStatusScript : IBattleScript, IEstimateBattleScript
    {
        public const Int32 Id = 0073;

        private readonly BattleCalculator _v;

        public ItemCureStatusScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            TranceSeekAPI.TryRemoveItemStatuses(_v);
            if ((_v.Command.ItemId == RegularItem.Remedy || _v.Command.ItemId == RegularItem.Annoyntment || _v.Command.ItemId == (RegularItem)1003) && _v.Target.IsUnderAnyStatus(TranceSeekStatus.Vieillissement))
            {
                _v.Target.RemoveStatus(TranceSeekStatus.Vieillissement);
                _v.Context.Flags = 0;
            }
            if (_v.Caster.PlayerIndex == CharacterId.Blank && _v.Command.Id == BattleCommandId.Item)
                btl_stat.AlterStatus(_v.Caster, TranceSeekStatusId.Special, _v.Caster, true, "SoakedBlade", _v.Command.ItemId);
        }

        public Single RateTarget()
        {
            if (_v.Target.IsUnderAnyStatus(TranceSeekStatus.Vieillissement) && (_v.Command.ItemId == RegularItem.Remedy || _v.Command.ItemId == RegularItem.Annoyntment || _v.Command.ItemId == (RegularItem)1003))
                return 20;

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
