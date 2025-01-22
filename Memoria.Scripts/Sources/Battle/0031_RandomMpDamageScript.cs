using FF9;
using System;
using Memoria.Data;

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
                if (!TranceSeekCustomAPI.TryPhysicalHit(_v))
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
                TranceSeekCustomAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                _v.BonusBackstabAndPenaltyLongDistance();
                TranceSeekCustomAPI.BonusWeaponElement(_v);
                if (TranceSeekCustomAPI.CanAttackWeaponElementalCommand(_v))
                {
                    if (_v.Context.IsAbsorb)
                    {
                        _v.Context.Flags |= BattleCalcFlags.Guard;
                    }
                    else
                    {
                        _v.TryCriticalHit();
                        _v.PenaltyReverseAttack();
                        _v.CalcMpDamage();
                        TranceSeekCustomAPI.RaiseTrouble(_v);
                    }
                }
                return;
            }
            if (_v.Command.AbilityId == BattleAbilityId.MagicHammer || _v.Command.AbilityId == (BattleAbilityId)1525 || _v.Command.HitRate == 20)
            {
                if (_v.Target.IsZombie)
                {
                    _v.Target.Flags |= (CalcFlag.MpAlteration | CalcFlag.MpRecovery);
                    _v.Caster.Flags |= CalcFlag.MpAlteration;
                }
                else
                {
                    _v.Target.Flags |= CalcFlag.MpAlteration;
                    _v.Caster.Flags |= (CalcFlag.MpAlteration | CalcFlag.MpRecovery);
                }
                if (_v.Target.CurrentMp > 0U)
                {
                    int num = (_v.Caster.Magic + Comn.random16() % (1 + (_v.Caster.Level + _v.Caster.Magic) / 3));
                    if (num > _v.Target.CurrentMp)
                    {
                        num = (int)_v.Target.CurrentMp;
                    }

                    if (_v.Target.IsUnderStatus(BattleStatus.Shell))
                        num /= 2;

                    if (_v.Command.IsManyTarget)
                    {
                        if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1126))
                            num = (num * 3) / 4;
                        else
                            num /= 2;
                    }

                    _v.Target.MpDamage = num;
                    _v.Caster.MpDamage = num;
                }
                TranceSeekCustomAPI.MagicAccuracy(_v);
                _v.TryAlterMagicStatuses();
            }
            else
            {
                _v.Target.Flags |= CalcFlag.MpAlteration;
                if (_v.Command.Power == 4) // Alchismiste Fou - Dark Ether
                {
                    if (_v.Target.CurrentMp > _v.Target.MaximumMp / 4U)
                    {
                        _v.Target.MpDamage = (int)(_v.Target.CurrentMp - _v.Target.MaximumMp / 4U);
                    }
                    else
                    {
                        _v.Context.Flags |= BattleCalcFlags.Miss;
                    }
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
                        {
                            _v.Target.MpDamage = (int)(Math.Min(9999, GameRandom.Next16() % (_v.Target.CurrentMp / 2U)));
                        }
                        else
                        {
                            _v.Target.MpDamage = (int)(Math.Min(9999, GameRandom.Next16() % _v.Target.CurrentMp));
                        }
                    }
                    if (_v.Command.Power == 10)
                    {
                        TranceSeekCustomAPI.MagicAccuracy(_v);
                        if (_v.Command.HitRate > Comn.random16() % 100)
                        {
                            _v.Target.AlterStatus(BattleStatus.Confuse, _v.Caster);
                        }
                        if (_v.Command.HitRate > Comn.random16() % 100)
                        {
                            _v.Target.AlterStatus(BattleStatus.Silence, _v.Caster);
                        }
                    }
                    _v.TryAlterMagicStatuses();
                }
            }
        }
    }
}
