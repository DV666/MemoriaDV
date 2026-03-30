using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using FF9;
using Memoria.Data;
using Memoria.Prime;

namespace Memoria.Scripts.Battle
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
            BTL_DATA data = _v.Caster.Data;
            Boolean reducemagique = _v.Command.AbilityId == BattleAbilityId.FreeEnergy || _v.Command.AbilityId == BattleAbilityId.Solution9 && _v.Caster.CurrentHp % 10 == 9;

            if (_v.Command.AbilityId == (BattleAbilityId)1002) // Lame effilée
                _v.Caster.RemoveStatus(BattleStatus.Slow);

            if (_v.Caster.IsPlayer)
            {
                _v.SetWeaponPower();
                _v.Caster.SetMagicAttack();
                _v.Target.SetMagicDefense();
                if (reducemagique)
                {
                    if (_v.Command.AbilityId == BattleAbilityId.FreeEnergy)
                        _v.Context.DefensePower = _v.Context.DefensePower - (_v.Context.DefensePower / 4);
                    else if (_v.Command.AbilityId == BattleAbilityId.Solution9)
                        _v.Context.DefensePower /= 2;
                }
                if (_v.Command.AbilityId == BattleAbilityId.MeoTwister && (_v.Target.CurrentStatus & BattleStatusConst.AnyNegative) == 0)
                {
                    BattleStatusId[] statuslist = { BattleStatusId.Poison, BattleStatusId.Venom, BattleStatusId.Blind, BattleStatusId.Silence, BattleStatusId.Trouble,
                    BattleStatusId.Sleep, BattleStatusId.Freeze, BattleStatusId.Heat, BattleStatusId.Doom, BattleStatusId.Mini, BattleStatusId.Petrify, BattleStatusId.GradualPetrify,
                    BattleStatusId.Berserk, BattleStatusId.Confuse, BattleStatusId.Stop, BattleStatusId.Zombie, BattleStatusId.Slow, TranceSeekStatusId.Vieillissement,
                    TranceSeekStatusId.ArmorBreak, TranceSeekStatusId.MagicBreak, TranceSeekStatusId.MentalBreak, TranceSeekStatusId.PowerBreak};

                    List<BattleStatusId> statuschoosen = new List<BattleStatusId>();

                    for (Int32 i = 0; i < statuslist.Length; i++)
                    {
                        if ((statuslist[i].ToBattleStatus() & _v.Target.ResistStatus) == 0)
                        {
                            if (statuslist[i] == TranceSeekStatusId.Vieillissement && _v.Target.IsUnderAnyStatus(BattleStatus.EasyKill))
                                continue;

                            statuschoosen.Add(statuslist[i]);
                        }
                    }
                    BattleStatusId statusselected = statuschoosen[GameRandom.Next16() % statuschoosen.Count];
                    btl_stat.AlterStatus(_v.Target, statusselected, _v.Caster);
                }
                TranceSeekAPI.CasterPenaltyMini(_v);
                TranceSeekAPI.PenaltyShellAttack(_v);
                TranceSeekAPI.PenaltyCommandDividedAttack(_v);
            }
            else
            {
                if (data.dms_geo_id == 569)
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
            }
            TranceSeekAPI.EnemyTranceBonusAttack(_v);
            TranceSeekAPI.BonusElement(_v);
            if (TranceSeekAPI.CanAttackMagic(_v))
            {
                if (_v.Caster.PlayerIndex == CharacterId.Beatrix && _v.Command.AbilityId == (BattleAbilityId)1043)
                {
                    _v.Target.Flags |= (CalcFlag.MpAlteration | CalcFlag.MpRecovery);
                    _v.Target.MpDamage = (_v.Target.HpDamage >> 5);
                }
                _v.CalcHpDamage();
                if (_v.Command.AbilityId == BattleAbilityId.ScoopArt)
                {
                    _v.Target.HpDamage /= 3;
                    TranceSeekAPI.TryCriticalHit(_v, 25);
                }
            }
            TranceSeekAPI.InfusedWeaponStatus(_v);
            TranceSeekAPI.TryAlterMagicStatuses(_v);
        }
    }
}
