using System;
using System.Collections.Generic;
using FF9;
using Memoria.Data;
using UnityEngine;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Special
    /// </summary>
    [BattleScript(Id)]
    public sealed class PassiveEikoScript : IBattleScript
    {
        public const Int32 Id = 0121;

        private readonly BattleCalculator _v;

        public PassiveEikoScript(BattleCalculator v)
        {
            _v = v;
        }

        public static Dictionary<BTL_DATA, Int32> NumberTargets = new Dictionary<BTL_DATA, Int32>();

        public void Perform()
        {
            if (_v.Caster.PlayerIndex == CharacterId.Eiko)
            {
                if (!NumberTargets.TryGetValue(_v.Caster.Data, out Int32 Targets))
                    NumberTargets[_v.Caster.Data] = 0;

                if (TranceSeekAPI.StateMoug[_v.Caster.Data] == 1 && TranceSeekAPI.ModelMoug[_v.Caster.Data] == null) // Moug appears.
                {
                    TranceSeekAPI.ModelMoug[_v.Caster.Data] = ModelFactory.CreateModel("GEO_NPC_F4_MOG", true);
                    // ANH_NPC_F4_MOG_INTO_EIK_PASSIVE_1
                    // ANH_NPC_F4_MOG_INTO_EIK_PASSIVE_2
                    if (FF9StateSystem.EventState.ScenarioCounter > 9990) // Moug has a "phantom" effect after Mont Gulug.
                        btl_util.GeoSetABR(TranceSeekAPI.ModelMoug[_v.Caster.Data], "GEO_POLYFLAGS_TRANS_100_PLUS_25");
                    TranceSeekAPI.ModelMoug[_v.Caster.Data].transform.localPosition = _v.Caster.Data.gameObject.transform.localPosition;
                    TranceSeekAPI.ModelMoug[_v.Caster.Data].transform.localRotation = _v.Caster.Data.gameObject.transform.localRotation;
                    TranceSeekAPI.ModelMoug[_v.Caster.Data].transform.localScale = _v.Caster.Data.gameObject.transform.localScale;
                    TranceSeekAPI.StateMoug[_v.Caster.Data] = 2;
                    TranceSeekAPI.ModelMoug[_v.Caster.Data].SetActive(true);
                    Animation animation = TranceSeekAPI.ModelMoug[_v.Caster.Data].GetComponent<Animation>();
                    if (animation.GetClip("ANH_NPC_F4_MOG_INTO_EIK_PASSIVE_1") == null)
                        AnimationFactory.AddAnimWithAnimatioName(TranceSeekAPI.ModelMoug[_v.Caster.Data], "ANH_NPC_F4_MOG_INTO_EIK_PASSIVE_1");
                    if (animation.GetClip("ANH_NPC_F4_MOG_INTO_EIK_PASSIVE_2") == null)
                        AnimationFactory.AddAnimWithAnimatioName(TranceSeekAPI.ModelMoug[_v.Caster.Data], "ANH_NPC_F4_MOG_INTO_EIK_PASSIVE_2");
                    if (animation != null)
                    {
                        animation.Play("ANH_NPC_F4_MOG_INTO_EIK_PASSIVE_1");
                        if (animation["ANH_NPC_F4_MOG_INTO_EIK_PASSIVE_1"] != null)
                            animation["ANH_NPC_F4_MOG_INTO_EIK_PASSIVE_1"].speed = 1f;
                    }
                    return;
                }
                else if (TranceSeekAPI.StateMoug[_v.Caster.Data] == 2 && TranceSeekAPI.ModelMoug[_v.Caster.Data] != null) // Moug cast.
                {
                    TranceSeekAPI.ModelMoug[_v.Caster.Data].transform.localPosition = _v.Caster.Data.gameObject.transform.localPosition + new Vector3(0f, 0f, 250f);
                    Animation animation = TranceSeekAPI.ModelMoug[_v.Caster.Data].GetComponent<Animation>();
                    if (animation != null)
                    {
                        animation.Play("ANH_NPC_F4_MOG_INTO_EIK_2");
                        if (animation["ANH_NPC_F4_MOG_INTO_EIK_2"] != null)
                            animation["ANH_NPC_F4_MOG_INTO_EIK_2"].speed = 1f;
                    }
                    TranceSeekAPI.StateMoug[_v.Caster.Data] = 3;
                    NumberTargets[_v.Caster.Data] = _v.Command.TargetCount;
                    return;
                }
                else if (TranceSeekAPI.StateMoug[_v.Caster.Data] == 3 && TranceSeekAPI.ModelMoug[_v.Caster.Data] != null) // Moug cast
                {
                    Animation animation = TranceSeekAPI.ModelMoug[_v.Caster.Data].GetComponent<Animation>();
                    if (animation != null)
                    {
                        animation.Play("ANH_NPC_F4_MOG_IDLE");
                        if (animation["ANH_NPC_F4_MOG_IDLE"] != null)
                            animation["ANH_NPC_F4_MOG_IDLE"].speed = 1f;
                    }
                    switch (_v.Command.AbilityId)
                    {
                        case (BattleAbilityId)2000: // Mog Cure
                        case (BattleAbilityId)2011: // Moga Cure
                            _v.Target.Flags = (CalcFlag.HpAlteration);
                            if (!_v.Target.IsZombie)
                                _v.Target.Flags |= CalcFlag.HpRecovery;
                            _v.Target.HpDamage = (int)(_v.Target.MaximumHp / _v.Command.Power);
                        break;
                        case (BattleAbilityId)2001: // Mog Hug
                        case (BattleAbilityId)2012: // Moga Hug
                            _v.Target.Flags = (CalcFlag.MpAlteration);
                            if (!_v.Target.IsZombie)
                                _v.Target.Flags |= CalcFlag.MpRecovery;
                            _v.Target.MpDamage = (int)(_v.Target.MaximumMp / _v.Command.Power);
                            break;
                        case (BattleAbilityId)2002: // Mog Regen
                        case (BattleAbilityId)2004: // Mog Mirror
                        case (BattleAbilityId)2005: // Mog AutoLife
                        case (BattleAbilityId)2013: // Moga Regen
                        case (BattleAbilityId)2015: // Moga Mirror
                        case (BattleAbilityId)2016: // Moga AutoLife
                            TranceSeekAPI.TryAlterCommandStatuses(_v);
                            break;
                        case (BattleAbilityId)2003: // Mog Shield
                        case (BattleAbilityId)2014: // Moga Shield
                            if (GameRandom.Next8() % 2 == 0)
                                _v.Command.AbilityStatus |= BattleStatus.Protect;
                            else
                                _v.Command.AbilityStatus |= BattleStatus.Shell;

                            TranceSeekAPI.TryAlterCommandStatuses(_v);
                            break;
                        case (BattleAbilityId)2006: // Mog Esuna
                        case (BattleAbilityId)2017: // Moga Esuna
                            _v.Target.RemoveStatus(BattleStatus.Poison | BattleStatus.Venom | BattleStatus.Silence | BattleStatus.Blind | BattleStatus.Trouble | BattleStatus.Mini | BattleStatus.Berserk | TranceSeekStatus.Vieillissement);
                            break;
                        case (BattleAbilityId)2007: // Mog Support
                        case (BattleAbilityId)2018: // Moga Support
                            _v.Command.AbilityStatus |= (TranceSeekStatus.MagicUp | TranceSeekStatus.MentalUp);
                            TranceSeekAPI.TryAlterCommandStatuses(_v);
                            break;
                        case (BattleAbilityId)2008: // Mog Life
                            if (!_v.Target.CanBeRevived())
                            {
                                _v.Context.Flags |= BattleCalcFlags.Miss;
                                return;
                            }

                            if (HitRateForZombie() && !TranceSeekAPI.TryMagicHit(_v))
                                return;

                            if (_v.Target.IsZombie && !_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill))
                            {
                                _v.Target.Kill();
                                return;
                            }

                            if (!_v.Target.CheckIsPlayer())
                                return;

                            _v.Target.Flags |= CalcFlag.HpAlteration | CalcFlag.HpRecovery;
                            if (_v.Target.HasSupportAbilityByIndex((SupportAbility)1004)) // Invincible+
                            {
                                _v.Target.Flags |= CalcFlag.MpAlteration | CalcFlag.MpRecovery;
                                _v.Target.HpDamage = (int)_v.Target.MaximumHp;
                                _v.Target.MpDamage = (int)_v.Target.MaximumMp;
                            }
                            else
                            {
                                _v.Target.HpDamage = (Int32)(_v.Target.MaximumHp * _v.Command.Power) / 100;
                                if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)100)) // Medecin
                                    _v.Target.HpDamage += _v.Caster.HpDamage / (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1100) ? 2 : 4);
                            }
                            TranceSeekAPI.TryRemoveAbilityStatuses(_v);
                            break;
                        case (BattleAbilityId)2009: // Atomoug
                        case (BattleAbilityId)2010: // Sidémoug
                        case (BattleAbilityId)2019: // Mouga Homing
                            if (_v.Target.MagicDefence == 255)
                            {
                                _v.Context.Flags |= BattleCalcFlags.Guard;
                            }
                            else
                            {
                                _v.NormalMagicParams();
                                TranceSeekAPI.CharacterBonusPassive(_v, "MagicAttack");
                                TranceSeekAPI.CasterPenaltyMini(_v);
                                TranceSeekAPI.EnemyTranceBonusAttack(_v);
                                TranceSeekAPI.PenaltyShellAttack(_v);
                                TranceSeekAPI.PenaltyCommandDividedAttack(_v);
                                TranceSeekAPI.BonusElement(_v);
                                if (TranceSeekAPI.CanAttackMagic(_v))
                                {
                                    if (_v.Target.HasCategory(EnemyCategory.Humanoid) && (_v.Command.AbilityId == BattleAbilityId.Poison || _v.Command.AbilityId == BattleAbilityId.Bio))
                                        _v.Context.DamageModifierCount += 4;
                                    if (_v.Target.IsZombie && (_v.Command.SpecialEffect == SpecialEffect.Poison__Single || _v.Command.SpecialEffect == SpecialEffect.Poison__Multi || _v.Command.SpecialEffect == SpecialEffect.Bio__Single || _v.Command.SpecialEffect == SpecialEffect.Bio__Multi || _v.Command.SpecialEffect == SpecialEffect.Bio_Sword))
                                        _v.Target.Flags |= CalcFlag.HpRecovery;
                                    _v.CalcHpDamage();
                                }
                                TranceSeekAPI.TryAlterMagicStatuses(_v);
                            }
                            break;
                    }

                    NumberTargets[_v.Caster.Data]--;
                    if (NumberTargets[_v.Caster.Data] <= 0)
                    {
                        if (!ff9abil.FF9Abil_IsMaster(_v.Caster.Player, (int)_v.Command.AbilityId))
                            ff9abil.FF9Abil_SetMaster(_v.Caster.Player, (int)_v.Command.AbilityId);

                        TranceSeekAPI.ModelMoug[_v.Caster.Data].transform.localPosition = _v.Caster.Data.gameObject.transform.localPosition;
                        if (animation != null)
                        {
                            animation.Play("ANH_NPC_F4_MOG_INTO_EIK_PASSIVE_2");
                            SoundLib.PlaySoundEffect(1363);
                            if (animation["ANH_NPC_F4_MOG_INTO_EIK_PASSIVE_2"] != null)
                                animation["ANH_NPC_F4_MOG_INTO_EIK_PASSIVE_2"].speed = 1f;
                        }
                        Int32 counter = 25;
                        _v.Caster.AddDelayedModifier(
                            caster => (counter -= BattleState.ATBTickCount) > 0,
                            caster =>
                            {
                                TranceSeekAPI.StateMoug[caster.Data] = 0;
                                UnityEngine.Object.Destroy(TranceSeekAPI.ModelMoug[_v.Caster.Data]);
                                TranceSeekAPI.ModelMoug[_v.Caster.Data] = null;
                            }
                        );
                    }
                }
            }
        }
        private Boolean HitRateForZombie()
        {
            if (_v.Target.IsZombie)
            {
                TranceSeekAPI.MagicAccuracy(_v);
                return true;
            }
            return false;
        }
    }
}
