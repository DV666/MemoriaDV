using FF9;
using Memoria.Data;
using Memoria.Prime;
using System;
using System.Collections.Generic;

namespace Memoria.Scripts.TranceSeek
{
    /// <summary>
    /// Goblin Punch
    /// </summary>
    [BattleScript(Id)]
    public sealed class GoblinPunchScript : IBattleScript
    {
        public const Int32 Id = 0021;

        private readonly BattleCalculator _v;

        public GoblinPunchScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            BTL_DATA data = _v.Caster.Data;
            if (_v.Caster.PlayerIndex != CharacterId.Steiner && data.dms_geo_id != 296 && data.dms_geo_id != 298)
            {
                _v.NormalMagicParams();
                
                if (data.dms_geo_id == 553)
                {
                    if (_v.Target.Level == _v.Caster.Level)
                    {
                        _v.Context.Attack += (int)_v.Caster.Level;
                    }
                    TranceSeekAPI.CasterPenaltyMini(_v);
                    TranceSeekAPI.PenaltyShellAttack(_v);
                    _v.CalcHpDamage();
                    TranceSeekAPI.TryAlterMagicStatuses(_v);
                }
                else
                {
                    if (_v.Target.Level == _v.Caster.Level)
                    {
                        _v.Context.Attack += (int)_v.Caster.Level;
                        _v.Context.DefensePower = 0;
                    }
                    TranceSeekAPI.CasterPenaltyMini(_v);
                    TranceSeekAPI.PenaltyShellAttack(_v);
                    _v.CalcHpDamage();
                }
            }
            else
            {
                if (!TranceSeekAPI.TryKillFrozen(_v))
                {
                    var Caster_TSVar = _v.CasterState();
                    int ChanceDeathBlow = 33;
                    if (Caster_TSVar.Steiner.PlutoStackUsed > 0)
                        ChanceDeathBlow += 10 * Caster_TSVar.Steiner.PlutoStackUsed;

                    if (Comn.random16() % 100 > ChanceDeathBlow && _v.Command.AbilityId != TranceSeekBattleAbility.PlutoStrike && false)
                    {
                        _v.Context.Flags |= BattleCalcFlags.Miss;
                    }
                    else
                    {
                        TranceSeekAPI.WeaponPhysicalParams(CalcAttackBonus.Simple, _v);
                        TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                        TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                        if (_v.Caster.IsUnderStatus(BattleStatus.Trance) && _v.Caster.PlayerIndex == CharacterId.Steiner)
                            _v.Context.DamageModifierCount++;
                        TranceSeekAPI.BonusBackstabAndPenaltyLongDistance(_v);
                        TranceSeekAPI.BonusWeaponElement(_v);
                        if (TranceSeekAPI.CanAttackWeaponElementalCommand(_v))
                        {
                            TranceSeekAPI.IpsenCastleMalus(_v);
                            _v.CalcPhysicalHpDamage();
                            _v.Target.HpDamage *= 2;
                            _v.Target.MpDamage *= 2;
                            _v.Target.Flags |= CalcFlag.Critical;
                            TranceSeekAPI.InfusedWeaponStatus(_v);
                            TranceSeekAPI.TryAlterCommandStatuses(_v, false);
                            TranceSeekAPI.RaiseTrouble(_v);
                        }
                    }

                    _v.CasterState().Steiner.Duelist = 0;
                    TranceSeekCharacterMechanic.ResetSteinerPassive(_v.Caster);
                }
            }
        }
    }
}

