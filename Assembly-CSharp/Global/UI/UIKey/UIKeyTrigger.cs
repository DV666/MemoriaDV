﻿using Assets.Scripts.Common;
using Memoria;
using Memoria.Assets;
using Memoria.Data;
using Memoria.Prime;
using Memoria.Prime.Text;
using Memoria.Scenes;
using Memoria.Speedrun;
using Memoria.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

#pragma warning disable 169
#pragma warning disable 414
#pragma warning disable 649

public class UIKeyTrigger : MonoBehaviour
{
    private Control keyCommand;
    private Control lazyKeyCommand;
    private Boolean isLockLazyInput;
    private Single disableMouseCounter;
    private Boolean firstTimeInput;
    private Single fastEventCounter;
    private Boolean triggleEventDialog;
    private Boolean quitConfirm;
    private Single autoConfirmDownTime = 0;
    private Boolean TurboKey;
    public static Boolean preventTurboKey;

    public static Boolean IsShiftKeyPressed { get; private set; }

    private Boolean AltKey => UnityXInput.Input.GetKey(KeyCode.LeftAlt) || UnityXInput.Input.GetKey(KeyCode.RightAlt);
    private Boolean ShiftKey => UnityXInput.Input.GetKey(KeyCode.LeftShift) || UnityXInput.Input.GetKey(KeyCode.RightShift);
    private Boolean ControlKey => UnityXInput.Input.GetKey(KeyCode.LeftControl) || UnityXInput.Input.GetKey(KeyCode.RightControl);
    private Boolean AltKeyDown => UnityXInput.Input.GetKeyDown(KeyCode.LeftAlt) || UnityXInput.Input.GetKeyDown(KeyCode.RightAlt);

    private Boolean F2Key => UnityXInput.Input.GetKey(KeyCode.F2);
    private Boolean F4Key => UnityXInput.Input.GetKey(KeyCode.F4);
    private Boolean F5Key => UnityXInput.Input.GetKey(KeyCode.F5);
    private Boolean F9Key => UnityXInput.Input.GetKey(KeyCode.F9);
    private Boolean F1KeyDown => UnityXInput.Input.GetKeyDown(KeyCode.F1);
    private Boolean F2KeyDown => UnityXInput.Input.GetKeyDown(KeyCode.F2);
    private Boolean F4KeyDown => UnityXInput.Input.GetKeyDown(KeyCode.F4);
    private Boolean F5KeyDown => UnityXInput.Input.GetKeyDown(KeyCode.F5);
    private Boolean F9KeyDown => UnityXInput.Input.GetKeyDown(KeyCode.F9);
    private Boolean F12KeyDown => UnityXInput.Input.GetKeyDown(KeyCode.F12);
    private Boolean SpaceKeyDown => UnityXInput.Input.GetKeyDown(KeyCode.Space);
    private Boolean MKeyDown => UnityXInput.Input.GetKeyDown(KeyCode.M);
    private Boolean SKeyDown => UnityXInput.Input.GetKeyDown(KeyCode.S);

    private Boolean SoftResetKeyPSXDown => // L1 + R1 + L2 + R2 + start + select
        PersistenSingleton<HonoInputManager>.Instance.IsInputDown(Control.LeftBumper) && PersistenSingleton<HonoInputManager>.Instance.IsInputDown(Control.LeftTrigger)
        && PersistenSingleton<HonoInputManager>.Instance.IsInputDown(Control.RightBumper) && PersistenSingleton<HonoInputManager>.Instance.IsInputDown(Control.RightTrigger)
        && PersistenSingleton<HonoInputManager>.Instance.IsInputDown(Control.Pause) && PersistenSingleton<HonoInputManager>.Instance.IsInputDown(Control.Select)
        || keyCommand == Control.LeftBumper && keyCommand == Control.LeftTrigger && keyCommand == Control.RightBumper && keyCommand == Control.RightTrigger
        && keyCommand == Control.Pause && keyCommand == Control.Select || UIManager.Input.GetKey(Control.LeftBumper) && UIManager.Input.GetKey(Control.LeftTrigger)
        && UIManager.Input.GetKey(Control.RightBumper) && UIManager.Input.GetKey(Control.RightTrigger) && UIManager.Input.GetKey(Control.Pause) && UIManager.Input.GetKey(Control.Select);

    private Boolean SoftResetKeyPSXForPause => // L1 + R1 + L2 + R2 + select
    PersistenSingleton<HonoInputManager>.Instance.IsInputDown(Control.LeftBumper) && PersistenSingleton<HonoInputManager>.Instance.IsInputDown(Control.LeftTrigger)
    && PersistenSingleton<HonoInputManager>.Instance.IsInputDown(Control.RightBumper) && PersistenSingleton<HonoInputManager>.Instance.IsInputDown(Control.RightTrigger)
    && PersistenSingleton<HonoInputManager>.Instance.IsInputDown(Control.Select)
    || keyCommand == Control.LeftBumper && keyCommand == Control.LeftTrigger && keyCommand == Control.RightBumper && keyCommand == Control.RightTrigger
    && keyCommand == Control.Select || UIManager.Input.GetKey(Control.LeftBumper) && UIManager.Input.GetKey(Control.LeftTrigger)
    && UIManager.Input.GetKey(Control.RightBumper) && UIManager.Input.GetKey(Control.RightTrigger) && UIManager.Input.GetKey(Control.Select);

    public UIKeyTrigger()
    {
        keyCommand = Control.None;
        lazyKeyCommand = Control.None;
    }

    public static Boolean IsOnlyTouchAndLeftClick()
    {
        return UICamera.currentTouchID > -2 && UICamera.currentTouchID < 2;
    }

    public static Boolean IsNeedToRemap()
    {
        return Application.platform == RuntimePlatform.Android && PersistenSingleton<UIManager>.Instance.Dialogs.IsDialogNeedControl() && PersistenSingleton<UIManager>.Instance.Dialogs.GetChoiceDialog() == null && EventHUD.CurrentHUD == MinigameHUD.None;
    }

    public void ResetTriggerEvent()
    {
        triggleEventDialog = false;
    }

    public Boolean GetKey(Control key)
    {
        if (PersistenSingleton<UIManager>.Instance.State != UIManager.UIState.FieldHUD && PersistenSingleton<UIManager>.Instance.State != UIManager.UIState.WorldHUD && (PersistenSingleton<UIManager>.Instance.State != UIManager.UIState.BattleHUD && PersistenSingleton<UIManager>.Instance.State != UIManager.UIState.QuadMistBattle) && PersistenSingleton<UIManager>.Instance.UnityScene != UIManager.Scene.EndGame)
            return false;
        if (triggleEventDialog && key == Control.Confirm)
        {
            triggleEventDialog = false;
            return true;
        }
        return (key != Control.Cancel || !PersistenSingleton<UIManager>.Instance.Dialogs.GetChoiceDialog()) &&
               (PersistenSingleton<UIManager>.Instance.State != UIManager.UIState.WorldHUD ||
                 !PersistenSingleton<UIManager>.Instance.Booster.IsSliderActive && (key != Control.Confirm || !UnityXInput.Input.GetMouseButtonDown(0) || UICamera.selectedObject != UIManager.World.RotationLockButtonGameObject && UICamera.selectedObject != UIManager.World.PerspectiveButtonGameObject) && (key != Control.Confirm || !UnityXInput.Input.GetMouseButtonDown(0) || UICamera.selectedObject != gameObject)) &&
               (PersistenSingleton<HonoInputManager>.Instance.IsInput(key) || lazyKeyCommand == key ||
                 key == Control.Confirm && UnityXInput.Input.GetMouseButtonDown(0) && EventHUD.CurrentHUD == MinigameHUD.None && PersistenSingleton<UIManager>.Instance.State != UIManager.UIState.EndGame && PersistenSingleton<UIManager>.Instance.Dialogs.GetChoiceDialog() == null && lazyKeyCommand == Control.None && !Configuration.Control.DisableMouseInFields);
    }

    public Boolean GetKeyTrigger(Control key)
    {
        if (PersistenSingleton<UIManager>.Instance.State != UIManager.UIState.FieldHUD && PersistenSingleton<UIManager>.Instance.State != UIManager.UIState.WorldHUD && PersistenSingleton<UIManager>.Instance.State != UIManager.UIState.BattleHUD && PersistenSingleton<UIManager>.Instance.State != UIManager.UIState.QuadMistBattle && PersistenSingleton<UIManager>.Instance.State != UIManager.UIState.Title && PersistenSingleton<UIManager>.Instance.UnityScene != UIManager.Scene.EndGame)
            return false;
        if (UnityXInput.Input.GetMouseButtonDown(0) || UnityXInput.Input.GetMouseButtonDown(1) || UnityXInput.Input.GetMouseButtonDown(2))
        {
            if (key != Control.Left && key != Control.Right && key != Control.Up && key != Control.Down && PersistenSingleton<HonoInputManager>.Instance.IsInputUp(key))
                return true;
        }
        else if (PersistenSingleton<HonoInputManager>.Instance.IsInputDown(key))
        {
            return true;
        }
        if (lazyKeyCommand != key)
            return false;
        ResetKeyCode();
        return true;
    }

    private void Update()
    {
        try
        {
            GameLoopManager.RaiseUpdateEvent();

            if (UnityXInput.Input.GetAxis("Mouse X") < 1.0 / 1000.0 && UnityXInput.Input.GetAxis("Mouse Y") < 1.0 / 1000.0)
            {
                disableMouseCounter += Time.deltaTime;
            }
            else
            {
                disableMouseCounter = 0.0f;
                if (!UICamera.list[0].useMouse)
                    UICamera.list[0].useMouse = true;
            }
            if (UnityXInput.Input.GetMouseButton(0) || UnityXInput.Input.GetMouseButton(1) || (UnityXInput.Input.GetMouseButton(2) || Mathf.Abs(UnityXInput.Input.GetAxis("Mouse ScrollWheel")) > 0.00999999977648258))
            {
                disableMouseCounter = 0.0f;
                if (!UICamera.list[0].useMouse)
                    UICamera.list[0].useMouse = true;
            }
            if (disableMouseCounter > 1.0 && UICamera.list[0].useMouse)
                UICamera.list[0].useMouse = false;
            if (!UnityXInput.Input.anyKey && !isLockLazyInput)
                ResetKeyCode();
            if (Configuration.Lang.DualLanguageMode == 1)
                Localization.UseSecondaryLanguage = IsKeyLocked(LockKey.Caps);
            AccelerateKeyNavigation();
            if (HandleMenuControlKeyPressCustomInput())
                return;
            HandleBoosterButton();
            HandleDialogControlKeyPressCustomInput();
        }
        catch (Exception err)
        {
            Log.Error(err);
        }
    }

    private void AccelerateKeyNavigation()
    {
        if (!UnityXInput.Input.anyKey &&
            HonoInputManager.Instance.GetHorizontalNavigation() <= HonoInputManager.AnalogThreadhold &&
            HonoInputManager.Instance.GetHorizontalNavigation() >= -HonoInputManager.AnalogThreadhold &&
            HonoInputManager.Instance.GetVerticalNavigation() <= HonoInputManager.AnalogThreadhold &&
            HonoInputManager.Instance.GetVerticalNavigation() >= -HonoInputManager.AnalogThreadhold)
        {
            if (!firstTimeInput)
                UICamera.EventWaitTime = 0.175f;
            fastEventCounter = RealTime.time;
            firstTimeInput = true;
        }
        else
        {
            if (firstTimeInput)
            {
                fastEventCounter = RealTime.time;
                firstTimeInput = false;
            }
            if (RealTime.time - (Double)fastEventCounter <= 0.300000011920929)
                return;
            UICamera.EventWaitTime = 0.1f;
        }
    }

    public void HandleBoosterButton(BoosterType triggerType = BoosterType.None)
    {
        if (!Configuration.Cheats.Enabled)
            return;
        if (PersistenSingleton<UIManager>.Instance.State == UIManager.UIState.Title || PersistenSingleton<UIManager>.Instance.State == UIManager.UIState.PreEnding || (PersistenSingleton<UIManager>.Instance.State == UIManager.UIState.Ending || !MBG.IsNull && !MBG.Instance.IsFinishedForDisableBooster()))
            return;
        if (UnityXInput.Input.GetKeyDown(KeyCode.F1) || triggerType == BoosterType.HighSpeedMode)
        {
            if (!Configuration.Cheats.SpeedMode)
            {
                Log.Message("[Cheats] SpeedMode was disabled.");
                FF9Sfx.FF9SFX_Play(102);
                return;
            }

            Boolean flag = !FF9StateSystem.Settings.IsBoosterButtonActive[1];
            FF9StateSystem.Settings.CallBoosterButtonFuntion(BoosterType.HighSpeedMode, flag);
            PersistenSingleton<UIManager>.Instance.Booster.SetBoosterHudIcon(BoosterType.HighSpeedMode, flag);
            PersistenSingleton<UIManager>.Instance.Booster.SetBoosterButton(BoosterType.HighSpeedMode, flag);
        }
        if (UnityXInput.Input.GetKeyDown(KeyCode.F2) || triggerType == BoosterType.BattleAssistance)
        {
            if (!Configuration.Cheats.BattleAssistance)
            {
                Log.Message("[Cheats] BattleAssistance was disabled.");
                FF9Sfx.FF9SFX_Play(102);
                return;
            }

            if ((FF9StateSystem.Battle.isNoBoosterMap() || FF9StateSystem.Battle.FF9Battle.btl_escape_fade != 32) && SceneDirector.IsBattleScene())
                return;
            Boolean flag = !FF9StateSystem.Settings.IsBoosterButtonActive[0];
            FF9StateSystem.Settings.CallBoosterButtonFuntion(BoosterType.BattleAssistance, flag);
            PersistenSingleton<UIManager>.Instance.Booster.SetBoosterHudIcon(BoosterType.BattleAssistance, flag);
            PersistenSingleton<UIManager>.Instance.Booster.SetBoosterButton(BoosterType.BattleAssistance, flag);
        }
        if (UnityXInput.Input.GetKeyDown(KeyCode.F3) || triggerType == BoosterType.Attack9999)
        {
            if (!Configuration.Cheats.Attack9999)
            {
                Log.Message("[Cheats] Attack9999 was disabled.");
                FF9Sfx.FF9SFX_Play(102);
                return;
            }

            Boolean flag = !FF9StateSystem.Settings.IsBoosterButtonActive[3];
            FF9StateSystem.Settings.CallBoosterButtonFuntion(BoosterType.Attack9999, flag);
            PersistenSingleton<UIManager>.Instance.Booster.SetBoosterHudIcon(BoosterType.Attack9999, flag);
            PersistenSingleton<UIManager>.Instance.Booster.SetBoosterButton(BoosterType.Attack9999, flag);
        }
        if (UnityXInput.Input.GetKeyDown(KeyCode.F4) || triggerType == BoosterType.NoRandomEncounter)
        {
            if (!Configuration.Cheats.NoRandomEncounter)
            {
                Log.Message("[Cheats] NoRandomEncounter was disabled.");
                FF9Sfx.FF9SFX_Play(102);
                return;
            }

            if (PersistenSingleton<UIManager>.Instance.State != UIManager.UIState.FieldHUD && PersistenSingleton<UIManager>.Instance.State != UIManager.UIState.WorldHUD && PersistenSingleton<UIManager>.Instance.State != UIManager.UIState.BattleHUD && PersistenSingleton<UIManager>.Instance.State != UIManager.UIState.Pause)
                return;
            Boolean flag = !FF9StateSystem.Settings.IsBoosterButtonActive[4];
            FF9StateSystem.Settings.CallBoosterButtonFuntion(BoosterType.NoRandomEncounter, flag);
            PersistenSingleton<UIManager>.Instance.Booster.SetBoosterHudIcon(BoosterType.NoRandomEncounter, flag);
            PersistenSingleton<UIManager>.Instance.Booster.SetBoosterButton(BoosterType.NoRandomEncounter, flag);
        }
        if (UnityXInput.Input.GetKeyDown(KeyCode.F5) && (PersistenSingleton<UIManager>.Instance.State == UIManager.UIState.FieldHUD || PersistenSingleton<UIManager>.Instance.State == UIManager.UIState.WorldHUD || PersistenSingleton<UIManager>.Instance.State == UIManager.UIState.Pause))
        {
            if (!Configuration.Cheats.MasterSkill)
            {
                Log.Message("[Cheats] MasterSkill was disabled.");
                FF9Sfx.FF9SFX_Play(102);
                return;
            }

            if (!FF9StateSystem.Settings.IsMasterSkill)
            {
                PersistenSingleton<UIManager>.Instance.Booster.ShowWaringDialog(BoosterType.MasterSkill);
            }
            else
            {
                FF9StateSystem.Settings.CallBoosterButtonFuntion(BoosterType.MasterSkill, false);
                PersistenSingleton<UIManager>.Instance.Booster.SetBoosterHudIcon(BoosterType.MasterSkill, false);
            }
        }
        if (UnityXInput.Input.GetKeyDown(KeyCode.F6) && (PersistenSingleton<UIManager>.Instance.State == UIManager.UIState.FieldHUD || PersistenSingleton<UIManager>.Instance.State == UIManager.UIState.WorldHUD || PersistenSingleton<UIManager>.Instance.State == UIManager.UIState.Pause))
        {
            if (!Configuration.Cheats.LvMax)
            {
                Log.Message("[Cheats] LvMax was disabled.");
                FF9Sfx.FF9SFX_Play(102);
                return;
            }

            PersistenSingleton<UIManager>.Instance.Booster.ShowWaringDialog(BoosterType.LvMax);
        }
        if (UnityXInput.Input.GetKeyDown(KeyCode.F7) && (PersistenSingleton<UIManager>.Instance.State == UIManager.UIState.FieldHUD || PersistenSingleton<UIManager>.Instance.State == UIManager.UIState.WorldHUD || PersistenSingleton<UIManager>.Instance.State == UIManager.UIState.Pause))
        {
            if (!Configuration.Cheats.GilMax)
            {
                Log.Message("[Cheats] GilMax was disabled.");
                FF9Sfx.FF9SFX_Play(102);
                return;
            }

            PersistenSingleton<UIManager>.Instance.Booster.ShowWaringDialog(BoosterType.GilMax);
        }
        if (Configuration.Control.SoftReset && ((UnityXInput.Input.GetKeyDown(KeyCode.F8) && PersistenSingleton<UIManager>.Instance.IsPause) || SoftResetKeyPSXDown))
        { // Soft Reset
            if (ButtonGroupState.ActiveGroup == QuitUI.WarningMenuGroupButton)
                return;

            if (Configuration.Debug.StartModelViewer)
            {
                ModelViewerScene.initialized = false;
                if (!ModelViewerScene.initialized)
                    ModelViewerScene.Init();
                if (ModelViewerScene.initialized)
                    ModelViewerScene.Update();
                if (PersistenSingleton<UIManager>.Instance.IsPause)
                    PersistenSingleton<UIManager>.Instance.GetSceneFromState(PersistenSingleton<UIManager>.Instance.State).OnKeyPause(null);
                return;
            }

            Log.Message("[Soft Reset]");
            preventTurboKey = false;

            if (PersistenSingleton<UIManager>.Instance.UnityScene == UIManager.Scene.World && PersistenSingleton<UIManager>.Instance.WorldHUDScene != (UnityEngine.Object)null) // World Map
            {
                PersistenSingleton<UIManager>.Instance.WorldHUDScene.MiniMapPanel.SetActive(false);
                PersistenSingleton<UIManager>.Instance.WorldHUDScene.FullMapPanel.SetActive(false);
                PersistenSingleton<UIManager>.Instance.WorldHUDScene.MapButtonGameObject.SetActive(false);
                if (PersistenSingleton<UIManager>.Instance.WorldHUDScene.CurrentState == WorldHUD.State.FullMap)
                    PersistenSingleton<UIManager>.Instance.WorldHUDScene.OnKeySelect(null);
                PersistenSingleton<UIManager>.Instance.WorldHUDScene.ClearFullMapLocations();
                UIManager.Input.ResetKeyCode();
                EIcon.IsProcessingFIcon = true;
                PersistenSingleton<EventEngine>.Instance.SetUserControl(true);
                PersistenSingleton<EventEngine>.Instance.eBin.setVarManually(9173, 0);
                Singleton<BubbleUI>.Instance.SetGameObjectActive(false);
                PersistenSingleton<UIManager>.Instance.SetPlayerControlEnable(true, (Action)null);
                PersistenSingleton<UIManager>.Instance.SetMenuControlEnable(true);
                ff9.w_naviMode = 0;
            }

            PersistenSingleton<UIManager>.Instance.Dialogs.PauseAllDialog(true);
            PersistenSingleton<UIManager>.Instance.HideAllHUD();
            Singleton<DialogManager>.Instance.CloseAll();
            ButtonGroupState.DisableAllGroup(true);
            UIManager.Battle.FF9BMenu_EnableMenu(false);
            if (PersistenSingleton<UIManager>.Instance.IsPause)
                PersistenSingleton<UIManager>.Instance.GetSceneFromState(PersistenSingleton<UIManager>.Instance.State).OnKeyPause(null);
            FF9StateSystem.Battle.FF9Battle.btl_seq = 1; // Prevent the Start button to pause again
            UIManager.Battle.DisableAutoBattle();
            EventHUD.Cleanup();
            EventInput.ClearPadMask();
            TimerUI.SetEnable(false);
            SceneDirector.FadeEventSetColor(FadeMode.Sub, Color.black);
            SceneDirector.Replace("Title", SceneTransition.FadeOutToBlack_FadeIn, true);
            return;
        }
        if (UnityXInput.Input.GetKeyDown(KeyCode.F9) && Configuration.Control.TurboDialog)
        {
            if (TurboKey)
                TurboKey = false;
            else
                TurboKey = true;
        }
    }

    [DllImport("user32.dll")]
    private static extern bool DestroyWindow(IntPtr hwnd);
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    private void OnApplicationQuit()
    {
        if (PersistenSingleton<UIManager>.Instance.UnityScene != UIManager.Scene.Bundle && !quitConfirm)
        {
            Application.CancelQuit();
            OnQuitCommandDetected();
        }
        else
        {
            GameLoopManager.RaiseQuitEvent();
            // DestroyWindow closes faster
            try { DestroyWindow(GetActiveWindow()); } catch { }
        }
    }

    public void ConfirmQuit()
    {
        AutoSplitterPipe.SignalLoadStart(); // Pause the in-game time when closing the program
        quitConfirm = true;
        BroadcastAll("OnQuit");
        Application.Quit();
    }

    public void OnQuitCommandDetected(UIScene scene)
    {
        if (PersistenSingleton<UIManager>.Instance.IsLoading || PersistenSingleton<UIManager>.Instance.QuitScene.isShowQuitUI)
            return;

        if (PersistenSingleton<UIManager>.Instance.TitleScene != null && PersistenSingleton<UIManager>.Instance.TitleScene.IsSplashTextActive && FF9StateSystem.PCPlatform)
            return;

        PersistenSingleton<UIManager>.Instance.QuitScene.SetPreviousActiveGroup();
        if (scene != null)
        {
            quitConfirm = false;
            scene.OnKeyQuit();
        }
        else
        {
            PersistenSingleton<UIManager>.Instance.QuitScene.Show(null);
        }
    }

    private void OnUIConfigDetected(UIScene scene)
    {
        UIManager uiManager = PersistenSingleton<UIManager>.Instance;
        if (uiManager.IsLoading || uiManager.QuitScene.isShowQuitUI || uiManager.State == UIManager.UIState.Serialize)
        {
            FF9Sfx.FF9SFX_Play(102);
            return;
        }

        switch (PersistenSingleton<UIManager>.Instance.State)
        {
            case UIManager.UIState.BattleHUD:
            {
                BattleHUD battleHUD = scene as BattleHUD;
                if (battleHUD == null)
                    break;
                if (battleHUD.UIControlPanel == null)
                    battleHUD.UIControlPanel = new BattleUIControlPanel(battleHUD);
                battleHUD.UIControlPanel.Show = !battleHUD.UIControlPanel.Show;
                FF9Sfx.FF9SFX_Play(103);
                return;
            }
            case UIManager.UIState.MainMenu:
            case UIManager.UIState.Item:
            case UIManager.UIState.Ability:
            case UIManager.UIState.Equip:
            case UIManager.UIState.Shop:
            case UIManager.UIState.Chocograph:
            {
                if (MainMenuUI.UIControlPanel == null)
                    MainMenuUI.UIControlPanel = new MenuUIControlPanel();
                MainMenuUI.UIControlPanel.Scene = scene;
                MainMenuUI.UIControlPanel.Show = !MainMenuUI.UIControlPanel.Show;
                FF9Sfx.FF9SFX_Play(103);
                return;
            }
        }
        FF9Sfx.FF9SFX_Play(102);
    }

    private void OnPartySceneCommandDetected(UIScene scene)
    {
        UIManager uiManager = PersistenSingleton<UIManager>.Instance;
        if (uiManager.IsLoading || uiManager.QuitScene.isShowQuitUI || uiManager.State == UIManager.UIState.Serialize)
        {
            FF9Sfx.FF9SFX_Play(102);
            return;
        }

        if (!uiManager.IsMenuControlEnable)
        {
            FF9Sfx.FF9SFX_Play(102);
            return;
        }

        switch (PersistenSingleton<UIManager>.Instance.State)
        {
            case UIManager.UIState.FieldHUD:
                if (FF9StateSystem.Common.FF9.fldMapNo == 2207 && FF9StateSystem.EventState.ScenarioCounter == 9840)
                {
                    // Hotfix: prevent party switch in "Palace/Hall" when Zidane returns with the Gulug Stone
                    FF9Sfx.FF9SFX_Play(102);
                    return;
                }
                break;
            case UIManager.UIState.WorldHUD:
                break;
            default:
                FF9Sfx.FF9SFX_Play(102);
                return;
        }

        FF9Sfx.FF9SFX_Play(103);
        scene?.Hide(UISceneHelper.OpenPartyMenu);
    }

    private static void OnSaveLoadSceneCommandDetected(UIScene scene, SaveLoadUI.SerializeType type)
    {
        UIManager uiManager = PersistenSingleton<UIManager>.Instance;
        if (uiManager.IsLoading || uiManager.QuitScene.isShowQuitUI || uiManager.State == UIManager.UIState.Serialize)
        {
            FF9Sfx.FF9SFX_Play(102);
            return;
        }

        if (!uiManager.IsMenuControlEnable)
        {
            FF9Sfx.FF9SFX_Play(102);
            return;
        }

        switch (type)
        {
            case SaveLoadUI.SerializeType.Save:
                TryShowSaveScene(scene);
                break;

            case SaveLoadUI.SerializeType.Load:
                TryShowLoadScene(scene);
                break;
        }
    }

    private static void OnSoundDebugRoomCommandDetected()
    {
        var instance = PersistenSingleton<SoundDebugRoom.SoundView>.Instance;
        instance.enabled = !instance.enabled;
    }

    private void OnMemoriaMenuCommandDetected()
    {
        var instance = PersistenSingleton<MemoriaConfigurationMenu>.Instance;
        instance.enabled = !instance.enabled;
    }

    private static void TryShowSaveScene(UIScene scene)
    {
        switch (PersistenSingleton<UIManager>.Instance.State)
        {
            case UIManager.UIState.FieldHUD:
            case UIManager.UIState.WorldHUD:
                break;
            default:
                FF9Sfx.FF9SFX_Play(102);
                return;
        }

        FF9Sfx.FF9SFX_Play(103);
        scene?.Hide(OnSaveGameButtonClick);
    }

    private static void TryShowLoadScene(UIScene scene)
    {
        FF9Sfx.FF9SFX_Play(103);
        scene?.Hide(OnLoadGameButtonClick);
    }

    private static void OnSaveGameButtonClick()
    {
        PersistenSingleton<UIManager>.Instance.SaveLoadScene.Type = SaveLoadUI.SerializeType.Save;
        PersistenSingleton<UIManager>.Instance.ChangeUIState(UIManager.UIState.Serialize);
    }

    private static void OnLoadGameButtonClick()
    {
        PersistenSingleton<UIManager>.Instance.SaveLoadScene.Type = SaveLoadUI.SerializeType.Load;
        PersistenSingleton<UIManager>.Instance.ChangeUIState(UIManager.UIState.Serialize);
    }

    public void OnQuitCommandDetected()
    {
        OnQuitCommandDetected(PersistenSingleton<UIManager>.Instance.GetSceneFromState(PersistenSingleton<UIManager>.Instance.State));
    }

    private static void BroadcastAll(String method)
    {
        foreach (GameObject gameObject in (GameObject[])FindObjectsOfType(typeof(GameObject)))
        {
            if (gameObject && gameObject.transform.parent == null)
                gameObject.gameObject.BroadcastMessage(method, SendMessageOptions.DontRequireReceiver);
        }
    }

    private Boolean HandleMenuControlKeyPressCustomInput(GameObject activeButton = null)
    {
        IsShiftKeyPressed = ShiftKey;

        UIScene sceneFromState = PersistenSingleton<UIManager>.Instance.GetSceneFromState(PersistenSingleton<UIManager>.Instance.State);
        Boolean battelAutoConfirm = Configuration.Control.BattleAutoConfirm && (UIManager.Instance.State == UIManager.UIState.BattleHUD || UIManager.Instance.State == UIManager.UIState.BattleResult);
        if (ButtonGroupState.ActiveButton && ButtonGroupState.ActiveButton != PersistenSingleton<UIManager>.Instance.gameObject)
            activeButton = ButtonGroupState.ActiveButton;
        else if (activeButton == null)
            activeButton = UICamera.selectedObject;
        if (sceneFromState != null && (!PersistenSingleton<UIManager>.Instance.Dialogs.Activate || PersistenSingleton<UIManager>.Instance.IsPause))
        {
            if (sceneFromState.GetType() == typeof(ConfigUI) && FF9StateSystem.AndroidTVPlatform && PersistenSingleton<HonoInputManager>.Instance.IsInputDown(Control.Pause) && !SoftResetKeyPSXForPause)
            {
                if (PersistenSingleton<UIManager>.Instance.IsPauseControlEnable)
                    sceneFromState.OnKeyPause(activeButton);
                return true;
            }
            if (PersistenSingleton<HonoInputManager>.Instance.IsInputDown(Control.Cancel) || keyCommand == Control.Cancel)
            {
                keyCommand = Control.None;
                sceneFromState.OnKeyCancel(activeButton);
                return true;
            }
            if (PersistenSingleton<HonoInputManager>.Instance.IsInputDown(Control.Confirm) || keyCommand == Control.Confirm)
            {
                if (battelAutoConfirm)
                    autoConfirmDownTime = Time.time;
                keyCommand = Control.None;
                sceneFromState.OnKeyConfirm(activeButton);
                return true;
            }
            if (battelAutoConfirm)
            {
                // The expected chain would be "IsInputDown -> IsInput -> IsInputUp" but it's not always like that (sometimes there is only "IsInputDown", sometimes "IsInput" procs before "IsInputDown"...)
                if (PersistenSingleton<HonoInputManager>.Instance.IsInput(Control.Confirm))
                {
                    // If confirm is held more than 500ms it will auto confirm at an interval of 100ms
                    const Single delay = 0.5f;
                    if (autoConfirmDownTime > 0 && Time.time - autoConfirmDownTime > delay)
                    {
                        autoConfirmDownTime = Time.time - delay + 0.1f;
                        sceneFromState.OnKeyConfirm(activeButton);
                        return true;
                    }
                }
                else
                {
                    autoConfirmDownTime = 0f;
                }
            }
            if ((PersistenSingleton<HonoInputManager>.Instance.IsInputDown(Control.Pause) || keyCommand == Control.Pause) && !SoftResetKeyPSXForPause)
            {
                keyCommand = Control.None;
                if (PersistenSingleton<UIManager>.Instance.IsPauseControlEnable)
                    sceneFromState.OnKeyPause(activeButton);
                return true;
            }
            if (PersistenSingleton<HonoInputManager>.Instance.IsInputDown(Control.Select) || keyCommand == Control.Select)
            {
                keyCommand = Control.None;
                sceneFromState.OnKeySelect(UICamera.selectedObject);
                return true;
            }
            if (PersistenSingleton<HonoInputManager>.Instance.IsInputDown(Control.Menu) || keyCommand == Control.Menu)
            {
                keyCommand = Control.None;
                if (FF9StateSystem.AndroidTVPlatform && FF9StateSystem.EnableAndroidTVJoystickMode && (PersistenSingleton<HonoInputManager>.Instance.GetSource(Control.Menu) == SourceControl.Joystick && PersistenSingleton<UIManager>.Instance.State == UIManager.UIState.Pause))
                    sceneFromState.OnKeyMenu(activeButton);
                else if (PersistenSingleton<UIManager>.Instance.IsMenuControlEnable)
                    sceneFromState.OnKeyMenu(activeButton);
                return true;
            }
            if (PersistenSingleton<HonoInputManager>.Instance.IsInputDown(Control.Special) || keyCommand == Control.Special)
            {
                keyCommand = Control.None;
                sceneFromState.OnKeySpecial(activeButton);
                return true;
            }
            if (PersistenSingleton<HonoInputManager>.Instance.IsInputDown(Control.LeftBumper) || keyCommand == Control.LeftBumper)
            {
                keyCommand = Control.None;
                sceneFromState.OnKeyLeftBumper(activeButton);
                return true;
            }
            if (PersistenSingleton<HonoInputManager>.Instance.IsInputDown(Control.RightBumper) || keyCommand == Control.RightBumper)
            {
                keyCommand = Control.None;
                sceneFromState.OnKeyRightBumper(activeButton);
                return true;
            }
            if (PersistenSingleton<HonoInputManager>.Instance.IsInputDown(Control.LeftTrigger) || keyCommand == Control.LeftTrigger)
            {
                if (UIManager.Battle.CanForceNextTurn)
                {
                    BattleHUD.ForceNextTurn = true;
                    FF9Sfx.FF9SFX_Play(103);
                }
                keyCommand = Control.None;
                sceneFromState.OnKeyLeftTrigger(activeButton);
                return true;
            }
            if (PersistenSingleton<HonoInputManager>.Instance.IsInputDown(Control.RightTrigger) || keyCommand == Control.RightTrigger)
            {
                keyCommand = Control.None;
                sceneFromState.OnKeyRightTrigger(activeButton);
                return true;
            }
        }

        if (AltKey)
        {
            if (F1KeyDown)
            {
                OnUIConfigDetected(sceneFromState);
                return true;
            }
            if (F2KeyDown)
            {
                OnPartySceneCommandDetected(sceneFromState);
                return true;
            }
            if (F4KeyDown)
            {
                OnQuitCommandDetected(sceneFromState);
                return true;
            }
            if (F5KeyDown)
            {
                OnSaveLoadSceneCommandDetected(sceneFromState, SaveLoadUI.SerializeType.Save);
                return true;
            }
            if (F9KeyDown)
            {
                OnSaveLoadSceneCommandDetected(sceneFromState, SaveLoadUI.SerializeType.Load);
                return true;
            }
            if (SpaceKeyDown)
            {
                Configuration.Graphics.WidescreenSupport = !Configuration.Graphics.WidescreenSupport;
                return true;
            }

            if (IsShiftKeyPressed && ControlKey)
            {
                if (SKeyDown)
                {
                    OnSoundDebugRoomCommandDetected();
                    return true;
                }

                if (MKeyDown)
                {
                    OnMemoriaMenuCommandDetected();
                    return true;
                }

                if (F12KeyDown)
                {
                    GameObjectService.Start();
                    return true;
                }
            }
        }

        if (IsShiftKeyPressed && F4KeyDown)
        {
            if (ControlKey)
            {
                if (Configuration.Cheats.NoRandomEncounter)
                {
                    SettingsState.IsFriendlyBattleOnly = (SettingsState.IsFriendlyBattleOnly + 1) % 3;
                    FF9Sfx.FF9SFX_Play(SettingsState.IsFriendlyBattleOnly == 1 ? 106 : SettingsState.IsFriendlyBattleOnly == 2 ? 1043 : 111);
                }
                else
                {
                    FF9Sfx.FF9SFX_Play(102);
                }
                return true;
            }

            SettingsState.IsRapidEncounter = !SettingsState.IsRapidEncounter;
            FF9Sfx.FF9SFX_Play(SettingsState.IsRapidEncounter ? 1325 : 1047);
            return true;
        }

        return false;
    }

    private void HandleDialogControlKeyPressCustomInput(GameObject activeButton = null)
    {
        if (activeButton == null)
            activeButton = UICamera.selectedObject;

        List<Control> dialogConfirmKeys = new List<Control>();
        foreach (String key in Configuration.Control.DialogProgressButtons)
            if (key.TryEnumParse<Control>(out Control ctrl))
                dialogConfirmKeys.Add(ctrl);
        if (dialogConfirmKeys.Any(ctrl => PersistenSingleton<HonoInputManager>.Instance.IsInputDown(ctrl) || keyCommand == ctrl) || ShouldTurboDialog(dialogConfirmKeys))
        {
            keyCommand = Control.None;
            PersistenSingleton<UIManager>.Instance.Dialogs.OnKeyConfirm(activeButton);
            preventTurboKey = false;
            if (PersistenSingleton<UIManager>.Instance.Dialogs.IsDialogNeedControl() || !PersistenSingleton<UIManager>.Instance.Dialogs.CompletlyVisible)
                return;

            if (PersistenSingleton<HonoInputManager>.Instance.IsInputDown(Control.Confirm))
                triggleEventDialog = true;
        }
        else if (PersistenSingleton<HonoInputManager>.Instance.IsInputDown(Control.Cancel) || keyCommand == Control.Cancel)
        {
            keyCommand = Control.None;
            PersistenSingleton<UIManager>.Instance.Dialogs.OnKeyCancel(activeButton);
            preventTurboKey = false;
        }
        else if (PersistenSingleton<HonoInputManager>.Instance.IsInputDown(Control.Pause) || keyCommand == Control.Pause)
        {
            keyCommand = Control.None;
            if (!PersistenSingleton<UIManager>.Instance.IsPauseControlEnable)
                return;
            PersistenSingleton<UIManager>.Instance.GetSceneFromState(PersistenSingleton<UIManager>.Instance.State)?.OnKeyPause(activeButton);
        }
        else if (PersistenSingleton<HonoInputManager>.Instance.IsInputDown(Control.Menu) || keyCommand == Control.Menu)
        {
            keyCommand = Control.None;
            if (!PersistenSingleton<UIManager>.Instance.IsMenuControlEnable)
                return;
            PersistenSingleton<UIManager>.Instance.GetSceneFromState(PersistenSingleton<UIManager>.Instance.State)?.OnKeyMenu(activeButton);
        }
    }

    protected virtual void OnSelect(Boolean selected)
    {
        if (!selected || PersistenSingleton<UIManager>.Instance.IsLoading || (!(ButtonGroupState.ActiveButton != null) || !(ButtonGroupState.ActiveButton != gameObject)) || (ButtonGroupState.AllTargetEnabled || !ButtonGroupState.ActiveButton.GetComponent<ButtonGroupState>().enabled))
            return;
        UICamera.selectedObject = ButtonGroupState.ActiveButton;
        ButtonGroupState.ActiveButton.GetComponent<ButtonGroupState>().SetHover(true);
    }

    public virtual void OnScreenButtonPressed(GameObject go)
    {
        keyCommand = go.GetComponent<OnScreenButton>().KeyCommand;
    }

    public void ResetKeyCode()
    {
        lazyKeyCommand = Control.None;
        isLockLazyInput = false;
    }

    public void SendKeyCode(Control control, Boolean isLock = false)
    {
        lazyKeyCommand = control;
        isLockLazyInput = isLock;
    }

    public Control GetLazyKey()
    {
        return lazyKeyCommand;
    }

    public void OnKeyNavigate(GameObject go, KeyCode key)
    {
        if (PersistenSingleton<HonoInputManager>.Instance.GetDirectionAxisSource() != SourceControl.Touch)
            return;

        switch (key)
        {
            case KeyCode.UpArrow:
                lazyKeyCommand = Control.Up;
                break;
            case KeyCode.DownArrow:
                lazyKeyCommand = Control.Down;
                break;
            case KeyCode.RightArrow:
                lazyKeyCommand = Control.Right;
                break;
            case KeyCode.LeftArrow:
                lazyKeyCommand = Control.Left;
                break;
        }
    }

    public virtual void OnItemSelect(GameObject go)
    {
        UIScene sceneFromState = PersistenSingleton<UIManager>.Instance.GetSceneFromState(PersistenSingleton<UIManager>.Instance.State);
        sceneFromState?.OnItemSelect(go);

        if (PersistenSingleton<UIManager>.Instance.Dialogs != null)
            PersistenSingleton<UIManager>.Instance.Dialogs.OnItemSelect(go);

        if (!go.GetComponent<ScrollItemKeyNavigation>())
            return;

        ScrollItemKeyNavigation component = go.GetComponent<ScrollItemKeyNavigation>();
        if (!component || !component.ListPopulator)
            return;

        component.ListPopulator.itemHasChanged(go);
    }

    public static String ControlToString(Control control)
    {
        switch (control)
        {
            case Control.Confirm:
                return "Submit";
            case Control.Cancel:
                return "Cancel";
            case Control.Menu:
                return "Menu";
            case Control.Special:
                return "Special";
            case Control.LeftBumper:
                return "Left Bumper";
            case Control.RightBumper:
                return "Right Bumper";
            case Control.LeftTrigger:
                return "Left Trigger";
            case Control.RightTrigger:
                return "Right Trigger";
            case Control.Pause:
                return "Pause";
            case Control.Select:
                return "Select";
            default:
                return String.Empty;
        }
    }

    public Boolean ContainsAndroidQuitKey()
    {
        if (Application.platform != RuntimePlatform.Android || !UnityXInput.Input.GetKey(KeyCode.Escape))
        {
        }
        return false;
    }

    private Boolean ShouldTurboDialog(List<Control> confirmKeys)
    {
        if (!Configuration.Control.TurboDialog || preventTurboKey)
            return false;

        if (TurboKey || ((HonoInputManager.Instance.IsInput(Control.RightBumper) || ShiftKey) && confirmKeys.Any(HonoInputManager.Instance.IsInput)))
        {
            if (UIManager.Instance.Dialogs.IsDialogNeedControl())
                return true;

            if (VoicePlayer.scriptRequestedButtonPress && DialogManager.Instance.ActiveDialogList.Any(dial => dial.gameObject.activeInHierarchy && (dial.Style == Dialog.WindowStyle.WindowStyleAuto || dial.Style == Dialog.WindowStyle.WindowStyleTransparent)))
            {
                ETb.sKey &= ~EventInput.GetKeyMaskFromControl(Control.Confirm);
                EventInput.ReceiveInput(EventInput.GetKeyMaskFromControl(Control.Confirm));
            }
        }
        return false;
    }

    private void Start()
    {
        UICamera.onNavigate = (UICamera.KeyCodeDelegate)Delegate.Combine(UICamera.onNavigate, (UICamera.KeyCodeDelegate)OnKeyNavigate);
        GameLoopManager.RaiseStartEvent();
        //DebugRectAroundObjectFactory.Run();
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
    private static extern Int16 GetKeyState(Int32 keyCode);

    public static Boolean IsKeyLocked(LockKey keyCode)
    {
        return (GetKeyState((Int32)keyCode) & 0xFFFF) != 0;
    }

    public enum LockKey
    {
        Caps = 0x14,
        Num = 0x90,
        Scroll = 0x91
    }
}


namespace Memoria
{
    public static class UISceneHelper
    {
        public static FF9PARTY_INFO GetCurrentPartyForMenu()
        {
            FF9PARTY_INFO party = new FF9PARTY_INFO();
            List<CharacterId> selectList = new List<CharacterId>();

            foreach (PLAYER p in FF9StateSystem.Common.FF9.PlayerList)
                if (p.info.party != 0)
                    selectList.Add(p.info.slot_no);
            party.party_ct = Math.Min(4, selectList.Count);
            party.exact_party_ct = -1;

            for (Int32 memberIndex = 0; memberIndex < 4; ++memberIndex)
            {
                if (FF9StateSystem.Common.FF9.party.member[memberIndex] != null)
                {
                    CharacterId characterId = FF9StateSystem.Common.FF9.party.member[memberIndex].info.slot_no;
                    party.menu[memberIndex] = characterId;
                    selectList.Remove(characterId);
                }
                else
                {
                    party.menu[memberIndex] = CharacterId.NONE;
                }
            }
            party.select = selectList.ToArray();

            if (PersistenSingleton<UIManager>.Instance.UnityScene == UIManager.Scene.Battle)
            {
                party.exact_party_ct = FF9StateSystem.Common.FF9.party.MemberCount;
                party.fix = UIManager.Battle.GetNonSwappableCharacters();
            }
            return party;
        }

        public static void OpenPartyMenu()
        {
            EventService.OpenPartyMenu(GetCurrentPartyForMenu());
        }
    }
}
