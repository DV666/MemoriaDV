using Assets.Scripts.Common;
using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Assets;
using Memoria.Data;
using Memoria.Database;
using Memoria.DefaultScripts;
using Memoria.Prime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using static BTL_DATA;
using static Memoria.Scripts.Battle.TranceSeekAPI;

namespace Memoria.Scripts.Battle
{
    public class OverloadOnBattleInitScript : IOverloadOnBattleInitScript
    {
        public Boolean InitHUDMessageChild;
        public HUDMessageChild HUDToReset = null;
        public Int32 magiclampcooldown;

        private BattleUnit Sister = null; // TO DELETE - After Memoria Update :) (fix cover)
        private Vector3 SisterPosition = Vector3.zero; // TO DELETE - After Memoria Update :) (fix cover)

        public void OnBattleInit()
        {
            // Zidane = 0, Vivi = 1, Eiko = 2, Kuja = 3, Necron = 4, Beatrix = 5, Ozma = 6, Garland = 7
            /*if (false)
            {
                FF9StateSystem.EventState.gEventGlobal[1403] = 0; // Debug difficulty mode
                if (FF9StateSystem.EventState.gEventGlobal[1403] >= 4 && FF9StateSystem.EventState.gEventGlobal[1403] <= 6) // Activate Hardcore IA
                    FF9StateSystem.EventState.gEventGlobal[1407] = 1;
                else
                    FF9StateSystem.EventState.gEventGlobal[1407] = 0;
            }*/

            int BattleID = FF9StateSystem.Battle.battleMapIndex;
            int GroupeBattleID = FF9StateSystem.Battle.FF9Battle.btl_scene.PatNum;

            BTL_SCENE btl_scene = FF9StateSystem.Battle.FF9Battle.btl_scene;
            SB2_PATTERN sb2Pattern = FF9StateSystem.Battle.FF9Battle.btl_scene.PatAddr[GroupeBattleID];
            KeyValuePair<Int32, Int32> BattleExID = new KeyValuePair<Int32, Int32>(BattleID, GroupeBattleID);

            if (CustomBBGfromBattleID.ContainsKey(BattleExID)) // Change BBG for specific, to have a better camera.
                ChangeBBG(CustomBBGfromBattleID[BattleExID]);

            if (ChangeDepthBBGfromBattleID.ContainsKey(BattleExID)) // Change BBG for specific, to have a better camera.
                ChangeDepthBBG(ChangeDepthBBGfromBattleID[BattleExID]);

            if (BattleExID.Equals(new KeyValuePair<Int32, Int32>(93, 3))) // Prison Cage + Little Girl
                HonoluluBattleMain.SetupAttachModel(FF9StateSystem.Battle.FF9Battle.btl_data[4], FF9StateSystem.Battle.FF9Battle.btl_data[5], 55, 25);

            foreach (HUDMessageChild HUDToReset in Singleton<HUDMessage>.Instance.AllMessagePool)
            HUDToReset.FontSize = 36;
            OverlapSHP.ClearInBattleInit();

            FF9StateSystem.EventState.gEventGlobal[1544] = 0; // Reset Charm Target

            for (Int32 i = 0; i < 8; i++) // CMD Engineer reset
            {
                int idAA = 1136 + i;
                BattleAbilityId abilityId = (BattleAbilityId)idAA;

                if (FF9StateSystem.Battle.FF9Battle.aa_data.ContainsKey(abilityId))
                    if (FF9StateSystem.Battle.FF9Battle.aa_data[abilityId] != null)
                        FF9StateSystem.Battle.FF9Battle.aa_data[abilityId].MP = 0;
            }

            if (!FF9StateSystem.EventState.gScriptDictionary.TryGetValue(1000, out Dictionary<Int32, Int32> dictbattle)) // Modificators for battle
            {
                dictbattle = new Dictionary<Int32, Int32>();
                FF9StateSystem.EventState.gScriptDictionary.Add(1000, dictbattle);
            }
            dictbattle[0] = 0; // Bonus gils from SA or items
            dictbattle[1] = 0; // Steiner mechanic
            dictbattle[2] = 0; // Beatrix mechanic

            if (!FF9StateSystem.EventState.gScriptDictionary.TryGetValue(1001, out Dictionary<Int32, Int32> dictdifficulty)) // Modificators from difficulties
            {
                dictdifficulty = new Dictionary<Int32, Int32>();
                FF9StateSystem.EventState.gScriptDictionary.Add(1001, dictdifficulty);
            }

            if (FF9StateSystem.EventState.gEventGlobal[1403] < 7)
            {
                // Bonuses here are written in % (positive or negative)
                dictdifficulty[0] = 0; // HP Bonus
                dictdifficulty[1] = 0; // MP Bonus
                dictdifficulty[2] = 0; // Level Bonus
                dictdifficulty[3] = 0; // Dexterity Bonus
                dictdifficulty[4] = 0; // Strength Bonus
                dictdifficulty[5] = 0; // Magic Bonus
                dictdifficulty[6] = 0; // Spirit Bonus
                dictdifficulty[7] = 0; // Defence.P Bonus
                dictdifficulty[8] = 0; // Evade.P Bonus
                dictdifficulty[9] = 0; // Defence.M Bonus
                dictdifficulty[10] = 0; // Evade.M Bonus
                dictdifficulty[11] = 0; // EXP Bonus
                dictdifficulty[12] = 0; // Gils Bonus
                dictdifficulty[13] = 0; // Power AA Bonus

                if (FF9StateSystem.EventState.gEventGlobal[1403] == 3) // Kuja mode
                {
                    if (FF9StateSystem.EventState.ScenarioCounter > 2250) // After Zidane/Vivi/Steiner get together in Evil Forest
                    {
                        dictdifficulty[0] = 25;
                        dictdifficulty[4] = 25;
                        dictdifficulty[5] = 25;
                    }
                    else
                    {
                        dictdifficulty[0] = 5;
                        dictdifficulty[4] = 10;
                        dictdifficulty[5] = 10;
                    }
                }
                else if (FF9StateSystem.EventState.gEventGlobal[1403] == 5 || FF9StateSystem.EventState.gEventGlobal[1403] == 6) // Necron mode + Ozma Mode
                {
                    if (FF9StateSystem.EventState.ScenarioCounter > 2250) // After Zidane/Vivi/Steiner get together in Evil Forest
                    {
                        dictdifficulty[0] = 50;
                        dictdifficulty[4] = 50;
                        dictdifficulty[5] = 50;
                        dictdifficulty[13] = 10;
                    }
                    else
                    {
                        dictdifficulty[0] = 10;
                        dictdifficulty[4] = 20;
                        dictdifficulty[5] = 20;
                        dictdifficulty[13] = 10;
                    }
                }
                else if (FF9StateSystem.EventState.gEventGlobal[1403] == 4 ) // Necron mode
                {
                    if (FF9StateSystem.EventState.ScenarioCounter > 2250) // After Zidane/Vivi/Steiner get together in Evil Forest
                    {
                        dictdifficulty[0] = 80;
                        dictdifficulty[4] = 75;
                        dictdifficulty[5] = 75;
                        dictdifficulty[13] = 10;
                    }
                    else
                    {
                        dictdifficulty[0] = 20;
                        dictdifficulty[4] = 25;
                        dictdifficulty[5] = 25;
                        dictdifficulty[13] = 10;
                    }
                    
                    dictdifficulty[12] = -90; // Gils Malus
                }
                else if (FF9StateSystem.EventState.gEventGlobal[1403] == 1) // Vivi mode
                {
                    dictdifficulty[11] = 25;
                    dictdifficulty[12] = 25;
                }

                if (dictdifficulty[13] > 0)
                {
                    List<AA_DATA> attackList = FF9StateSystem.Battle.FF9Battle.enemy_attack;

                    for (int i = 0; i < attackList.Count; i++)
                    {
                        AA_DATA attack = attackList[i];
                        if ((attack.Type & 2) != 0) // Hide AP Figure, dummied for monsters
                            attack.Ref.Power = attack.Ref.Power + Math.Max(1, (Int32)Math.Round((attack.Ref.Power * dictdifficulty[13]) / 100.0));
                    }
                }
            }

            if (Configuration.Mod.FolderNames.Contains("TranceSeek/StuffListed"))
                SpecialFilesTranceSeek.WriteStuffInFile();

            InitTSVariables();

            Boolean RefinedMonocleTrigger = false;

            foreach (BattleUnit unit in BattleState.EnumerateUnits())
            {
                var StateDict = TranceSeekBattleDictionary.GetState(unit.Data);

                if ((BattleID == 849 || GroupeBattleID == 2) && !unit.IsPlayer) // TO DELETE - After Memoria Update :) (fix cover)
                {
                    if (unit.Data.dms_geo_id == 592)
                    {
                        Log.Message("[FIX DV] Fix cover visual for the battle");
                        unit.AddDelayedModifier(FixCoverVisualForBrother, null);
                    }
                    else if (unit.Data.dms_geo_id == 591)
                    {
                        Sister = unit;
                        SisterPosition = unit.Data.pos;
                    }
                }

                if (unit.IsPlayer)
                {
                    if (btl_scene.Info.StartType == battle_start_type_tags.BTL_START_BACK_ATTACK)
                        StateDict.IsBackAttack = true;

                    if ((BattleID == 334 || BattleID == 335)) // Add Steal command for Zidane/Marcus against Steiner 2nd
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
                        if (StateDict.EffectElement.Poison < ItemAffinitiesPoison[unit.Weapon])
                            StateDict.EffectElement.Poison = ItemAffinitiesPoison[unit.Weapon];
                    if (ItemAffinitiesPoison.ContainsKey(unit.Head))
                        if (StateDict.EffectElement.Poison < ItemAffinitiesPoison[unit.Head])
                            StateDict.EffectElement.Poison = ItemAffinitiesPoison[unit.Head];
                    if (ItemAffinitiesPoison.ContainsKey(unit.Armor))
                        if (StateDict.EffectElement.Poison < ItemAffinitiesPoison[unit.Armor])
                            StateDict.EffectElement.Poison = ItemAffinitiesPoison[unit.Armor];
                    if (ItemAffinitiesPoison.ContainsKey(unit.Wrist))
                        if (StateDict.EffectElement.Poison < ItemAffinitiesPoison[unit.Wrist])
                            StateDict.EffectElement.Poison = ItemAffinitiesPoison[unit.Wrist];
                    if (ItemAffinitiesPoison.ContainsKey(unit.Accessory))
                        if (StateDict.EffectElement.Poison < ItemAffinitiesPoison[unit.Accessory])
                            StateDict.EffectElement.Poison = ItemAffinitiesPoison[unit.Accessory];

                    // Gravity element
                    if (ItemAffinitiesGravity.ContainsKey(unit.Weapon))
                        if (StateDict.EffectElement.Gravity < ItemAffinitiesGravity[unit.Weapon])
                            StateDict.EffectElement.Gravity = ItemAffinitiesGravity[unit.Weapon];
                    if (ItemAffinitiesGravity.ContainsKey(unit.Head))
                        if (StateDict.EffectElement.Gravity < ItemAffinitiesGravity[unit.Head])
                            StateDict.EffectElement.Gravity = ItemAffinitiesGravity[unit.Head];
                    if (ItemAffinitiesGravity.ContainsKey(unit.Armor))
                        if (StateDict.EffectElement.Gravity < ItemAffinitiesGravity[unit.Armor])
                            StateDict.EffectElement.Gravity = ItemAffinitiesGravity[unit.Armor];
                    if (ItemAffinitiesGravity.ContainsKey(unit.Wrist))
                        if (StateDict.EffectElement.Gravity < ItemAffinitiesGravity[unit.Wrist])
                            StateDict.EffectElement.Gravity = ItemAffinitiesGravity[unit.Wrist];
                    if (ItemAffinitiesGravity.ContainsKey(unit.Accessory))
                        if (StateDict.EffectElement.Gravity < ItemAffinitiesGravity[unit.Accessory])
                            StateDict.EffectElement.Gravity = ItemAffinitiesGravity[unit.Accessory];
                        
                    if (unit.HasSupportAbilityByIndex((SupportAbility)1041)) // Alert+
                    {
                        btl_stat.AlterStatus(unit, TranceSeekStatusId.PerfectDodge, parameters: "+2");
                    }
                    if (unit.HasSupportAbilityByIndex((SupportAbility)52)) // Last Stand
                    {
                        StateDict.SpecialSA.LastStand = unit.HasSupportAbilityByIndex((SupportAbility)1052) ? 2 : 1;
                    }
                    if (unit.HasSupportAbilityByIndex((SupportAbility)1252)) // SA I'm all set
                    {
                        unit.AlterStatus(TranceSeekStatus.ArmorUp, unit);
                        unit.AlterStatus(TranceSeekStatus.MentalUp, unit);
                    }
                    if (unit.HasSupportAbilityByIndex((SupportAbility)1045)) // Pluriche+
                    {
                        dictbattle[0] += 3;
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
                    else if (unit.HasSupportAbilityByIndex(SupportAbility.Millionaire)) // Pluriche+
                    {
                        dictbattle[0] += 2;
                    }
                    if (unit.Accessory == (RegularItem)1212) // Cait's Eye
                    {
                        dictbattle[0] += 1;
                    }
                    if (unit.Weapon == RegularItem.Defender)
                    {
                        unit.AlterStatus(TranceSeekStatus.ArmorUp, unit);
                        unit.AlterStatus(TranceSeekStatus.MentalUp, unit);
                    }
                    if (unit.Wrist == RegularItem.ThiefGloves)
                    {
                        foreach (BattleUnit monster in BattleState.EnumerateUnits())
                            if (!monster.IsPlayer)
                                TranceSeekBattleDictionary.GetState(monster.Data).Zidane.EyeOfTheThief = true;
                    }
                    if (unit.Armor == (RegularItem)1220) // Mechanical Armor
                    {
                        StateDict.SpecialItem.MechanicalArmor = 10;
                        btl_stat.AlterStatus(unit, TranceSeekStatusId.MechanicalArmor, parameters: StateDict.SpecialItem.MechanicalArmor);
                    }
                    if (unit.Accessory == (RegularItem)1253) // Ishgard Scarf
                    {
                        unit.AddDelayedModifier(
                            caster => FF9StateSystem.Battle.FF9Battle.btl_phase < FF9StateBattleSystem.PHASE_MENU_ON,
                            caster =>
                            {
                                List<UInt16> TargetsAvailable = new List<UInt16>(4);
                                for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next)
                                    if (next.bi.player == 0 && !btl_stat.CheckStatus(next, BattleStatus.Death | BattleStatus.Petrify) && next.bi.target != 0)
                                        TargetsAvailable.Add(next.btl_id);

                                if (TargetsAvailable.Count > 0)
                                    btl_stat.AlterStatus(btl_scrp.FindBattleUnitUnlimited(TargetsAvailable[UnityEngine.Random.Range(0, TargetsAvailable.Count)]), TranceSeekStatusId.Dragon);
                            }
                        );
                    }
                    else if (unit.Accessory == (RegularItem)1254) // Strange Cube
                    {
                        unit.MaximumHp = (uint)UnityEngine.Random.Range(unit.MaximumHp - (unit.MaximumHp / 2), unit.MaximumHp + (unit.MaximumHp / 2));
                        StateDict.SpecialSA.NewMaximumHP = (int)unit.MaximumHp;
                        if (unit.CurrentHp > unit.MaximumHp)
                            unit.CurrentHp = unit.MaximumHp;
                        unit.MaximumMp = (uint)UnityEngine.Random.Range(unit.MaximumMp - (unit.MaximumMp / 2), unit.MaximumMp + (unit.MaximumMp / 2));
                        StateDict.SpecialSA.NewMaximumMP = (int)unit.MaximumMp;
                        if (unit.CurrentMp > unit.MaximumMp)
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
                        unit.AddDelayedModifier(
                            caster => FF9StateSystem.Battle.FF9Battle.btl_phase < FF9StateBattleSystem.PHASE_MENU_ON,
                            caster =>
                            {
                                foreach (BattleUnit monsteratb in BattleState.EnumerateUnits())
                                    if (!monsteratb.IsPlayer && monsteratb.IsTargetable)
                                    {
                                        ScanScript.TriggerOneTime[monsteratb.Data] = false;
                                        ScanScript.HPBarHidden[monsteratb.Data] = false;
                                        ScanScript.ATBGreenBarHUD[monsteratb.Data] = null;
                                        ScanScript.ATBRedBarHUD[monsteratb.Data] = null;
                                        monsteratb.AddDelayedModifier(ScanScript.ShowATBBar, null);
                                    }
                            }
                        );
                    }
                    if (unit.PlayerIndex == CharacterId.Eiko) // Init Moug
                    {
                        PassiveEikoScript.ModelMoug[unit.Data] = ModelFactory.CreateModel("GEO_NPC_F4_MOG", true);
                        PassiveEikoScript.ModelMoug[unit.Data].SetActive(false);
                        PassiveEikoScript.StateMoug[unit.Data] = 0;
                    }
                    else if (unit.PlayerIndex == (CharacterId)14)
                    {
                        unit.SummonCount = 1; // [TODO] Change to Memoria Dict : Used for SA Take that!
                        StateDict.Baku.InTopForm = unit.PhysicalDefence;
                    }
                    else if (unit.PlayerIndex == (CharacterId)15) // Reset CMD Komrade
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

                    if (FF9StateSystem.EventState.ScenarioCounter >= 11100 && FF9StateSystem.EventState.gEventGlobal[1500] == 0)
                    {
                        unit.AlterStatus(BattleStatus.Death, unit);
                        unit.CurrentHp = 0;
                    }
                    if (unit.HasSupportAbilityByIndex((SupportAbility)132) && false) // SA Anastrophe
                    {
                        int factor = unit.HasSupportAbilityByIndex((SupportAbility)1132) ? 1 : 2;
                        uint UnitOldMaximumHP = unit.MaximumHp;
                        uint UnitOldMaximumMP = unit.MaximumMp;
                        unit.MaximumHp = (uint)(UnitOldMaximumMP / factor);
                        unit.MaximumMp = (uint)(UnitOldMaximumHP / factor);
                        if (unit.CurrentHp > unit.MaximumHp)
                            unit.CurrentHp = unit.MaximumHp;
                        if (unit.CurrentMp > unit.MaximumMp)
                            unit.CurrentMp = unit.MaximumMp;
                        StateDict.SpecialSA.NewMaximumHP = (int)unit.MaximumHp;
                        StateDict.SpecialSA.NewMaximumMP = (int)unit.MaximumMp;
                    }

                    if (unit.HasSupportAbilityByIndex((SupportAbility)1212)) // SA Protector+
                    {
                        btl_stat.AlterStatus(unit, TranceSeekStatusId.PowerBreak, parameters: "+4");
                        btl_stat.AlterStatus(unit, TranceSeekStatusId.ArmorUp, parameters: "+4");
                    }
                    else if (unit.HasSupportAbilityByIndex((SupportAbility)212)) // SA Protector
                    {
                        btl_stat.AlterStatus(unit, TranceSeekStatusId.PowerBreak, parameters: "+2");
                        btl_stat.AlterStatus(unit, TranceSeekStatusId.ArmorUp, parameters: "+2");
                    }

                    if (unit.Row == 1)
                        btl_stat.AlterStatus(unit, TranceSeekStatusId.Special, parameters: "CanCover1");
                    else
                        btl_stat.AlterStatus(unit, TranceSeekStatusId.Special, parameters: "CanCover0");

                    if (unit.PlayerIndex == CharacterId.Steiner)
                    {
                        FF9TextTool.SetCommandName(BattleCommandId.SwordAct, TranceSeekBattleCommand.SwdArtCMDNameVanilla[Localization.CurrentDisplaySymbol]);
                        if (FF9TextTool.DisplayBatch.commandName.TryGetValue(BattleCommandId.SwordAct, out String SwordActName))
                            InfusedWeaponScript.CMDVanillaName[unit.Data][0] = SwordActName;

                        unit.AddDelayedModifier(TranceSeekCharacterMechanic.SteinerMechanic, null);
                    }
                    if (unit.PlayerIndex == CharacterId.Beatrix)
                    {
                        FF9TextTool.SetCommandName(BattleCommandId.HolySword1, TranceSeekBattleCommand.SeikenCMDNameVanilla[Localization.CurrentDisplaySymbol]);
                        FF9TextTool.SetCommandName(BattleCommandId.HolySword2, TranceSeekBattleCommand.SeikenPlusCMDNameVanilla[Localization.CurrentDisplaySymbol]);
                        if (FF9TextTool.DisplayBatch.commandName.TryGetValue(BattleCommandId.HolySword1, out String SeikenName))
                            InfusedWeaponScript.CMDVanillaName[unit.Data][0] = SeikenName;

                        if (FF9TextTool.DisplayBatch.commandName.TryGetValue(BattleCommandId.HolySword2, out String SeikenPlusName))
                            InfusedWeaponScript.CMDVanillaName[unit.Data][1] = SeikenPlusName;
                    }
                }
                else // Monsters init
                {
                    if (btl_scene.Info.StartType == battle_start_type_tags.BTL_START_FIRST_ATTACK)
                        StateDict.IsBackAttack = true;

                    BattleEnemy battleEnemy = BattleEnemy.Find(unit);

                    if (unit.IsUnderAnyStatus(BattleStatus.EasyKill))
                    {
                        StateDict.Monster.DurationDeadlyStatus = 100; // Reduce time for Sleep/Freeze/Stop
                        StateDict.Monster.NerfGravity = 4; // Reduce gravity damage (start at 1 for Elite)
                    }

                    if ((btl_util.getEnemyPtr(unit).info.flags & 32) == 0)  // Unused (6)
                    {
                        uint bonusHP = unit.MaximumHp;
                        if ((btl_util.getEnemyPtr(unit).info.flags & 64) != 0) // Unused (7)
                        {
                            if (bonusHP > 10000)
                            {
                                bonusHP = unit.MaximumHp - 10000;
                                unit.MaximumHp += (uint)((bonusHP * dictdifficulty[0]) / 100);
                                unit.CurrentHp = unit.MaximumHp;
                                StateDict.Monster.HPBoss10000 = true;
                            }
                            else
                            {
                                Log.Message($"[Trance Seek] Boss HP Bonus can't be applied on {unit.Name} : HP is under 10000.");
                                unit.MaximumHp += (uint)((bonusHP * dictdifficulty[0]) / 100);
                                unit.CurrentHp = unit.MaximumHp;
                            }

                        }
                        else
                        {
                            unit.MaximumHp += (uint)((bonusHP * dictdifficulty[0]) / 100);
                            unit.CurrentHp = unit.MaximumHp;
                        }

                        unit.MaximumMp += (uint)((unit.MaximumMp * dictdifficulty[1]) / 100);
                        unit.CurrentMp = unit.MaximumMp;

                        unit.Level = (byte)Math.Min((unit.Level + (unit.Level * dictdifficulty[2]) / 100), byte.MaxValue);
                        unit.Dexterity = (byte)Math.Min((unit.Dexterity + (unit.Dexterity * dictdifficulty[3]) / 100), 50);
                        unit.Strength = (byte)Math.Min((unit.Strength + (unit.Strength * dictdifficulty[4]) / 100), byte.MaxValue);
                        unit.Magic = (byte)Math.Min((unit.Magic + (unit.Magic * dictdifficulty[5]) / 100), byte.MaxValue);
                        unit.Will = (byte)Math.Min((unit.Will + (unit.Will * dictdifficulty[6]) / 100), 50);
                        unit.PhysicalDefence = (byte)Math.Min((unit.PhysicalDefence + (unit.PhysicalDefence * dictdifficulty[7]) / 100), byte.MaxValue);
                        unit.PhysicalEvade = (byte)Math.Min((unit.PhysicalEvade + (unit.PhysicalEvade * dictdifficulty[8]) / 100), byte.MaxValue);
                        unit.MagicDefence = (byte)Math.Min((unit.MagicDefence + (unit.MagicDefence * dictdifficulty[9]) / 100), byte.MaxValue);
                        unit.MagicEvade = (byte)Math.Min((unit.MagicEvade + (unit.MagicEvade * dictdifficulty[10]) / 100), byte.MaxValue);


                        battleEnemy.Data.bonus_exp = (uint)(battleEnemy.Data.bonus_exp + ((battleEnemy.Data.bonus_exp * dictdifficulty[11]) / 100));
                        battleEnemy.Data.bonus_gil = (uint)(battleEnemy.Data.bonus_gil + ((battleEnemy.Data.bonus_gil * dictdifficulty[12]) / 100));
                    }

                    if (BattleID == 838 && sb2Pattern.Monster[unit.Data.bi.slot_no].TypeNo == 1) // Golden Pidove (fake Sleep)
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

                    SB2_PUT enemyPlacement = btl_scene.PatAddr[GroupeBattleID].Monster[unit.Data.bi.slot_no];
                    SB2_MON_PARM monParam = btl_scene.MonAddr[enemyPlacement.TypeNo];

                    // Pad 0 (byte) => Unused (1) ; Pad 1 (uint16) => Unused (2) ; Pad 2 (uint16) => Unused (3)
                    // Pad 1 :
                    // 1 -> Weak Poison     16 -> Weak Gravity
                    // 2 -> Half Poison     32 -> Half Gravity
                    // 4 -> Immune Poison   64 -> Immune Gravity
                    // 8 -> Absorb Poison   128 -> Absorb Gravity

                    if (monParam.Pad1 > 0)
                    {
                        if ((monParam.Pad1 & 1) != 0)
                            StateDict.EffectElement.Poison += 1;
                        if ((monParam.Pad1 & 2) != 0)
                            StateDict.EffectElement.Poison += 2;
                        if ((monParam.Pad1 & 4) != 0)
                            StateDict.EffectElement.Poison += 4;
                        if ((monParam.Pad1 & 8) != 0)
                            StateDict.EffectElement.Poison += 8;
                        if ((monParam.Pad1 & 16) != 0)
                            StateDict.EffectElement.Gravity += 1;
                        if ((monParam.Pad1 & 32) != 0)
                            StateDict.EffectElement.Gravity += 2;
                        if ((monParam.Pad1 & 64) != 0)
                            StateDict.EffectElement.Gravity += 4;
                        if ((monParam.Pad1 & 128) != 0)
                            StateDict.EffectElement.Gravity += 8;
                    }

                    if (Configuration.Battle.Speed == 2)
                    {
                        unit.AddDelayedModifier(SearchTargetAvailable, null);
                    }
                }
            }

            if (SpecialFilesTranceSeek.DebugBattle)
            {
                SpecialFilesTranceSeek.WriteDebugBattleFile();
                SpecialFilesTranceSeek.WriteDebugMonsterAttacks();
            }
        }

        public static void InitTSVariables()
        {
            foreach (BattleUnit unit in BattleState.EnumerateUnits())
            {
                var state = TranceSeekBattleDictionary.GetState(unit.Data);

                state.Zidane.FirstItemMug = RegularItem.NoItem;
                state.Zidane.SecondItemMug = RegularItem.NoItem;
                state.Vivi.PreviousSpell = BattleAbilityId.Void;
                state.ProtectStatus = new Dictionary<BattleStatus, int> { { (BattleStatus)0, 0 } };
                state.AbsorbElement = -1;
                state.Monster.DurationDeadlyStatus = 100;
                state.Monster.NerfGravity = 2;
                state.SpecialSA.Instinct = 2;
                state.SpecialSA.NewMaximumHP = (int)unit.MaximumHp;
                state.SpecialSA.NewMaximumMP = (int)unit.MaximumMp;
                state.SpecialItem.EmergencySatchel = 3;
                state.SpecialItem.MagicalSatchel = 3;

                InfusedWeaponScript.WeaponNewElement[unit.Data] = EffectElement.None;
                InfusedWeaponScript.WeaponNewCustomElement[unit.Data] = 0;
                InfusedWeaponScript.WeaponNewStatus[unit.Data] = 0;
                InfusedWeaponScript.CMDVanillaName[unit.Data] = new string[] { null, null };

                if (unit.PlayerIndex == CharacterId.Zidane)
                    SwitchWeaponScript.InitZidaneModel(unit);
            }
        }

        private Boolean FixCoverVisualForBrother(BattleUnit brother) // TO DELETE AFTER MEMORIA UPDATE
        {
            if (brother == null || Sister == null)
                return true;

            if (brother.IsCovering)
                Sister.Data.pos[2] = SisterPosition[2] + 400f;
            else if (Sister.Data.pos[2] != SisterPosition[2] && !btlseq.BtlSeqBusy())
                Sister.Data.pos[2] = SisterPosition[2];

            return true;
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

        private Boolean SearchTargetAvailable(BattleUnit unit) // Avoid Freyja to spam "Jump" against monsters on 1vs1, with almost same Speed.
        {
            Boolean TargetAvailable = false;
            for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next)
                if (next.bi.player == 1 && !btl_stat.CheckStatus(next, BattleStatus.Death | BattleStatus.Jump) && next.bi.target != 0)
                    TargetAvailable = true;

            if (TargetAvailable)
                return true;

            if (unit.CurrentAtb > ((4 * unit.MaximumAtb) / 5))
                unit.CurrentAtb = (short)(Math.Max(1, unit.CurrentAtb - (unit.MaximumAtb / 10)));

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

        public void ChangeDepthBBG(int depthvalue)
        {
            FF9StateBattleSystem ff9Battle = FF9StateSystem.Battle.FF9Battle;
            foreach (Renderer rend in ff9Battle.map.btlBGPtr.GetComponentsInChildren<Renderer>())
            {
                foreach (Material mat in rend.materials)
                    mat.SetInt("_ZWrite", depthvalue);
            }
        }

        public static Dictionary<KeyValuePair<Int32, Int32>, Int32> ChangeDepthBBGfromBattleID = new Dictionary<KeyValuePair<Int32, Int32>, Int32>
        {
            { new KeyValuePair<Int32, Int32>(326, 1), 1 } // Ze Big Bertha + Misty (reason : legs from Ze Big Bertha don't go on the water)
        };

        public static Dictionary<KeyValuePair<Int32, Int32>, String> CustomBBGfromBattleID = new Dictionary<KeyValuePair<Int32, Int32>, String>
        {
            { new KeyValuePair<Int32, Int32>(299, 1), "BBG_B023" }, // Lindblum boss (Steiner Quest)
            { new KeyValuePair<Int32, Int32>(838, 1), "BBG_B042" }, // Golden Pidove
            { new KeyValuePair<Int32, Int32>(84, 1), "BBG_B024" } // Armodullahan (Cinna Quest)
        };

        private static readonly HashSet<Int32> DarkBBG = new HashSet<Int32>(new[] { 4, 5, 6, 8, 9, 12, 13, 14, 15, 16, 32, 36, 37, 38, 39, 40, 41, 42, 45, 46, 67, 68, 69, 70, 71, 72,
        80, 84, 85, 86, 87, 88, 89, 90, 100, 114, 116, 117, 118, 119, 121, 122, 128, 131, 152, 153, 155, 156, 158, 162, 163, 164, 165, 167, 168, 169, 174, 175 });

        private static readonly Dictionary<RegularItem, Int32> ItemAffinitiesPoison = new Dictionary<RegularItem, Int32> // 0 = None, 1 = Weak, 2 = Half, 4 = Immune, 8 = Absorb
        {
            { RegularItem.MythrilArmlet, 2 },
            { RegularItem.GoldHelm, 2 },
            { TranceSeekRegularItem.UmbrellaShoes, 8 },
            { TranceSeekRegularItem.CursedCoin, 4 }
        };

        private static readonly Dictionary<RegularItem, Int32> ItemAffinitiesGravity = new Dictionary<RegularItem, Int32>
        {
            { RegularItem.PlatinaArmor, 2 },
            { RegularItem.DarkGear, 4 },
            { RegularItem.BlackBelt, 8 }
        };
    }
}
