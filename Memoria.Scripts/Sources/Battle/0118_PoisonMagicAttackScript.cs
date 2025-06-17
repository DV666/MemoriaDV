using System;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    [BattleScript(Id)]
    public sealed class PoisonMagicAttackScript : IBattleScript, IEstimateBattleScript
    {
        public const Int32 Id = 0118;

        private readonly BattleCalculator _v;

        public PoisonMagicAttackScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            _v.NormalMagicParams();
            TranceSeekAPI.CharacterBonusPassive(_v, "MagicAttack");
            TranceSeekAPI.CasterPenaltyMini(_v);
            TranceSeekAPI.EnemyTranceBonusAttack(_v);
            TranceSeekAPI.PenaltyShellAttack(_v);
            TranceSeekAPI.PenaltyCommandDividedAttack(_v);
            TranceSeekAPI.BonusElement(_v);
            if (TranceSeekAPI.CanAttackMagic(_v))
            {
                if (_v.Target.HasCategory(EnemyCategory.Humanoid))
                    _v.Context.DamageModifierCount += 4;
                if (_v.Target.IsZombie)
                    _v.Target.Flags |= CalcFlag.HpRecovery;
                if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)102))
                    TranceSeekAPI.TryCriticalHit(_v);
                _v.CalcHpDamage();
            }
            TranceSeekAPI.TryAlterMagicStatuses(_v);          
        }

        public Single RateTarget()
        {
            _v.NormalMagicParams();
            TranceSeekAPI.CharacterBonusPassive(_v, "MagicAttack");
            TranceSeekAPI.CasterPenaltyMini(_v);
            TranceSeekAPI.PenaltyShellAttack(_v);
            TranceSeekAPI.PenaltyCommandDividedAttack(_v);
            TranceSeekAPI.BonusElement(_v);

            if (!TranceSeekAPI.CanAttackMagic(_v))
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
