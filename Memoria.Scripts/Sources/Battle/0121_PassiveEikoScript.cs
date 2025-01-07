using System;
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

        public void Perform()
        {
            if (_v.Caster.PlayerIndex == CharacterId.Eiko)
            {
                if (!TranceSeekCustomAPI.StateMoug.TryGetValue(_v.Caster.Data, out Int32 State))
                    TranceSeekCustomAPI.StateMoug[_v.Caster.Data] = 0;

                if (!TranceSeekCustomAPI.ModelMoug.TryGetValue(_v.Caster.Data, out GameObject ModelMoug))
                    TranceSeekCustomAPI.ModelMoug[_v.Caster.Data] = null;

                if (TranceSeekCustomAPI.StateMoug[_v.Caster.Data] == 0) // Moug appears.
                {
                    TranceSeekCustomAPI.ModelMoug[_v.Caster.Data] = ModelFactory.CreateModel("GEO_NPC_F4_MOG", true);
                    // ANH_NPC_F4_MOG_INTO_EIK_PASSIVE_1
                    // ANH_NPC_F4_MOG_INTO_EIK_PASSIVE_2
                    TranceSeekCustomAPI.ModelMoug[_v.Caster.Data].transform.localPosition = _v.Caster.Data.gameObject.transform.localPosition;
                    TranceSeekCustomAPI.ModelMoug[_v.Caster.Data].transform.localRotation = _v.Caster.Data.gameObject.transform.localRotation;
                    TranceSeekCustomAPI.ModelMoug[_v.Caster.Data].transform.localScale = _v.Caster.Data.gameObject.transform.localScale;
                    TranceSeekCustomAPI.StateMoug[_v.Caster.Data]++;
                    TranceSeekCustomAPI.ModelMoug[_v.Caster.Data].SetActive(true);
                    Animation animation = TranceSeekCustomAPI.ModelMoug[_v.Caster.Data].GetComponent<Animation>();
                    if (animation.GetClip("ANH_NPC_F4_MOG_INTO_EIK_PASSIVE_1") == null)
                        AnimationFactory.AddAnimWithAnimatioName(TranceSeekCustomAPI.ModelMoug[_v.Caster.Data], "ANH_NPC_F4_MOG_INTO_EIK_PASSIVE_1");
                    if (animation.GetClip("ANH_NPC_F4_MOG_INTO_EIK_PASSIVE_2") == null)
                        AnimationFactory.AddAnimWithAnimatioName(TranceSeekCustomAPI.ModelMoug[_v.Caster.Data], "ANH_NPC_F4_MOG_INTO_EIK_PASSIVE_2");
                    if (animation != null)
                    {
                        animation.Play("ANH_NPC_F4_MOG_INTO_EIK_PASSIVE_1");
                        if (animation["ANH_NPC_F4_MOG_INTO_EIK_PASSIVE_1"] != null)
                            animation["ANH_NPC_F4_MOG_INTO_EIK_PASSIVE_1"].speed = 1f;
                    }
                    return;
                }
                else if (TranceSeekCustomAPI.StateMoug[_v.Caster.Data] == 1) // Moug cast.
                {
                    TranceSeekCustomAPI.ModelMoug[_v.Caster.Data].transform.localPosition = _v.Caster.Data.gameObject.transform.localPosition + new Vector3(0f, 0f, 250f);
                    Animation animation = TranceSeekCustomAPI.ModelMoug[_v.Caster.Data].GetComponent<Animation>();
                    if (animation != null)
                    {
                        animation.Play("ANH_NPC_F4_MOG_INTO_EIK_2");
                        if (animation["ANH_NPC_F4_MOG_INTO_EIK_2"] != null)
                            animation["ANH_NPC_F4_MOG_INTO_EIK_2"].speed = 1f;
                    }
                    TranceSeekCustomAPI.StateMoug[_v.Caster.Data]++;
                    return;
                }
                else if (TranceSeekCustomAPI.StateMoug[_v.Caster.Data] == 2) // Moug cast
                {
                    Animation animation = TranceSeekCustomAPI.ModelMoug[_v.Caster.Data].GetComponent<Animation>();
                    if (animation != null)
                    {
                        animation.Play("ANH_NPC_F4_MOG_IDLE");
                        if (animation["ANH_NPC_F4_MOG_IDLE"] != null)
                            animation["ANH_NPC_F4_MOG_IDLE"].speed = 1f;
                    }
                    switch (_v.Command.AbilityId)
                    {
                        case (BattleAbilityId)1017: // Soin Moug
                            _v.Target.Flags = (CalcFlag.HpAlteration);
                            if (!_v.Target.IsZombie)
                                _v.Target.Flags |= CalcFlag.HpRecovery;
                            _v.Target.HpDamage = (int)(_v.Target.MaximumHp / 2);
                        break;
                        case (BattleAbilityId)1018: // Calin Moug
                            _v.Target.Flags = (CalcFlag.MpAlteration);
                            if (!_v.Target.IsZombie)
                                _v.Target.Flags |= CalcFlag.MpRecovery;
                            _v.Target.MpDamage = (int)(_v.Target.MaximumMp / 4);
                            _v.Target.RemoveStatus(BattleStatus.Poison | BattleStatus.Venom | BattleStatus.Silence | BattleStatus.Blind | BattleStatus.Trouble | BattleStatus.Mini | BattleStatus.Berserk | TranceSeekCustomAPI.CustomStatus.Vieillissement);
                            break;
                        case (BattleAbilityId)1019: // Récup Moug
                            _v.Target.AlterStatus(BattleStatus.Regen, _v.Caster);
                            _v.Target.AlterStatus(BattleStatus.Haste, _v.Caster);
                            break;
                        case (BattleAbilityId)1020: // Barrière Moug
                            if (GameRandom.Next8() % 2 != 0)
                                _v.Target.AlterStatus(BattleStatus.Protect, _v.Caster);
                            else
                                _v.Target.AlterStatus(BattleStatus.Shell, _v.Caster);
                            break;
                        case (BattleAbilityId)1021: // Mirroir Moug
                            _v.Target.AlterStatus(BattleStatus.Vanish, _v.Caster);
                            break;
                        case (BattleAbilityId)1022: // Pakaho Moug
                            _v.Target.AlterStatus(BattleStatus.AutoLife, _v.Caster);
                            break;
                        case (BattleAbilityId)1023: // Atomoug
                        case (BattleAbilityId)1024: // Sidémoug
                            if (_v.Target.MagicDefence == 255)
                            {
                                _v.Context.Flags |= BattleCalcFlags.Guard;
                            }
                            else
                            {
                                _v.NormalMagicParams();
                                TranceSeekCustomAPI.CharacterBonusPassive(_v, "MagicAttack");
                                TranceSeekCustomAPI.CasterPenaltyMini(_v);
                                TranceSeekCustomAPI.EnemyTranceBonusAttack(_v);
                                TranceSeekCustomAPI.PenaltyShellAttack(_v);
                                TranceSeekCustomAPI.PenaltyCommandDividedAttack(_v);
                                TranceSeekCustomAPI.BonusElement(_v);
                                if (TranceSeekCustomAPI.CanAttackMagic(_v))
                                {
                                    if (_v.Target.HasCategory(EnemyCategory.Humanoid) && (_v.Command.AbilityId == BattleAbilityId.Poison || _v.Command.AbilityId == BattleAbilityId.Bio))
                                    {
                                        _v.Context.Attack = _v.Context.Attack * 2;
                                    }
                                    if (_v.Target.IsZombie && (_v.Command.SpecialEffect == SpecialEffect.Poison__Single || _v.Command.SpecialEffect == SpecialEffect.Poison__Multi || _v.Command.SpecialEffect == SpecialEffect.Bio__Single || _v.Command.SpecialEffect == SpecialEffect.Bio__Multi || _v.Command.SpecialEffect == SpecialEffect.Bio_Sword))
                                    {
                                        _v.Target.Flags |= CalcFlag.HpRecovery;
                                    }
                                    _v.CalcHpDamage();
                                }
                                _v.TryAlterMagicStatuses();
                            }
                            break;
                    }
                    TranceSeekCustomAPI.StateMoug[_v.Caster.Data]++;
                }
                else if (TranceSeekCustomAPI.StateMoug[_v.Caster.Data] == 3) // Moug dissapear.
                {
                    TranceSeekCustomAPI.ModelMoug[_v.Caster.Data].transform.localPosition = _v.Caster.Data.gameObject.transform.localPosition;
                    Animation animation = TranceSeekCustomAPI.ModelMoug[_v.Caster.Data].GetComponent<Animation>();
                    if (animation != null)
                    {
                        animation.Play("ANH_NPC_F4_MOG_INTO_EIK_PASSIVE_2");
                        if (animation["ANH_NPC_F4_MOG_INTO_EIK_PASSIVE_2"] != null)
                            animation["ANH_NPC_F4_MOG_INTO_EIK_PASSIVE_2"].speed = 1f;
                    }
                    _v.Caster.AddDelayedModifier(
                        caster => TranceSeekCustomAPI.ModelMoug[_v.Caster.Data].GetComponent<Animation>().IsPlaying("ANH_NPC_F4_MOG_INTO_EIK_PASSIVE_2"),
                        caster =>
                        {
                            TranceSeekCustomAPI.StateMoug[caster.Data] = 0;
                            UnityEngine.Object.Destroy(TranceSeekCustomAPI.ModelMoug[_v.Caster.Data]);
                        }
                    );
                    TranceSeekCustomAPI.StateMoug[_v.Caster.Data]++;
                    return;
                }
            }         
        }
    }
}
