﻿using System;
using System.Collections.Generic;
using System.Threading;
using Memoria.Data;
using Memoria.DefaultScripts;
using Memoria.Prime;
using static Memoria.Scripts.Battle.TranceSeekCustomAPI;

namespace Memoria.Scripts.Battle
{
    public class OverloadOnBattleInitScript : IOverloadOnBattleInitScript
    {
        public Boolean InitHUDMessageChild;
        public HUDMessageChild HUDToReset = null;

        public void OnBattleInit()
        {
            SB2_PATTERN sb2Pattern = FF9StateSystem.Battle.FF9Battle.btl_scene.PatAddr[FF9StateSystem.Battle.FF9Battle.btl_scene.PatNum];

            if (FF9StateSystem.Battle.battleMapIndex == 93 && FF9StateSystem.Battle.FF9Battle.btl_scene.PatNum == 3) // Prison Cage + Little Girl
                HonoluluBattleMain.SetupAttachModel(FF9StateSystem.Battle.FF9Battle.btl_data[4], FF9StateSystem.Battle.FF9Battle.btl_data[5], 55, 25);

            if (!InitHUDMessageChild)
            {
                foreach (HUDMessageChild HUDToReset in Singleton<HUDMessage>.Instance.AllMessagePool)
                    HUDToReset.FontSize = 36;
                OverlapSHP.ClearInBattleInit();
                InitHUDMessageChild = true;
            }

            for (Int32 i = 0; i < 8; i++)
            {
                int idAA = 1136 + i;
                FF9StateSystem.Battle.FF9Battle.aa_data[(BattleAbilityId)idAA].MP = 0;
            }

            foreach (BattleUnit unit in BattleState.EnumerateUnits())
            {
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
                    ZidanePassive[unit.Data] = new Int32[] { 0, 0, 0, 0, 0, 255, 255, 0, 0 };
                if (!ViviPreviousSpell.TryGetValue(unit.Data, out BattleAbilityId e))
                    ViviPreviousSpell[unit.Data] = BattleAbilityId.Void;
                if (!ViviPassive.TryGetValue(unit.Data, out Int32[] vivipassive))
                    ViviPassive[unit.Data] = new Int32[] { 0, 0, 0 };
                if (!BeatrixPassive.TryGetValue(unit.Data, out Int32[] beatrixpassive))
                    BeatrixPassive[unit.Data] = new Int32[] { 0, 0, 0, 0 };
                if (!ProtectStatus.TryGetValue(unit.Data, out Dictionary<BattleStatus, Int32> statusprotect))
                    ProtectStatus[unit.Data] = new Dictionary<BattleStatus, Int32> { { 0, 0 } };
                if (!StackBreakOrUpStatus.TryGetValue(unit.Data, out Int32[] stackstatus))
                    StackBreakOrUpStatus[unit.Data] = new Int32[] { 0, 0, 0, 0 };
                if (!MonsterMechanic.TryGetValue(unit.Data, out Int32[] monstermechanic))
                    MonsterMechanic[unit.Data] = new Int32[] { 0, 0, 0, 0, 100, 0 };
                if (!SpecialSAEffect.TryGetValue(unit.Data, out Int32[] specialSAeffect))
                    SpecialSAEffect[unit.Data] = new Int32[] { 0, 0, 2, 0, 0, 0, 0, 0, 0, 0 };
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
                    btl_stat.AlterStatus(unit, CustomStatusId.PerfectDodge, parameters: "+2");
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
                if (unit.Armor == (RegularItem)1220) // Mechanical Armor
                {
                    MonsterMechanic[unit.Data][1] = 10;
                    btl_stat.AlterStatus(unit, CustomStatusId.MechanicalArmor, parameters: MonsterMechanic[unit.Data][1]);
                }
                if (unit.IsUnderAnyStatus(BattleStatus.EasyKill))
                {
                    MonsterMechanic[unit.Data][4] = 100; // Reduce time for Sleep/Freeze/Stop
                    MonsterMechanic[unit.Data][5] = 4; // Reduce gravity damage
                }
                if (!unit.IsPlayer) // Check if boss have +10000 HP for scripts
                {
                    Boolean ChangeHP = false;
                    for (Int32 i = 0; i < BossBattleBonusHP.GetLength(0); i++)
                    {
                        if (FF9StateSystem.Battle.battleMapIndex == BossBattleBonusHP[i, 0] && sb2Pattern.Monster[unit.Data.bi.slot_no].TypeNo == BossBattleBonusHP[i, 1])
                        {
                            if (FF9StateSystem.EventState.gEventGlobal[1403] == 3) // Kuja mode
                            {
                                ChangeHP = true;
                                uint bonusHP = unit.MaximumHp - 10000;
                                unit.MaximumHp += (bonusHP / 10);
                                unit.CurrentHp += (bonusHP / 10);
                                unit.Strength = (byte)Math.Min(unit.Strength + (unit.Strength / 4), byte.MaxValue);
                                unit.Magic = (byte)Math.Min(unit.Magic + (unit.Magic / 4), byte.MaxValue);
                            }
                            else if (FF9StateSystem.EventState.gEventGlobal[1403] == 4) // Necron mode
                            {
                                ChangeHP = true;
                                uint bonusHP = unit.MaximumHp - 10000;
                                unit.MaximumHp += (bonusHP / 4);
                                unit.CurrentHp += (bonusHP / 4);
                                unit.Strength = (byte)Math.Min(unit.Strength + (unit.Strength / 2), byte.MaxValue);
                                unit.Magic = (byte)Math.Min(unit.Magic + (unit.Magic / 2), byte.MaxValue);
                            }
                            MonsterMechanic[unit.Data][3] = 1;
                            break;
                        }
                    }
                    BattleEnemy battleEnemy = BattleEnemy.Find(unit);
                    if (!ChangeHP)
                    {
                        if (FF9StateSystem.EventState.gEventGlobal[1403] == 3) // Kuja mode
                        {
                            uint bonusHP = unit.MaximumHp;
                            unit.MaximumHp += (bonusHP / 10);
                            unit.CurrentHp += (bonusHP / 10);
                            unit.Strength = (byte)Math.Min(unit.Strength + (unit.Strength / 4), byte.MaxValue);
                            unit.Magic = (byte)Math.Min(unit.Magic + (unit.Magic / 4), byte.MaxValue);
                        }
                        else if (FF9StateSystem.EventState.gEventGlobal[1403] == 4) // Necron mode
                        {
                            uint bonusHP = unit.MaximumHp;
                            unit.MaximumHp += (bonusHP / 4);
                            unit.CurrentHp += (bonusHP / 4);
                            unit.Strength = (byte)Math.Min(unit.Strength + (unit.Strength / 2), byte.MaxValue);
                            unit.Magic = (byte)Math.Min(unit.Magic + (unit.Magic / 2), byte.MaxValue);
                            battleEnemy.Data.bonus_exp /= 2;
                            battleEnemy.Data.bonus_gil /= 10;
                        }
                    }
                    if (FF9StateSystem.EventState.gEventGlobal[1403] == 1) // Vivi mode
                    {
                        battleEnemy.Data.bonus_exp += (battleEnemy.Data.bonus_exp / 4);
                        battleEnemy.Data.bonus_gil += (battleEnemy.Data.bonus_gil / 4);
                    }
                }
                else
                {
                    if (Configuration.TetraMaster.TripleTriad >= 16388)
                    {
                        unit.MagicDefence = 254;
                        unit.PhysicalDefence = 254;
                    }
                    if (FF9StateSystem.EventState.ScenarioCounter >= 11100 && FF9StateSystem.EventState.gEventGlobal[1500] == 0)
                    {
                        unit.AlterStatus(BattleStatus.Death, unit);
                        unit.CurrentHp = 0;
                    }
                }
            }
        }

        public static Int32[,] BossBattleBonusHP = new Int32[,]
        {
            { 73, 0 }, // Beatrix 3rd
		    { 299, 0 }, // Beatrix 2nd
		    { 4, 0 }, // Beatrix 1st
            { 4, 1 }, // Dark Beatrix
            { 295, 0 }, // Bach
		    { 115, 1 },	// Kuja 1st (Ifa)
		    { 920, 0 }, { 921, 0 }, // Friendly Belhamel
            { 938, 0 }, // Necron
            { 251, 0 }, { 363, 0 }, { 364, 0 }, { 838, 0 }, // Friendly Eskuriax
            { 192, 0 }, { 193, 0 }, { 196, 0 }, { 197, 0 }, { 199, 0 }, // Friendly Fantôme
            { 300, 0 }, // Fourmillion
            { 2, 2 }, // Gardienne du feu
            { 107, 0 }, // Gargantua (1st)
            { 890, 0 }, // Garland
            { 2, 2 }, { 2, 3 }, // Wind Guardian + Fire Guardian
            { 326, 0 }, // Gisamark
            { 723, 0 }, // Friendly Garuda
            { 57, 0 }, // Ozma
            { 211, 0 }, // Ozma
            { 365, 0 }, { 367, 0 }, { 368, 0 }, { 595, 0 }, { 605, 0 }, { 606, 0 }, // Friendly Jabah
            { 891, 0 }, { 891, 1 },  // Kuja + Trance Kuja (fin CD3)
            { 937, 0 }, // Trance Kuja 2nd (Crystal World)
            { 330, 0 }, // Kwell
            { 76, 0 }, // Larvalar Junior
            { 132, 0 }, // Amarant Enemy
            { 631, 0 }, { 632, 0 },  // Friendly Manta
            { 336, 0 }, // Maskedefer
            { 302, 0 }, // Maton + Grenat
            { 301, 0 }, // Maton + Vivi
            { 682, 0 }, { 686, 0 }, { 687, 0 }, { 689, 0 }, { 270, 0 }, { 235, 0 }, { 841, 0 }, { 239, 0 }, // Friendly Miskoxy
            { 112, 2 }, // Lamie 2nd
            { 636, 0 }, { 637, 0 }, { 641, 0 }, { 268, 0 }, { 647, 0 }, { 188, 0 }, { 189, 0 }, // Friendly Nymphe
            { 525, 0 }, // Obélisk
            { 74, 0 }, { 74, 1 }, // Pile et Face (CD2)
            { 191, 0 }, // Pluton
            { 338, 0 }, // Roi Lear
            { 931, 0 }, // Shinryu
            { 889, 0 }, // Silver Dragon
            { 337, 0 }, // Steiner 1st
            { 335, 0 }, // Steiner 2nd
            { 337, 0 }, { 337, 1 }, // Steiner 3rd + Bombo
            { 936, 0 }, // Sulfura
            { 294, 0 }, // Valseur 2
            { 296, 0 }, // Valseur 3
            { 930, 1 }, { 930, 2 }, { 930, 3 }, // Adds from Lovecraft fight
            { 668, 0 },  { 217, 0 }, { 670, 0 }, { 751, 0 }, { 652, 0 }, { 664, 0 }, { 216, 0 }, // Friendly Yeti
            // ########### CD4 Bosses ##############
            { 93, 2 }, { 93, 3 }, { 93, 4 }, { 93, 5 } // Prison Cage + Little Girl
        };
    }
}