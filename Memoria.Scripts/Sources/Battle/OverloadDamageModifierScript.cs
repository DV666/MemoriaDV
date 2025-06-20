using Memoria.Data;
using Memoria.Database;
using System;
using System.Collections.Generic;

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
            uint HPMob = btl_para.IsNonDyingVanillaBoss(v.Target) ? (v.Target.CurrentHp - 10000) : v.Target.CurrentHp;
            if (v.Caster.IsPlayer && HPMob < v.Target.HpDamage && !v.Target.IsPlayer)
            {
                int CustomCharacterId = (int)(10000 + v.Caster.Player.PresetId);
                if (FF9StateSystem.EventState.gScriptDictionary.TryGetValue(CustomCharacterId, out Dictionary<Int32, Int32> dict))
                {
                    FF9BattleDB.SceneData.TryGetKey(FF9StateSystem.Battle.battleMapIndex, out string btlName);
                    SB2_PATTERN sb2Pattern = FF9StateSystem.Battle.FF9Battle.btl_scene.PatAddr[FF9StateSystem.Battle.FF9Battle.btl_scene.PatNum];
                    btlName = FF9StateSystem.Battle.FF9Battle.btl_scene.nameIdentifier.Replace("BSC_", "");
                    CharacterPresetId presetId = v.Caster.Player.PresetId;
                    v.Caster.AddDelayedModifier(
                        caster => caster.CurrentAtb >= caster.MaximumAtb,
                        caster =>
                        {
                            v.Caster.ChangeToMonster(btlName, sb2Pattern.Monster[v.Target.Data.bi.slot_no].TypeNo, CharacterCommands.CommandSets[presetId].Regular[2], (BattleCommandId)1111, false, true, true, true, true);
                            CharacterCommands.CommandSets[presetId].Regular[3] = BattleCommandId.None;
                            if (btl_para.IsNonDyingVanillaBoss(v.Target))
                            {
                                v.Caster.MaximumHp -= 10000;
                                v.Caster.MaximumMp -= 10000;
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
                            dict[1] = sb2Pattern.Monster[v.Target.Data.bi.slot_no].TypeNo;
                        }
                    );
                }
            }
        }
    }
}
