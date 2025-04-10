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
            if (_v.Target.IsUnderAnyStatus(_v.Command.AbilityStatus) && _v.Target.CheckUnsafetyOrGuard())
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
            TranceSeekCustomAPI.CharacterBonusPassive(_v, "MagicAttack");
            TranceSeekCustomAPI.CasterPenaltyMini(_v);
            TranceSeekCustomAPI.EnemyTranceBonusAttack(_v);
            TranceSeekCustomAPI.PenaltyShellAttack(_v);
            TranceSeekCustomAPI.PenaltyCommandDividedAttack(_v);
            TranceSeekCustomAPI.BonusElement(_v);
            if (TranceSeekCustomAPI.CanAttackMagic(_v))
            {
                _v.CalcHpDamage();
            }
            if (_v.Command.HitRate > 0)
                TranceSeekCustomAPI.TryAlterMagicStatuses(_v);
        }
    }
}
