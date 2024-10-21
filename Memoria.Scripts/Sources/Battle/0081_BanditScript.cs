using System;
using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Data;
using Memoria.Prime;

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
            if (_v.Command.AbilityId == (BattleAbilityId)163) // Larcin
            {
                if ((_v.Target.AbsorbElement & EffectElement.Darkness) != 0)
                {
                    _v.Context.Flags = BattleCalcFlags.Guard;
                    return;
                }

                BattleEnemy battleEnemy = BattleEnemy.Find(_v.Target);
                float BonusRatioHP = 0;
                if (TranceSeekCustomAPI.MonsterMechanic[_v.Target.Data][3] > 0)
                    BonusRatioHP = 100 - ((_v.Target.CurrentHp * 100) / (_v.Target.MaximumHp - 10000));
                else
                    BonusRatioHP = 100 - ((_v.Target.CurrentHp * 100)/ _v.Target.MaximumHp);

                if (GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[3], _v.Caster.Weapon) + (BonusRatioHP / 4) && battleEnemy.StealableItems[3] != RegularItem.NoItem)
                {
                    _v.StealItem(battleEnemy, 3);
                }
                else if (GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[2], _v.Caster.Weapon) + (BonusRatioHP / 2) && battleEnemy.StealableItems[2] != RegularItem.NoItem)
                {
                    _v.StealItem(battleEnemy, 2);
                }
                else if (GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[1], _v.Caster.Weapon) + BonusRatioHP && battleEnemy.StealableItems[1] != RegularItem.NoItem)
                {
                    _v.StealItem(battleEnemy, 1);
                }
                else if (GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[0], _v.Caster.Weapon) + BonusRatioHP && battleEnemy.StealableItems[0] != RegularItem.NoItem)
                {
                    _v.StealItem(battleEnemy, 0);
                }
                return;
            }
            if (_v.Caster.IsPlayer)
            {
                _v.WeaponPhysicalParams();
            }
            else
            {
                _v.NormalPhysicalParams();
            }
            TranceSeekCustomAPI.CharacterBonusPassive(_v, "PhysicalAttack");
            TranceSeekCustomAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
            TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
            TranceSeekCustomAPI.EnemyTranceBonusAttack(_v);
            TranceSeekCustomAPI.BonusElement(_v);
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
