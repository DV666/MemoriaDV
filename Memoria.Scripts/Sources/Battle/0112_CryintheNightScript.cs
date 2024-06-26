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
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
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
            _v.Caster.PenaltyMini();
            _v.Caster.EnemyTranceBonusAttack();
            TranceSeekCustomAPI.PenaltyShellAttack(_v);
            _v.PenaltyCommandDividedAttack();
            _v.BonusElement();
            if (TranceSeekCustomAPI.CanAttackMagic(_v))
            {
                _v.CalcHpDamage();
            }
            if (_v.Command.HitRate > 0)
                _v.TryAlterMagicStatuses();
            TranceSeekCustomAPI.SpecialSA(_v);
        }
    }
}