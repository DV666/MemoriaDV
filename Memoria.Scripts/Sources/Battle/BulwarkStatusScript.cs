using System;
using Memoria.Data;
using Memoria.Scripts.Battle;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.CustomStatus13)] // Bulwark
    public class BulwarkStatusScript : StatusScriptBase
    {
        public Boolean ShowNumberHUD;

        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            OverlapSHP.SetupOverlappingSHP2(target);
            TranceSeekAPI.SA_StatusApply(inflicter, true);
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            return true;
        }

        public void OnSHPShow(Boolean show)
        {
            ShowNumberHUD = show;
        }
    }
}
