﻿using FF9;
using Memoria.Data;
using Memoria.Prime;
using System;
using static Memoria.Scripts.Battle.TranceSeekCustomAPI;

namespace Memoria.Scripts.Battle
{
    public class OverloadDamageModifierScript : IOverloadDamageModifierScript
    {
        public void OnDamageModifierChange(BattleCalculator v, Int32 previousValue, Int32 bonus)
        {
        }

        public void OnDamageDrasticReduction(BattleCalculator v)
        {
            v.Context.Attack = 1;
        }

        public void OnDamageFinalChanges(BattleCalculator v)
        {
            if (v.Target.Flags == 0)
                return;

            if (!v.Caster.IsPlayer && (FF9StateSystem.EventState.gEventGlobal[1403] == 1 || FF9StateSystem.EventState.gEventGlobal[1403] == 2)) // Lower Difficulty
            {
                Int32 malusHPdamage = 0;
                Int32 malusMPdamage = 0;
                if (FF9StateSystem.EventState.gEventGlobal[1403] == 1) // Vivi mode
                {
                    malusHPdamage = v.Target.HpDamage / 2;
                    malusMPdamage = v.Target.MpDamage / 2;
                }
                    
                else if (FF9StateSystem.EventState.gEventGlobal[1403] == 2) // Eiko mode
                {
                    malusHPdamage = v.Target.HpDamage / 4;
                    malusMPdamage = v.Target.MpDamage / 4;
                }                   

                if ((v.Target.Flags & CalcFlag.HpAlteration) != 0)
                    v.Target.HpDamage = Math.Max(1, v.Target.HpDamage - malusHPdamage);
                if ((v.Target.Flags & CalcFlag.MpAlteration) != 0)
                    v.Target.MpDamage = Math.Max(1, v.Target.MpDamage - malusMPdamage);
            }

            Single modifier_factor = 1f + v.Context.DamageModifierCount * 0.25f;
            while (v.Context.DamageModifierCount < 0)
            {
                modifier_factor *= 0.5f;
                ++v.Context.DamageModifierCount;
            }
            Int32 reflectMultiplier = v.Command.GetReflectMultiplierOnTarget(v.Target.Id);
            if ((v.Target.Flags & CalcFlag.HpAlteration) != 0)
            {
                v.Target.HpDamage = (Int32)Math.Round(modifier_factor * v.Target.HpDamage) * reflectMultiplier;
                if (v.Target.PlayerIndex == (CharacterId)12 && (v.Target.Flags & CalcFlag.HpRecovery) == 0) // Lani's Rage Mechanic
                    v.Target.AlterStatus(CustomStatus.Rage, v.Caster);
            }
            if (v.Target.IsUnderAnyStatus(BattleStatus.EasyKill) && ((v.Target.Flags & CalcFlag.HpRecovery) != 0 || v.Context.IsAbsorb || v.Command.ScriptId == 10 || v.Command.ScriptId == 69 ||
                 v.Command.ScriptId == 79 || v.Command.ScriptId == 30 || v.Command.ScriptId == 37))
            {
                if (v.Target.IsUnderAnyStatus(BattleStatus.Zombie))
                {
                    v.Target.HpDamage = 0;
                    v.Target.RemoveStatus(BattleStatus.Zombie);
                }
            }
            if ((v.Target.Flags & CalcFlag.MpAlteration) != 0)
                v.Target.MpDamage = (Int32)Math.Round(modifier_factor * v.Target.MpDamage) * reflectMultiplier;

            if ((v.Caster.Flags & CalcFlag.HpAlteration) != 0)
                v.Caster.HpDamage = (Int32)Math.Round(modifier_factor * v.Caster.HpDamage) * reflectMultiplier;
            if ((v.Caster.Flags & CalcFlag.MpAlteration) != 0)
                v.Caster.MpDamage = (Int32)Math.Round(modifier_factor * v.Caster.MpDamage) * reflectMultiplier;

            if (Configuration.TetraMaster.TripleTriad == 16389 && v.Caster.IsPlayer)
                v.Target.HpDamage = 9999;

            if (v.Target.HasTrance && v.Target.Data.cur.hp > 0 && !btl_stat.CheckStatus(v.Target.Data, BattleStatusConst.CannotTrance)) // Prevent to earn easy Trance.
            {
                float ratio = (v.Target.HpDamage * 100) / v.Target.MaximumHp; // En %
                if (ratio <= 15)
                    v.Context.TranceIncrease = 0;
            }

            if (v.Command.IsManyTarget && v.Command.AbilityId >= (BattleAbilityId)1500 && v.Command.AbilityId <= (BattleAbilityId)1526)
            {
                if (v.Caster.HasSupportAbilityByIndex((SupportAbility)1126))
                    v.Target.HpDamage = (v.Target.HpDamage * 3) / 4;
                else
                    v.Target.HpDamage /= 2;
            }

            if (v.Caster.IsUnderAnyStatus(CustomStatus.Special) && v.Command.Id == BattleCommandId.Item && (v.Target.Flags & CalcFlag.HpRecovery) != 0) // Secret ingredient
            {
                object Secretingredient = v.Caster.GetPropertyByName("StatusProperty CustomStatus21 Secretingredient");
                if ((int)Secretingredient > 0)
                {
                    v.Target.HpDamage *= 2;
                    btl_stat.AlterStatus(v.Caster, CustomStatusId.Special, parameters: "Secretingredient--");
                }
            }

            TranceSeekCustomAPI.SpecialSA(v);

            if (v.Target.PlayerIndex == CharacterId.Marcus && (v.Command.Element & EffectElement.Darkness) != 0
                && (v.Target.Flags & CalcFlag.HpAlteration) != 0 && (v.Target.Flags & CalcFlag.MpAlteration) == 0) // Marcus mechanic
            {
                int HealMP = ((v.Target.HpDamage * (1 + Comn.random16() % 9)) / 1000);
                v.Target.CurrentMp = Math.Min(v.Caster.CurrentMp + (uint)HealMP, v.Caster.MaximumMp);
                btl2d.Btl2dStatReq(v.Target, 0, -HealMP);
            }

            if (v.Command.ItemId != (RegularItem)2487 && v.Command.ItemId != (RegularItem)2488 && v.Command.ItemId != (RegularItem)2489) // Blank mix
            {
                if ((v.Caster.Flags & CalcFlag.HpAlteration) != 0)
                    v.Caster.HpDamage = Math.Min(v.Caster.HpDamage, 9999);
                if ((v.Caster.Flags & CalcFlag.MpAlteration) != 0)
                    v.Caster.MpDamage = Math.Min(v.Caster.MpDamage, 9999);
                if ((v.Target.Flags & CalcFlag.HpAlteration) != 0)
                    v.Target.HpDamage = Math.Min(v.Target.HpDamage, 9999);
                if ((v.Target.Flags & CalcFlag.MpAlteration) != 0)
                    v.Target.MpDamage = Math.Min(v.Target.MpDamage, 9999);
            }

            if ((v.Target.Flags & CalcFlag.HpRecovery) != 0 && v.Caster.HasSupportAbilityByIndex((SupportAbility)127) && !v.Target.IsZombie && v.Target.HpDamage > (v.Target.MaximumHp - v.Target.CurrentHp)) // SA Invigorating
            {
                uint factor = (uint)(v.Caster.HasSupportAbilityByIndex((SupportAbility)1127) ? 20 : 10);
                v.Target.MaximumHp += Math.Min((v.Target.MaximumHp * factor) / 100, (uint)(v.Target.HpDamage - (v.Target.MaximumHp - v.Target.CurrentHp)));
                v.Target.CurrentHp = v.Target.MaximumHp;
            }
        }
    }
}