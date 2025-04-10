using Memoria.Data;
using System;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Special
    /// </summary>
    [BattleScript(Id)]
    public sealed class BloodyStrikeScript : IBattleScript
    {
        public const Int32 Id = 0124;

        private readonly BattleCalculator _v;

        public BloodyStrikeScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
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
                    TranceSeekCustomAPI.EnemyTranceBonusAttack(_v);
                    TranceSeekCustomAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                    TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                    TranceSeekCustomAPI.BonusBackstabAndPenaltyLongDistance(_v);
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
                        _v.Caster.HpDamage = _v.Target.HpDamage / _v.Command.Power;
                    }
                    else
                    {
                        TranceSeekCustomAPI.TryAlterMagicStatuses(_v);
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
