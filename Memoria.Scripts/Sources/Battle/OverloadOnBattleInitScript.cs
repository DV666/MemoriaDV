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
using static Memoria.Scripts.TranceSeek.TranceSeekAPI;

namespace Memoria.Scripts.TranceSeek
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

            OverloadOnBattleScriptStartScript.InitProtectMessages();

            int BattleID = FF9StateSystem.Battle.battleMapIndex;
            int GroupeBattleID = FF9StateSystem.Battle.FF9Battle.btl_scene.PatNum;

            BTL_SCENE btl_scene = FF9StateSystem.Battle.FF9Battle.btl_scene;
            SB2_PATTERN sb2Pattern = FF9StateSystem.Battle.FF9Battle.btl_scene.PatAddr[GroupeBattleID];
            KeyValuePair<Int32, Int32> BattleExID = new KeyValuePair<Int32, Int32>(BattleID, GroupeBattleID);

            if (CustomBBGfromBattleID.TryGetValue(BattleExID, out string customBBG)) // Change BBG for specific, to have a better camera.
                ChangeBBG(customBBG);

            if (ChangeDepthBBGfromBattleID.TryGetValue(BattleExID, out int customDepth)) // Fix depth for specific battles, to avoid some weird graphical bugs.
                ChangeDepthBBG(customDepth);

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
            dictbattle[3] = 0; // Master Alchemy
            dictbattle[4] = 0; // Duelist
            dictbattle[5] = 0; // FlexibleLevel
            dictbattle[6] = 0; // CanCover

            if (!FF9StateSystem.EventState.gScriptDictionary.TryGetValue(1001, out Dictionary<Int32, Int32> dictdifficulty)) // Modificators from difficulties
            {
                dictdifficulty = new Dictionary<Int32, Int32>();
                FF9StateSystem.EventState.gScriptDictionary.Add(1001, dictdifficulty);
            }

            int difficultyMode = FF9StateSystem.EventState.gEventGlobal[1403];

            if (difficultyMode < 7)
            {
                for (int i = 0; i <= DifficultyParameters.Bonus_PowerAA; i++)
                    dictdifficulty[i] = 0;

                switch (difficultyMode)
                {
                    case 1: // Vivi mode
                        dictdifficulty[DifficultyParameters.Bonus_EXP] = 25;
                        dictdifficulty[DifficultyParameters.Bonus_Gil] = 25;
                        break;

                    case 3: // Kuja mode
                        if (FF9StateSystem.EventState.ScenarioCounter > 2250)
                        {
                            dictdifficulty[DifficultyParameters.Bonus_MaxHP] = 25;
                            dictdifficulty[DifficultyParameters.Bonus_Strength] = 25;
                            dictdifficulty[DifficultyParameters.Bonus_Magic] = 25;
                        }
                        else
                        {
                            dictdifficulty[DifficultyParameters.Bonus_MaxHP] = 5;
                            dictdifficulty[DifficultyParameters.Bonus_Strength] = 10;
                            dictdifficulty[DifficultyParameters.Bonus_Magic] = 10;
                        }
                        break;

                    case 4: // Necron mode
                        if (FF9StateSystem.EventState.ScenarioCounter > 2250)
                        {
                            dictdifficulty[DifficultyParameters.Bonus_MaxHP] = 80;
                            dictdifficulty[DifficultyParameters.Bonus_Strength] = 75;
                            dictdifficulty[DifficultyParameters.Bonus_Magic] = 75;
                            dictdifficulty[DifficultyParameters.Bonus_PowerAA] = 10;
                        }
                        else
                        {
                            dictdifficulty[DifficultyParameters.Bonus_MaxHP] = 20;
                            dictdifficulty[DifficultyParameters.Bonus_Strength] = 25;
                            dictdifficulty[DifficultyParameters.Bonus_Magic] = 25;
                            dictdifficulty[DifficultyParameters.Bonus_PowerAA] = 10;
                        }
                        dictdifficulty[12] = -90; // Gils Malus
                        break;

                    case 5: // Beatrix mode + Ozma Mode
                    case 6:
                        if (FF9StateSystem.EventState.ScenarioCounter > 2250)
                        {
                            dictdifficulty[DifficultyParameters.Bonus_MaxHP] = 50;
                            dictdifficulty[DifficultyParameters.Bonus_Strength] = 50;
                            dictdifficulty[DifficultyParameters.Bonus_Magic] = 50;
                            dictdifficulty[DifficultyParameters.Bonus_PowerAA] = 10;
                        }
                        else
                        {
                            dictdifficulty[DifficultyParameters.Bonus_MaxHP] = 10;
                            dictdifficulty[DifficultyParameters.Bonus_Strength] = 20;
                            dictdifficulty[DifficultyParameters.Bonus_Magic] = 20;
                            dictdifficulty[DifficultyParameters.Bonus_PowerAA] = 10;
                        }
                        break;
                }

                if (dictdifficulty[DifficultyParameters.Bonus_PowerAA] > 0)
                {
                    List<AA_DATA> attackList = FF9StateSystem.Battle.FF9Battle.enemy_attack;
                    for (int i = 0; i < attackList.Count; i++)
                    {
                        AA_DATA attack = attackList[i];
                        if ((attack.Type & 2) != 0) // Boost Power for monsters in Hardcore (using the Hide AP Figure, dummied for monsters)
                            attack.Ref.Power = attack.Ref.Power + Math.Max(1, (Int32)Math.Round((attack.Ref.Power * dictdifficulty[DifficultyParameters.Bonus_PowerAA]) / 100.0));
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
                    if (ItemAffinitiesPoison.TryGetValue(unit.Weapon, out int poisonWeapon) && StateDict.EffectElement.Poison < poisonWeapon)
                        StateDict.EffectElement.Poison = poisonWeapon;

                    if (ItemAffinitiesPoison.TryGetValue(unit.Head, out int poisonHead) && StateDict.EffectElement.Poison < poisonHead)
                        StateDict.EffectElement.Poison = poisonHead;

                    if (ItemAffinitiesPoison.TryGetValue(unit.Armor, out int poisonArmor) && StateDict.EffectElement.Poison < poisonArmor)
                        StateDict.EffectElement.Poison = poisonArmor;

                    if (ItemAffinitiesPoison.TryGetValue(unit.Wrist, out int poisonWrist) && StateDict.EffectElement.Poison < poisonWrist)
                        StateDict.EffectElement.Poison = poisonWrist;

                    if (ItemAffinitiesPoison.TryGetValue(unit.Accessory, out int poisonAccessory) && StateDict.EffectElement.Poison < poisonAccessory)
                        StateDict.EffectElement.Poison = poisonAccessory;

                    // Gravity element
                    if (ItemAffinitiesGravity.TryGetValue(unit.Weapon, out int gravityWeapon) && StateDict.EffectElement.Gravity < gravityWeapon)
                        StateDict.EffectElement.Gravity = gravityWeapon;

                    if (ItemAffinitiesGravity.TryGetValue(unit.Head, out int gravityHead) && StateDict.EffectElement.Gravity < gravityHead)
                        StateDict.EffectElement.Gravity = gravityHead;

                    if (ItemAffinitiesGravity.TryGetValue(unit.Armor, out int gravityArmor) && StateDict.EffectElement.Gravity < gravityArmor)
                        StateDict.EffectElement.Gravity = gravityArmor;

                    if (ItemAffinitiesGravity.TryGetValue(unit.Wrist, out int gravityWrist) && StateDict.EffectElement.Gravity < gravityWrist)
                        StateDict.EffectElement.Gravity = gravityWrist;

                    if (ItemAffinitiesGravity.TryGetValue(unit.Accessory, out int gravityAccessory) && StateDict.EffectElement.Gravity < gravityAccessory)
                        StateDict.EffectElement.Gravity = gravityAccessory;

                        
                    if (unit.HasSupportAbilityByIndex(TranceSeekSupportAbility.Alert_Boosted)) // Alert+
                    {
                        btl_stat.AlterStatus(unit, TranceSeekStatusId.PerfectDodge, parameters: "+2");
                    }
                    if (unit.HasSupportAbilityByIndex((SupportAbility)52)) // Last Stand
                    {
                        StateDict.SpecialSA.LastStand = unit.HasSupportAbilityByIndex(TranceSeekSupportAbility.LastStand_Boosted) ? 2 : 1;
                    }
                    if (unit.HasSupportAbilityByIndex(TranceSeekSupportAbility.ImAllSet_Boosted)) // SA I'm all set
                    {
                        unit.AlterStatus(TranceSeekStatus.ArmorUp, unit);
                        unit.AlterStatus(TranceSeekStatus.MentalUp, unit);
                    }
                    if (unit.HasSupportAbilityByIndex(TranceSeekSupportAbility.Millionaire_Boosted)) // Pluriche+
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
                    if (unit.Accessory == TranceSeekRegularItem.CaitsEye) // Cait's Eye
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
                    if (unit.Armor == TranceSeekRegularItem.GarlandsArmor) // Mechanical Armor
                    {
                        StateDict.SpecialItem.MechanicalArmor = 10;
                        btl_stat.AlterStatus(unit, TranceSeekStatusId.MechanicalArmor, parameters: StateDict.SpecialItem.MechanicalArmor);
                    }
                    if (unit.Accessory == TranceSeekRegularItem.IshgardScarf) // Ishgard Scarf
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
                    else if (unit.Accessory == TranceSeekRegularItem.StrangeCube) // Strange Cube
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

                        btl_stat.AlterStatus(unit, StrangeCubeStatuses[Comn.random16() % StrangeCubeStatuses.Length]);
                    }
                    else if (unit.Accessory == TranceSeekRegularItem.MagicLamp) // Magic Lamp
                    {
                        magiclampcooldown = (60 - unit.Will) * UnityEngine.Random.Range(1, 11) * 100;
                        unit.AddDelayedModifier(ProcessMagicLampRecast, null);
                    }
                    else if (unit.Accessory == TranceSeekRegularItem.CursedCoin) // Cursed Coin
                    {
                        if (DarkBBG.Contains(battlebg.nf_BbgNumber))
                        {
                            CharacterPresetId presetId = unit.Player.PresetId;
                            unit.ChangeToMonster("GZ_R002", 0, CharacterCommands.CommandSets[presetId].Regular[2], TranceSeekBattleCommand.Monster, false, false, false, false, false);
                            CharacterCommands.CommandSets[presetId].Regular[3] = BattleCommandId.None;
                        }
                    }
                    else if (unit.Accessory == TranceSeekRegularItem.RefinedMonocle && !RefinedMonocleTrigger) // Refined Monocle
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
                        var komradeAbilities = CharacterCommands.Commands[TranceSeekBattleCommand.Komrade].EnumerateAbilities();

                        int firstAAKomradeId = (int)komradeAbilities.First();
                        int totalAAKomrade = 2 * komradeAbilities.Count();

                        if (!FF9StateSystem.EventState.gScriptDictionary.TryGetValue(1030, out Dictionary<Int32, Int32> dict))
                        {
                            dict = new Dictionary<Int32, Int32>();
                            FF9StateSystem.EventState.gScriptDictionary.Add(1030, dict);
                        }

                        for (int i = 0; i < totalAAKomrade; i++)
                            dict[firstAAKomradeId + i] = 1;
                    }

                    int ID = 2000 + (int)unit.PlayerIndex;
                    if (FF9StateSystem.EventState.gScriptDictionary.ContainsKey(ID)) // Reset infused weapon.
                        FF9StateSystem.EventState.gScriptDictionary.Remove(ID);

                    if (FF9StateSystem.EventState.ScenarioCounter >= 11100 && FF9StateSystem.EventState.gEventGlobal[1500] == 0)
                    {
                        unit.AlterStatus(BattleStatus.Death, unit);
                        unit.CurrentHp = 0;
                    }
                    if (unit.HasSupportAbilityByIndex(TranceSeekSupportAbility.Anastrophe) && false) // SA Anastrophe
                    {
                        int factor = unit.HasSupportAbilityByIndex(TranceSeekSupportAbility.Anastrophe_Boosted) ? 1 : 2;
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

                    if (unit.HasSupportAbilityByIndex(TranceSeekSupportAbility.Protector_Boosted)) // SA Protector+
                    {
                        btl_stat.AlterStatus(unit, TranceSeekStatusId.PowerBreak, parameters: "+4");
                        btl_stat.AlterStatus(unit, TranceSeekStatusId.ArmorUp, parameters: "+4");
                    }
                    else if (unit.HasSupportAbilityByIndex(TranceSeekSupportAbility.Protector)) // SA Protector
                    {
                        btl_stat.AlterStatus(unit, TranceSeekStatusId.PowerBreak, parameters: "+2");
                        btl_stat.AlterStatus(unit, TranceSeekStatusId.ArmorUp, parameters: "+2");
                    }

                    if (unit.Row == 1)
                        unit.State().CanCover = 1;
                    else
                        unit.State().CanCover = 0;

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

                    TranceSeekRegularItem.CheckCreateVisualAccessory(unit);
                }
                else // Monsters init
                {
                    if (btl_scene.Info.StartType == battle_start_type_tags.BTL_START_FIRST_ATTACK)
                        StateDict.IsBackAttack = true;

                    BattleEnemy battleEnemy = BattleEnemy.Find(unit);

                    if (GameState.ModelKillCount(unit.Data.dms_geo_id) > 0 && (GameState.ModelKillCount(unit.Data.dms_geo_id) % 10) == 0)
                        OverTranceTrigger(unit);

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
                                unit.MaximumHp += (uint)((bonusHP * dictdifficulty[DifficultyParameters.Bonus_MaxHP]) / 100);
                                unit.CurrentHp = unit.MaximumHp;
                                StateDict.Monster.HPBoss10000 = true;
                            }
                            else
                            {
                                Log.Message($"[Trance Seek] Boss HP Bonus can't be applied on {unit.Name} : HP is under 10000.");
                                unit.MaximumHp += (uint)((bonusHP * dictdifficulty[DifficultyParameters.Bonus_MaxHP]) / 100);
                                unit.CurrentHp = unit.MaximumHp;
                            }

                        }
                        else
                        {
                            unit.MaximumHp += (uint)((bonusHP * dictdifficulty[DifficultyParameters.Bonus_MaxHP]) / 100);
                            unit.CurrentHp = unit.MaximumHp;
                        }

                        unit.MaximumMp += (uint)((unit.MaximumMp * dictdifficulty[DifficultyParameters.Bonus_MaxMP]) / 100);
                        unit.CurrentMp = unit.MaximumMp;

                        unit.Level = (byte)Math.Min((unit.Level + (unit.Level * dictdifficulty[DifficultyParameters.Bonus_Level]) / 100), byte.MaxValue);
                        unit.Dexterity = (byte)Math.Min((unit.Dexterity + (unit.Dexterity * dictdifficulty[DifficultyParameters.Bonus_Dexterity]) / 100), 50);
                        unit.Strength = (byte)Math.Min((unit.Strength + (unit.Strength * dictdifficulty[DifficultyParameters.Bonus_Strength]) / 100), byte.MaxValue);
                        unit.Magic = (byte)Math.Min((unit.Magic + (unit.Magic * dictdifficulty[DifficultyParameters.Bonus_Magic]) / 100), byte.MaxValue);
                        unit.Will = (byte)Math.Min((unit.Will + (unit.Will * dictdifficulty[DifficultyParameters.Bonus_Will]) / 100), 50);
                        unit.PhysicalDefence = (byte)Math.Min((unit.PhysicalDefence + (unit.PhysicalDefence * dictdifficulty[DifficultyParameters.Bonus_PhysicalDefence]) / 100), byte.MaxValue);
                        unit.PhysicalEvade = (byte)Math.Min((unit.PhysicalEvade + (unit.PhysicalEvade * dictdifficulty[DifficultyParameters.Bonus_PhysicalEvade]) / 100), byte.MaxValue);
                        unit.MagicDefence = (byte)Math.Min((unit.MagicDefence + (unit.MagicDefence * dictdifficulty[DifficultyParameters.Bonus_MagicDefence]) / 100), byte.MaxValue);
                        unit.MagicEvade = (byte)Math.Min((unit.MagicEvade + (unit.MagicEvade * dictdifficulty[DifficultyParameters.Bonus_MagicEvade]) / 100), byte.MaxValue);


                        battleEnemy.Data.bonus_exp = (uint)(battleEnemy.Data.bonus_exp + ((battleEnemy.Data.bonus_exp * dictdifficulty[DifficultyParameters.Bonus_EXP]) / 100));
                        battleEnemy.Data.bonus_gil = (uint)(battleEnemy.Data.bonus_gil + ((battleEnemy.Data.bonus_gil * dictdifficulty[DifficultyParameters.Bonus_Gil]) / 100));
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

                    // Pad 1 - Unused (2)
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
                state.Blank.SoakedBlade = RegularItem.NoItem;

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
                BattleAbilityId SummonChoosen = MagicLampSummons[Comn.random16() % MagicLampSummons.Length];
                ushort target = 0;

                if (SummonChoosen == TranceSeekBattleAbility.RubyLight || SummonChoosen == TranceSeekBattleAbility.EmeraldLight || SummonChoosen == TranceSeekBattleAbility.PearlLight || SummonChoosen == TranceSeekBattleAbility.DiamondLight) // Carbuncle
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
            { TranceSeekRegularItem.CursedCoin, 4 },
            { TranceSeekRegularItem.Onyxarmor, 2 }
        };


        private static readonly Dictionary<RegularItem, Int32> ItemAffinitiesGravity = new Dictionary<RegularItem, Int32>
        {
            { RegularItem.PlatinaArmor, 2 },
            { RegularItem.DarkGear, 4 },
            { RegularItem.BlackBelt, 8 },
            { TranceSeekRegularItem.Onyxarmor, 4 }
        };

        public static class DifficultyParameters
        {
            public const int Bonus_MaxHP = 0;
            public const int Bonus_MaxMP = 1;
            public const int Bonus_Level = 2;
            public const int Bonus_Dexterity = 3;
            public const int Bonus_Strength = 4;
            public const int Bonus_Magic = 5;
            public const int Bonus_Will = 6;
            public const int Bonus_PhysicalDefence = 7;
            public const int Bonus_PhysicalEvade = 8;
            public const int Bonus_MagicDefence = 9;
            public const int Bonus_MagicEvade = 10;
            public const int Bonus_EXP = 11;
            public const int Bonus_Gil = 12;
            public const int Bonus_PowerAA = 13;
        }

        private static readonly BattleStatusId[] StrangeCubeStatuses =
{
            BattleStatusId.Poison, BattleStatusId.Venom, BattleStatusId.Blind, BattleStatusId.Silence, BattleStatusId.Trouble,
            BattleStatusId.Sleep, BattleStatusId.Freeze, BattleStatusId.Heat, BattleStatusId.Doom, BattleStatusId.Mini, BattleStatusId.Petrify, BattleStatusId.GradualPetrify,
            BattleStatusId.Berserk, BattleStatusId.Confuse, BattleStatusId.Stop, BattleStatusId.Zombie, BattleStatusId.Slow, TranceSeekStatusId.Vieillissement,
            TranceSeekStatusId.ArmorBreak, TranceSeekStatusId.MagicBreak, TranceSeekStatusId.MentalBreak, TranceSeekStatusId.PowerBreak, BattleStatusId.Virus, BattleStatusId.Regen,
            BattleStatusId.Haste, BattleStatusId.Float, BattleStatusId.Shell, BattleStatusId.Protect, BattleStatusId.Vanish, BattleStatusId.Reflect, TranceSeekStatusId.PowerUp,
            TranceSeekStatusId.MagicUp, TranceSeekStatusId.ArmorUp, TranceSeekStatusId.MentalUp, TranceSeekStatusId.PowerBreak, TranceSeekStatusId.Dragon, TranceSeekStatusId.Bulwark,
            TranceSeekStatusId.PerfectDodge, TranceSeekStatusId.PerfectCrit
        };

        private static readonly BattleAbilityId[] MagicLampSummons =
        {
            BattleAbilityId.DiamondDust, BattleAbilityId.FlamesofHell, BattleAbilityId.JudgementBolt, BattleAbilityId.WormHole,
            BattleAbilityId.Zantetsuken, BattleAbilityId.Tsunami, BattleAbilityId.MegaFlare, BattleAbilityId.EternalDarkness,
            TranceSeekBattleAbility.TerrestrialRage, TranceSeekBattleAbility.MillennialDecay, TranceSeekBattleAbility.RubyLight, TranceSeekBattleAbility.EmeraldLight, TranceSeekBattleAbility.PearlLight,
            TranceSeekBattleAbility.DiamondLight, TranceSeekBattleAbility.RebirthFlame, TranceSeekBattleAbility.TerraHoming
        };
    }
}




