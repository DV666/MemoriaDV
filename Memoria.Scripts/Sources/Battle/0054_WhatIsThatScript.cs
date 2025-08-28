using Assets.Sources.Scripts.UI.Common;
using System;
using Memoria.Data;
using System.Collections.Generic;
using System.Linq;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// What’s That!?
    /// </summary>
    [BattleScript(Id)]
    public sealed class WhatIsThatScript : IBattleScript
    {
        public const Int32 Id = 0054;

        private readonly BattleCalculator _v;

        public WhatIsThatScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            BattleUnit boss = FF9StateSystem.Battle.FF9Battle.EnumerateBattleUnits().FirstOrDefault(u => (u.CurrentStatus & BattleStatus.EasyKill) != 0);
            if (_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill))
            {
                _v.Context.Flags |= BattleCalcFlags.Miss;
                return;
            }

            if (_v.Target.Data == _v.Caster.Data && boss == null)
                MultipleSteal(_v, false);

            if (boss == null)
                _v.Target.FaceAsUnit(_v.Caster);

            _v.Target.ChangeRowToDefault();
        }

        public static void MultipleSteal(BattleCalculator v, Boolean AllEnnemy = false)
        {
            List<RegularItem> ItemStolen = new List<RegularItem>();
            foreach (BattleUnit monster in BattleState.EnumerateUnits())
                if (!monster.IsPlayer && (monster.IsTargetable || AllEnnemy))
                {
                    BattleEnemy battleEnemy = BattleEnemy.Find(monster);
                    if (HasStealableItems(battleEnemy))
                        ItemStolen.Add(ClassicSteal(v, monster, v.Caster));
                }

            if (ItemStolen.Count > 0)
            {
                string steal_text = "";
                foreach (RegularItem item in ItemStolen)
                {
                    if (item == RegularItem.NoItem)
                        continue;

                    if (String.IsNullOrEmpty(steal_text))
                        steal_text += FF9TextTool.ItemName(item);
                    else
                        steal_text += " / " + FF9TextTool.ItemName(item);
                }
                if (!String.IsNullOrEmpty(steal_text))
                    UiState.SetBattleFollowFormatMessage(BattleMesages.Stole, steal_text);
            }
        }

        private static bool HasStealableItems(BattleEnemy enemy)
        {
            bool result = false;
            for (short num = 0; num < 4; num += 1)
            {
                bool flag = enemy.StealableItems[num] != Data.RegularItem.NoItem;
                if (flag)
                {
                    result = true;
                }
            }
            return result;
        }

        public static RegularItem ClassicSteal(BattleCalculator v, BattleUnit monster, BattleCaster caster)
        {
            RegularItem ItemStolen = RegularItem.NoItem;
            BattleEnemy battleEnemy = BattleEnemy.Find(monster);
            if (GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[3], caster) && battleEnemy.StealableItems[3] != RegularItem.NoItem)
            {
                ItemStolen = battleEnemy.StealableItems[3];
                StealScript.StealItem(v, battleEnemy, 3, false);
            }
            else if (GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[2], caster) && battleEnemy.StealableItems[2] != RegularItem.NoItem)
            {
                ItemStolen = battleEnemy.StealableItems[2];
                StealScript.StealItem(v, battleEnemy, 2, false);
            }
            else if (GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[1], caster) && battleEnemy.StealableItems[1] != RegularItem.NoItem)
            {
                ItemStolen = battleEnemy.StealableItems[1];
                StealScript.StealItem(v, battleEnemy, 1, false);
            }
            else if (GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[0], caster) && battleEnemy.StealableItems[0] != RegularItem.NoItem)
            {
                ItemStolen = battleEnemy.StealableItems[0];
                StealScript.StealItem(v, battleEnemy, 0, false);
            }
            else if (TranceSeekAPI.ZidanePassive[monster.Data][2] > 0 && caster.PlayerIndex == CharacterId.Zidane) // Oeil de voleur activé
            {
                if (caster.PlayerIndex == CharacterId.Zidane)
                    AddBonusSteal(monster);

                Dictionary<String, String> ThiefEyeMessage = new Dictionary<String, String>
                {
                    { "US", "Thief's Eye!" },
                    { "UK", "Thief's Eye!" },
                    { "JP", "泥棒の目!" },
                    { "ES", "Ojo del ladrón!" },
                    { "FR", "Œil du voleur !" },
                    { "GR", "Auge des Diebes!" },
                    { "IT", "Occhio del ladro!" },
                };
                if (GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[3], caster) && battleEnemy.StealableItems[3] != RegularItem.NoItem)
                {
                    ItemStolen = battleEnemy.StealableItems[3];
                    btl2d.Btl2dReqSymbolMessage(monster.Data, "[FDEE00]", ThiefEyeMessage, HUDMessage.MessageStyle.DAMAGE, 5);
                    StealScript.StealItem(v, battleEnemy, 3, false);
                }
                else if (GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[2], caster) && battleEnemy.StealableItems[2] != RegularItem.NoItem)
                {
                    ItemStolen = battleEnemy.StealableItems[2];
                    btl2d.Btl2dReqSymbolMessage(monster.Data, "[FDEE00]", ThiefEyeMessage, HUDMessage.MessageStyle.DAMAGE, 5);
                    StealScript.StealItem(v, battleEnemy, 2, false);
                }
                else if (GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[1], caster) && battleEnemy.StealableItems[1] != RegularItem.NoItem)
                {
                    ItemStolen = battleEnemy.StealableItems[1];
                    btl2d.Btl2dReqSymbolMessage(monster.Data, "[FDEE00]", ThiefEyeMessage, HUDMessage.MessageStyle.DAMAGE, 5);
                    StealScript.StealItem(v, battleEnemy, 1, false);
                }
                else if (GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[0], caster) && battleEnemy.StealableItems[0] != RegularItem.NoItem)
                {
                    ItemStolen = battleEnemy.StealableItems[0];
                    btl2d.Btl2dReqSymbolMessage(monster.Data, "[FDEE00]", ThiefEyeMessage, HUDMessage.MessageStyle.DAMAGE, 5);
                    StealScript.StealItem(v, battleEnemy, 0, false);
                }
            }
            return ItemStolen;
        }

        public static void AddBonusSteal(BattleUnit monster)
        {
            BattleEnemy battleEnemy = BattleEnemy.Find(monster);
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
    }
}
