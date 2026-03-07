using Memoria;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class ModuleInitializerAttribute : Attribute { }
}

namespace Memoria.Scripts.Battle
{
    public static class ModInitializer
    {
        [ModuleInitializer]
        public static void RunOnAssemblyLoad()
        {
            try
            {
                GameObject watcherObj = new GameObject("SaveLoadWatcher_TonMod");
                GameObject.DontDestroyOnLoad(watcherObj);
                watcherObj.AddComponent<TonModWatcher>();

            }
            catch (Exception)
            {
            }
        }
    }

    // 2. Le fameux "Timer/Watcher" qui tourne en boucle
    public class TonModWatcher : MonoBehaviour
    {
        private bool _wasInLoadMenu = false;
        private bool _wasInTitleScreen = true;

        void Update()
        {
            try
            {
                var ui = PersistenSingleton<UIManager>.Instance;

                if (ui == null || ui.SaveLoadScene == null || ui.TitleScene == null)
                    return;

                bool isInLoadMenu = ui.SaveLoadScene.isActiveAndEnabled && ui.SaveLoadScene.Type == SaveLoadUI.SerializeType.Load;
                bool isInTitleScreen = ui.TitleScene.isActiveAndEnabled;

                if (_wasInLoadMenu && !isInLoadMenu)
                {
                    if (IsPlayerReady()) OnSaveLoaded("Moogle");
                }

                if (_wasInTitleScreen && !isInTitleScreen)
                {
                    if (IsPlayerReady()) OnSaveLoaded("Écran Titre");
                }

                _wasInLoadMenu = isInLoadMenu;
                _wasInTitleScreen = isInTitleScreen;
            }
            catch (Exception)
            {
            }
        }

        private bool IsPlayerReady()
        {
            return FF9StateSystem.Common != null &&
                   FF9StateSystem.Common.FF9 != null &&
                   FF9StateSystem.Common.FF9.party != null;
        }

        private void OnSaveLoaded(string source)
        {
            if (FF9StateSystem.EventState.gEventGlobal[1403] >= 4 && FF9StateSystem.EventState.gEventGlobal[1403] <= 6)
            {
                ForceCheatValue("SpeedTimer", true);
                ForceCheatValue("BattleAssistance", false);
                ForceCheatValue("Attack9999", false);
                ForceCheatValue("NoRandomEncounter", false);
                ForceCheatValue("MasterSkill", false);
                ForceCheatValue("LvMax", false);
                ForceCheatValue("GilMax", false);
            }
        }

        private void ForceCheatValue(string cheatName, bool newValue)
        {
            try
            {
                Type configType = typeof(Configuration);
                object instance = null;

                var instanceProp = configType.GetProperty("Instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (instanceProp != null) instance = instanceProp.GetValue(null, null);
                else
                {
                    var instanceField = configType.GetField("Instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    if (instanceField != null) instance = instanceField.GetValue(null);
                }

                if (instance == null) return;

                var cheatsField = configType.GetField("_cheats", BindingFlags.NonPublic | BindingFlags.Instance);
                if (cheatsField == null) return;

                object cheatsSection = cheatsField.GetValue(instance);
                if (cheatsSection == null) return;

                var specificCheatField = cheatsSection.GetType().GetField(cheatName, BindingFlags.Public | BindingFlags.Instance);
                if (specificCheatField != null)
                {
                    object iniValueObj = specificCheatField.GetValue(cheatsSection);
                    if (iniValueObj != null)
                    {
                        var valueField = iniValueObj.GetType().GetField("Value", BindingFlags.Public | BindingFlags.Instance);
                        if (valueField != null)
                        {
                            valueField.SetValue(iniValueObj, newValue);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
