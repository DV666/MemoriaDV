using System;
using System.Collections.Generic;
using Memoria.Data;
using Memoria.Prime;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.CustomStatus21)] // Free slot for new status (previously, was SpecialStatus but converted into MemoriaDict or TranceSeekBattleDict)
    public class SpecialStatusScript : StatusScriptBase
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
