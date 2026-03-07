using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UIManager;

namespace Memoria.Scripts.Battle
{
    public class OverloadedPlayerUI : IOverloadPlayerUIScript
    {
        private static bool _isMenuInjected = false;

        public IOverloadPlayerUIScript.Result UpdatePointStatus(PLAYER player)
        {
            if (!_isMenuInjected && DifficultyDebugMenu._isDebugMenuCalled)
            {
                GameObject menuObj = new GameObject("DifficultyDebugMenu_Obj");
                GameObject.DontDestroyOnLoad(menuObj);
                menuObj.AddComponent<DifficultyDebugMenu>();
                _isMenuInjected = true;
                Memoria.Prime.Log.Message("[Trance Seek DEBUG] Menu de difficulté injecté depuis l'UI !");
            }

            Boolean HPColored = Configuration.Mod.FolderNames.Contains("TranceSeek/ColoredHP");
            Boolean MPColored = Configuration.Mod.FolderNames.Contains("TranceSeek/ColoredMP");
            Boolean GemColored = Configuration.Mod.FolderNames.Contains("TranceSeek/ColoredGems");

            IOverloadPlayerUIScript.Result result = new IOverloadPlayerUIScript.Result();
            result.ColorHP = (player.cur.hp == 0) ? FF9TextTool.Red
                           : (player.cur.hp <= player.max.hp / 6) ? FF9TextTool.Yellow : FF9TextTool.White;
            result.ColorMP = (player.cur.mp <= player.max.mp / 6) ? FF9TextTool.Yellow : FF9TextTool.White;

            if (player.cur.hp == player.max.hp && HPColored)
                result.ColorHP = FF9TextTool.Green;
            if (player.cur.mp == player.max.mp && MPColored)
                result.ColorMP = new Color(0.28104f, 0.43712f, 0.96821f);

            if (!GemColored)
            {
                result.ColorMagicStone = (player.cur.capa == 0) ? FF9TextTool.Yellow : FF9TextTool.White;
            }
            else
            {
                if (ff9abil._FF9Abil_PaData.TryGetValue(player.PresetId, out CharacterAbility[] paArray))
                {
                    Boolean NoSA = true;
                    foreach (CharacterAbility pa in paArray)
                        if (pa.IsPassive)
                            NoSA = false;

                    if (NoSA)
                    {
                        result.ColorMagicStone = FF9TextTool.Gray;
                        return result;
                    }
                }

                if (player.cur.capa == 0)
                {
                    result.ColorMagicStone = FF9TextTool.White;
                    return result;
                }

                float CurCapa = (float)player.cur.capa;
                float MaxCapa = (float)player.max.capa;
                float RatioCapa = CurCapa / MaxCapa;
                float red = 0.80f - (0.80f * RatioCapa);
                float green = 1.0f;
                float blue = 1.0f;

                result.ColorMagicStone = new Color(red, green, blue);
            }

            int IdDict = (int)(2000 + player.Index);
            if (!FF9StateSystem.EventState.gScriptDictionary.TryGetValue(IdDict, out Dictionary<Int32, Int32> dictbattle))
            {
                dictbattle = new Dictionary<Int32, Int32>();
                FF9StateSystem.EventState.gScriptDictionary.Add(IdDict, dictbattle);
                dictbattle[1] = 0;
                dictbattle[2] = 0;
                dictbattle[3] = 0;
            }

            if (player.saExtended.Contains((SupportAbility)1132)) // SA Anastrophe+
            {
                if (dictbattle[3] != 2)
                {
                    dictbattle[1] = 0;
                    dictbattle[2] = 0;
                    dictbattle[3] = 2;
                    ff9play.FF9Play_Update(player);
                    dictbattle[1] = (int)(player.max.hp);
                    dictbattle[2] = (int)(player.max.mp);
                    ff9play.FF9Play_Update(player);
                }
            }
            else if (player.saExtended.Contains((SupportAbility)132)) // SA Anastrophe
            {
                if (dictbattle[3] != 1)
                {
                    dictbattle[1] = 0;
                    dictbattle[2] = 0;
                    dictbattle[3] = 1;
                    ff9play.FF9Play_Update(player);
                    dictbattle[1] = (int)(player.max.hp / 2);
                    dictbattle[2] = (int)(player.max.mp / 2);
                    ff9play.FF9Play_Update(player);
                }
            }
            else
            {
                dictbattle[1] = 0;
                dictbattle[2] = 0;
                dictbattle[3] = 0;
            }

            return result;
        }
    }

    public class DifficultyDebugMenu : MonoBehaviour
    {
        private bool _showMenu = false;
        private Rect _windowRect = new Rect(50, 50, 250, 350);
        public static bool _isDebugMenuCalled = true;
        public static int MegaCheat = 0;

        void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.KeypadPlus) && _isDebugMenuCalled)
            {
                _showMenu = !_showMenu;
                SoundLib.PlaySoundEffect(1362);
            }
        }

        void OnGUI()
        {
            if (_showMenu)
            {
                GUI.backgroundColor = Color.black;
                _windowRect = GUI.Window(1403, _windowRect, DrawMenu, "Mod : Choix Difficulte");
            }
        }

        void DrawMenu(int windowID)
        {
            GUILayout.Space(10);

            if (GUILayout.Button("Zidane")) SetDifficulty(0, 84);
            if (GUILayout.Button("Vivi")) SetDifficulty(1, 82);
            if (GUILayout.Button("Eiko")) SetDifficulty(2, 83);
            if (GUILayout.Button("Kuja")) SetDifficulty(3, 85);
            if (GUILayout.Button("Necron")) SetDifficulty(4, 86);
            if (GUILayout.Button("Beatrix")) SetDifficulty(5, 87);
            if (GUILayout.Button("Ozma")) SetDifficulty(6, 88);
            if (GUILayout.Button("Garland")) SetDifficulty(7, 89);
            if (GUILayout.Button("Disable MegaCheat"))
            {
                MegaCheat = 0;
                SoundLib.PlaySoundEffect(108);
                _showMenu = false;
            }
            if (GUILayout.Button("Activate MegaCheat"))
            {
                MegaCheat = 1;
                SoundLib.PlaySoundEffect(108);
                _showMenu = false;
            }
            if (GUILayout.Button("Activate MegaCheatFULL"))
            {
                MegaCheat = 2;
                SoundLib.PlaySoundEffect(108);
                _showMenu = false;
            }

            GUILayout.Space(15);
            if (GUILayout.Button("Fermer le menu"))
            {
                _showMenu = false;
                SoundLib.PlaySoundEffect(1363);
                _showMenu = false;
            }

            GUI.DragWindow();
        }

        private void SetDifficulty(int globalValue, int importantItemId)
        {
            try
            {
                for (int i = 82; i <= 89; i++)
                {
                    ff9item.FF9Item_RemoveImportant(i);
                }

                ff9item.FF9Item_AddImportant(importantItemId);

                if (FF9StateSystem.EventState.gEventGlobal[1403] >= 4 && FF9StateSystem.EventState.gEventGlobal[1403] <= 6)
                    FF9StateSystem.EventState.gEventGlobal[1407] = 1;
                else
                    FF9StateSystem.EventState.gEventGlobal[1407] = 0;

                SoundLib.PlaySoundEffect(108);
                _showMenu = false;
                Memoria.Prime.Log.Message("[Trance Seek DEBUG] Difficulté changée : " + globalValue + " / Hardcore activée ? : " + (FF9StateSystem.EventState.gEventGlobal[1407] == 1));

            }
            catch (Exception ex)
            {
                Memoria.Prime.Log.Error(ex, "[Trance Seek DEBUG] Erreur dans le changement de difficulte.");
            }
        }
    }
}
