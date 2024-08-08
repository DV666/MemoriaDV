using System;
using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Dark Matter
    /// </summary>
    [BattleScript(Id)]
    public sealed class BanditScript : IBattleScript
    {
        public const Int32 Id = 0081;

        private readonly BattleCalculator _v;

        public BanditScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Caster.IsPlayer)
            {
                _v.WeaponPhysicalParams();
            }
            else
            {
                _v.NormalPhysicalParams();
            }
            TranceSeekCustomAPI.CharacterBonusPassive(_v, "PhysicalAttack");
            _v.Caster.PhysicalPenaltyAndBonusAttack();
            TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
            _v.Caster.EnemyTranceBonusAttack();
            _v.BonusElement();
            if (_v.CanAttackElementalCommand())
            {
                BattleEnemy battleEnemy = BattleEnemy.Find(_v.Target);
                for (Int32 i = 0; i < battleEnemy.StealableItems.Length; i++)
                {
                    if (battleEnemy.StealableItems[i] != RegularItem.NoItem)
                        ++_v.Context.DamageModifierCount;
                }
                if (GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[3], _v.Caster.Weapon) && battleEnemy.StealableItems[3] != RegularItem.NoItem)
                {
                    _v.StealItem(battleEnemy, 3);
                }
                else if (GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[2], _v.Caster.Weapon) && battleEnemy.StealableItems[2] != RegularItem.NoItem)
                {
                    _v.StealItem(battleEnemy, 2);
                }
                else if (GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[1], _v.Caster.Weapon) && battleEnemy.StealableItems[1] != RegularItem.NoItem)
                {
                    _v.StealItem(battleEnemy, 1);
                }
                else if (GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[0], _v.Caster.Weapon) && battleEnemy.StealableItems[0] != RegularItem.NoItem)
                {
                    _v.StealItem(battleEnemy, 0);
                }
                _v.CalcHpDamage();
                _v.TryAlterMagicStatuses();
            }
        }
    }
}
