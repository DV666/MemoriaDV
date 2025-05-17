using System;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    [BattleScript(Id)]
    public sealed class CryintheNightScript : IBattleScript
    {
        public const Int32 Id = 0112;

        private readonly BattleCalculator _v;

        public CryintheNightScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Target.IsUnderAnyStatus(_v.Command.AbilityStatus) && TranceSeekAPI.CheckUnsafetyOrGuard(_v))
            {
                _v.Target.TryAlterStatuses(BattleStatus.Death, false, _v.Target);
                return;
            }
            if (_v.Target.MagicDefence == 255)
            {
                _v.Context.Flags |= BattleCalcFlags.Guard;
                return;
            }
            _v.NormalMagicParams();
            TranceSeekAPI.CharacterBonusPassive(_v, "MagicAttack");
            TranceSeekAPI.CasterPenaltyMini(_v);
            TranceSeekAPI.EnemyTranceBonusAttack(_v);
            TranceSeekAPI.PenaltyShellAttack(_v);
            TranceSeekAPI.PenaltyCommandDividedAttack(_v);
            TranceSeekAPI.BonusElement(_v);
            if (TranceSeekAPI.CanAttackMagic(_v))
            {
                _v.CalcHpDamage();
            }
            if (_v.Command.HitRate > 0)
                TranceSeekAPI.TryAlterMagicStatuses(_v);
        }
    }
}
