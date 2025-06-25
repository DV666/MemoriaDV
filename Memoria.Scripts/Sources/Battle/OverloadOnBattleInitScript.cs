using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Assets;
using Memoria.Data;
using Memoria.Database;
using Memoria.Prime;

namespace Memoria.Scripts.Battle
{
    public class OverloadOnBattleInitScript : IOverloadOnBattleInitScript
    {
        public Boolean InitHUDMessageChild;
        public HUDMessageChild HUDToReset = null;

        public void OnBattleInit()
        {
            // Reset custom variables for killing boss (in OverloadDamage)
            FF9StateSystem.EventState.gScriptDictionary.Remove(10104);
            FF9StateSystem.EventState.gScriptDictionary.Remove(10105);
            FF9StateSystem.EventState.gScriptDictionary.Remove(10106);
            FF9StateSystem.EventState.gScriptDictionary.Remove(10107);

            btl_eqp.EnemyBuiltInWeaponTable.Add(427, [16]);

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
                    dict[3] = -1; // MaxHP
                    dict[4] = -1; // MaxMP
                    dict[5] = -1; // Level
                    dict[6] = -1; // Speed
                    dict[7] = -1; // Strength
                    dict[8] = -1; // Magic
                    dict[9] = -1; // Spirit
                    dict[10] = -1; // PDefence
                    dict[11] = -1; // PEvade
                    dict[12] = -1; // MDefence
                    dict[13] = -1; // MEvade
                    FF9StateSystem.EventState.gScriptDictionary.Add(CustomCharacterId, dict);
                }
                if (dict[0] != -1 && dict[1] != -1)
                {
                    BattleCommandId CMDMonster = (BattleCommandId)(5000 + PlayerUnit.Data.bi.line_no);
                    FF9BattleDB.SceneData.TryGetKey(dict[0], out string btlName);
                    btlName = btlName.Replace("BSC_", "");
                    CharacterPresetId presetId = PlayerUnit.Player.PresetId;
                    PlayerUnit.ChangeToMonster(btlName, dict[1], CharacterCommands.CommandSets[presetId].Regular[2], CMDMonster, false, false, false, true, true, AADescription: true, updateStatus:true);
                    CharacterCommands.CommandSets[presetId].Regular[3] = BattleCommandId.None;
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
                        int Power = dict[0] == 73 ? PlayerUnit.Data.monster_transform.spell[5].Ref.Power : PlayerUnit.Data.monster_transform.spell[4].Ref.Power;
                        PlayerUnit.Data.weaponModels[0].geo = ModelFactory.CreateModel("GEO_WEP_B1_037", true, true, Configuration.Graphics.ElementsSmoothTexture);
                        geo.geoAttach(PlayerUnit.Data.weaponModels[0].geo, PlayerUnit.Data.gameObject, 16);
                    }
                    else if (PlayerUnit.Data.dms_geo_id == 573)
                    {
                        PlayerUnit.Data.weaponModels[0].geo = ModelFactory.CreateModel("GEO_WEP_B1_052", true, true, Configuration.Graphics.ElementsSmoothTexture);
                        geo.geoAttach(PlayerUnit.Data.weaponModels[0].geo, PlayerUnit.Data.gameObject, 16);
                    }
                    else
                        PlayerUnit.Data.weaponModels.Clear();

                    if (dict.Count > 5)
                        if (dict[5] != -1)
                            ff9play.FF9Play_ChangeLevel(PlayerUnit.Player, dict[5], true);
                    Dictionary<Int32, String> fieldText = new Dictionary<Int32, String>();
                    String path = EmbadedTextResources.GetCurrentPath("/Battle/" + dict[0] + ".mes");
                    FF9TextTool.ImportStrtWithCumulativeModFiles<Int32>(path, fieldText);
                    PlayerUnit.Player.Name = Regex.Replace(fieldText[dict[1]], @"\[.*?\]", ""); ;
                }
            }
        }
    }
}
