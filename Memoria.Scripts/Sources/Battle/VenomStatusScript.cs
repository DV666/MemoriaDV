using System;
using Memoria.Data;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.Venom)]
    public class VenomStatusScript : StatusScriptBase, IOprStatusScript
    {
        public BattleUnit VenomInflicter = null;

        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            VenomInflicter = inflicter;
            if (target.IsUnderAnyStatus(BattleStatus.EasyKill))
            {
                BattleStatusDataEntry statusData = FF9StateSystem.Battle.FF9Battle.status_data[BattleStatusId.Poison];
                Int32 wait = (short)(((400 + (inflicter.Will * 2) - target.Will) * statusData.ContiCnt) * (inflicter.HasSupportAbilityByIndex((SupportAbility)1124) ? (150 / 100) : inflicter.HasSupportAbilityByIndex((SupportAbility)124) ? (125 / 100) : 1));
                target.AddDelayedModifier(
                target => (wait -= target.Data.cur.at_coef * BattleState.ATBTickCount) > 0,
                target =>
                {
                    target.RemoveStatus(BattleStatus.Venom);
                }
                );
            }    
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            return true;
        }

        public IOprStatusScript.SetupOprMethod SetupOpr => SetupVenomOpr;
        public Int32 SetupVenomOpr()
        {
            return (Target.IsUnderAnyStatus(BattleStatus.EasyKill) || Target.IsPlayer && FF9StateSystem.EventState.gEventGlobal[1403] == 1) ? 600 : 100;
        }
        public Boolean OnOpr()
        {
            if (Target.IsUnderAnyStatus(BattleStatus.Petrify))
                return false;
            UInt32 HPdamage = Target.MaximumHp >> (Target.IsUnderAnyStatus(BattleStatus.EasyKill) ? 7 : 4);
            UInt32 MPdamage = 0;
            if (!Target.IsUnderAnyStatus(BattleStatus.EasyKill))
            {
                MPdamage = Target.MaximumMp >> 5;
                if (Target.IsZombie)
                    MPdamage /= 2;
                if (Target.CurrentMp > MPdamage)
                    Target.CurrentMp -= MPdamage;
                else
                    Target.CurrentMp = 0;
            }
            if (Target.IsZombie)
                HPdamage /= 2;
            if (Target.CurrentHp > HPdamage)
                Target.CurrentHp -= HPdamage;
            else
                Target.Kill(VenomInflicter);
            btl2d.Btl2dStatReq(Target, (Int32)HPdamage, (Int32)MPdamage);
            BattleVoice.TriggerOnStatusChange(Target, "Used", BattleStatusId.Venom);
            return false;
        }
    }
}
