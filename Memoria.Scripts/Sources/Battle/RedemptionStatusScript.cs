using System;
using UnityEngine;
using Memoria.Data;
using Object = System.Object;
using System.Linq;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.CustomStatus12)] // Redemption
    public class RedemptionStatusScript : StatusScriptBase
    {
        public HUDMessageChild NumberHUD = null;
        public Int32 Stack;
        public Int32 DefautSize;
        public Boolean ShowNumberHUD;
        public Vector3 ModelScale;

        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            Boolean RedemptionSHPActivate = Configuration.Mod.FolderNames.Contains("TranceSeek/RedemptionIconInBattle");
            base.Apply(target, inflicter, parameters);
            OverlapSHP.SetupOverlappingSHP2(target);
            Int32 StackMaximum = target.HasSupportAbilityByIndex((SupportAbility)232) ? 3 : 2; // SA Expiation
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
                        target.RemoveStatus(BattleStatusId.CustomStatus12);
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
                        target.RemoveStatus(BattleStatusId.CustomStatus12);
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
            else if (Stack > 1 && RedemptionSHPActivate)
            {
                if (NumberHUD == null)
                {
                    BattleStatusDataEntry statusData = FF9StateSystem.Battle.FF9Battle.status_data[BattleStatusId.CustomStatus12];
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
            return true;
        }

        public void OnSHPShow(Boolean show)
        {
            ShowNumberHUD = show;
        }

        private Boolean UpdateMessageShow(BattleUnit unit)
        {
            if (!unit.IsUnderAnyStatus(BattleStatusId.CustomStatus12))
                return false;
            if (unit.Data.bi.disappear != 0 || Stack <= 1 || ModelScale != unit.ModelStatusScale || unit.IsUnderAnyStatus(BattleStatus.Death))
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
                BattleStatusDataEntry statusData = FF9StateSystem.Battle.FF9Battle.status_data[BattleStatusId.CustomStatus12];
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
