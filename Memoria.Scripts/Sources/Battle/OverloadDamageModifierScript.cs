using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Assets;
using Memoria.Data;
using Memoria.Database;
using Memoria.Prime;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using static UIManager;

namespace Memoria.Scripts.Battle
{
    public class OverloadDamageModifierScript : IOverloadDamageModifierScript
    {
        public void OnDamageModifierChange(BattleCalculator v, Int32 previousValue, Int32 bonus)
        {
        }

        public void OnDamageDrasticReduction(BattleCalculator v)
        {
            v.Context.Attack = 1;
        }

        public void OnDamageFinalChanges(BattleCalculator v)
        {
            if (v.Caster.IsPlayer && false)
                v.Target.HpDamage = 9999;

            uint HPMob = v.Target.CurrentHp;
            SB2_PATTERN sb2Pattern = FF9StateSystem.Battle.FF9Battle.btl_scene.PatAddr[FF9StateSystem.Battle.FF9Battle.btl_scene.PatNum];
            if (!v.Target.IsPlayer)
            {
                HPMob = btl_para.IsNonDyingVanillaBoss(v.Target) ? (v.Target.CurrentHp - 10000) : v.Target.CurrentHp;
                int MonsterId = (int)(10100 + v.Target.Data.bi.line_no);
                if (v.Target.MaximumHp == UInt16.MaxValue)
                {
                    if (!FF9StateSystem.EventState.gScriptDictionary.TryGetValue(MonsterId, out Dictionary<Int32, Int32> dictMonster))
                    {
                        BTL_SCENE scene = new BTL_SCENE();
                        scene.ReadBattleScene(FF9StateSystem.Battle.FF9Battle.btl_scene.nameIdentifier.Replace("BSC_", ""));
                        SB2_MON_PARM monsterParam = scene.MonAddr[sb2Pattern.Monster[v.Target.Data.bi.slot_no].TypeNo];
                        dictMonster = new Dictionary<Int32, Int32>();
                        dictMonster[0] = (int)monsterParam.MaxHP; // True Maximum HP
                        dictMonster[1] = (int)monsterParam.MaxHP; // True Current HP
                        FF9StateSystem.EventState.gScriptDictionary.Add(MonsterId, dictMonster);
                    }
                    HPMob = (uint)dictMonster[1];
                    dictMonster[1] = (v.Target.Flags & CalcFlag.HpRecovery) != 0 ? Math.Min(dictMonster[0], dictMonster[1] + v.Target.HpDamage) : Math.Max(0, dictMonster[1] - v.Target.HpDamage);
                }
            }
            if (v.Caster.IsPlayer && (!v.Target.IsPlayer && HPMob <= v.Target.HpDamage && (v.Target.Flags & CalcFlag.HpRecovery) == 0) || (v.Target.IsPlayer && (v.Target.Flags & CalcFlag.HpRecovery) != 0 && v.Target.Data != v.Caster.Data && v.Caster.IsNonMorphedPlayer))
            {
                //Transform builtInBone = v.Caster.Data.gameObject.transform.GetChildByName($"bone{v.Caster.Data.weapon_bone:D3}");
                //Log.Message("v.Caster.Data.weapon_bone = " + v.Caster.Data.weapon_bone);
                //Log.Message("builtInBone.localScale = " + builtInBone.localScale);

                int CustomCharacterId = (int)(10000 + v.Caster.PlayerIndex);
                if (FF9StateSystem.EventState.gScriptDictionary.TryGetValue(CustomCharacterId, out Dictionary<Int32, Int32> dict))
                {
                    FF9BattleDB.SceneData.TryGetKey(FF9StateSystem.Battle.battleMapIndex, out string btlName);
                    btlName = FF9StateSystem.Battle.FF9Battle.btl_scene.nameIdentifier.Replace("BSC_", "");
                    CharacterPresetId presetId = v.Caster.Player.PresetId;
                    v.Caster.AddDelayedModifier(
                        caster => caster.CurrentAtb >= caster.MaximumAtb,
                        caster =>
                        {
                            BattleCommandId CMDMonster = (BattleCommandId)(5000 + v.Caster.Data.bi.line_no);
                            v.Caster.ChangeToMonster(btlName, sb2Pattern.Monster[v.Target.Data.bi.slot_no].TypeNo, CharacterCommands.CommandSets[presetId].Regular[2], CMDMonster, false, true, true, true, true, AADescription: true, updateStatus:true);
                            CharacterCommands.CommandSets[presetId].Regular[3] = BattleCommandId.None;
                            if ((btl_para.IsNonDyingVanillaBoss(v.Target) || v.Caster.Data.dms_geo_id == 347 || v.Caster.Data.dms_geo_id == 125) && v.Caster.Data.dms_geo_id != 349 && v.Caster.Data.dms_geo_id != 300 && v.Caster.Data.dms_geo_id != 302)
                            {
                                v.Caster.MaximumHp -= 10000;
                                v.Caster.CurrentHp = Math.Max(1, v.Target.MaximumHp);
                                v.Caster.CurrentMp = Math.Max(1, v.Target.MaximumMp);
                                dict[2] = 1;
                            }
                            else
                            {
                                v.Caster.CurrentHp = v.Target.MaximumHp;
                                v.Caster.CurrentMp = v.Target.MaximumMp;
                                dict[2] = 0;
                            }
                            dict[0] = FF9StateSystem.Battle.battleMapIndex;
                            if (v.Caster.Data.dms_geo_id == 339)
                                dict[1] = 0;
                            else
                                dict[1] = sb2Pattern.Monster[v.Target.Data.bi.slot_no].TypeNo;
                            Dictionary<Int32, String> fieldText = new Dictionary<Int32, String>();
                            String path = EmbadedTextResources.GetCurrentPath("/Battle/" + dict[0] + ".mes");
                            FF9TextTool.ImportStrtWithCumulativeModFiles<Int32>(path, fieldText);
                            caster.Player.Name = Regex.Replace(fieldText[dict[1]], @"\[.*?\]", "");
                            if (true) // TO DELETE ! (Change Level)
                            {
                                BTL_SCENE scene = new BTL_SCENE();
                                scene.ReadBattleScene(btlName);
                                if (scene.header.TypCount <= 0)
                                    return;
                                if (dict[1] < 0)
                                    dict[1] = Comn.random8() % scene.header.TypCount;
                                if (dict[1] >= scene.header.TypCount)
                                    return;
                                SB2_MON_PARM monsterParam = scene.MonAddr[dict[1]];
                                v.Caster.Level = monsterParam.Level;
                                ff9play.FF9Play_ChangeLevel(v.Caster.Player, v.Caster.Level, true);
                            }
                            dict[3] = (int)v.Caster.MaximumHp; // MaxHP
                            dict[4] = (int)v.Caster.MaximumMp; // MaxMP
                            dict[5] = v.Caster.Level; // Level
                            dict[6] = v.Caster.Dexterity; // Speed
                            dict[7] = v.Caster.Strength; // Strength
                            dict[8] = v.Caster.Magic; // Magic
                            dict[9] = v.Caster.Will; // Spirit
                            dict[10] = v.Caster.PhysicalDefence; // PDefence
                            dict[11] = v.Caster.PhysicalEvade; // PEvade
                            dict[12] = v.Caster.MagicDefence; // MDefence
                            dict[13] = v.Caster.MagicEvade; // MEvade
                        }
                    );
                }
            }

            if (v.Target.Flags == 0)
                return;
            Int32 reflectMultiplier = v.Command.GetReflectMultiplierOnTarget(v.Target.Id);
            if ((v.Target.Flags & CalcFlag.HpAlteration) != 0)
                v.Target.HpDamage *= reflectMultiplier;
            if ((v.Target.Flags & CalcFlag.MpAlteration) != 0)
                v.Target.MpDamage *= reflectMultiplier;
        }
    }
}
