using System;
using UnityEngine;
using Memoria.Data;
using static Memoria.Scripts.Battle.TranceSeekAPI;
using Object = System.Object;
using Memoria.Scripts.Battle;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.CustomStatus7)] // Armor Up

    public class ArmorUpStatusScript : StatusScriptBase
    {
        public HUDMessageChild NumberHUD = null;
        public Int32 BasicPhysicalDefence;
        public Int32 Stack;
        public Int32 DefautSize;
        public Boolean ShowNumberHUD;
        public Vector3 ModelScale;

        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            OverlapSHP.SetupOverlappingSHP1(target);
            Int32 StackMaximum = 5;
            ModelScale = target.ModelStatusScale;
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
                        target.RemoveStatus(BattleStatusId.CustomStatus7);
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
                        target.RemoveStatus(BattleStatusId.CustomStatus7);
                        return btl_stat.ALTER_SUCCESS_NO_SET;
                    }
                }
            }
            else
            {
                Stack++;
            }
            if (target.IsUnderAnyStatus(BattleStatusId.CustomStatus3))
            {
                Int32 ReduceStack = Stack;
                for (int i = 0; i < ReduceStack; i++)
                {
                    if (target.IsUnderAnyStatus(BattleStatusId.CustomStatus3))
                    {
                        btl_stat.AlterStatus(Target, BattleStatusId.CustomStatus3, parameters: "Remove");
                        Stack--;
                        if (Stack <= 0)
                            return btl_stat.ALTER_SUCCESS_NO_SET;
                    }
                }
            }
            if (BasicPhysicalDefence == 0)
                BasicPhysicalDefence = Target.PhysicalDefence;

            StackBreakOrUpStatus[Target.Data][2] = (Stack * 10);

            if (Stack > StackMaximum)
            {
                Stack = StackMaximum;
                return btl_stat.ALTER_INVALID;
            }
            else if (Stack > 1)
            {
                if (NumberHUD == null)
                {
                    BattleStatusDataEntry statusData = FF9StateSystem.Battle.FF9Battle.status_data[BattleStatusId.CustomStatus7];
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
            }
            else
            {
                if (NumberHUD != null)
                {
                    NumberHUD.FontSize = DefautSize;
                    btl2d.StatusMessages.Remove(NumberHUD);
                    Singleton<HUDMessage>.Instance.ReleaseObject(NumberHUD);
                }
            }
            target.PhysicalDefence = (byte)Math.Min(BasicPhysicalDefence + ((BasicPhysicalDefence * Stack) / 10), 255);
            TranceSeekAPI.SA_StatusApply(inflicter, true);
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            Stack = 0;
            if (NumberHUD != null)
            {
                NumberHUD.FontSize = DefautSize;
                btl2d.StatusMessages.Remove(NumberHUD);
                Singleton<HUDMessage>.Instance.ReleaseObject(NumberHUD);
            }
            Target.PhysicalDefence = (Byte)BasicPhysicalDefence;
            return true;
        }

        public void OnSHPShow(Boolean show)
        {
            ShowNumberHUD = show;
        }

        private Boolean UpdateMessageShow(BattleUnit unit)
        {
            if (!unit.IsUnderAnyStatus(BattleStatusId.CustomStatus7))
                return false;
            if (unit.Data.bi.disappear != 0 || Stack <= 1 || ModelScale != unit.ModelStatusScale || !unit.Data.gameObject.activeSelf)
            {
                ModelScale = unit.ModelStatusScale;
                if (NumberHUD != null)
                {
                    NumberHUD.FontSize = DefautSize;
                    btl2d.StatusMessages.Remove(NumberHUD);
                    Singleton<HUDMessage>.Instance.ReleaseObject(NumberHUD);
                    NumberHUD = null;
                }
                return true;
            }

            if (NumberHUD == null)
            {
                BattleStatusDataEntry statusData = FF9StateSystem.Battle.FF9Battle.status_data[BattleStatusId.CustomStatus7];
                btl2d.GetIconPosition(Target, btl2d.ICON_POS_DEFAULT, out Transform attachTransf, out Vector3 iconOff);
                Vector3 OffSetPos = (statusData.SHPExtraPos + iconOff);
                NumberHUD = Singleton<HUDMessage>.Instance.Show(attachTransf, $"[FFA500]   {Stack}", HUDMessage.MessageStyle.DEATH_SENTENCE, OffSetPos, 0);
                DefautSize = NumberHUD.FontSize;
                UILabel UILabelHUD = NumberHUD.GetComponent<UILabel>();
                UILabelHUD.spacingY = -10;
                NumberHUD.FontSize = 20;
                NumberHUD.Follower.clampToScreen = false;
                btl2d.StatusMessages.Add(NumberHUD);
            }

            if (btl2d.ShouldShowSPS && ShowNumberHUD)
                NumberHUD.Label = $"[FFA500]   {Stack}";
            else
                NumberHUD.Label = "";
            return true;
        }
    }
}
