using System;
using Memoria.Data;
using Memoria.Prime;

namespace Memoria.Scripts.TranceSeek
{
    public class OverloadOnBattleScriptEndScript
    {

        public static void OnHitEnd(BattleCalculator v)
        {
            SOS_SA(v);
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

            casterState.SpecialSA.Propagation = 0;
        }

        public static void SOS_SA(BattleCalculator v)
        {
            var targetState = v.TargetState();
            BattleUnit target = v.Target;

            bool isHpBelowHalf = target.CurrentHp <= (target.MaximumHp / 2);
            bool isLowHp = target.IsUnderAnyStatus(BattleStatus.LowHP);

            if (!isHpBelowHalf)
                targetState.SpecialSA.OneTriggerSOS &= ~(1 | 4 | 16 | 64 | 256 | 1024);

            if (!isLowHp)
                targetState.SpecialSA.OneTriggerSOS &= ~(2 | 8 | 32 | 128 | 512 | 2048);

            CheckAndTriggerSOS(target, targetState, isLowHp, isHpBelowHalf, TranceSeekSupportAbility.SOS_Protect, TranceSeekSupportAbility.SOS_Protect_Boosted, BattleStatus.Protect, 2, 1);
            CheckAndTriggerSOS(target, targetState, isLowHp, isHpBelowHalf, TranceSeekSupportAbility.SOS_Shell, TranceSeekSupportAbility.SOS_Shell_Boosted, BattleStatus.Shell, 8, 4);
            CheckAndTriggerSOS(target, targetState, isLowHp, isHpBelowHalf, TranceSeekSupportAbility.SOS_Regen, TranceSeekSupportAbility.SOS_Regen_Boosted, BattleStatus.Regen, 32, 16);
            CheckAndTriggerSOS(target, targetState, isLowHp, isHpBelowHalf, TranceSeekSupportAbility.SOS_Haste, TranceSeekSupportAbility.SOS_Haste_Boosted, BattleStatus.Haste, 128, 64);
            CheckAndTriggerSOS(target, targetState, isLowHp, isHpBelowHalf, TranceSeekSupportAbility.SOS_Reflect, TranceSeekSupportAbility.SOS_Reflect_Boosted, BattleStatus.Reflect, 512, 256);
            CheckAndTriggerSOS(target, targetState, isLowHp, isHpBelowHalf, TranceSeekSupportAbility.SOS_Vanish, TranceSeekSupportAbility.SOS_Vanish_Boosted, BattleStatus.Vanish, 2048, 1024);
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
