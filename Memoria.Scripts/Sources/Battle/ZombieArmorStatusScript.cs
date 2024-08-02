using System;
using UnityEngine;
using Memoria.Data;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.CustomStatus10)] // Zombie Armor

    public class ZombieArmorStatusScript : StatusScriptBase
    {
        public HUDMessageChild ZombieArmorHUD = null;
        public Int32 BasicPhysicalDefence;
        public Int32 BasicMagicDefence;
        public Int32 Stack;
        public Int32 DefautSize;

        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            if (parameters[0] is not Int32)
                return btl_stat.ALTER_INVALID;

            Int32 damage = (Int32)parameters[0];
            base.Apply(target, inflicter, parameters);
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
                    if (ZombieArmorHUD != null)
                    {
                        ZombieArmorHUD.FontSize = DefautSize;
                        btl2d.StatusMessages.Remove(ZombieArmorHUD);
                        Singleton<HUDMessage>.Instance.ReleaseObject(ZombieArmorHUD);
                    }
                    btl2d.GetIconPosition(target, btl2d.ICON_POS_DEFAULT, out Transform attachTransf, out Vector3 iconOff);
                    Vector3 OffSetPos = (statusData.SHPExtraPos + iconOff);
                    ZombieArmorHUD = Singleton<HUDMessage>.Instance.Show(attachTransf, $"[FFA500]   {Stack}", HUDMessage.MessageStyle.DEATH_SENTENCE, OffSetPos, 0);
                    DefautSize = ZombieArmorHUD.FontSize;
                    UILabel UILabelHUD = ZombieArmorHUD.GetComponent<UILabel>();
                    UILabelHUD.spacingY = -10;
                    ZombieArmorHUD.FontSize = 20;
                    target.AddDelayedModifier(UpdateMessageShow, null);
                    btl2d.StatusMessages.Add(ZombieArmorHUD);
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
                    if (ZombieArmorHUD != null)
                    {
                        ZombieArmorHUD.FontSize = DefautSize;
                        btl2d.StatusMessages.Remove(ZombieArmorHUD);
                        Singleton<HUDMessage>.Instance.ReleaseObject(ZombieArmorHUD);
                    }
                    btl2d.GetIconPosition(Target, btl2d.ICON_POS_DEFAULT, out Transform attachTransf, out Vector3 iconOff);
                    Vector3 OffSetPos = (statusData.SHPExtraPos + iconOff);
                    ZombieArmorHUD = Singleton<HUDMessage>.Instance.Show(attachTransf, $"[FFA500]   {Stack}", HUDMessage.MessageStyle.DEATH_SENTENCE, OffSetPos, 0);
                    DefautSize = ZombieArmorHUD.FontSize;
                    UILabel UILabelHUD = ZombieArmorHUD.GetComponent<UILabel>();
                    UILabelHUD.spacingY = -10;
                    ZombieArmorHUD.FontSize = 20;
                    target.AddDelayedModifier(UpdateMessageShow, null);
                    btl2d.StatusMessages.Add(ZombieArmorHUD);
                }
                else
                {
                    ZombieArmorHUD.FontSize = DefautSize;
                    btl2d.StatusMessages.Remove(ZombieArmorHUD);
                    Singleton<HUDMessage>.Instance.ReleaseObject(ZombieArmorHUD);
                }
                target.PhysicalDefence = (byte)Math.Max(1, BasicPhysicalDefence + (4 * Stack));
                target.MagicDefence = (byte)Math.Max(1, BasicMagicDefence + (4 * Stack));
            }
            return btl_stat.ALTER_SUCCESS;
        }

        private Boolean UpdateMessageShow(BattleUnit unit)
        {
            if (!unit.IsUnderAnyStatus(BattleStatusId.CustomStatus10))
                return false;
            if (btl2d.ShouldShowSPS && unit.Data.bi.disappear == 0)
                Refresh(true);
            else
                Refresh(false);
            return true;
        }

        private void Refresh(Boolean KeepText)
        {
            BattleStatusDataEntry statusData = FF9StateSystem.Battle.FF9Battle.status_data[BattleStatusId.CustomStatus10];
            if (ZombieArmorHUD != null)
            {
                ZombieArmorHUD.FontSize = DefautSize;
                btl2d.StatusMessages.Remove(ZombieArmorHUD);
                Singleton<HUDMessage>.Instance.ReleaseObject(ZombieArmorHUD);
            }
            btl2d.GetIconPosition(Target, btl2d.ICON_POS_DEFAULT, out Transform attachTransf, out Vector3 iconOff);
            Vector3 OffSetPos = (statusData.SHPExtraPos + iconOff);
            ZombieArmorHUD = Singleton<HUDMessage>.Instance.Show(attachTransf, $"[FFA500]   {Stack}", HUDMessage.MessageStyle.DEATH_SENTENCE, OffSetPos, 0);
            DefautSize = ZombieArmorHUD.FontSize;
            UILabel UILabelHUD = ZombieArmorHUD.GetComponent<UILabel>();
            UILabelHUD.spacingY = -10;
            ZombieArmorHUD.FontSize = 20;
            if (KeepText)
                ZombieArmorHUD.Label = $"[FFA500]   {Stack}";
            else
                ZombieArmorHUD.Label = "";
            btl2d.StatusMessages.Add(ZombieArmorHUD);
        }

        public override Boolean Remove()
        {
            ZombieArmorHUD.FontSize = DefautSize;
            btl2d.StatusMessages.Remove(ZombieArmorHUD);
            Singleton<HUDMessage>.Instance.ReleaseObject(ZombieArmorHUD);
            Target.PhysicalDefence = (Byte)BasicPhysicalDefence;
            Target.MagicDefence = (Byte)BasicMagicDefence;
            return true;
        }
    }
}
