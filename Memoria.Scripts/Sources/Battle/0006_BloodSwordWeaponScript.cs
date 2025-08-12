using Memoria.Data;
using System;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Weapon: Blood Sword
    /// </summary>
    [BattleScript(Id)]
    public sealed class BloodSwordWeaponScript : IBattleScript
    {
        public const Int32 Id = 0006;

        private readonly BattleCalculator _v;

        public BloodSwordWeaponScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Target.CanBeAttacked() && !_v.Target.TryKillFrozen())
            {
                _v.PhysicalAccuracy();
                if (TranceSeekAPI.TryPhysicalHit(_v))
                {
                    if (_v.Caster.IsPlayer)
                    {
                        TranceSeekAPI.WeaponPhysicalParams(CalcAttackBonus.Simple, _v);
                    }
                    else
                    {
                        _v.NormalPhysicalParams();
                    }
                    TranceSeekAPI.EnemyTranceBonusAttack(_v);
                    TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                    TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                    if (_v.Caster.IsUnderStatus(BattleStatus.Trance) && _v.Caster.PlayerIndex == CharacterId.Steiner)
                        _v.Context.DamageModifierCount++;
                    TranceSeekAPI.BonusBackstabAndPenaltyLongDistance(_v);
                    TranceSeekAPI.TryCriticalHit(_v);
                    TranceSeekAPI.IpsenCastleMalus(_v);
                    _v.Target.Flags |= CalcFlag.HpAlteration;
                    _v.Caster.Flags |= CalcFlag.HpAlteration;
                    if (_v.Target.IsZombie)
                    {
                        _v.Target.Flags |= CalcFlag.HpRecovery;
                    }
                    else
                    {
                        _v.Caster.Flags |= CalcFlag.HpRecovery;
                    }
                    uint currentHp = _v.Target.CurrentHp;
                    _v.CalcPhysicalHpDamage();
                    if (_v.Caster.IsPlayer)
                    {
                        _v.Caster.HpDamage = _v.Target.HpDamage / 4;
                    }
                    else
                    {
                        TranceSeekAPI.TryAlterMagicStatuses(_v);
                        if (_v.Target.HpDamage < currentHp)
                        {
                            _v.Caster.HpDamage = _v.Target.HpDamage;
                        }
                        else
                        {
                            _v.Caster.HpDamage = (int)currentHp;
                        }
                    }
                }
            }
        }
    }
}
