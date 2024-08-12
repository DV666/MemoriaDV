using System;
using System.Collections.Generic;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Item, Hi-Potion
    /// </summary>
    [BattleScript(Id)]
    public sealed class ItemPotionScript : IBattleScript, IEstimateBattleScript
    {
        public const Int32 Id = 0069;

        private readonly BattleCalculator _v;

        public ItemPotionScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            _v.Context.Attack = 15;
            if (!_v.Caster.IsPlayer)
            {
                if (_v.Command.Power == 254) // Fandalf
                {
                    if (_v.Target.Data.dms_geo_id == 328 && SFX.currentEffectID == SpecialEffect.Shell) // Magic Shield
                    {
                        Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                        {
                        { "US", "Invincible!" },
                        { "UK", "Invincible!" },
                        { "JP", "インビンシブル!" },
                        { "ES", "Invencible!" },
                        { "FR", "Invincible !" },
                        { "GR", "Unbesiegbar!" },
                        { "IT", "Invincibile!" },
                        };
                        btl2d.Btl2dReqSymbolMessage(_v.Caster.Data, "[FF69B4]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 8);
                        return;
                    }   
                    _v.Target.Flags |= (CalcFlag.HpAlteration | CalcFlag.HpRecovery); // Full Life Frimoire
                    _v.Target.HpDamage = 2500;
                    return;
                }
                _v.Context.AttackPower = _v.Command.Power;
                if (_v.Command.Power == 15) // Potion
                {
                    _v.Context.Attack = 1;
                    _v.Context.AttackPower = 200;
                }
                else if (_v.Command.Power == 40) // Hi-Potion
                {
                    _v.Context.Attack = 1;
                    _v.Context.AttackPower = 500;
                }
                else if (_v.Command.Power == 70) // Ultra Potion
                {
                    _v.Context.Attack = 1;
                    _v.Context.AttackPower = 1250;
                }
                _v.Context.DefensePower = 0;
                _v.Target.Flags |= CalcFlag.HpAlteration;
                if (!_v.Target.IsZombie)
                {
                    _v.Target.Flags |= CalcFlag.HpRecovery;
                }
                _v.CalcHpMagicRecovery();
            }
            else
            {
                _v.Context.AttackPower = _v.Command.Item.Power;
                _v.Context.DefensePower = 0;
                if (_v.Command.Item.Power == 15) // Potion
                {
                    _v.Context.Attack = 1;
                    _v.Context.AttackPower = 200;
                }
                else if (_v.Command.Item.Power == 40) // Hi-Potion
                {
                    _v.Context.Attack = 1;
                    _v.Context.AttackPower = 500;
                }
                else if (_v.Command.Item.Power == 70) // Ultra Potion
                {
                    _v.Context.Attack = 1;
                    _v.Context.AttackPower = 1250;
                }
                if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1027) && (_v.Target.IsPlayer && BattleState.BattleUnitCount(true) > 1 || !_v.Target.IsPlayer && BattleState.BattleUnitCount(false) > 1))
                { // Herboriste +
                    Int32 Medecin = 0;
                    if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)100)) // Medecin
                    {
                        Medecin = 1;
                    }
                    else if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1100)) // Medecin +
                    {
                        Medecin = 2;
                    }
                    foreach (BattleUnit unit in BattleState.EnumerateUnits())
                    {
                        _v.Caster.Flags = CalcFlag.HpAlteration;
                        if (_v.Target.IsPlayer)
                        {
                            if (!unit.IsPlayer || !unit.IsTargetable || unit.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump))
                                continue;
                          
                            if (!unit.IsZombie)
                                _v.Caster.Flags |= CalcFlag.HpRecovery;

                            if (unit.Data == _v.Target.Data)
                            {
                                _v.Caster.HpDamage = _v.Context.AttackPower * _v.Context.Attack * 2;
                            }
                            else
                            {
                                _v.Caster.HpDamage = _v.Context.AttackPower * _v.Context.Attack;
                            }
                        }
                        else
                        {
                            if (unit.IsPlayer || !unit.IsTargetable || unit.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump))
                                continue;

                            if (!unit.IsZombie)
                                _v.Caster.Flags |= CalcFlag.HpRecovery;

                            if (unit.Data == _v.Target.Data)
                            {
                                _v.Caster.HpDamage = _v.Context.AttackPower * _v.Context.Attack * 2;
                            }
                            else
                            {
                                _v.Caster.HpDamage = _v.Context.AttackPower * _v.Context.Attack;
                            }
                        }
                        if (Medecin == 1) // Medecin
                        {
                            _v.Caster.HpDamage += _v.Caster.HpDamage / 4;
                        }
                        else if (Medecin == 2) // Medecin +
                        {
                            _v.Caster.HpDamage += _v.Caster.HpDamage / 2;
                        }
                        _v.Caster.Change(unit);
                        SBattleCalculator.CalcResult(_v);
                        BattleState.Unit2DReq(unit);
                    }
                    _v.Caster.Flags = 0;
                    _v.Caster.HpDamage = 0;
                    _v.PerformCalcResult = false;
                }
                else
                {
                    _v.CalcHpMagicRecovery();
                }  
            }
            if (_v.Target.Data.dms_geo_id == 416)
            {
                TranceSeekCustomAPI.MonsterMechanic[_v.Target.Data][1] = _v.Target.HpDamage;
                _v.Target.TryAlterSingleStatus(BattleStatusId.CustomStatus10, true, _v.Caster, _v.Target.HpDamage);
            }         
        }

        public Single RateTarget()
        {
            _v.Context.Attack = 15;
            _v.Context.AttackPower = _v.Command.Item.Power;
            _v.Context.DefensePower = 0;

            _v.CalcHpMagicRecovery();

            Single rate = _v.Target.HpDamage * BattleScriptDamageEstimate.RateHpMp((Int32)_v.Target.CurrentHp, (Int32)_v.Target.MaximumHp);

            if ((_v.Target.Flags & CalcFlag.HpRecovery) != CalcFlag.HpRecovery)
                rate *= -1;
            if (!_v.Target.IsPlayer)
                rate *= -1;

            return rate;
        }
    }
}
