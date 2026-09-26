using System;
using UnityEngine;
using Memoria.Data;
using System.Collections.Generic;
using Object = System.Object;
using Memoria.Scripts.TranceSeek;

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
                if ((Target.Data.stat.permanent & BattleStatus.Doom) != 0 && Target.State().Marcus.LifeOrDeath == false)
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
                if (Target.IsUnderAnyStatus(BattleStatus.EasyKill) && !TranceSeekAPI.EliteMonster(Target.Data))
                {
                    List<BattleStatus> allPossible = new List<BattleStatus>
                    {
                        BattleStatus.Poison, BattleStatus.Venom, BattleStatus.Blind, BattleStatus.Silence, BattleStatus.Trouble,
                        BattleStatus.Sleep, BattleStatus.Freeze, BattleStatus.Heat, BattleStatus.Mini, BattleStatus.Petrify, BattleStatus.GradualPetrify,
                        BattleStatus.Berserk, BattleStatus.Confuse, BattleStatus.Stop, BattleStatus.Zombie, BattleStatus.Slow, BattleStatus.Virus
                    };

                    List<BattleStatus> validStatuses = new List<BattleStatus>();
                    List<BattleStatus> preferredStatuses = new List<BattleStatus>();

                    foreach (BattleStatus status in allPossible)
                    {
                        if ((status & Target.Data.stat.invalid) != 0)
                            continue;

                        validStatuses.Add(status);

                        if ((status & Target.Data.stat.cur) == 0)
                            preferredStatuses.Add(status);
                    }

                    for (Int32 i = 0; i < 2; i++)
                    {
                        if (validStatuses.Count == 0)
                            break;

                        BattleStatus status_selected;

                        if (preferredStatuses.Count > 0)
                        {
                            status_selected = preferredStatuses[GameRandom.Next16() % preferredStatuses.Count];
                            preferredStatuses.Remove(status_selected);
                        }
                        else
                        {
                            status_selected = validStatuses[GameRandom.Next16() % validStatuses.Count];
                        }

                        validStatuses.Remove(status_selected);
                        Target.AlterStatus(status_selected, DoomInflicter);
                    }
                }
                else if (btl_stat.AlterStatus(Target, BattleStatusId.Death, DoomInflicter) == btl_stat.ALTER_SUCCESS)
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
