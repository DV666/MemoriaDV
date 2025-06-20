using System;
using System.Collections.Generic;
using Memoria.Data;
using Memoria.Database;

namespace Memoria.Scripts.Battle
{
    public class OverloadOnBattleInitScript : IOverloadOnBattleInitScript
    {
        public Boolean InitHUDMessageChild;
        public HUDMessageChild HUDToReset = null;

        public void OnBattleInit()
        {
            foreach (BattleUnit PlayerUnit in BattleState.EnumerateUnits())
            {
                if (!PlayerUnit.IsPlayer)
                    continue;

                int CustomCharacterId = (int)(10000 + PlayerUnit.Player.PresetId);
                if (!FF9StateSystem.EventState.gScriptDictionary.TryGetValue(CustomCharacterId, out Dictionary<Int32, Int32> dict))
                {
                    dict = new Dictionary<Int32, Int32>();
                    dict[0] = -1; // Scene
                    dict[1] = -1; // ID Monster 
                    dict[2] = -1; // Boss ?
                    FF9StateSystem.EventState.gScriptDictionary.Add(CustomCharacterId, dict);
                }
                if (dict[0] != -1 && dict[1] != -1)
                {
                    FF9BattleDB.SceneData.TryGetKey(dict[0], out string btlName);
                    btlName = btlName.Replace("BSC_", "");
                    CharacterPresetId presetId = PlayerUnit.Player.PresetId;
                    PlayerUnit.ChangeToMonster(btlName, dict[1], CharacterCommands.CommandSets[presetId].Regular[2], (BattleCommandId)1111, false, true, true, true, true);
                    CharacterCommands.CommandSets[presetId].Regular[3] = BattleCommandId.None;
                    if (dict[2] > 0)
                    {
                        PlayerUnit.MaximumHp -= 10000;
                        PlayerUnit.MaximumMp -= 10000;
                        PlayerUnit.CurrentHp = Math.Max(1, PlayerUnit.MaximumHp);
                        PlayerUnit.CurrentMp = Math.Max(1, PlayerUnit.MaximumMp);
                    }
                    else
                    {
                        PlayerUnit.CurrentHp = PlayerUnit.MaximumHp;
                        PlayerUnit.CurrentMp = PlayerUnit.MaximumMp;
                    }
                }
            }

        }
    }
}
