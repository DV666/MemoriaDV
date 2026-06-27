using FF9;
using Memoria.Data;
using System;
using System.Collections.Generic;

namespace Memoria.Scripts.TranceSeek
{
    /// <summary>
    /// Grand Cross
    /// </summary>
    [BattleScript(Id)]
    public sealed class GrandCrossScript : IBattleScript
    {
        public const Int32 Id = 0105;

        private readonly BattleCalculator _v;

        public GrandCrossScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            List<BattleStatus> alteringStatuses = new List<BattleStatus>();
            int ChanceProc = 15; // (12.5% in vanilla)
            TranceSeekAPI.MagicAccuracy(_v);
            _v.Target.PenaltyShellHitRate();

            if (_v.Caster.Data.dms_geo_id == 166) // Thousand Fears (from Dark Beatrix)
            {
                int MagicAndLevel = _v.Caster.Magic + _v.Caster.Level;
                _v.Context.Attack = UnityEngine.Random.Range(MagicAndLevel / 4, (MagicAndLevel * 3) / 2);
                _v.SetCommandPower();
                _v.Target.SetMagicDefense();
                TranceSeekAPI.CasterPenaltyMini(_v);
                TranceSeekAPI.EnemyTranceBonusAttack(_v);
                TranceSeekAPI.PenaltyShellAttack(_v);
                TranceSeekAPI.PenaltyCommandDividedAttack(_v);
                TranceSeekAPI.BonusElement(_v);
                if (TranceSeekAPI.CanAttackMagic(_v))
                    _v.CalcHpDamage();

                ChanceProc = 25;
                alteringStatuses = new List<BattleStatus>{ BattleStatus.Sleep, BattleStatus.Blind, BattleStatus.Silence, BattleStatus.Doom, BattleStatus.Berserk,
                BattleStatus.Confuse, BattleStatus.Freeze, BattleStatus.GradualPetrify, BattleStatus.Slow, BattleStatus.Virus, BattleStatus.Trouble,
                    TranceSeekStatus.PowerBreak, TranceSeekStatus.MagicBreak, TranceSeekStatus.MentalBreak, TranceSeekStatus.MentalBreak};
            }
            else
            {
                if (_v.Command.Power == 1) // Friendly Mu - Surprise
                {
                    alteringStatuses = new List<BattleStatus>{ BattleStatus.Silence, BattleStatus.Blind, BattleStatus.Trouble, BattleStatus.Slow,
                     BattleStatus.Death, BattleStatus.Confuse, BattleStatus.Berserk, BattleStatus.Poison,
                     BattleStatus.Sleep, BattleStatus.Heat, BattleStatus.Freeze, BattleStatus.Doom, BattleStatus.Venom };
                }
                else
                {
                    alteringStatuses = new List<BattleStatus>{BattleStatus.Petrify, BattleStatus.Silence, BattleStatus.Blind, BattleStatus.Trouble, BattleStatus.Slow,
                     BattleStatus.Death, BattleStatus.Confuse, BattleStatus.Berserk, BattleStatus.Poison, BattleStatus.Zombie, BattleStatus.Stop,
                     BattleStatus.Sleep, BattleStatus.Heat, BattleStatus.Freeze, BattleStatus.Doom, BattleStatus.Mini, BattleStatus.LowHP };
                }
            }


            if (!_v.Target.CanBeAttacked())
                return;

            foreach (BattleStatus status in alteringStatuses)
            {
                if (Comn.random16() % 100 > ChanceProc)
                    continue;

                if (TranceSeekAPI.TryMagicHitWithoutBattleCalcFlag(_v))
                {
                    if ((status & BattleStatus.LowHP) != 0 && !_v.Target.IsUnderStatus(BattleStatus.Death))
                    {
                        _v.Context.Flags |= BattleCalcFlags.DirectHP;
                        _v.Target.CurrentHp = (UInt32)(1 + GameRandom.Next8() % 9);
                    }
                    else
                        _v.Command.AbilityStatus |= status;
                }
            }
            
            TranceSeekAPI.TryAlterCommandStatuses(_v);
        }
    }
}
