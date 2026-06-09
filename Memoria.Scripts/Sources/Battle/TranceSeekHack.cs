using Memoria.Prime;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static Memoria.Scripts.TranceSeek.TranceSeekDebug;

namespace Memoria.Scripts.TranceSeek
{
    public static class TranceSeekHack
    {
        public static void AbsoluteForceResetCamera() // To unlock camera locked on monsters, mostly after an intro.
        {
            byte defaultCam = 0;
            if (FF9StateSystem.Battle.FF9Battle.btl_scene != null)
            {
                byte sceneCam = FF9StateSystem.Battle.FF9Battle.btl_scene.PatAddr[FF9StateSystem.Battle.FF9Battle.btl_scene.PatNum].Camera;
                defaultCam = sceneCam >= 3 ? (byte)0 : sceneCam;
            }

            if (btlseq.instance != null && btlseq.instance.seq_work_set != null)
                btlseq.instance.seq_work_set.CameraNo = defaultCam;

            SFX.SFX_SendIntData(4, 0, 0, 0);
            SFX.SFX_SendIntData(1, 0, 0, 0);
            SFX.SFX_SendIntData(2, 0, 0, 0);

            SFX.SetCameraPhase(0);
            SFX.SkipCameraAnimation(-1);
            SFX.ResetViewPort();
            SFXDataCamera.currentCameraEngine = SFXDataCamera.CameraEngine.SFX_PLUGIN;

            /*Camera battleCam = Camera.main;
            if (battleCam == null)
            {
                HonoluluBattleMain battleMain = UnityEngine.Object.FindObjectOfType<HonoluluBattleMain>();
                if (battleMain != null && battleMain.cameraController != null)
                    battleCam = battleMain.cameraController.GetComponent<Camera>();
                else
                {
                    GameObject camObj = GameObject.Find("Battle Camera");
                    if (camObj != null) battleCam = camObj.GetComponent<Camera>();
                }
            }

            if (battleCam != null)
            {
                battleCam.ResetWorldToCameraMatrix();
                battleCam.ResetProjectionMatrix();
                battleCam.nearClipPlane = 1f;
                battleCam.farClipPlane = 65535f;
            }

            HonoluluBattleMain mainScript = UnityEngine.Object.FindObjectOfType<HonoluluBattleMain>();
            if (mainScript != null && mainScript.cameraController != null)
                mainScript.cameraController.enabled = true;*/
        }

        public static void InitWatchdog(GameObject battleRoot)
        {
            if (battleRoot != null && battleRoot.GetComponent<TranceSeekCameraWatchdog>() == null)
            {
                battleRoot.AddComponent<TranceSeekCameraWatchdog>();
                Log.Message("[TranceSeekHack] Watchdog immédiat configuré pour SFXRework.");
            }
        }
    }
    public static class Initializer
    {
        [ModuleInitializer]
        public static void RunOnAssemblyLoad()
        {
            try
            {
                GameObject watcherObj = new GameObject("TranceSeek_Watcher");
                GameObject.DontDestroyOnLoad(watcherObj);
                watcherObj.AddComponent<OverloadOnFieldScript>();
#if DEV_TS
                watcherObj.AddComponent<TranceSeekDebugMenu>();
                Log.Message("[Trance Seek Init] TranceSeekDebugMenu loaded.");
#endif
            }
            catch (Exception ex)
            {
                Log.Error($"[Trance Seek Init] ERROR ON LOADING: {ex.Message}");
            }
        }
    }

    public class TranceSeekCameraWatchdog : MonoBehaviour
    {
        private void Update()
        {
            if (!FF9StateSystem.EventState.gScriptDictionary.TryGetValue(1007, out Dictionary<Int32, Int32> dictCameraHack))
                return;

            if (dictCameraHack.TryGetValue(0, out int hackValue) && hackValue == 1)
            {
                Log.Message("[TranceSeekHack] Force reset Camera");

                TranceSeekHack.AbsoluteForceResetCamera();

                dictCameraHack[0] = 0;
            }
        }
    }
}
