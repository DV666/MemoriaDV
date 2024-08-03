using System;
using Memoria.Data;
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
            VirusInflicter = inflicter;
            return btl_stat.ALTER_SUCCESS;
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
            BattleVoice.TriggerOnStatusChange(Target, "Used", BattleStatusId.Virus);
            return false;
        }
    }
}
