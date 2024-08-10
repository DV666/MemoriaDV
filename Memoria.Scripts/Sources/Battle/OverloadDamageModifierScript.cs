using FF9;
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
                if (v.Target.PlayerIndex == (CharacterId)12) // Lani's Rage Mechanic
                    v.Target.AlterStatus(CustomStatus.Rage, v.Caster);
            }
            if ((v.Target.Flags & CalcFlag.MpAlteration) != 0)
                v.Target.MpDamage = (Int32)Math.Round(modifier_factor * v.Target.MpDamage) * reflectMultiplier;

            if ((v.Caster.Flags & CalcFlag.HpAlteration) != 0)
                v.Caster.HpDamage = (Int32)Math.Round(modifier_factor * v.Caster.HpDamage) * reflectMultiplier;
            if ((v.Caster.Flags & CalcFlag.MpAlteration) != 0)
                v.Caster.MpDamage = (Int32)Math.Round(modifier_factor * v.Caster.MpDamage) * reflectMultiplier;

            if (Configuration.TetraMaster.TripleTriad == 16389 && v.Caster.IsPlayer)
                v.Target.HpDamage = 9999;

            TranceSeekCustomAPI.SpecialSA(v);
        }
    }
}
