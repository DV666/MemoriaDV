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
            if (v.Target.CurrentHp > (v.Target.MaximumHp / 2) && SpecialSAEffect[v.Target.Data][14] > 0 &&
                (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Protect_Boosted) || v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Shell_Boosted) ||
                v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Regen_Boosted) || v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Haste_Boosted) ||
                v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Reflect_Boosted) || v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Vanish_Boosted)))
            {
                if ((SpecialSAEffect[v.Target.Data][14] & 1) > 0)
                    SpecialSAEffect[v.Target.Data][14] -= 1;
                if ((SpecialSAEffect[v.Target.Data][14] & 4) > 0)
                    SpecialSAEffect[v.Target.Data][14] -= 4;
                if ((SpecialSAEffect[v.Target.Data][14] & 16) > 0)
                    SpecialSAEffect[v.Target.Data][14] -= 16;
                if ((SpecialSAEffect[v.Target.Data][14] & 64) > 0)
                    SpecialSAEffect[v.Target.Data][14] -= 64;
                if ((SpecialSAEffect[v.Target.Data][14] & 256) > 0)
                    SpecialSAEffect[v.Target.Data][14] -= 256;
                if ((SpecialSAEffect[v.Target.Data][14] & 1024) > 0)
                    SpecialSAEffect[v.Target.Data][14] -= 1024;
            }
            else if (!v.Target.IsUnderAnyStatus(BattleStatus.LowHP) && SpecialSAEffect[v.Target.Data][14] > 0 &&
                (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Protect) || v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Shell) ||
                v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Regen) || v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Haste) ||
                v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Reflect) || v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Vanish)))
            {
                if ((SpecialSAEffect[v.Target.Data][14] & 2) > 0)
                    SpecialSAEffect[v.Target.Data][14] -= 2;
                if ((SpecialSAEffect[v.Target.Data][14] & 8) > 0)
                    SpecialSAEffect[v.Target.Data][14] -= 8;
                if ((SpecialSAEffect[v.Target.Data][14] & 32) > 0)
                    SpecialSAEffect[v.Target.Data][14] -= 32;
                if ((SpecialSAEffect[v.Target.Data][14] & 128) > 0)
                    SpecialSAEffect[v.Target.Data][14] -= 128;
                if ((SpecialSAEffect[v.Target.Data][14] & 512) > 0)
                    SpecialSAEffect[v.Target.Data][14] -= 512;
                if ((SpecialSAEffect[v.Target.Data][14] & 2048) > 0)
                    SpecialSAEffect[v.Target.Data][14] -= 2048;
            }

            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Protect_Boosted) && (SpecialSAEffect[v.Target.Data][14] & 1) == 0 && v.Target.CurrentHp <= (v.Target.MaximumHp / 2))
            {
                v.Target.AlterStatus(BattleStatus.Protect, v.Target);
                SpecialSAEffect[v.Target.Data][14] += 1;
            }
            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Protect) && (SpecialSAEffect[v.Target.Data][14] & 2) == 0 && v.Target.IsUnderAnyStatus(BattleStatus.LowHP) && !v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Protect_Boosted))
            {
                v.Target.AlterStatus(BattleStatus.Protect, v.Target);
                SpecialSAEffect[v.Target.Data][14] += 2;
            }
            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Shell_Boosted) && (SpecialSAEffect[v.Target.Data][14] & 4) == 0 && v.Target.CurrentHp <= (v.Target.MaximumHp / 2))
            {
                v.Target.AlterStatus(BattleStatus.Shell, v.Target);
                SpecialSAEffect[v.Target.Data][14] += 4;
            }
            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Shell) && (SpecialSAEffect[v.Target.Data][14] & 8) == 0 && v.Target.IsUnderAnyStatus(BattleStatus.LowHP) && !v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Shell_Boosted))
            {
                v.Target.AlterStatus(BattleStatus.Shell, v.Target);
                SpecialSAEffect[v.Target.Data][14] += 8;
            }
            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Regen_Boosted) && (SpecialSAEffect[v.Target.Data][14] & 16) == 0 && v.Target.CurrentHp <= (v.Target.MaximumHp / 2))
            {
                v.Target.AlterStatus(BattleStatus.Regen, v.Target);
                SpecialSAEffect[v.Target.Data][14] += 16;
            }
            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Regen) && (SpecialSAEffect[v.Target.Data][14] & 32) == 0 && v.Target.IsUnderAnyStatus(BattleStatus.LowHP) && !v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Regen_Boosted)) 
            {
                v.Target.AlterStatus(BattleStatus.Regen, v.Target);
                SpecialSAEffect[v.Target.Data][14] += 32;
            }
            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Haste_Boosted) && (SpecialSAEffect[v.Target.Data][14] & 64) == 0 && v.Target.CurrentHp <= (v.Target.MaximumHp / 2))
            {
                v.Target.AlterStatus(BattleStatus.Haste, v.Target);
                SpecialSAEffect[v.Target.Data][14] += 64;
            }
            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Haste) && (SpecialSAEffect[v.Target.Data][14] & 128) == 0 && v.Target.IsUnderAnyStatus(BattleStatus.LowHP) && !v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Haste_Boosted))
            {
                v.Target.AlterStatus(BattleStatus.Haste, v.Target);
                SpecialSAEffect[v.Target.Data][14] += 128;
            }
            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Reflect_Boosted) && (SpecialSAEffect[v.Target.Data][14] & 256) == 0 && v.Target.CurrentHp <= (v.Target.MaximumHp / 2))
            {
                v.Target.AlterStatus(BattleStatus.Reflect, v.Target);
                SpecialSAEffect[v.Target.Data][14] += 256;
            }
            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Reflect) && (SpecialSAEffect[v.Target.Data][14] & 512) == 0 && v.Target.IsUnderAnyStatus(BattleStatus.LowHP) && !v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Reflect_Boosted))
            {
                v.Target.AlterStatus(BattleStatus.Reflect, v.Target);
                SpecialSAEffect[v.Target.Data][14] += 512;
            }
            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Vanish_Boosted) && (SpecialSAEffect[v.Target.Data][14] & 1024) == 0 && v.Target.CurrentHp <= (v.Target.MaximumHp / 2))
            {
                v.Target.AlterStatus(BattleStatus.Vanish, v.Target);
                SpecialSAEffect[v.Target.Data][14] += 1024;
            }
            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Vanish) && (SpecialSAEffect[v.Target.Data][14] & 2048) == 0 && v.Target.IsUnderAnyStatus(BattleStatus.LowHP) && !v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Vanish_Boosted))
            {
                v.Target.AlterStatus(BattleStatus.Vanish, v.Target);
                SpecialSAEffect[v.Target.Data][14] += 2048;
            }
        }
    }
}
