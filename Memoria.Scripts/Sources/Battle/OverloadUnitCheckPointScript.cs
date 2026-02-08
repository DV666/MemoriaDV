using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Data;
using System;
using System.Linq;
using UnityEngine;

namespace Memoria.Scripts.Battle
{
    public class OverloadUnitCheckPointScript : IOverloadUnitCheckPointScript
    {
        public BattleStatus UpdatePointStatus(BattleUnit unit)
        {
            if (!unit.IsPlayer)
                return 0;

            Boolean HPColored = Configuration.Mod.FolderNames.Contains("TranceSeek/ColoredHP");
            Boolean MPColored = Configuration.Mod.FolderNames.Contains("TranceSeek/ColoredMP");

            Boolean isLowHP = unit.IsPlayer && unit.CurrentHp * 6 <= unit.MaximumHp;
            if (isLowHP)
            {
                unit.UIColorHP = FF9TextTool.Yellow;
                if (!btl_stat.CheckStatus(unit, BattleStatus.LowHP))
                    btl_stat.AlterStatus(unit, BattleStatusId.LowHP);
            }
            else
            {
                if (unit.IsPlayer && unit.CurrentHp == unit.MaximumHp && HPColored)
                    unit.UIColorHP = FF9TextTool.Green;
                else
                    unit.UIColorHP = FF9TextTool.White;

                btl_stat.RemoveStatus(unit, BattleStatusId.LowHP);
            }

            if (unit.IsPlayer && unit.CurrentMp == unit.MaximumMp && MPColored)
                unit.UIColorMP = new Color(0.28104f, 0.43712f, 0.96821f);
            else
                unit.UIColorMP = unit.CurrentMp <= unit.MaximumMp / 6f ? FF9TextTool.Yellow : FF9TextTool.White;

            return isLowHP ? BattleStatus.LowHP : 0;
        }
    }
}
