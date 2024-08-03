using System;
using Memoria.Data;

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
            _v.TryRemoveItemStatuses();
            if (_v.Command.ItemId == RegularItem.Annoyntment)
            {
                _v.Target.AlterStatus(TranceSeekCustomAPI.CustomStatus.Vieillissement, _v.Caster);
                return;
            }
            if (_v.Command.ItemId == RegularItem.Remedy || _v.Command.ItemId == RegularItem.Annoyntment || _v.Command.ItemId == (RegularItem)1003)
            {
                _v.Target.RemoveStatus(TranceSeekCustomAPI.CustomStatus.Vieillissement);
                _v.Context.Flags = 0;
            }
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
