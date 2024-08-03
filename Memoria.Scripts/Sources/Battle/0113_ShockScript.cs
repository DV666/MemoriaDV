using System;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Dark Matter
    /// </summary>
    [BattleScript(Id)]
    public sealed class ShockScript : IBattleScript
    {
        public const Int32 Id = 00113;

        private readonly BattleCalculator _v;

        public ShockScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Caster.IsPlayer)
            {
                _v.Target.RemoveStatus(BattleStatus.Protect);
                _v.Target.RemoveStatus(BattleStatus.Shell);
                _v.Target.RemoveStatus(BattleStatus.Vanish);
                _v.Target.RemoveStatus(BattleStatus.Reflect);
                _v.WeaponPhysicalParams();
                TranceSeekCustomAPI.CharacterBonusPassive(_v, "PhysicalAttack");
                TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                _v.Caster.PhysicalPenaltyAndBonusAttack();
                _v.BonusElement();
                if (_v.CanAttackWeaponElementalCommand())
                {
                    _v.CalcHpDamage();
                    _v.TryAlterMagicStatuses();
                }
                if (_v.Command.HitRate == 214 && _v.Caster.PlayerIndex == CharacterId.Beatrix)
                {
                    _v.Target.Flags |= CalcFlag.MpAlteration;
                    int num = Math.Min(9999, _v.Context.PowerDifference * _v.Context.EnsureAttack);
                    _v.Target.MpDamage = (int)((short)(num >> 4));
                }
                return;
            }
            _v.PhysicalAccuracy();
            if (_v.Command.AbilityStatus == BattleStatus.Protect)
            {
                if (!TranceSeekCustomAPI.TryPhysicalHit(_v))
                {
                    return;
                }
                _v.NormalPhysicalParams();
                TranceSeekCustomAPI.CharacterBonusPassive(_v, "PhysicalAttack");
                _v.Caster.PhysicalPenaltyAndBonusAttack();
                _v.Caster.EnemyTranceBonusAttack();
                TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                _v.BonusElement();
                if (_v.CanAttackElementalCommand())
                {
                    _v.CalcPhysicalHpDamage();
                    TranceSeekCustomAPI.RaiseTrouble(_v);
                    _v.Target.RemoveStatus(BattleStatus.Protect);
                    return;
                }
            }
            if (!_v.Target.TryKillFrozen())
            {
                _v.Target.RemoveStatus(BattleStatus.Protect);
                _v.Target.RemoveStatus(BattleStatus.Shell);
                _v.Target.RemoveStatus(BattleStatus.Vanish);
                _v.Target.RemoveStatus(BattleStatus.Reflect);
                _v.NormalPhysicalParams();
                TranceSeekCustomAPI.CharacterBonusPassive(_v, "PhysicalAttack");
                _v.Caster.PhysicalPenaltyAndBonusAttack();
                TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                _v.BonusElement();
                if (_v.CanAttackElementalCommand())
                {
                    _v.CalcPhysicalHpDamage();
                    if (_v.Command.HitRate == 255)
                    {
                        _v.Target.Flags |= CalcFlag.MpAlteration;
                        int num = Math.Min(9999, _v.Context.PowerDifference * _v.Context.EnsureAttack);
                        _v.Target.MpDamage = (int)((short)(num >> 4));
                    }
                    _v.TryAlterMagicStatuses();
                }
            }
        }
    }
}
