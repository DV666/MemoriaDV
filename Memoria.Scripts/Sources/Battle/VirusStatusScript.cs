using System;
using Memoria.Data;
using Memoria.Scripts.Battle;
using UnityEngine;
using static SiliconStudio.Social.ResponseData;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.Virus)]
    public class VirusStatusScript : StatusScriptBase, IOprStatusScript
    {
        public BattleUnit VirusInflicter = null;

        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            target.AddDelayedModifier(HideSHP, null);
            VirusInflicter = inflicter;
            TranceSeekAPI.SA_StatusApply(inflicter, false);
            return btl_stat.ALTER_SUCCESS;
        }

        private Boolean HideSHP(BattleUnit unit)
        {
            if (!unit.IsUnderAnyStatus(BattleStatusId.Virus))
                return false;
            SHPEffect shp = HonoluluBattleMain.battleSPS.GetBtlSHPObj(unit, BattleStatusId.Virus);
            if (unit.IsPlayer && shp != null)
                shp.attr |= SPSConst.ATTR_HIDDEN;
            return true;
        }

        public override Boolean Remove()
        {
            return true;
        }
        public IOprStatusScript.SetupOprMethod SetupOpr => null;
        public Boolean OnOpr()
        {
            if (Target.IsUnderAnyStatus(BattleStatus.Petrify))
                return false;

            if (Target.CurrentHp > 0)
                Target.CurrentHp -= 1;
            else
                Target.Kill(VirusInflicter);
            BattleVoice.TriggerOnStatusChange(Target, BattleVoice.BattleMoment.Used, BattleStatusId.Virus);
            return false;
        }
    }
}
