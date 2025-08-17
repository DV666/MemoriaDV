using System;
using System.Collections;
using System.Collections.Generic;
using FF9;
using Memoria.Data;
using UnityEngine;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Scan
    /// </summary>
    [BattleScript(Id)]
    public sealed class ScanScript : IBattleScript
    {
        public const Int32 Id = 0059;

        private readonly BattleCalculator _v;

        public static Dictionary<BTL_DATA, HUDMessageChild> HPGreenBarHUD = new Dictionary<BTL_DATA, HUDMessageChild>();
        public static Dictionary<BTL_DATA, HUDMessageChild> HPRedBarHUD = new Dictionary<BTL_DATA, HUDMessageChild>();
        public static Dictionary<BTL_DATA, HUDMessageChild> ATBGreenBarHUD = new Dictionary<BTL_DATA, HUDMessageChild>();
        public static Dictionary<BTL_DATA, HUDMessageChild> ATBRedBarHUD = new Dictionary<BTL_DATA, HUDMessageChild>();
        public static Dictionary<BTL_DATA, UInt32> HPBarValue = new Dictionary<BTL_DATA, UInt32>();
        public static Dictionary<BTL_DATA, Boolean> HPBarHidden = new Dictionary<BTL_DATA, Boolean>();
        public static Dictionary<BTL_DATA, Boolean> TriggerOneTime = new Dictionary<BTL_DATA, Boolean>();

        public ScanScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Caster.IsPlayer)
            {
                if (_v.Command.AbilityId == (BattleAbilityId)1075 || (_v.Command.Power == 33 && _v.Command.HitRate == 33)) // Lani - Predator's Eye
                {
                    _v.Target.Libra(BattleHUD.LibraInformation.NameLevel | BattleHUD.LibraInformation.Category | BattleHUD.LibraInformation.ElementalAffinities | BattleHUD.LibraInformation.StatusAffinities);
                }
                else if (_v.Command.AbilityId == (BattleAbilityId)1585) // Scan X
                {
                    if (_v.Target.IsUnderStatus(BattleStatus.EasyKill) && !TranceSeekAPI.EliteMonster(_v.Target.Data))
                        _v.Target.Libra(BattleHUD.LibraInformation.Name | BattleHUD.LibraInformation.Level | BattleHUD.LibraInformation.Category | BattleHUD.LibraInformation.ElementalAffinities | BattleHUD.LibraInformation.ItemSteal);
                    else
                        _v.Target.Libra(BattleHUD.LibraInformation.All);

                    if (btl_para.IsNonDyingVanillaBoss(_v.Target))
                        HPBarValue[_v.Target.Data] = (_v.Target.CurrentHp - 10000);
                    else
                        HPBarValue[_v.Target.Data] = _v.Target.CurrentHp;

                    //HPBarHidden[_v.Target.Data] = false;
                    //TriggerOneTime[_v.Target.Data] = false;
                    //HPGreenBarHUD[_v.Target.Data] = null;
                    //HPRedBarHUD[_v.Target.Data] = null;
                    //ATBGreenBarHUD[_v.Target.Data] = null;
                    //ATBRedBarHUD[_v.Target.Data] = null;
                    _v.Target.AddDelayedModifier(ShowScan, null);
                }
                else if (_v.Target.IsUnderStatus(BattleStatus.EasyKill) && !TranceSeekAPI.EliteMonster(_v.Target.Data)) // Boss
                {
                    _v.Target.Libra(BattleHUD.LibraInformation.Name | BattleHUD.LibraInformation.Level | BattleHUD.LibraInformation.Category | BattleHUD.LibraInformation.ItemSteal);
                }
                else if (_v.Command.AbilityId == (BattleAbilityId)1584) // Scan +
                {
                    _v.Target.Libra(BattleHUD.LibraInformation.All);
                }
                else
                {
                    _v.Target.Libra(BattleHUD.LibraInformation.Default | BattleHUD.LibraInformation.ItemSteal);
                }
            }
            else
            {
                if (_v.Command.Power == 1)
                {
                    _v.Target.Libra(BattleHUD.LibraInformation.Default);
                }
            }
        }

        private Boolean ShowScan(BattleUnit mob)
        {
            if (mob.IsUnderAnyStatus(BattleStatusConst.BattleEndFull))
                return false;


            if (((Input.GetKey(KeyCode.Alpha2) || (UIManager.Input.GetKey(Control.LeftBumper) && UIManager.Input.GetKey(Control.Special))) && mob.Id == 16)
                || ((Input.GetKey(KeyCode.Alpha3) || (UIManager.Input.GetKey(Control.LeftTrigger) && UIManager.Input.GetKey(Control.Special))) && mob.Id == 32)
                || ((Input.GetKey(KeyCode.Alpha4) || (UIManager.Input.GetKey(Control.RightBumper) && UIManager.Input.GetKey(Control.Special))) && mob.Id == 64)
                || ((Input.GetKey(KeyCode.Alpha5) || (UIManager.Input.GetKey(Control.RightTrigger) && UIManager.Input.GetKey(Control.Special))) && mob.Id == 128))
            {
                SoundLib.PlaySoundEffect(1362); // Mog effect sound
                if (mob.IsUnderStatus(BattleStatus.EasyKill) && !TranceSeekAPI.EliteMonster(mob.Data)) // Boss
                    mob.Libra(BattleHUD.LibraInformation.Name | BattleHUD.LibraInformation.Level | BattleHUD.LibraInformation.Category | BattleHUD.LibraInformation.ElementalAffinities | BattleHUD.LibraInformation.ItemSteal);
                else
                    mob.Libra(BattleHUD.LibraInformation.All);
            }
            return true;
        }

        private Boolean ShowHPBar(BattleUnit mob)
        {
            if (mob.IsUnderAnyStatus(BattleStatusConst.BattleEndFull) || btl_para.IsNonDyingVanillaBoss(mob) && mob.CurrentHp <= 10000)
            {
                btl2d.StatusMessages.Remove(HPRedBarHUD[mob.Data]);
                btl2d.StatusMessages.Remove(HPGreenBarHUD[mob.Data]);
                Singleton<HUDMessage>.Instance.ReleaseObject(HPRedBarHUD[mob.Data]);
                Singleton<HUDMessage>.Instance.ReleaseObject(HPGreenBarHUD[mob.Data]);
                HPRedBarHUD[mob.Data] = null;
                HPGreenBarHUD[mob.Data] = null;
                return false;
            }

            if (FF9StateSystem.Battle.FF9Battle.btl_phase < FF9StateBattleSystem.PHASE_MENU_ON) // Don't show HP Bar in intro
                return true;

            if (HPGreenBarHUD[mob.Data] == null && HPRedBarHUD[mob.Data] == null)
            {
                BattleStatusDataEntry statusData = FF9StateSystem.Battle.FF9Battle.status_data[BattleStatusId.Poison];
                btl2d.GetIconPosition(mob.Data, btl2d.ICON_POS_HEAD, out Transform attachTransf, out Vector3 iconOff);
                Vector3 HPBarHUD_Offset = statusData.SHPExtraPos + iconOff + new Vector3(200, 200, 0);

                uint HPValue = btl_para.IsNonDyingVanillaBoss(mob) ? ((mob.CurrentHp - 10000) * 150) / (mob.MaximumHp - 10000) : ((mob.CurrentHp * 150) / mob.MaximumHp);
                // Red HP Bar (background)
                HPRedBarHUD[mob.Data] = Singleton<HUDMessage>.Instance.Show(attachTransf, "[SPRT=GeneralAtlas,ap_bar_complete,150,15]", HUDMessage.MessageStyle.DEATH_SENTENCE, HPBarHUD_Offset, 0);
                HPRedBarHUD[mob.Data].Follower.clampToScreen = false;
                btl2d.StatusMessages.Add(HPRedBarHUD[mob.Data]);

                // Green HP Bar (actual)               
                HPGreenBarHUD[mob.Data] = Singleton<HUDMessage>.Instance.Show(attachTransf, $"[SPRT=GeneralAtlas,ap_bar_progress,{HPValue},15]", HUDMessage.MessageStyle.DEATH_SENTENCE, HPBarHUD_Offset, 0);
                UILabel UILabelHPGreenBarHUD = HPGreenBarHUD[mob.Data].GetComponent<UILabel>();
                HPGreenBarHUD[mob.Data].Follower.clampToScreen = false;
                UILabelHPGreenBarHUD.spacingY = -10;
                btl2d.StatusMessages.Add(HPGreenBarHUD[mob.Data]);
            }

            if (HPGreenBarHUD[mob.Data] != null && HPRedBarHUD[mob.Data] != null && (Input.GetKey(KeyCode.Alpha2) || UIManager.Input.GetKey(Control.Special)) && !TriggerOneTime[mob.Data])
            {
                HPBarHidden[mob.Data] = !HPBarHidden[mob.Data];
                TriggerOneTime[mob.Data] = true;

                if (HPBarHidden[mob.Data])
                {
                    HPRedBarHUD[mob.Data].gameObject.SetActive(false);
                    HPGreenBarHUD[mob.Data].gameObject.SetActive(false);
                }
                else
                {      
                    HPRedBarHUD[mob.Data].gameObject.SetActive(true);
                    HPGreenBarHUD[mob.Data].gameObject.SetActive(true);
                    uint ShowHPValue = btl_para.IsNonDyingVanillaBoss(mob) ? ((mob.CurrentHp - 10000) * 150) / (mob.MaximumHp - 10000) : ((mob.CurrentHp * 150) / mob.MaximumHp);
                    HPGreenBarHUD[mob.Data].Label = $"[SPRT=GeneralAtlas,ap_bar_progress,{ShowHPValue},15]";
                }
            }
            else if (!Input.GetKey(KeyCode.Alpha2) && !UIManager.Input.GetKey(Control.Special))
            {
                TriggerOneTime[mob.Data] = false;
            }

            if (HPBarValue[mob.Data] != mob.CurrentHp)
            {
                uint newHPValue = btl_para.IsNonDyingVanillaBoss(mob) ? ((mob.CurrentHp - 10000) * 150) / (mob.MaximumHp - 10000) : ((mob.CurrentHp * 150) / mob.MaximumHp);
                HPGreenBarHUD[mob.Data].Label = $"[SPRT=GeneralAtlas,ap_bar_progress,{newHPValue},15]";               
            }
            return true;
        }

        public static Boolean ShowATBBar(BattleUnit mob)
        {
            if (mob.IsUnderAnyStatus(BattleStatusConst.BattleEndFull) || btl_para.IsNonDyingVanillaBoss(mob) && mob.CurrentHp <= 10000)
            {
                btl2d.StatusMessages.Remove(ATBRedBarHUD[mob.Data]);
                btl2d.StatusMessages.Remove(ATBGreenBarHUD[mob.Data]);
                Singleton<HUDMessage>.Instance.ReleaseObject(ATBRedBarHUD[mob.Data]);
                Singleton<HUDMessage>.Instance.ReleaseObject(ATBGreenBarHUD[mob.Data]);
                ATBRedBarHUD[mob.Data] = null;
                ATBGreenBarHUD[mob.Data] = null;
                return false;
            }
            if (FF9StateSystem.Battle.FF9Battle.btl_phase < FF9StateBattleSystem.PHASE_MENU_ON) // Don't show ATB Bar in intro
                return true;

            short ATBValue = (short)((mob.CurrentAtb * 150) / mob.MaximumAtb);
            if (ATBGreenBarHUD[mob.Data] == null && ATBRedBarHUD[mob.Data] == null)
            {
                BattleStatusDataEntry statusData = FF9StateSystem.Battle.FF9Battle.status_data[BattleStatusId.Poison];
                btl2d.GetIconPosition(mob.Data, btl2d.ICON_POS_HEAD, out Transform attachTransf, out Vector3 iconOff);
                Vector3 HPBarHUD_Offset = statusData.SHPExtraPos + iconOff + new Vector3(200, 150, 0);

                // Red ATB Bar (background)
                ATBRedBarHUD[mob.Data] = Singleton<HUDMessage>.Instance.Show(attachTransf, "[SPRT=GeneralAtlas,ap_bar_complete,150,15]", HUDMessage.MessageStyle.DEATH_SENTENCE, HPBarHUD_Offset, 0);
                ATBRedBarHUD[mob.Data].Follower.clampToScreen = false;
                btl2d.StatusMessages.Add(ATBRedBarHUD[mob.Data]);

                // Blue ATB Bar (actual)               
                ATBGreenBarHUD[mob.Data] = Singleton<HUDMessage>.Instance.Show(attachTransf, $"[SPRT=GeneralAtlas,slider_bar,{ATBValue},15]", HUDMessage.MessageStyle.DEATH_SENTENCE, HPBarHUD_Offset, 0);
                UILabel UILabelHPGreenBarHUD = ATBGreenBarHUD[mob.Data].GetComponent<UILabel>();
                ATBGreenBarHUD[mob.Data].Follower.clampToScreen = false;
                UILabelHPGreenBarHUD.spacingY = -10;
                btl2d.StatusMessages.Add(ATBGreenBarHUD[mob.Data]);
            }
            ATBGreenBarHUD[mob.Data].Label = $"[SPRT=GeneralAtlas,slider_bar,{ATBValue},15]";

            if (ATBGreenBarHUD[mob.Data] != null && ATBRedBarHUD[mob.Data] != null && (Input.GetKey(KeyCode.Alpha2) || UIManager.Input.GetKey(Control.Special)) && !TriggerOneTime[mob.Data])
            {
                HPBarHidden[mob.Data] = !HPBarHidden[mob.Data];
                TriggerOneTime[mob.Data] = true;

                if (HPBarHidden[mob.Data])
                {
                    ATBRedBarHUD[mob.Data].gameObject.SetActive(false);
                    ATBGreenBarHUD[mob.Data].gameObject.SetActive(false);
                }
                else
                {
                    ATBRedBarHUD[mob.Data].gameObject.SetActive(true);
                    ATBGreenBarHUD[mob.Data].gameObject.SetActive(true);
                }
            }
            else if (!Input.GetKey(KeyCode.Alpha2) && !UIManager.Input.GetKey(Control.Special))
            {
                TriggerOneTime[mob.Data] = false;
            }
            return true;
        }
    }
}
