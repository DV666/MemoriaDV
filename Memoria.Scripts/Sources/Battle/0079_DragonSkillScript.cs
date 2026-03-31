using System;
using System.Collections.Generic;
using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Assets;
using Memoria.Data;
using UnityEngine;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Dark Matter
    /// </summary>
    [BattleScript(Id)]
    public sealed class DragonSkillScript : IBattleScript
    {
        public const Int32 Id = 0079;

        private readonly BattleCalculator _v;

        public DragonSkillScript(BattleCalculator v)
        {
            _v = v;
        }

        public static Dictionary<BTL_DATA, SPSEffect> ReiWrathSPS = new Dictionary<BTL_DATA, SPSEffect>();
        public static Dictionary<BTL_DATA, Int32> ReiWrathChance = new Dictionary<BTL_DATA, Int32>();

        public void Perform()
        {
            BattleAbilityId abilityId = _v.Command.AbilityId;

            bool IsAbility(BattleAbilityId targetId) => abilityId == targetId || (!_v.Caster.IsPlayer && _v.Command.Data.aa.Vfx2 == (ushort)targetId);
            bool isDragonOrTrance = _v.Target.IsUnderAnyStatus(TranceSeekStatus.Dragon) || _v.Caster.IsUnderStatus(BattleStatus.Trance);

            if (_v.Target.IsUnderAnyStatus(TranceSeekStatus.Dragon))
                _v.Context.DamageModifierCount++;

            if (IsAbility(BattleAbilityId.Lancer))
            {
                if (_v.IsCasterNotTarget() && _v.Target.CanBeAttacked() && !_v.Target.TryKillFrozen())
                {
                    if (_v.Caster.IsPlayer)
                        _v.WeaponPhysicalParams();
                    else
                        _v.NormalPhysicalParams();

                    TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                    TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                    _v.Caster.Flags |= CalcFlag.HpAlteration;
                    TranceSeekAPI.BonusBackstabAndPenaltyLongDistance(_v);
                    TranceSeekAPI.BonusWeaponElement(_v);

                    if (TranceSeekAPI.CanAttackWeaponElementalCommand(_v))
                    {
                        TranceSeekAPI.IpsenCastleMalus(_v);
                        TranceSeekAPI.RaiseTrouble(_v);
                        if (_v.Caster.HasSupportAbility(SupportAbility1.AddStatus))
                        {
                            _v.Context.StatusRate = _v.Caster.WeaponRate;
                            if (_v.Context.StatusRate > GameRandom.Next16() % 100)
                            {
                                _v.Context.Flags |= BattleCalcFlags.AddStat;
                            }
                        }
                    }
                    TranceSeekAPI.TryCriticalHit(_v);
                    _v.CalcPhysicalHpDamage();
                    int hpDamage = _v.Target.HpDamage = Math.Max(1, _v.Target.HpDamage);
                    _v.Target.FaceTheEnemy();

                    if (!_v.Context.IsAbsorb)
                    {
                        _v.Caster.Flags |= CalcFlag.HpRecovery;
                        _v.Target.Flags |= (CalcFlag.HpAlteration | CalcFlag.MpAlteration);
                        _v.Target.MpDamage = Math.Max(1, hpDamage >> 5);
                        _v.Caster.HpDamage = Math.Max(1, hpDamage / 2);
                        if (isDragonOrTrance)
                        {
                            _v.Caster.Flags |= (CalcFlag.MpAlteration | CalcFlag.MpRecovery);
                            _v.Caster.MpDamage = _v.Target.MpDamage / 2;
                        }
                    }
                    TranceSeekAPI.TryAlterMagicStatuses(_v);
                }
            }
            else if (IsAbility(BattleAbilityId.Luna))
            {
                btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.Dragon, _v.Caster, parameters: "Add");
            }
            else if (IsAbility(TranceSeekBattleAbility.ReiWrath))
            {
                SPSEffect sps = HonoluluBattleMain.battleSPS.AddSequenceSPS(15, -1, 1, true);
                if (sps == null)
                    return;
                btl2d.GetIconPosition(_v.Target, btl2d.ICON_POS_ROOT, out Transform attachTransf, out Vector3 iconOff);
                sps.charTran = _v.Target.Data.gameObject.transform;
                sps.boneTran = attachTransf;
                sps.posOffset = new Vector3(0, -100, 0);
                sps.scale *= 2;
                ReiWrathSPS[_v.Target.Data] = sps;
                ReiWrathChance[_v.Target.Data] = (_v.Target.IsUnderAnyStatus(TranceSeekStatus.Dragon)) ? 100 : (_v.Target.Will / 2);
            }
            else if (abilityId == TranceSeekBattleAbility.LitanyOfBurmecia) // Note: Pas de Vfx2 check dans l'original
            {
                _v.Target.AlterStatus(BattleStatus.Float, _v.Caster);
                _v.Target.AlterStatus(TranceSeekStatus.MentalUp, _v.Caster);
                if (isDragonOrTrance)
                {
                    _v.Target.Flags |= CalcFlag.MpDamageOrHeal;
                    _v.Target.MpDamage = (int)(_v.Target.MaximumMp / 4);
                }
            }
            else if (IsAbility(BattleAbilityId.DragonBreath))
            {
                SetupMagicAttack(_v);
                if (TranceSeekAPI.CanAttackMagic(_v))
                {
                    TranceSeekAPI.TryCriticalHit(_v);
                    if (isDragonOrTrance)
                    {
                        int bonusdamage = 0;
                        foreach (BattleStatusId statusId in _v.Target.Data.stat.cur.ToStatusList())
                        {
                            if (statusId != BattleStatusId.EasyKill)
                            {
                                bonusdamage += 5;
                                btl_stat.RemoveStatus(_v.Target, statusId);
                            }
                        }
                        if (bonusdamage > 0)
                            _v.Context.Attack += (_v.Context.Attack * bonusdamage) / 100;
                    }
                    _v.CalcHpDamage();
                }
                TranceSeekAPI.TryAlterMagicStatuses(_v);
            }
            else if (IsAbility(BattleAbilityId.SixDragons))
            {
                SetupMagicAttack(_v);
                if (TranceSeekAPI.CanAttackMagic(_v))
                {
                    TranceSeekAPI.TryCriticalHit(_v);
                    if (isDragonOrTrance)
                    {
                        if (_v.Target.MagicDefence != 255)
                            _v.Context.DefensePower = (_v.Context.DefensePower / 2);
                        _v.Target.TryAlterStatuses(TranceSeekStatus.MentalBreak, false, _v.Caster);
                    }
                    _v.CalcHpDamage();
                }
                TranceSeekAPI.TryAlterMagicStatuses(_v);
            }
            else if (IsAbility(BattleAbilityId.ReisWind))
            {
                if (!_v.Target.CanBeAttacked())
                    return;

                var Target_TSVar = _v.TargetState();

                uint TargetCurrentHP = Target_TSVar.Monster.HPBoss10000 ? (_v.Target.CurrentHp - 10000) : _v.Target.CurrentHp;
                uint HPratio = (TargetCurrentHP / _v.Target.MaximumHp) * 100;
                _v.Target.Flags = CalcFlag.HpAlteration;
                _v.NormalMagicParams();
                TranceSeekAPI.CasterPenaltyMini(_v);
                _v.CalcHpMagicRecovery();

                if (!_v.Target.IsUnderAnyStatus(BattleStatus.Zombie))
                    _v.Target.Flags |= CalcFlag.HpRecovery;

                if (isDragonOrTrance)
                    _v.Target.HpDamage *= 2;

                if (_v.Target.IsUnderAnyStatus(BattleStatus.LowHP) || (TargetCurrentHP <= _v.Target.MaximumHp / 4 && HPratio <= Comn.random16() % 100))
                {
                    BattleStatusId[] statuslist = { BattleStatusId.Regen, BattleStatusId.Haste, BattleStatusId.Float, BattleStatusId.Shell, BattleStatusId.Vanish,
                    BattleStatusId.Protect, BattleStatusId.AutoLife};

                    List<BattleStatusId> statuschoosen = new List<BattleStatusId>();

                    for (Int32 i = 0; i < statuslist.Length; i++)
                    {
                        if ((statuslist[i].ToBattleStatus() & _v.Target.CurrentStatus) == 0)
                        {
                            statuschoosen.Add(statuslist[i]);
                        }
                    }

                    if (statuschoosen.Count > 0)
                    {
                        BattleStatusId statusselected = statuschoosen[GameRandom.Next16() % statuschoosen.Count];
                        btl_stat.AlterStatus(_v.Target, statusselected, _v.Caster);
                        return;
                    }

                    _v.Target.Flags |= (CalcFlag.MpAlteration);
                    if (!_v.Target.IsUnderAnyStatus(BattleStatus.Zombie))
                        _v.Target.Flags |= CalcFlag.MpRecovery;

                    _v.Target.MpDamage = (int)(_v.Target.MaximumMp / 4);
                    return;
                }
            }
            else if (IsAbility(BattleAbilityId.CherryBlossom))
            {
                SetupPhysicalAttack(_v);
                if (_v.CanAttackMagic())
                {
                    _v.CalcHpDamage();

                    if (isDragonOrTrance)
                    {
                        if (_v.Target.IsUnderAnyStatus(BattleStatus.Poison))
                            _v.Target.TryAlterStatuses(BattleStatus.Venom, false, _v.Caster);
                        else
                            TranceSeekAPI.TryAlterMagicStatuses(_v);
                    }
                }
            }
            else if (IsAbility(BattleAbilityId.WhiteDraw))
            {
                SetupMagicAttack(_v);
                if (TranceSeekAPI.CanAttackMagic(_v))
                {
                    TranceSeekAPI.TryCriticalHit(_v);
                    _v.CalcHpDamage();
                }
                TranceSeekAPI.TryAlterMagicStatuses(_v);
            }
            else if (IsAbility(BattleAbilityId.DragonCrest))
            {
                SetupPhysicalAttack(_v);
                if (_v.CanAttackMagic())
                {
                    _v.CalcHpDamage();

                    if (isDragonOrTrance)
                        _v.Target.TryAlterStatuses(BattleStatus.Doom, false, _v.Caster);
                }
            }
            else if (IsAbility(TranceSeekBattleAbility.Geirskögul))
            {
                SetupPhysicalAttack(_v);
                if (_v.CanAttackMagic())
                {
                    _v.CalcHpDamage();

                    if (_v.Command.AbilityId == TranceSeekBattleAbility.Geirskögul)
                        _v.Target.HpDamage /= 3;

                    if (isDragonOrTrance)
                        TranceSeekAPI.TryCriticalHit(_v, 255);
                }
            }
            else if (IsAbility(TranceSeekBattleAbility.Hraesvelgr))
            {
                SetupMagicAttack(_v);
                if (TranceSeekAPI.CanAttackMagic(_v))
                {
                    TranceSeekAPI.TryCriticalHit(_v);
                    if (isDragonOrTrance)
                    {
                        _v.Caster.RemoveStatus(BattleStatusConst.AnyNegative);
                        _v.Caster.Flags |= CalcFlag.HpDamageOrHeal;
                        _v.Caster.HpDamage = (int)_v.Caster.MaximumHp;
                    }
                    _v.CalcHpDamage();
                }
                TranceSeekAPI.TryAlterMagicStatuses(_v);
            }
        }

        public static void ReiWrathTrigger(BattleCalculator v)
        {
            if (!ReiWrathSPS.TryGetValue(v.Caster.Data, out SPSEffect sps))
                ReiWrathSPS[v.Caster.Data] = null;

            if (sps != null)
            {
                int DragonStack = v.Target.IsUnderAnyStatus(TranceSeekStatus.Dragon) ? (int)v.Target.GetPropertyByName("StatusProperty CustomStatus9 Stack") : 0;
                int StackMaximum = v.Caster.HasSupportAbilityByIndex((SupportAbility)1218) ? 3 : (v.Caster.HasSupportAbilityByIndex((SupportAbility)218) ? 2 : 1);
                if ((Comn.random16() % 100) <= ReiWrathChance[v.Caster.Data] && DragonStack < StackMaximum)
                {
                    ReiWrathSPS[v.Caster.Data].attr = 0;
                    ReiWrathSPS[v.Caster.Data].meshRenderer.enabled = false;
                    v.Target.AlterStatus(TranceSeekStatus.Dragon, v.Caster);
                }
            }
        }

        private void SetupMagicAttack(BattleCalculator v)
        {
            if (v.Caster.IsPlayer)
            {
                v.OriginalMagicParams();
            }
            else
            {
                v.SetCommandPower();
                v.Caster.SetLowPhysicalAttack();
                v.Target.SetMagicDefense();
            }

            TranceSeekAPI.CasterPenaltyMini(v);
            if (v.Target.IsUnderStatus(BattleStatus.Defend))
                v.Context.DamageModifierCount -= 2;

            TranceSeekAPI.PenaltyShellAttack(v);
            TranceSeekAPI.EnemyTranceBonusAttack(v);
            TranceSeekAPI.BonusElement(v);
        }

        private void SetupPhysicalAttack(BattleCalculator v)
        {
            if (v.Caster.IsPlayer)
                v.WeaponPhysicalParams();
            else
                v.NormalPhysicalParams();

            TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(v);
            TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(v);
            TranceSeekAPI.EnemyTranceBonusAttack(v);
            TranceSeekAPI.BonusElement(v);
        }
    }
}
