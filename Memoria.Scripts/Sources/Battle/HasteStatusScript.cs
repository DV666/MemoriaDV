using FF9;
using Memoria.Data;
using Memoria.Scripts.Battle;
using System;
using System.Collections.Generic;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.Haste)]
    public class HasteStatusScript : StatusScriptBase
    {
        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            if (target.IsUnderStatus(BattleStatus.Slow))
            {
                if (btl_stat.RemoveStatus(target, BattleStatusId.Slow) == 2)
                    return btl_stat.ALTER_SUCCESS_NO_SET;
                return btl_stat.ALTER_RESIST;
            }
            btl_para.SetupATBCoef(target, btl_para.GetATBCoef() * 3 / 2);
            target.UISpriteATB = Target.IsUnderAnyStatus(BattleStatus.Stop) ? BattleHUD.ATEGray : BattleHUD.ATEOrange;
            //target.AddDelayedModifier(DoubleSpeedAnimation, null);
            TranceSeekAPI.SA_StatusApply(inflicter, true);
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            btl_para.SetupATBCoef(Target, btl_para.GetATBCoef());
            Target.Data.animSpeed = 1f;
            Target.UISpriteATB = Target.IsUnderAnyStatus(BattleStatus.Stop | BattleStatus.Slow) ? BattleHUD.ATEGray : BattleHUD.ATENormal;
            return true;
        }

        private Boolean DoubleSpeedAnimation(BattleUnit target) // Not perfect, testing for the fun.
        {
            if (!target.IsUnderAnyStatus(BattleStatus.Haste))
                return false;

            target.Data.animSpeed = 2f;
            return true;
        }
    }
}
