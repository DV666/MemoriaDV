using System;
using Memoria.Data;
using UnityEngine;

namespace Memoria.Scripts.Battle
{
    public class OverloadOnBattleInitScript : IOverloadOnBattleInitScript
    {
        public Boolean InitHUDMessageChild;
        public HUDMessageChild HUDToReset = null;

        public void OnBattleInit()
        {
            foreach (BattleUnit unit in BattleState.EnumerateUnits())
            {
                if (!InitHUDMessageChild) // Just to reset FontSize (didn't adjust after soft reset)
                {
                    btl2d.GetIconPosition(unit.Data, btl2d.ICON_POS_DEFAULT, out Transform attachTransf, out Vector3 iconOff);
                    HUDToReset = Singleton<HUDMessage>.Instance.Show(attachTransf, "", HUDMessage.MessageStyle.DEATH_SENTENCE, Vector3.zero, 0);
                    HUDToReset.FontSize = 36;
                    btl2d.StatusMessages.Add(HUDToReset);
                    btl2d.StatusMessages.Remove(HUDToReset);
                    Singleton<HUDMessage>.Instance.ReleaseObject(HUDToReset);
                    InitHUDMessageChild = true;
                }

                SB2_PATTERN sb2Pattern = FF9StateSystem.Battle.FF9Battle.btl_scene.PatAddr[FF9StateSystem.Battle.FF9Battle.btl_scene.PatNum];
                foreach (BattleUnit PlayerUnit in BattleState.EnumerateUnits())
                {
                    if (!PlayerUnit.IsPlayer)
                        continue;

                    if (PlayerUnit.HasSupportAbilityByIndex((SupportAbility)1045)) // Pluriche+
                    {
                        foreach (BattleUnit monster in BattleState.EnumerateUnits())
                        {
                            if (!monster.IsPlayer)
                            {
                                BattleEnemy battleEnemy = BattleEnemy.Find(monster);
                                battleEnemy.Data.bonus_item_rate[3] = 16;
                                battleEnemy.Data.bonus_item_rate[2] = 96;
                                battleEnemy.Data.bonus_item_rate[1] = 192;
                            }
                        }
                        break;
                    }
                }

                if (!TranceSeekCustomAPI.ZidanePassive.TryGetValue(unit.Data, out Int32[] zidanepassive))
                    TranceSeekCustomAPI.ZidanePassive[unit.Data] = new Int32[] { 0, 0, 0, 0, 0 };
                if (!TranceSeekCustomAPI.ViviPreviousSpell.TryGetValue(unit.Data, out BattleAbilityId e))
                    TranceSeekCustomAPI.ViviPreviousSpell[unit.Data] = BattleAbilityId.Void;
                if (!TranceSeekCustomAPI.ViviPassive.TryGetValue(unit.Data, out Int32[] vivipassive))
                    TranceSeekCustomAPI.ViviPassive[unit.Data] = new Int32[] { 0, 0 };
                if (!TranceSeekCustomAPI.SteinerPassive.TryGetValue(unit.Data, out Int32[] steinerpassive))
                    TranceSeekCustomAPI.SteinerPassive[unit.Data] = new Int32[] { 0, 0 };
                if (!TranceSeekCustomAPI.BeatrixPassive.TryGetValue(unit.Data, out Int32[] beatrixpassive))
                    TranceSeekCustomAPI.BeatrixPassive[unit.Data] = new Int32[] { 0, 0, 0, 0 };
                if (!TranceSeekCustomAPI.MonsterMechanic.TryGetValue(unit.Data, out Int32[] monstermechanic))
                    TranceSeekCustomAPI.MonsterMechanic[unit.Data] = new Int32[] { 0, 0, 0, 0, 100, 0 };
                if (!TranceSeekCustomAPI.SpecialSAEffect.TryGetValue(unit.Data, out Int32[] specialSAeffect))
                    TranceSeekCustomAPI.SpecialSAEffect[unit.Data] = new Int32[] { 0, 0, 2, 0, 0 };
                if (!TranceSeekCustomAPI.RollBackStats.TryGetValue(unit.Data, out Int32[] rb))
                    TranceSeekCustomAPI.RollBackStats[unit.Data] = new Int32[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                if (!TranceSeekCustomAPI.RollBackBattleStatus.TryGetValue(unit.Data, out BattleStatus rs))
                    TranceSeekCustomAPI.RollBackBattleStatus[unit.Data] = 0;
                if (!TranceSeekCustomAPI.StatusBeforeScript.TryGetValue(unit.Data, out BattleStatus sbs))
                    TranceSeekCustomAPI.StatusBeforeScript[unit.Data] = 0;

                if (unit.HasSupportAbilityByIndex((SupportAbility)1041)) // Alert+
                {
                    //PerfectBonus[unit.Data][0] += 2;
                }
                if (unit.HasSupportAbilityByIndex((SupportAbility)52)) // Last Stand
                {
                    TranceSeekCustomAPI.SpecialSAEffect[unit.Data][1] = unit.HasSupportAbilityByIndex((SupportAbility)1052) ? 2 : 1;
                }

                if (!unit.IsPlayer) // Check if boss have +10000 HP for scripts
                {
                    for (Int32 i = 0; i < TranceSeekCustomAPI.BossBattleBonusHP.GetLength(0); i++)
                    {
                        if (TranceSeekCustomAPI.BossBattleBonusHP[i, 0] == FF9StateSystem.Battle.battleMapIndex && TranceSeekCustomAPI.BossBattleBonusHP[i, 1] == sb2Pattern.Monster[unit.Data.bi.slot_no].TypeNo)
                        {
                            TranceSeekCustomAPI.MonsterMechanic[unit.Data][3] = 1;
                            break;
                        }
                    }
                }

                if (!unit.IsPlayer && unit.IsUnderAnyStatus(BattleStatus.Trance) && TranceSeekCustomAPI.MonsterMechanic[unit.Data][0] == 0 && !unit.IsUnderAnyStatus(BattleStatus.EasyKill)) // +50% HP/MP Max if monster get under Trance
                { // [!!!TODO!!!] Move to end method
                    TranceSeekCustomAPI.MonsterMechanic[unit.Data][0] = 1;
                    unit.MaximumHp += (unit.MaximumHp / 2);
                    unit.MaximumMp += (unit.MaximumMp / 2);
                    unit.CurrentHp = unit.MaximumHp;
                }
            }
        }
    }
}
