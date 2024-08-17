using System;
using UnityEngine;
using Memoria.Data;
using Object = System.Object;
using Memoria.Scripts.Battle;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.CustomStatus1)] // Power Break

    public class PowerBreakStatusScript : StatusScriptBase
    {
        public HUDMessageChild NumberHUD = null;
        public Int32 BasicStrength;
        public Int32 Stack;
        public Int32 DefautSize;

        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            if (target.IsUnderAnyStatus(BattleStatus.EasyKill) && target.IsUnderAnyStatus(BattleStatus.CustomStatus1))
                return btl_stat.ALTER_INVALID;
            base.Apply(target, inflicter, parameters);
            Int32 StackMaximum = 9;
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
                        target.RemoveStatus(BattleStatusId.CustomStatus1);
                        return btl_stat.ALTER_SUCCESS_NO_SET;
                    }
                }
                else
                {
                    Int32.TryParse(Parameter, out Int32 PutStack);
                    Stack += PutStack;
                    if (Stack > StackMaximum)
                        Stack = StackMaximum;
                    else if (Stack <= 0)
                    {
                        target.RemoveStatus(BattleStatusId.CustomStatus1);
                        return btl_stat.ALTER_SUCCESS_NO_SET;
                    }
                }
            }
            else
            {
                Stack++;
            }
            if (target.IsUnderAnyStatus(BattleStatusId.CustomStatus5))
            {
                btl_stat.AlterStatus(Target, BattleStatusId.CustomStatus5, parameters: "Remove");
                return btl_stat.ALTER_SUCCESS_NO_SET;
            }
            if (BasicStrength == 0)
                BasicStrength = Target.Strength;

            if (Stack > StackMaximum)
            {
                Stack = StackMaximum;
                NumberHUD.Label = $"[FFA500]   {Stack}";
                return btl_stat.ALTER_INVALID;
            }
            else if (Stack > 1)
            {
                if (NumberHUD == null)
                {
                    BattleStatusDataEntry statusData = FF9StateSystem.Battle.FF9Battle.status_data[BattleStatusId.CustomStatus1];
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
                NumberHUD.Label = $"[FFA500]   {Stack}";
            }
            else
            {
                if (NumberHUD != null)
                    NumberHUD.Label = "";
            }
            target.Strength = (byte)Math.Max(1, BasicStrength - (BasicStrength * Stack) / 10);
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            if (NumberHUD != null)
            {
                NumberHUD.FontSize = DefautSize;
                btl2d.StatusMessages.Remove(NumberHUD);
                Singleton<HUDMessage>.Instance.ReleaseObject(NumberHUD);
            }
            Target.Strength = (Byte)BasicStrength;
            return true;
        }

        private Boolean UpdateMessageShow(BattleUnit unit)
        {
            if (!unit.IsUnderAnyStatus(BattleStatusId.CustomStatus1))
                return false;
            SHPEffect shp = HonoluluBattleMain.battleSPS.GetBtlSHPObj(unit, BattleStatusId.CustomStatus1);
            if (btl2d.ShouldShowSPS && unit.Data.bi.disappear == 0)
                Refresh(true);
            else
                Refresh(false);
            return true;
        }

        private void Refresh(Boolean KeepText)
        {
            BattleStatusDataEntry statusData = FF9StateSystem.Battle.FF9Battle.status_data[BattleStatusId.CustomStatus1];
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
