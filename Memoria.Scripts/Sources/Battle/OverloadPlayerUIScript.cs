using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Data;
using System;
using System.Linq;
using UnityEngine;

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

            return result;
        }
    }
}
