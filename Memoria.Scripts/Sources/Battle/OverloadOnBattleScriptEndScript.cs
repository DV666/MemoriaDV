using FF9;
using Memoria.Data;
using Memoria.Prime;
using System;
using System.Collections.Generic;

namespace Memoria.Scripts.TranceSeek
{
    public class OverloadOnBattleScriptEndScript
    {

        public static void OnHitEnd(BattleCalculator v)
        {
            SOS_SA(v.Target);
            TranceSeekCharacterMechanic.DragonMechanic(v);
        }

        public static void OnCommandEnd(BattleCalculator v)
        {
            var casterState = v.CasterState();

            // Mode EX
            if (v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.EXMode) && v.Caster.IsUnderAnyStatus(BattleStatus.Trance))
            {
                Int32 healPercent = v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.EXMode_Boosted) ? 50 : 25;
                Int32 healHp = (int)(v.Caster.MaximumHp * healPercent / 100);
                Int32 healMp = (int)(v.Caster.MaximumMp * healPercent / 100);

                if (healHp > 0)
                    v.Caster.CurrentHp = Math.Min(v.Caster.CurrentHp + (uint)healHp, v.Caster.MaximumHp);
                if (healMp > 0)
                    v.Caster.CurrentMp = Math.Min(v.Caster.CurrentMp + (uint)healMp, v.Caster.MaximumMp);

                btl2d.Btl2dStatReq(v.Caster, -healHp, -healMp);
            }

            TranceSeekRegularItem.SpecialItemsAtEnd(v);
            TranceSeekCharacterMechanic.EikoMougMechanic(v);
            TranceSeekCharacterMechanic.HeheTriggered = false;

            casterState.SpecialSA.Propagation = 0;

            if (v.Target.Data.dms_geo_id == 353 && !v.Target.IsPlayer && v.Caster.IsPlayer && FF9StateSystem.Battle.battleMapIndex == 930 && FF9StateSystem.Battle.FF9Battle.btl_scene.PatNum == 0)
            { // Tantarian because it piss me off ! -_-. Also this script can handle "multi hit".
                int page = 0;
                if (FF9StateSystem.EventState.gScriptDictionary.TryGetValue(1004, out Dictionary<int, int> dict))
                    dict.TryGetValue(0, out page);
                int previouspage = FF9StateSystem.EventState.gEventGlobal[1305];
                int summonchance = FF9StateSystem.EventState.gEventGlobal[1306];

                if (previouspage == page)
                    return;

                if (page < 230)
                {
                    if ((Comn.random8() % 2) == 0)
                    {
                        summonchance = 1;
                        btl_cmd.SetEnemyCommand(v.Target, BattleCommandId.EnemyCounter, 9, v.Target.Id);
                    }
                    else
                    {
                        if (summonchance == 0)
                        {
                            summonchance = 1;
                            btl_cmd.SetEnemyCommand(v.Target, BattleCommandId.EnemyCounter, 9, v.Target.Id);
                        }
                        else
                        {
                            if (summonchance > 0)
                            {
                                summonchance--;
                            }
                            btl_cmd.SetEnemyCommand(v.Target, BattleCommandId.EnemyCounter, 6, v.Target.Id);
                        }
                    }
                }
                else
                {
                    btl_cmd.SetEnemyCommand(v.Target, BattleCommandId.EnemyCounter, 3, v.Target.Id);
                }
                previouspage = page;
            }
        }

        public static void SOS_SA(BattleUnit unit, Boolean ForceTrigger = false)
        {
            var targetState = unit.State();
            bool isHpBelowHalf = unit.CurrentHp <= (unit.MaximumHp / 2) || ForceTrigger;
            bool isLowHp = unit.IsUnderAnyStatus(BattleStatus.LowHP) || ForceTrigger;

            if (!isHpBelowHalf)
                targetState.SpecialSA.OneTriggerSOS &= ~(1 | 4 | 16 | 64 | 256 | 1024);

            if (!isLowHp)
                targetState.SpecialSA.OneTriggerSOS &= ~(2 | 8 | 32 | 128 | 512 | 2048);

            CheckAndTriggerSOS(unit, targetState, isLowHp, isHpBelowHalf, TranceSeekSupportAbility.SOS_Protect, TranceSeekSupportAbility.SOS_Protect_Boosted, BattleStatus.Protect, 2, 1);
            CheckAndTriggerSOS(unit, targetState, isLowHp, isHpBelowHalf, TranceSeekSupportAbility.SOS_Shell, TranceSeekSupportAbility.SOS_Shell_Boosted, BattleStatus.Shell, 8, 4);
            CheckAndTriggerSOS(unit, targetState, isLowHp, isHpBelowHalf, TranceSeekSupportAbility.SOS_Regen, TranceSeekSupportAbility.SOS_Regen_Boosted, BattleStatus.Regen, 32, 16);
            CheckAndTriggerSOS(unit, targetState, isLowHp, isHpBelowHalf, TranceSeekSupportAbility.SOS_Haste, TranceSeekSupportAbility.SOS_Haste_Boosted, BattleStatus.Haste, 128, 64);
            CheckAndTriggerSOS(unit, targetState, isLowHp, isHpBelowHalf, TranceSeekSupportAbility.SOS_Reflect, TranceSeekSupportAbility.SOS_Reflect_Boosted, BattleStatus.Reflect, 512, 256);
            CheckAndTriggerSOS(unit, targetState, isLowHp, isHpBelowHalf, TranceSeekSupportAbility.SOS_Vanish, TranceSeekSupportAbility.SOS_Vanish_Boosted, BattleStatus.Vanish, 2048, 1024);
        }

        private static void CheckAndTriggerSOS(BattleUnit target, TranceSeekFighterState state, bool isLowHp, bool isHpBelowHalf, SupportAbility normal, SupportAbility boosted, BattleStatus status, int normalBit, int boostedBit)
        {
            bool hasBoosted = target.HasSupportAbilityByIndex(boosted);
            bool hasNormal = target.HasSupportAbilityByIndex(normal);

            if (hasBoosted && isHpBelowHalf && (state.SpecialSA.OneTriggerSOS & boostedBit) == 0)
            {
                target.AlterStatus(status, target);
                state.SpecialSA.OneTriggerSOS |= boostedBit;
            }
            else if (hasNormal && !hasBoosted && isLowHp && (state.SpecialSA.OneTriggerSOS & normalBit) == 0)
            {
                target.AlterStatus(status, target);
                state.SpecialSA.OneTriggerSOS |= normalBit;
            }
        }
    }
}
