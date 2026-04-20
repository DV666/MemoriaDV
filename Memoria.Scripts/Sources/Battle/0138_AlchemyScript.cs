using FF9;
using Memoria.Data;
using NCalc;
using System;
using System.Collections.Generic;
using static Memoria.Assets.DataResources;

namespace Memoria.Scripts.TranceSeek
{
    [BattleScript(Id)]
    public sealed class AlchemyScript : IBattleScript
    {
        public const Int32 Id = 0138;

        private readonly BattleCalculator _v;

        public AlchemyScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            switch (_v.Command.AbilityId)
            {
                case TranceSeekBattleAbility.Concoction: // Concoction
                {
                    TranceSeekAPI.TryRemoveAbilityStatuses(_v);
                    break;
                }
                case TranceSeekBattleAbility.Pansement: // Bandage
                case TranceSeekBattleAbility.FirstAid: // Premiers soins
                {
                    _v.Target.Flags |= CalcFlag.HpDamageOrHeal;
                    _v.CalcDamageCommon();
                    _v.Target.HpDamage = (int)((_v.Target.MaximumHp * _v.Command.Power) / 100);
                    break;
                }
                case TranceSeekBattleAbility.Secretingredient: // Ingrédient secret
                {
                    _v.CasterState().Blank.SecretIngredient++;
                    break;
                }
                case TranceSeekBattleAbility.Soakedblade: // Lame trempée
                {
                    RegularItem SoakedBladeItem = _v.CasterState().Blank.SoakedBlade;
                    if (SoakedBladeItem != RegularItem.NoItem)
                        _v.Command.AbilityStatus = ff9item.GetItemEffect(SoakedBladeItem).status;

                    _v.WeaponPhysicalParams();
                    TranceSeekAPI.MagicAccuracy(_v);
                    
                    TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                    TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                    TranceSeekAPI.EnemyTranceBonusAttack(_v);
                    TranceSeekAPI.BonusElement(_v);
                    if (_v.CanAttackElementalCommand())
                    {
                        _v.CalcHpDamage();
                        TranceSeekAPI.TryCriticalHit(_v);
                        TranceSeekAPI.RaiseTrouble(_v);
                    }                   
                    _v.Target.PenaltyShellHitRate();
                    if (TranceSeekAPI.TryMagicHit(_v))
                    {
                        TranceSeekAPI.TryAlterCommandStatuses(_v);
                    }
                    break;
                }
                case TranceSeekBattleAbility.UrgentTreatment: // Traitement urgent
                case TranceSeekBattleAbility.MassTreatment: // Traitement collectif
                {
                    RegularItem ItemChoosen = _v.CasterState().Blank.SoakedBlade; // Last item used

                    if (ItemChoosen == 0 || GameState.ItemCount(ItemChoosen) <= 0)
                        _v.Context.Flags = BattleCalcFlags.Miss;

                    switch (ItemChoosen)
                    {
                        case RegularItem.Potion:
                        case RegularItem.HiPotion:
                        case TranceSeekRegularItem.UltraPotion: // Ultra Potion
                        {
                            _v.Context.AttackPower = ff9item.GetItemEffect(ItemChoosen).Ref.Power;
                            _v.Context.DefensePower = 0;
                            if (ff9item.GetItemEffect(ItemChoosen).Ref.Power == 15) // Potion
                            {
                                _v.Context.Attack = 1;
                                _v.Context.AttackPower = 200;
                            }
                            else if (ff9item.GetItemEffect(ItemChoosen).Ref.Power == 40) // Hi-Potion
                            {
                                _v.Context.Attack = 1;
                                _v.Context.AttackPower = 500;
                            }
                            else if (ff9item.GetItemEffect(ItemChoosen).Ref.Power == 70) // Ultra Potion
                            {
                                _v.Context.Attack = 1;
                                _v.Context.AttackPower = 1250;
                            }

                            if (_v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Doctor)) // Medecin
                                _v.Target.HpDamage += _v.Caster.HpDamage / (_v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Doctor_Boosted) ? 2 : 4);

                            _v.CalcHpMagicRecovery();
                            break;
                        }
                        case RegularItem.Ether:
                        case TranceSeekRegularItem.HiEther: // Ether +
                        {
                            _v.Context.Attack = 15;
                            _v.Context.AttackPower = ff9item.GetItemEffect(ItemChoosen).Ref.Power;
                            _v.Context.DefensePower = 0;
                            _v.CalcMpMagicRecovery();
                            break;
                        }
                        case RegularItem.Elixir:
                        case TranceSeekRegularItem.Megalixir: // Megalixir
                        {
                            if (!_v.Target.CanBeAttacked())
                                return;

                            if (_v.Target.IsZombie)
                            {
                                if (_v.Target.Data.dms_geo_id == 416)
                                {
                                    _v.TargetState().Monster.Special1 = 9999;
                                    _v.Target.CurrentHp = 1;
                                    return;
                                }
                                if (_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill))
                                {
                                    _v.Target.Flags |= (CalcFlag.HpAlteration | CalcFlag.MpAlteration);
                                    _v.Target.HpDamage = 9999;
                                    _v.Target.MpDamage = 999;
                                    return;
                                }
                                _v.Target.CurrentMp = 0;
                                _v.Target.Kill();
                            }
                            else
                            {
                                if (_v.Target.IsPlayer)
                                {
                                    _v.Target.Flags |= (CalcFlag.HpAlteration | CalcFlag.HpRecovery | CalcFlag.MpAlteration | CalcFlag.MpRecovery);
                                    _v.Target.HpDamage = (int)_v.Target.MaximumHp;
                                    _v.Target.MpDamage = (int)_v.Target.MaximumMp;
                                }
                                else
                                {
                                    _v.Target.CurrentHp = _v.Target.MaximumHp;
                                    _v.Target.CurrentMp = _v.Target.MaximumMp;
                                }
                                if (!_v.Caster.IsPlayer)
                                {
                                    TranceSeekAPI.TryAlterMagicStatuses(_v);
                                }
                            }
                            break;
                        }
                        case RegularItem.PhoenixDown:
                        case RegularItem.PhoenixPinion:
                        {
                            if (!_v.Target.CanBeRevived())
                                return;

                            if (_v.Target.Accessory == TranceSeekRegularItem.CursedRing) // Anneau Maudit
                            {
                                if (_v.Command.ItemId == RegularItem.PhoenixPinion && (_v.Target.Data.stat.permanent & BattleStatus.Doom) != 0 && !_v.Target.IsUnderAnyStatus(BattleStatus.Death))
                                {
                                    _v.Context.Flags |= BattleCalcFlags.Miss;
                                    return;
                                }
                            }

                            if (_v.Target.IsZombie && !_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill))
                            {
                                if ((_v.Target.CurrentHp = (UInt32)(GameRandom.Next8() % 10)) == 0)
                                    _v.Target.Kill();
                            }
                            else if (_v.Target.CheckIsPlayer())
                            {
                                if (_v.Target.IsUnderStatus(BattleStatus.Death))
                                    if (_v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.AutoLife_Boosted)) // Invincible+
                                    {
                                        _v.Target.Flags |= CalcFlag.HpAlteration | CalcFlag.HpRecovery | CalcFlag.MpAlteration | CalcFlag.MpRecovery;
                                        _v.Target.HpDamage = (int)_v.Target.MaximumHp;
                                        _v.Target.MpDamage = (int)_v.Target.MaximumMp;
                                    }
                                    else
                                    {
                                        _v.Target.CurrentHp = (UInt32)(1 + GameRandom.Next8() % 10);
                                    }
                                TranceSeekAPI.TryRemoveItemStatuses(_v);
                            }
                            break;
                        }
                        case RegularItem.EchoScreen:
                        case RegularItem.Soft:
                        case RegularItem.Antidote:
                        case RegularItem.EyeDrops:
                        case RegularItem.MagicTag:
                        case RegularItem.Vaccine:
                        case RegularItem.Remedy:
                        case RegularItem.Annoyntment:
                        case TranceSeekRegularItem.HiRemedy:
                        {
                            _v.Command.AbilityStatus = ff9item.GetItemEffect(ItemChoosen).status;
                            if ((_v.Command.ItemId == RegularItem.Remedy || _v.Command.ItemId == RegularItem.Annoyntment || _v.Command.ItemId == TranceSeekRegularItem.HiRemedy) && _v.Target.IsUnderAnyStatus(TranceSeekStatus.Vieillissement))
                            {
                                _v.Command.AbilityStatus |= TranceSeekStatus.Vieillissement;
                            }
                            TranceSeekAPI.TryRemoveAbilityStatuses(_v);
                            break;
                        }
                        case RegularItem.Garnet:
                        case RegularItem.Amethyst:
                        case RegularItem.Aquamarine:
                        case RegularItem.Diamond:
                        case RegularItem.Emerald:
                        case RegularItem.Moonstone:
                        case RegularItem.Ruby:
                        case RegularItem.Peridot:
                        case RegularItem.Sapphire:
                        case RegularItem.Opal:
                        case RegularItem.Topaz:
                        case RegularItem.LapisLazuli:
                        case RegularItem.Ore:
                        {
                            _v.Target.Flags |= CalcFlag.HpAlteration | CalcFlag.HpRecovery;
                            _v.Target.HpDamage = ff9item.GetItemEffect(ItemChoosen).Ref.Power * (ff9item.FF9Item_GetCount(ItemChoosen) + 1);
                            break;
                        }
                    }
                    BattleItem.RemoveFromInventory(ItemChoosen);
                    break;
                }
                case TranceSeekBattleAbility.MasterofAlchemy: // Maître Alchimiste
                {
                    _v.CasterState().Blank.MasterOfAlchemy++;
                    break;
                }                
            }
        }
    }
}




