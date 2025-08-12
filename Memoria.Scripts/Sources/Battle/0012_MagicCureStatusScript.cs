using FF9;
using Memoria.Data;
using System;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Panacea, Stona, Esuna, Dispel
    /// </summary>
    [BattleScript(Id)]
    public sealed class MagicCureStatusScript : IBattleScript, IEstimateBattleScript
    {
        public const Int32 Id = 0012;

        private readonly BattleCalculator _v;

        public MagicCureStatusScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {       
            if (_v.Command.AbilityId == BattleAbilityId.Esuna)
            {
                _v.Command.AbilityStatus |= TranceSeekStatus.Vieillissement;
            }
            if (_v.Command.HitRate == 255)
            {
                _v.Command.AbilityStatus |= (TranceSeekStatus.PowerUp | TranceSeekStatus.MagicUp | TranceSeekStatus.ArmorUp
                    | TranceSeekStatus.MentalUp | TranceSeekStatus.Bulwark | TranceSeekStatus.PerfectCrit | TranceSeekStatus.PerfectDodge);
            }
            TranceSeekAPI.TryRemoveAbilityStatuses(_v);
            if (_v.Command.Power == 111)
            {
                Boolean easykill = false;
                if (_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill))
                    easykill = true;

                foreach (BattleStatusId statusId in _v.Target.Data.stat.cur.ToStatusList())
                btl_stat.RemoveStatus(_v.Target, statusId);

                if (easykill)
                    _v.Target.AlterStatus(BattleStatus.EasyKill);

                _v.Context.Flags = 0;
            }
            if (_v.Command.HitRate == 222)
            {
                _v.Context.Flags = 0;
            }
        }

        public Single RateTarget()
        {
            if (_v.Target.IsUnderAnyStatus(TranceSeekStatus.Vieillissement) && _v.Command.AbilityId == BattleAbilityId.Esuna)
                return 20;

            BattleStatus playerStatus = _v.Target.CurrentStatus;
            BattleStatus removeStatus = _v.Command.AbilityStatus;
            BattleStatus removedStatus = playerStatus & removeStatus;
            Int32 rating = BattleScriptStatusEstimate.RateStatuses(removedStatus);

            if (_v.Target.IsPlayer)
                return -1 * rating;

            return rating;
        }
    }
}
