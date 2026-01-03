using Memoria.Data;
using Memoria.Prime;
using System;
using System.Collections;
using System.Runtime.InteropServices;

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
            if (_v.Command.Power == 111 && _v.Command.HitRate == 111)
            {
                uint dice = (uint)(GameRandom.Next8() % 4);
                if (dice < 3)
                {
                    BattleStatusId statusId = dice switch
                    {
                        0 => BattleStatusId.Death,
                        1 => BattleStatusId.Doom,
                        2 => BattleStatusId.Stop,
                        _ => BattleStatusId.Death
                    };

                    btl_stat.AlterStatus(_v.Target, statusId, _v.Caster);
                }
                else if (_v.Target.CurrentHp > 1)
                {
                    _v.Target.CurrentHp = (uint)(1 + GameRandom.Next8() % _v.Target.CurrentHp);
                }
            }
            else if (TranceSeekAPI.CheckUnsafetyOrGuard(_v) && _v.Target.CanBeAttacked())
            {
                Boolean ForceMaelstrom = (_v.Command.HitRate == 255);
                TranceSeekAPI.MagicAccuracy(_v);
                _v.Target.PenaltyShellHitRate();
                _v.PenaltyCommandDividedHitRate();
                _v.Command.HitRate += _v.Caster.Level - _v.Target.Level;
                TranceSeekAPI.ReduceAccuracyEliteMonsters(_v, true);
                if (TranceSeekAPI.TryMagicHit(_v) || ForceMaelstrom)
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
                    TranceSeekAPI.TryAlterCommandStatuses(_v, false);
                }
            }
        }
    }
}
