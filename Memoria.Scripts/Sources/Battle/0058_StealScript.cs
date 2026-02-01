using Memoria.Data;
using System;
using System.Collections.Generic;
using FF9;
using static Memoria.Scripts.Battle.TranceSeekAPI;
using Assets.Sources.Scripts.UI.Common;
using Memoria.Prime;
using System.Runtime.Remoting.Contexts;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Steal, Mug
    /// </summary>
    [BattleScript(Id)]
    public sealed class StealScript : IBattleScript
    {
        public const Int32 Id = 0058;

        private readonly BattleCalculator _v;

        public StealScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            BattleEnemy battleEnemy = BattleEnemy.Find(_v.Target);
            if (!HasStealableItems(battleEnemy))
            {
                UiState.SetBattleFollowFormatMessage(BattleMesages.DoesNotHaveAnything, new object[0]);
            }
            else
            {
                if (_v.Target.IsUnderStatus(BattleStatus.Vanish) | _v.Target.PhysicalEvade == 255)
                {
                    _v.Context.Flags |= BattleCalcFlags.Miss;
                }
                else
                {
                    if (_v.Caster.IsUnderStatus(BattleStatus.Blind))
                    {
                        _v.Context.HitRate = (short)((_v.Caster.Level + _v.Caster.Will) / 2);
                        _v.Context.Evade = _v.Target.Level;
                        if (GameRandom.Next16() % _v.Context.HitRate < GameRandom.Next16() % _v.Context.Evade)
                        {
                            _v.Context.Flags |= BattleCalcFlags.Miss;
                            return;
                        }
                    }
                    if (!_v.Caster.HasSupportAbility(SupportAbility2.Bandit) && !_v.Caster.IsUnderAnyStatus(BattleStatus.Trance))
                    {
                        _v.Context.HitRate = (short)(_v.Caster.Level + _v.Caster.Will);
                        _v.Context.Evade = _v.Target.Level;
                        if (GameRandom.Next16() % _v.Context.HitRate < GameRandom.Next16() % _v.Context.Evade)
                        {
                            UiState.SetBattleFollowFormatMessage(BattleMesages.CouldNotStealAnything);
                            return;
                        }
                    }

                    if (_v.Caster.HasSupportAbility(SupportAbility1.StealGil))
                    {
                        StealGils();
                    }
                    if (_v.Caster.IsUnderAnyStatus(BattleStatus.Trance) && _v.Caster.PlayerIndex == CharacterId.Zidane)
                        StealWhenTrance();
                    else
                        ClassicSteal();
                }
            }
        }

        public static Boolean HasStealableItems(BattleEnemy enemy)
        {
            for (Int16 slot = 0; slot < 4; ++slot)
                if (enemy.StealableItems[slot] != RegularItem.NoItem)
                    return true;
            return false;
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

        public void StealWhenTrance()
        {
            BattleEnemy battleEnemy = BattleEnemy.Find(_v.Target);
            if (battleEnemy.StealableItems[0] != RegularItem.NoItem)
            {
                StealItem(battleEnemy, 0);
            }
            else if (battleEnemy.StealableItems[1] != RegularItem.NoItem)
            {
                StealItem(battleEnemy, 1);
            }
            else if (battleEnemy.StealableItems[2] != RegularItem.NoItem)
            {
                StealItem(battleEnemy, 2);
            }
            else if (battleEnemy.StealableItems[3] != RegularItem.NoItem && GameRandom.Next8() < (127 + battleEnemy.StealableItemRates[3]))
            {
                StealItem(battleEnemy, 3);
            }
            else if (ZidanePassive[_v.Target.Data][2] > 0) // Oeil de voleur activé
            {
                EyeOfThief();
            }
            else
            {
                AddBonusSteal();
                UiState.SetBattleFollowFormatMessage(BattleMesages.CouldNotStealAnything);
            }
        }

        public void ClassicSteal(Boolean ShowHUDMessage = true)
        {
            BattleEnemy battleEnemy = BattleEnemy.Find(_v.Target);
            if (GameRandom.Next8() < NewStealableItemRates(battleEnemy.StealableItemRates[3], _v.Caster) && battleEnemy.StealableItems[3] != RegularItem.NoItem)
            {
                StealItem(battleEnemy, 3, ShowHUDMessage);
            }
            else if (GameRandom.Next8() < NewStealableItemRates(battleEnemy.StealableItemRates[2], _v.Caster) && battleEnemy.StealableItems[2] != RegularItem.NoItem)
            {
                StealItem(battleEnemy, 2, ShowHUDMessage);
            }
            else if (GameRandom.Next8() < NewStealableItemRates(battleEnemy.StealableItemRates[1], _v.Caster) && battleEnemy.StealableItems[1] != RegularItem.NoItem)
            {
                StealItem(battleEnemy, 1, ShowHUDMessage);
            }
            else if (GameRandom.Next8() < NewStealableItemRates(battleEnemy.StealableItemRates[0], _v.Caster) && battleEnemy.StealableItems[0] != RegularItem.NoItem)
            {
                StealItem(battleEnemy, 0, ShowHUDMessage);
            }
            else if (ZidanePassive[_v.Target.Data][2] > 0) // Oeil de voleur activé
            {
                EyeOfThief();
            }
            else
            {
                if (_v.Caster.PlayerIndex == CharacterId.Zidane) // Zidane's passive (old Bandit+)
                    AddBonusSteal();

                if (ShowHUDMessage)
                    UiState.SetBattleFollowFormatMessage(BattleMesages.CouldNotStealAnything);
            }
        }

        public void EyeOfThief()
        {
            BattleEnemy battleEnemy = BattleEnemy.Find(_v.Target);
            AddBonusSteal();
            Dictionary<String, String> localizedMessage = new Dictionary<String, String>
            {
                { "US", "Thief's Eye!" },
                { "UK", "Thief's Eye!" },
                { "JP", "泥棒の目!" },
                { "ES", "Ojo del ladrón!" },
                { "FR", "Œil du voleur !" },
                { "GR", "Auge des Diebes!" },
                { "IT", "Occhio del ladro!" },
            };
            if (_v.Caster.IsUnderAnyStatus(BattleStatus.Trance) && _v.Caster.PlayerIndex == CharacterId.Zidane)
            {
                if (battleEnemy.StealableItems[0] != RegularItem.NoItem)
                {
                    StealItem(battleEnemy, 0);
                }
                else if (battleEnemy.StealableItems[1] != RegularItem.NoItem)
                {
                    StealItem(battleEnemy, 1);
                }
                else if (battleEnemy.StealableItems[2] != RegularItem.NoItem)
                {
                    StealItem(battleEnemy, 2);
                }
                else if (battleEnemy.StealableItems[3] != RegularItem.NoItem && GameRandom.Next8() < (127 + battleEnemy.StealableItemRates[3]))
                {
                    StealItem(battleEnemy, 3);
                }
                else
                {
                    AddBonusSteal();
                    UiState.SetBattleFollowFormatMessage(BattleMesages.CouldNotStealAnything);
                    return;
                }
                btl2d.Btl2dReqSymbolMessage(_v.Target.Data, "[FDEE00]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 5);
                SoundLib.PlaySoundEffect(4005); //se511116 - Héhé !
                return;
            }
            else if (GameRandom.Next8() < NewStealableItemRates(battleEnemy.StealableItemRates[3], _v.Caster) && battleEnemy.StealableItems[3] != RegularItem.NoItem)
            {
                StealItem(battleEnemy, 3);
            }
            else if (GameRandom.Next8() < NewStealableItemRates(battleEnemy.StealableItemRates[2], _v.Caster) && battleEnemy.StealableItems[2] != RegularItem.NoItem)
            {
                StealItem(battleEnemy, 2);
            }
            else if (GameRandom.Next8() < NewStealableItemRates(battleEnemy.StealableItemRates[1], _v.Caster) && battleEnemy.StealableItems[1] != RegularItem.NoItem)
            {
                StealItem(battleEnemy, 1);
            }
            else if (GameRandom.Next8() < NewStealableItemRates(battleEnemy.StealableItemRates[0], _v.Caster) && battleEnemy.StealableItems[0] != RegularItem.NoItem)
            {
                StealItem(battleEnemy, 0);
            }
            else
            {
                AddBonusSteal();
                UiState.SetBattleFollowFormatMessage(BattleMesages.CouldNotStealAnything);
                return;
            }
            btl2d.Btl2dReqSymbolMessage(_v.Target.Data, "[FDEE00]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 5);
            if (_v.Caster.PlayerIndex == CharacterId.Zidane)
                SoundLib.PlaySoundEffect(4005); //se511116 - Héhé !
        }

        public static float NewStealableItemRates(ushort StealableItemRates, BattleCaster Caster)
        {
            Int32 BonusWeaponSteal = 0;
            switch (Caster.Weapon)
            {
                case RegularItem.MageMasher:
                    BonusWeaponSteal += 10;
                    break;
                case RegularItem.MythrilDagger:
                    BonusWeaponSteal += 20;
                    break;
                case (RegularItem)1007: // Butterfly Sword
                    BonusWeaponSteal += 30;
                    break;
                case (RegularItem)1008: // The Ogre
                    BonusWeaponSteal += 35;
                    break;
                case RegularItem.Gladius:
                    BonusWeaponSteal += 45;
                    break;
                case (RegularItem)1009: // Exploda
                    BonusWeaponSteal += 40;
                    break;
                case (RegularItem)1010: // Rune Tooth
                    BonusWeaponSteal += 45;
                    break;
                case RegularItem.ZorlinShape:
                    BonusWeaponSteal += 50;
                    break;
                case (RegularItem)1011: // Angel Bless
                    BonusWeaponSteal += 55;
                    break;
                case (RegularItem)1012: // Sargatanas
                    BonusWeaponSteal += 60;
                    break;
                case (RegularItem)1013: // Masamune
                    BonusWeaponSteal += 70;
                    break;
                case RegularItem.Orichalcon:
                    BonusWeaponSteal += 80;
                    break;
                case (RegularItem)1014: // The Tower
                    BonusWeaponSteal += 90;
                    break;
                case (RegularItem)1015: // The Monarch
                    BonusWeaponSteal += 100;
                    break;
            }

            if (Caster.HasSupportAbilityByIndex((SupportAbility)200) && Caster.PlayerIndex == CharacterId.Zidane) // SA Knavery
                BonusWeaponSteal += ZidanePassive[Caster.Data][0];

            return (StealableItemRates + ((float)(StealableItemRates * BonusWeaponSteal) / 100));
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

        public void StealItem(BattleEnemy enemy, Int32 slot, Boolean ShowHUDMessage = true)
        {
            _v.Context.ItemSteal = enemy.StealableItems[slot];
            if (_v.Context.ItemSteal == RegularItem.NoItem)
            {
                if (ShowHUDMessage)
                    UiState.SetBattleFollowFormatMessage(BattleMesages.CouldNotStealAnything);
                return;
            }

            enemy.StealableItems[slot] = RegularItem.NoItem;
            GameState.Thefts++;

            foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(_v.Caster))
                saFeature.TriggerOnAbility(_v, "Steal", false);
            foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(_v.Target))
                saFeature.TriggerOnAbility(_v, "Steal", true);

            if (_v.Caster.HasSupportAbility(SupportAbility1.MasterThief) && slot == 0 || _v.Caster.HasSupportAbilityByIndex((SupportAbility)1022) && slot == 1)
            {
                ff9item.FF9Item_Add(_v.Context.ItemSteal, 2);
                UiState.SetBattleFollowFormatMessage(BattleMesages.Stole, FF9TextTool.ItemName(_v.Context.ItemSteal) + " X 2");
            }
            else
            {
                BattleItem.AddToInventory(_v.Context.ItemSteal);
                UiState.SetBattleFollowFormatMessage(BattleMesages.Stole, FF9TextTool.ItemName(_v.Context.ItemSteal));
            }
            TranceSeekAPI.PhantomHandSA(_v);
        }

        public static void ClassicSteal(BattleCalculator v, Boolean ShowHUDMessage = true)
        {
            BattleEnemy battleEnemy = BattleEnemy.Find(v.Target);
            if (GameRandom.Next8() < NewStealableItemRates(battleEnemy.StealableItemRates[3], v.Caster) && battleEnemy.StealableItems[3] != RegularItem.NoItem)
            {
                StealItem(v, battleEnemy, 3, ShowHUDMessage);
            }
            else if (GameRandom.Next8() < NewStealableItemRates(battleEnemy.StealableItemRates[2], v.Caster) && battleEnemy.StealableItems[2] != RegularItem.NoItem)
            {
                StealItem(v, battleEnemy, 2, ShowHUDMessage);
            }
            else if (GameRandom.Next8() < NewStealableItemRates(battleEnemy.StealableItemRates[1], v.Caster) && battleEnemy.StealableItems[1] != RegularItem.NoItem)
            {
                StealItem(v, battleEnemy, 1, ShowHUDMessage);
            }
            else if (GameRandom.Next8() < NewStealableItemRates(battleEnemy.StealableItemRates[0], v.Caster) && battleEnemy.StealableItems[0] != RegularItem.NoItem)
            {
                StealItem(v, battleEnemy, 0, ShowHUDMessage);
            }
            else
            {
                if (ShowHUDMessage)
                    UiState.SetBattleFollowFormatMessage(BattleMesages.CouldNotStealAnything);
            }
        }

        public static void StealItem(BattleCalculator v, BattleEnemy enemy, Int32 slot, Boolean ShowHUDMessage = true)
        {
            v.Context.ItemSteal = enemy.StealableItems[slot];
            if (v.Context.ItemSteal == RegularItem.NoItem)
            {
                if (ShowHUDMessage)
                    UiState.SetBattleFollowFormatMessage(BattleMesages.CouldNotStealAnything);
                return;
            }

            enemy.StealableItems[slot] = RegularItem.NoItem;
            GameState.Thefts++;

            foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(v.Caster))
                saFeature.TriggerOnAbility(v, "Steal", false);
            foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(v.Target))
                saFeature.TriggerOnAbility(v, "Steal", true);

            if (v.Caster.HasSupportAbility(SupportAbility1.MasterThief) && slot == 0 || v.Caster.HasSupportAbilityByIndex((SupportAbility)1022) && slot == 1)
            {
                ff9item.FF9Item_Add(v.Context.ItemSteal, 2);
                UiState.SetBattleFollowFormatMessage(BattleMesages.Stole, FF9TextTool.ItemName(v.Context.ItemSteal) + " X 2");
            }
            else
            {
                BattleItem.AddToInventory(v.Context.ItemSteal);
                UiState.SetBattleFollowFormatMessage(BattleMesages.Stole, FF9TextTool.ItemName(v.Context.ItemSteal));
            }
            TranceSeekAPI.PhantomHandSA(v);
        }
    }
}
