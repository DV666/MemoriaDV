<<<<<<< HEAD
=======
using System;
using System.Threading;
>>>>>>> origin/TranceSeekCurrent
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
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            _v.NormalPhysicalParams();
            TranceSeekCustomAPI.CharacterBonusPassive(_v, "PhysicalAttack");
            _v.Caster.PhysicalPenaltyAndBonusAttack();
            _v.Caster.EnemyTranceBonusAttack();
            TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
            TranceSeekCustomAPI.BonusBackstabAndPenaltyLongDistanceTranceSeek(_v);
            _v.CalcHpDamage();
            if (_v.Caster.Data.dms_geo_id == 410 && !_v.Caster.IsPlayer) // Lamie
            {              
                RegularItem itemId = (RegularItem)_v.Command.HitRate;
                if (_v.Command.HitRate == 227)
                    itemId = (RegularItem)1000; // Ultra Poton
                if (ff9item.FF9Item_GetCount(itemId) == 0)
                {
                    UiState.SetBattleFollowFormatMessage(BattleMesages.DoesNotHaveAnything);
                }
                else
                {
                    BattleEnemy battleEnemy = BattleEnemy.Find(_v.Caster);
                    battleEnemy.Data.steal_item[0] = itemId;
                    BattleItem.RemoveFromInventory(itemId);
                    UiState.SetBattleFollowFormatMessage(BattleMesages.WasStolen, FF9TextTool.ItemName(itemId));
                }
            }
            else
            {
                RemoveItem();
            }
            TranceSeekCustomAPI.SpecialSA(_v);
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
