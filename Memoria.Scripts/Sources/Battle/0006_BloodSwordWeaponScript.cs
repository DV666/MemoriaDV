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
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            if (_v.Target.CanBeAttacked() && !_v.Target.TryKillFrozen())
            {
                _v.PhysicalAccuracy();
                if (TranceSeekCustomAPI.TryPhysicalHit(_v))
                {
                    if (_v.Caster.IsPlayer)
                    {
                        TranceSeekCustomAPI.WeaponPhysicalParams(CalcAttackBonus.Simple, _v);
                    }
                    else
                    {
                        _v.NormalPhysicalParams();
                    }
                    _v.Caster.EnemyTranceBonusAttack();
                    _v.Caster.PhysicalPenaltyAndBonusAttack();
                    TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                    if (_v.Caster.IsUnderStatus(BattleStatus.Trance) && _v.Caster.PlayerIndex == CharacterId.Steiner)
                    {
                        _v.Context.Attack += _v.Context.Attack / 4;
                    }
                    TranceSeekCustomAPI.BonusBackstabAndPenaltyLongDistanceTranceSeek(_v);
                    TranceSeekCustomAPI.TryCriticalHit(_v);
                    TranceSeekCustomAPI.IpsenCastleMalus(_v);
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
                        _v.TryAlterMagicStatuses();
                        if (_v.Target.HpDamage < currentHp)
                        {
                            _v.Caster.HpDamage = _v.Target.HpDamage;
                        }
                        else
                        {
                            _v.Caster.HpDamage = (int)currentHp;
                        }
                    }
                    TranceSeekCustomAPI.SpecialSA(_v);
                }
            }
        }
    }
}
