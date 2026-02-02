using FF9;
using System;
using Memoria.Data;
using Memoria.Prime;
using System.Runtime.Remoting.Contexts;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Magic Hammer, Flare Star
    /// </summary>
    [BattleScript(Id)]
    public sealed class RandomMpDamageScript : IBattleScript
    {
        public const Int32 Id = 0031;

        private readonly BattleCalculator _v;

        public RandomMpDamageScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Command.Id == BattleCommandId.Attack && _v.Caster.PlayerIndex == CharacterId.Quina) // Magik Fork
            {
                _v.PhysicalAccuracy();
                if (!TranceSeekAPI.TryPhysicalHit(_v))
                    return;

                Int32 baseDamage = Comn.random16() % (1 + (_v.Caster.Level + _v.Caster.Magic >> 3));
                _v.Context.AttackPower = _v.Caster.GetWeaponPower(_v.Command);
                _v.Target.SetMagicDefense();
                if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1222)) // SA Sharpening +
                    _v.Context.Attack = _v.Caster.Magic + baseDamage;
                else if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)222)) // SA Sharpening +
                    _v.Context.Attack = UnityEngine.Random.Range(_v.Caster.Magic / 2, _v.Caster.Magic) + baseDamage;
                else
                    _v.Context.Attack = Comn.random16() % _v.Caster.Magic + baseDamage;
                TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                _v.BonusBackstabAndPenaltyLongDistance();
                TranceSeekAPI.BonusWeaponElement(_v);
                if (TranceSeekAPI.CanAttackWeaponElementalCommand(_v))
                {
                    _v.TryCriticalHit();
                    _v.PenaltyReverseAttack();
                    _v.CalcPhysicalHpDamage();
                    _v.Target.HpDamage /= 2;
                    if (!_v.Context.IsAbsorb)
                    {
                        _v.Target.Flags |= CalcFlag.MpAlteration;
                        foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(_v.Caster))
                            saFeature.TriggerOnAbility(_v, "CalcDamage", false);
                        foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(_v.Target))
                            saFeature.TriggerOnAbility(_v, "CalcDamage", true);


                        _v.Target.MpDamage = Math.Max(0, _v.Context.PowerDifference) * _v.Context.EnsureAttack >> 3;
                    }
                    TranceSeekAPI.RaiseTrouble(_v);
                }
            }
            else
            {
                _v.Target.Flags |= CalcFlag.MpAlteration;
                if (_v.Command.Power == 4) // Alchismiste Fou - Dark Ether
                {
                    if (_v.Target.CurrentMp > _v.Target.MaximumMp / 4U)
                        _v.Target.MpDamage = (int)(_v.Target.CurrentMp - _v.Target.MaximumMp / 4U);
                    else
                        _v.Context.Flags |= BattleCalcFlags.Miss;
                }
                else
                {
 
                    if (_v.Target.CurrentMp > 0U)
                    {
                        _v.Target.Flags |= (CalcFlag.MpAlteration);
                        if (_v.Command.Power == 1)
                        {
                            _v.Target.MpDamage = (int)(_v.Target.CurrentMp / 2U);
                            return;
                        }

                        if (_v.Target.IsUnderStatus(BattleStatus.Shell))
                            _v.Target.MpDamage = (int)(Math.Min(9999, GameRandom.Next16() % (_v.Target.CurrentMp / 2U)));
                        else
                            _v.Target.MpDamage = (int)(Math.Min(9999, GameRandom.Next16() % _v.Target.CurrentMp));
                    }
                    if (_v.Command.Power == 10)
                    {
                        TranceSeekAPI.MagicAccuracy(_v);
                        _v.Command.AbilityStatus |= (BattleStatus.Confuse | BattleStatus.Silence);
                    }
                    TranceSeekAPI.TryAlterMagicStatuses(_v);
                }
            }
        }
    }
}
