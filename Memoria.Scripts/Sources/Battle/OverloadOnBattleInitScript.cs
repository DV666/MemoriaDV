﻿using System;
using System.Collections.Generic;
using System.Linq;
using FF9;
using Memoria.Data;
using Memoria.Database;
using Memoria.DefaultScripts;
using UnityEngine;
using static Memoria.Scripts.Battle.TranceSeekAPI;

namespace Memoria.Scripts.Battle
{
    public class OverloadOnBattleInitScript : IOverloadOnBattleInitScript
    {
        public Boolean InitHUDMessageChild;
        public HUDMessageChild HUDToReset = null;
        public Int32 magiclampcooldown;

        public void OnBattleInit()
        {
            // FF9StateSystem.EventState.gEventGlobal[1403] = 4; // Debug difficulty mode

            SB2_PATTERN sb2Pattern = FF9StateSystem.Battle.FF9Battle.btl_scene.PatAddr[FF9StateSystem.Battle.FF9Battle.btl_scene.PatNum];
            KeyValuePair<Int32, Int32> BattleExID = new KeyValuePair<Int32, Int32>(FF9StateSystem.Battle.battleMapIndex, FF9StateSystem.Battle.FF9Battle.btl_scene.PatNum);

            if (CustomBBGonBattleID.ContainsKey(BattleExID)) // Change BBG for specific, to have a better camera.
                ChangeBBG(CustomBBGonBattleID[BattleExID]);

            if (BattleExID.Equals(new KeyValuePair<Int32, Int32>(93, 3))) // Prison Cage + Little Girl
                HonoluluBattleMain.SetupAttachModel(FF9StateSystem.Battle.FF9Battle.btl_data[4], FF9StateSystem.Battle.FF9Battle.btl_data[5], 55, 25);

            if (!InitHUDMessageChild)
            {
                foreach (HUDMessageChild HUDToReset in Singleton<HUDMessage>.Instance.AllMessagePool)
                    HUDToReset.FontSize = 36;
                OverlapSHP.ClearInBattleInit();
                InitHUDMessageChild = true;
            }

            for (Int32 i = 0; i < 8; i++) // CMD Engineer reset
            {
                int idAA = 1136 + i;
                FF9StateSystem.Battle.FF9Battle.aa_data[(BattleAbilityId)idAA].MP = 0;
            }

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
                            battleEnemy.Data.bonus_item_rate[3] += 15;
                            battleEnemy.Data.bonus_item_rate[2] += 64;
                            battleEnemy.Data.bonus_item_rate[1] += 96;
                        }
                    }
                }
            }

            Boolean RefinedMonocleTrigger = false;

            foreach (BattleUnit unit in BattleState.EnumerateUnits())
            {
                ZidanePassive[unit.Data] = [0, 0, 0, 0, 0, 255, 255, 0, 0, 0, 0, 0 ];
                ViviPreviousSpell[unit.Data] = BattleAbilityId.Void;
                ViviPassive[unit.Data] = [0, 0, 0];
                BeatrixPassive[unit.Data] = [0, 0, 0, 0];
                ProtectStatus[unit.Data] = new Dictionary<BattleStatus, Int32> { { 0, 0 } };
                AbsorbElement[unit.Data] = -1;
                StackBreakOrUpStatus[unit.Data] = [0, 0, 0, 0];
                MonsterMechanic[unit.Data] = [ 0, 0, 0, 0, 100, 1, 0 ];
                SpecialSAEffect[unit.Data] = [ 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 ];
                SpecialItemEffect[unit.Data] = [3, 3];
                ElementAffinitiesItem[unit.Data] = [0, 0];
                TriggerSPSResistStatus[unit.Data] = false;
                RollBackStats[unit.Data] = [ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 ];
                RollBackBattleStatus[unit.Data] = 0;
                WeaponNewElement[unit.Data] = EffectElement.None;
                WeaponNewStatus[unit.Data] = 0;
                StateMoug[unit.Data] = 0;
                ModelMoug[unit.Data] = null;

                if (unit.IsPlayer)
                {
                    if ((FF9StateSystem.Battle.battleMapIndex == 334 || FF9StateSystem.Battle.battleMapIndex == 335)) // Add Steal command for Zidane/Marcus against Steiner 2nd
                    {
                        if (unit.PlayerIndex == CharacterId.Zidane || unit.PlayerIndex == CharacterId.Marcus)
                        {
                            CharacterPresetId presetId = unit.Player.PresetId;
                            CharacterCommands.CommandSets[presetId].Regular[2] = BattleCommandId.Steal;
                            CharacterCommands.CommandSets[presetId].Regular[3] = unit.PlayerIndex == CharacterId.Zidane ? BattleCommandId.StageMagicZidane : BattleCommandId.StageMagicMarcus;
                        }
                    }

                    // Poison element
                    if (ItemAffinitiesPoison.ContainsKey(unit.Weapon))
                        if (ElementAffinitiesItem[unit.Data][0] < ItemAffinitiesPoison[unit.Weapon])
                            ElementAffinitiesItem[unit.Data][0] = ItemAffinitiesPoison[unit.Weapon];
                    if (ItemAffinitiesPoison.ContainsKey(unit.Head))
                        if (ElementAffinitiesItem[unit.Data][0] < ItemAffinitiesPoison[unit.Head])
                         ElementAffinitiesItem[unit.Data][0] = ItemAffinitiesPoison[unit.Head];
                    if (ItemAffinitiesPoison.ContainsKey(unit.Armor))
                        if (ElementAffinitiesItem[unit.Data][0] < ItemAffinitiesPoison[unit.Armor])
                            ElementAffinitiesItem[unit.Data][0] = ItemAffinitiesPoison[unit.Armor];
                    if (ItemAffinitiesPoison.ContainsKey(unit.Wrist))
                        if (ElementAffinitiesItem[unit.Data][0] < ItemAffinitiesPoison[unit.Wrist])
                            ElementAffinitiesItem[unit.Data][0] = ItemAffinitiesPoison[unit.Wrist];
                    if (ItemAffinitiesPoison.ContainsKey(unit.Accessory))
                        if (ElementAffinitiesItem[unit.Data][0] < ItemAffinitiesPoison[unit.Accessory])
                            ElementAffinitiesItem[unit.Data][0] = ItemAffinitiesPoison[unit.Accessory];

                    // Gravity element
                    if (ItemAffinitiesGravity.ContainsKey(unit.Weapon))
                        if (ElementAffinitiesItem[unit.Data][1] < ItemAffinitiesGravity[unit.Weapon])
                            ElementAffinitiesItem[unit.Data][1] = ItemAffinitiesGravity[unit.Weapon];
                    if (ItemAffinitiesGravity.ContainsKey(unit.Head))
                        if (ElementAffinitiesItem[unit.Data][1] < ItemAffinitiesGravity[unit.Head])
                            ElementAffinitiesItem[unit.Data][1] = ItemAffinitiesGravity[unit.Head];
                    if (ItemAffinitiesGravity.ContainsKey(unit.Armor))
                        if (ElementAffinitiesItem[unit.Data][1] < ItemAffinitiesGravity[unit.Armor])
                            ElementAffinitiesItem[unit.Data][1] = ItemAffinitiesGravity[unit.Armor];
                    if (ItemAffinitiesGravity.ContainsKey(unit.Wrist))
                        if (ElementAffinitiesItem[unit.Data][1] < ItemAffinitiesGravity[unit.Wrist])
                            ElementAffinitiesItem[unit.Data][1] = ItemAffinitiesGravity[unit.Wrist];
                    if (ItemAffinitiesGravity.ContainsKey(unit.Accessory))
                        if (ElementAffinitiesItem[unit.Data][1] < ItemAffinitiesGravity[unit.Accessory])
                            ElementAffinitiesItem[unit.Data][1] = ItemAffinitiesGravity[unit.Accessory];

                    if (unit.HasSupportAbilityByIndex((SupportAbility)1041)) // Alert+
                    {
                        btl_stat.AlterStatus(unit, TranceSeekStatusId.PerfectDodge, parameters: "+2");
                    }
                    if (unit.HasSupportAbilityByIndex((SupportAbility)52)) // Last Stand
                    {
                        SpecialSAEffect[unit.Data][1] = unit.HasSupportAbilityByIndex((SupportAbility)1052) ? 2 : 1;
                    }
                    if (unit.HasSupportAbilityByIndex((SupportAbility)1252)) // SA I'm all set
                    {
                        unit.AlterStatus(TranceSeekStatus.ArmorUp, unit);
                        unit.AlterStatus(TranceSeekStatus.MentalUp, unit);
                    }
                    if (unit.Weapon == RegularItem.Defender)
                    {
                        unit.AlterStatus(TranceSeekStatus.ArmorUp, unit);
                        unit.AlterStatus(TranceSeekStatus.MentalUp, unit);
                    }
                    if (unit.Armor == (RegularItem)1220) // Mechanical Armor
                    {
                        MonsterMechanic[unit.Data][1] = 10;
                        btl_stat.AlterStatus(unit, TranceSeekStatusId.MechanicalArmor, parameters: MonsterMechanic[unit.Data][1]);
                    }
                    if (unit.Accessory == (RegularItem)1253) // Ishgard Scarf
                    {
                        btl_stat.AlterStatus(btl_scrp.FindBattleUnit(btl_util.GetRandomBtlID(0U, false)), TranceSeekStatusId.Dragon);
                    }
                    else if (unit.Accessory == (RegularItem)1254) // Strange Cube
                    {
                        unit.MaximumHp = (uint)UnityEngine.Random.Range(unit.MaximumHp - (unit.MaximumHp / 2), unit.MaximumHp + (unit.MaximumHp / 2));
                        unit.CurrentHp = unit.MaximumHp;
                        unit.MaximumMp = (uint)UnityEngine.Random.Range(unit.MaximumMp - (unit.MaximumMp / 2), unit.MaximumMp + (unit.MaximumMp / 2));
                        unit.CurrentMp = unit.MaximumMp;
                        unit.Dexterity = (byte)UnityEngine.Random.Range(unit.Dexterity - (unit.Dexterity / 2), unit.Dexterity + (unit.Dexterity / 2));
                        unit.Strength = (byte)UnityEngine.Random.Range(unit.Strength - (unit.Strength / 2), unit.Strength + (unit.Strength / 2));
                        unit.Magic = (byte)UnityEngine.Random.Range(unit.Magic - (unit.Magic / 2), unit.Magic + (unit.Magic / 2));
                        unit.Will = (byte)UnityEngine.Random.Range(unit.Will - (unit.Will / 2), unit.Will + (unit.Will / 2));
                        unit.PhysicalDefence = (byte)UnityEngine.Random.Range(unit.PhysicalDefence - (unit.PhysicalDefence / 2), unit.PhysicalDefence + (unit.PhysicalDefence / 2));
                        unit.MagicDefence = (byte)UnityEngine.Random.Range(unit.MagicDefence - (unit.MagicDefence / 2), unit.MagicDefence + (unit.MagicDefence / 2));
                        unit.PhysicalEvade = (byte)UnityEngine.Random.Range(unit.PhysicalEvade - (unit.PhysicalEvade / 2), unit.PhysicalEvade + (unit.PhysicalEvade / 2));
                        unit.MagicEvade = (byte)UnityEngine.Random.Range(unit.MagicEvade - (unit.MagicEvade / 2), unit.MagicEvade + (unit.MagicEvade / 2));
                        unit.WeakElement = (EffectElement)(1 << Comn.random16() % 8);
                        unit.HalfElement = (EffectElement)(1 << Comn.random16() % 8);
                        unit.GuardElement = (EffectElement)(1 << Comn.random16() % 8);
                        unit.AbsorbElement = (EffectElement)(1 << Comn.random16() % 8);

                    List<BattleStatusId> statuschoosen = new List<BattleStatusId>{ BattleStatusId.Poison, BattleStatusId.Venom, BattleStatusId.Blind, BattleStatusId.Silence, BattleStatusId.Trouble,
                    BattleStatusId.Sleep, BattleStatusId.Freeze, BattleStatusId.Heat, BattleStatusId.Doom, BattleStatusId.Mini, BattleStatusId.Petrify, BattleStatusId.GradualPetrify,
                    BattleStatusId.Berserk, BattleStatusId.Confuse, BattleStatusId.Stop, BattleStatusId.Zombie, BattleStatusId.Slow, TranceSeekStatusId.Vieillissement,
                    TranceSeekStatusId.ArmorBreak, TranceSeekStatusId.MagicBreak, TranceSeekStatusId.MentalBreak, TranceSeekStatusId.PowerBreak, BattleStatusId.Virus, BattleStatusId.Regen,
                    BattleStatusId.Haste, BattleStatusId.Float, BattleStatusId.Shell, BattleStatusId.Protect, BattleStatusId.Vanish, BattleStatusId.Reflect, TranceSeekStatusId.PowerUp,
                    TranceSeekStatusId.MagicUp, TranceSeekStatusId.ArmorUp, TranceSeekStatusId.MentalUp, TranceSeekStatusId.PowerBreak, TranceSeekStatusId.Dragon, TranceSeekStatusId.Bulwark,
                    TranceSeekStatusId.PerfectDodge, TranceSeekStatusId.PerfectCrit };

                        btl_stat.AlterStatus(unit, statuschoosen[Comn.random16() % statuschoosen.Count]);
                    }
                    else if (unit.Accessory == (RegularItem)1256) // Magic Lamp
                    {
                        magiclampcooldown = (60 - unit.Will) * UnityEngine.Random.Range(1, 11) * 100;
                        unit.AddDelayedModifier(ProcessMagicLampRecast, null);
                    }
                    else if (unit.Accessory == (RegularItem)1257) // Cursed Coin
                    {
                        if (DarkBBG.Contains(battlebg.nf_BbgNumber))
                        {
                            CharacterPresetId presetId = unit.Player.PresetId;
                            unit.ChangeToMonster("GZ_R002", 0, CharacterCommands.CommandSets[presetId].Regular[2], (BattleCommandId)1111, false, false, false, false, false);
                            CharacterCommands.CommandSets[presetId].Regular[3] = BattleCommandId.None;
                        }
                    }
                    else if (unit.Accessory == (RegularItem)1261 && !RefinedMonocleTrigger) // Refined Monocle
                    {
                        RefinedMonocleTrigger = true;
                        foreach (BattleUnit monsteratb in BattleState.EnumerateUnits())
                            if (!monsteratb.IsPlayer)
                            {
                                ScanScript.TriggerOneTime[monsteratb.Data] = false;
                                ScanScript.HPBarHidden[monsteratb.Data] = false;
                                ScanScript.ATBGreenBarHUD[monsteratb.Data] = null;
                                ScanScript.ATBRedBarHUD[monsteratb.Data] = null;
                                monsteratb.AddDelayedModifier(ScanScript.ShowATBBar, null);
                            }
                    }
                    if (unit.IsUnderAnyStatus(BattleStatus.EasyKill))
                    {
                        MonsterMechanic[unit.Data][4] = 100; // Reduce time for Sleep/Freeze/Stop
                        MonsterMechanic[unit.Data][5] = 4; // Reduce gravity damage (start at 1 for Elite)
                    }

                    if (unit.PlayerIndex == (CharacterId)14)
                    {
                        unit.SummonCount = 1; // [TODO] Change to Memoria Dict : Used for SA Take that!
                        SpecialSAEffect[unit.Data][13] = unit.PhysicalDefence; // In top form!
                    }

                    if (unit.PlayerIndex == (CharacterId)15) // Reset CMD Komrade
                    {
                        List<BattleAbilityId> ListAAKomrade = CharacterCommands.Commands[(BattleCommandId)1030].EnumerateAbilities().ToList();
                        int TotalAAKomrade = 2 * ListAAKomrade.Count;
                        int FirstAAKomradeId = (Int32)ListAAKomrade[0];
                        if (!FF9StateSystem.EventState.gScriptDictionary.TryGetValue(1030, out Dictionary<Int32, Int32> dict))
                        {
                            dict = new Dictionary<Int32, Int32>();
                            for (Int32 i = 0; i < TotalAAKomrade; i++)
                                dict[FirstAAKomradeId + i] = 1;

                            FF9StateSystem.EventState.gScriptDictionary.Add(1030, dict);
                        }
                        else
                        {
                            for (Int32 i = 0; i < TotalAAKomrade; i++)
                                dict[FirstAAKomradeId + i] = 1;
                        }
                    }

                    int ID = 2000 + (int)unit.PlayerIndex;
                    if (FF9StateSystem.EventState.gScriptDictionary.ContainsKey(ID)) // Reset infused weapon.
                        FF9StateSystem.EventState.gScriptDictionary.Remove(ID);

                    if (Configuration.TetraMaster.TripleTriad >= 16388 && Configuration.TetraMaster.TripleTriad != 16390)
                    {
                        unit.MagicDefence = 254;
                        unit.PhysicalDefence = 254;
                    }
                    if (FF9StateSystem.EventState.ScenarioCounter >= 11100 && FF9StateSystem.EventState.gEventGlobal[1500] == 0)
                    {
                        unit.AlterStatus(BattleStatus.Death, unit);
                        unit.CurrentHp = 0;
                    }
                    if (unit.HasSupportAbilityByIndex((SupportAbility)132)) // SA Anastrophe
                    {
                        int factor = unit.HasSupportAbilityByIndex((SupportAbility)1132) ? 1 : 2;
                        uint UnitOldMaximumHP = unit.MaximumHp;
                        uint UnitOldMaximumMP = unit.MaximumMp;
                        unit.MaximumHp = (uint)(UnitOldMaximumMP / factor);
                        unit.MaximumMp = (uint)(UnitOldMaximumHP / factor);
                        unit.CurrentHp = unit.MaximumHp;
                        unit.CurrentMp = unit.MaximumMp;
                    }

                    if (unit.HasSupportAbilityByIndex((SupportAbility)1212)) // SA Protector+
                    {
                        btl_stat.AlterStatus(unit, TranceSeekStatusId.PowerBreak, parameters: "+2");
                        btl_stat.AlterStatus(unit, TranceSeekStatusId.ArmorUp, parameters: "+2");
                    }
                    else if (unit.HasSupportAbilityByIndex((SupportAbility)212)) // SA Protector
                    {
                        btl_stat.AlterStatus(unit, TranceSeekStatusId.PowerBreak, parameters: "+1");
                        btl_stat.AlterStatus(unit, TranceSeekStatusId.ArmorUp, parameters: "+1");
                    }

                    if (unit.Row == 1)
                        btl_stat.AlterStatus(unit, TranceSeekStatusId.Special, parameters: "CanCover1");
                    else
                        btl_stat.AlterStatus(unit, TranceSeekStatusId.Special, parameters: "CanCover0");
                }
                else // Monsters init
                {
                    BattleEnemy battleEnemy = BattleEnemy.Find(unit);
                    Boolean ChangeStats = true;
                    if (FF9StateSystem.EventState.gEventGlobal[1403] >= 3)
                    {
                        for (Int32 i = 0; i < PreventBossModificationDifficulty.GetLength(0); i++) // Prevent stats modification for specific ennemy.
                        {
                            if (FF9StateSystem.Battle.battleMapIndex == PreventBossModificationDifficulty[i, 0] && sb2Pattern.Monster[unit.Data.bi.slot_no].TypeNo == PreventBossModificationDifficulty[i, 1])
                            {
                                ChangeStats = false;
                            }
                        }
                    }

                    if (ChangeStats)
                    {
                        Boolean ChangeHP = false;
                        for (Int32 i = 0; i < BossBattleBonusHP.GetLength(0); i++) // Check if boss have +10000 HP for scripts
                        {
                            if (FF9StateSystem.Battle.battleMapIndex == BossBattleBonusHP[i, 0] && sb2Pattern.Monster[unit.Data.bi.slot_no].TypeNo == BossBattleBonusHP[i, 1])
                            {
                                ChangeHP = true;
                                uint bonusHP = unit.MaximumHp - 10000;
                                if (FF9StateSystem.EventState.gEventGlobal[1403] == 3) // Kuja mode
                                {
                                    if (FF9StateSystem.EventState.ScenarioCounter > 2250) // After Zidane/Vivi/Steiner get together in Evil Forest
                                    {
                                        unit.MaximumHp += (bonusHP / 10);
                                        unit.CurrentHp += (bonusHP / 10);
                                        unit.Strength = (byte)Math.Min(unit.Strength + (unit.Strength / 4), byte.MaxValue);
                                        unit.Magic = (byte)Math.Min(unit.Magic + (unit.Magic / 4), byte.MaxValue);
                                    }
                                    else
                                    {
                                        unit.MaximumHp += (bonusHP / 20);
                                        unit.CurrentHp += (bonusHP / 20);
                                        unit.Strength = (byte)Math.Min(unit.Strength + 1, byte.MaxValue);
                                        unit.Magic = (byte)Math.Min(unit.Magic + 1, byte.MaxValue);
                                    }
                                }
                                else if (FF9StateSystem.EventState.gEventGlobal[1403] == 4) // Necron mode
                                {
                                    if (FF9StateSystem.EventState.ScenarioCounter > 2250) // After Zidane/Vivi/Steiner get together in Evil Forest
                                    {
                                        unit.MaximumHp += (bonusHP / 4);
                                        unit.CurrentHp += (bonusHP / 4);
                                        unit.Strength = (byte)Math.Min(unit.Strength + (unit.Strength / 2), byte.MaxValue);
                                        unit.Magic = (byte)Math.Min(unit.Magic + (unit.Magic / 2), byte.MaxValue);
                                    }
                                    else
                                    {
                                        unit.MaximumHp += (bonusHP / 8);
                                        unit.CurrentHp += (bonusHP / 8);
                                        unit.Strength = (byte)Math.Min(unit.Strength + 2, byte.MaxValue);
                                        unit.Magic = (byte)Math.Min(unit.Magic + 2, byte.MaxValue);
                                    }
                                    battleEnemy.Data.bonus_exp /= 2;
                                    battleEnemy.Data.bonus_gil /= 10;
                                }
                                MonsterMechanic[unit.Data][3] = 1;
                                break;
                            }
                        }
                        if (!ChangeHP)
                        {
                            uint bonusHP = unit.MaximumHp;
                            if (FF9StateSystem.EventState.gEventGlobal[1403] == 3) // Kuja mode
                            {
                                if (FF9StateSystem.EventState.ScenarioCounter > 2250) // After Zidane/Vivi/Steiner get together in Evil Forest
                                {
                                    unit.MaximumHp += (bonusHP / 10);
                                    unit.CurrentHp += (bonusHP / 10);
                                    unit.Strength = (byte)Math.Min(unit.Strength + (unit.Strength / 4), byte.MaxValue);
                                    unit.Magic = (byte)Math.Min(unit.Magic + (unit.Magic / 4), byte.MaxValue);
                                }
                                else
                                {
                                    unit.MaximumHp += (bonusHP / 20);
                                    unit.CurrentHp += (bonusHP / 20);
                                    unit.Strength = (byte)Math.Min(unit.Strength + 1, byte.MaxValue);

                                }
                            }
                            else if (FF9StateSystem.EventState.gEventGlobal[1403] == 4) // Necron mode
                            {
                                if (FF9StateSystem.EventState.ScenarioCounter > 2250) // After Zidane/Vivi/Steiner get together in Evil Forest
                                {
                                    unit.MaximumHp += (bonusHP / 4);
                                    unit.CurrentHp += (bonusHP / 4);
                                    unit.Strength = (byte)Math.Min(unit.Strength + (unit.Strength / 2), byte.MaxValue);
                                    unit.Magic = (byte)Math.Min(unit.Magic + (unit.Magic / 2), byte.MaxValue);
                                }
                                else
                                {
                                    unit.MaximumHp += (bonusHP / 8);
                                    unit.CurrentHp += (bonusHP / 8);
                                    unit.Strength = (byte)Math.Min(unit.Strength + 2, byte.MaxValue);
                                    unit.Magic = (byte)Math.Min(unit.Magic + 2, byte.MaxValue);
                                }
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

                    if (FF9StateSystem.Battle.battleMapIndex == 838 && sb2Pattern.Monster[unit.Data.bi.slot_no].TypeNo == 1) // Golden Pidove (fake Sleep)
                    {
                        if (!TranceSeekSpecial.PolaritySPS.TryGetValue(unit, out SPSEffect sps))
                            TranceSeekSpecial.PolaritySPS[unit] = null;

                        sps = HonoluluBattleMain.battleSPS.AddSequenceSPS(2, -1, 1, true);
                        if (sps == null)
                            return;
                        btl2d.GetIconPosition(unit, btl2d.ICON_POS_HEAD, out Transform attachTransf, out Vector3 iconOff);
                        sps.charTran = unit.Data.gameObject.transform;
                        sps.boneTran = attachTransf;
                        sps.posOffset = Vector3.zero;
                        TranceSeekSpecial.PolaritySPS[unit] = sps;
                    }
                }
            }
        }

        private Boolean ProcessMagicLampRecast(BattleUnit caster)
        {
            if (magiclampcooldown <= 0)
            {
                List<BattleAbilityId> MagicLampSummon = new List<BattleAbilityId> { BattleAbilityId.DiamondDust, BattleAbilityId.FlamesofHell, BattleAbilityId.JudgementBolt, BattleAbilityId.WormHole,
                BattleAbilityId.Zantetsuken, BattleAbilityId.Tsunami, BattleAbilityId.MegaFlare, BattleAbilityId.EternalDarkness, (BattleAbilityId)1576, (BattleAbilityId)1577, (BattleAbilityId)1578,
                (BattleAbilityId)1579, (BattleAbilityId)1580, (BattleAbilityId)1581, (BattleAbilityId)1582, (BattleAbilityId)1583};

                BattleAbilityId SummonChoosen = MagicLampSummon[Comn.random16() % MagicLampSummon.Count];
                ushort target = 0;

                if (SummonChoosen == (BattleAbilityId)1578 || SummonChoosen == (BattleAbilityId)1579 || SummonChoosen == (BattleAbilityId)1580 || SummonChoosen == (BattleAbilityId)1581) // Carbuncle
                    target = btl_util.GetStatusBtlID(0u, 0);
                else
                    target = btl_util.GetStatusBtlID(1u, 0);

                btl_cmd.SetCommand(caster.Data.cmd[3], BattleCommandId.SysPhantom, (Int32)SummonChoosen, target, 8u);
                return false;
            }
            else
            {
                magiclampcooldown -= caster.Data.cur.at_coef * BattleState.ATBTickCount;
            }
            return true;
        }

        public void ChangeBBG(string BBGNameID)
        {
            FF9StateBattleSystem ff9Battle = FF9StateSystem.Battle.FF9Battle;
            ff9Battle.map.btlBGPtr.SetActive(false);
            ff9Battle.map.btlBGPtr = ModelFactory.CreateModel("BattleMap/BattleModel/battleMap_all/" + BBGNameID + "/" + BBGNameID, Vector3.zero, Vector3.zero, true);
            GEOTEXHEADER geotexheader = new GEOTEXHEADER();
            geotexheader.ReadBGTextureAnim(BBGNameID);
            ff9Battle.map.btlBGTexAnimPtr = geotexheader;
            BBGINFO bbginfo = new BBGINFO();
            bbginfo.ReadBattleInfo(BBGNameID);
            ff9Battle.map.btlBGInfoPtr = bbginfo;
            battle.InitBattleMap();
            ff9Battle.map.btlBGPtr.SetActive(true);
        }

        public static Int32[,] BossBattleBonusHP = new Int32[,]
        {
            { 73, 0 }, // Beatrix 3rd
		    { 299, 0 }, // Beatrix 2nd
		    { 4, 0 }, // Beatrix 1st
            { 4, 1 }, // Dark Beatrix
            { 14, 0 }, // Zaghnol
            { 295, 0 }, // Bach
		    { 115, 1 },	// Kuja 1st (Ifa)
		    { 920, 0 }, { 921, 0 }, // Friendly Belhamel
            { 938, 0 }, // Necron
            { 251, 0 }, { 363, 0 }, { 364, 0 }, { 838, 0 }, // Friendly Eskuriax
            { 192, 0 }, { 193, 0 }, { 196, 0 }, { 197, 0 }, { 199, 0 }, // Friendly Fantôme
            { 300, 0 }, // Fourmillion
            { 107, 0 }, // Gargantua (1st)
            { 890, 0 }, // Garland
            { 2, 0 }, { 2, 2 }, { 2, 3 }, { 2, 4 }, // Shrines Guardian
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
            { 191, 0 }, // Pluton
            { 338, 0 }, // Roi Lear
            { 931, 0 }, // Shinryu
            { 889, 0 }, // Silver Dragon
            { 337, 0 }, // Steiner 1st
            { 335, 0 }, { 335, 1 }, { 335, 2 }, // Steiner 2nd
            { 337, 0 }, { 337, 1 }, // Steiner 3rd + Bombo
            { 936, 0 }, // Sulfura
            { 294, 0 }, // Valseur 2
            { 296, 0 }, // Valseur 3
            { 74, 0 }, { 74, 1 }, // Zorn & Thorn
            { 930, 1 }, { 930, 2 }, { 930, 3 }, // Adds from Lovecraft fight
            { 600, 2 }, { 600, 3 }, { 600, 4 }, // Adds from Fandalf fight
            { 668, 0 },  { 217, 0 }, { 670, 0 }, { 751, 0 }, { 652, 0 }, { 664, 0 }, { 216, 0 }, // Friendly Yeti
            // ########### CD4 Bosses ##############
            { 93, 2 }, { 93, 3 }, { 93, 4 }, { 93, 5 }, // Prison Cage + Little Girl
            { 299, 1 }, { 299, 2 } // Jötunn + Flarder
        };

        public static Int32[,] PreventBossModificationDifficulty = new Int32[,]
        {
            { 301, 1 }, // Prison Cage + Vivi
            { 302, 1 }, // Prison Cage + Dagga
            { 303, 1 } // Dagga (Plant Brain CD1)
        };

        public static Dictionary<KeyValuePair<Int32, Int32>, String> CustomBBGonBattleID = new Dictionary<KeyValuePair<Int32, Int32>, String>
        {
            { new KeyValuePair<Int32, Int32>(299, 1), "BBG_B023" }, // Lindblum boss (Steiner Quest)
            { new KeyValuePair<Int32, Int32>(838, 1), "BBG_B042" } // Golden Pidove
        };

        private static readonly HashSet<Int32> DarkBBG = new HashSet<Int32>(new[] { 4, 5, 6, 8, 9, 12, 13, 14, 15, 16, 32, 36, 37, 38, 39, 40, 41, 42, 45, 46, 67, 68, 69, 70, 71, 72,
        80, 84, 85, 86, 87, 88, 89, 90, 100, 114, 116, 117, 118, 119, 121, 122, 128, 131, 152, 153, 155, 156, 158, 162, 163, 164, 165, 167, 168, 169, 174, 175 });

        private static readonly Dictionary<RegularItem, Int32> ItemAffinitiesPoison = new Dictionary<RegularItem, Int32> // 0 = None, 1 = Weak, 2 = Half, 4 = Immune, 8 = Absorb
        {
            { RegularItem.MythrilArmlet, 2 },
            { RegularItem.GoldHelm, 2 },
            { (RegularItem)121, 8 }, // Umbrella Shoes
            { (RegularItem)1257, 4 } // Cursed Coin
        };

        private static readonly Dictionary<RegularItem, Int32> ItemAffinitiesGravity = new Dictionary<RegularItem, Int32>
        {
            { RegularItem.PlatinaArmor, 2 },
            { RegularItem.DarkGear, 4 },
            { RegularItem.BlackBelt, 8 }
        };
    }
}
