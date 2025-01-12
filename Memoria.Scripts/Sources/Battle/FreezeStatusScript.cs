using System;
using Memoria.Data;
using Memoria.Scripts.Battle;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.Freeze)]
    public class FreezeStatusScript : StatusScriptBase
    {
        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            if (Target.IsUnderAnyStatus(BattleStatus.EasyKill))
            {
                if (TranceSeekCustomAPI.MonsterMechanic[target.Data][4] > 0)
                {
                    Target.Data.stat.duration_factor[BattleStatusId.Sleep] = (Target.Data.stat.duration_factor[BattleStatusId.Freeze] * TranceSeekCustomAPI.MonsterMechanic[target.Data][4]) / 100f;
                    TranceSeekCustomAPI.MonsterMechanic[target.Data][4] -= 20;
                }
                else
                    return btl_stat.ALTER_RESIST;
            }
            TranceSeekCustomAPI.SA_Strategist(inflicter);
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            return true;
        }
    }
}
