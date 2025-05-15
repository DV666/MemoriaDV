using System;
using System.Collections.Generic;
using FF9;
using Memoria.Data;

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
            Boolean reducemagique = _v.Command.AbilityId == BattleAbilityId.FreeEnergy || _v.Command.AbilityId == BattleAbilityId.Solution9 && _v.Caster.CurrentHp % 10 != 9;
            if (_v.Command.AbilityId == (BattleAbilityId)1002) // Lame effil�e
            {
                _v.Caster.RemoveStatus(BattleStatus.Slow);
            }
            if (_v.Caster.IsPlayer)
            {
                _v.SetWeaponPower();
                _v.Caster.SetMagicAttack();
                _v.Target.SetMagicDefense();
                if (reducemagique)
                {
                    if (_v.Command.AbilityId == BattleAbilityId.FreeEnergy)
                    {
                        _v.Context.DefensePower = _v.Context.DefensePower - (_v.Context.DefensePower / 4);
                    }
                    else if (_v.Command.AbilityId == BattleAbilityId.Solution9)
                    {
                        _v.Context.DefensePower /= 2;
                    }
                }
                if (_v.Command.AbilityId == BattleAbilityId.MeoTwister && (_v.Target.CurrentStatus & BattleStatusConst.AnyNegative) == 0)
                {
                    BattleStatusId[] statuslist = { BattleStatusId.Poison, BattleStatusId.Venom, BattleStatusId.Blind, BattleStatusId.Silence, BattleStatusId.Trouble,
                    BattleStatusId.Sleep, BattleStatusId.Freeze, BattleStatusId.Heat, BattleStatusId.Doom, BattleStatusId.Mini, BattleStatusId.Petrify, BattleStatusId.GradualPetrify,
                    BattleStatusId.Berserk, BattleStatusId.Confuse, BattleStatusId.Stop, BattleStatusId.Zombie, BattleStatusId.Slow, TranceSeekCustomStatusId.Vieillissement,
                    TranceSeekCustomStatusId.ArmorBreak, TranceSeekCustomStatusId.MagicBreak, TranceSeekCustomStatusId.MentalBreak, TranceSeekCustomStatusId.PowerBreak};

                    List<BattleStatusId> statuschoosen = new List<BattleStatusId>();

                    for (Int32 i = 0; i < statuslist.Length; i++)
                    {
                        if ((statuslist[i].ToBattleStatus() & _v.Target.ResistStatus) == 0)
                        {
                            if (statuslist[i] == TranceSeekCustomStatusId.Vieillissement && _v.Target.IsUnderAnyStatus(BattleStatus.EasyKill))
                                continue;

                            statuschoosen.Add(statuslist[i]);
                        }
                    }
                    BattleStatusId statusselected = statuschoosen[GameRandom.Next16() % statuschoosen.Count];
                    btl_stat.AlterStatus(_v.Target, statusselected, _v.Caster);
                }
                TranceSeekCustomAPI.CasterPenaltyMini(_v);
                TranceSeekCustomAPI.PenaltyShellAttack(_v);
                TranceSeekCustomAPI.PenaltyCommandDividedAttack(_v);
                if ((_v.Command.AbilityId == BattleAbilityId.ScoopArt && GameRandom.Next8() % 4 == 0))
                {
                    _v.Context.Attack *= 2;
                    _v.Target.Flags |= CalcFlag.Critical;
                }
            }
            else
            {
                if (data.dms_geo_id == 569)
                {
                    _v.SetCommandPower();
                    _v.Caster.SetLowPhysicalAttack();
                    _v.Target.SetMagicDefense();
                    if (_v.Command.HitRate == 98)
                    {
                        _v.Context.DefensePower = _v.Context.DefensePower - (_v.Context.DefensePower / 4);
                    }
                    _v.Target.SetMagicDefense();
                    TranceSeekCustomAPI.CasterPenaltyMini(_v);
                    TranceSeekCustomAPI.PenaltyShellAttack(_v);
                    TranceSeekCustomAPI.PenaltyCommandDividedAttack(_v);
                    if (_v.Command.HitRate == 99 && GameRandom.Next8() % 4 == 0)
                    {
                        _v.Context.Attack *= 2;
                        _v.Target.Flags |= CalcFlag.Critical;
                    }
                }
                else
                {

                    if (_v.Caster.IsPlayer)
                    {
                        _v.OriginalMagicParams();
                        TranceSeekCustomAPI.CharacterBonusPassive(_v, "LowPhysicalAttack");
                    }
                    else
                    {
                        _v.SetCommandPower();
                        _v.Caster.SetLowPhysicalAttack();
                        _v.Target.SetMagicDefense();
                    }
                    TranceSeekCustomAPI.CasterPenaltyMini(_v);
                    if (_v.Target.IsUnderStatus(BattleStatus.Defend))
                    {
                        _v.Context.Attack >>= 1;
                    }
                    TranceSeekCustomAPI.PenaltyShellAttack(_v);
                    if (!_v.Caster.IsPlayer)
                    {
                        TranceSeekCustomAPI.BonusBackstabAndPenaltyLongDistance(_v);
                    }
                }
            }
            TranceSeekCustomAPI.EnemyTranceBonusAttack(_v);
            TranceSeekCustomAPI.BonusElement(_v);
            if (TranceSeekCustomAPI.CanAttackMagic(_v))
            {
                if (_v.Caster.PlayerIndex == CharacterId.Freya) // Dragon abilities
                {
                    TranceSeekCustomAPI.TryCriticalHit(_v);
                    if (_v.Target.IsUnderAnyStatus(TranceSeekCustomStatus.Dragon) || _v.Caster.IsUnderStatus(BattleStatus.Trance))
                    {
                        switch (_v.Command.AbilityId)
                        {
                            case BattleAbilityId.DragonBreath:
                                if (_v.Target.MagicDefence != 255)
                                    _v.Context.DefensePower /= 2;
                                break;
                            case BattleAbilityId.WhiteDraw:
                                _v.Caster.Flags = (CalcFlag.HpAlteration | CalcFlag.HpRecovery);
                                _v.Caster.HpDamage = _v.Context.EnsureAttack * _v.Context.EnsurePowerDifference / 4;
                                foreach (BattleUnit battleUnit in BattleState.EnumerateUnits())
                                {
                                    if (battleUnit.IsPlayer && battleUnit.IsTargetable && !battleUnit.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify))
                                    {
                                        _v.Caster.Change(battleUnit);
                                        SBattleCalculator.CalcResult(_v);
                                        BattleState.Unit2DReq(battleUnit);
                                    }
                                }
                                _v.Caster.Flags = 0;
                                _v.Caster.HpDamage = 0;
                                break;
                            case BattleAbilityId.SixDragons:
                                btl_stat.RemoveStatuses(_v.Target, _v.Command.AbilityStatus);
                                break;
                            case BattleAbilityId.DragonCrest:
                                _v.Target.TryAlterStatuses(BattleStatus.Doom, false, _v.Caster);
                                break;
                            case BattleAbilityId.CherryBlossom:
                                if (_v.Caster.Will > Comn.random16() % 100)
                                {
                                    _v.Target.TryAlterStatuses(BattleStatus.Venom, false, _v.Caster);
                                }
                                else
                                {
                                    _v.Target.TryAlterStatuses(BattleStatus.Poison, false, _v.Caster);
                                }                                   
                                break;
                        }
                    } 
                }
                else if (_v.Caster.PlayerIndex == CharacterId.Beatrix && _v.Command.AbilityId == (BattleAbilityId)1043)
                {
                    _v.Caster.Flags |= (CalcFlag.MpAlteration | CalcFlag.MpRecovery);
                    _v.Caster.MpDamage = (_v.Target.HpDamage >> 5);
                }
                _v.CalcHpDamage();
            }
            TranceSeekCustomAPI.InfusedWeaponStatus(_v);
            TranceSeekCustomAPI.TryAlterMagicStatuses(_v);
        }
    }
}
