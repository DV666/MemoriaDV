using System;
using Memoria.Data;
using Memoria.Prime;

namespace Memoria.Scripts.Battle
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
                TranceSeekAPI.CharacterBonusPassive(_v, "MagicAttack");
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
                if (!_v.Target.TryKillFrozen())
                {
                    CalcContext context = _v.Context;
                    if (_v.Caster.IsPlayer)
                    {
                        TranceSeekAPI.WeaponPhysicalParams(CalcAttackBonus.Simple, _v);
                        TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                        _v.Target.GambleDefence();
                        TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                    }
                    else
                    {
                        _v.NormalPhysicalParams();
                        TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                        TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                    }
                    if ((GameRandom.Next8() % (_v.Command.AbilityId == (BattleAbilityId)1550 ? 2 : 3) != 0) && _v.Command.AbilityId != (BattleAbilityId)1551)
                    {
                        _v.Context.Flags |= BattleCalcFlags.Miss;
                    }
                    else
                    {
                        if (_v.Caster.IsUnderStatus(BattleStatus.Trance) && _v.Caster.PlayerIndex == CharacterId.Steiner)
                        {
                            context.Attack += context.Attack / 4;
                        }
                        context.Attack += context.Attack;
                        _v.Target.Flags |= CalcFlag.Critical;
                        TranceSeekAPI.BonusBackstabAndPenaltyLongDistance(_v);
                        if (_v.Caster.IsPlayer)
                        {
                            TranceSeekAPI.BonusWeaponElement(_v);
                        }
                        else
                        {
                            TranceSeekAPI.BonusElement(_v);
                        }
                        if (TranceSeekAPI.CanAttackWeaponElementalCommand(_v))
                        {
                            TranceSeekAPI.IpsenCastleMalus(_v);
                            _v.CalcPhysicalHpDamage();
                            if (_v.Command.AbilityId == (BattleAbilityId)1551)
                            {
                                _v.Caster.Flags |= CalcFlag.HpDamageOrHeal;
                                _v.Caster.HpDamage = _v.Target.HpDamage / 2;
                            }
                            TranceSeekAPI.RaiseTrouble(_v);
                            btl_stat.AlterStatus(_v.Caster, TranceSeekStatusId.Special, parameters: "Duelist--");
                        }
                    }
                }
            }
        }
    }
}
