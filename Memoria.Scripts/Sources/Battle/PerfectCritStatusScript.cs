using System;
using UnityEngine;
using Memoria.Data;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.CustomStatus15)] // Perfect Crit
    public class PerfectCritStatusScript : StatusScriptBase
    {
        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            return true;
        }
    }
}
