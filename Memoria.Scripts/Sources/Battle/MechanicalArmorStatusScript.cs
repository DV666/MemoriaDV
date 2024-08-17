using System;
using UnityEngine;
using Memoria.Data;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.CustomStatus11)] // Mechanical Armor

    public class MechanicalArmorStatusScript : StatusScriptBase
    {
        public HUDMessageChild MechanicalArmorHUD = null;
        public Int32 Stack;
        public Int32 DefautSize;

        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            if (parameters[0] is not Int32)
                return btl_stat.ALTER_INVALID;

            Int32 level = (Int32)parameters[0];
            base.Apply(target, inflicter, parameters);
            BattleStatusDataEntry statusData = FF9StateSystem.Battle.FF9Battle.status_data[BattleStatusId.CustomStatus11];

            if (level > 0)
            {
                Stack = level;
                if (Stack > 1)
                {
                    if (MechanicalArmorHUD == null)
                    {
                        btl2d.GetIconPosition(target, btl2d.ICON_POS_DEFAULT, out Transform attachTransf, out Vector3 iconOff);
                        Vector3 OffSetPos = (statusData.SHPExtraPos + iconOff);
                        MechanicalArmorHUD = Singleton<HUDMessage>.Instance.Show(attachTransf, $"[FFA500]   {Stack}", HUDMessage.MessageStyle.DEATH_SENTENCE, OffSetPos, 0);
                        DefautSize = MechanicalArmorHUD.FontSize;
                        UILabel UILabelHUD = MechanicalArmorHUD.GetComponent<UILabel>();
                        UILabelHUD.spacingY = -10;
                        MechanicalArmorHUD.FontSize = 20;
                        MechanicalArmorHUD.Follower.clampToScreen = false;
                        target.AddDelayedModifier(UpdateMessageShow, null);
                        btl2d.StatusMessages.Add(MechanicalArmorHUD);
                    }
                    MechanicalArmorHUD.Label = $"[FFA500]   {Stack}";
                }
                else
                {
                    if (MechanicalArmorHUD != null)
                        MechanicalArmorHUD.Label = "";
                }
                return btl_stat.ALTER_SUCCESS;
            }
            else
            {
                btl_stat.RemoveStatus(target, BattleStatusId.CustomStatus11);
                return btl_stat.ALTER_SUCCESS_NO_SET;
            }           
        }

        private Boolean UpdateMessageShow(BattleUnit unit)
        {
            if (!unit.IsUnderAnyStatus(BattleStatusId.CustomStatus11))
                return false;
            if (btl2d.ShouldShowSPS && unit.Data.bi.disappear == 0)
                MechanicalArmorHUD.Label = $"[FFA500]   {Stack}";
            else
                MechanicalArmorHUD.Label = "";
            return true;
        }

        public override Boolean Remove()
        {
            Stack = 0;
            if (MechanicalArmorHUD != null)
            {
                MechanicalArmorHUD.FontSize = DefautSize;
                btl2d.StatusMessages.Remove(MechanicalArmorHUD);
                Singleton<HUDMessage>.Instance.ReleaseObject(MechanicalArmorHUD);
            }
            return true;
        }
    }
}
