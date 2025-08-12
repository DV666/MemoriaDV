using System;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Lancer
    /// </summary>
    [BattleScript(Id)]
    public sealed class LancerScript : IBattleScript
    {
        public const Int32 Id = 0039;

        private readonly BattleCalculator _v;

        public LancerScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Caster.PlayerIndex == CharacterId.Freya)
            {
                if (_v.IsCasterNotTarget() && _v.Target.CanBeAttacked() && !_v.Target.TryKillFrozen())
                {
                    _v.WeaponPhysicalParams();
                    TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                    TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                    _v.Caster.Flags |= CalcFlag.HpAlteration;
                    TranceSeekAPI.BonusBackstabAndPenaltyLongDistance(_v);
                    TranceSeekAPI.BonusWeaponElement(_v);
                    if (TranceSeekAPI.CanAttackWeaponElementalCommand(_v))
                    {
                        TranceSeekAPI.IpsenCastleMalus(_v);
                        TranceSeekAPI.RaiseTrouble(_v);
                        if (_v.Caster.HasSupportAbility(SupportAbility1.AddStatus))
                        {
                            _v.Context.StatusRate = _v.Caster.WeaponRate;
                            if (_v.Context.StatusRate > GameRandom.Next16() % 100)
                            {
                                _v.Context.Flags |= BattleCalcFlags.AddStat;
                            }
                        }
                    }
                    TranceSeekAPI.TryCriticalHit(_v);
                    _v.CalcPhysicalHpDamage();                     
                    int hpDamage = _v.Target.HpDamage = Math.Max(1, _v.Target.HpDamage);
                    _v.Target.FaceTheEnemy();
                    if (!_v.Context.IsAbsorb)
                    {
                        _v.Caster.Flags |= CalcFlag.HpRecovery;
                        _v.Target.Flags |= (CalcFlag.HpAlteration | CalcFlag.MpAlteration);
                        _v.Target.MpDamage = Math.Max(1, hpDamage >> 5);
                        _v.Caster.HpDamage = Math.Max(1, hpDamage / 2);
                        if (_v.Target.IsUnderAnyStatus(TranceSeekStatus.Dragon) || _v.Caster.IsUnderStatus(BattleStatus.Trance))
                        {
                            _v.Caster.Flags |= (CalcFlag.MpAlteration | CalcFlag.MpRecovery);
                            _v.Caster.MpDamage = _v.Target.MpDamage / 2;
                        }
                    }
                    TranceSeekAPI.TryAlterMagicStatuses(_v);
                }
            }
            else
            {
                if (_v.Command.Power == 1)
                {
                    if (_v.Target.CanBeAttacked())
                    {
                        _v.Target.CurrentHp = 1U;
                        _v.Target.CurrentMp = 1U;
                        TranceSeekAPI.TryAlterMagicStatuses(_v);
                    }
                }
                else
                {
                    if (_v.IsCasterNotTarget() && _v.Target.CanBeAttacked() && !_v.Target.TryKillFrozen())
                    {
                        _v.PhysicalAccuracy();
                        if (TranceSeekAPI.TryPhysicalHit(_v))
                        {
                            _v.NormalPhysicalParams();
                            TranceSeekAPI.CharacterBonusPassive(_v, "PhysicalAttack");
                            TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                            TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                            TranceSeekAPI.BonusBackstabAndPenaltyLongDistance(_v);
                            TranceSeekAPI.BonusElement(_v);
                            if (_v.CanAttackElementalCommand())
                            {
                                TranceSeekAPI.IpsenCastleMalus(_v);
                                _v.Target.Flags |= (CalcFlag.HpAlteration | CalcFlag.MpAlteration);
                                _v.CalcPhysicalHpDamage();
                                int hpDamage3 = _v.Target.HpDamage;
                                if ((_v.Target.Flags & CalcFlag.HpRecovery) != 0)
                                {
                                    _v.Target.FaceTheEnemy();
                                }
                                _v.Target.MpDamage = hpDamage3 >> 4;
                                BTL_DATA data = _v.Caster.Data;
                                if (data.dms_geo_id == 297)
                                {
                                    _v.Caster.Flags |= (CalcFlag.HpAlteration | CalcFlag.HpRecovery);
                                    _v.Caster.HpDamage = hpDamage3 / 2;
                                }
                                TranceSeekAPI.RaiseTrouble(_v);
                                TranceSeekAPI.TryAlterMagicStatuses(_v);
                            }
                        }
                    }
                }
            }
        }
    }
}
