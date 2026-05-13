using Memoria.Data;
using System;
using System.Collections.Generic;

namespace Memoria.Scripts.TranceSeek
{
    [BattleScript(Id)]
    public sealed class MagicRecoveryHPMPScript : IBattleScript
    {
        public const Int32 Id = 0143;

        private readonly BattleCalculator _v;

        public MagicRecoveryHPMPScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            _v.NormalMagicParams();
            var Caster_TSVar = _v.CasterState();

            TranceSeekAPI.CasterPenaltyMini(_v);
            TranceSeekAPI.EnemyTranceBonusAttack(_v);
            TranceSeekAPI.PenaltyCommandDividedAttack(_v);
            if (_v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Archmage))
                TranceSeekAPI.TryCriticalHit(_v);

            if (Caster_TSVar.StackStatus.Magic != 0)
                _v.Context.Attack += ((Caster_TSVar.StackStatus.Magic * _v.Context.Attack) / 100);

            _v.CalcHpMagicRecovery();
            _v.Target.Flags |= (CalcFlag.MpDamageOrHeal);
            int HpHealing = _v.Target.HpDamage;
            _v.Target.MpDamage = HpHealing >> 4;
            if (!_v.Target.IsZombie && !_v.Context.IsAbsorb)
                _v.Target.MpDamage = HpHealing >> 4;
            TranceSeekAPI.TryAlterCommandStatuses(_v);
        }
    }
}

