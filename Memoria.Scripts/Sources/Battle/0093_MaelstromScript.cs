using Memoria.Prime;
using System;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Maelstrom
    /// </summary>
    [BattleScript(Id)]
    public sealed class MaelstromScript : IBattleScript
    {
        public const Int32 Id = 0093;

        private readonly BattleCalculator _v;

        public MaelstromScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (TranceSeekAPI.CheckUnsafetyOrGuard(_v) && _v.Target.CanBeAttacked())
            {
                TranceSeekAPI.MagicAccuracy(_v);
                _v.Target.PenaltyShellHitRate();
                _v.PenaltyCommandDividedHitRate();
                _v.Command.HitRate += _v.Caster.Level - _v.Target.Level;
                TranceSeekAPI.ReduceAccuracyEliteMonsters(_v, true);
                if (TranceSeekAPI.TryMagicHit(_v) || _v.Command.HitRate == 255)
                {
                    _v.Context.Flags |= BattleCalcFlags.DirectHP;
                    if (_v.Caster.Data.dms_geo_id == 401) // Friendly Feather Circle - Heartless Angel
                    {
                        _v.Target.CurrentHp = 1;
                        return;
                    }
                    if (_v.Target.CurrentHp < 10U)
                    {
                        _v.Target.CurrentHp = (uint)(1L + GameRandom.Next8() % _v.Target.CurrentHp);
                    }
                    else
                    {
                        _v.Target.CurrentHp = (uint)(1 + GameRandom.Next8() % 9);
                    }
                    TranceSeekAPI.TryAlterCommandStatuses(_v);
                }
            }
        }
    }
}
