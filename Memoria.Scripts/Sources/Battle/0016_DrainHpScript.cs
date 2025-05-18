using Memoria.Data;
using System;
using System.Collections.Generic;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Absorb Strength
    /// </summary>
    [BattleScript(Id)]
    public sealed class DrainHpScript : IBattleScript
    {
        public const Int32 Id = 0016;

        private readonly BattleCalculator _v;

        public DrainHpScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            _v.Context.IsDrain = true;
            if (_v.Caster.Data.dms_geo_id == 105 && _v.Command.HitRate == 222) // Soul Dance - Zombance
            {
                if (_v.Caster.SummonCount == 1)
                {
                    if (!_v.IsCasterNotTarget() || !_v.Target.CanBeAttacked())
                        return;

                    Int32 damage = 0;
                    _v.NormalMagicParams();
                    TranceSeekAPI.CharacterBonusPassive(_v, "MagicAttack");
                    TranceSeekAPI.CasterPenaltyMini(_v);
                    TranceSeekAPI.EnemyTranceBonusAttack(_v);
                    TranceSeekAPI.PenaltyShellAttack(_v);
                    _v.Target.Flags |= CalcFlag.MpAlteration;
                    _v.Caster.Flags |= CalcFlag.MpAlteration;

                    _v.CalcMpDamage();
                    damage = _v.Target.MpDamage / 3;

                    if (_v.Target.IsZombie)
                    {
                        _v.Target.Flags |= CalcFlag.MpRecovery;
                        if (damage > _v.Caster.CurrentMp)
                            damage = (Int32)_v.Caster.CurrentMp;
                    }
                    else
                    {
                        _v.Caster.Flags |= CalcFlag.MpRecovery;
                        if (damage > _v.Target.CurrentMp)
                            damage = (Int32)_v.Target.CurrentMp;
                    }

                    _v.Target.MpDamage = damage;
                    _v.Caster.MpDamage = damage;
                    if (GameRandom.Next16() % 2 == 0)
                    {
                        _v.Caster.SummonCount = 0;
                    }
                    else
                    {
                        _v.Caster.SummonCount = 1;
                    }
                    return;
                }
                if (GameRandom.Next16() % 2 == 0)
                {
                    _v.Caster.SummonCount = 0;
                }
                else
                {
                    _v.Caster.SummonCount = 1;
                }
            }
            if (_v.IsCasterNotTarget() && _v.Target.CanBeAttacked())
            {
                uint currentHp = _v.Target.CurrentHp;
                _v.NormalMagicParams();
                TranceSeekAPI.CharacterBonusPassive(_v, "MagicAttack");
                TranceSeekAPI.EnemyTranceBonusAttack(_v);
                TranceSeekAPI.BonusElement(_v);
                TranceSeekAPI.CasterPenaltyMini(_v);
                if (_v.Command.HitRate != 111)
                    TranceSeekAPI.PenaltyShellAttack(_v);
                if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)102))
                    TranceSeekAPI.TryCriticalHit(_v);
                if (_v.Command.HitRate == 254)
                {
                    _v.Caster.Flags |= CalcFlag.HpDamageOrHeal | CalcFlag.MpDamageOrHeal;
                    _v.Target.Flags |= CalcFlag.HpAlteration | CalcFlag.MpAlteration;
                    _v.CalcMpDamage();
                    _v.CalcHpDamage();
                    int num = _v.Target.MpDamage / 2;
                    _v.Caster.HpDamage = _v.Target.HpDamage;
                    if (num > _v.Target.CurrentMp)
                    {
                        num = (int)_v.Target.CurrentMp;
                    }
                    _v.Target.MpDamage = num;
                    _v.Caster.MpDamage = num;
                    TranceSeekAPI.TryAlterMagicStatuses(_v);
                }
                else
                {
                    if (TranceSeekAPI.CanAttackMagic(_v))
                    {
                        TranceSeekAPI.PrepareHpDraining(_v);
                        if (_v.Command.HitRate == 222) // Prison Cage CD4 - Vampire
                        {
                            if (_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill) || _v.Target.Data.dms_geo_id == 557 || _v.Target.Data.dms_geo_id == 558) // Vivi & Dagga from CD1.
                                _v.Target.HpDamage = (int)((_v.Target.MaximumHp - 10000) / 3);
                            else
                                _v.Target.HpDamage = (int)(_v.Target.MaximumHp / 3);
                        }
                        else if (_v.Command.HitRate == 223) // Prison Cage CD4 - Super Vampire / Plant Brain - Super Punction
                        {
                            if (_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill) && _v.Target.MaximumHp > 10000)
                                _v.Target.HpDamage = (int)((_v.Target.MaximumHp - 10000) / 2) + 1;
                            else
                                _v.Target.HpDamage = (int)(_v.Target.MaximumHp / 2) + 1;

                            _v.Caster.AlterStatus(TranceSeekStatus.PowerUp, _v.Caster);
                            _v.Caster.AlterStatus(TranceSeekStatus.MagicUp, _v.Caster);
                        }
                        else if (_v.Command.HitRate == 224) // Plant Brain CD1
                        {
                            if (_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill))
                                _v.Target.HpDamage = (int)((_v.Target.MaximumHp - 10000) / 3) + 1;
                            else
                                _v.Target.HpDamage = (int)(_v.Target.MaximumHp / 3) + 1;

                            _v.Caster.AlterStatus(TranceSeekStatus.PowerUp, _v.Caster);
                            _v.Caster.AlterStatus(TranceSeekStatus.MagicUp, _v.Caster);
                        }
                        else if (_v.Command.HitRate == 255) // Crowler - Deadly Drain
                        {
                            _v.Target.HpDamage = (int)(_v.Target.CurrentHp - 1U);
                        }
                        else
                        {
                            if (_v.Context.PowerDifference >= 1)
                            {
                                _v.CalcHpDamage();

                                if (_v.Target.HpDamage > currentHp)
                                {
                                    _v.Caster.HpDamage = (int)currentHp;
                                }
                                else
                                {
                                    _v.Caster.HpDamage = _v.Target.HpDamage;
                                }
                            }
                            TranceSeekAPI.TryAlterMagicStatuses(_v);
                        }
                    }
                }
            }
        }
    }
}
