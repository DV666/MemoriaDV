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
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            // SFX.currentEffectID == SpecialEffect.Osmose
            if (_v.Caster.Data.dms_geo_id == 105 && _v.Command.HitRate == 222) // Soul Dance - Zombance
            {
                if (_v.Caster.SummonCount == 1)
                {
                    if (!_v.IsCasterNotTarget() || !_v.Target.CanBeAttacked())
                        return;

                    Int32 damage = 0;
                    _v.NormalMagicParams();
                    TranceSeekCustomAPI.CharacterBonusPassive(_v, "MagicAttack");
                    _v.Caster.PenaltyMini();
                    _v.Caster.EnemyTranceBonusAttack();
                    TranceSeekCustomAPI.PenaltyShellAttack(_v);
                    _v.Target.Flags |= CalcFlag.MpAlteration;
                    _v.Caster.Flags |= CalcFlag.MpAlteration;
                    _v.Context.IsDrain = true;

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
                    TranceSeekCustomAPI.SpecialSA(_v);
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
                TranceSeekCustomAPI.CharacterBonusPassive(_v, "MagicAttack");
                _v.Caster.EnemyTranceBonusAttack();
                _v.BonusElement();
                _v.Caster.PenaltyMini();
                if (_v.Command.HitRate != 111)
                    TranceSeekCustomAPI.PenaltyShellAttack(_v);
                if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)102))
                    TranceSeekCustomAPI.TryCriticalHit(_v);
                if (_v.Command.HitRate == 254)
                {
                    _v.Caster.Flags |= (CalcFlag)27;
                    _v.Target.Flags |= (CalcFlag)9;
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
                    _v.TryAlterMagicStatuses();
                }
                else
                {
                    _v.PrepareHpDraining();
                    if (_v.Command.HitRate == 255)
                    {
                        _v.Target.HpDamage = (int)(_v.Target.CurrentHp - 1U);
                    }
                    else
                    {
                        if (_v.Context.PowerDifference >= 1 && TranceSeekCustomAPI.CanAttackMagic(_v))
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
                        _v.TryAlterMagicStatuses();
                    }
                }
                if (_v.Caster.Data.dms_geo_id == 347 && _v.Command.HitRate == 1)
                {
                    Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                    {
                        { "US", "↑ Strength ↑" },
                        { "UK", "↑ Strength ↑" },
                        { "JP", "↑ ちから！ ↑" },
                        { "ES", "↑ Forza ↑" },
                        { "FR", "↑ Force ↑" },
                        { "GR", "↑ Fuerza ↑" },
                        { "IT", "↑ Stärke ↑" },
                    };
                    btl2d.Btl2dReqSymbolMessage(_v.Caster.Data, "[FFA500]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 15);
                    Dictionary<String, String> localizedMessage2 = new Dictionary<String, String>
                    {
                        { "US", "↑ Magic ↑" },
                        { "UK", "↑ Magic ↑" },
                        { "JP", "↑ まりょく！ ↑" },
                        { "ES", "↑ POT magico ↑" },
                        { "FR", "↑ Magie ↑" },
                        { "GR", "↑ Magia ↑" },
                        { "IT", "↑ Zauber ↑" },
                    };
                    btl2d.Btl2dReqSymbolMessage(_v.Caster.Data, "[FFA500]", localizedMessage2, HUDMessage.MessageStyle.DAMAGE, 20);
                }
                TranceSeekCustomAPI.SpecialSA(_v);
            }
        }
    }
}
