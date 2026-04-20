using Memoria.Data;
using System;

namespace Memoria.Scripts.TranceSeek
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
            if (_v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.StoneSkin_Boosted) && !_v.Caster.IsPlayer)
                return;

            TranceSeekAPI.ChangeRow(_v.Target);          
        }
    }
}

