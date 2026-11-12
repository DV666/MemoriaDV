using Memoria.Data;
using Memoria.Prime;
using System;
using System.Runtime.Remoting.Contexts;

namespace Memoria.Scripts.TranceSeek
{
    /// <summary>
    /// Magic Sword
    /// </summary>
    [BattleScript(Id)]
    public sealed class MagicSwordAttackScript : IBattleScript
    {
        public const Int32 Id = 0063;

        private readonly BattleCalculator _v;

        public MagicSwordAttackScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Command.AbilityId == TranceSeekBattleAbility.MagicSword_Quarter || _v.Command.AbilityId == TranceSeekBattleAbility.MagicSword_Demi || _v.Command.AbilityId == TranceSeekBattleAbility.MagicSword_Gravija) // Gravity Sword
            {
                _v.SetCommandAttack();
                TranceSeekAPI.PenaltyCommandDividedAttack(_v);
                if (_v.Target.IsUnderStatus(BattleStatus.Shell))
                    _v.Context.DamageModifierCount -= 2;
                if (_v.Target.HasCategory(EnemyCategory.Stone) && !_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill))
                    _v.Context.DamageModifierCount += 4;
                TranceSeekAPI.BonusElement(_v);
                if (TranceSeekAPI.CanAttackMagic(_v))
                {
                    if (_v.Command.HitRate == 255)
                    {
                        _v.CalcDamageCommon();
                        if (_v.Context.Attack > 100)
                        {
                            _v.Context.Attack = 100;
                        }
                        int num = (int)(_v.Target.MaximumHp * _v.Context.Attack / 100U);
                        if (_v.Command.IsShortSummon)
                        {
                            num = num * 2 / 3;
                        }
                        _v.Target.HpDamage = num;
                    }
                    else
                    {
                        _v.CalcCannonProportionDamage();
                    }
                }
                var Target_TSVar = _v.TargetState();
                if (_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill) || TranceSeekAPI.EliteMonster(_v.Target.Data))
                {
                    _v.Target.HpDamage = Math.Max(1, (_v.Target.HpDamage / Target_TSVar.Monster.NerfGravity));
                    Target_TSVar.Monster.NerfGravity = Target_TSVar.Monster.NerfGravity * 2;
                }
                TranceSeekAPI.TryAlterMagicStatuses(_v);
                if ((Target_TSVar.AbsorbElement & 256) != 0)
                    _v.Target.Flags |= CalcFlag.HpRecovery;
            }
            else
            {
                if (_v.Caster.PlayerIndex == CharacterId.Steiner && (_v.Command.AbilityId == BattleAbilityId.None7 || _v.Command.AbilityId == BattleAbilityId.DoomsdaySword || _v.Command.AbilityId == TranceSeekBattleAbility.RubyLight)) // Comet/Meteor/Doomsday Sword
                {
                    _v.Context.Attack = UnityEngine.Random.Range(((_v.Caster.Strength + _v.Caster.Level) / 3), (_v.Caster.Strength + _v.Caster.Level));
                }
                else
                {
                    _v.Caster.SetLowPhysicalAttack();
                }
                _v.Context.AttackPower = (Int16)(_v.Caster.GetWeaponPower(_v.Command) + _v.Command.Power);
                _v.Target.SetMagicDefense();
                TranceSeekAPI.EnemyTranceBonusAttack(_v);
                TranceSeekAPI.PenaltyShellAttack(_v);
                TranceSeekAPI.PenaltyCommandDividedAttack(_v);
                TranceSeekAPI.BonusElement(_v);
                if (TranceSeekAPI.CanAttackMagic(_v))
                {
                    if (_v.Target.HasCategory(EnemyCategory.Humanoid) && (_v.Command.AbilityId == BattleAbilityId.None5 || _v.Command.AbilityId == BattleAbilityId.BioSword || _v.Command.AbilityId == TranceSeekBattleAbility.MagicSword_Poison)) // Poison/Arsenic/Bio Sword
                        _v.Context.DamageModifierCount += 2;
                    if (_v.Command.AbilityId == TranceSeekBattleAbility.MagicSword_Drain) // Drain Sword
                        _v.PrepareHpDraining();
                    _v.CalcHpDamage();
                }
                TranceSeekAPI.TryAlterMagicStatuses(_v);
            }
        }
    }
}

