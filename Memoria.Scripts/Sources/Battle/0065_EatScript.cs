using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Assets;
using Memoria.Data;
using Memoria.Prime;
using Memoria.Test;

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
        private static Dictionary<string, string> multiLangMessage = new Dictionary<string, string>();

        public EatScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            BattleEnemyPrototype enemyPrototype = BattleEnemyPrototype.Find(_v.Target);
            Int32 blueMagicId = enemyPrototype.BlueMagicId;
            int PowerCMD = _v.Command.Power;
            if (_v.Caster.Head == (RegularItem)1249)
                PowerCMD = _v.Command.AbilityId == BattleAbilityId.Eat ? 4 : (_v.Command.AbilityId == BattleAbilityId.Cook ? 2 : PowerCMD);
            else if (_v.Caster.Head == (RegularItem)1258)
                PowerCMD = _v.Command.AbilityId == BattleAbilityId.Eat ? 2 : (_v.Command.AbilityId == BattleAbilityId.Cook ? 1 : PowerCMD);

            if (_v.Caster.InTrance || _v.Caster.HasSupportAbilityByIndex((SupportAbility)220) || _v.Caster.HasSupportAbilityByIndex((SupportAbility)221))
            {
                if (!_v.Caster.InTrance && _v.Caster.HasSupportAbilityByIndex((SupportAbility)220) && _v.Caster.HasSupportAbilityByIndex((SupportAbility)221)) // SA Appetite AND Gourmandise
                {
                    Int32 MixStrMag = (_v.Caster.Strength + _v.Caster.Magic) / 2;
                    Int32 baseDamage = Comn.random16() % (1 + (_v.Caster.Level + MixStrMag >> 3));
                    _v.Context.AttackPower = _v.Caster.GetWeaponPower(_v.Command);
                    _v.Context.DefensePower = (_v.Target.PhysicalDefence + _v.Target.MagicDefence) / 2;
                    _v.Context.Attack = Comn.random16() % MixStrMag + baseDamage;
                    TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                    TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                    TranceSeekAPI.BonusBackstabAndPenaltyLongDistance(_v);
                    TranceSeekAPI.PenaltyShellAttack(_v);
                }
                else if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)220) || _v.Caster.InTrance) // SA Appetite
                {
                    Int32 baseDamage = Comn.random16() % (1 + (_v.Caster.Level + _v.Caster.Strength >> 3));
                    _v.Context.AttackPower = _v.Caster.GetWeaponPower(_v.Command);
                    _v.Target.SetPhysicalDefense();
                    _v.Context.Attack = Comn.random16() % _v.Caster.Strength + baseDamage;
                    TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                    TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                    TranceSeekAPI.BonusBackstabAndPenaltyLongDistance(_v);
                }
                else if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)221)) // SA Gourmandise
                {
                    Int32 baseDamage = Comn.random16() % (1 + (_v.Caster.Level + _v.Caster.Magic >> 3));
                    _v.Context.AttackPower = _v.Caster.GetWeaponPower(_v.Command);
                    _v.Target.SetMagicDefense();
                    _v.Context.Attack = Comn.random16() % _v.Caster.Magic + baseDamage;
                    TranceSeekAPI.PenaltyShellAttack(_v);
                }

                _v.BonusKillerAbilities();
                TranceSeekAPI.BonusWeaponElement(_v);
                if (TranceSeekAPI.CanAttackWeaponElementalCommand(_v))
                {
                    TranceSeekAPI.TryCriticalHit(_v);
                    TranceSeekAPI.IpsenCastleMalus(_v);
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
                    if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)220))
                        _v.Caster.HpDamage /= 4;
                    if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1220))
                        _v.Caster.AlterStatus(TranceSeekStatus.PowerUp, _v.Caster);
                    if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1221))
                        _v.Caster.AlterStatus(TranceSeekStatus.MagicUp, _v.Caster);
                }
            }

            if (_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill) || !_v.Target.CanBeAttacked() || btl_util.getEnemyTypePtr(_v.Target.Data).category == 1)
            {
                if (!_v.Caster.IsUnderAnyStatus(BattleStatus.Trance))
                {
                    UiState.SetBattleFollowFormatMessage(BattleMesages.CannotEat);
                    _v.Context.Flags |= BattleCalcFlags.Guard;
                    return;
                }
            }
            else
            {
                if (_v.Target.CurrentHp <= _v.Target.MaximumHp / PowerCMD)
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
                    if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)223)) // Voracious
                    {
                        Int32 HPDigest = (int)(_v.Caster.MaximumHp / (_v.Command.Power / 2));
                        Int32 MPDigest = (int)(_v.Caster.MaximumMp / (_v.Command.Power / 2));
                        if (!FF9StateSystem.EventState.gScriptDictionary.TryGetValue(1035, out Dictionary<Int32, Int32> VoraciousDict))
                        {
                            VoraciousDict = new Dictionary<Int32, Int32>();
                            VoraciousDict[0] = (int)Math.Min(HPDigest, _v.Caster.MaximumHp);
                            VoraciousDict[1] = (int)Math.Min(MPDigest, _v.Caster.MaximumMp);
                            FF9StateSystem.EventState.gScriptDictionary.Add(1035, VoraciousDict);
                        }
                        else
                        {
                            VoraciousDict[0] = (int)Math.Min(VoraciousDict[0] + HPDigest, _v.Caster.MaximumHp);
                            VoraciousDict[1] = (int)Math.Min(VoraciousDict[1] + MPDigest, _v.Caster.MaximumMp);
                        }
                    }
                    else
                    {
                        _v.Caster.Flags |= (CalcFlag.HpAlteration | CalcFlag.HpRecovery | CalcFlag.MpAlteration | CalcFlag.MpRecovery);
                        if (blueMagicId == 0 || ff9abil.FF9Abil_IsMaster(_v.Caster.Player, blueMagicId))
                        {
                            _v.Caster.HpDamage = (int)(_v.Caster.MaximumHp / (PowerCMD / 2));
                            _v.Caster.MpDamage = (int)(_v.Caster.MaximumMp / (PowerCMD / 2));
                        }
                        else
                        {
                            _v.Caster.HpDamage = (int)(_v.Caster.MaximumHp / PowerCMD);
                            _v.Caster.MpDamage = (int)(_v.Caster.MaximumMp / PowerCMD);
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
                    if (!_v.Caster.IsUnderAnyStatus(BattleStatus.Trance))
                    {
                        if (_v.Target.CurrentHp <= _v.Target.MaximumHp / PowerCMD)
                            UIManager.Battle.SetBattleFollowMessage(3, Localization.GetWithDefault("Eat25"));
                        else
                            if (_v.Target.CurrentHp <= _v.Target.MaximumHp / PowerCMD)
                                UIManager.Battle.SetBattleFollowMessage(3, Localization.GetWithDefault("Eat50"));
                            else
                                UiState.SetBattleFollowFormatMessage(BattleMesages.CannotEatStrong);
                    } 
                    else
                    {
                        if (!_v.Target.HasCategory(EnemyCategory.Humanoid) && blueMagicId != 0)
                        {
                            if (_v.Target.CurrentHp <= _v.Target.MaximumHp / PowerCMD)
                                UIManager.Battle.SetBattleFollowMessage(3, Localization.GetWithDefault("Eat25"));
                            else
                                if (_v.Target.CurrentHp <= _v.Target.MaximumHp / PowerCMD)
                                    UIManager.Battle.SetBattleFollowMessage(3, Localization.GetWithDefault("Eat50"));
                                else
                                    UiState.SetBattleFollowFormatMessage(BattleMesages.CannotEatStrong);
                        }
                    }
                }
            }
        }

        public Single RateTarget()
        {
            if (!_v.Target.CheckUnsafetyOrMiss() || !_v.Target.CanBeAttacked() || _v.Target.HasCategory(EnemyCategory.Humanoid))
                return 0;

            int PowerCMD = _v.Command.Power;
            if (_v.Caster.Head == TranceSeekRegularItem.ChefHat)
                PowerCMD = _v.Command.AbilityId == BattleAbilityId.Eat ? 4 : (_v.Command.AbilityId == BattleAbilityId.Cook ? 2 : PowerCMD);
            else if (_v.Caster.Head == TranceSeekRegularItem.StarredChefHat)
                PowerCMD = _v.Command.AbilityId == BattleAbilityId.Eat ? 2 : (_v.Command.AbilityId == BattleAbilityId.Cook ? 1 : PowerCMD);

            if (_v.Target.CurrentHp > _v.Target.MaximumHp / PowerCMD)
                return 0;

            BattleEnemyPrototype enemyPrototype = BattleEnemyPrototype.Find(_v.Target);
            Int32 blueMagicId = enemyPrototype.BlueMagicId;
            if (blueMagicId == 0 || ff9abil.FF9Abil_IsMaster(_v.Caster.Player, blueMagicId))
                return 0;

            return 1;
        }
    }
}
