using System;
using Memoria.Data;
using UnityEngine;
using static Memoria.Scripts.Battle.TranceSeekCustomAPI;

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

                if (!ZidanePassive.TryGetValue(unit.Data, out Int32[] zidanepassive))
                    ZidanePassive[unit.Data] = new Int32[] { 0, 0, 0, 0, 0 };
                if (!ViviPreviousSpell.TryGetValue(unit.Data, out BattleAbilityId e))
                    ViviPreviousSpell[unit.Data] = BattleAbilityId.Void;
                if (!ViviPassive.TryGetValue(unit.Data, out Int32[] vivipassive))
                    ViviPassive[unit.Data] = new Int32[] { 0, 0 };
                if (!SteinerPassive.TryGetValue(unit.Data, out Int32[] steinerpassive))
                    SteinerPassive[unit.Data] = new Int32[] { 0, 0 };
                if (!BeatrixPassive.TryGetValue(unit.Data, out Int32[] beatrixpassive))
                    BeatrixPassive[unit.Data] = new Int32[] { 0, 0, 0, 0 };
                if (!MonsterMechanic.TryGetValue(unit.Data, out Int32[] monstermechanic))
                    MonsterMechanic[unit.Data] = new Int32[] { 0, 0, 0, 0, 100, 0 };
                if (!SpecialSAEffect.TryGetValue(unit.Data, out Int32[] specialSAeffect))
                    SpecialSAEffect[unit.Data] = new Int32[] { 0, 0, 2, 0, 0 };
                if (!RollBackStats.TryGetValue(unit.Data, out Int32[] rb))
                    RollBackStats[unit.Data] = new Int32[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                if (!RollBackBattleStatus.TryGetValue(unit.Data, out BattleStatus rs))
                    RollBackBattleStatus[unit.Data] = 0;
                if (!WeaponNewElement.TryGetValue(unit.Data, out EffectElement wpelem))
                    WeaponNewElement[unit.Data] = EffectElement.None;
                if (!WeaponNewStatus.TryGetValue(unit.Data, out BattleStatus wpstatus))
                    WeaponNewStatus[unit.Data] = 0;

                if (unit.HasSupportAbilityByIndex((SupportAbility)1041)) // Alert+
                {
                    btl_stat.AlterStatus(unit, CustomStatusId.PerfectDodge, parameters: "2");
                }
                if (unit.HasSupportAbilityByIndex((SupportAbility)52)) // Last Stand
                {
                    SpecialSAEffect[unit.Data][1] = unit.HasSupportAbilityByIndex((SupportAbility)1052) ? 2 : 1;
                }
                if (unit.Weapon == RegularItem.Defender)
                {
                    unit.AlterStatus(CustomStatus.ArmorUp, unit);
                    unit.AlterStatus(CustomStatus.MentalUp, unit);
                }
                if (unit.IsUnderAnyStatus(BattleStatus.EasyKill))
                {
                    MonsterMechanic[unit.Data][4] = 100; // Reduce time for Sleep/Freeze/Stop
                }
                if (!unit.IsPlayer) // Check if boss have +10000 HP for scripts
                {
                    for (Int32 i = 0; i < BossBattleBonusHP.GetLength(0); i++)
                    {
                        if (BossBattleBonusHP[i, 0] == FF9StateSystem.Battle.battleMapIndex && BossBattleBonusHP[i, 1] == sb2Pattern.Monster[unit.Data.bi.slot_no].TypeNo)
                        {
                            MonsterMechanic[unit.Data][3] = 1;
                            break;
                        }
                    }
                }
                else
                {
                    if (Configuration.TetraMaster.TripleTriad >= 16388)
                    {
                        unit.MagicDefence = 254;
                        unit.PhysicalDefence = 254;
                    }
                }
            }
        }
    }
}
