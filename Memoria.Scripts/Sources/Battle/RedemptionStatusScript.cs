using System;
using UnityEngine;
using Memoria.Data;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.CustomStatus12)] // Redemption
    public class RedemptionStatusScript : StatusScriptBase
    {
        public HUDMessageChild RedemptionHUD = null;
        public Int32 Stack;
        public Int32 DefautSize;

        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            if (parameters.Length > 0)
            {
                String Parameter = parameters[0] as String;
                if (Parameter == "Add")
                {
                    Stack++;
                    if (Stack > 2)
                        Stack = 2;
                }
                else if (Parameter == "Remove")
                {
                    Stack--;
                    if (Stack == 0)
                    {
                        target.RemoveStatus(BattleStatusId.CustomStatus12);
                        return btl_stat.ALTER_SUCCESS_NO_SET;
                    }
                }
                else
                {
                    Int32.TryParse(Parameter, out Int32 PutStack);
                    Stack = PutStack;
                }
            }
            else
            {
                Stack++;
            }
            BattleStatusDataEntry statusData = FF9StateSystem.Battle.FF9Battle.status_data[BattleStatusId.CustomStatus12];

            if (Stack > 2)
            {
                Stack = 2;
                return btl_stat.ALTER_SUCCESS_NO_SET;
            }
            else if (Stack > 1)
            {
                if (RedemptionHUD != null)
                {
                    RedemptionHUD.FontSize = DefautSize;
                    btl2d.StatusMessages.Remove(RedemptionHUD);
                    Singleton<HUDMessage>.Instance.ReleaseObject(RedemptionHUD);
                }
                btl2d.GetIconPosition(target, btl2d.ICON_POS_DEFAULT, out Transform attachTransf, out Vector3 iconOff);
                Vector3 OffSetPos = (statusData.SHPExtraPos + iconOff);
                RedemptionHUD = Singleton<HUDMessage>.Instance.Show(attachTransf, $"[FFA500]   {Stack}", HUDMessage.MessageStyle.DEATH_SENTENCE, OffSetPos, 0);
                DefautSize = RedemptionHUD.FontSize;
                UILabel UILabelHUD = RedemptionHUD.GetComponent<UILabel>();
                UILabelHUD.spacingY = -10;
                RedemptionHUD.FontSize = 20;
                RedemptionHUD.Follower.clampToScreen = false;
                target.AddDelayedModifier(UpdateMessageShow, null);
                btl2d.StatusMessages.Add(RedemptionHUD);
            }
            else if (RedemptionHUD != null)
            {
                RedemptionHUD.FontSize = DefautSize;
                btl2d.StatusMessages.Remove(RedemptionHUD);
                Singleton<HUDMessage>.Instance.ReleaseObject(RedemptionHUD);
            }
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            Stack = 0;
            if (RedemptionHUD != null)
            {
                RedemptionHUD.FontSize = DefautSize;
                btl2d.StatusMessages.Remove(RedemptionHUD);
                Singleton<HUDMessage>.Instance.ReleaseObject(RedemptionHUD);
            }
            return true;
        }

        private Boolean UpdateMessageShow(BattleUnit unit)
        {
            if (!unit.IsUnderAnyStatus(BattleStatusId.CustomStatus12))
                return false;
            if (btl2d.ShouldShowSPS && unit.Data.bi.disappear == 0)
                Refresh(true);
            else
                Refresh(false);
            return true;
        }

        private void Refresh(Boolean KeepText)
        {
            BattleStatusDataEntry statusData = FF9StateSystem.Battle.FF9Battle.status_data[BattleStatusId.CustomStatus12];
            if (RedemptionHUD != null)
            {
                RedemptionHUD.FontSize = DefautSize;
                btl2d.StatusMessages.Remove(RedemptionHUD);
                Singleton<HUDMessage>.Instance.ReleaseObject(RedemptionHUD);
            }
            if (Stack > 1)
            {
                btl2d.GetIconPosition(Target, btl2d.ICON_POS_DEFAULT, out Transform attachTransf, out Vector3 iconOff);
                Vector3 OffSetPos = (statusData.SHPExtraPos + iconOff);
                RedemptionHUD = Singleton<HUDMessage>.Instance.Show(attachTransf, $"[FFA500]   {Stack}", HUDMessage.MessageStyle.DEATH_SENTENCE, OffSetPos, 0);
                DefautSize = RedemptionHUD.FontSize;
                UILabel UILabelHUD = RedemptionHUD.GetComponent<UILabel>();
                UILabelHUD.spacingY = -10;
                RedemptionHUD.FontSize = 20;
                RedemptionHUD.Follower.clampToScreen = false;
                if (KeepText)
                    RedemptionHUD.Label = $"[FFA500]   {Stack}";
                else
                    RedemptionHUD.Label = "";
                btl2d.StatusMessages.Add(RedemptionHUD);
            }
        }
    }
}
