using Assets.Sources.Scripts.UI.Common;
using Memoria.Data;
using System;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Mug (enemy)
    /// </summary>
    [BattleScript(Id)]
    public sealed class EnemyMugScript : IBattleScript
    {
        public const Int32 Id = 0102;

        private readonly BattleCalculator _v;

        public EnemyMugScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            _v.NormalPhysicalParams();
            _v.Caster.EnemyTranceBonusAttack();
            _v.Caster.PhysicalPenaltyAndBonusAttack();
            _v.Target.PhysicalPenaltyAndBonusAttack();
            _v.BonusBackstabAndPenaltyLongDistance();
            _v.CalcHpDamage();
            RemoveItem();
        }

        private void RemoveItem()
        {
            RegularItem itemId = (RegularItem)_v.Command.HitRate;

            if (_v.Caster.IsPlayer)
            {
                BattleEnemy enemy = BattleEnemy.Find(_v.Target);
                if (enemy.StealableItems[0] != RegularItem.NoItem && enemy.StealableItems[0] == itemId)
                    _v.StealItem(enemy, 0);
                else if (enemy.StealableItems[1] != RegularItem.NoItem && enemy.StealableItems[1] == itemId)
                    _v.StealItem(enemy, 1);
                else if (enemy.StealableItems[2] != RegularItem.NoItem && enemy.StealableItems[2] == itemId)
                    _v.StealItem(enemy, 2);
                else if (enemy.StealableItems[3] != RegularItem.NoItem && enemy.StealableItems[3] == itemId)
                    _v.StealItem(enemy, 3);
                else if (!HasStealableItems(enemy))
                    UiState.SetBattleFollowFormatMessage(BattleMesages.DoesNotHaveAnything);
                else
                    UiState.SetBattleFollowFormatMessage(BattleMesages.CouldNotStealAnything);
            }
            else
            {
                if (ff9item.FF9Item_GetCount(itemId) == 0)
                {
                    UiState.SetBattleFollowFormatMessage(BattleMesages.CouldNotStealAnything);
                }
                else
                {
                    BattleItem.RemoveFromInventory(itemId);
                    UiState.SetBattleFollowFormatMessage(BattleMesages.WasStolen, FF9TextTool.ItemName(itemId));
                }
            }
        }

        private static Boolean HasStealableItems(BattleEnemy enemy)
        {
            for (Int16 slot = 0; slot < 4; ++slot)
                if (enemy.StealableItems[slot] != RegularItem.NoItem)
                    return true;
            return false;
        }
    }
}
