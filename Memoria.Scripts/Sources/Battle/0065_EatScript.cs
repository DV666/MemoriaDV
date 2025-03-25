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
            if (_v.Caster.IsUnderAnyStatus(BattleStatus.Trance) || _v.Caster.HasSupportAbilityByIndex((SupportAbility)220) || _v.Caster.HasSupportAbilityByIndex((SupportAbility)221))
            {
                if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)221)) // SA Gourmandise
                {
                    Int32 baseDamage = Comn.random16() % (1 + (_v.Caster.Level + _v.Caster.Magic >> 3));
                    _v.Context.AttackPower = _v.Caster.GetWeaponPower(_v.Command);
                    _v.Target.SetMagicDefense();
                    _v.Context.Attack = Comn.random16() % _v.Caster.Magic + baseDamage;
                }
                else
                {
                    Int32 baseDamage = Comn.random16() % (1 + (_v.Caster.Level + _v.Caster.Strength >> 3));
                    _v.Context.AttackPower = _v.Caster.GetWeaponPower(_v.Command);
                    _v.Target.SetPhysicalDefense();
                    _v.Context.Attack = Comn.random16() % _v.Caster.Strength + baseDamage;
                }
                _v.BonusKillerAbilities();
                TranceSeekCustomAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                TranceSeekCustomAPI.BonusBackstabAndPenaltyLongDistance(_v);
                TranceSeekCustomAPI.BonusWeaponElement(_v);
                if (TranceSeekCustomAPI.CanAttackWeaponElementalCommand(_v))
                {
                    TranceSeekCustomAPI.TryCriticalHit(_v);
                    TranceSeekCustomAPI.IpsenCastleMalus(_v);
                    if (_v.Caster.IsUnderAnyStatus(BattleStatus.Trance))
                    {
                        _v.Target.Flags |= (CalcFlag.HpAlteration | CalcFlag.MpAlteration);
                        _v.Caster.Flags |= (CalcFlag.HpDamageOrHeal | CalcFlag.MpDamageOrHeal);
                    }
                    else if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)220))
                    {
                        _v.Target.Flags |= CalcFlag.HpAlteration;
                        _v.Caster.Flags |= CalcFlag.HpDamageOrHeal;
                    }
                    else
                    {
                        _v.Target.Flags |= CalcFlag.HpAlteration;
                    }
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
                    if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1220))
                        _v.Caster.AlterStatus(TranceSeekCustomAPI.CustomStatus.PowerUp, _v.Caster);
                    if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1221))
                        _v.Caster.AlterStatus(TranceSeekCustomAPI.CustomStatus.MagicUp, _v.Caster);
                }
            }
            if ((blueMagicId == 0 || !_v.Target.CanBeAttacked() || btl_util.getEnemyTypePtr(_v.Target.Data).category == 1)) // TODO - Remplacer BlueMagicId == 0
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
                    if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)223))
                    {
                        Int32 HPDigest = (int)(_v.Caster.MaximumHp / (_v.Command.Power / 2));
                        Int32 MPDigest = (int)(_v.Caster.MaximumMp / (_v.Command.Power / 2));
                        HPDigest += (FF9StateSystem.EventState.gEventGlobal[1320] * 256) + FF9StateSystem.EventState.gEventGlobal[1321];
                        MPDigest += (FF9StateSystem.EventState.gEventGlobal[1322] * 256) + FF9StateSystem.EventState.gEventGlobal[1323];
                        FF9StateSystem.EventState.gEventGlobal[1320] = (byte)(HPDigest / 256);
                        FF9StateSystem.EventState.gEventGlobal[1321] = (byte)(HPDigest % 256);
                        FF9StateSystem.EventState.gEventGlobal[1322] = (byte)(MPDigest / 256);
                        FF9StateSystem.EventState.gEventGlobal[1323] = (byte)(MPDigest % 256);
                    }
                    else
                    {
                        _v.Caster.Flags |= (CalcFlag.HpAlteration | CalcFlag.HpRecovery | CalcFlag.MpAlteration | CalcFlag.MpRecovery);
                        if (blueMagicId == 0 || ff9abil.FF9Abil_IsMaster(_v.Caster.Player, blueMagicId))
                        {
                            _v.Caster.HpDamage = (int)(_v.Caster.MaximumHp / (_v.Command.Power / 2));
                            _v.Caster.MpDamage = (int)(_v.Caster.MaximumMp / (_v.Command.Power / 2));
                        }
                        else
                        {
                            _v.Caster.HpDamage = (int)(_v.Caster.MaximumHp / _v.Command.Power);
                            _v.Caster.MpDamage = (int)(_v.Caster.MaximumMp / _v.Command.Power);
                        }
                        _v.Caster.HpDamage += (_v.Caster.HpDamage * BonusHealFork) / 100;
                        _v.Caster.MpDamage += (_v.Caster.MpDamage * BonusHealFork) / 100;
                    }
                    if (blueMagicId == 0 || ff9abil.FF9Abil_IsMaster(_v.Caster.Player, blueMagicId))
                    {

                        UiState.SetBattleFollowFormatMessage(BattleMesages.TasteBad);
                    }
                    else
                    {
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
                                    UIManager.Battle.SetBattleMessage("Plus de la moiti� du travail a �t� fait... Miam !", 3);
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
                                        UIManager.Battle.SetBattleMessage("Plus de la moiti� du travail a �t� fait... Miam !", 3);
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
