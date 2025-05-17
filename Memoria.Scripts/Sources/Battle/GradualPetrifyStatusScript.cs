using System;
using UnityEngine;
using Memoria.Data;
using FF9;
using Object = System.Object;
using System.Security.Cryptography;
using Memoria.Scripts.Battle;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.GradualPetrify)]
    public class GradualPetrifyStatusScript : StatusScriptBase, IOprStatusScript
    {
        public HUDMessageChild Message = null;
        public Int32 InitialCounter;
        public Int32 GeoID;
        public Int32 Counter;

        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            btl2d.GetIconPosition(target, btl2d.ICON_POS_NUMBER, out Transform attachTransf, out Vector3 iconOff);
            InitialCounter = parameters.Length > 0 ? Convert.ToInt32(parameters[0]) : 10;
            InitialCounter *= (Target.HasSupportAbility(SupportAbility1.AutoRegen) ? 2 : 1);
            InitialCounter *= (TranceSeekAPI.EliteMonster(target.Data) ? 3 : 1);
            Counter = InitialCounter;
            Message = Singleton<HUDMessage>.Instance.Show(attachTransf, $"{Counter}", HUDMessage.MessageStyle.DEATH_SENTENCE, new Vector3(0f, iconOff.y), 0);
            btl2d.StatusMessages.Add(Message);
            UpdateLabel();
            target.AddDelayedModifier(UpdateMessageShow, null);
            GeoID = target.Data.dms_geo_id;
            TranceSeekAPI.SA_StatusApply(inflicter, false);
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            btl_cmd.KillSpecificCommand(Target, BattleCommandId.SysStone);
            btl2d.StatusMessages.Remove(Message);
            Singleton<HUDMessage>.Instance.ReleaseObject(Message);
            return true;
        }

        public IOprStatusScript.SetupOprMethod SetupOpr => SetupGradualPetrifyOpr;
        public Int32 SetupGradualPetrifyOpr()
        {
            // Use the duration "ContiCnt" of GradualPetrify even if it is not registered as BattleStatusConst.ContiCount
            return (Int32)(Target.StatusDurationFactor[BattleStatusId.GradualPetrify] * BattleStatusId.GradualPetrify.GetStatData().ContiCnt * (60 - Target.Will << 3) / 10);
        }

        public Boolean OnOpr()
        {
            if (Message != null)
            {
                Counter--;
                if (Counter > 0)
                {
                    UpdateLabel();
                    return false;
                }
                if (!btl_cmd.CheckUsingCommand(Target.PetrifyCommand))
                {
                    UInt32 tryPetrify = btl_stat.AlterStatus(Target, BattleStatusId.Petrify, Inflicter);
                    if (tryPetrify == btl_stat.ALTER_SUCCESS || tryPetrify == btl_stat.ALTER_SUCCESS_NO_SET)
                        BattleVoice.TriggerOnStatusChange(Target, BattleVoice.BattleMoment.Used, BattleStatusId.GradualPetrify);
                    else
                        btl2d.Btl2dReqInstant(Target, Param.FIG_INFO_MISS, 0, 0);
                }
                return true;
            }
            return false;
        }

        private void UpdateLabel()
        {
            Byte intensity = (Byte)Mathf.Lerp(16f, 176f, (Single)Counter / InitialCounter);
            Int32 color = intensity << 16 | intensity << 8 | intensity;
            Message.Label = $"[{color:X6}]{Counter}";
            Message.gameObject.SetActive(true);
        }

        private Boolean UpdateMessageShow(BattleUnit unit)
        {
            if (!unit.IsUnderAnyStatus(BattleStatusId.GradualPetrify))
                return false;

            if (unit.Data.bi.disappear != 0 || GeoID != unit.Data.dms_geo_id)
            {
                GeoID = unit.Data.dms_geo_id;
                if (Message != null)
                {
                    btl2d.StatusMessages.Remove(Message);
                    Singleton<HUDMessage>.Instance.ReleaseObject(Message);
                    Message = null;
                }
                return true;
            }
            if (Message == null)
            {
                btl2d.GetIconPosition(unit, btl2d.ICON_POS_NUMBER, out Transform attachTransf, out Vector3 iconOff);
                Message = Singleton<HUDMessage>.Instance.Show(attachTransf, $"{Counter}", HUDMessage.MessageStyle.DEATH_SENTENCE, new Vector3(0f, iconOff.y), 0);
                btl2d.StatusMessages.Add(Message);
            }
            return true;
        }
    }
}
