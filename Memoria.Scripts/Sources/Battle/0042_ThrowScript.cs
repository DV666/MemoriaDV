using System;
using FF9;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// White Draw
    /// </summary>
    [BattleScript(Id)]
    public sealed class ThrowScript : IBattleScript
    {
        public const Int32 Id = 0042;

        private readonly BattleCalculator _v;

        public ThrowScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Caster.IsPlayer)
            {
                if (_v.Command.AbilityId == (BattleAbilityId)1136 || _v.Command.AbilityId == (BattleAbilityId)1138) // Accelerator hammer
                {
                    if (_v.Target.IsUnderAnyStatus(BattleStatus.Vanish))
                    {
                        _v.Context.Flags |= BattleCalcFlags.Miss;
                    }
                    TranceSeekCustomAPI.WeaponPhysicalParams(CalcAttackBonus.Simple, _v);
                    TranceSeekCustomAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                    TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                    TranceSeekCustomAPI.BonusWeaponElement(_v);
                    if (TranceSeekCustomAPI.CanAttackWeaponElementalCommand(_v))
                    {
                        TranceSeekCustomAPI.TryCriticalHit(_v);
                        TranceSeekCustomAPI.IpsenCastleMalus(_v);
                        _v.CalcPhysicalHpDamage();
                        TranceSeekCustomAPI.InfusedWeaponStatus(_v);
                        TranceSeekCustomAPI.TryAlterCommandStatuses(_v, false);
                        TranceSeekCustomAPI.RaiseTrouble(_v);
                    }
                    return;
                }
                else if (_v.Command.ItemId == (RegularItem)1032) // Smoking Bomb
                {
                    btl_sys.CheckEscape(false);
                    if (_v.CanEscape())
                    {
                        btl_mot.HideMesh(_v.Target.Data, 65535, true);
                        BattleState.EnqueueCommand(BattleState.EscapeCommand, BattleCommandId.SysEscape, 0U, 15, true);
                    }
                }
                else if (_v.Command.ItemId == (RegularItem)1033) // Image
                {
                    _v.Command.AbilityStatus |= _v.Command.Weapon.Status;
                    TranceSeekCustomAPI.TryAlterCommandStatuses(_v);
                }
                else
                {
                    if (_v.Command.ItemId == (RegularItem)1034 || _v.Command.ItemId == (RegularItem)1035 || _v.Command.ItemId == (RegularItem)1036)
                    {
                        _v.Context.AttackPower = _v.Command.Weapon.Power << 1;
                        _v.Context.Attack = (Int16)(_v.Caster.Magic + Comn.random16() % (1 + (_v.Caster.Level + _v.Caster.Magic >> 3)));
                        _v.Target.SetMagicDefense();
                        TranceSeekCustomAPI.CasterPenaltyMini(_v);
                        TranceSeekCustomAPI.PenaltyShellAttack(_v);
                        if (_v.Target.IsUnderAnyStatus(BattleStatus.Vanish))
                            _v.Target.RemoveStatus(BattleStatus.Vanish);
                    }
                    else
                    {
                        if (!_v.Target.TryKillFrozen())
                        {
                            if (_v.Target.IsUnderAnyStatus(BattleStatus.Vanish))
                            {
                                _v.Context.Flags |= BattleCalcFlags.Miss;
                            }
                            _v.Caster.SetLowPhysicalAttack();
                            TranceSeekCustomAPI.CharacterBonusPassive(_v, "LowPhysicalAttack");
                            _v.Target.SetPhysicalDefense();
                            _v.Context.AttackPower = _v.Command.Weapon.Power << 1;
                            TranceSeekCustomAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                        }
                    }
                    _v.Command.Element = _v.Command.Weapon.Element;
                    if (_v.Command.Weapon.HitRate > Comn.random16() % 100)
                        _v.Command.AbilityStatus |= _v.Command.Weapon.Status;

                    TranceSeekCustomAPI.BonusWeaponElement(_v);
                    TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                    TranceSeekCustomAPI.TryAlterCommandStatuses(_v);
                    if (TranceSeekCustomAPI.CanAttackMagic(_v))
                        _v.CalcPhysicalHpDamage();
                }
            }
            else
            {
                if (!_v.Target.TryKillFrozen())
                {
                    if (_v.Target.IsUnderAnyStatus(BattleStatus.Vanish))
                    {
                        _v.Context.Flags |= BattleCalcFlags.Miss;
                    }
                    else
                    {
                        _v.NormalPhysicalParams();
                        TranceSeekCustomAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                        TranceSeekCustomAPI.EnemyTranceBonusAttack(_v);
                        TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                        TranceSeekCustomAPI.BonusBackstabAndPenaltyLongDistance(_v);
                        _v.CalcHpDamage();
                        TranceSeekCustomAPI.RaiseTrouble(_v);
                        TranceSeekCustomAPI.TryAlterMagicStatuses(_v);
                    }
                }
            }
        }

        public static Boolean CanAttackMagic(BattleCalculator _v)
        {
            if (_v.Target.CanGuardElement(_v.Command.Weapon.Element))
                return false;

            _v.Target.PenaltyHalfElement(_v.Command.Weapon.Element);
            _v.Target.BonusWeakElement(_v.Command.Weapon.Element);
            if (_v.Target.CanAbsorbElement(_v.Command.Weapon.Element))
            {
                // v.Context.DefensePower = 0;
            }
            _v.Target.AlterStatuses(_v.Command.Weapon.Element);
            return true;
        }
    }
}
