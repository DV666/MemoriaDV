using System;
using UnityEngine;
using Memoria.Data;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.CustomStatus19)] // Rage
    public class RageStatusScript : StatusScriptBase
    {
        public HUDMessageChild NumberHUD = null;
        public Int32 Stack;
        public Int32 DefautSize;

        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            BattleStatusDataEntry statusData = FF9StateSystem.Battle.FF9Battle.status_data[BattleStatusId.CustomStatus19];
            Int32 StackMaximum = 3;
            if (parameters.Length > 0)
            {
                String Parameter = parameters[0] as String;
                if (Parameter == "Add")
                {
                    Stack++;
                    if (Stack > StackMaximum)
                        Stack = StackMaximum;
                }
                else if (Parameter == "Remove")
                {
                    Stack--;
                    if (Stack == 0)
                    {
                        target.RemoveStatus(BattleStatusId.CustomStatus19);
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
            if (Stack < StackMaximum)
            {
                if (NumberHUD != null)
                {
                    NumberHUD.FontSize = DefautSize;
                    btl2d.StatusMessages.Remove(NumberHUD);
                    Singleton<HUDMessage>.Instance.ReleaseObject(NumberHUD);
                }
                if (Stack > 1)
                {
                    btl2d.GetIconPosition(target, btl2d.ICON_POS_DEFAULT, out Transform attachTransf, out Vector3 iconOff);
                    Vector3 OffSetPos = (statusData.SHPExtraPos + iconOff);
                    NumberHUD = Singleton<HUDMessage>.Instance.Show(attachTransf, $"[FFA500]   {Stack}", HUDMessage.MessageStyle.DEATH_SENTENCE, OffSetPos, 0);
                    DefautSize = NumberHUD.FontSize;
                    UILabel UILabelHUD = NumberHUD.GetComponent<UILabel>();
                    UILabelHUD.spacingY = -10;
                    NumberHUD.FontSize = 20;
                    NumberHUD.Follower.clampToScreen = false;
                    target.AddDelayedModifier(UpdateMessageShow, null);
                    btl2d.StatusMessages.Add(NumberHUD);
                }
                return btl_stat.ALTER_SUCCESS;
            }
            return btl_stat.ALTER_SUCCESS_NO_SET;
        }

        public override Boolean Remove()
        {
            NumberHUD.FontSize = DefautSize;
            btl2d.StatusMessages.Remove(NumberHUD);
            Singleton<HUDMessage>.Instance.ReleaseObject(NumberHUD);
            return true;
        }

        private Boolean UpdateMessageShow(BattleUnit unit)
        {
            if (!unit.IsUnderAnyStatus(BattleStatusId.CustomStatus19))
                return false;
            if (btl2d.ShouldShowSPS && unit.Data.bi.disappear == 0)
                Refresh(true);
            else
                Refresh(false);
            return true;
        }

        private void Refresh(Boolean KeepText)
        {
            BattleStatusDataEntry statusData = FF9StateSystem.Battle.FF9Battle.status_data[BattleStatusId.CustomStatus19];
            if (NumberHUD != null)
            {
                NumberHUD.FontSize = DefautSize;
                btl2d.StatusMessages.Remove(NumberHUD);
                Singleton<HUDMessage>.Instance.ReleaseObject(NumberHUD);
            }
            if (Stack > 1)
            {
                btl2d.GetIconPosition(Target, btl2d.ICON_POS_DEFAULT, out Transform attachTransf, out Vector3 iconOff);
                Vector3 OffSetPos = (statusData.SHPExtraPos + iconOff);
                NumberHUD = Singleton<HUDMessage>.Instance.Show(attachTransf, $"[FFA500]   {Stack}", HUDMessage.MessageStyle.DEATH_SENTENCE, OffSetPos, 0);
                DefautSize = NumberHUD.FontSize;
                UILabel UILabelHUD = NumberHUD.GetComponent<UILabel>();
                UILabelHUD.spacingY = -10;
                NumberHUD.FontSize = 20;
                NumberHUD.Follower.clampToScreen = false;
                if (KeepText)
                    NumberHUD.Label = $"[FFA500]   {Stack}";
                else
                    NumberHUD.Label = "";
                btl2d.StatusMessages.Add(NumberHUD);
            }
        }
    }
}
