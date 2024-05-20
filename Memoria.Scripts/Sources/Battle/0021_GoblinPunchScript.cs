using System;
using Memoria.Data;

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
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            BTL_DATA data = _v.Caster.Data;
            if (_v.Caster.PlayerIndex != CharacterId.Steiner && data.dms_geo_id != 296 && data.dms_geo_id != 298)
            {
                _v.NormalMagicParams();
                TranceSeekCustomAPI.CharacterBonusPassive(_v, "MagicAttack");
                if (data.dms_geo_id == 553)
                {
                    if (_v.Target.Level == _v.Caster.Level)
                    {
                        _v.Context.Attack += (int)_v.Caster.Level;
                    }
                    _v.Caster.PenaltyMini();
                    TranceSeekCustomAPI.PenaltyShellAttack(_v);
                    _v.CalcHpDamage();
                    _v.TryAlterMagicStatuses();
                }
                else
                {
                    if (_v.Target.Level == _v.Caster.Level)
                    {
                        _v.Context.Attack += (int)_v.Caster.Level;
                        _v.Context.DefensePower = 0;
                    }
                    _v.Caster.PenaltyMini();
                    TranceSeekCustomAPI.PenaltyShellAttack(_v);
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
                        TranceSeekCustomAPI.WeaponPhysicalParams(CalcAttackBonus.Simple, _v);
                        _v.Caster.PhysicalPenaltyAndBonusAttack();
                        _v.Target.GambleDefence();
                        TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                    }
                    else
                    {
                        _v.NormalPhysicalParams();
                        _v.Caster.PhysicalPenaltyAndBonusAttack();
                        TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                    }
                    if (GameRandom.Next8() % 3 != 0)
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
                        TranceSeekCustomAPI.BonusBackstabAndPenaltyLongDistanceTranceSeek(_v);
                        if (_v.Caster.IsPlayer)
                        {
                            _v.Caster.BonusWeaponElement();
                        }
                        else
                        {
                            _v.BonusElement();
                        }
                        if (_v.CanAttackWeaponElementalCommand())
                        {
                            TranceSeekCustomAPI.IpsenCastleMalus(_v);
                            _v.CalcPhysicalHpDamage();
                            TranceSeekCustomAPI.RaiseTrouble(_v);
                        }
                    }
                }
            }
            TranceSeekCustomAPI.SpecialSA(_v);
        }
    }
}