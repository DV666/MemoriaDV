﻿using System;
using Memoria.Data;
using Memoria.Prime;
using Memoria.Scripts.Battle;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.CustomStatus17)]
    public class SleepEasyKillStatusScript : StatusScriptBase
    {
        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            if (TranceSeekCustomAPI.MonsterMechanic[target.Data][4] > 0)
            {                                     
                Target.Data.stat.duration_factor[BattleStatusId.CustomStatus17] = (Target.Data.stat.duration_factor[BattleStatusId.CustomStatus17] * (TranceSeekCustomAPI.MonsterMechanic[target.Data][4]) / 100);
                TranceSeekCustomAPI.MonsterMechanic[target.Data][4] -= 20;
            }
            else
                return btl_stat.ALTER_RESIST;

            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            return true;
        }
    }
}
