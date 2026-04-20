using FF9;
using Memoria.Assets;
using Memoria.Data;
using Memoria.Database;
using Memoria.Prime;
using System;
using System.Collections.Generic;
using System.Linq;
using static Memoria.Scripts.TranceSeek.TranceSeekAPI;
using static Memoria.Scripts.TranceSeek.TranceSeekBattleDictionary;

namespace Memoria.Scripts.TranceSeek
{
    public class OverloadOnBattleScriptStartScript : IOverloadOnBattleScriptStartScript
    {

        public static readonly BattleStatus[] CustomStatusAAMonster = // Custom status for monsters (using the Animation 2 value in HW) 
        {
            TranceSeekStatus.PowerBreak,    // Bit 0 (valeur 1)
            TranceSeekStatus.MagicBreak,    // Bit 1 (valeur 2)
            TranceSeekStatus.ArmorBreak,    // Bit 2 (valeur 4)
            TranceSeekStatus.MentalBreak,   // Bit 3 (valeur 8)
            TranceSeekStatus.PowerUp,       // Bit 4 (valeur 16)
            TranceSeekStatus.MagicUp,       // Bit 5 (valeur 32)
            TranceSeekStatus.ArmorUp,       // Bit 6 (valeur 64)
            TranceSeekStatus.MentalUp,      // Bit 7 (valeur 128)
            TranceSeekStatus.Bulwark,       // Bit 8 (valeur 256) -> CustomStatus13
            TranceSeekStatus.PerfectDodge,  // Bit 9 (valeur 512) -> CustomStatus14
            TranceSeekStatus.PerfectCrit,   // Bit 10 (valeur 1024) -> CustomStatus15
            TranceSeekStatus.Vieillissement,// Bit 11 (valeur 2048) -> CustomStatus16
            TranceSeekStatus.Charm,         // Bit 12 (valeur 4096) -> CustomStatus23
            TranceSeekStatus.Dragon         // Bit 13 (valeur 8192) -> CustomStatus9
        };

        public Boolean OnBattleScriptStart(BattleCalculator v)
        {
            var Caster_TSVar = v.CasterState();
            var Target_TSVar = v.TargetState();

            if (Target_TSVar.Monster.HPBoss10000 && v.Target.CurrentHp <= 10000) // Prevent boss to die => Maybe use CustomBattleFlagsMeaning ?
                v.Target.CurrentHp = 10000;

            if (Caster_TSVar.IsBackAttack)
                Caster_TSVar.IsBackAttack = false;

            if (DifficultyDebugMenu.MegaCheat > 0)
            {
                foreach (BattleUnit unit in BattleState.EnumerateUnits())
                    if (unit.IsPlayer)
                    {
                        unit.MagicDefence = 254;
                        unit.PhysicalDefence = 254;
                    }
            }

            if (FF9StateSystem.Battle.battleMapIndex == 52 && FF9StateSystem.Battle.FF9Battle.btl_scene.PatNum == 0 && FF9StateSystem.EventState.gEventGlobal[1305] > 0 && v.Caster.IsPlayer && v.Command.Id == BattleCommandId.Attack && v.Caster.Data != v.Target.Data)
            { // Black Waltz 3 Broken (Polarity Mechanic)
                if (!TranceSeekSpecial.PolaritySPS.TryGetValue(v.Target.Data, out SPSEffect spsc))
                    TranceSeekSpecial.PolaritySPS[v.Caster.Data] = null;

                if (!TranceSeekSpecial.PolaritySPS.TryGetValue(v.Target.Data, out SPSEffect spst))
                    TranceSeekSpecial.PolaritySPS[v.Target.Data] = null;

                if (TranceSeekSpecial.PolaritySPS[v.Target.Data] != null && TranceSeekSpecial.PolaritySPS[v.Caster.Data] != null)
                {
                    TranceSeekSpecial.PolaritySPS[v.Target.Data].attr = 0;
                    TranceSeekSpecial.PolaritySPS[v.Target.Data].meshRenderer.enabled = false;
                    TranceSeekSpecial.PolaritySPS[v.Target.Data] = null;
                    v.Context.Flags |= BattleCalcFlags.Guard;
                    Caster_TSVar.Monster.NoDodge = true; // Don't miss the attack.
                    btl_stat.RemoveStatus(v.Target, BattleStatusId.Haste);
                    FF9StateSystem.EventState.gEventGlobal[1305] = 0;
                    UIManager.Battle.SetBattleFollowMessage(3, Localization.GetWithDefault("PolarityOFF"));
                    foreach (BattleUnit unit in FF9StateSystem.Battle.FF9Battle.EnumerateBattleUnits())
                    {
                        if (unit.Data.dms_geo_id == 331) // Electric Shield Mob
                            unit.KillStandardCommands();
                    }
                }
            }

            if (!v.Caster.IsPlayer && FF9StateSystem.Battle.battleMapIndex == 84 && FF9StateSystem.Battle.FF9Battle.btl_scene.PatNum == 1) // Armodullahan V2
            {
                if (v.Command.RawIndex != 54 && v.Command.RawIndex != 55 && v.Command.RawIndex != 71 && v.Command.RawIndex != 72 && v.Command.RawIndex != 26 && v.Command.RawIndex != 27 && v.Command.Power > 0)
                    v.Command.Power += 20;
            }

            if (!v.Caster.IsPlayer && v.Command.Data.aa.Vfx2 > 0 && v.Command.ScriptId != 79) // Animation 2 (use a CustomStatus or specify a DragonSkill)
            {
                ulong AACustomStatus = v.Command.Data.aa.Vfx2;

                for (int i = 0; i < CustomStatusAAMonster.Length; i++)
                    if ((AACustomStatus & (1UL << i)) != 0)
                        v.Command.AbilityStatus |= CustomStatusAAMonster[i];
            }

            if (v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.EXMode) && !Caster_TSVar.SpecialSA.ModeEX && v.Caster.IsUnderAnyStatus(BattleStatus.Trance)) // Mode EX
            {
                Int32 HealHPSAOrItem = (int)(v.Caster.MaximumHp * (v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.EXMode_Boosted) ? 16 : 8) / 100);
                Int32 HealMPSAOrItem = (int)(v.Caster.MaximumMp * (v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.EXMode_Boosted) ? 16 : 8) / 100);
                Caster_TSVar.SpecialSA.ModeEX = true;
                v.Caster.AddDelayedModifier(
                caster => caster.CurrentAtb >= caster.MaximumAtb,
                caster =>
                {
                    Caster_TSVar.SpecialSA.ModeEX = false;
                    if (HealHPSAOrItem > 0)
                    {
                        caster.CurrentHp = Math.Min(caster.CurrentHp + (uint)HealHPSAOrItem, caster.MaximumHp);
                    }
                    if (HealMPSAOrItem > 0)
                    {
                        caster.CurrentMp = Math.Min(caster.CurrentMp + (uint)HealMPSAOrItem, caster.MaximumMp);
                    }
                    btl2d.Btl2dStatReq(caster, -HealHPSAOrItem, -HealMPSAOrItem);
                }
                );
            }
            if (v.Caster.Data.dms_geo_id == 410 && !v.Caster.IsPlayer) // Refresh Lani boss version animations (after Runic)
            {
                v.Caster.Data.mot[0] = "ANH_MON_B3_122_000";
                v.Caster.Data.mot[2] = "ANH_MON_B3_122_003";
                btl_stat.MakeStatusesPermanent(v.Caster, TranceSeekStatus.Runic, false);
                btl_stat.MakeStatusesPermanent(v.Caster, BattleStatus.Defend, false);
            }
            if (v.Caster.PlayerIndex == (CharacterId)12 && v.Command.Data.info.effect_counter == 1 && !v.Caster.InTrance) // Lani's Rage Mechanic
            {
                switch (v.Command.AbilityId)
                {
                    case TranceSeekBattleAbility.Combo: // Combo
                    case TranceSeekBattleAbility.MadRush: // Mad Rush
                    case TranceSeekBattleAbility.Flametongue: // Flame Tongue
                    case TranceSeekBattleAbility.IceBrand: // Ice Brand
                    case TranceSeekBattleAbility.ThunderBlade: // Thunder Blade
                    case TranceSeekBattleAbility.LiquidSteel: // Liquid Steel
                    {
                        btl_stat.AlterStatus(v.Caster, TranceSeekStatusId.Rage, parameters: "-1");
                        break;
                    }
                    case TranceSeekBattleAbility.Carnage: // Carnage
                    case TranceSeekBattleAbility.Hatred: // Hatred
                    case TranceSeekBattleAbility.AgnisBlade: // Agni's Blade
                    case TranceSeekBattleAbility.ShivaBlade: // Shiva Blade
                    case TranceSeekBattleAbility.IndraBlade: // Indra Blade
                    case TranceSeekBattleAbility.VarunaBlade: // Varuna Blade
                    {
                        btl_stat.AlterStatus(v.Caster, TranceSeekStatusId.Rage, parameters: "-3");
                        break;
                    }
                    case TranceSeekBattleAbility.Ripping: // Ripping
                    case TranceSeekBattleAbility.SuperMuscles: // Super Muscles
                    case TranceSeekBattleAbility.PrithviBlade: // Prithvi Blade
                    {
                        btl_stat.AlterStatus(v.Caster, TranceSeekStatusId.Rage, parameters: "-5");
                        break;
                    }
                }
            }
            if (v.Caster.PlayerIndex == CharacterId.Cinna) // Cinna's Mechanic
            {
                int InventionsCD = 0; // For Genie/Eureka mechanic.

                foreach (BattleAbilityId AA in InventionAAs)
                {
                    if (FF9StateSystem.Battle.FF9Battle.aa_data.TryGetValue(AA, out AA_DATA aaData) && aaData != null)
                        if (aaData.MP > 0)
                        {
                            aaData.MP--;
                            if (AA != TranceSeekBattleAbility.Idea && AA != TranceSeekBattleAbility.Eureka) // Genie & Eureka
                                InventionsCD++;
                        }
                }

                Caster_TSVar.Cinna.InventionCoolDown = InventionsCD;

                if (v.Command.AbilityId == TranceSeekBattleAbility.Acceleratorhammer || v.Command.Id == BattleCommandId.Attack && v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Mecano)) // Accelerator hammer / SA Mecano
                {
                    List<AA_DATA> AAlist = new List<AA_DATA>();

                    for (Int32 i = 0; i < 8; i++)
                    {
                        int idAA = 1136 + i;
                        if (FF9StateSystem.Battle.FF9Battle.aa_data[(BattleAbilityId)idAA].MP > 0)
                            AAlist.Add(FF9StateSystem.Battle.FF9Battle.aa_data[(BattleAbilityId)idAA]);
                    }
                    if (AAlist.Count > 0)
                    {
                        AA_DATA AAChoosen = AAlist[GameRandom.Next16() % AAlist.Count];
                        AAChoosen.MP--;
                        if (v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Mecano_Boosted) && AAlist.Count > 0) // SA Mecano++
                        {
                            AAlist.Remove(AAChoosen);
                            if (AAlist.Count > 0)
                                AAlist[GameRandom.Next16() % AAlist.Count].MP--;
                        }
                    }
                }

                if (FF9StateSystem.Battle.FF9Battle.aa_data[v.Command.AbilityId].MP > 0 && v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.EmergencPlan))
                {
                    v.Caster.CurrentMp = 0;
                    btl2d.Btl2dReqSymbolMessage(v.Caster.Data, "[FFFF00]", MessageEmergencyPlan, HUDMessage.MessageStyle.DAMAGE, 10);
                }

                if (v.Command.Id == TranceSeekBattleCommand.Engineer || v.Command.Id == TranceSeekBattleCommand.Idea || v.Command.Id == TranceSeekBattleCommand.Eureka)  // CMD Invention
                {
                    int mpCost = 0;
                    KeyValuePair<RegularItem, BattleAbilityId> PassiveHammer = new KeyValuePair<RegularItem, BattleAbilityId>(v.Caster.Weapon, v.Command.AbilityId);
                    if ((GameRandom.Next8() % 100) < 25 && InventionPreserved.TryGetValue(v.Caster.Weapon, out BattleAbilityId preservedAA) && preservedAA == v.Command.AbilityId)
                        btl2d.Btl2dReqSymbolMessage(v.Caster.Data, "[FFC000]", MessagePreserved, HUDMessage.MessageStyle.DAMAGE, 10);
                    else
                    {
                        switch (v.Command.AbilityId)
                        {
                            case TranceSeekBattleAbility.Hammerthrow: // Hammer throw
                                mpCost = 2;
                                break;
                            case TranceSeekBattleAbility.Springboots: // Spring boots
                                mpCost = 3;
                                break;
                            case TranceSeekBattleAbility.Acceleratorhammer: // Accelerator hammer
                                mpCost = 4;
                                break;
                            case TranceSeekBattleAbility.Criticalaim: // Critical aim
                                mpCost = 5;
                                break;
                            case TranceSeekBattleAbility.Electroshock: // Electroshock
                                mpCost = 6;
                                break;
                            case TranceSeekBattleAbility.Flurryofhammers: // Flurry of hammers
                                mpCost = 7;
                                break;
                            case TranceSeekBattleAbility.AdjustableWrench: // Adjustable Wrench
                                mpCost = 8;
                                break;
                            case TranceSeekBattleAbility.HymnoftheTantalas: // Hymn of the Tantalas
                                mpCost = 9;
                                break;
                            case TranceSeekBattleAbility.Idea: // Idea
                                mpCost = 10;
                                break;
                            case TranceSeekBattleAbility.Eureka: // Eureka
                                mpCost = 6;
                                break;
                        }
                        FF9StateSystem.Battle.FF9Battle.aa_data[v.Command.AbilityId].MP = mpCost;
                    }
                }
            }
            if (Caster_TSVar.Cinna.SpringBoots > 0) // AA SpringBoots
            {
                v.Caster.AddDelayedModifier(
                    caster => caster.CurrentAtb >= caster.MaximumAtb,
                    caster =>
                    {
                        if (!caster.IsUnderAnyStatus(BattleStatusConst.StopAtb) && caster.CurrentAtb < (4 * caster.MaximumAtb / 5))
                            caster.CurrentAtb += (Int16)(4 * caster.MaximumAtb / 5);
                        Caster_TSVar.Cinna.SpringBoots--;
                    }
                );
            }

            if (v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Flexible) && v.Caster.PlayerIndex == CharacterId.Zidane && Caster_TSVar.Zidane.DaggerAttack == 0
                && v.Command.Id != BattleCommandId.Counter && v.Command.Id != BattleCommandId.RushAttack && v.Command.Data.info.effect_counter == 1) // SA Flexible
            {
                // Permanent [code=Condition] WeaponId == 1 || WeaponId == 2 || WeaponId == 3 || WeaponId == 1153 || WeaponId == 1155 || WeaponId == 1158 || WeaponId == 1161 || WeaponId == 1164 || WeaponId == 1167 [/code] [code=BanishSAByLvl] 203 ; -1 [/code]
                if (!BlackListedWeaponForFlexible.Contains(v.Caster.Weapon))
                {
                    if (Caster_TSVar.Zidane.FlexibleLvl > 0)
                        Caster_TSVar.Zidane.FlexibleLvl = 0;

                    Caster_TSVar.Zidane.Flexible++;
                    int FlexibleTurn = v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Flexible_Boosted) ? 2 : 4;
                    if (Caster_TSVar.Zidane.Flexible >= FlexibleTurn)
                    {
                        Caster_TSVar.Zidane.Flexible = 0;
                        if (btl_util.getSerialNumber(v.Caster.Data) == CharacterSerialNumber.ZIDANE_SWORD)
                            BattleState.EnqueueCounter(v.Caster, BattleCommandId.RushAttack, TranceSeekBattleAbility.Thief, v.Caster.Id);
                        else
                            BattleState.EnqueueCounter(v.Caster, BattleCommandId.RushAttack, TranceSeekBattleAbility.Bandit, v.Caster.Id);

                        if (v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Flexible_Boosted))
                            Caster_TSVar.Zidane.FlexibleLvl = 2;
                        else
                            Caster_TSVar.Zidane.FlexibleLvl = 1;
                    }
                }
            }

            if (v.Command.AbilityStatus > 0 && Target_TSVar.ProtectStatus.Count > 1)
            {
                foreach (BattleStatusId statusID in v.Command.AbilityStatus.ToStatusList())
                {
                    BattleStatus status = statusID.ToBattleStatus();

                    if (Target_TSVar.ProtectStatus.TryGetValue(status, out int protectCount) && protectCount > 0)
                    {
                        v.Command.AbilityStatus &= ~status;

                        if (protectCount != 255)
                        {
                            Target_TSVar.ProtectStatus[status]--;
                            string message = $"-{status}";
                            if (ProtectMessages.TryGetValue(status, out var localizedStatusProtect))
                            {
                                btl2d.Btl2dReqSymbolMessage(v.Target.Data, "[38FF1F]", localizedStatusProtect, HUDMessage.MessageStyle.DAMAGE, 5);
                            }
                        }
                    }
                }
            }

            if (v.Command.Id == TranceSeekBattleCommand.Alchemy2) // CMD Mixing
            {
                int TranceDelta = v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Artificer_Boosted) ? 42 : v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Artificer) ? 64 : 128;
                v.Caster.Trance = (byte)Math.Max(0, v.Caster.Trance - TranceDelta);
            }

            if (v.Caster.PlayerIndex == CharacterId.Marcus && v.Caster.InTrance) // Refresh Trance data in Player for Marcus, for AbilityFeatures.txt purpose.
            {
                v.Caster.AddDelayedModifier(
                    caster => caster.CurrentAtb >= caster.MaximumAtb,
                    caster =>
                    {
                        caster.Player.trance = caster.Trance;
                    }
                );
            }

            if (v.Command.Id == TranceSeekBattleCommand.Witchcraft && !v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Wizard_Boosted)) // Witchcraft (Vivi's SA)
                v.Command.HitRate /= 2;

            if (v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Propagation) && v.Command.IsManyTarget && v.Command.AbilityId >= TranceSeekBattleAbility.Regen_Multi && v.Command.AbilityId <= TranceSeekBattleAbility.AngelWhisper_Multi)
            {
                if (v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Propagation_Boosted))
                    v.Command.HitRate = (v.Command.HitRate * 3) / 4;
                else
                    v.Command.HitRate /= 2;

                if (Caster_TSVar.SpecialSA.Propagation > 0)
                {
                    int CostMP = FF9StateSystem.Battle.FF9Battle.aa_data[v.Command.AbilityId].MP;
                    if (v.Caster.CurrentMp < CostMP)
                        v.Context.Flags |= (TranceSeekBattleCalcFlags.PropagationFail | BattleCalcFlags.Miss);
                    else
                        v.Caster.CurrentMp -= (uint)CostMP;
                }
                Caster_TSVar.SpecialSA.Propagation++;

                v.Caster.AddDelayedModifier(
                    caster => caster.CurrentAtb >= caster.MaximumAtb,
                    caster =>
                    {
                        Caster_TSVar.SpecialSA.Propagation = 0;
                    }
                );
            }

            if (v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.EnchantedBlade)) // SA Enchanted blade
            {
                if (v.Command.Id == BattleCommandId.MagicSword)
                {
                    int StatusInfused = -1;
                    int ElementInfused = (int)v.Command.Element;
                    if (v.Command.AbilityStatus > 0)
                        StatusInfused = (int)v.Command.AbilityStatus;

                    if (v.Command.AbilityId == TranceSeekBattleAbility.MagicSword_Poison || v.Command.AbilityId == TranceSeekBattleAbility.MagicSword_Arsenic || v.Command.AbilityId == BattleAbilityId.BioSword)
                        ElementInfused = 256;
                    else if (v.Command.AbilityId == TranceSeekBattleAbility.MagicSword_Quarter || v.Command.AbilityId == TranceSeekBattleAbility.MagicSword_Demi || v.Command.AbilityId == TranceSeekBattleAbility.MagicSword_Gravija)
                        ElementInfused = 512;

                    InfusedWeaponScript.InfuseWeapon(v, v.Caster.Data, ElementInfused, StatusInfused);
                    Caster_TSVar.Steiner.SteinerEnchantedBlade = v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.EnchantedBlade_Boosted) ? 2 : 1;
                }
                else if (Caster_TSVar.Steiner.SteinerEnchantedBlade > 0)
                {
                    Caster_TSVar.Steiner.SteinerEnchantedBlade--;
                    if (Caster_TSVar.Steiner.SteinerEnchantedBlade <= 0)
                    {
                        InfusedWeaponScript.WeaponNewElement[v.Caster.Data] = 0;
                        InfusedWeaponScript.WeaponNewCustomElement[v.Caster.Data] = 0;
                        InfusedWeaponScript.WeaponNewStatus[v.Caster.Data] = 0;
                        InfusedWeaponScript.ClearInfuseWeapon(v.Caster.Data);
                    }
                }
            }

            if (v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.PeaceOfMind) && v.Command.Data.info.effect_counter == 1) // SA Enchanted blade
            {
                if ((v.Command.AbilityId == BattleAbilityId.Regen || v.Command.AbilityId == TranceSeekBattleAbility.Regen_Multi) && v.Caster.Weapon == RegularItem.Hamelin ||
                    (v.Command.AbilityId == BattleAbilityId.Life || v.Command.AbilityId == TranceSeekBattleAbility.Life_Multi) && v.Caster.Weapon == RegularItem.SirenFlute ||
                    v.Command.AbilityId == BattleAbilityId.Berserk && v.Caster.Weapon == RegularItem.LamiaFlute ||
                    (v.Command.AbilityId == BattleAbilityId.Protect || v.Command.AbilityId == TranceSeekBattleAbility.Protect_Multi) && v.Caster.Weapon == RegularItem.GolemFlute ||
                    (v.Command.AbilityId == BattleAbilityId.Haste || v.Command.AbilityId == TranceSeekBattleAbility.Haste_Multi) && v.Caster.Weapon == RegularItem.FairyFlute)
                {
                    v.Caster.Flags |= CalcFlag.HpDamageOrHeal;
                    v.Caster.HpDamage = v.Command.Data.aa.MP * 10;
                    if (v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.PeaceOfMind_Boosted)) // SA Enchanted blade
                    {
                        v.Caster.Flags |= CalcFlag.MpDamageOrHeal;
                        v.Caster.MpDamage = v.Command.Data.aa.MP;
                    }
                }
            }

            if (v.Caster.PlayerIndex == CharacterId.Beatrix) // Redemption mechanic
            {
                if (!Caster_TSVar.Beatrix.RedemptionTrigger && v.Caster.PlayerIndex == CharacterId.Beatrix)
                {
                    if (v.Command.Id == BattleCommandId.Attack || v.Command.Id == BattleCommandId.Defend || v.Command.Id == BattleCommandId.Counter && v.Command.AbilityId == BattleAbilityId.Attack ||
                        v.Command.Id == BattleCommandId.HolyWhiteMagic || v.Caster.IsUnderAnyStatus(BattleStatus.Trance))
                    {
                        v.Caster.AlterStatus(TranceSeekStatus.Redemption, v.Caster);
                    }
                    else if (v.Command.Id == BattleCommandId.HolySword1 || v.Command.Id == BattleCommandId.Counter && !v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Dominance_Boosted) &&
                        (v.Command.AbilityId == BattleAbilityId.ThunderSlash || v.Command.AbilityId == BattleAbilityId.StockBreak || v.Command.AbilityId == BattleAbilityId.Climhazzard || v.Command.AbilityId == BattleAbilityId.Shock
                        | v.Command.AbilityId == TranceSeekBattleAbility.Cleave || v.Command.AbilityId == TranceSeekBattleAbility.Braver || v.Command.AbilityId == TranceSeekBattleAbility.FourfoldFlurry || v.Command.AbilityId == TranceSeekBattleAbility.TriShock)) // SA Dominance+
                    {
                        if (v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Amnesty) && (v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Amnesty_Boosted) ? 50 : 25) < Comn.random16() % 100)
                        {
                            btl_stat.AlterStatus(v.Caster, TranceSeekStatusId.Redemption, v.Caster, parameters: "Remove");
                        }
                        else
                        {
                            v.Caster.RemoveStatus(TranceSeekStatus.Redemption);
                        }
                    }
                    Caster_TSVar.Beatrix.RedemptionTrigger = true;
                    TranceSeekCharacterMechanic.UpdateRedemptionHUD(v.Caster);
                    v.Caster.AddDelayedModifier(
                        caster => caster.CurrentAtb >= caster.MaximumAtb,
                        caster =>
                        {
                            Caster_TSVar.Beatrix.RedemptionTrigger = false;
                        }
                    );
                }
            }
            else if (!Target_TSVar.Beatrix.RedemptionTrigger && v.Target.PlayerIndex == CharacterId.Beatrix && v.Target.IsCovering)
            {
                v.Target.AlterStatus(TranceSeekStatus.Redemption, v.Caster);
                Target_TSVar.Beatrix.RedemptionTrigger = true;
                TranceSeekCharacterMechanic.UpdateRedemptionHUD(v.Target);
                v.Caster.AddDelayedModifier(
                    caster => caster.CurrentAtb >= caster.MaximumAtb,
                    caster =>
                    {
                        Target_TSVar.Beatrix.RedemptionTrigger = false;
                    }
                );
            }

            if (v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Offering) && v.Command.Data.info.effect_counter == 1 && v.Command.Id != BattleCommandId.Counter && v.Caster.CurrentMp < v.Caster.MaximumMp) // SA Offering
            {
                v.Caster.AddDelayedModifier(
                    caster => caster.CurrentAtb < (9 * caster.MaximumAtb) / 10 || caster.CurrentAtb >= caster.MaximumAtb,
                    caster =>
                    {
                        int HPDamage = (int)(caster.MaximumHp / 10);
                        int MPRecover = (int)(caster.MaximumMp / (v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Offering_Boosted) ? 10 : 20));
                        if (HPDamage > 0)
                        {
                            caster.CurrentHp = Math.Max(caster.CurrentHp - (uint)HPDamage, 0);
                        }
                        if (MPRecover > 0)
                        {
                            caster.CurrentMp = Math.Min(caster.CurrentMp + (uint)MPRecover, caster.MaximumMp);
                        }
                        btl2d.Btl2dStatReq(caster.Data, HPDamage, -MPRecover);
                    }
                );
            }

            if (v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Econome) && v.Command.Id == BattleCommandId.Item) // SA Econome
            {
                if ((v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Econome_Boosted) ? 50 : 25) < Comn.random16() % 100)
                    ff9item.FF9Item_Add(v.Command.ItemId, 1);
            }

            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.InTopForm)) // SA In top form!
            {
                v.Caster.AddDelayedModifier(
                    caster => caster.CurrentAtb >= caster.MaximumAtb,
                    caster =>
                    {
                        int RatioHP = (int)((v.Target.CurrentHp * 100) / v.Target.MaximumHp);
                        v.Target.PhysicalDefence = (Target_TSVar.Baku.InTopForm * ((v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.InTopForm_Boosted) ? 50 : 25) + RatioHP)) / 100;
                    }
                );
            }

            if (v.Caster.PlayerIndex == (CharacterId)14)
            {
                CharacterPresetId presetId = v.Caster.Player.PresetId;
                v.Caster.SummonCount++;
                if (v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.TakeThat_Boosted) || v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.TakeThat) && v.Caster.SummonCount % 2 == 0) // SA Take that!
                    CharacterCommands.CommandSets[presetId].Regular[0] = (BattleCommandId)(UnityEngine.Random.Range(1042, 1045));
                else
                    CharacterCommands.CommandSets[presetId].Regular[0] = (BattleCommandId)(UnityEngine.Random.Range(1038, 1041));
            }

            // [TODO] To remove when this function will be fixed (in my PR https://github.com/Albeoris/Memoria/pull/1255 or before)

            Int32 Castercounter = 5;

            v.Caster.AddDelayedModifier(
                caster => (Castercounter -= 1) > 0,
                caster =>
                {
                    OverloadOnBattleScriptEndScript.OnBattleScriptEnd(v); 
                }
            );

            if (Configuration.Battle.Speed == 2)
            {
                TranceSeekCharacterMechanic.EikoMougMechanic(v);
            }
            else // [TODO] Can be improved ?
            {
                Int32 counter = 15;
                v.Caster.AddDelayedModifier(
                    caster => (counter -= BattleState.ATBTickCount) > 0,
                    caster =>
                    {
                        TranceSeekCharacterMechanic.EikoMougMechanic(v);
                    }
                );
            }
            return false;
        }

        private static readonly Dictionary<BattleStatus, Dictionary<string, string>> ProtectMessages = new Dictionary<BattleStatus, Dictionary<string, string>>();

        public static void InitProtectMessages()
        {
            foreach (BattleStatus status in Enum.GetValues(typeof(BattleStatus))) // Don't work with customstatus... maybe later, with a Memoria update ? (or made it here)
            {
                string message = $"-{status}";
                ProtectMessages[status] = new Dictionary<string, string>
                {
                    { "US", message }, { "UK", message }, { "JP", message },
                    { "ES", message }, { "FR", message }, { "GR", message }, { "IT", message }
                };
            }
        }

        private static readonly BattleAbilityId[] InventionAAs =
        {
            TranceSeekBattleAbility.Hammerthrow, TranceSeekBattleAbility.Springboots, TranceSeekBattleAbility.Acceleratorhammer, TranceSeekBattleAbility.Criticalaim,
            TranceSeekBattleAbility.Electroshock, TranceSeekBattleAbility.Flurryofhammers, TranceSeekBattleAbility.AdjustableWrench, TranceSeekBattleAbility.HymnoftheTantalas,
            TranceSeekBattleAbility.Idea, TranceSeekBattleAbility.Eureka
        };

        private static readonly RegularItem[] BlackListedWeaponForFlexible =
{
            RegularItem.Dagger, RegularItem.MageMasher, RegularItem.MythrilDagger, TranceSeekRegularItem.CryptDagger,
            TranceSeekRegularItem.ViceDagger, TranceSeekRegularItem.LamiaDagger, TranceSeekRegularItem.TwinLance, TranceSeekRegularItem.OgraSurin, TranceSeekRegularItem.ImpDagger
        };

        public static readonly Dictionary<RegularItem, BattleAbilityId> InventionPreserved = new Dictionary<RegularItem, BattleAbilityId>
        {
            { TranceSeekRegularItem.SteelHammer, TranceSeekBattleAbility.Hammerthrow }, // Steel Hammer
            { TranceSeekRegularItem.BoingHammer, TranceSeekBattleAbility.Springboots }, // Boing Hammer
            { TranceSeekRegularItem.Overhammerclock, TranceSeekBattleAbility.Acceleratorhammer }, // OverHammerClock
            { TranceSeekRegularItem.MithrilHammer, TranceSeekBattleAbility.Criticalaim }, // Mithril Hammer
            { TranceSeekRegularItem.Tazermmer, TranceSeekBattleAbility.Electroshock }, // Tazermmer
            { TranceSeekRegularItem.FieryHammer, TranceSeekBattleAbility.Flurryofhammers }, // Fiery Hammer
            { TranceSeekRegularItem.GregtechHammer, TranceSeekBattleAbility.AdjustableWrench }, // GregTech Hammer
            { TranceSeekRegularItem.EMcinna2, TranceSeekBattleAbility.HymnoftheTantalas }, // E=MCinna²
        };

        private static readonly Dictionary<String, String> MessageEmergencyPlan = new Dictionary<String, String>
        {
            { "US", "Emergency Plan!" }, { "UK", "Emergency Plan!" }, { "JP", "緊急時対策!" },
            { "ES", "¡Plan de emergencia!" }, { "FR", "Plan d'urgence !" }, { "GR", "Notfallplan!" }, { "IT", "Piano di emergenza!" }
        };

        private static readonly Dictionary<String, String> MessagePreserved = new Dictionary<String, String>
        {
            { "US", "Preserved!" }, { "UK", "Preserved!" }, { "JP", "プリザーブド！" },
            { "ES", "¡Conservado!" }, { "FR", "Conservée !" }, { "GR", "Erhalten!" }, { "IT", "Conservata!" }
        };
    }
}




