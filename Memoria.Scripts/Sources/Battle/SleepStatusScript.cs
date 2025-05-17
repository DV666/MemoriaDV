using System;
using Memoria.Data;
using Memoria.Scripts.Battle;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.Sleep)]
    public class SleepStatusScript : StatusScriptBase
    {
        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            if (target.IsUnderAnyStatus(BattleStatus.EasyKill))
            {
                target.AlterStatus(BattleStatus.CustomStatus17, inflicter); // Sleep Easy Kill
                return btl_stat.ALTER_SUCCESS_NO_SET;
            }
            base.Apply(target, inflicter, parameters);
            TranceSeekAPI.SA_StatusApply(inflicter, false);
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            return true;
        }
    }
}
