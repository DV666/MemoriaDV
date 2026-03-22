using System;
using UnityEngine;
using Memoria.Data;
using Object = System.Object;
using Memoria.Scripts.Battle;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.Float)]
    public class FloatStatusScript : StatusScriptBase
    {
        public Boolean CurrentlyDodging = false;

        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            target.AddDelayedModifier(HoveringMovement, null);
            TranceSeekAPI.SA_StatusApply(inflicter, true);
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            Target.ChangePositionCoordinate(0f, 1, true, true);
            return true;
        }

        private Boolean HoveringMovement(BattleUnit unit)
        {
            if (!unit.IsUnderAnyStatus(BattleStatus.Float))
                return false;
            if (!unit.IsUnderPermanentStatus(BattleStatus.Float) || unit.IsNonMorphedPlayer)
            {
                Single height = 200 + (Int32)(30 * Math.Sin(Math.PI * (FF9StateSystem.Battle.FF9Battle.btl_cnt & 15) / 8f));
                unit.ChangePositionCoordinate(height, 1, true, true);
            }
            return true;
        }
    }
}
