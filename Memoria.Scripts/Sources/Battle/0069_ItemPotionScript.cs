using System;
using System.Collections.Generic;
using Memoria.Data;
using Memoria.Prime;
using static UIRoot;

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
                    _v.Target.HpDamage = (int)(_v.Target.MaximumHp - 10000);
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

                if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)100)) // Medecin
                    _v.Target.HpDamage += _v.Caster.HpDamage / (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1100) ? 2 : 4);

                if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1027) && (_v.Target.IsPlayer && BattleState.BattleUnitCount(true) > 1 || !_v.Target.IsPlayer && BattleState.BattleUnitCount(false) > 1))
                { // Herboriste +                    
                    foreach (BattleUnit unit in BattleState.EnumerateUnits())
                    {
                        int healing = 0;
                        if (_v.Target.IsPlayer)
                        {
                            if (!unit.IsPlayer || !unit.IsTargetable || unit.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump))
                                continue;                  

                            if (unit.Data == _v.Target.Data)
                            {
                                healing = _v.Context.AttackPower * _v.Context.Attack * 2;
                            }
                            else
                            {
                                healing = _v.Context.AttackPower * _v.Context.Attack;
                            }
                        }
                        else
                        {
                            if (unit.IsPlayer || !unit.IsTargetable || unit.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump))
                                continue;

                            if (unit.Data == _v.Target.Data)
                            {
                                healing = _v.Context.AttackPower * _v.Context.Attack * 2;
                            }
                            else
                            {
                                healing = _v.Context.AttackPower * _v.Context.Attack;
                            }
                        }

                        if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)100)) // Medecin
                            healing += healing / (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1100) ? 2 : 4);

                        if (unit.IsZombie)
                        {
                            btl2d.Btl2dStatReq(unit, healing, 0);
                            btl_para.SetDamage(unit, healing, 1, _v.Command.Data);
                        }
                        else
                        {
                            btl2d.Btl2dStatReq(unit, -healing, 0);
                            btl_para.SetRecover(unit, (uint)healing);
                        }
                        if (unit.Data.dms_geo_id == 416) // Meltigemini
                        {
                            TranceSeekAPI.MonsterMechanic[unit.Data][1] = Math.Min(healing, 9999);
                            btl_stat.AlterStatus(unit, TranceSeekStatusId.ZombieArmor, parameters: healing);
                        }
                    }
                }
                else
                {
                    _v.CalcHpMagicRecovery();
                    if (_v.Target.Data.dms_geo_id == 416) // Meltigemini
                    {
                        TranceSeekAPI.MonsterMechanic[_v.Target.Data][1] = Math.Min(_v.Target.HpDamage, 9999);
                        btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.ZombieArmor, parameters: _v.Target.HpDamage);
                    }
                }  
            }
            if (_v.Caster.PlayerIndex == CharacterId.Blank && _v.Command.Id == BattleCommandId.Item)
                btl_stat.AlterStatus(_v.Caster, TranceSeekStatusId.Special, _v.Caster, true, "SoakedBlade", (Int32)_v.Command.ItemId);
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
