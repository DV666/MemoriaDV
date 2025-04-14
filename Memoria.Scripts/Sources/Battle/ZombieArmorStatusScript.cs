using System;
using UnityEngine;
using Memoria.Data;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.CustomStatus10)] // Zombie Armor

    public class ZombieArmorStatusScript : StatusScriptBase
    {
        public HUDMessageChild NumberHUD = null;
        public Int32 BasicPhysicalDefence;
        public Int32 BasicMagicDefence;
        public Int32 Stack;
        public Int32 DefautSize;
        public Vector3 ModelScale;

        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            if (parameters[0] is not Int32)
                return btl_stat.ALTER_INVALID;

            Int32 damage = (Int32)parameters[0];
            base.Apply(target, inflicter, parameters);
            ModelScale = target.ModelStatusScale;
            BattleStatusDataEntry statusData = FF9StateSystem.Battle.FF9Battle.status_data[BattleStatusId.CustomStatus10];
            if (BasicPhysicalDefence == 0)
                BasicPhysicalDefence = Target.PhysicalDefence;
            if (BasicMagicDefence == 0)
                BasicMagicDefence = Target.MagicDefence;

            if (damage < 0)
            {
                Stack++;
                if (Stack > 20)
                    return btl_stat.ALTER_INVALID;
                if (Stack > 1)
                {
                    if (NumberHUD != null)
                    {
                        NumberHUD.FontSize = DefautSize;
                        btl2d.StatusMessages.Remove(NumberHUD);
                        Singleton<HUDMessage>.Instance.ReleaseObject(NumberHUD);
                    }
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
                target.PhysicalDefence = (byte)Math.Max(1, BasicPhysicalDefence + (4 * Stack));
                target.MagicDefence = (byte)Math.Max(1, BasicMagicDefence + (4 * Stack));
            }
            else
            {
                if (damage < 500)
                {
                    if (Stack > 0)
                        Stack--;
                    else
                        Stack = 0;
                }
                else if (damage < 1000)
                {
                    if (Stack > 2)
                        Stack -= 2;
                    else
                        Stack = 0;
                }
                else
                {
                    if (Stack > 4)
                        Stack -= 4;
                    else
                        Stack = 0;
                }
                if (Stack == 0)
                {
                    btl_stat.RemoveStatus(target, BattleStatusId.CustomStatus10);
                    return btl_stat.ALTER_SUCCESS_NO_SET;
                }
                if (Stack > 1)
                {
                    if (NumberHUD != null)
                    {
                        NumberHUD.FontSize = DefautSize;
                        btl2d.StatusMessages.Remove(NumberHUD);
                        Singleton<HUDMessage>.Instance.ReleaseObject(NumberHUD);
                    }
                    btl2d.GetIconPosition(Target, btl2d.ICON_POS_DEFAULT, out Transform attachTransf, out Vector3 iconOff);
                    Vector3 OffSetPos = (statusData.SHPExtraPos + iconOff);
                    NumberHUD = Singleton<HUDMessage>.Instance.Show(attachTransf, $"[FFA500]   {Stack}", HUDMessage.MessageStyle.DEATH_SENTENCE, OffSetPos, 0);
                    DefautSize = NumberHUD.FontSize;
                    UILabel UILabelHUD = NumberHUD.GetComponent<UILabel>();
                    UILabelHUD.spacingY = -10;
                    NumberHUD.FontSize = 20;
                    target.AddDelayedModifier(UpdateMessageShow, null);
                    btl2d.StatusMessages.Add(NumberHUD);
                }
                else
                {
                    NumberHUD.FontSize = DefautSize;
                    btl2d.StatusMessages.Remove(NumberHUD);
                    Singleton<HUDMessage>.Instance.ReleaseObject(NumberHUD);
                }
                target.PhysicalDefence = (byte)Math.Max(1, BasicPhysicalDefence + (4 * Stack));
                target.MagicDefence = (byte)Math.Max(1, BasicMagicDefence + (4 * Stack));
            }
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
            Target.PhysicalDefence = (Byte)BasicPhysicalDefence;
            Target.MagicDefence = (Byte)BasicMagicDefence;
            return true;
        }

        private Boolean UpdateMessageShow(BattleUnit unit)
        {
            if (!unit.IsUnderAnyStatus(BattleStatusId.CustomStatus10))
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
                BattleStatusDataEntry statusData = FF9StateSystem.Battle.FF9Battle.status_data[BattleStatusId.CustomStatus10];
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
