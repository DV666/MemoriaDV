using Memoria.Data;
using System;
using System.Collections.Generic;

namespace Memoria.Scripts.Battle
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
            TranceSeekAPI.CharacterBonusPassive(_v, "MagicAttack");
            TranceSeekAPI.CasterPenaltyMini(_v);
            TranceSeekAPI.EnemyTranceBonusAttack(_v);
            TranceSeekAPI.PenaltyCommandDividedAttack(_v);
            if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)102))
                TranceSeekAPI.TryCriticalHit(_v);
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
