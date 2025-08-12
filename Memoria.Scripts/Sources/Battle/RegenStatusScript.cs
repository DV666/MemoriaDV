using System;
using Memoria.Data;
using Object = System.Object;
using Memoria.Scripts.Battle;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.Regen)]
    public class RegenStatusScript : StatusScriptBase, IOprStatusScript
    {
        public BattleUnit RegenInflicter = null;

        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            RegenInflicter = inflicter;
            TranceSeekAPI.SA_StatusApply(inflicter, true);
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            return true;
        }

        public IOprStatusScript.SetupOprMethod SetupOpr => SetupRegenOpr;
        public Int32 SetupRegenOpr()
        {
            return 600;
        }
        public Boolean OnOpr()
        {
            if (Target.IsUnderAnyStatus(BattleStatus.Petrify))
                return false;

            Boolean isDmg = false;
            if (Target.HasSupportAbilityByIndex((SupportAbility)130)) // SA Harmony
            {
                UInt32 healMP = Target.HasSupportAbilityByIndex((SupportAbility)1130) ? (Target.MaximumMp / 50) : (Target.MaximumMp / 100);
                if (Target.IsZombie)
                {
                    isDmg = true;
                    if (Target.CurrentMp > healMP)
                        Target.CurrentMp -= healMP;
                    else
                        Target.Kill(RegenInflicter);
                }
                else
                {
                    Target.CurrentMp = Math.Min(Target.CurrentMp + healMP, Target.MaximumMp);
                }
                btl2d.Btl2dStatReq(Target, 0, isDmg ? (Int32)healMP : -(Int32)healMP);
            }
            else
            {
                UInt32 healHP = Target.MaximumHp >> (Target.IsUnderAnyStatus(BattleStatus.EasyKill) ? 7 : 5);
                if (Target.HasSupportAbilityByIndex((SupportAbility)129)) // SA Rejuvenate
                    healHP += Target.HasSupportAbilityByIndex((SupportAbility)1129) ? (healHP / 2) : (healHP / 4);

                if (Target.IsZombie)
                {
                    isDmg = true;
                    if (Target.CurrentHp > healHP)
                        Target.CurrentHp -= healHP;
                    else
                        Target.Kill(RegenInflicter);
                }
                else
                {
                    Target.CurrentHp = Math.Min(Target.CurrentHp + healHP, Target.MaximumHp);
                }
                btl2d.Btl2dStatReq(Target, isDmg ? (Int32)healHP : -(Int32)healHP, 0);
            }
            BattleVoice.TriggerOnStatusChange(Target, BattleVoice.BattleMoment.Used, BattleStatusId.Regen);
            return false;
        }
    }
}
