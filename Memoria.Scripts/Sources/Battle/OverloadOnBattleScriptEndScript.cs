using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Data;
using Memoria.Database;
using Memoria.Prime;
using System;
using System.Collections.Generic;
using static Memoria.Scripts.Battle.TranceSeekAPI;
using static Memoria.Scripts.Battle.TranceSeekBattleDictionary;

namespace Memoria.Scripts.Battle
{
    public class OverloadOnBattleScriptEndScript //: IOverloadOnBattleScriptEndScript
    {
        public static void OnBattleScriptEnd(BattleCalculator v)
        {
            SOS_SA(v);
            TranceSeekCharacterMechanic.DragonMechanic(v);

            //if (Configuration.Battle.Speed == 2)
            //{
            //EikoMougMechanic(v);
            //}
            //else // [TODO] Can be improved ?
            //{
            //    Int32 counter = 15;
            //    v.Caster.AddDelayedModifier(
            //        caster => (counter -= BattleState.ATBTickCount) > 0,
            //        caster =>
            //        {
            //            EikoMougMechanic(v);
            //        }
            //     );
            //}
        }

        public static void SOS_SA(BattleCalculator v)
        {
            var targetState = v.TargetState();

            bool isHpBelowHalf = v.Target.CurrentHp <= (v.Target.MaximumHp / 2);
            bool isLowHp = v.Target.IsUnderAnyStatus(BattleStatus.LowHP);

            if (!isHpBelowHalf)
            {
                targetState.SpecialSA.OneTriggerSOS &= ~(1 | 4 | 16 | 64 | 256 | 1024);
            }

            if (!isLowHp)
            {
                targetState.SpecialSA.OneTriggerSOS &= ~(2 | 8 | 32 | 128 | 512 | 2048);
            }

            void CheckAndTriggerSOS(SupportAbility normalAbility, SupportAbility boostedAbility, BattleStatus statusToApply, int normalBit, int boostedBit)
            {
                bool hasBoosted = v.Target.HasSupportAbilityByIndex(boostedAbility);
                bool hasNormal = v.Target.HasSupportAbilityByIndex(normalAbility);

                if (hasBoosted && isHpBelowHalf && (targetState.SpecialSA.OneTriggerSOS & boostedBit) == 0)
                {
                    v.Target.AlterStatus(statusToApply, v.Target);
                    targetState.SpecialSA.OneTriggerSOS |= boostedBit;
                }
                else if (hasNormal && !hasBoosted && isLowHp && (targetState.SpecialSA.OneTriggerSOS & normalBit) == 0)
                {
                    v.Target.AlterStatus(statusToApply, v.Target);
                    targetState.SpecialSA.OneTriggerSOS |= normalBit;
                }
            }

            CheckAndTriggerSOS(TranceSeekSupportAbility.SOS_Protect, TranceSeekSupportAbility.SOS_Protect_Boosted, BattleStatus.Protect, 2, 1);
            CheckAndTriggerSOS(TranceSeekSupportAbility.SOS_Shell, TranceSeekSupportAbility.SOS_Shell_Boosted, BattleStatus.Shell, 8, 4);
            CheckAndTriggerSOS(TranceSeekSupportAbility.SOS_Regen, TranceSeekSupportAbility.SOS_Regen_Boosted, BattleStatus.Regen, 32, 16);
            CheckAndTriggerSOS(TranceSeekSupportAbility.SOS_Haste, TranceSeekSupportAbility.SOS_Haste_Boosted, BattleStatus.Haste, 128, 64);
            CheckAndTriggerSOS(TranceSeekSupportAbility.SOS_Reflect, TranceSeekSupportAbility.SOS_Reflect_Boosted, BattleStatus.Reflect, 512, 256);
            CheckAndTriggerSOS(TranceSeekSupportAbility.SOS_Vanish, TranceSeekSupportAbility.SOS_Vanish_Boosted, BattleStatus.Vanish, 2048, 1024);
        }
    }
}
