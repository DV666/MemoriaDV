using System;
using UnityEngine;
using Memoria.Data;
using System.Collections.Generic;
using Object = System.Object;
using Memoria.Scripts.Battle;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.Doom)]
    public class DoomStatusScript : StatusScriptBase, IOprStatusScript
    {
        public HUDMessageChild Message = null;
        public BattleUnit DoomInflicter = null;
        public Int32 GeoID;
        public Int32 Counter;
        public Int32 InitialCounter;

        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            btl2d.GetIconPosition(target, btl2d.ICON_POS_NUMBER, out Transform attachTransf, out Vector3 iconOff);
            DoomInflicter = inflicter;
            InitialCounter = parameters.Length > 0 ? Convert.ToInt32(parameters[0]) : 10;
            InitialCounter *= (Target.HasSupportAbility(SupportAbility1.AutoRegen) ? 2 : 1);
            InitialCounter *= (TranceSeekAPI.EliteMonster(target.Data) ? 3 : 1);
            Counter = InitialCounter;
            Message = Singleton<HUDMessage>.Instance.Show(attachTransf, $"[FF0000]{Counter}", HUDMessage.MessageStyle.DEATH_SENTENCE, new Vector3(0f, iconOff.y), 0);
            btl2d.StatusMessages.Add(Message);
            target.AddDelayedModifier(UpdateMessageShow, null);
            GeoID = target.Data.dms_geo_id;
            TranceSeekAPI.SA_StatusApply(inflicter, false);
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            btl2d.StatusMessages.Remove(Message);
            Singleton<HUDMessage>.Instance.ReleaseObject(Message);
            if (Target.IsUnderAnyStatus(BattleStatus.EasyKill) && !TranceSeekAPI.EliteMonster(Target.Data))
            {
                List<BattleStatus> statuschoosen = new List<BattleStatus>{ BattleStatus.Poison, BattleStatus.Venom, BattleStatus.Blind, BattleStatus.Silence, BattleStatus.Trouble,
                BattleStatus.Sleep, BattleStatus.Freeze, BattleStatus.Heat, BattleStatus.Mini, BattleStatus.Petrify, BattleStatus.GradualPetrify,
                BattleStatus.Berserk, BattleStatus.Confuse, BattleStatus.Stop, BattleStatus.Zombie, BattleStatus.Slow };

                for (Int32 i = 0; i < (statuschoosen.Count - 1); i++)
                {
                    if ((statuschoosen[i] & Target.Data.stat.invalid) != 0)
                    {
                        statuschoosen.Remove(statuschoosen[i]);
                    }
                }

                for (Int32 i = 0; i < 2; i++)
                {
                    Target.AlterStatus(statuschoosen[GameRandom.Next16() % statuschoosen.Count], DoomInflicter);
                }
            }
            return true;
        }

        public IOprStatusScript.SetupOprMethod SetupOpr => SetupDoomOpr;
        public Int32 SetupDoomOpr()
        {
            // Use the duration "ContiCnt" of Doom even if it is not registered as BattleStatusConst.ContiCount
            return (Int32)(Target.StatusDurationFactor[BattleStatusId.Doom] * BattleStatusId.Doom.GetStatData().ContiCnt * (60 + Target.Will << 2) / 10);
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
                if ((Target.Data.stat.permanent & BattleStatus.Doom) != 0 && (Int32)Target.GetPropertyByName("StatusProperty CustomStatus21 LifeorDeath") == 0)
                {
                    Remove();
                    Target.AddDelayedModifier(
                    target => (target.IsUnderAnyStatus(BattleStatus.Death)),
                    target =>
                    {
                        btl2d.GetIconPosition(target, btl2d.ICON_POS_NUMBER, out Transform attachTransf, out Vector3 iconOff);
                        Counter = (target.HasSupportAbility(SupportAbility1.AutoRegen) ? 20 : 10);
                        Message = Singleton<HUDMessage>.Instance.Show(attachTransf, $"[FF0000]{Counter}", HUDMessage.MessageStyle.DEATH_SENTENCE, new Vector3(0f, iconOff.y), 0);
                        btl2d.StatusMessages.Add(Message);
                    }
                    );
                }
                if (btl_stat.AlterStatus(Target, BattleStatusId.Death, DoomInflicter) == btl_stat.ALTER_SUCCESS)
                    BattleVoice.TriggerOnStatusChange(Target, BattleVoice.BattleMoment.Used, BattleStatusId.Doom);
                btl2d.Btl2dReq(Target);
                return true;
            }
            return false;
        }

        private void UpdateLabel()
        {
            Int32 intensity = 200 / (InitialCounter);
            string color = (255 - intensity * (InitialCounter - Counter)).ToString("X");
            Message.Label = $"[{color}0000]{Counter}";
            Message.gameObject.SetActive(true);
        }

        private Boolean UpdateMessageShow(BattleUnit unit)
        {
            if (!unit.IsUnderAnyStatus(BattleStatusId.Doom))
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
                Message = Singleton<HUDMessage>.Instance.Show(attachTransf, $"[FF0000]{Counter}", HUDMessage.MessageStyle.DEATH_SENTENCE, new Vector3(0f, iconOff.y), 0);
                btl2d.StatusMessages.Add(Message);
            }
            return true;
        }
    }
}
