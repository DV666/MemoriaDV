<<<<<<< HEAD
using Memoria.Data;
using System;
=======
﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using FF9;
using Memoria.Data;
using static Memoria.Configuration;
using static SiliconStudio.Social.ResponseData;
>>>>>>> origin/TranceSeekCurrent

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

            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            TranceSeekCustomAPI.SpecialSA(_v);
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

                    if (_v.Caster.HasSupportAbility(SupportAbility1.StealGil) || _v.Caster.HasSupportAbilityByIndex((SupportAbility)1023))
                    {
                        StealGils();
                    }
                    if (_v.Caster.HasSupportAbility(SupportAbility1.MasterThief) || _v.Caster.HasSupportAbilityByIndex((SupportAbility)1022))
                    {
                        MasterThief();
                        return;
                    }
                    ClassicSteal();
                }
            }
<<<<<<< HEAD

=======
>>>>>>> origin/TranceSeekCurrent
        }

        private static Boolean HasStealableItems(BattleEnemy enemy)
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
                _v.StealItem(battleEnemy, 0);
            }
            else if (battleEnemy.StealableItems[1] != RegularItem.NoItem)
            {
                _v.StealItem(battleEnemy, 1);
            }
            else if (battleEnemy.StealableItems[2] != RegularItem.NoItem)
            {
                _v.StealItem(battleEnemy, 2);
            }
            else if (battleEnemy.StealableItems[3] != RegularItem.NoItem && GameRandom.Next8() < (127 + battleEnemy.StealableItemRates[3]))
            {
                _v.StealItem(battleEnemy, 3);
            }
            else
            {
                UiState.SetBattleFollowFormatMessage(BattleMesages.CouldNotStealAnything);
            }
        }

        public void MasterThief() 
        {
            short PreviousGameStateThefts = GameState.Thefts;
            BattleEnemy battleEnemy = BattleEnemy.Find(_v.Target);
            int MasterThiefTrigger = TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][3];
            if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1022) && MasterThiefTrigger == 0) // If steal failed, will work the second time.
                MasterThiefTrigger = 1;
            if ((battleEnemy.StealableItems[0] != RegularItem.NoItem) && (MasterThiefTrigger >= 2 || GameRandom.Next8() < battleEnemy.StealableItemRates[0]))
            {
                _v.StealItem(battleEnemy, 0);
                TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][3] = 0;
            }
            else if ((battleEnemy.StealableItems[1] != RegularItem.NoItem) && (MasterThiefTrigger >= 2 || GameRandom.Next8() < battleEnemy.StealableItemRates[1]))
            {
                _v.StealItem(battleEnemy, 1);
                TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][3] = 0;
            }
            else if ((battleEnemy.StealableItems[2] != RegularItem.NoItem) && (MasterThiefTrigger >= 2 || GameRandom.Next8() < battleEnemy.StealableItemRates[2]))
            {
                _v.StealItem(battleEnemy, 2);
                TranceSeekCustomAPI.ZidanePassive[_v.Caster.Data][3] = 0;
            }
            else if ((battleEnemy.StealableItems[3] != RegularItem.NoItem) && (MasterThiefTrigger >= 2 || GameRandom.Next8() < battleEnemy.StealableItemRates[3]))
            {
                _v.StealItem(battleEnemy, 3);
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
                    StealWhenTrance();
                else
                {
                    if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1062)) // Lupin+
                    {
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
                    UiState.SetBattleFollowFormatMessage(BattleMesages.CouldNotStealAnything);
                }             
            }
        }

        public void ClassicSteal()
        {
            // short PreviousGameStateThefts = GameState.Thefts;
            BattleEnemy battleEnemy = BattleEnemy.Find(_v.Target);
            if (GameRandom.Next8() < battleEnemy.StealableItemRates[3] && battleEnemy.StealableItems[3] != RegularItem.NoItem)
            {
                _v.StealItem(battleEnemy, 3);
            }
            else if (GameRandom.Next8() < battleEnemy.StealableItemRates[2] && battleEnemy.StealableItems[2] != RegularItem.NoItem)
            {
                _v.StealItem(battleEnemy, 2);
            }
            else if (GameRandom.Next8() < battleEnemy.StealableItemRates[1] && battleEnemy.StealableItems[1] != RegularItem.NoItem)
            {
                _v.StealItem(battleEnemy, 1);
            }
            else if (GameRandom.Next8() < battleEnemy.StealableItemRates[0] && battleEnemy.StealableItems[0] != RegularItem.NoItem)
            {
                _v.StealItem(battleEnemy, 0);
            }
            else if (TranceSeekCustomAPI.ZidanePassive[_v.Target.Data][2] > 0) // Oeil de voleur activé
            {  
                if (GameRandom.Next8() < battleEnemy.StealableItemRates[3] && battleEnemy.StealableItems[3] != RegularItem.NoItem)
                {
                    _v.StealItem(battleEnemy, 3);
                }
                else if (GameRandom.Next8() < battleEnemy.StealableItemRates[2] && battleEnemy.StealableItems[2] != RegularItem.NoItem)
                {
                    _v.StealItem(battleEnemy, 2);
                }
                else if (GameRandom.Next8() < battleEnemy.StealableItemRates[1] && battleEnemy.StealableItems[1] != RegularItem.NoItem)
                {
                    _v.StealItem(battleEnemy, 1);
                }
                else if (GameRandom.Next8() < battleEnemy.StealableItemRates[0] && battleEnemy.StealableItems[0] != RegularItem.NoItem)
                {
                    _v.StealItem(battleEnemy, 0);
                }
                else if (_v.Caster.IsUnderAnyStatus(BattleStatus.Trance) && _v.Caster.PlayerIndex == CharacterId.Zidane)
                {
                    StealWhenTrance();
                }
                else
                {
                    if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1062)) // Lupin+ (sous Oeil de voleur)
                    {
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
                    UiState.SetBattleFollowFormatMessage(BattleMesages.CouldNotStealAnything);
                }
            }
            else if (_v.Caster.IsUnderAnyStatus(BattleStatus.Trance) && _v.Caster.PlayerIndex == CharacterId.Zidane)
            {
                StealWhenTrance();
            }
            else
            {
                if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1062)) // Lupin+
                {
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
                UiState.SetBattleFollowFormatMessage(BattleMesages.CouldNotStealAnything);
            }
        }
    }
}
