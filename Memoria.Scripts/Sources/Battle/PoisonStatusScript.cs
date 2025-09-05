using System;
using Memoria.Data;
using FF9;
using Object = System.Object;
using Memoria.Scripts.Battle;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.Poison)]
    public class PoisonStatusScript : StatusScriptBase, IOprStatusScript
    {
        public BattleUnit PoisonInflicter = null;

        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            PoisonInflicter = inflicter;
            if (Target.IsUnderAnyStatus(BattleStatus.EasyKill))
            {
                BattleStatusDataEntry statusData = FF9StateSystem.Battle.FF9Battle.status_data[BattleStatusId.Poison];
                Int32 wait = (short)(((400 + (inflicter.Will * 2) - target.Will) * statusData.ContiCnt) * (inflicter.HasSupportAbilityByIndex((SupportAbility)1124) ? (150 / 100) : inflicter.HasSupportAbilityByIndex((SupportAbility)124) ? (125 / 100) : 1));
                Target.AddDelayedModifier(
                target => (wait -= target.Data.cur.at_coef * BattleState.ATBTickCount) > 0,
                target =>
                {
                    target.RemoveStatus(BattleStatus.Poison);
                }
                );
            }
            TranceSeekAPI.SA_StatusApply(inflicter, false);
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            return true;
        }

        public IOprStatusScript.SetupOprMethod SetupOpr => SetupPoisonOpr;
        public Int32 SetupPoisonOpr()
        {
            return 600;
        }
        public Boolean OnOpr()
        {
            if (Target.IsUnderAnyStatus(BattleStatus.Petrify))
                return false;
            UInt32 damage = Target.MaximumHp >> (Target.IsUnderAnyStatus(BattleStatus.EasyKill) ? 8 : 5);
            Boolean isDmg = false;

            if (!Target.IsZombie && (Int32)Target.GetPropertyByName("StatusProperty CustomStatus21 CursedBlood") == 0 && (TranceSeekAPI.NewEffectElement[Target.Data][0] & 8) == 0)
            {
                isDmg = true;
                if (Target.CurrentHp > damage)
                    Target.CurrentHp -= damage;
                else
                    Target.Kill(PoisonInflicter);
            }
            else
            {
                Target.CurrentHp = Math.Min(Target.CurrentHp + damage, Target.MaximumHp);
            }
            btl2d.Btl2dStatReq(Target, isDmg ? (Int32)damage : -(Int32)damage, 0);
            BattleVoice.TriggerOnStatusChange(Target, BattleVoice.BattleMoment.Used, BattleStatusId.Poison);
            return false;
        }
    }
}
