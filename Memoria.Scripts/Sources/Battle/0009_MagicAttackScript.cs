using System;
using System.Runtime.Remoting.Contexts;
using FF9;
using Memoria.Data;
using Memoria.Prime;

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
            if (_v.Command.AbilityId == BattleAbilityId.Fire)
            {
                _v.Target.AlterStatus(TranceSeekCustomAPI.CustomStatus.Vieillissement);
            }
            if (_v.Caster.Data.dms_geo_id == 404 && _v.Command.Power == 57 && SFX.currentEffectID == SpecialEffect.Aerial_Slash_Garuda) // [!!!TODO!!!] Kjata's Dance - Friendly Garuda
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
                if (_v.Command.AbilityId == BattleAbilityId.Attack) // Racket
                {
                    _v.PhysicalAccuracy();
                    if (TranceSeekCustomAPI.TryPhysicalHit(_v))
                    {
                        Int32 baseDamage = Comn.random16() % (1 + (_v.Caster.Level + _v.Caster.Magic >> 3));
                        _v.Context.AttackPower = _v.Caster.GetWeaponPower(_v.Command);
                        _v.Target.SetMagicDefense();
                        _v.Context.Attack = _v.Caster.Magic + baseDamage;
                        _v.Command.Element = _v.Caster.WeaponElement;
                        TranceSeekCustomAPI.BonusBackstabAndPenaltyLongDistance(_v);
                    }
                    else
                        return;
                }
                else
                    _v.NormalMagicParams();

                TranceSeekCustomAPI.CharacterBonusPassive(_v, "MagicAttack");
                TranceSeekCustomAPI.CasterPenaltyMini(_v);
                TranceSeekCustomAPI.EnemyTranceBonusAttack(_v);
                TranceSeekCustomAPI.PenaltyShellAttack(_v);
                TranceSeekCustomAPI.PenaltyCommandDividedAttack(_v);
                if (_v.Caster.Data.dms_geo_id == 5 || _v.Caster.Data.dms_geo_id == 267) // Kuja (multiple target malus)
                {
                    if (_v.Context.sfxThread.targetId != 1 && _v.Context.sfxThread.targetId != 2 && _v.Context.sfxThread.targetId != 4 && _v.Context.sfxThread.targetId != 8)
                    {
                        _v.Context.Attack /= 2;
                        _v.Context.HitRate /= 2;
                    }
                }
                TranceSeekCustomAPI.BonusElement(_v);
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
                    TranceSeekCustomAPI.RaiseTrouble(_v);
                }
                TranceSeekCustomAPI.TryAlterMagicStatuses(_v);
            }
            if (FF9StateSystem.Battle.battleMapIndex == 303)
            {
                SB2_PATTERN sb2Pattern = FF9StateSystem.Battle.FF9Battle.btl_scene.PatAddr[FF9StateSystem.Battle.FF9Battle.btl_scene.PatNum];
                if (sb2Pattern.Monster[_v.Caster.Data.bi.slot_no].TypeNo == 0 && (_v.Command.AbilityStatus & BattleStatus.Heat) != 0) // Buzz - Blambourine
                {
                    BattleStatusDataEntry statusData = FF9StateSystem.Battle.FF9Battle.status_data[BattleStatusId.Heat];
                    Int32 wait = (short)(((400 + (_v.Caster.Will * 2) - _v.Target.Will) * statusData.ContiCnt) / 4);
                    _v.Target.AddDelayedModifier(
                    target => (wait -= target.Data.cur.at_coef * BattleState.ATBTickCount) > 0,
                    target =>
                    {
                        target.RemoveStatus(BattleStatus.Heat);
                    }
                    );
                }
            }
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
