using System;
using UnityEngine;
using Memoria.Data;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.CustomStatus9)] // Dragon
    public class DragonStatusScript : StatusScriptBase
    {
        public HUDMessageChild NumberHUD = null;
        public Int32 Stack;
        public Int32 DefautSize;
        public Int32 DiffPhysicalEvade;
        public Int32 DiffMagicalEvade;
        public Boolean ShowNumberHUD;
        public Vector3 ModelScale;

        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            Int32 StackMaximum = 1;
            if (inflicter != null)
                StackMaximum = inflicter.HasSupportAbilityByIndex((SupportAbility)1218) ? 3 : (inflicter.HasSupportAbilityByIndex((SupportAbility)218) ? 2 : 1);
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
                    if (Stack <= 0)
                    {
                        target.RemoveStatus(BattleStatusId.CustomStatus9);
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
                        target.RemoveStatus(BattleStatusId.CustomStatus9);
                        return btl_stat.ALTER_SUCCESS_NO_SET;
                    }
                }
            }
            else
            {
                Stack++;
            }

            if (Stack > StackMaximum)
            {
                Stack = StackMaximum;
                if (NumberHUD != null)
                    NumberHUD.Label = $"[FFA500]   {Stack}";
                return btl_stat.ALTER_INVALID;
            }
            else if (Stack > 1)
            {
                if (NumberHUD == null)
                {
                    BattleStatusDataEntry statusData = FF9StateSystem.Battle.FF9Battle.status_data[BattleStatusId.CustomStatus9];
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
            if (inflicter != null)
            {
                if (inflicter.HasSupportAbilityByIndex((SupportAbility)219)) // SA Embrace
                {
                    DiffPhysicalEvade = target.PhysicalEvade / 4;
                    target.PhysicalEvade = Math.Max(0, target.PhysicalEvade - DiffPhysicalEvade);
                    if (inflicter.HasSupportAbilityByIndex((SupportAbility)1219))
                    {
                        DiffMagicalEvade = target.MagicEvade / 4;
                        target.MagicEvade = Math.Max(0, target.MagicEvade - DiffMagicalEvade);
                    }
                }
            }
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
            if (DiffPhysicalEvade > 0)
            {
                Target.PhysicalEvade = Math.Min(255, Target.PhysicalEvade + DiffPhysicalEvade);
            }
            if (DiffMagicalEvade > 0)
            {
                Target.MagicEvade = Math.Min(255, Target.MagicEvade + DiffMagicalEvade);
            }
            return true;
        }

        private Boolean UpdateMessageShow(BattleUnit unit)
        {
            if (!unit.IsUnderAnyStatus(BattleStatusId.CustomStatus9))
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
                BattleStatusDataEntry statusData = FF9StateSystem.Battle.FF9Battle.status_data[BattleStatusId.CustomStatus9];
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

            if (btl2d.ShouldShowSPS)
                NumberHUD.Label = $"[FFA500]   {Stack}";
            else
                NumberHUD.Label = "";
            return true;
        }
    }
}
