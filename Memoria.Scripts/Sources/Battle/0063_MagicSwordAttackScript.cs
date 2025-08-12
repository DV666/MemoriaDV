using System;
using Memoria.Data;

namespace Memoria.Scripts.Battle
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
            if (_v.Command.AbilityId == (BattleAbilityId)1564 || _v.Command.AbilityId == (BattleAbilityId)1565 || _v.Command.AbilityId == (BattleAbilityId)1566) // Gravity Sword
            {
                SB2_PATTERN sb2Pattern = FF9StateSystem.Battle.FF9Battle.btl_scene.PatAddr[FF9StateSystem.Battle.FF9Battle.btl_scene.PatNum];
                for (Int32 i = 0; i < MagicGravityDamageScript.ImmuneGravity.GetLength(0); i++)
                {
                    if (FF9StateSystem.Battle.battleMapIndex == MagicGravityDamageScript.ImmuneGravity[i, 0] && sb2Pattern.Monster[_v.Target.Data.bi.slot_no].TypeNo == MagicGravityDamageScript.ImmuneGravity[i, 1])
                    {
                        _v.Context.Flags = BattleCalcFlags.Guard;
                        return;
                    }
                }

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
                if (_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill) || TranceSeekAPI.EliteMonster(_v.Target.Data))
                {
                    _v.Target.HpDamage = Math.Max(1, (_v.Target.HpDamage / TranceSeekAPI.MonsterMechanic[_v.Target.Data][5]));
                    TranceSeekAPI.MonsterMechanic[_v.Target.Data][5] = TranceSeekAPI.MonsterMechanic[_v.Target.Data][5] * 2;
                }
                TranceSeekAPI.TryAlterMagicStatuses(_v);
                if (TranceSeekAPI.AbsorbElement.TryGetValue(_v.Target.Data, out Int32 elementprotect))
                    if (elementprotect == 256)
                        _v.Target.Flags |= CalcFlag.HpRecovery;
            }
            else
            {
                if (_v.Caster.PlayerIndex == CharacterId.Steiner && (_v.Command.AbilityId == BattleAbilityId.None7 || _v.Command.AbilityId == BattleAbilityId.DoomsdaySword || _v.Command.AbilityId == (BattleAbilityId)1578)) // Comet/Meteor/Doomsday Sword
                {
                    _v.Context.Attack = UnityEngine.Random.Range(((_v.Caster.Strength + _v.Caster.Level) / 3), (_v.Caster.Strength + _v.Caster.Level));
                }
                else
                {
                    _v.Caster.SetLowPhysicalAttack();
                }
                _v.SetWeaponPowerSum();
                _v.Target.SetMagicDefense();
                TranceSeekAPI.EnemyTranceBonusAttack(_v);
                TranceSeekAPI.PenaltyShellAttack(_v);
                TranceSeekAPI.PenaltyCommandDividedAttack(_v);
                TranceSeekAPI.BonusElement(_v);
                if (TranceSeekAPI.CanAttackMagic(_v))
                {
                    if (_v.Target.HasCategory(EnemyCategory.Humanoid) && (_v.Command.AbilityId == BattleAbilityId.None5 || _v.Command.AbilityId == BattleAbilityId.BioSword || _v.Command.AbilityId == (BattleAbilityId)1563)) // Poison/Arsenic/Bio Sword
                        _v.Context.DamageModifierCount += 4;
                    _v.CalcHpDamage();
                }
                TranceSeekAPI.TryAlterMagicStatuses(_v);
            }
        }
    }
}
