using System;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    [BattleScript(Id)]
    public sealed class MagicAttackScript : IBattleScript, IEstimateBattleScript
    {
        public const Int32 Id = 0009;

        private readonly BattleCalculator _v;

        public MagicAttackScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            if (_v.Caster.Data.dms_geo_id == 404 && _v.Command.Power == 57 && SFX.currentEffectID == SpecialEffect.Aerial_Slash_Garuda) // Kjata's Dance - Friendly Garuda
            {
                if (_v.Caster.SummonCount == 0)
                {
                    _v.Caster.SummonCount = 1;
                    _v.Command.Element = EffectElement.Thunder;
                }
                else if (_v.Caster.SummonCount == 1)
                {
                    _v.Caster.SummonCount = 2;
                    _v.Command.Element = EffectElement.Cold;
                }
                else if (_v.Caster.SummonCount == 2)
                {
                    _v.Caster.SummonCount = 0;
                    _v.Command.Element = EffectElement.Fire;
                }
            }
            if (_v.Target.MagicDefence == 255)
            {
                _v.Context.Flags |= BattleCalcFlags.Guard;
            }
            else
            {
                _v.NormalMagicParams();
                TranceSeekCustomAPI.CharacterBonusPassive(_v, "MagicAttack");
                _v.Caster.PenaltyMini();
                _v.Caster.EnemyTranceBonusAttack();
                TranceSeekCustomAPI.PenaltyShellAttack(_v);
                _v.PenaltyCommandDividedAttack();
                if (_v.Caster.Data.dms_geo_id == 5 || _v.Caster.Data.dms_geo_id == 267) 
                {
                    if (_v.Context.sfxThread.targetId != 1 && _v.Context.sfxThread.targetId != 2 && _v.Context.sfxThread.targetId != 4 && _v.Context.sfxThread.targetId != 8)
                    {
                        _v.Context.Attack /= 2;
                        _v.Context.HitRate /= 2;
                    }
                }
                _v.BonusElement();
                if (TranceSeekCustomAPI.CanAttackMagic(_v))
                {
                    if (_v.Target.HasCategory(EnemyCategory.Humanoid) && (_v.Command.AbilityId == BattleAbilityId.Poison || _v.Command.AbilityId == BattleAbilityId.Bio))
                    {
                        _v.Context.Attack = _v.Context.Attack * 2;
                    }
                    if (_v.Target.IsZombie && (_v.Command.AbilityId == BattleAbilityId.Poison || _v.Command.AbilityId == BattleAbilityId.Bio || _v.Command.AbilityId == (BattleAbilityId)1036 || _v.Command.AbilityId == (BattleAbilityId)1037))
                    {
                        _v.Target.Flags |= CalcFlag.HpRecovery;
                    }
                    if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)102))
                        TranceSeekCustomAPI.TryCriticalHit(_v);
                    _v.CalcHpDamage(); 
                }
                _v.TryAlterMagicStatuses();
            }
            TranceSeekCustomAPI.RaiseTrouble(_v);
            TranceSeekCustomAPI.SpecialSA(_v);
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
