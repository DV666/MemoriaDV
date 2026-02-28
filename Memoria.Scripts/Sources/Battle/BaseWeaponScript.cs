using Assets.Sources.Scripts.UI.Common;
using Memoria.Data;
using System.Collections.Generic;
using System;
using FF9;
using static Memoria.Scripts.Battle.TranceSeekBattleDictionary;

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
                DragonSkillScript.ReiWrathTrigger(_v);
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
            if (TranceSeekAPI.TryPhysicalHit(_v) || _v.Command.AbilityId == (BattleAbilityId)1161) // Attack from King Leo
            {
                if (_v.Command.Id == TranceSeekBattleCommand.MagicWeapon_Weak || _v.Command.Id == TranceSeekBattleCommand.MagicWeapon_Normal || _v.Command.Id == TranceSeekBattleCommand.MagicWeapon_Strong)
                {
                    _v.Target.RemoveStatus(BattleStatusConst.RemoveOnPhysicallyAttacked & ~_v.Context.AddedStatuses);
                    if (FF9StateSystem.EventState.gScriptDictionary.TryGetValue(1050, out Dictionary<Int32, Int32> dict))
                        dict[0] = 1;
                }

                TranceSeekAPI.WeaponPhysicalParams(_bonus, _v);
                TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                if (_v.Caster.IsUnderStatus(BattleStatus.Trance) && _v.Caster.PlayerIndex == CharacterId.Steiner)
                    _v.Context.DamageModifierCount ++;     
                TranceSeekAPI.BonusBackstabAndPenaltyLongDistance(_v);
                TranceSeekAPI.BonusWeaponElement(_v);
                if (TranceSeekAPI.CanAttackWeaponElementalCommand(_v))
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
                                MugScript();                             
                                if (_v.Caster.HasSupportAbility(SupportAbility1.StealGil))
                                    StealGils();
                            }
                        }
                        ShowMugMessage();
                    }
                    if (_v.Caster.PlayerIndex == CharacterId.Amarant)
                    {
                        TranceSeekCharacterMechanic.AmarantPassive(_v);
                        if (_v.Caster.IsUnderAnyStatus(BattleStatus.Defend) && _v.Command.Id == BattleCommandId.Counter && !_v.CasterState().Amarant.Duel) // Duel Amarant
                        {
                            short previouscriticalbonus = _v.Caster.Data.critical_rate_deal_bonus;
                            _v.Caster.Data.critical_rate_deal_bonus += _v.Caster.Will;
                            BattleStatus WeaponStatus = _v.Caster.WeaponStatus;
                            int HitRateWeaponStatus = (50 + _v.Caster.WeaponRate + (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1025) ? (_v.Target.Will / 2) : 0));
                            if (WeaponStatus != 0)
                            {
                                if (((WeaponStatus & BattleStatus.Death) != 0 && TranceSeekAPI.EliteMonster(_v.Target.Data)) || (WeaponStatus & BattleStatus.Death) == 0) // Don't force Death status.
                                {
                                    if ((WeaponStatus & BattleStatus.Death) != 0 && TranceSeekAPI.EliteMonster(_v.Target.Data))
                                        HitRateWeaponStatus /= 2;

                                    if ((GameRandom.Next8() % 100) < HitRateWeaponStatus)
                                        _v.Command.AbilityStatus |= WeaponStatus;
                                }
                            }
                            else if ((WeaponStatus & BattleStatus.Death) != 0 && (_v.Target.ResistStatus & BattleStatus.Death) != 0)
                                _v.TargetState().TriggerSPSResistStatus = true;

                            TranceSeekAPI.TryCriticalHit(_v);
                            _v.Caster.Data.critical_rate_deal_bonus = previouscriticalbonus;
                        }
                        else
                        {
                            TranceSeekAPI.TryCriticalHit(_v);
                        }
                    }
                    else
                    {
                        TranceSeekAPI.TryCriticalHit(_v);
                    }
                    if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)229) && (_v.Target.Flags & CalcFlag.Critical) != 0) // SA Lethality
                    {
                        _v.Command.AbilityStatus |= _v.Caster.WeaponStatus;
                        if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1229))
                            _v.Command.AbilityStatus |= BattleStatus.Doom;
                    }
                    if (_v.Caster.HasSupportAbility(SupportAbility1.AddStatus)) // SA Add Status (to handle specific case, like Elite Monsters). Can be improved ...?
                    {
                        BattleStatus WeaponStatus = _v.Caster.WeaponStatus;

                        if (((WeaponStatus & BattleStatus.Death) != 0 && !_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill)) || (WeaponStatus & BattleStatus.Death) == 0) // Don't force Death status.
                        {
                            int HitRateWeaponStatus = _v.Caster.WeaponRate + (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1025) ? (_v.Target.Will / 3) : 0);
                            if ((WeaponStatus & BattleStatus.Death) != 0 && TranceSeekAPI.EliteMonster(_v.Target.Data))
                                    HitRateWeaponStatus /= 2;

                            if ((GameRandom.Next8() % 100) < HitRateWeaponStatus)
                                _v.Command.AbilityStatus |= WeaponStatus;
                        }
                        else if ((WeaponStatus & BattleStatus.Death) != 0 && (_v.Target.ResistStatus & BattleStatus.Death) != 0)
                            _v.TargetState().TriggerSPSResistStatus = true;
                    }
                    TranceSeekAPI.IpsenCastleMalus(_v);
                    _v.CalcPhysicalHpDamage();
                    TranceSeekAPI.InfusedWeaponStatus(_v);
                    TranceSeekAPI.TryAlterCommandStatuses(_v, false);
                    TranceSeekAPI.RaiseTrouble(_v);
                    if (_v.Caster.PlayerIndex == CharacterId.Zidane && ff9item._FF9Item_Data[_v.Caster.Weapon].shape == 1) // Zidane - Dagger double hits
                    { 
                        _v.Target.HpDamage /= 2;
                        if (_v.Command.Data.info.effect_counter == 0)
                            _v.Command.AbilityCategory -= 64; // Hit Anim off
                    }
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
                    _v.Context.AttackPower = 15;
                    break;
            }
            
            TranceSeekAPI.CasterPenaltyMini(_v);
            TranceSeekAPI.EnemyTranceBonusAttack(_v);
            if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)102))
                TranceSeekAPI.TryCriticalHit(_v);
            _v.Caster.HpDamage = _v.Context.EnsureAttack * _v.Context.EnsurePowerDifference;
            if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)100)) // Medecin
                _v.Caster.HpDamage += _v.Caster.HpDamage / (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1100) ? 2 : 4);

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

        public void MugScript()
        {
            BattleEnemy battleEnemy = BattleEnemy.Find(_v.Target);
            if (!HasStealableItems(battleEnemy))
                return;

            var casterState = _v.CasterState();
            bool isFirstMug = _v.Command.Data.info.effect_counter == 1;

            void AssignStolenItem(int slotIndex)
            {
                RegularItem itemToSteal = battleEnemy.StealableItems[slotIndex];
                if (isFirstMug) casterState.Zidane.FirstItemMug = itemToSteal;
                else casterState.Zidane.SecondItemMug = itemToSteal;

                MugItem(battleEnemy, slotIndex);
            }

            bool TryStealSlot(int slot)
            {
                return GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[slot], _v.Caster) &&
                       battleEnemy.StealableItems[slot] != RegularItem.NoItem;
            }

            bool TryGuaranteedTranceSteal()
            {
                if (battleEnemy.StealableItems[0] != RegularItem.NoItem) { AssignStolenItem(0); return true; }
                if (battleEnemy.StealableItems[1] != RegularItem.NoItem) { AssignStolenItem(1); return true; }
                if (battleEnemy.StealableItems[2] != RegularItem.NoItem) { AssignStolenItem(2); return true; }
                if (battleEnemy.StealableItems[3] != RegularItem.NoItem && GameRandom.Next8() < (127 + battleEnemy.StealableItemRates[3]))
                {
                    AssignStolenItem(3); return true;
                }
                return false;
            }

            bool IsZidaneTrance() => _v.Caster.IsUnderAnyStatus(BattleStatus.Trance) && _v.Caster.PlayerIndex == CharacterId.Zidane;

            if (TryStealSlot(3)) AssignStolenItem(3);
            else if (TryStealSlot(2)) AssignStolenItem(2);
            else if (TryStealSlot(1)) AssignStolenItem(1);
            else if (TryStealSlot(0)) AssignStolenItem(0);

            else if (_v.TargetState().Zidane.EyeOfTheThief)
            {
                AddBonusSteal();
                bool success = false;

                if (TryStealSlot(3)) { AssignStolenItem(3); success = true; }
                else if (TryStealSlot(2)) { AssignStolenItem(2); success = true; }
                else if (TryStealSlot(1)) { AssignStolenItem(1); success = true; }
                else if (TryStealSlot(0)) { AssignStolenItem(0); success = true; }
                else if (IsZidaneTrance())
                {
                    success = TryGuaranteedTranceSteal();
                    if (!success) AddBonusSteal();
                }

                if (success)
                {
                    Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                    {
                        { "US", $"Thief's Eye!" },
                        { "UK", $"Thief's Eye!" },
                        { "JP", $"盗賊の眼！" },
                        { "ES", $"¡Ojo de ladrón!" },
                        { "FR", $"Œil du voleur !" },
                        { "DE", $"Diebesauge!" },
                        { "IT", $"Occhio del ladro!" },
                    };
                    btl2d.Btl2dReqSymbolMessage(_v.Target.Data, "[FDEE00]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 10);
                }
            }
            else if (IsZidaneTrance())
            {
                if (!TryGuaranteedTranceSteal()) AddBonusSteal();
            }
            else AddBonusSteal();
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
            var casterState = _v.CasterState(); // Nouvel appel !

            foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(_v.Caster))
                saFeature.TriggerOnAbility(_v, "Steal", false);
            foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(_v.Target))
                saFeature.TriggerOnAbility(_v, "Steal", true);

            if (_v.Caster.HasSupportAbility(SupportAbility1.MasterThief) && slot == 0 || _v.Caster.HasSupportAbilityByIndex((SupportAbility)1022) && slot == 1)
            {
                ff9item.FF9Item_Add(_v.Context.ItemSteal, 2);

                if (_v.Caster.HasSupportAbility(SupportAbility1.MasterThief) && slot == 0)
                    casterState.Zidane.ItemMugMasterThief = 1;
                else if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1022) && slot == 1)
                    casterState.Zidane.ItemMugMasterThief = 2;

                if (_v.Caster.PlayerIndex == CharacterId.Zidane && ff9item._FF9Item_Data[FF9StateSystem.Common.FF9.player[(CharacterId)_v.Caster.Data.bi.slot_no].equip.Weapon].shape == 2)
                {
                    UiState.SetBattleFollowFormatMessage(BattleMesages.Stole, FF9TextTool.ItemName(_v.Context.ItemSteal) + " X 2");
                    if (StealScript.ForcedHeheZidane) SoundLib.PlaySoundEffect(4005);
                }
            }
            else
            {
                BattleItem.AddToInventory(_v.Context.ItemSteal);
                if (_v.Caster.PlayerIndex == CharacterId.Zidane && ff9item._FF9Item_Data[FF9StateSystem.Common.FF9.player[(CharacterId)_v.Caster.Data.bi.slot_no].equip.Weapon].shape == 2)
                {
                    UiState.SetBattleFollowFormatMessage(BattleMesages.Stole, FF9TextTool.ItemName(_v.Context.ItemSteal));
                    if (StealScript.ForcedHeheZidane) SoundLib.PlaySoundEffect(4005);
                }
            }
            TranceSeekAPI.PhantomHandSA(_v);
        }

        public void ShowMugMessage()
        {
            if (_v.Caster.PlayerIndex != CharacterId.Zidane) return;

            var casterState = _v.CasterState();

            if (ff9item._FF9Item_Data[FF9StateSystem.Common.FF9.player[(CharacterId)_v.Caster.Data.bi.slot_no].equip.Weapon].shape == 1 && _v.Command.Data.info.effect_counter == 2)
            {
                if (casterState.Zidane.FirstItemMug != RegularItem.NoItem || casterState.Zidane.SecondItemMug != RegularItem.NoItem)
                {
                    RegularItem firstItem = casterState.Zidane.FirstItemMug;
                    RegularItem secondItem = casterState.Zidane.SecondItemMug;

                    string firstText = casterState.Zidane.ItemMugMasterThief >= 1 ? $"{FF9TextTool.ItemName(firstItem)} X 2" : $"{FF9TextTool.ItemName(firstItem)}";
                    string secondText = casterState.Zidane.ItemMugMasterThief == 2 ? $"{FF9TextTool.ItemName(secondItem)} X 2" : $"{FF9TextTool.ItemName(secondItem)}";

                    if (firstItem != RegularItem.NoItem && secondItem != RegularItem.NoItem)
                        UiState.SetBattleFollowFormatMessage(BattleMesages.Stole, firstText + " / " + secondText);
                    else if (firstItem != RegularItem.NoItem)
                        UiState.SetBattleFollowFormatMessage(BattleMesages.Stole, firstText);
                    else if (secondItem != RegularItem.NoItem)
                        UiState.SetBattleFollowFormatMessage(BattleMesages.Stole, secondText);

                    casterState.Zidane.FirstItemMug = RegularItem.NoItem;
                    casterState.Zidane.SecondItemMug = RegularItem.NoItem;
                    casterState.Zidane.ItemMugMasterThief = 0;

                    if (StealScript.ForcedHeheZidane) SoundLib.PlaySoundEffect(4005);
                }
            }
        }
        public void StealGils()
        {
            if (btl_util.getEnemyPtr(_v.Target).bonus_gil <= 0) return;

            var casterState = _v.CasterState();
            int bonusgil = 0;
            byte delay = btl_util.getSerialNumber(_v.Caster.Data) == CharacterSerialNumber.ZIDANE_SWORD ? (byte)8 : (byte)16;

            if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1023))
                bonusgil = (int)UnityEngine.Random.Range(btl_util.getEnemyPtr(_v.Target).bonus_gil / 12, btl_util.getEnemyPtr(_v.Target).bonus_gil / 6);
            else
                bonusgil = (int)(GameRandom.Next16() % (btl_util.getEnemyPtr(_v.Target).bonus_gil / 8));

            if (ff9item._FF9Item_Data[FF9StateSystem.Common.FF9.player[(CharacterId)_v.Caster.Data.bi.slot_no].equip.Weapon].shape == 1 && _v.Command.Data.info.effect_counter != 2)
            {
                casterState.Zidane.StealGil = bonusgil;
            }
            else
            {
                bonusgil += casterState.Zidane.StealGil;
                casterState.Zidane.StealGil = 0;

                var localizedMessage = new Dictionary<string, string> {
            { "US", $"+{bonusgil} gils!" }, { "UK", $"+{bonusgil} gils!" }, { "JP", $"+{bonusgil} ギル!" },
            { "ES", $"+{bonusgil} guiles!" }, { "FR", $"+{bonusgil} gils !" }, { "GR", $"+{bonusgil} Gil!" }, { "IT", $"+{bonusgil} Guil!" }
        };
                btl2d.Btl2dReqSymbolMessage(_v.Caster.Data, NGUIText.FF9YellowColor, localizedMessage, HUDMessage.MessageStyle.DAMAGE, delay);
                GameState.Gil += (uint)bonusgil;
            }
        }
    }
}
