using System;
using UnityEngine;
using Memoria.Data;
using Object = System.Object;
using Memoria.Scripts.Battle;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.CustomStatus9)] // Dragon
    public class DragonStatusScript : StatusScriptBase
    {
        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            TranceSeekCustomAPI.SA_StatusApply(inflicter, false);
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            return true;
        }
    }
}
