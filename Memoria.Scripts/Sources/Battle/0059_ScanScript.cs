using System;
using System.Collections.Generic;
using Memoria.Data;
using UnityEngine;

namespace Memoria.Scripts.TranceSeek
{
    /// <summary>
    /// Scan
    /// </summary>
    [BattleScript(Id)]
    public sealed class ScanScript : IBattleScript
    {
        public const Int32 Id = 0059;

        private readonly BattleCalculator _v;

        public static Dictionary<KeyValuePair<Int32, Int32>, Vector3> AdjustOffsetForScan = new Dictionary<KeyValuePair<Int32, Int32>, Vector3>
        {
            { new KeyValuePair<Int32, Int32>(4, 2), new Vector3(200, 0, 0) }, // Nightmare
            { new KeyValuePair<Int32, Int32>(4, 3), new Vector3(200, -400, 0) }, // Thousand Fears
        };

        public ScanScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            var TargetMonster_TSVAR = _v.Target.State().Monster;
            if (_v.Caster.IsPlayer)
            {
                if (_v.Command.AbilityId == TranceSeekBattleAbility.PredatorsEye) // Lani - Predator's Eye
                {
                    _v.Target.Libra(BattleHUD.LibraInformation.Name | BattleHUD.LibraInformation.Category | BattleHUD.LibraInformation.ElementalAffinities | BattleHUD.LibraInformation.StatusAffinities);
                }
                else if (_v.Command.AbilityId == TranceSeekBattleAbility.Scanga) // Scan X
                {
                    if (_v.Target.IsUnderStatus(BattleStatus.EasyKill) && !TranceSeekAPI.EliteMonster(_v.Target.Data))
                        _v.Target.Libra(BattleHUD.LibraInformation.Name | BattleHUD.LibraInformation.Level | BattleHUD.LibraInformation.Category | BattleHUD.LibraInformation.ElementalAffinities | BattleHUD.LibraInformation.ItemSteal);
                    else
                        _v.Target.Libra(BattleHUD.LibraInformation.All);

                    if (btl_para.IsNonDyingVanillaBoss(_v.Target))
                        TargetMonster_TSVAR.HPBarValue = (_v.Target.CurrentHp - 10000);
                    else
                        TargetMonster_TSVAR.HPBarValue = _v.Target.CurrentHp;

                    _v.Target.State().CasterScanga = _v.Caster.Data;
                    _v.Target.AddDelayedModifier(ShowScan, null);
                }
                else if (_v.Target.IsUnderStatus(BattleStatus.EasyKill) && !TranceSeekAPI.EliteMonster(_v.Target.Data)) // Boss
                {
                    _v.Target.Libra(BattleHUD.LibraInformation.Name | BattleHUD.LibraInformation.Level | BattleHUD.LibraInformation.Category | BattleHUD.LibraInformation.ItemSteal);
                }
                else if (_v.Command.AbilityId == TranceSeekBattleAbility.Scanra) // Scan +
                {
                    _v.Target.Libra(BattleHUD.LibraInformation.All);
                    TranceSeekBestiaryDB.UpdateMonsterStatus(FF9StateSystem.Battle.battleMapIndex, _v.Target.Data.typeNo, TranceSeekBestiaryDB.StatusScannedPlus);
                }
                else
                {
                    _v.Target.Libra(BattleHUD.LibraInformation.Default | BattleHUD.LibraInformation.ItemSteal);
                    TranceSeekBestiaryDB.UpdateMonsterStatus(FF9StateSystem.Battle.battleMapIndex, _v.Target.Data.typeNo, TranceSeekBestiaryDB.StatusScanned);
                }
            }
            else
            {
                if (_v.Command.Power == 33 && _v.Command.HitRate == 33) // Lani - Predator's Eye
                {
                    _v.Target.Libra(BattleHUD.LibraInformation.Name | BattleHUD.LibraInformation.Category | BattleHUD.LibraInformation.ElementalAffinities | BattleHUD.LibraInformation.StatusAffinities);
                }
                else if (_v.Command.Power == 1)
                {
                    _v.Target.Libra(BattleHUD.LibraInformation.Default);
                }
            }
        }

        private Boolean ShowScan(BattleUnit mob)
        {
            if (mob.IsUnderAnyStatus(BattleStatusConst.BattleEndFull))
                return false;

            var Unit_TSVAR = mob.State();
            BattleUnit caster = new BattleUnit (Unit_TSVAR.CasterScanga);
            if (caster.CurrentAtb >= caster.MaximumAtb && Unit_TSVAR.CasterScanga.currentAnimationName.Contains("_000"))
            {
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
            }
            return true;
        }

        private Boolean ShowHPBar(BattleUnit mob)
        {
            var TargetMonster_TSVAR = mob.State().Monster;

            if (mob.IsUnderAnyStatus(BattleStatusConst.BattleEndFull) || btl_para.IsNonDyingVanillaBoss(mob) && mob.CurrentHp <= 10000)
            {
                btl2d.StatusMessages.Remove(TargetMonster_TSVAR.HPRedBarHUD);
                btl2d.StatusMessages.Remove(TargetMonster_TSVAR.HPGreenBarHUD);
                Singleton<HUDMessage>.Instance.ReleaseObject(TargetMonster_TSVAR.HPRedBarHUD);
                Singleton<HUDMessage>.Instance.ReleaseObject(TargetMonster_TSVAR.HPGreenBarHUD);
                TargetMonster_TSVAR.HPRedBarHUD = null;
                TargetMonster_TSVAR.HPGreenBarHUD = null;
                return false;
            }

            if (FF9StateSystem.Battle.FF9Battle.btl_phase < FF9StateBattleSystem.PHASE_MENU_ON) // Don't show HP Bar in intro
                return true;

            if (TargetMonster_TSVAR.HPGreenBarHUD == null && TargetMonster_TSVAR.HPRedBarHUD == null)
            {
                BattleStatusDataEntry statusData = FF9StateSystem.Battle.FF9Battle.status_data[BattleStatusId.Poison];
                btl2d.GetIconPosition(mob.Data, btl2d.ICON_POS_HEAD, out Transform attachTransf, out Vector3 iconOff);
                Vector3 HPBarHUD_Offset = statusData.SHPExtraPos + iconOff + new Vector3(200, 200, 0);

                uint HPValue = btl_para.IsNonDyingVanillaBoss(mob) ? ((mob.CurrentHp - 10000) * 150) / (mob.MaximumHp - 10000) : ((mob.CurrentHp * 150) / mob.MaximumHp);
                // Red HP Bar (background)
                TargetMonster_TSVAR.HPRedBarHUD = Singleton<HUDMessage>.Instance.Show(attachTransf, "[SPRT=GeneralAtlas,ap_bar_complete,150,15]", HUDMessage.MessageStyle.DEATH_SENTENCE, HPBarHUD_Offset, 0);
                TargetMonster_TSVAR.HPRedBarHUD.Follower.clampToScreen = false;
                btl2d.StatusMessages.Add(TargetMonster_TSVAR.HPRedBarHUD);

                // Green HP Bar (actual)               
                TargetMonster_TSVAR.HPGreenBarHUD = Singleton<HUDMessage>.Instance.Show(attachTransf, $"[SPRT=GeneralAtlas,ap_bar_progress,{HPValue},15]", HUDMessage.MessageStyle.DEATH_SENTENCE, HPBarHUD_Offset, 0);
                UILabel UILabelHPGreenBarHUD = TargetMonster_TSVAR.HPGreenBarHUD.GetComponent<UILabel>();
                TargetMonster_TSVAR.HPGreenBarHUD.Follower.clampToScreen = false;
                UILabelHPGreenBarHUD.spacingY = -10;
                btl2d.StatusMessages.Add(TargetMonster_TSVAR.HPGreenBarHUD);
            }

            if (TargetMonster_TSVAR.HPGreenBarHUD != null && TargetMonster_TSVAR.HPRedBarHUD != null && (Input.GetKey(KeyCode.Alpha2) || UIManager.Input.GetKey(Control.Special)) && !TargetMonster_TSVAR.TriggerHPHUDOneTime)
            {
                TargetMonster_TSVAR.HPBarHidden = !TargetMonster_TSVAR.HPBarHidden;
                TargetMonster_TSVAR.TriggerHPHUDOneTime = true;

                if (TargetMonster_TSVAR.HPBarHidden)
                {
                    TargetMonster_TSVAR.HPRedBarHUD.gameObject.SetActive(false);
                    TargetMonster_TSVAR.HPGreenBarHUD.gameObject.SetActive(false);
                }
                else
                {
                    TargetMonster_TSVAR.HPRedBarHUD.gameObject.SetActive(true);
                    TargetMonster_TSVAR.HPGreenBarHUD.gameObject.SetActive(true);
                    uint ShowHPValue = btl_para.IsNonDyingVanillaBoss(mob) ? ((mob.CurrentHp - 10000) * 150) / (mob.MaximumHp - 10000) : ((mob.CurrentHp * 150) / mob.MaximumHp);
                    TargetMonster_TSVAR.HPGreenBarHUD.Label = $"[SPRT=GeneralAtlas,ap_bar_progress,{ShowHPValue},15]";
                }
            }
            else if (!Input.GetKey(KeyCode.Alpha2) && !UIManager.Input.GetKey(Control.Special))
            {
                TargetMonster_TSVAR.TriggerHPHUDOneTime = false;
            }

            if (TargetMonster_TSVAR.HPBarValue != mob.CurrentHp)
            {
                uint newHPValue = btl_para.IsNonDyingVanillaBoss(mob) ? ((mob.CurrentHp - 10000) * 150) / (mob.MaximumHp - 10000) : ((mob.CurrentHp * 150) / mob.MaximumHp);
                TargetMonster_TSVAR.HPGreenBarHUD.Label = $"[SPRT=GeneralAtlas,ap_bar_progress,{newHPValue},15]";
            }
            return true;
        }

        public static Boolean ShowATBBar(BattleUnit mob)
        {
            var TargetMonster_TSVAR = mob.State().Monster;
            HUDMessageChild greenHUD = TargetMonster_TSVAR.ATBGreenBarHUD;
            HUDMessageChild frameHUD = TargetMonster_TSVAR.ATBFrameHUD;

            if (mob.IsUnderAnyStatus(BattleStatusConst.BattleEndFull) || mob.CurrentHp == 0)
            {
                if (greenHUD != null)
                {
                    btl2d.StatusMessages.Remove(greenHUD);
                    Singleton<HUDMessage>.Instance.ReleaseObject(greenHUD);
                    TargetMonster_TSVAR.ATBGreenBarHUD = null;
                }

                if (frameHUD != null)
                {
                    btl2d.StatusMessages.Remove(frameHUD);
                    Singleton<HUDMessage>.Instance.ReleaseObject(frameHUD);
                    TargetMonster_TSVAR.ATBFrameHUD = null;
                }
                return false;
            }

            BattleStateSystem FF9BattleState = FF9StateSystem.Battle;
            if (FF9BattleState.FF9Battle.btl_phase < FF9StateBattleSystem.PHASE_MENU_ON || FF9BattleState.FF9Battle.btl_phase == FF9StateBattleSystem.PHASE_MENU_OFF || mob.Data.bi.disappear == 1 || !mob.Data.gameObject.activeSelf || !mob.IsTargetable)
            {
                if (frameHUD != null)
                    frameHUD.Label = string.Empty;

                if (greenHUD != null)
                    greenHUD.Label = string.Empty;

                return true;
            }

            float atbPercent = (float)mob.CurrentAtb / (float)mob.MaximumAtb;
            atbPercent = Mathf.Clamp01(atbPercent);

            string ATBSprite = "battle_bar_atb";
            if (mob.IsUnderAnyStatus(BattleStatus.Slow))
                ATBSprite = "battle_bar_slow";
            else if (mob.IsUnderAnyStatus(BattleStatus.Haste))
                ATBSprite = "battle_bar_haste";

            if (greenHUD == null && frameHUD == null)
            {
                BattleStatusDataEntry statusData = FF9StateSystem.Battle.FF9Battle.status_data[BattleStatusId.Poison];
                btl2d.GetIconPosition(mob.Data, btl2d.ICON_POS_HEAD, out Transform attachTransf, out Vector3 iconOff);

                Vector3 offset = Vector3.zero;
                KeyValuePair<Int32, Int32> MobBattleId = new KeyValuePair<Int32, Int32>(FF9StateSystem.Battle.battleMapIndex, mob.Data.typeNo);
                if (!AdjustOffsetForScan.TryGetValue(MobBattleId, out offset))
                    offset = new Vector3(200, 150, 0);

                Vector3 ATB_HUD_Offset = statusData.SHPExtraPos + iconOff + offset;

                frameHUD = Singleton<HUDMessage>.Instance.Show(attachTransf, "[SPRT=GeneralAtlas,battle_bar_bg_monster,150,18]", HUDMessage.MessageStyle.DEATH_SENTENCE, ATB_HUD_Offset, 0);
                frameHUD.Follower.clampToScreen = false;

                UILabel bgLabel = frameHUD.GetComponent<UILabel>();
                bgLabel.pivot = UIWidget.Pivot.Center;
                bgLabel.spacingY = 0;
                bgLabel.depth = 20;
                frameHUD.transform.localScale = Vector3.one;

                btl2d.StatusMessages.Add(frameHUD);

                greenHUD = Singleton<HUDMessage>.Instance.Show(attachTransf, $"[SPRT=GeneralAtlas,{ATBSprite},148,16]", HUDMessage.MessageStyle.DEATH_SENTENCE, ATB_HUD_Offset, 0);
                greenHUD.Follower.clampToScreen = false;

                UILabel atbLabel = greenHUD.GetComponent<UILabel>();
                atbLabel.pivot = UIWidget.Pivot.Left;
                atbLabel.alignment = NGUIText.Alignment.Left;
                atbLabel.spacingY = -10;
                atbLabel.depth = 10;

                greenHUD.transform.localPosition = new Vector3(-165f, 0f, 0f);

                btl2d.StatusMessages.Add(greenHUD);

                TargetMonster_TSVAR.ATBFrameHUD = frameHUD;
                TargetMonster_TSVAR.ATBGreenBarHUD = greenHUD;
            }

            if (greenHUD != null && frameHUD != null)
            {
                if (PersistenSingleton<BattleHUD>.Instance.AllMenuPanel.gameObject.activeSelf && btl2d.ShouldShowSPS && mob.Data.bi.disappear == 0)
                {
                    string bgText = "[SPRT=GeneralAtlas,battle_bar_bg_monster,150,18]";
                    if (frameHUD.Label != bgText)
                        frameHUD.Label = bgText;

                    string currentText = $"[SPRT=GeneralAtlas,{ATBSprite},148,16]";
                    if (greenHUD.Label != currentText)
                        greenHUD.Label = currentText;

                    greenHUD.transform.localScale = new Vector3(atbPercent, 1f, 1f);
                }
                else
                {
                    if (frameHUD.Label != string.Empty)
                        frameHUD.Label = string.Empty;
                    if (greenHUD.Label != string.Empty)
                        greenHUD.Label = string.Empty;
                }
            }

            return true;
        }
    }
}

