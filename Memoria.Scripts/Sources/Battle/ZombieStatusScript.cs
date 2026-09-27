using System;
using Memoria.Data;
using Memoria.Scripts.TranceSeek;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.Zombie)]
    public class ZombieStatusScript : StatusScriptBase
    {
        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            if (inflicter == null)
                inflicter = target;
            base.Apply(target, inflicter, parameters);
            if (target.IsZombie)
                return btl_stat.ALTER_INVALID;
            if (target.IsPlayer && !target.IsUnderAnyStatus(BattleStatus.Trance))
                target.Trance = 0;
            TranceSeekAPI.SA_StatusApply(inflicter, false);
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            return true;
        }
    }
}
