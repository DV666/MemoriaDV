using Memoria.Data;
using System;
using System.Collections.Generic;

namespace Memoria.Scripts.Battle
{
    [BattleScript(Id)]
    public sealed class MagicRecoveryScript : IBattleScript
    {
        public const Int32 Id = 0010;

        private readonly BattleCalculator _v;

        public MagicRecoveryScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            if (_v.Command.Power == 1)
            {
                _v.Target.Flags |= CalcFlag.HpAlteration;
                if (!_v.Target.IsZombie)
                {
                    _v.Target.Flags |= CalcFlag.HpRecovery;
                }
                _v.Target.HpDamage = 1;
            }
            else
            {
                _v.NormalMagicParams();
                TranceSeekCustomAPI.CharacterBonusPassive(_v, "MagicAttack");
                _v.Caster.PenaltyMini();
                _v.Caster.EnemyTranceBonusAttack();
                _v.PenaltyCommandDividedAttack();
                if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)102))
                    TranceSeekCustomAPI.TryCriticalHit(_v);
                _v.CalcHpMagicRecovery();
                if (_v.Command.HitRate == 255)
                {
                    _v.Target.TryRemoveStatuses(_v.Command.AbilityStatus);
                }
                else
                {
                    _v.TryAlterMagicStatuses();
                }
                if (_v.Target.Data.dms_geo_id == 416)
                    TranceSeekCustomAPI.MonsterMechanic[_v.Target.Data][1] = _v.Target.HpDamage;
            }
            TranceSeekCustomAPI.SpecialSA(_v);
        }
    }
}
