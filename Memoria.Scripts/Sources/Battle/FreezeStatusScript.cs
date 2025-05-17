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
                if (TranceSeekAPI.MonsterMechanic[target.Data][4] > 0)
                {
                    Target.Data.stat.duration_factor[BattleStatusId.Sleep] = (Target.Data.stat.duration_factor[BattleStatusId.Freeze] * TranceSeekAPI.MonsterMechanic[target.Data][4]) / 100f;
                    TranceSeekAPI.MonsterMechanic[target.Data][4] -= 20;
                }
                else
                    return btl_stat.ALTER_RESIST;
            }
            TranceSeekAPI.SA_StatusApply(inflicter, false);
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            return true;
        }
    }
}
