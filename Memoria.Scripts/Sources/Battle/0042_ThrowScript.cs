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
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            if (_v.Caster.IsPlayer)
            {
                if (_v.Command.ItemId == (RegularItem)1032) // Smoking Bomb
                {
                    btl_mot.HideMesh(_v.Target.Data, 65535, true);
                    BattleState.EnqueueCommand(BattleState.EscapeCommand, BattleCommandId.SysEscape, 0U, 15, true);
                }
                else if (_v.Command.ItemId == (RegularItem)1033) // Image
                {
                    _v.Target.AlterStatus(BattleStatus.Vanish);
                }
                else
                {
                    if (_v.Command.ItemId == (RegularItem)1034 || _v.Command.ItemId == (RegularItem)1035 || _v.Command.ItemId == (RegularItem)1036)
                    {
                        _v.Context.AttackPower = _v.Command.Weapon.Power << 1;
                        _v.Context.Attack = (Int16)(_v.Caster.Magic + Comn.random16() % (1 + (_v.Caster.Level + _v.Caster.Magic >> 3)));
                        _v.Target.SetMagicDefense();
                        _v.Caster.PenaltyMini();
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
                            _v.Caster.PhysicalPenaltyAndBonusAttack();
                        }
                    }
                    if ((_v.Command.Weapon.Element & _v.Caster.WeakElement) != 0)
                        _v.Context.Attack = (Int16)(_v.Context.Attack * 3 >> 1);
                    TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                    if (CanAttackMagic(_v))
                    {
                        _v.CalcPhysicalHpDamage();
                    }
                    if (_v.Command.Weapon.HitRate > Comn.random16() % 100)
                        _v.Target.TryAlterStatuses(_v.Command.Weapon.Status, false);
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
                        _v.Caster.PhysicalPenaltyAndBonusAttack();
                        _v.Caster.EnemyTranceBonusAttack();
                        _v.Target.GambleDefence();
                        TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                        TranceSeekCustomAPI.BonusBackstabAndPenaltyLongDistanceTranceSeek(_v);
                        _v.CalcHpDamage();
                        TranceSeekCustomAPI.RaiseTrouble(_v);
                        _v.TryAlterMagicStatuses();
                    }
                }
            }
            TranceSeekCustomAPI.SpecialSA(_v);
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
