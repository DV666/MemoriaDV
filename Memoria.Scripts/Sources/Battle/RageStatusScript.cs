using System;
using UnityEngine;
using Memoria.Data;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.CustomStatus19)] // Rage
    public class RageStatusScript : StatusScriptBase
    {
        public HUDMessageChild RedemptionHUD = null;
        public Int32 Stack;
        public Int32 DefautSize;

        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            BattleStatusDataEntry statusData = FF9StateSystem.Battle.FF9Battle.status_data[BattleStatusId.CustomStatus19];
            Int32 StackMaximum = 3;
            if (Stack < StackMaximum)
            {
                Stack++;
                if (RedemptionHUD != null)
                {
                    RedemptionHUD.FontSize = DefautSize;
                    btl2d.StatusMessages.Remove(RedemptionHUD);
                    Singleton<HUDMessage>.Instance.ReleaseObject(RedemptionHUD);
                }
                if (Stack > 1)
                {
                    btl2d.GetIconPosition(target, btl2d.ICON_POS_DEFAULT, out Transform attachTransf, out Vector3 iconOff);
                    Vector3 OffSetPos = (statusData.SHPExtraPos + iconOff);
                    RedemptionHUD = Singleton<HUDMessage>.Instance.Show(attachTransf, $"[FFA500]   {Stack}", HUDMessage.MessageStyle.DEATH_SENTENCE, OffSetPos, 0);
                    DefautSize = RedemptionHUD.FontSize;
                    UILabel UILabelHUD = RedemptionHUD.GetComponent<UILabel>();
                    UILabelHUD.spacingY = -10;
                    RedemptionHUD.FontSize = 20;
                    btl2d.StatusMessages.Add(RedemptionHUD);
                }
                return btl_stat.ALTER_SUCCESS;
            }
            return btl_stat.ALTER_SUCCESS_NO_SET;
        }

        public override Boolean Remove()
        {
            RedemptionHUD.FontSize = DefautSize;
            btl2d.StatusMessages.Remove(RedemptionHUD);
            Singleton<HUDMessage>.Instance.ReleaseObject(RedemptionHUD);
            return true;
        }
    }
}
