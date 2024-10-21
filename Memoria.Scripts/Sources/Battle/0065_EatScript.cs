using System;
using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Assets;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Eat, Cook
    /// </summary>
    [BattleScript(Id)]
    public sealed class EatScript : IBattleScript, IEstimateBattleScript
    {
        public const Int32 Id = 0065;

        private readonly BattleCalculator _v;

        public EatScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            BattleEnemyPrototype enemyPrototype = BattleEnemyPrototype.Find(_v.Target);
            Int32 blueMagicId = enemyPrototype.BlueMagicId;
            if (_v.Caster.IsUnderAnyStatus(BattleStatus.Trance))
            {
                _v.WeaponPhysicalParams(CalcAttackBonus.Random);
                _v.BonusKillerAbilities();
                TranceSeekCustomAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                TranceSeekCustomAPI.BonusBackstabAndPenaltyLongDistance(_v);
                TranceSeekCustomAPI.BonusWeaponElement(_v);
                if (_v.CanAttackWeaponElementalCommand())
                {
                    TranceSeekCustomAPI.TryCriticalHit(_v);
                    TranceSeekCustomAPI.IpsenCastleMalus(_v);
                    _v.Target.Flags |= (CalcFlag.HpAlteration | CalcFlag.MpAlteration);
                    _v.Caster.Flags |= (CalcFlag.HpAlteration | CalcFlag.HpRecovery | CalcFlag.MpAlteration | CalcFlag.MpRecovery);
                    int hpDamage = Math.Max(1, _v.Context.PowerDifference * _v.Context.EnsureAttack);
                    int mpDamage = Math.Max(1, _v.Context.PowerDifference * _v.Context.EnsureAttack >> 4);
                    if (_v.Target.CurrentHp < hpDamage && !_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill) && !_v.Target.HasCategory(EnemyCategory.Humanoid))
                    {
                        hpDamage = (int)(_v.Target.CurrentHp - 1);
                    }
                    _v.Target.HpDamage = hpDamage;
                    _v.Target.MpDamage = mpDamage;
                    _v.Caster.HpDamage = hpDamage;
                    _v.Caster.MpDamage = mpDamage;
                }
            }
            if ((blueMagicId == 0 || !_v.Target.CanBeAttacked() || btl_util.getEnemyTypePtr(_v.Target.Data).category == 1))
            {
                if (!_v.Caster.IsUnderAnyStatus(BattleStatus.Trance))
                {
                    UiState.SetBattleFollowFormatMessage(BattleMesages.CannotEat, new object[0]);
                    _v.Context.Flags |= BattleCalcFlags.Miss;
                    return;
                }
            }
            else
            {
                if (_v.Target.CurrentHp <= _v.Target.MaximumHp / _v.Command.Power)
                {
                    Int32 BonusHealFork = 0;
                    switch (_v.Caster.Weapon)
                    {
                        case RegularItem.NeedleFork:
                            BonusHealFork += 25;
                            break;
                        case RegularItem.MythrilFork:
                            BonusHealFork += 50;
                            break;
                        case RegularItem.SilverFork:
                            BonusHealFork += 75;
                            break;
                        case RegularItem.BistroFork:
                            BonusHealFork += 100;
                            break;
                        case RegularItem.GastroFork:
                            BonusHealFork += 100;
                            break;
                    }
                    if (blueMagicId == 0 || ff9abil.FF9Abil_IsMaster(_v.Caster.Player, blueMagicId))
                    {
                        _v.Caster.Flags |= (CalcFlag.HpAlteration | CalcFlag.HpRecovery | CalcFlag.MpAlteration | CalcFlag.MpRecovery);
                        _v.Caster.HpDamage = (int)(_v.Caster.MaximumHp / (_v.Command.Power / 2));
                        _v.Caster.MpDamage = (int)(_v.Caster.MaximumMp / (_v.Command.Power / 2));
                        _v.Caster.HpDamage += (_v.Caster.HpDamage * BonusHealFork) / 100;                        
                        _v.Caster.MpDamage += (_v.Caster.MpDamage * BonusHealFork) / 100;
                        UiState.SetBattleFollowFormatMessage(BattleMesages.TasteBad);
                    }
                    else
                    {
                        _v.Caster.Flags |= (CalcFlag.HpAlteration | CalcFlag.HpRecovery | CalcFlag.MpAlteration | CalcFlag.MpRecovery);
                        _v.Caster.HpDamage = (int)(_v.Caster.MaximumHp / _v.Command.Power);
                        _v.Caster.MpDamage = (int)(_v.Caster.MaximumMp / _v.Command.Power);
                        _v.Caster.HpDamage += (_v.Caster.HpDamage * BonusHealFork) / 100;
                        _v.Caster.MpDamage += (_v.Caster.MpDamage * BonusHealFork) / 100;
                        ff9abil.FF9Abil_SetMaster(_v.Caster.Player, blueMagicId);
                        BattleState.RaiseAbilitiesAchievement(blueMagicId);
                        if (ff9abil.IsAbilityActive(blueMagicId))
                            UiState.SetBattleFollowFormatMessage(BattleMesages.Learned, FF9TextTool.ActionAbilityName(ff9abil.GetActiveAbilityFromAbilityId(blueMagicId)));
                        else
                            UiState.SetBattleFollowFormatMessage(BattleMesages.Learned, FF9TextTool.SupportAbilityName(ff9abil.GetSupportAbilityFromAbilityId(blueMagicId)));
                    }
                    _v.Target.Kill(_v.Caster);
                }
                else
                {
                    if (!_v.Caster.IsUnderAnyStatus(BattleStatus.Trance) && blueMagicId != 0)
                    {
                        if (_v.Target.CurrentHp <= _v.Target.MaximumHp / 4U)
                        {
                            if ((EmbadedTextResources.CurrentSymbol ?? Localization.GetSymbol()) == "FR")
                            {
                                UIManager.Battle.SetBattleMessage("Miam ! Encore quelques coups de fourchettes...", 3);
                            }
                            else
                            {
                                UIManager.Battle.SetBattleMessage("Yummy! A few more fork strokes...", 3);
                            }
                        }
                        else
                        {
                            if (_v.Target.CurrentHp > _v.Target.MaximumHp / 2U)
                            {
                                UiState.SetBattleFollowFormatMessage(BattleMesages.CannotEatStrong, new object[0]);
                            }
                            else
                            {
                                if ((EmbadedTextResources.CurrentSymbol ?? Localization.GetSymbol()) == "FR")
                                {
                                    UIManager.Battle.SetBattleMessage("Plus de la moitié du travail a été fait... Miam !", 3);
                                }
                                else
                                {
                                    UIManager.Battle.SetBattleMessage("More than half the work has been done... Yummy !", 3);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!_v.Target.HasCategory(EnemyCategory.Humanoid) && blueMagicId != 0)
                        {
                            if (_v.Target.CurrentHp <= _v.Target.MaximumHp / 4U)
                            {
                                if ((EmbadedTextResources.CurrentSymbol ?? Localization.GetSymbol()) == "FR")
                                {
                                    UIManager.Battle.SetBattleMessage("Miam ! Encore quelques coups de fourchettes...", 3);
                                }
                                else
                                {
                                    UIManager.Battle.SetBattleMessage("Yummy! A few more fork strokes...", 3);
                                }
                            }
                            else
                            {
                                if (_v.Target.CurrentHp <= _v.Target.MaximumHp / 2U)
                                {
                                    if ((EmbadedTextResources.CurrentSymbol ?? Localization.GetSymbol()) == "FR")
                                    {
                                        UIManager.Battle.SetBattleMessage("Plus de la moitié du travail a été fait... Miam !", 3);
                                    }
                                    else
                                    {
                                        UIManager.Battle.SetBattleMessage("More than half the work has been done... Yummy !", 3);
                                    }
                                }
                                else
                                {
                                    UiState.SetBattleFollowFormatMessage(BattleMesages.CannotEatStrong, new object[0]);
                                }
                            }
                        }
                    }
                }
            }
        }

        public Single RateTarget()
        {
            if (!_v.Target.CheckUnsafetyOrGuard() || !_v.Target.CanBeAttacked() || _v.Target.HasCategory(EnemyCategory.Humanoid))
                return 0;

            if (_v.Target.CurrentHp > _v.Target.MaximumHp / _v.Command.Power)
                return 0;

            BattleEnemyPrototype enemyPrototype = BattleEnemyPrototype.Find(_v.Target);
            Int32 blueMagicId = enemyPrototype.BlueMagicId;
            if (blueMagicId == 0 || ff9abil.FF9Abil_IsMaster(_v.Caster.Player, blueMagicId))
                return 0;

            return 1;
        }
    }
}
