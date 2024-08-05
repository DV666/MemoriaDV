using System;
using Memoria.Data;
using UnityEngine;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.AutoLife)]
    public class AutoLifeStatusScript : StatusScriptBase, IDeathChangerStatusScript
    {
        public UInt32 HPRestore = 1;

        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            HPRestore = Math.Max(HPRestore, parameters.Length > 0 ? (UInt32)parameters[0] : 1);
            target.AddDelayedModifier(UpdateSPS, null);
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            return true;
        }

        public Boolean OnDeath()
        {
            btl_stat.RemoveStatus(Target, BattleStatusId.AutoLife);
            if (HPRestore > 0)
            {
                Target.CurrentHp = Math.Min(HPRestore, Target.MaximumHp);
                btl_stat.RemoveStatus(Target, BattleStatusId.Death);
            }
            BattleVoice.TriggerOnStatusChange(Target, "Used", BattleStatusId.AutoLife);
            return true;
        }

        private Boolean UpdateSPS(BattleUnit unit)
        {
            if (!unit.IsUnderAnyStatus(BattleStatusId.AutoLife))
                return false;
            SPSEffect sps = HonoluluBattleMain.battleSPS.GetBtlSPSObj(unit, BattleStatusId.AutoLife);
            sps.rotMode = 1;
            sps.rot = new Vector3(90f, 0, 0);
            return true;
        }
    }
}
