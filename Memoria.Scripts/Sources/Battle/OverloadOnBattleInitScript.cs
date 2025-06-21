using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Assets;
using Memoria.Data;
using Memoria.Database;
using Memoria.Prime;
using static BTL_DATA;
using static UIManager;

namespace Memoria.Scripts.Battle
{
    public class OverloadOnBattleInitScript : IOverloadOnBattleInitScript
    {
        public Boolean InitHUDMessageChild;
        public HUDMessageChild HUDToReset = null;

        public void OnBattleInit()
        {
            // Reset
            FF9StateSystem.EventState.gScriptDictionary.Remove(10104);
            FF9StateSystem.EventState.gScriptDictionary.Remove(10105);
            FF9StateSystem.EventState.gScriptDictionary.Remove(10106);
            FF9StateSystem.EventState.gScriptDictionary.Remove(10107);

            foreach (BattleUnit PlayerUnit in BattleState.EnumerateUnits())
            {
                if (!PlayerUnit.IsPlayer)
                    continue;

                int CustomCharacterId = (int)(10000 + PlayerUnit.PlayerIndex);
                if (!FF9StateSystem.EventState.gScriptDictionary.TryGetValue(CustomCharacterId, out Dictionary<Int32, Int32> dict))
                {
                    dict = new Dictionary<Int32, Int32>();
                    dict[0] = -1; // ID Battle (for Scene) 
                    dict[1] = -1; // ID Monster 
                    dict[2] = -1; // Boss ?
                    FF9StateSystem.EventState.gScriptDictionary.Add(CustomCharacterId, dict);
                }
                if (dict[0] != -1 && dict[1] != -1)
                {
                    BattleCommandId CMDMonster = (BattleCommandId)(5000 + PlayerUnit.Data.bi.line_no);
                    FF9BattleDB.SceneData.TryGetKey(dict[0], out string btlName);
                    btlName = btlName.Replace("BSC_", "");
                    CharacterPresetId presetId = PlayerUnit.Player.PresetId;
                    PlayerUnit.ChangeToMonster(btlName, dict[1], CharacterCommands.CommandSets[presetId].Regular[2], CMDMonster, false, true, true, true, true, AADescription: true, updateStatus:true);
                    CharacterCommands.CommandSets[presetId].Regular[3] = BattleCommandId.None;
                    if (dict[2] > 0 || PlayerUnit.Data.dms_geo_id == 347 || PlayerUnit.Data.dms_geo_id == 125)
                    {
                        PlayerUnit.MaximumHp -= 10000;
                        PlayerUnit.CurrentHp = Math.Max(1, PlayerUnit.MaximumHp);
                        PlayerUnit.CurrentMp = Math.Max(1, PlayerUnit.MaximumMp);
                    }
                    else
                    {
                        PlayerUnit.CurrentHp = PlayerUnit.MaximumHp;
                        PlayerUnit.CurrentMp = PlayerUnit.MaximumMp;
                    }
                    if (PlayerUnit.Data.dms_geo_id == 427) // Beatrix (Stock Break & ClimHazard nerfed)
                    {
                        if (dict[0] == 4)
                        {
                            PlayerUnit.Data.monster_transform.spell[4].Ref.ScriptId = 100;
                            PlayerUnit.Data.monster_transform.spell[4].Ref.Power = 28;
                            PlayerUnit.Data.monster_transform.spell[4].Ref.Rate = 0;
                        }
                        else if (dict[0] == 299)
                        {
                            PlayerUnit.Data.monster_transform.spell[4].Ref.ScriptId = 100;
                            PlayerUnit.Data.monster_transform.spell[4].Ref.Power = 47;
                            PlayerUnit.Data.monster_transform.spell[4].Ref.Rate = 0;
                        }
                        else if (dict[0] == 73)
                        {
                            PlayerUnit.Data.monster_transform.spell[5].Ref.ScriptId = 100;
                            PlayerUnit.Data.monster_transform.spell[5].Ref.Power = 50;
                            PlayerUnit.Data.monster_transform.spell[5].Ref.Rate = 0;
                        }
                        string Description = "";
                        int Power = dict[0] == 73 ? PlayerUnit.Data.monster_transform.spell[5].Ref.Power : PlayerUnit.Data.monster_transform.spell[4].Ref.Power;
                        Description += "[ICON=95] ";
                        Description += $"{Localization.Get("AttackStats")} : {Power}";
                        PlayerUnit.Data.monster_transform.spell_desc[4] = Description;
                    }
                    Dictionary<Int32, String> fieldText = new Dictionary<Int32, String>();
                    String path = EmbadedTextResources.GetCurrentPath("/Battle/" + dict[0] + ".mes");
                    FF9TextTool.ImportStrtWithCumulativeModFiles<Int32>(path, fieldText);
                    PlayerUnit.Player.Name = Regex.Replace(fieldText[dict[1]], @"\[.*?\]", ""); ;
                }
            }
        }
    }
}
