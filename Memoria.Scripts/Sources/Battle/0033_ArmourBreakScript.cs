using System;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Armour Break
    /// </summary>
    [BattleScript(Id)]
    public sealed class ArmourBreakScript : IBattleScript, IEstimateBattleScript
    {
        public const Int32 Id = 0033;

        private readonly BattleCalculator _v;

        public ArmourBreakScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            if (!_v.Target.TryKillFrozen())
            {
                if (_v.Target.PhysicalDefence == 255)
                {
                    _v.Context.Flags |= BattleCalcFlags.Guard;
                    return;
                }
                if (_v.Target.IsUnderAnyStatus(BattleStatus.Vanish) || _v.Target.PhysicalEvade == 255)
                {
                    _v.Context.Flags |= BattleCalcFlags.Miss;
                    return;
                }

                if (_v.Caster.IsPlayer)
                {
                    _v.WeaponPhysicalParams();
                    TranceSeekCustomAPI.CharacterBonusPassive(_v, "LowPhysicalAttack");
                }
                else
                {
                    _v.NormalPhysicalParams();
                }
                _v.MagicAccuracy();
                _v.Caster.EnemyTranceBonusAttack();
                _v.Caster.PhysicalPenaltyAndBonusAttack();
                TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                _v.BonusElement();
                _v.Target.AlterStatus(TranceSeekCustomAPI.CustomStatus.ArmorBreak, _v.Caster);
                _v.CalcHpDamage();
                _v.TryAlterMagicStatuses();
                TranceSeekCustomAPI.SpecialSA(_v);
            } 
        }

        public Single RateTarget()
        {
            _v.NormalMagicParams();
            TranceSeekCustomAPI.CharacterBonusPassive(_v, "MagicAttack");
            _v.Caster.PenaltyMini();
            TranceSeekCustomAPI.PenaltyShellAttack(_v);
            _v.PenaltyCommandDividedAttack();
            _v.BonusElement();

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
