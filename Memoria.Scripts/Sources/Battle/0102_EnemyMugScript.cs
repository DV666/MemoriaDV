using System;
using Assets.Sources.Scripts.UI.Common;
using Memoria.Data;

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
            TranceSeekAPI.CharacterBonusPassive(_v, "PhysicalAttack");
            TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
            TranceSeekAPI.EnemyTranceBonusAttack(_v);
            TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
            TranceSeekAPI.BonusBackstabAndPenaltyLongDistance(_v);
            TranceSeekAPI.InfusedWeaponStatus(_v);
            _v.CalcHpDamage();
            if ((_v.Caster.Data.dms_geo_id == 410 || _v.Caster.Data.dms_geo_id == 412) && !_v.Caster.IsPlayer) // Lamie et Bandit
            {              
                RegularItem itemId = (RegularItem)_v.Command.HitRate;
                if (_v.Command.HitRate == 227)
                    itemId = (RegularItem)1000; // Ultra Potion
                if (ff9item.FF9Item_GetCount(itemId) == 0)
                {
                    UiState.SetBattleFollowFormatMessage(BattleMesages.DoesNotHaveAnything);
                }
                else
                {
                    BattleEnemy battleEnemy = BattleEnemy.Find(_v.Caster);
                    battleEnemy.Data.steal_item[0] = itemId;
                    battleEnemy.Data.bonus_item[0] = itemId;
                    BattleItem.RemoveFromInventory(itemId);
                    UiState.SetBattleFollowFormatMessage(BattleMesages.WasStolen, FF9TextTool.ItemName(itemId));
                }
            }
            else
            {
                RemoveItem();
            }
            TranceSeekAPI.TryAlterMagicStatuses(_v);
        }

        private void RemoveItem()
        {
            RegularItem itemId = (RegularItem)_v.Command.HitRate;
            if (ff9item.FF9Item_GetCount(itemId) == 0)
            {
                UiState.SetBattleFollowFormatMessage(BattleMesages.DoesNotHaveAnything);
            }
            else
            {
                if (_v.Caster.Data.dms_geo_id == 265)
                {
                    BattleEnemy battleEnemy = BattleEnemy.Find(_v.Caster);
                    battleEnemy.Data.bonus_item[0] = itemId;
                }
                BattleItem.RemoveFromInventory(itemId);
                UiState.SetBattleFollowFormatMessage(BattleMesages.WasStolen, FF9TextTool.ItemName(itemId));
            }
        }
    }
}
