using Memoria.Data;
using System;
using static Memoria.Scripts.Battle.TranceSeekCustomAPI;

namespace Memoria.Scripts.Battle
{
    public class OverloadOnBattleScriptStartScript : IOverloadOnBattleScriptStartScript
    {
        public Boolean OnBattleScriptStart(BattleCalculator v)
        {
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
            return false;
        }
    }
}
