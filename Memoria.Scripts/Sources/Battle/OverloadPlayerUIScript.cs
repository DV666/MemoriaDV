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
        public IOverloadPlayerUIScript.Result UpdatePointStatus(PLAYER player)
        {
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
            if (!FF9StateSystem.EventState.gScriptDictionary.TryGetValue(IdDict, out Dictionary<Int32, Int32> dictbattle)) // Modificators for battle
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

            //if (UnityEngine.Input.GetKey(KeyCode.KeypadPlus) && FF9StateSystem.EventState.ScenarioCounter >= 11100 && FF9StateSystem.EventState.gEventGlobal[1500] > 0)
            if (UnityEngine.Input.GetKey(KeyCode.KeypadPlus))
            { // Debug, to delete at the release ?
                SoundLib.PlaySoundEffect(1362);
                if (GameState.HasKeyItem(82))
                {
                    ff9item.FF9Item_RemoveImportant(82);
                    ff9item.FF9Item_AddImportant(83);
                    FF9StateSystem.EventState.gEventGlobal[1403] = 2;
                }
                else if (GameState.HasKeyItem(83))
                {
                    ff9item.FF9Item_RemoveImportant(83);
                    ff9item.FF9Item_AddImportant(84);
                    FF9StateSystem.EventState.gEventGlobal[1403] = 0;
                }
                else if (GameState.HasKeyItem(84))
                {
                    ff9item.FF9Item_RemoveImportant(84);
                    ff9item.FF9Item_AddImportant(85);
                    FF9StateSystem.EventState.gEventGlobal[1403] = 3;
                }
                else if (GameState.HasKeyItem(85))
                {
                    ff9item.FF9Item_RemoveImportant(85);
                    ff9item.FF9Item_AddImportant(86);
                    FF9StateSystem.EventState.gEventGlobal[1403] = 4;
                }
                else if (GameState.HasKeyItem(86))
                {
                    ff9item.FF9Item_RemoveImportant(86);
                    ff9item.FF9Item_AddImportant(87);
                    FF9StateSystem.EventState.gEventGlobal[1403] = 5;
                }
                else if (GameState.HasKeyItem(87))
                {
                    ff9item.FF9Item_RemoveImportant(87);
                    ff9item.FF9Item_AddImportant(88);
                    FF9StateSystem.EventState.gEventGlobal[1403] = 6;
                }
                else if (GameState.HasKeyItem(88))
                {
                    ff9item.FF9Item_RemoveImportant(88);
                    ff9item.FF9Item_AddImportant(82);
                    FF9StateSystem.EventState.gEventGlobal[1403] = 1;
                }
                else
                {
                    ff9item.FF9Item_AddImportant(84);
                    FF9StateSystem.EventState.gEventGlobal[1403] = 0;
                }
                if (FF9StateSystem.EventState.gEventGlobal[1403] >= 4 && FF9StateSystem.EventState.gEventGlobal[1403] <= 6) // Activate Hardcore IA
                    FF9StateSystem.EventState.gEventGlobal[1407] = 1;
                else
                    FF9StateSystem.EventState.gEventGlobal[1407] = 0;
            }

            return result;
        }
    }
}
