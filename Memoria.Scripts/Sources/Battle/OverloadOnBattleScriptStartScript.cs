using FF9;
using Memoria.Data;
using Memoria.Prime;
using System;
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
            TranceSeekCustomAPI.SOS_SA(v);
            return false;
        }
    }
}
