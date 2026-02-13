using Memoria.Data;
using System;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Iai Strike
    /// </summary>
    [BattleScript(Id)]
    public sealed class MiniScript : IBattleScript
    {
        public const Int32 Id = 0109;

        private readonly BattleCalculator _v;

        public MiniScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Target.IsUnderAnyStatus(BattleStatus.Mini) || _v.Command.HitRate == 255)
            {
                TranceSeekAPI.TryAlterCommandStatuses(_v);
                return;
            }

            TranceSeekAPI.MagicAccuracy(_v);
            _v.Target.PenaltyShellHitRate();
            _v.PenaltyCommandDividedHitRate();
            if (TranceSeekAPI.TryMagicHit(_v))
                TranceSeekAPI.TryAlterCommandStatuses(_v);
        }
    }
}
