﻿using System;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Magic Break
    /// </summary>
    [BattleScript(Id)]
    public sealed class MagicMagicBreakScript : IBattleScript, IEstimateBattleScript
    {
        public const Int32 Id = 0136;

        private readonly BattleCalculator _v;

        public MagicMagicBreakScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            _v.NormalMagicParams();
            TranceSeekCustomAPI.CharacterBonusPassive(_v, "MagicAttack");
            TranceSeekCustomAPI.CasterPenaltyMini(_v);
            TranceSeekCustomAPI.EnemyTranceBonusAttack(_v);
            TranceSeekCustomAPI.PenaltyShellAttack(_v);
            TranceSeekCustomAPI.PenaltyCommandDividedAttack(_v);
            TranceSeekCustomAPI.BonusElement(_v);
            if (TranceSeekCustomAPI.CanAttackMagic(_v))
            {
                _v.CalcHpDamage();
                TranceSeekCustomAPI.RaiseTrouble(_v);
            }
            _v.Command.AbilityStatus |= TranceSeekCustomAPI.CustomStatus.MagicBreak;
            _v.TryAlterMagicStatuses();
        }

        public Single RateTarget()
        {
            _v.NormalMagicParams();
            TranceSeekCustomAPI.CharacterBonusPassive(_v, "MagicAttack");
            TranceSeekCustomAPI.CasterPenaltyMini(_v);
            TranceSeekCustomAPI.PenaltyShellAttack(_v);
            TranceSeekCustomAPI.PenaltyCommandDividedAttack(_v);
            TranceSeekCustomAPI.BonusElement(_v);

            if (!TranceSeekCustomAPI.CanAttackMagic(_v))
                return 0;

            if (_v.Target.IsUnderAnyStatus(BattleStatus.Reflect) && !_v.Command.IsReflectNull)
                return 0;

            _v.CalcHpDamage();

            Single rate = Math.Min(_v.Target.HpDamage, _v.Target.CurrentHp);

            if ((_v.Target.Flags & CalcFlag.HpRecovery) == CalcFlag.HpRecovery)
                rate *= -1;
            if (_v.Target.IsPlayer)
                rate *= -1;

            return rate;
        }
    }
}