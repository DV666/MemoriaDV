using System;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Dark Matter
    /// </summary>
    [BattleScript(Id)]
    public sealed class ShockMagicalScript : IBattleScript
    {
        public const Int32 Id = 00113;

        private readonly BattleCalculator _v;

        public ShockMagicalScript(BattleCalculator v)
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
                TranceSeekAPI.CharacterBonusPassive(_v, "MagicAttack");
                TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                TranceSeekAPI.BonusElement(_v);
                if (TranceSeekAPI.CanAttackWeaponElementalCommand(_v))
                {
                    _v.CalcHpDamage();
                    TranceSeekAPI.TryAlterMagicStatuses(_v);
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
            if (!_v.Target.TryKillFrozen())
            {
                _v.Target.RemoveStatus(BattleStatus.Protect);
                _v.Target.RemoveStatus(BattleStatus.Shell);
                _v.Target.RemoveStatus(BattleStatus.Vanish);
                _v.Target.RemoveStatus(BattleStatus.Reflect);
                _v.NormalPhysicalParams();
                TranceSeekAPI.CharacterBonusPassive(_v, "PhysicalAttack");
                TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                TranceSeekAPI.BonusElement(_v);
                if (_v.CanAttackElementalCommand())
                {
                    _v.CalcPhysicalHpDamage();
                    if (_v.Command.HitRate == 255)
                    {
                        _v.Target.Flags |= CalcFlag.MpAlteration;
                        int num = Math.Min(9999, _v.Context.PowerDifference * _v.Context.EnsureAttack);
                        _v.Target.MpDamage = (int)((short)(num >> 4));
                    }
                    TranceSeekAPI.TryAlterMagicStatuses(_v);
                }
            }
        }
    }
}
