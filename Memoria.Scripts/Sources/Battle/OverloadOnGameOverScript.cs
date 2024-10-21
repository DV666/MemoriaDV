using System;
using FF9;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    public class OverloadOnGameOverScript : IOverloadOnGameOverScript
    {
        public Boolean OnGameOver(FF9StateBattleSystem state, BattleUnit dyingPC)
        {
            for (BTL_DATA btl = state.btl_list.next; btl != null; btl = btl.next)
            {
                if (btl.bi.player != 0 && (CharacterId)btl.bi.slot_no == CharacterId.Eiko)
                {
                    if (!btl_stat.CheckStatus(btl, BattleStatusConst.NoRebirthFlame))
                    {
                        if (btl_cmd.CheckSpecificCommand(btl, BattleCommandId.SysLastPhoenix))
                            return false;

                        BattleUnit unit = new BattleUnit(btl);
                        if (unit.Accessory == RegularItem.PhoenixPinion && ff9item.FF9Item_GetCount(RegularItem.PhoenixPinion) > Comn.random8())
                        {
                            UIManager.Battle.FF9BMenu_EnableMenu(true);
                            btl_cmd.SetCommand(btl.cmd[0], BattleCommandId.SysLastPhoenix, (Int32)BattleAbilityId.RebirthFlame, btl_scrp.GetBattleID(0U), 1u);
                            FF9StateSystem.Common.FF9.GetPlayer((CharacterId)btl.bi.slot_no).equip.Accessory = RegularItem.NoItem;
                            return true;
                        }
                    }
                    break;
                }
            }
            return false;
        }
    }
}
