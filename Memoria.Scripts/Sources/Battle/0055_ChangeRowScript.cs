using Memoria.Data;
using System;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Change
    /// </summary>
    [BattleScript(Id)]
    public sealed class ChangeRowScript : IBattleScript
    {
        public const Int32 Id = 0055;

        private readonly BattleCalculator _v;

        public ChangeRowScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (!_v.Target.HasSupportAbilityByIndex((SupportAbility)1026))
                _v.Target.ChangeRow();
        }
    }
}
