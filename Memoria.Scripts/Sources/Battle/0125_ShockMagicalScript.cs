using System;
using System.Collections.Generic;
using FF9;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Dark Matter
    /// </summary>
    [BattleScript(Id)]
    public sealed class ShockScript : IBattleScript
    {
        public const Int32 Id = 00125;

        private readonly BattleCalculator _v;

        public ShockScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            _v.Target.RemoveStatus(BattleStatus.Protect);
            _v.Target.RemoveStatus(BattleStatus.Shell);
            _v.Target.RemoveStatus(BattleStatus.Vanish);
            _v.Target.RemoveStatus(BattleStatus.Reflect);
            if (_v.Caster.IsPlayer)
            {
                _v.SetWeaponPower();
            }
            else
            {
                _v.SetCommandPower();

            }
            _v.Caster.SetMagicAttack();
            _v.Target.SetMagicDefense();
            TranceSeekAPI.CharacterBonusPassive(_v, "MagicAttack");
            TranceSeekAPI.CasterPenaltyMini(_v);
            TranceSeekAPI.PenaltyShellAttack(_v);
            TranceSeekAPI.PenaltyCommandDividedAttack(_v);
            TranceSeekAPI.EnemyTranceBonusAttack(_v);
            TranceSeekAPI.BonusElement(_v);
            if (TranceSeekAPI.CanAttackMagic(_v))
            {
                _v.CalcHpDamage();
            }
            TranceSeekAPI.TryAlterMagicStatuses(_v);
            if (_v.Command.AbilityId == (BattleAbilityId)1044 || _v.Command.AbilityId == (BattleAbilityId)1056 || _v.Command.HitRate == 255)
            {
                _v.Target.Flags |= CalcFlag.MpAlteration;
                int num = Math.Min(9999, _v.Context.PowerDifference * _v.Context.EnsureAttack);
                _v.Target.MpDamage = num >> 4;
            }
        }
    }
}
