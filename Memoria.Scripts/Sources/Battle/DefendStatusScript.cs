﻿using System;
using Memoria.Data;
using Memoria.Prime;
using Memoria.Scripts.Battle;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.Defend)]
    public class DefendStatusScript : StatusScriptBase, IFigurePointStatusScript
    {
        public Int32 Gardien;
        public Int32 Duel;

        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            if (target.PlayerIndex == CharacterId.Steiner)
                Gardien = 1;
            if (target.PlayerIndex == CharacterId.Amarant)
            {
                Duel = 1;
                TranceSeekCustomAPI.SpecialSAEffect[target.Data][0] = 1;
            }
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            Gardien = 0;
            Duel = 0;
            return true;
        }

        public void OnFigurePoint(ref UInt16 fig_info, ref Int32 fig, ref Int32 m_fig)
        {
            if (Target.PlayerIndex == CharacterId.Steiner && Target.IsUnderAnyStatus(BattleStatus.Defend) && Target.IsCovering)
                Gardien = 0;
            if (Target.PlayerIndex == CharacterId.Amarant && TranceSeekCustomAPI.SpecialSAEffect[Target.Data][0] == 1)
                Duel = 0;
        }
    }
}