using System;
using Memoria.Data;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.Zombie)]
    public class ZombieStatusScript : StatusScriptBase
    {
        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            if (target.IsPlayer && !target.IsUnderAnyStatus(BattleStatus.Trance))
                target.Trance = 0;
            if (Target.IsUnderAnyStatus(BattleStatus.EasyKill))
            {
                BattleStatusDataEntry statusData = FF9StateSystem.Battle.FF9Battle.status_data[BattleStatusId.Poison];
                Int32 wait = (short)((400 + (inflicter.Will * 2) - target.Will) * statusData.ContiCnt);
                Target.AddDelayedModifier(
                target => (wait -= target.Data.cur.at_coef) > 0,
                target =>
                {
                    target.RemoveStatus(BattleStatus.Zombie);
                }
                );
            }
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            return true;
        }
    }
}
