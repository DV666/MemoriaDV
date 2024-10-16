using FF9;
using Memoria.Data;
using Memoria.Prime;
using System;
using System.Collections.Generic;
using static Memoria.Scripts.Battle.TranceSeekCustomAPI;

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
                v.Caster.Data.mot[2] = "ANH_MON_B3_122_043";
                btl_stat.MakeStatusesPermanent(v.Caster, CustomStatus.Runic, false);
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
                        btl_stat.AlterStatus(v.Caster, CustomStatusId.Rage, parameters: "-1");
                        break;
                    }
                    case (BattleAbilityId)1077: // Carnage
                    case (BattleAbilityId)1080: // Hatred
                    case (BattleAbilityId)1086: // Agni's Blade
                    case (BattleAbilityId)1087: // Shiva Blade
                    case (BattleAbilityId)1088: // Indra Blade
                    case (BattleAbilityId)1089: // Varuna Blade
                    {
                        btl_stat.AlterStatus(v.Caster, CustomStatusId.Rage, parameters: "-2");
                        break;
                    }
                    case (BattleAbilityId)1078: // Ripping
                    case (BattleAbilityId)1081: // Super Muscles
                    case (BattleAbilityId)1090: // Prithvi Blade
                    {
                        btl_stat.AlterStatus(v.Caster, CustomStatusId.Rage, parameters: "-3");
                        break;
                    }
                }
            }
            if (v.Caster.PlayerIndex == CharacterId.Cinna) // Cinna's Mechanic
            {
                for (Int32 i = 0; i < 8; i++) // Pas terrible... à revoir la méthode je pense.
                {
                    int idAA = 1136 + i;
                    if (FF9StateSystem.Battle.FF9Battle.aa_data[(BattleAbilityId)idAA].MP > 0)
                        FF9StateSystem.Battle.FF9Battle.aa_data[(BattleAbilityId)idAA].MP--;
                }

                if (v.Command.AbilityId == (BattleAbilityId)1138) // Accelerator hammer
                {
                    List<AA_DATA> AAlist = new List<AA_DATA>();

                    for (Int32 i = 0; i < 8; i++)
                    {
                        int idAA = 1136 + i;
                        if (FF9StateSystem.Battle.FF9Battle.aa_data[(BattleAbilityId)idAA].MP > 0)
                            AAlist.Add(FF9StateSystem.Battle.FF9Battle.aa_data[(BattleAbilityId)idAA]);
                    }
                    AAlist[GameRandom.Next16() % AAlist.Count].MP--;
                }

                if (v.Command.Id == (BattleCommandId)10021)
                {
                    int mpCost = 0;
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
                    }
                    FF9StateSystem.Battle.FF9Battle.aa_data[v.Command.AbilityId].MP = mpCost;
                }

            }
            if (SpecialSAEffect[v.Caster.Data][8] > 0)
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

            TranceSeekCustomAPI.SOS_SA(v);
            return false;
        }
    }
}
