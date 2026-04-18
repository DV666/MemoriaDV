using FF9;
using Memoria.Assets;
using Memoria.Data;
using Memoria.Prime;
using System;
using System.Collections.Generic;
using static Memoria.Scripts.TranceSeek.TranceSeekAPI;

namespace Memoria.Scripts.TranceSeek
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
            if ((v.Target.FigInfo & Param.FIG_INFO_DEATH) != 0) // Avoid to have "Miss !" if the target die with HpDamage + Death status.
            {
                v.Target.Flags = 0;
                v.Context.Flags = 0;
                return;
            }

            int difficultyMode = FF9StateSystem.EventState.gEventGlobal[1403];

            if (!v.Caster.IsPlayer && (difficultyMode == 1 || difficultyMode == 2)) // Lower Difficulty
            {
                Int32 malusHPdamage = 0;
                Int32 malusMPdamage = 0;
                if (difficultyMode == 1) // Vivi mode
                {
                    malusHPdamage = v.Target.HpDamage / 2;
                    malusMPdamage = v.Target.MpDamage / 2;
                }

                else if (difficultyMode == 2) // Eiko mode
                {
                    malusHPdamage = v.Target.HpDamage / 4;
                    malusMPdamage = v.Target.MpDamage / 4;
                }

                if ((v.Target.Flags & CalcFlag.HpAlteration) != 0)
                    v.Target.HpDamage = Math.Max(1, v.Target.HpDamage - malusHPdamage);
                if ((v.Target.Flags & CalcFlag.MpAlteration) != 0)
                    v.Target.MpDamage = Math.Max(1, v.Target.MpDamage - malusMPdamage);
            }

            TranceSeekAPI.SpecialEffect(v);
            TranceSeekCharacterMechanic.GarnetGemMechanic(v);
            var Caster_TSVar = v.CasterState();
            var Target_TSVar = v.TargetState();

            if (v.Caster.IsPlayer)
            {
                if ((DifficultyDebugMenu.MegaCheat == 2))
                {
                    v.Target.HpDamage = 9999;
                    if (v.Target.IsPlayer)
                        v.Caster.Flags |= CalcFlag.HpAlteration;
                }

                if (FF9StateSystem.Settings.IsDmg9999 && difficultyMode >= 3)
                {
                    v.Target.Flags = CalcFlag.HpRecovery;
                    btl2d.Btl2dReqSymbolMessage(v.Target.Data, "[FF19EE]", MessageNope, HUDMessage.MessageStyle.DAMAGE, 0);
                    SoundLib.PlaySoundEffect(4004); //se511115
                }

                if (Caster_TSVar.Blank.SecretIngredient > 0 && v.Command.Id == BattleCommandId.Item && (v.Target.Flags & CalcFlag.HpRecovery) != 0) // Secret ingredient
                {
                    v.Target.HpDamage *= 2;
                    Caster_TSVar.Blank.SecretIngredient--;
                }
            }
            if (v.Target.IsPlayer)
            {
                if (v.Target.HasTrance && v.Target.Data.cur.hp > 0 && !btl_stat.CheckStatus(v.Target.Data, BattleStatusConst.CannotTrance)) // Prevent to earn easy Trance.
                {
                    if (v.Target.HpDamage * 10 <= v.Target.MaximumHp)
                        v.Context.TranceIncrease = 0;
                }

                if (v.Target.PlayerIndex == (CharacterId)12 && (v.Target.Flags & CalcFlag.HpRecovery) == 0) // Lani's Rage Mechanic
                {
                    v.Target.AlterStatus(TranceSeekStatus.Rage, v.Caster);

                    if (v.Target.HasSupportAbilityByIndex((SupportAbility)236) && (v.Target.HasSupportAbilityByIndex((SupportAbility)1236) ? 50 : 25) < Comn.random16() % 100) // SA Enraged
                        v.Target.AlterStatus(TranceSeekStatus.Rage, v.Caster);

                    if (v.Target.HasSupportAbilityByIndex((SupportAbility)238)) // SA Crisis level
                    {
                        float RatioCrisisLevel = (v.Target.CurrentHp * 100) / v.Target.MaximumHp;
                        v.Context.TranceIncrease += (short)((v.Context.TranceIncrease * (100 - RatioCrisisLevel)) / 100);
                    }
                }

                if (v.Target.PlayerIndex == CharacterId.Amarant && Target_TSVar.Amarant.Duel && (v.Command.AbilityCategory & 8) != 0 && v.Target.IsUnderAnyStatus(BattleStatus.Defend)) // Duel Amarant
                {
                    if (v.Target.HasSupportAbilityByIndex((SupportAbility)231) && (v.Target.HasSupportAbilityByIndex((SupportAbility)1231) ? 50 : 25) > Comn.random16() % 100) // SA Ferocity
                        btl2d.Btl2dReqSymbolMessage(v.Target.Data, "[FF2716]", MessageFerocity, HUDMessage.MessageStyle.DAMAGE, 10);
                    else
                        Target_TSVar.Amarant.Duel = false;
                }
                else if (v.Target.PlayerIndex == CharacterId.Marcus && (v.Command.Element & EffectElement.Darkness) != 0
                    && (v.Target.Flags & CalcFlag.HpAlteration) != 0 && (v.Target.Flags & CalcFlag.MpAlteration) == 0) // Marcus mechanic
                {
                    int HealMP = ((v.Target.HpDamage * (1 + Comn.random16() % 9)) / 1000);
                    if (HealMP > 0)
                    {
                        v.Target.CurrentMp = (uint)Math.Min(v.Target.CurrentMp + HealMP, v.Target.MaximumMp);
                        btl2d.Btl2dStatReq(v.Target, 0, -HealMP);
                    }
                }
                else if (Target_TSVar.Baku.Peuh && v.Target.HpDamage < v.Target.CurrentHp && (v.Command.AbilityCategory & 8) != 0 && (v.Target.Flags & CalcFlag.HpRecovery) == 0) // Peuh!
                {
                    Target_TSVar.Baku.Peuh = false;
                    v.Context.Flags |= BattleCalcFlags.Guard;
                    v.Target.HpDamage = 0;
                    btl2d.Btl2dReqSymbolMessage(v.Target.Data, "[FF0000]", MessagePeuh, HUDMessage.MessageStyle.DAMAGE, 10);
                }

                if (v.Target.IsCovering && v.Target.HasSupportAbilityByIndex((SupportAbility)213)) // SA Duelist
                    v.TargetState().Steiner.Duelist++;
            }
            else
            {
                int BattleID = FF9StateSystem.Battle.battleMapIndex;

                if (BattleID == 28 && v.Target.Data.dms_geo_id == 338 && FF9StateSystem.EventState.gEventGlobal[1305] > 0)
                {
                    if (TranceSeekAPI.IsAttackElement(v, EffectElement.Fire)) // Hibernation - Ice Dragon
                        FF9StateSystem.EventState.gEventGlobal[1305]--;
                }

                if (v.Command.Element == 0 && (v.Target.Flags & CalcFlag.HpRecovery) == 0 && (v.Target.Data.dms_geo_id == 354 || v.Target.Data.dms_geo_id == 221 || v.Target.Data.dms_geo_id == 83)) // Stone monsters
                {
                    v.Context.DamageModifierCount -= 2;
                    SoundLib.PlaySoundEffect(5003); //se770003
                }
            }

            TranceSeekRegularItem.SpecialItems(v);

            Single modifier_factor = 1f; // [TODO] Make something more cleaner about PowerUp status like here ?
            if (v.Context.DamageModifierCount >= 0)
                modifier_factor += v.Context.DamageModifierCount * 0.25f;
            else if (v.Context.DamageModifierCount == -1)
                modifier_factor = 0.75f;
            else
                modifier_factor /= -v.Context.DamageModifierCount;
            if (modifier_factor < 0)
                modifier_factor = 0.01f; // Or 0 ? Hmm...

            Int32 reflectMultiplier = v.Command.GetReflectMultiplierOnTarget(v.Target.Id);
            if ((v.Target.Flags & CalcFlag.HpAlteration) != 0)
            {
                v.Target.HpDamage = (Int32)Math.Round(modifier_factor * v.Target.HpDamage) * reflectMultiplier;
            }
            if (v.Target.IsUnderAnyStatus(BattleStatus.EasyKill) && v.Target.IsUnderAnyStatus(BattleStatus.Zombie))
            {
                if ((v.Target.Flags & CalcFlag.HpRecovery) != 0 || v.Context.IsAbsorb || v.Command.ScriptId == 10 || v.Command.ScriptId == 69
                    || v.Command.ScriptId == 79 || v.Command.ScriptId == 30 || v.Command.ScriptId == 37)
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
        }

        private static readonly Dictionary<String, String> MessageNope = new Dictionary<String, String>
        {
            { "US", "It's a NO!" }, { "UK", "It's a NO!" }, { "JP", "ダメだ！" },
            { "ES", "¡Es un NO!" }, { "FR", "C'est non !" }, { "DE", "Das ist ein NEIN!" }, { "IT", "È un NO!" }
        };

        private static readonly Dictionary<String, String> MessageFerocity = new Dictionary<String, String>
        {
            { "US", "Ferocity!" }, { "UK", "Ferocity!" }, { "JP", "凶暴！" },
            { "ES", "¡Ferocidad!" }, { "FR", "Férocité !" }, { "DE", "Ferozität!" }, { "IT", "Ferocia!" }
        };

        private static readonly Dictionary<String, String> MessagePeuh = new Dictionary<String, String>
        {
            { "US", "--Hmph!" }, { "UK", "--Hmph!" }, { "JP", "--ふん！" },
            { "ES", "--¡Bah!" }, { "FR", "--Peuh !" }, { "DE", "--Pah!" }, { "IT", "--Pah!" }
        };
    }
}
