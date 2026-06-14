using FF9;
using Memoria.Data;
using Memoria.Prime;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Memoria.Scripts.TranceSeek.TranceSeekVisualAccessoryDB;

namespace Memoria.Scripts.TranceSeek
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

        public void Perform()
        {
            if (_v.Caster.PlayerIndex == CharacterId.Eiko)
            {
                var Eiko_TSVar = _v.CasterState().Eiko;
                string IdleAnim = "ANH_NPC_F4_MOG_IDLE";

                if (Eiko_TSVar.StateMoug == 1) // Moug appears.
                {
                    if (Eiko_TSVar.ModelMoug == null) // For safety (normally, init in OverloadBattleInit).
                        InitMogModel(_v.Caster);

                    if ((_v.Command.AbilityCategory & 16) != 0)
                        _v.Command.AbilityCategory -= 16; // Remove magical effect (for Vanish)

                    Eiko_TSVar.ModelMoug.SetActive(true);
                    // ANH_NPC_F4_MOG_INTO_EIK_PASSIVE_1
                    // ANH_NPC_F4_MOG_INTO_EIK_PASSIVE_2
                    if (FF9StateSystem.EventState.ScenarioCounter > 9990) // Moug has a "phantom" effect after Mont Gulug.
                        btl_util.GeoSetABR(Eiko_TSVar.ModelMoug, "GEO_POLYFLAGS_TRANS_100_PLUS_25");
                    Eiko_TSVar.ModelMoug.transform.localPosition = _v.Caster.Data.gameObject.transform.localPosition;
                    Eiko_TSVar.ModelMoug.transform.localRotation = _v.Caster.Data.gameObject.transform.localRotation;
                    Eiko_TSVar.ModelMoug.transform.localScale = _v.Caster.Data.gameObject.transform.localScale;
                    Eiko_TSVar.StateMoug = 2;
                    if (Eiko_TSVar.AnimationMoug != null)
                    {
                        Eiko_TSVar.AnimationMoug.Play("ANH_NPC_F4_MOG_INTO_EIK_PASSIVE_1");
                        _v.Caster.AddDelayedModifier(
                            caster => Eiko_TSVar.AnimationMoug != null && Eiko_TSVar.AnimationMoug.IsPlaying("ANH_NPC_F4_MOG_INTO_EIK_PASSIVE_1"),
                            caster =>
                            {
                                if (Eiko_TSVar.ModelMoug != null)
                                {
                                    Eiko_TSVar.ModelMoug.transform.localPosition = caster.Data.gameObject.transform.localPosition + new Vector3(0f, 0f, 250f);
                                    Eiko_TSVar.AnimationMoug.Play(IdleAnim);
                                }
                            }
                        );
                    }
                    return;
                }
                else if (Eiko_TSVar.StateMoug == 2 && Eiko_TSVar.ModelMoug != null) // Moug cast.
                {
                    if ((_v.Command.AbilityCategory & 16) != 0)
                        _v.Command.AbilityCategory -= 16; // Remove magical effect (for Vanish)

                    if (Eiko_TSVar.AnimationMoug != null)
                    {
                        Eiko_TSVar.AnimationMoug.PlayQueued("ANH_NPC_F4_MOG_INTO_EIK_2", QueueMode.PlayNow);
                        Eiko_TSVar.AnimationMoug.PlayQueued(IdleAnim, QueueMode.CompleteOthers);
                    }
                    Eiko_TSVar.StateMoug = 3;
                    Eiko_TSVar.NumberTargets = _v.Command.TargetCount;
                    return;
                }
                else if (Eiko_TSVar.StateMoug == 3 && Eiko_TSVar.ModelMoug != null) // Moug cast
                {
                    if (Eiko_TSVar.AnimationMoug != null)
                    {
                        Eiko_TSVar.AnimationMoug.Play("ANH_NPC_F4_MOG_IDLE");
                        if (Eiko_TSVar.AnimationMoug["ANH_NPC_F4_MOG_IDLE"] != null)
                            Eiko_TSVar.AnimationMoug["ANH_NPC_F4_MOG_IDLE"].speed = 1f;
                    }
                    switch (_v.Command.AbilityId)
                    {
                        case TranceSeekBattleAbility.MogCure: // Mog Cure
                        case TranceSeekBattleAbility.MogaCure: // Moga Cure
                            _v.Target.Flags = (CalcFlag.HpAlteration);
                            if (!_v.Target.IsZombie)
                                _v.Target.Flags |= CalcFlag.HpRecovery;
                            _v.Target.HpDamage = (int)(_v.Target.MaximumHp / _v.Command.Power);
                        break;
                        case TranceSeekBattleAbility.MogHug: // Mog Hug
                        case TranceSeekBattleAbility.MogHug2: // Moga Hug
                            _v.Target.Flags = (CalcFlag.MpAlteration);
                            if (!_v.Target.IsZombie)
                                _v.Target.Flags |= CalcFlag.MpRecovery;
                            _v.Target.MpDamage = (int)(_v.Target.MaximumMp / _v.Command.Power);
                            break;
                        case TranceSeekBattleAbility.MogRegen: // Mog Regen
                        case TranceSeekBattleAbility.MogMirror: // Mog Mirror
                        case TranceSeekBattleAbility.MogAutoLife: // Mog AutoLife
                        case TranceSeekBattleAbility.MogRegen2: // Moga Regen
                        case TranceSeekBattleAbility.MogMirror2: // Moga Mirror
                        case TranceSeekBattleAbility.MogAutoLife2: // Moga AutoLife
                            TranceSeekAPI.TryAlterCommandStatuses(_v);
                            break;
                        case TranceSeekBattleAbility.MogShield: // Mog Shield
                        case TranceSeekBattleAbility.MogShield2: // Moga Shield
                            if (GameRandom.Next8() % 2 == 0)
                                _v.Command.AbilityStatus |= BattleStatus.Protect;
                            else
                                _v.Command.AbilityStatus |= BattleStatus.Shell;

                            TranceSeekAPI.TryAlterCommandStatuses(_v);
                            break;
                        case TranceSeekBattleAbility.MogEsuna: // Mog Esuna
                        case TranceSeekBattleAbility.MogaEsuna: // Moga Esuna
                            _v.Target.RemoveStatus(BattleStatus.Poison | BattleStatus.Venom | BattleStatus.Silence | BattleStatus.Blind | BattleStatus.Trouble | BattleStatus.Mini | BattleStatus.Berserk | TranceSeekStatus.Vieillissement);
                            break;
                        case TranceSeekBattleAbility.MogSupport: // Mog Support
                        case TranceSeekBattleAbility.MogSupport2: // Moga Support
                            _v.Command.AbilityStatus |= (TranceSeekStatus.MagicUp | TranceSeekStatus.MentalUp);
                            TranceSeekAPI.TryAlterCommandStatuses(_v);
                            break;
                        case TranceSeekBattleAbility.MogLife: // Mog Life
                            if (!_v.Target.CanBeRevived())
                            {
                                _v.Context.Flags |= BattleCalcFlags.Miss;
                            }
                            else
                            {
                                if (!HitRateForZombie() || TranceSeekAPI.TryMagicHit(_v))
                                {
                                    if (_v.Target.IsZombie && !_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill))
                                    {
                                        _v.Target.Kill();
                                    }
                                    else
                                    {
                                        _v.Target.Flags |= CalcFlag.HpAlteration | CalcFlag.HpRecovery;
                                        TranceSeekAPI.ReviveHeal(_v, (int)((_v.Target.MaximumHp * _v.Command.Power) / 100));
                                        TranceSeekAPI.TryRemoveAbilityStatuses(_v);
                                    }
                                }
                            }
                            break;
                        case TranceSeekBattleAbility.MogFlare: // Atomoug
                        case TranceSeekBattleAbility.MogHoly: // Sidémoug
                        case TranceSeekBattleAbility.MougaHoming: // Mouga Homing
                            if (_v.Target.MagicDefence == 255)
                            {
                                _v.Context.Flags |= BattleCalcFlags.Guard;
                            }
                            else
                            {
                                _v.NormalMagicParams();
                                
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

                    Eiko_TSVar.NumberTargets--;
                    if (Eiko_TSVar.NumberTargets <= 0)
                    {
                        if (!ff9abil.FF9Abil_IsMaster(_v.Caster.Player, (int)_v.Command.AbilityId))
                            ff9abil.FF9Abil_SetMaster(_v.Caster.Player, (int)_v.Command.AbilityId);

                        Eiko_TSVar.ModelMoug.transform.localPosition = _v.Caster.Data.gameObject.transform.localPosition;
                        if (Eiko_TSVar.AnimationMoug != null)
                        {
                            Eiko_TSVar.AnimationMoug.Play("ANH_NPC_F4_MOG_INTO_EIK_PASSIVE_2");
                            SoundLib.PlaySoundEffect(1363);
                            if (Eiko_TSVar.AnimationMoug["ANH_NPC_F4_MOG_INTO_EIK_PASSIVE_2"] != null)
                                Eiko_TSVar.AnimationMoug["ANH_NPC_F4_MOG_INTO_EIK_PASSIVE_2"].speed = 1f;
                        }
                        Int32 counter = 25;
                        _v.Caster.AddDelayedModifier(
                            caster => (counter -= BattleState.ATBTickCount) > 0,
                            caster =>
                            {
                                Eiko_TSVar.StateMoug = 0;
                                Eiko_TSVar.ModelMoug.SetActive(false);
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

        public static void InitMogModel(BattleUnit unit)
        {
            var Eiko_TSVar = unit.State().Eiko;
            Eiko_TSVar.ModelMoug = ModelFactory.CreateModel("GEO_NPC_F4_MOG", true);

            GameObject MogGeo = Eiko_TSVar.ModelMoug;

            Eiko_TSVar.AnimationMoug = MogGeo.GetComponent<Animation>();
            AnimationFactory.AddAnimWithAnimatioName(MogGeo, "ANH_NPC_F4_MOG_INTO_EIK_PASSIVE_1");
            AnimationFactory.AddAnimWithAnimatioName(MogGeo, "ANH_NPC_F4_MOG_INTO_EIK_PASSIVE_2");
            AnimationFactory.AddAnimWithAnimatioName(MogGeo, "ANH_NPC_F4_MOG_INTO_EIK_2");
            AnimationFactory.AddAnimWithAnimatioName(MogGeo, "ANH_NPC_F4_MOG_IDLE");

            if (Eiko_TSVar.AnimationMoug != null && Eiko_TSVar.AnimationMoug.GetClip("ANH_NPC_F4_MOG_IDLE") != null)
                Eiko_TSVar.AnimationMoug["ANH_NPC_F4_MOG_IDLE"].wrapMode = WrapMode.Loop;

            Eiko_TSVar.ModelMoug.SetActive(false);
            Eiko_TSVar.StateMoug = 0;
        }

    }
}


