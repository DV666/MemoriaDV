using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Assets;
using Memoria.Data;
using Memoria.Database;
using Memoria.Prime;
using System;
using System.Collections.Generic;
using static Memoria.Scripts.Battle.TranceSeekAPI;

namespace Memoria.Scripts.Battle
{
    public class OverloadOnBattleScriptStartScript : IOverloadOnBattleScriptStartScript
    {
        public Boolean OnBattleScriptStart(BattleCalculator v)
        {
            if (MonsterMechanic[v.Target.Data][3] == 1 && v.Target.CurrentHp <= 10000) // Prevent boss to die => Maybe use CustomBattleFlagsMeaning ?
            {
                v.Target.CurrentHp = 10000;
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
                    MonsterMechanic[v.Caster.Data][6] = 1; // Don't miss the attack.
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

            if (!v.Caster.IsPlayer && v.Command.Data.aa.Vfx2 > 0) // Custom status for monsters (using the Animation 2 value in HW) 
            {
                ulong AACustomStatus = v.Command.Data.aa.Vfx2;
                if ((AACustomStatus & 1) != 0)
                    v.Command.AbilityStatus |= TranceSeekStatus.PowerBreak;
                if ((AACustomStatus & 2) != 0)
                    v.Command.AbilityStatus |= TranceSeekStatus.MagicBreak;
                if ((AACustomStatus & 4) != 0)
                    v.Command.AbilityStatus |= TranceSeekStatus.ArmorBreak;
                if ((AACustomStatus & 8) != 0)
                    v.Command.AbilityStatus |= TranceSeekStatus.MentalBreak;
                if ((AACustomStatus & 16) != 0)
                    v.Command.AbilityStatus |= TranceSeekStatus.PowerUp;
                if ((AACustomStatus & 32) != 0)
                    v.Command.AbilityStatus |= TranceSeekStatus.MagicUp;
                if ((AACustomStatus & 64) != 0)
                    v.Command.AbilityStatus |= TranceSeekStatus.ArmorUp;
                if ((AACustomStatus & 128) != 0)
                    v.Command.AbilityStatus |= TranceSeekStatus.MentalUp;
                if ((AACustomStatus & 256) != 0)
                    v.Command.AbilityStatus |= TranceSeekStatus.Bulwark;
                if ((AACustomStatus & 512) != 0)
                    v.Command.AbilityStatus |= TranceSeekStatus.PerfectDodge;
                if ((AACustomStatus & 1024) != 0)
                    v.Command.AbilityStatus |= TranceSeekStatus.PerfectCrit;
                if ((AACustomStatus & 2048) != 0)
                    v.Command.AbilityStatus |= TranceSeekStatus.Vieillissement;
            }

            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)117) && SpecialSAEffect[v.Caster][4] == 0 && v.Caster.IsUnderAnyStatus(BattleStatus.Trance)) // Mode EX
            {
                Int32 HealHPSAOrItem = (int)(v.Caster.MaximumHp * (v.Caster.HasSupportAbilityByIndex((SupportAbility)1117) ? 16 : 8) / 100);
                Int32 HealMPSAOrItem = (int)(v.Caster.MaximumMp * (v.Caster.HasSupportAbilityByIndex((SupportAbility)1117) ? 16 : 8) / 100);
                SpecialSAEffect[v.Caster][4] = 1;
                v.Caster.AddDelayedModifier(
                caster => caster.CurrentAtb >= caster.MaximumAtb,
                caster =>
                {
                    SpecialSAEffect[v.Caster][4] = 0;
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
                    case (BattleAbilityId)1076: // Combo
                    case (BattleAbilityId)1079: // Mad Rush
                    case (BattleAbilityId)1082: // Flame Tongue
                    case (BattleAbilityId)1083: // Ice Brand
                    case (BattleAbilityId)1084: // Thunder Blade
                    case (BattleAbilityId)1085: // Liquid Steel
                    {
                        btl_stat.AlterStatus(v.Caster, TranceSeekStatusId.Rage, parameters: "-1");
                        break;
                    }
                    case (BattleAbilityId)1077: // Carnage
                    case (BattleAbilityId)1080: // Hatred
                    case (BattleAbilityId)1086: // Agni's Blade
                    case (BattleAbilityId)1087: // Shiva Blade
                    case (BattleAbilityId)1088: // Indra Blade
                    case (BattleAbilityId)1089: // Varuna Blade
                    {
                        btl_stat.AlterStatus(v.Caster, TranceSeekStatusId.Rage, parameters: "-2");
                        break;
                    }
                    case (BattleAbilityId)1078: // Ripping
                    case (BattleAbilityId)1081: // Super Muscles
                    case (BattleAbilityId)1090: // Prithvi Blade
                    {
                        btl_stat.AlterStatus(v.Caster, TranceSeekStatusId.Rage, parameters: "-3");
                        break;
                    }
                }
            }
            if (v.Caster.PlayerIndex == CharacterId.Cinna) // Cinna's Mechanic
            {
                int InventionsCD = 0; // For Genie/Eureka mechanic.

                List<BattleAbilityId> InventionAA = new List<BattleAbilityId>{ (BattleAbilityId)1136, (BattleAbilityId)1137, (BattleAbilityId)1138, (BattleAbilityId)1139,
                    (BattleAbilityId)1140, (BattleAbilityId)1141, (BattleAbilityId)1142, (BattleAbilityId)1143, (BattleAbilityId)1538, (BattleAbilityId)1539};

                foreach (BattleAbilityId AA in InventionAA)
                {
                    if (FF9StateSystem.Battle.FF9Battle.aa_data[AA].MP > 0)
                    {
                        FF9StateSystem.Battle.FF9Battle.aa_data[AA].MP--;
                        if (AA != (BattleAbilityId)1538 && AA != (BattleAbilityId)1539) // Genie & Eureka
                            InventionsCD++;
                    }
                }

                ViviPassive[v.Caster.Data][0] = InventionsCD;

                if (v.Command.AbilityId == (BattleAbilityId)1138 || v.Command.Id == BattleCommandId.Attack && v.Caster.HasSupportAbilityByIndex((SupportAbility)244)) // Accelerator hammer / SA Mecano
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
                        if (v.Caster.HasSupportAbilityByIndex((SupportAbility)1244) && AAlist.Count > 0) // SA Mecano++
                        {
                            AAlist.Remove(AAChoosen);
                            if (AAlist.Count > 0)
                                AAlist[GameRandom.Next16() % AAlist.Count].MP--;
                        }
                    }
                }

                if (FF9StateSystem.Battle.FF9Battle.aa_data[v.Command.AbilityId].MP > 0 && v.Caster.HasSupportAbilityByIndex((SupportAbility)246))
                {
                    v.Caster.CurrentMp = 0;
                    Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                    {
                        { "US", "Emergency Plan!" },
                        { "UK", "Emergency Plan!" },
                        { "JP", "緊急時対策!" },
                        { "ES", "¡Plan de emergencia!" },
                        { "FR", "Plan d'urgence !" },
                        { "GR", "Notfallplan!" },
                        { "IT", "Piano di emergenza!" },
                    };
                    btl2d.Btl2dReqSymbolMessage(v.Caster.Data, "[FFFF00]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 10);
                }

                if (v.Command.Id == (BattleCommandId)1021 || v.Command.Id == (BattleCommandId)1036 || v.Command.Id == (BattleCommandId)1037)  // CMD Invention
                {
                    int mpCost = 0;
                    KeyValuePair<RegularItem, BattleAbilityId> PassiveHammer = new KeyValuePair<RegularItem, BattleAbilityId>(v.Caster.Weapon, v.Command.AbilityId);
                    if ((GameRandom.Next8() % 100) < 25 && InventionPreserved.Contains(PassiveHammer))
                    {
                        Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                        {
                            { "US", "Preserved!" },
                            { "UK", "Preserved!" },
                            { "JP", "プリザーブド！" },
                            { "ES", "¡Conservado!" },
                            { "FR", "Conservée !" },
                            { "GR", "Erhalten!" },
                            { "IT", "Conservata!" },
                        };
                        btl2d.Btl2dReqSymbolMessage(v.Caster.Data, "[FFC000]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 10);
                    }
                    else
                    {
                        switch (v.Command.AbilityId)
                        {
                            case (BattleAbilityId)1136: // Hammer throw
                                mpCost = 2;
                                break;
                            case (BattleAbilityId)1137: // Spring boots
                                mpCost = 3;
                                break;
                            case (BattleAbilityId)1138: // Accelerator hammer
                                mpCost = 4;
                                break;
                            case (BattleAbilityId)1139: // Critical aim
                                mpCost = 5;
                                break;
                            case (BattleAbilityId)1140: // Electroshock
                                mpCost = 6;
                                break;
                            case (BattleAbilityId)1141: // Flurry of hammers
                                mpCost = 7;
                                break;
                            case (BattleAbilityId)1142: // Adjustable Wrench
                                mpCost = 8;
                                break;
                            case (BattleAbilityId)1143: // Hymn of the Tantalas
                                mpCost = 9;
                                break;
                            case (BattleAbilityId)1538: // Idea
                                mpCost = 10;
                                break;
                            case (BattleAbilityId)1539: // Eureka
                                mpCost = 6;
                                break;
                        }
                        FF9StateSystem.Battle.FF9Battle.aa_data[v.Command.AbilityId].MP = mpCost;
                    }
                }
            }
            if (SpecialSAEffect[v.Caster.Data][8] > 0) // AA SpringBoots
            {
                v.Caster.AddDelayedModifier(
                    caster => caster.CurrentAtb >= caster.MaximumAtb,
                    caster =>
                    {
                        if (!caster.IsUnderAnyStatus(BattleStatusConst.StopAtb) && caster.CurrentAtb < (4 * caster.MaximumAtb / 5))
                            caster.CurrentAtb += (Int16)(4 * caster.MaximumAtb / 5);
                        SpecialSAEffect[v.Caster.Data][8]--;
                    }
                );
            }

            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)203) && v.Caster.PlayerIndex == CharacterId.Zidane && ZidanePassive[v.Caster.Data][4] == 0
                && v.Command.Id != BattleCommandId.Counter && v.Command.Id != BattleCommandId.RushAttack) // SA Flexible
            {
                // Permanent [code=Condition] WeaponId == 1 || WeaponId == 2 || WeaponId == 3 || WeaponId == 1153 || WeaponId == 1155 || WeaponId == 1158 || WeaponId == 1161 || WeaponId == 1164 || WeaponId == 1167 [/code] [code=BanishSAByLvl] 203 ; -1 [/code]
                List<RegularItem> BlackListedWeapon = new List<RegularItem>{ RegularItem.Dagger, RegularItem.MageMasher, RegularItem.MythrilDagger, (RegularItem)1153,
                (RegularItem)1155, (RegularItem)1158, (RegularItem)1161, (RegularItem)1164, (RegularItem)1167 };
                if (!BlackListedWeapon.Contains(v.Caster.Weapon))
                {
                    ZidanePassive[v.Caster.Data][9]++;
                    btl_stat.AlterStatus(v.Caster, TranceSeekStatusId.Special, parameters: "Flexible0");
                    int FlexibleTurn = v.Caster.HasSupportAbilityByIndex((SupportAbility)1203) ? 2 : 4;
                    if (ZidanePassive[v.Caster.Data][9] >= FlexibleTurn)
                    {
                        ZidanePassive[v.Caster.Data][9] = 0;
                        if (btl_util.getSerialNumber(v.Caster.Data) == CharacterSerialNumber.ZIDANE_SWORD)
                            BattleState.EnqueueCounter(v.Caster, BattleCommandId.RushAttack, (BattleAbilityId)1000, v.Caster.Id);
                        else
                            BattleState.EnqueueCounter(v.Caster, BattleCommandId.RushAttack, (BattleAbilityId)1001, v.Caster.Id);

                        if (v.Caster.HasSupportAbilityByIndex((SupportAbility)1203))
                            btl_stat.AlterStatus(v.Caster, TranceSeekStatusId.Special, parameters: "Flexible2"); // SA Flexible+
                        else
                            btl_stat.AlterStatus(v.Caster, TranceSeekStatusId.Special, parameters: "Flexible1"); // SA Flexible
                    }
                }
            }

            if (v.Command.AbilityStatus > 0 && ProtectStatus.TryGetValue(v.Target.Data, out Dictionary<BattleStatus, Int32> statusprotect))
            {
                if (statusprotect.Count > 1)
                {
                    foreach (BattleStatusId statusID in v.Command.AbilityStatus.ToStatusList())
                    {
                        BattleStatus status = statusID.ToBattleStatus();
                        if (statusprotect.ContainsKey(status))
                        {
                            if (statusprotect[status] > 0)
                            {
                                v.Command.AbilityStatus &= ~status;
                                if (statusprotect[status] != 255)
                                {
                                    statusprotect[status]--;
                                    Dictionary<String, String> localizedStatusProtect = new Dictionary<String, String>
                                    {
                                        { "US", $"-{status}" },
                                        { "UK", $"-{status}" },
                                        { "JP", $"-{status}" },
                                        { "ES", $"-{status}" },
                                        { "FR", $"-{status}" },
                                        { "GR", $"-{status}" },
                                        { "IT", $"-{status}" },
                                    };
                                    btl2d.Btl2dReqSymbolMessage(v.Target.Data, "[38FF1F]", localizedStatusProtect, HUDMessage.MessageStyle.DAMAGE, 5);
                                }
                            }
                        }
                    }
                }
            }

            if (v.Command.Id == (BattleCommandId)1020) // CMD Mixing
            {
                int TranceDelta = v.Caster.HasSupportAbilityByIndex((SupportAbility)1251) ? 42 : v.Caster.HasSupportAbilityByIndex((SupportAbility)251) ? 64 : 128;
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

            if (v.Command.Id == (BattleCommandId)1032 && !v.Caster.HasSupportAbilityByIndex((SupportAbility)1205)) // Witchcraft (Vivi's SA)
            {
                v.Command.HitRate /= 2;
            }

            if (v.Command.IsManyTarget && v.Command.AbilityId >= (BattleAbilityId)1500 && v.Command.AbilityId <= (BattleAbilityId)1526)
            {
                if (v.Caster.HasSupportAbilityByIndex((SupportAbility)1126))
                    v.Command.HitRate = (v.Command.HitRate * 3) / 4;
                else
                    v.Command.HitRate /= 2;

                if (v.Caster.HasSupportAbilityByIndex((SupportAbility)1126))
                    btl_stat.AlterStatus(v.Caster, TranceSeekStatusId.Special, parameters: "Propagation2"); // SA Propagation+
                else
                    btl_stat.AlterStatus(v.Caster, TranceSeekStatusId.Special, parameters: "Propagation1"); // SA Propagation

                v.Caster.AddDelayedModifier(
                    caster => caster.CurrentAtb >= caster.MaximumAtb,
                    caster =>
                    {
                        btl_stat.AlterStatus(caster, TranceSeekStatusId.Special, parameters: "Propagation--");
                    }
                );
            }

            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)214)) // SA Enchanted blade
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
                    SpecialSAEffect[v.Caster.Data][10] = v.Caster.HasSupportAbilityByIndex((SupportAbility)1214) ? 2 : 1;
                }
                else if (SpecialSAEffect[v.Caster.Data][10] > 0)
                {
                    SpecialSAEffect[v.Caster.Data][10]--;
                    if (SpecialSAEffect[v.Caster.Data][10] <= 0)
                    {
                        WeaponNewElement[v.Caster.Data] = 0;
                        WeaponNewCustomElement[v.Caster.Data] = 0;
                        WeaponNewStatus[v.Caster.Data] = 0;
                        InfusedWeaponScript.ClearInfuseWeapon(v.Caster.Data);
                    }
                }
            }

            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)227) && v.Command.Data.info.effect_counter == 1) // SA Enchanted blade
            {
                if ((v.Command.AbilityId == BattleAbilityId.Regen || v.Command.AbilityId == (BattleAbilityId)1500) && v.Caster.Weapon == RegularItem.Hamelin ||
                    (v.Command.AbilityId == BattleAbilityId.Life || v.Command.AbilityId == (BattleAbilityId)1501) && v.Caster.Weapon == RegularItem.SirenFlute ||
                    v.Command.AbilityId == BattleAbilityId.Berserk && v.Caster.Weapon == RegularItem.LamiaFlute ||
                    (v.Command.AbilityId == BattleAbilityId.Protect || v.Command.AbilityId == (BattleAbilityId)1504) && v.Caster.Weapon == RegularItem.GolemFlute ||
                    (v.Command.AbilityId == BattleAbilityId.Haste || v.Command.AbilityId == (BattleAbilityId)1505) && v.Caster.Weapon == RegularItem.FairyFlute)
                {
                    v.Caster.Flags |= CalcFlag.HpDamageOrHeal;
                    v.Caster.HpDamage = v.Command.Data.aa.MP * 10;
                    if (v.Caster.HasSupportAbilityByIndex((SupportAbility)1227)) // SA Enchanted blade
                    {
                        v.Caster.Flags |= CalcFlag.MpDamageOrHeal;
                        v.Caster.MpDamage = v.Command.Data.aa.MP;
                    }
                }
            }

            if (v.Caster.PlayerIndex == CharacterId.Beatrix) // Redemption mechanic
            {
                if (BeatrixPassive[v.Caster.Data][3] == 0 && v.Caster.PlayerIndex == CharacterId.Beatrix)
                {
                    if (v.Command.Id == BattleCommandId.Attack || v.Command.Id == BattleCommandId.Defend || v.Command.Id == BattleCommandId.Counter && v.Command.AbilityId == BattleAbilityId.Attack ||
                        v.Command.Id == BattleCommandId.HolyWhiteMagic || v.Caster.IsUnderAnyStatus(BattleStatus.Trance))
                    {
                        v.Caster.AlterStatus(TranceSeekStatus.Redemption, v.Caster);
                    }
                    else if (v.Command.Id == BattleCommandId.HolySword1 || v.Command.Id == BattleCommandId.Counter && !v.Caster.HasSupportAbilityByIndex((SupportAbility)1234) &&
                        (v.Command.AbilityId == BattleAbilityId.ThunderSlash || v.Command.AbilityId == BattleAbilityId.StockBreak || v.Command.AbilityId == BattleAbilityId.Climhazzard || v.Command.AbilityId == BattleAbilityId.Shock
                        | v.Command.AbilityId == (BattleAbilityId)1011 || v.Command.AbilityId == (BattleAbilityId)1012 || v.Command.AbilityId == (BattleAbilityId)1013 || v.Command.AbilityId == (BattleAbilityId)1014)) // SA Dominance+
                    {
                        if (v.Caster.HasSupportAbilityByIndex((SupportAbility)233) && (v.Caster.HasSupportAbilityByIndex((SupportAbility)1233) ? 50 : 25) < Comn.random16() % 100)
                        {
                            btl_stat.AlterStatus(v.Caster, TranceSeekStatusId.Redemption, v.Caster, parameters: "Remove");
                        }
                        else
                        {
                            v.Caster.RemoveStatus(TranceSeekStatus.Redemption);
                        }
                    }
                    BeatrixPassive[v.Caster.Data][3] = 1;
                    UpdateRedemptionHUD(v.Caster);
                    v.Caster.AddDelayedModifier(
                        caster => caster.CurrentAtb >= caster.MaximumAtb,
                        caster =>
                        {
                            BeatrixPassive[v.Caster.Data][3] = 0;
                        }
                    );
                }
            }
            else if (BeatrixPassive[v.Target.Data][3] == 0 && v.Target.PlayerIndex == CharacterId.Beatrix && v.Target.IsCovering)
            {
                v.Target.AlterStatus(TranceSeekStatus.Redemption, v.Caster);
                BeatrixPassive[v.Target.Data][3] = 1;
                UpdateRedemptionHUD(v.Target);
                v.Caster.AddDelayedModifier(
                    caster => caster.CurrentAtb >= caster.MaximumAtb,
                    caster =>
                    {
                        BeatrixPassive[v.Target.Data][3] = 0;
                    }
                );
            }

            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)240) && v.Command.Data.info.effect_counter == 1 && v.Command.Id != BattleCommandId.Counter && v.Caster.CurrentMp < v.Caster.MaximumMp) // SA Offering
            {
                v.Caster.AddDelayedModifier(
                    caster => caster.CurrentAtb < (9 * caster.MaximumAtb) / 10 || caster.CurrentAtb >= caster.MaximumAtb,
                    caster =>
                    {
                        int HPDamage = (int)(caster.MaximumHp / 10);
                        int MPRecover = (int)(caster.MaximumMp / (v.Caster.HasSupportAbilityByIndex((SupportAbility)1240) ? 10 : 20));
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

            if (v.Caster.HasSupportAbilityByIndex((SupportAbility)248) && v.Command.Id == BattleCommandId.Item) // SA Econome
            {
                if ((v.Caster.HasSupportAbilityByIndex((SupportAbility)1248) ? 50 : 25) < Comn.random16() % 100)
                    ff9item.FF9Item_Add(v.Command.ItemId, 1);
            }

            if (v.Target.HasSupportAbilityByIndex((SupportAbility)254)) // SA In top form!
            {
                v.Caster.AddDelayedModifier(
                    caster => caster.CurrentAtb >= caster.MaximumAtb,
                    caster =>
                    {
                        int RatioHP = (int)((v.Target.CurrentHp * 100) / v.Target.MaximumHp);
                        v.Target.PhysicalDefence = (SpecialSAEffect[v.Target.Data][13] * ((v.Target.HasSupportAbilityByIndex((SupportAbility)1254) ? 50 : 25) + RatioHP)) / 100;
                    }
                );
            }

            if (v.Caster.PlayerIndex == (CharacterId)14)
            {
                CharacterPresetId presetId = v.Caster.Player.PresetId;
                v.Caster.SummonCount++;
                if (v.Caster.HasSupportAbilityByIndex((SupportAbility)1253) || v.Caster.HasSupportAbilityByIndex((SupportAbility)253) && v.Caster.SummonCount % 2 == 0) // SA Take that!
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
                OverloadOnBattleScriptEndScript.EikoMougMechanic(v);
            }
            else // [TODO] Can be improved ?
            {
                Int32 counter = 15;
                v.Caster.AddDelayedModifier(
                    caster => (counter -= BattleState.ATBTickCount) > 0,
                    caster =>
                    {
                        OverloadOnBattleScriptEndScript.EikoMougMechanic(v);
                    }
                );
            }
            return false;
        }

        public static List<KeyValuePair<RegularItem, BattleAbilityId>> InventionPreserved = new List<KeyValuePair<RegularItem, BattleAbilityId>>
        {
            { new KeyValuePair<RegularItem, BattleAbilityId>((RegularItem)1108, (BattleAbilityId)1136) }, // Steel Hammer
            { new KeyValuePair<RegularItem, BattleAbilityId>((RegularItem)1109, (BattleAbilityId)1137) }, // Boing Hammer
            { new KeyValuePair<RegularItem, BattleAbilityId>((RegularItem)1110, (BattleAbilityId)1138) }, // OverHammerClock
            { new KeyValuePair<RegularItem, BattleAbilityId>((RegularItem)1111, (BattleAbilityId)1139) }, // Mithril Hammer
            { new KeyValuePair<RegularItem, BattleAbilityId>((RegularItem)1112, (BattleAbilityId)1140) }, // Tazermmer
            { new KeyValuePair<RegularItem, BattleAbilityId>((RegularItem)1113, (BattleAbilityId)1141) }, // Fiery Hammer
            { new KeyValuePair<RegularItem, BattleAbilityId>((RegularItem)1114, (BattleAbilityId)1142) }, // GregTech Hammer
            { new KeyValuePair<RegularItem, BattleAbilityId>((RegularItem)1115, (BattleAbilityId)1143) } // E=MCinna²
        };
    }
}
