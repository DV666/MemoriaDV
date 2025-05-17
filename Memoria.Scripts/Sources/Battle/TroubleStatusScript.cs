using System;
using Memoria.Data;
using FF9;
using Object = System.Object;
using Memoria.Scripts.Battle;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.Trouble)]
    public class TroubleStatusScript : StatusScriptBase, IFigurePointStatusScript
    {
        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            if (target.IsUnderAnyStatus(BattleStatus.EasyKill))
            {
                BattleStatusDataEntry statusData = FF9StateSystem.Battle.FF9Battle.status_data[BattleStatusId.Poison];
                Int32 wait = (short)(((400 + (inflicter.Will * 2) - target.Will) * statusData.ContiCnt) * (inflicter.HasSupportAbilityByIndex((SupportAbility)1124) ? (150 / 100) : inflicter.HasSupportAbilityByIndex((SupportAbility)124) ? (125 / 100) : 1));
                target.AddDelayedModifier(
                target => (wait -= target.Data.cur.at_coef * BattleState.ATBTickCount) > 0,
                target =>
                {
                    target.RemoveStatus(BattleStatus.Trouble);
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

        public void OnFigurePoint(ref UInt16 fig_info, ref Int32 fig, ref Int32 m_fig)
        {
            if ((fig_info & Param.FIG_INFO_TROUBLE) == 0)
                return;
            if ((fig_info & Param.FIG_INFO_DISP_HP) == 0)
                return;
            if ((fig_info & (Param.FIG_INFO_HP_RECOVER | Param.FIG_INFO_GUARD | Param.FIG_INFO_MISS | Param.FIG_INFO_DEATH)) != 0)
                return;
            Int32 dmg = fig >> 1;
            foreach (BattleUnit unit in FF9StateSystem.Battle.FF9Battle.EnumerateBattleUnits())
            {
                if (unit.IsPlayer == Target.IsPlayer && unit.Id != Target.Id && unit.IsTargetable && !unit.IsUnderAnyStatus(BattleStatus.Death))
                {
                    btl_para.SetDamage(unit, dmg, 0, requestFigureNow: true);
                    BattleVoice.TriggerOnStatusChange(Target, BattleVoice.BattleMoment.Used, BattleStatusId.Trouble);
                }
            }
        }
    }
}
