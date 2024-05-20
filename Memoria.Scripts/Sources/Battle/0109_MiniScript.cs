using System;
using Memoria.Data;

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
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            if (_v.Target.IsUnderAnyStatus(BattleStatus.Mini))
            {
                TranceSeekCustomAPI.TryAlterCommandStatuses(_v);
                return;
            }

            _v.MagicAccuracy();
            _v.Target.PenaltyShellHitRate();
            _v.PenaltyCommandDividedHitRate();
            if (_v.TryMagicHit())
                TranceSeekCustomAPI.TryAlterCommandStatuses(_v);
            TranceSeekCustomAPI.SpecialSA(_v);
        }
    }
}