﻿using Assets.Sources.Scripts.UI.Common;
using Memoria.Data;
using System.Collections.Generic;
using System;
using FF9;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Weapon 1-5, 7
    /// </summary>
    public abstract class BaseWeaponScript : IBattleScript
    {
        private readonly BattleCalculator _v;
        private readonly CalcAttackBonus _bonus;

        protected BaseWeaponScript(BattleCalculator v, CalcAttackBonus bonus)
        {
            _v = v;
            _bonus = bonus;
        }

        public virtual void Perform()
        {
            if ((_v.Caster.Weapon == RegularItem.Rod || _v.Caster.Weapon == RegularItem.MythrilRod || _v.Caster.Weapon == RegularItem.HealingRod || _v.Caster.Weapon == RegularItem.AsuraRod || _v.Caster.Weapon == RegularItem.WizardRod || _v.Caster.Weapon == RegularItem.WhaleWhisker))
            { 
                if (_v.Caster.PlayerIndex == _v.Target.PlayerIndex)
                    PrayDagga();
                else
                    Attack();             
            }
            else if (!_v.Target.TryKillFrozen())
            {
                Attack();
            }
        }

        public virtual void Attack()
        {
            _v.PhysicalAccuracy();
            if (_v.Caster.PlayerIndex == CharacterId.Amarant)
            {
                if (_v.Caster.IsUnderAnyStatus(BattleStatus.Defend) && _v.Command.Id == BattleCommandId.Counter && _v.Target.PhysicalEvade != 255) // Duel Amarant - Prevent dodge
                {
                    _v.Context.Evade = 0;
                }
            }
            if (TranceSeekCustomAPI.TryPhysicalHit(_v) || _v.Command.AbilityId == (BattleAbilityId)1161) // Attack from King Leo
            {
                TranceSeekCustomAPI.WeaponPhysicalParams(_bonus, _v);
                TranceSeekCustomAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                if (_v.Caster.IsUnderStatus(BattleStatus.Trance) && _v.Caster.PlayerIndex == CharacterId.Steiner)
                {
                    _v.Context.Attack += _v.Context.Attack / 4;
                }
                TranceSeekCustomAPI.BonusBackstabAndPenaltyLongDistance(_v);
                TranceSeekCustomAPI.BonusWeaponElement(_v);
                if (TranceSeekCustomAPI.CanAttackWeaponElementalCommand(_v))
                {
                    if (_v.Caster.HasSupportAbility(SupportAbility2.Mug) && !_v.Target.IsPlayer)
                    {
                        BattleEnemy battleEnemy = BattleEnemy.Find(_v.Target);
                        if (HasStealableItems(battleEnemy))
                        {
                            _v.Context.HitRate = (short)(_v.Caster.Level + _v.Caster.Will);
                            _v.Context.Evade = _v.Target.Level;
                            if (GameRandom.Next16() % _v.Context.HitRate >= GameRandom.Next16() % _v.Context.Evade || _v.Caster.HasSupportAbility(SupportAbility2.Bandit))
                            {
                                if (_v.Caster.HasSupportAbility(SupportAbility1.MasterThief))
                                    MugMasterThief();
                                else
                                    MugScript();                             

                                if (_v.Caster.HasSupportAbility(SupportAbility1.StealGil))
                                    StealGils();
                            }
                        }
                        ShowMugMessage();
                    }
                    if (_v.Caster.PlayerIndex == CharacterId.Amarant)
                    {
                        TranceSeekCustomAPI.AmarantPassive(_v);
                        if (_v.Caster.IsUnderAnyStatus(BattleStatus.Defend) && _v.Command.Id == BattleCommandId.Counter && TranceSeekCustomAPI.SpecialSAEffect[_v.Caster.Data][0] == 1) // Duel Amarant
                        {
                            short previouscriticalbonus = _v.Caster.Data.critical_rate_deal_bonus;
                            _v.Caster.Data.critical_rate_deal_bonus += _v.Caster.Will;
                            BattleStatus status = _v.Caster.WeaponStatus;
                            int statusrate = (50 + _v.Caster.WeaponRate + (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1025) ? (_v.Target.Will / 2) : 0));
                            if (!_v.Target.IsPlayer && status != 0)
                            {
                                if (((status & BattleStatus.Death) != 0 && !_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill)) || (status & BattleStatus.Death) == 0) // Don't force Death status.
                                {
                                    if ((GameRandom.Next8() % 100) < statusrate)
                                        _v.Command.AbilityStatus |= status;
                                }
                            }
                            TranceSeekCustomAPI.TryCriticalHit(_v);
                            _v.Caster.Data.critical_rate_deal_bonus = previouscriticalbonus;
                        }
                        else
                        {
                            TranceSeekCustomAPI.TryCriticalHit(_v);
                        }
                    }
                    else
                    {
                        TranceSeekCustomAPI.TryCriticalHit(_v);
                        TranceSeekCustomAPI.TryApplyDragon(_v);
                    }
                    if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)229) && (_v.Target.Flags & CalcFlag.Critical) != 0) // SA Lethality
                    {
                        _v.Command.AbilityStatus |= _v.Caster.WeaponStatus;
                        if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1229))
                            _v.Command.AbilityStatus |= BattleStatus.Doom;
                    }
                    TranceSeekCustomAPI.IpsenCastleMalus(_v);
                    _v.CalcPhysicalHpDamage();
                    TranceSeekCustomAPI.InfusedWeaponStatus(_v);
                    TranceSeekCustomAPI.TryAlterCommandStatuses(_v, false);
                    TranceSeekCustomAPI.RaiseTrouble(_v);
                }
            }
            else
            {
                if (_v.Caster.HasSupportAbility(SupportAbility2.Mug))
                {
                    ShowMugMessage();
                }
            }
        }

        public virtual void PrayDagga()
        {
            _v.NormalMagicParams();
            _v.Context.DefensePower = 0;
            switch (_v.Caster.Weapon)
            {
                // Prière
                case RegularItem.Rod:
                case RegularItem.MythrilRod:
                    _v.Context.AttackPower = 1;
                    break;
                // Prière +
                case RegularItem.HealingRod:
                case RegularItem.AsuraRod:
                    _v.Context.AttackPower = 6;
                    break;
                // Prière X
                case RegularItem.WizardRod:
                case RegularItem.WhaleWhisker:
                    _v.Context.AttackPower = 20;
                    break;
            }
            TranceSeekCustomAPI.CharacterBonusPassive(_v, "MagicAttack");
            TranceSeekCustomAPI.CasterPenaltyMini(_v);
            TranceSeekCustomAPI.EnemyTranceBonusAttack(_v);
            --_v.Context.DamageModifierCount;
            if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)102))
                TranceSeekCustomAPI.TryCriticalHit(_v);
            _v.Caster.HpDamage = _v.Context.EnsureAttack * _v.Context.EnsurePowerDifference;
            foreach (BattleUnit unit in BattleState.EnumerateUnits())
            {
                if (!unit.IsPlayer || !unit.IsTargetable || unit.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump))
                    continue;

                _v.Caster.Flags = CalcFlag.HpAlteration;
                if (!unit.IsUnderAnyStatus(BattleStatus.Zombie))
                    _v.Caster.Flags = CalcFlag.HpDamageOrHeal;

                _v.Caster.Change(unit);
                SBattleCalculator.CalcResult(_v);
                BattleState.Unit2DReq(unit);
            }
            _v.Caster.Flags = 0;
            _v.Caster.HpDamage = 0;
            _v.PerformCalcResult = false;           
        }

        public void MugScript()
        {
            BattleEnemy battleEnemy = BattleEnemy.Find(_v.Target);
            if (!HasStealableItems(battleEnemy))
            {
                return;
            }
            int SlotMugSteal = 4 + _v.Command.Data.info.effect_counter;
            if (GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[3], _v.Caster) && battleEnemy.StealableItems[3] != RegularItem.NoItem)
            {
                TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][SlotMugSteal] = (Int32)battleEnemy.StealableItems[3];
                MugItem(battleEnemy, 3);            
            }
            else if (GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[2], _v.Caster) && battleEnemy.StealableItems[2] != RegularItem.NoItem)
            {
                TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][SlotMugSteal] = (Int32)battleEnemy.StealableItems[2];
                MugItem(battleEnemy, 2);
            }
            else if (GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[1], _v.Caster) && battleEnemy.StealableItems[1] != RegularItem.NoItem)
            {
                TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][SlotMugSteal] = (Int32)battleEnemy.StealableItems[1];
                MugItem(battleEnemy, 1);
            }
            else if (GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[0], _v.Caster) && battleEnemy.StealableItems[0] != RegularItem.NoItem)
            {
                TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][SlotMugSteal] = (Int32)battleEnemy.StealableItems[0];
                MugItem(battleEnemy, 0);
            }
            else if (TranceSeekCustomAPI.ZidanePassive[_v.Target.Data][2] > 0) // Oeil de voleur activé
            {
                AddBonusSteal();
                Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                {
                    { "US", "Eye of the thief!" },
                    { "UK", "Eye of the thief!" },
                    { "JP", "Eye of the thief!" },
                    { "ES", "Eye of the thief!" },
                    { "FR", "Œil du voleur !" },
                    { "GR", "Eye of the thief!" },
                    { "IT", "Eye of the thief!" },
                };
                if (GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[3], _v.Caster) && battleEnemy.StealableItems[3] != RegularItem.NoItem)
                {
                    TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][SlotMugSteal] = (Int32)battleEnemy.StealableItems[3];
                    MugItem(battleEnemy, 3);
                }
                else if (GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[2], _v.Caster) && battleEnemy.StealableItems[2] != RegularItem.NoItem)
                {
                    TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][SlotMugSteal] = (Int32)battleEnemy.StealableItems[2];
                    MugItem(battleEnemy, 2);
                }
                else if (GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[1], _v.Caster) && battleEnemy.StealableItems[1] != RegularItem.NoItem)
                {
                    TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][SlotMugSteal] = (Int32)battleEnemy.StealableItems[1];
                    MugItem(battleEnemy, 1);
                }
                else if (GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[0], _v.Caster) && battleEnemy.StealableItems[0] != RegularItem.NoItem)
                {
                    TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][SlotMugSteal] = (Int32)battleEnemy.StealableItems[0];
                    MugItem(battleEnemy, 0);
                }
                else if (_v.Caster.IsUnderAnyStatus(BattleStatus.Trance) && _v.Caster.PlayerIndex == CharacterId.Zidane)
                {
                    if (battleEnemy.StealableItems[0] != RegularItem.NoItem)
                    {
                        TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][SlotMugSteal] = (Int32)battleEnemy.StealableItems[0];
                        MugItem(battleEnemy, 0);
                    }
                    else if (battleEnemy.StealableItems[1] != RegularItem.NoItem)
                    {
                        TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][SlotMugSteal] = (Int32)battleEnemy.StealableItems[1];
                        MugItem(battleEnemy, 1);
                    }
                    else if (battleEnemy.StealableItems[2] != RegularItem.NoItem)
                    {
                        TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][SlotMugSteal] = (Int32)battleEnemy.StealableItems[2];
                        MugItem(battleEnemy, 2);
                    }
                    else if (battleEnemy.StealableItems[3] != RegularItem.NoItem && GameRandom.Next8() < (127 + battleEnemy.StealableItemRates[3]))
                    {
                        TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][SlotMugSteal] = (Int32)battleEnemy.StealableItems[3];
                        MugItem(battleEnemy, 3);
                    }
                    else
                    {
                        AddBonusSteal();
                        return;
                    }
                    btl2d.Btl2dReqSymbolMessage(_v.Target.Data, "[FDEE00]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 10);
                }
                else
                {
                    AddBonusSteal();
                    return;
                }
                btl2d.Btl2dReqSymbolMessage(_v.Target.Data, "[FDEE00]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 10);
            }
            else if (_v.Caster.IsUnderAnyStatus(BattleStatus.Trance) && _v.Caster.PlayerIndex == CharacterId.Zidane)
            {
                if (battleEnemy.StealableItems[0] != RegularItem.NoItem)
                {
                    TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][SlotMugSteal] = (Int32)battleEnemy.StealableItems[0];
                    MugItem(battleEnemy, 0);
                }
                else if (battleEnemy.StealableItems[1] != RegularItem.NoItem)
                {
                    TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][SlotMugSteal] = (Int32)battleEnemy.StealableItems[1];
                    MugItem(battleEnemy, 1);
                }
                else if (battleEnemy.StealableItems[2] != RegularItem.NoItem)
                {
                    TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][SlotMugSteal] = (Int32)battleEnemy.StealableItems[2];
                    MugItem(battleEnemy, 2);
                }
                else if (battleEnemy.StealableItems[3] != RegularItem.NoItem && GameRandom.Next8() < (127 + battleEnemy.StealableItemRates[3]))
                {
                    TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][SlotMugSteal] = (Int32)battleEnemy.StealableItems[3];
                    MugItem(battleEnemy, 3);
                }
                else
                    AddBonusSteal();
            }
            else
            {
                AddBonusSteal();
            }
        }

        public void MugMasterThief()
        {
            short PreviousGameStateThefts = GameState.Thefts;
            BattleEnemy battleEnemy = BattleEnemy.Find(_v.Target);
            if (!HasStealableItems(battleEnemy))
            {
                return;
            }
            int MasterThiefTrigger = TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][3];
            int SlotMugSteal = 4 + _v.Command.Data.info.effect_counter;
            if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1022) && MasterThiefTrigger == 0) // If steal failed, will work the second time.
                MasterThiefTrigger = 1;
            if ((battleEnemy.StealableItems[3] != RegularItem.NoItem) && (MasterThiefTrigger >= 2 || GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[3], _v.Caster)))
            {
                TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][SlotMugSteal] = (Int32)battleEnemy.StealableItems[3];
                _v.StealItem(battleEnemy, 3);
                TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][3] = 0;
            }
            else if ((battleEnemy.StealableItems[2] != RegularItem.NoItem) && (MasterThiefTrigger >= 2 || GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[2], _v.Caster)))
            {
                TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][SlotMugSteal] = (Int32)battleEnemy.StealableItems[2];
                _v.StealItem(battleEnemy, 2);
                TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][3] = 0;
            }
            else if ((battleEnemy.StealableItems[1] != RegularItem.NoItem) && (MasterThiefTrigger >= 2 || GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[1], _v.Caster)))
            {
                TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][SlotMugSteal] = (Int32)battleEnemy.StealableItems[1];
                _v.StealItem(battleEnemy, 1);
                TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][3] = 0;
            }
            else if ((battleEnemy.StealableItems[0] != RegularItem.NoItem) && (MasterThiefTrigger >= 2 || GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[0], _v.Caster)))
            {
                TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][SlotMugSteal] = (Int32)battleEnemy.StealableItems[0];
                _v.StealItem(battleEnemy, 0);
                TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][3] = 0;
            }
            else
            {
                if (MasterThiefTrigger == 1)
                {
                    byte delay = (byte)(btl_util.getSerialNumber(_v.Caster.Data) == CharacterSerialNumber.ZIDANE_SWORD ? 8 : 16);
                    Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                    {
                        { "US", "Master Thief!" },
                        { "UK", "Master Thief!" },
                        { "JP", "目利きの手触り!" },
                        { "ES", "Toque experto!" },
                        { "FR", "Maître voleur !" },
                        { "GR", "Scharfsinn!" },
                        { "IT", "Mano di velluto!" },
                    };
                    btl2d.Btl2dReqSymbolMessage(_v.Caster.Data, "[FDEE00]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, delay);
                }
                TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][3] = MasterThiefTrigger + 1;
                if (_v.Caster.IsUnderAnyStatus(BattleStatus.Trance) && _v.Caster.PlayerIndex == CharacterId.Zidane && PreviousGameStateThefts == GameState.Thefts)
                    if (battleEnemy.StealableItems[0] != RegularItem.NoItem)
                    {
                        TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][SlotMugSteal] = (Int32)battleEnemy.StealableItems[0];
                        TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][SlotMugSteal] = (Int32)battleEnemy.StealableItems[0];
                        MugItem(battleEnemy, 0);
                    }
                    else if (battleEnemy.StealableItems[1] != RegularItem.NoItem)
                    {
                        TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][SlotMugSteal] = (Int32)battleEnemy.StealableItems[1];
                        TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][SlotMugSteal] = (Int32)battleEnemy.StealableItems[1];
                        MugItem(battleEnemy, 1);
                    }
                    else if (battleEnemy.StealableItems[2] != RegularItem.NoItem)
                    {
                        TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][SlotMugSteal] = (Int32)battleEnemy.StealableItems[2];
                        TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][SlotMugSteal] = (Int32)battleEnemy.StealableItems[2];
                        MugItem(battleEnemy, 2);
                    }
                    else if (battleEnemy.StealableItems[3] != RegularItem.NoItem && GameRandom.Next8() < (127 + battleEnemy.StealableItemRates[3]))
                    {
                        TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][SlotMugSteal] = (Int32)battleEnemy.StealableItems[3];
                        TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][SlotMugSteal] = (Int32)battleEnemy.StealableItems[3];
                        MugItem(battleEnemy, 3);
                    }
                else
                    AddBonusSteal();
            }
        }

        private static Boolean HasStealableItems(BattleEnemy enemy)
        {
            for (Int16 slot = 0; slot < 4; ++slot)
                if (enemy.StealableItems[slot] != RegularItem.NoItem)
                    return true;
            return false;
        }

        public void AddBonusSteal()
        {
            BattleEnemy battleEnemy = BattleEnemy.Find(_v.Target);
            var slot = new List<int>();
            for (Int32 i = 1; i < 4; i++)
            {
                if (battleEnemy.StealableItems[i] != RegularItem.NoItem)
                {
                    slot.Add(i);
                }

            }
            int slotchoosen = UnityEngine.Random.Range(0, slot.Count);
            battleEnemy.Data.steal_item_rate[slot[slotchoosen]] += 8;
        }

        public void MugItem(BattleEnemy enemy, Int32 slot)
        {            
            _v.Context.ItemSteal = enemy.StealableItems[slot];
            if (_v.Context.ItemSteal == RegularItem.NoItem)
            {
                UiState.SetBattleFollowFormatMessage(BattleMesages.CouldNotStealAnything);
                return;
            }

            enemy.StealableItems[slot] = RegularItem.NoItem;
            GameState.Thefts++;

            foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(_v.Caster))
                saFeature.TriggerOnAbility(_v, "Steal", false);
            foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(_v.Target))
                saFeature.TriggerOnAbility(_v, "Steal", true);

            BattleItem.AddToInventory(_v.Context.ItemSteal);
            if (_v.Caster.PlayerIndex == CharacterId.Zidane && ff9item._FF9Item_Data[FF9StateSystem.Common.FF9.player[(CharacterId)_v.Caster.Data.bi.slot_no].equip.Weapon].shape != 1) // Thief Sword
                UiState.SetBattleFollowFormatMessage(BattleMesages.Stole, FF9TextTool.ItemName(_v.Context.ItemSteal));         
        }

        public void ShowMugMessage()
        {
            if (_v.Caster.PlayerIndex == CharacterId.Zidane && ff9item._FF9Item_Data[FF9StateSystem.Common.FF9.player[(CharacterId)_v.Caster.Data.bi.slot_no].equip.Weapon].shape == 1) // Dagger
            {
                RegularItem FirstItemMugged = (RegularItem)TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][5];
                RegularItem SecondItemMugged = (RegularItem)TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][6];
                if (_v.Command.Data.info.effect_counter == 2)
                {

                    if (FirstItemMugged != RegularItem.NoItem && SecondItemMugged != RegularItem.NoItem)
                        UiState.SetBattleFollowFormatMessage(BattleMesages.Stole, $"{FF9TextTool.ItemName(SecondItemMugged)} / {FF9TextTool.ItemName(FirstItemMugged)}");
                    else if (FirstItemMugged != RegularItem.NoItem)
                        UiState.SetBattleFollowFormatMessage(BattleMesages.Stole, FF9TextTool.ItemName(FirstItemMugged));
                    else if (SecondItemMugged != RegularItem.NoItem)
                        UiState.SetBattleFollowFormatMessage(BattleMesages.Stole, FF9TextTool.ItemName(SecondItemMugged));
                    TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][5] = 255;
                    TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][6] = 255;
                }
            }
        }

        public void StealGils()
        {
            if (btl_util.getEnemyPtr(_v.Target).bonus_gil > 0)
            {
                int bonusgil = 0;
                byte delay = 16;
                CharacterSerialNumber serialNumber = btl_util.getSerialNumber(_v.Caster.Data);
                if (serialNumber == CharacterSerialNumber.ZIDANE_SWORD)
                    delay = 8;

                if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1023))
                {
                    bonusgil = (int)UnityEngine.Random.Range(btl_util.getEnemyPtr(_v.Target).bonus_gil / 12, btl_util.getEnemyPtr(_v.Target).bonus_gil / 6);
                }
                else
                {
                    bonusgil = (int)(GameRandom.Next16() % (btl_util.getEnemyPtr(_v.Target).bonus_gil / 8));
                }

                if (ff9item._FF9Item_Data[FF9StateSystem.Common.FF9.player[(CharacterId)_v.Caster.Data.bi.slot_no].equip.Weapon].shape == 1 && _v.Command.Data.info.effect_counter != 2)
                {
                    TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][8] = bonusgil;
                }
                else
                {
                    bonusgil = bonusgil + TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][8];
                    TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][8] = 0;
                    Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                        {
                          { "US", $"+{bonusgil} gils!" },
                          { "UK", $"+{bonusgil} gils!" },
                          { "JP", $"+{bonusgil} ギル!" },
                          { "ES", $"+{bonusgil} guiles!" },
                          { "FR", $"+{bonusgil} gils !" },
                          { "GR", $"+{bonusgil} Gil!" },
                          { "IT", $"+{bonusgil} Guil!" },
                        };
                    btl2d.Btl2dReqSymbolMessage(_v.Caster.Data, NGUIText.FF9YellowColor, localizedMessage, HUDMessage.MessageStyle.DAMAGE, delay);
                    GameState.Gil += (uint)bonusgil;
                }
            }
        }
    }
}
