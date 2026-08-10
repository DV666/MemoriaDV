using FF9;
using Memoria.Data;
using Memoria.Prime;
using Memoria.Scripts.TranceSeek;
using NCalc;
using System;
using static SiliconStudio.Social.ResponseData;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.Stop)]
    public class StopStatusScript : StatusScriptBase
    {
        public Int32 Duration;

        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            target.UISpriteATB = BattleHUD.ATEGray;

            if (inflicter == null)
                inflicter = target;

            // Since Stop is freezing the ATB, we need to use this functions to "calculate" a kind of counti value.
            Duration = CalculationDuration(target, inflicter);
            target.AddDelayedModifier(CountiCountForStop, null);

            TranceSeekAPI.SA_StatusApply(inflicter, false);
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            Target.UISpriteATB = BattleHUD.ATENormal;
            return true;
        }

        private int CalculationDuration(BattleUnit target, BattleUnit inflicter)
        {
            STAT_INFO stat = target.Data.stat;
            BattleStatusDataEntry statusData = FF9StateSystem.Battle.FF9Battle.status_data[BattleStatusId.Stop];
            Int16 defaultFactor = (Int16)(200 + inflicter.Will * 2 - target.Will);
            int CountiStop = (Int16)(statusData.ContiCnt * defaultFactor);

            if (target.IsUnderAnyStatus(BattleStatus.EasyKill))
            {
                var Target_TSVar = target.State();
                target.Data.stat.duration_factor[BattleStatusId.Stop] = (target.Data.stat.duration_factor[BattleStatusId.Stop] * (Target_TSVar.Monster.DurationDeadlyStatus) / 100);
                Target_TSVar.Monster.DurationDeadlyStatus -= 20;
            }

            return (Int16)(stat.duration_factor[BattleStatusId.Stop] * CountiStop);
        }

        private Boolean CountiCountForStop(BattleUnit target)
        {
            if (Duration > 0)
            {
                Duration -= btl_para.GetATBCoef() * BattleState.ATBTickCount;
                return true;
            }
            else
            {
                target.RemoveStatus(BattleStatus.Stop);
                return false;
            }
        }
    }
}

