using Assets.Sources.Scripts.UI.Common;
using Memoria.Data;
using Memoria.Prime;
using System.Runtime.Remoting.Contexts;

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
                if (_v.Caster.IsUnderAnyStatus(BattleStatus.Defend) && _v.Caster.SummonCount != 0 && _v.Target.PhysicalEvade != 255) // Duel Amarant - Prevent dodge
                {
                    _v.Context.Evade = 0;
                }
            }
            if (TranceSeekCustomAPI.TryPhysicalHit(_v))
            {
                TranceSeekCustomAPI.WeaponPhysicalParams(_bonus, _v);
                _v.Caster.PhysicalPenaltyAndBonusAttack();
                TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                if (_v.Caster.IsUnderStatus(BattleStatus.Trance) && _v.Caster.PlayerIndex == CharacterId.Steiner)
                {
                    _v.Context.Attack += _v.Context.Attack / 4;
                }
                TranceSeekCustomAPI.BonusBackstabAndPenaltyLongDistanceTranceSeek(_v);
                TranceSeekCustomAPI.BonusWeaponElement(_v);
                if (_v.CanAttackWeaponElementalCommand())
                {
                    if (_v.Caster.HasSupportAbility(SupportAbility2.Mug) && !_v.Target.IsPlayer)
                    {
                        _v.Context.HitRate = (short)(_v.Caster.Level + _v.Caster.Will);
                        _v.Context.Evade = _v.Target.Level;
                        if (GameRandom.Next16() % _v.Context.HitRate >= GameRandom.Next16() % _v.Context.Evade || _v.Caster.HasSupportAbility(SupportAbility2.Bandit))
                        {
                            BattleEnemy battleEnemy = BattleEnemy.Find(_v.Target);
                            if (battleEnemy.StealableItems[3] != RegularItem.NoItem && GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[3], _v.Caster.Weapon))
                            {
                                GameState.Thefts += 1;
                                BattleItem.AddToInventory(battleEnemy.StealableItems[3]);
                                UiState.SetBattleFollowFormatMessage(BattleMesages.Stole, new object[]
                                {
                                        FF9TextTool.ItemName(battleEnemy.StealableItems[3])
                                });
                                battleEnemy.StealableItems[3] = RegularItem.NoItem;
                            }
                            else if (battleEnemy.StealableItems[2] != RegularItem.NoItem && GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[2], _v.Caster.Weapon))
                            {
                                GameState.Thefts += 1;
                                BattleItem.AddToInventory(battleEnemy.StealableItems[2]);
                                UiState.SetBattleFollowFormatMessage(BattleMesages.Stole, new object[]
                                {
                                            FF9TextTool.ItemName(battleEnemy.StealableItems[2])
                                });
                                battleEnemy.StealableItems[2] = RegularItem.NoItem;
                            }
                            else if (battleEnemy.StealableItems[1] != RegularItem.NoItem && GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[1], _v.Caster.Weapon))
                            {
                                GameState.Thefts += 1;
                                BattleItem.AddToInventory(battleEnemy.StealableItems[1]);
                                UiState.SetBattleFollowFormatMessage(BattleMesages.Stole, new object[]
                                {
                                                FF9TextTool.ItemName(battleEnemy.StealableItems[1])
                                });
                                battleEnemy.StealableItems[1] = RegularItem.NoItem;
                            }
                            else if (battleEnemy.StealableItems[0] != RegularItem.NoItem)
                            {
                                GameState.Thefts += 1;
                                BattleItem.AddToInventory(battleEnemy.StealableItems[0]);
                                UiState.SetBattleFollowFormatMessage(BattleMesages.Stole, new object[]
                                {
                                                    FF9TextTool.ItemName(battleEnemy.StealableItems[0])
                                });
                                battleEnemy.StealableItems[0] = RegularItem.NoItem;
                            }
                        }                      
                    }
                    if (_v.Caster.PlayerIndex == CharacterId.Amarant)
                    {
                        TranceSeekCustomAPI.AmarantPassive(_v);
                        if (_v.Caster.IsUnderAnyStatus(BattleStatus.Defend) && _v.Caster.SummonCount != 0) // Duel Amarant
                        {
                            short criticalbonus = _v.Caster.Data.critical_rate_deal_bonus;
                            _v.Caster.Data.critical_rate_deal_bonus += _v.Caster.Will;
                            BattleStatus status = _v.Caster.WeaponStatus;
                            int statusrate = (50 + _v.Caster.WeaponRate + (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1025) ? (_v.Target.Will / 2) : 0));
                            if (!_v.Target.IsPlayer && status != 0)
                            {
                                if (((status & BattleStatus.Death) != 0 && !_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill)) || (status & BattleStatus.Death) == 0)
                                {
                                    if ((GameRandom.Next8() % 100) < statusrate)
                                        _v.Target.TryAlterStatuses(status, false, _v.Caster);
                                }
                            }
                            _v.Caster.SummonCount = 0;
                            TranceSeekCustomAPI.TryCriticalHit(_v);
                            _v.Caster.Data.critical_rate_deal_bonus = criticalbonus;
                        }
                    }
                    else
                    {
                        TranceSeekCustomAPI.TryCriticalHit(_v);
                        TranceSeekCustomAPI.TryApplyDragon(_v);
                    }
                    TranceSeekCustomAPI.IpsenCastleMalus(_v);
                    _v.CalcPhysicalHpDamage();
                    TranceSeekCustomAPI.InfusedWeaponStatus(_v);
                    TranceSeekCustomAPI.RaiseTrouble(_v);
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
            _v.Caster.PenaltyMini();
            _v.Caster.EnemyTranceBonusAttack();
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
    }
}
