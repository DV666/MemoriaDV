using System;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Flare Star
    /// </summary>
    [BattleScript(Id)]
    public sealed class FlareStarScript : IBattleScript
    {
        public const Int32 Id = 0099;

        private readonly BattleCalculator _v;

        public FlareStarScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            TranceSeekAPI.MagicAccuracy(_v);
            TranceSeekAPI.EnemyTranceBonusAttack(_v);
            _v.Target.PenaltyShellHitRate();
            _v.PenaltyCommandDividedHitRate();
            if (TranceSeekAPI.TryMagicHit(_v))
            {
                _v.Target.Flags |= CalcFlag.HpAlteration;
                _v.Target.HpDamage = _v.Target.Level * _v.Command.Power;
            }
        }
    }
}
