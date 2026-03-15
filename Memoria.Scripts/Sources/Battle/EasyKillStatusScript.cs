using System;
using Memoria.Data;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.EasyKill)]
    public class EasyKillStatusScript : StatusScriptBase
    {
        public Boolean DeathResistanceAdded = false;
        public Boolean OldResistanceAdded = false;

        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            DeathResistanceAdded = DeathResistanceAdded || (target.ResistStatus & BattleStatus.Death) == 0;
            target.ResistStatus |= BattleStatus.Death;
            if (!target.IsPlayer)
            {
                OldResistanceAdded = OldResistanceAdded || (target.ResistStatus & BattleStatus.CustomStatus16) == 0;
                target.ResistStatus |= BattleStatus.CustomStatus16; // Old
            }
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            if (DeathResistanceAdded)
                Target.ResistStatus &= ~BattleStatus.Death;
            if (OldResistanceAdded)
                Target.ResistStatus &= ~BattleStatus.CustomStatus16;
            return true;
        }
    }
}
