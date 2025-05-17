using System;
using Memoria.Data;
using Memoria.Scripts.Battle;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.Stop)]
    public class StopStatusScript : StatusScriptBase
    {
        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            target.UISpriteATB = BattleHUD.ATEGray;
            if (Target.IsUnderAnyStatus(BattleStatus.EasyKill))
            {
                if (TranceSeekAPI.MonsterMechanic[target.Data][4] > 0)
                {
                    BattleStatusDataEntry statusData = FF9StateSystem.Battle.FF9Battle.status_data[BattleStatusId.Poison];
                    Int32 wait = (short)(((200 + (inflicter.Will * 2) - target.Will) * statusData.ContiCnt) * (inflicter.HasSupportAbilityByIndex((SupportAbility)1124) ? (150 / 100) : inflicter.HasSupportAbilityByIndex((SupportAbility)124) ? (125 / 100) : 1)); ;
                    wait = (wait * TranceSeekAPI.MonsterMechanic[target.Data][4]) / 100;
                    Target.AddDelayedModifier(
                    target => (wait -= target.Data.cur.at_coef * BattleState.ATBTickCount) > 0,
                    target =>
                    {
                        target.RemoveStatus(BattleStatus.Stop);
                    }
                    );
                    TranceSeekAPI.MonsterMechanic[target.Data][4] -= 20;
                }
                else
                    return btl_stat.ALTER_RESIST;
            }
            TranceSeekAPI.SA_StatusApply(inflicter, false);
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            Target.UISpriteATB = BattleHUD.ATENormal;
            return true;
        }
    }
}
