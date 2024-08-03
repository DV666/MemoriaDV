using Assets.Sources.Scripts.UI.Common;
using Memoria.Data;

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
            //TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            if (Configuration.TetraMaster.TripleTriad == 16388 && _v.Caster.IsPlayer)
            {
                if (!_v.Target.IsPlayer)
                {
                    _v.Target.Flags |= CalcFlag.HpAlteration;
                }
                else
                {
                    _v.Target.Flags |= (CalcFlag.HpDamageOrHeal);
                    _v.Target.Flags |= (CalcFlag.MpAlteration);
                }
                _v.Caster.Flags |= (CalcFlag.HpDamageOrHeal);
                _v.Target.HpDamage = 9999;
                _v.Target.MpDamage = 999;
                _v.Caster.HpDamage = _v.Target.HpDamage;
                _v.Caster.MagicDefence = 254;
                _v.Caster.PhysicalDefence = 254;
            }
            else if (Configuration.TetraMaster.TripleTriad == 16389 && _v.Caster.IsPlayer)
            {
                if (!_v.Target.IsPlayer)
                {
                    _v.Target.Flags |= CalcFlag.HpAlteration;
                }
                else
                {
                    _v.Target.Flags |= (CalcFlag.HpAlteration | CalcFlag.HpRecovery);
                }
                _v.Caster.Flags |= (CalcFlag.HpAlteration | CalcFlag.HpRecovery);
                _v.Target.HpDamage = (int)(_v.Target.CurrentHp - 1);
                _v.Caster.HpDamage = _v.Target.HpDamage;
                _v.Caster.MagicDefence = 254;
                _v.Caster.PhysicalDefence = 254;
                _v.Target.AlterStatus(TranceSeekCustomAPI.CustomStatus.PowerBreak, _v.Caster);
                _v.Target.AlterStatus(TranceSeekCustomAPI.CustomStatus.MagicBreak, _v.Caster);
                _v.Target.AlterStatus(TranceSeekCustomAPI.CustomStatus.ArmorBreak, _v.Caster);
                _v.Target.AlterStatus(TranceSeekCustomAPI.CustomStatus.MentalBreak, _v.Caster);
            }
            else
            {
                if (!_v.Target.TryKillFrozen())
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
                        _v.Caster.BonusWeaponElement();
                        if (_v.CanAttackWeaponElementalCommand())
                        {
                            if (_v.Caster.HasSupportAbility(SupportAbility2.Mug) && !_v.Target.IsPlayer)
                            {
                                _v.Context.HitRate = (short)(_v.Caster.Level + _v.Caster.Will);
                                _v.Context.Evade = _v.Target.Level;
                                if (GameRandom.Next16() % _v.Context.HitRate >= GameRandom.Next16() % _v.Context.Evade || _v.Caster.HasSupportAbility(SupportAbility2.Bandit))
                                {
                                    BattleEnemy battleEnemy = BattleEnemy.Find(_v.Target);
                                    if (battleEnemy.StealableItems[3] != RegularItem.NoItem && GameRandom.Next8() < 1)
                                    {
                                        GameState.Thefts += 1;
                                        BattleItem.AddToInventory(battleEnemy.StealableItems[3]);
                                        UiState.SetBattleFollowFormatMessage(BattleMesages.Stole, new object[]
                                        {
                                            FF9TextTool.ItemName(battleEnemy.StealableItems[3])
                                        });
                                        battleEnemy.StealableItems[3] = RegularItem.NoItem;
                                    }
                                    else
                                    {
                                        if (battleEnemy.StealableItems[2] != RegularItem.NoItem && GameRandom.Next8() < 16)
                                        {
                                            GameState.Thefts += 1;
                                            BattleItem.AddToInventory(battleEnemy.StealableItems[2]);
                                            UiState.SetBattleFollowFormatMessage(BattleMesages.Stole, new object[]
                                            {
                                                FF9TextTool.ItemName(battleEnemy.StealableItems[2])
                                            });
                                            battleEnemy.StealableItems[2] = RegularItem.NoItem;
                                        }
                                        else
                                        {
                                            if (battleEnemy.StealableItems[1] != RegularItem.NoItem && GameRandom.Next8() < 64)
                                            {
                                                GameState.Thefts += 1;
                                                BattleItem.AddToInventory(battleEnemy.StealableItems[1]);
                                                UiState.SetBattleFollowFormatMessage(BattleMesages.Stole, new object[]
                                                {
                                                    FF9TextTool.ItemName(battleEnemy.StealableItems[1])
                                                });
                                                battleEnemy.StealableItems[1] = RegularItem.NoItem;
                                            }
                                            else
                                            {
                                                if (battleEnemy.StealableItems[0] != RegularItem.NoItem)
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
                            }
                            TranceSeekCustomAPI.IpsenCastleMalus(_v);
                            _v.CalcPhysicalHpDamage();
                            TranceSeekCustomAPI.RaiseTrouble(_v);
                            //TranceSeekCustomAPI.SpecialSA(_v);
                        }
                    }
                }
            }
        }
    }
}
