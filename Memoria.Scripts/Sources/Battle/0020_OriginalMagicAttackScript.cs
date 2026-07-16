using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using FF9;
using Memoria.Data;
using Memoria.Prime;

namespace Memoria.Scripts.TranceSeek
{
    /// <summary>
    /// Cherry Blossom, Climhazzard
    /// </summary>
    [BattleScript(Id)]
    public sealed class OriginalMagicAttackScript : IBattleScript
    {
        public const Int32 Id = 0020;

        private readonly BattleCalculator _v;

        public OriginalMagicAttackScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Command.AbilityId == TranceSeekBattleAbility.WindyBlade) // Lame effilée
                _v.Caster.RemoveStatus(BattleStatus.Slow);

            if (_v.Caster.Data.dms_geo_id == 569)
            {
                _v.SetCommandPower();
                _v.Caster.SetLowPhysicalAttack();
                _v.Target.SetMagicDefense();
                if (_v.Command.HitRate == 98)
                    _v.Context.DefensePower = _v.Context.DefensePower - (_v.Context.DefensePower / 4);

                TranceSeekAPI.CasterPenaltyMini(_v);
                TranceSeekAPI.PenaltyShellAttack(_v);
                TranceSeekAPI.PenaltyCommandDividedAttack(_v);
                if (_v.Command.HitRate == 99 && GameRandom.Next8() % 4 == 0)
                {
                    _v.Context.DamageModifierCount += 4;
                    _v.Target.Flags |= CalcFlag.Critical;
                }
            }
            else
            {

                if (_v.Caster.IsPlayer)
                {
                    _v.OriginalMagicParams();                     
                }
                else
                {
                    _v.SetCommandPower();
                    _v.Caster.SetLowPhysicalAttack();
                    _v.Target.SetMagicDefense();
                }
                TranceSeekAPI.CasterPenaltyMini(_v);
                if (_v.Target.IsUnderStatus(BattleStatus.Defend))
                    _v.Context.DamageModifierCount -= 2;
                TranceSeekAPI.PenaltyShellAttack(_v);
                if (!_v.Caster.IsPlayer)
                    TranceSeekAPI.BonusBackstabAndPenaltyLongDistance(_v);
            }
            TranceSeekAPI.EnemyTranceBonusAttack(_v);
            TranceSeekAPI.BonusElement(_v);
            if (TranceSeekAPI.CanAttackMagic(_v))
            {
                _v.CalcHpDamage();
                if (_v.Command.AbilityId == TranceSeekBattleAbility.Zantetsu)
                {
                    _v.Target.Flags |= (CalcFlag.MpAlteration | CalcFlag.MpRecovery);
                    _v.Target.MpDamage = (_v.Target.HpDamage >> 5);
                }
            }
            TranceSeekAPI.InfusedWeaponStatus(_v);
            TranceSeekAPI.TryAlterMagicStatuses(_v);
        }
    }
}

