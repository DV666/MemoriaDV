using FF9;
using Memoria.Data;
using System;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Weapon: Blood Sword
    /// </summary>
    [BattleScript(Id)]
    public sealed class EnemyPhysicalAttackScript : IBattleScript
    {
        public const Int32 Id = 0008;

        private readonly BattleCalculator _v;

    public EnemyPhysicalAttackScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Caster.Data.dms_geo_id == 573)
            {

                if (_v.Caster.Data.mot[0] == "ANH_MAIN_B0_012_401") // Red Scarlet Fight
                {
                    _v.Caster.PhysicalEvade = 10;
                    _v.Caster.Data.mot[0] = "ANH_MON_B3_182_000";
                    _v.Caster.Data.mot[2] = "ANH_MON_B3_182_003";
                    _v.Caster.RemoveStatus(BattleStatus.Defend);
                }
            }
            if (!_v.Target.TryKillFrozen())
            {
                _v.PhysicalAccuracy();
                if (TranceSeekAPI.TryPhysicalHit(_v))
                {
                    if (_v.Command.HitRate == 222)
                    {
                        _v.SetCommandAttack();
                        TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                        TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                        TranceSeekAPI.BonusElement(_v);
                        if (_v.CanAttackElementalCommand())
                        {
                            _v.CalcDamageCommon();
                            if (_v.Context.Attack > 100)
                            {
                                _v.Context.Attack = 100;
                            }
                            int hpDamage = (int)(_v.Target.MaximumHp * (uint)_v.Context.Attack / 100U);
                            _v.Target.HpDamage = hpDamage;
                            TranceSeekAPI.TryAlterMagicStatuses(_v);
                        }
                    }
                    else
                    {
                        if (_v.Command.HitRate == 223)
                        {
                            _v.SetCommandPower();
                            _v.Caster.SetPhysicalAttack();
                        }
                        else
                        {
                            if (_v.Command.HitRate == 221)
                            {
                                _v.SetCommandPower();
                                _v.Context.Attack = Comn.random16() % (int)_v.Caster.Strength + Comn.random16() % (1 + (_v.Caster.Level + _v.Caster.Strength >> 3));
                                _v.Target.SetPhysicalDefense();
                            }
                            else
                            {
                                _v.NormalPhysicalParams();
                                TranceSeekAPI.CharacterBonusPassive(_v, "PhysicalAttack");
                            }
                        }
                        TranceSeekAPI.EnemyTranceBonusAttack(_v);
                        TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                        TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                        if (_v.Command.HitRate != 101)
                        {
                            TranceSeekAPI.BonusBackstabAndPenaltyLongDistance(_v);
                        }
                        TranceSeekAPI.BonusElement(_v);
                        if (_v.CanAttackElementalCommand())
                        {
                            if (_v.Command.HitRate == 224) // Contre-attaque avec Critique
                            {
                                _v.Context.DamageModifierCount += 4;
                                _v.Target.Flags |= CalcFlag.Critical;
                            }
                            else
                            {
                                TranceSeekAPI.TryCriticalHit(_v);
                            }
                            _v.CalcPhysicalHpDamage();
                            TranceSeekAPI.InfusedWeaponStatus(_v);
                            TranceSeekAPI.RaiseTrouble(_v);
                            TranceSeekAPI.TryAlterMagicStatuses(_v);
                        }
                    }
                }
            }
        }
    }
}
