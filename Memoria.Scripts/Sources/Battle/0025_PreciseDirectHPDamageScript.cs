using System;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Roulette, Photon
    /// </summary>
    [BattleScript(Id)]
    public sealed class PreciseDirectHPDamageScript : IBattleScript
    {
        public const Int32 Id = 0025;

        private readonly BattleCalculator _v;

        public PreciseDirectHPDamageScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (TranceSeekAPI.CheckUnsafetyOrGuard(_v) && _v.Target.CanBeAttacked())
                _v.TryDirectHPDamage();
        }
    }
}
