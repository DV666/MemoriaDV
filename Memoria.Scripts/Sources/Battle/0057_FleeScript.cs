using FF9;
using Memoria.Data;
using System;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Flee
    /// </summary>
    [BattleScript(Id)]
    public sealed class FleeScript : IBattleScript
    {
        public const Int32 Id = 0057;

        private readonly BattleCalculator _v;

        public FleeScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            btl_sys.CheckEscape(false);
            if (_v.CanEscape())
            {
                if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1046)) // Flee-Gil +
                {
                    btl_cmd.SetCommand(FF9StateSystem.Battle.FF9Battle.cmd_escape, BattleCommandId.SysEscape, 1, 15, 1U);
                }
                else
                {
                    BattleState.EnqueueCommand(BattleState.EscapeCommand, BattleCommandId.SysEscape, 0U, 15, true);
                }
            }
        }
    }
}
