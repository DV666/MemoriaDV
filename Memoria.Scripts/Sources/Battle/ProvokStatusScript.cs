using System;
using Memoria.Data;
using Memoria.Prime;
using static SiliconStudio.Social.ResponseData;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.CustomStatus22)] // Special
    public class ProvokStatusScript : StatusScriptBase
    {
        public Int32 ForcedTargetId;

        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            ForcedTargetId = inflicter.Id;
            target.PartialResistStatus[BattleStatusId.CustomStatus22] = Math.Min(target.PartialResistStatus[BattleStatusId.CustomStatus22] + 0.20f, 1f);
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            return true;
        }
    }
}
