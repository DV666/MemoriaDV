using System;
using System.Collections.Generic;
using FF9;
using Memoria.Data;
using UnityEngine;

namespace Memoria.Scripts.Battle
{
    [BattleScript(Id)]
    public sealed class SummonMonsterScript : IBattleScript
    {
        public const Int32 Id = 0140;

        private readonly BattleCalculator _v;

        public static Dictionary<BTL_DATA, Int32> SummonStep = new Dictionary<BTL_DATA, Int32>();
        public static Dictionary<BTL_DATA, Vector3> InitPosition = new Dictionary<BTL_DATA, Vector3>();
        public static Dictionary<BTL_DATA, Int32> NumberTargets = new Dictionary<BTL_DATA, Int32>();

        public SummonMonsterScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (!SummonStep.TryGetValue(_v.Caster.Data, out Int32 IDStep)) // Init
                SummonStep[_v.Caster.Data] = 0;
            if (!InitPosition.TryGetValue(_v.Caster.Data, out Vector3 InitPos)) // Init
                InitPosition[_v.Caster.Data] = Vector3.zero;
            if (!NumberTargets.TryGetValue(_v.Caster.Data, out Int32 Targets)) // Init
                NumberTargets[_v.Caster.Data] = 0;

            _v.Caster.AddDelayedModifier(
                caster => caster.CurrentAtb >= caster.MaximumAtb,
                caster =>
                {
                    SummonStep[_v.Caster.Data] = 0;
                }
            );

            if (SummonStep[_v.Caster.Data] == 0) // Change into the monster
            {
                short IDMonster = GeoMonsterWithAA[(Int32)_v.Command.AbilityId];
                String geoName = FF9BattleDB.GEO.GetValue(IDMonster);
                InitPosition[_v.Caster.Data] = _v.Caster.Data.gameObject.transform.localPosition;
                _v.Caster.Data.weapon_geo.SetActive(false);
                _v.Caster.Data.flags |= geo.GEO_FLAGS_CLIP;
                _v.Caster.Data.ChangeModel(ModelFactory.CreateModel(geoName, true, true, Configuration.Graphics.ElementsSmoothTexture), IDMonster);
                _v.Caster.Data.gameObject.transform.localPosition = InitPosition[_v.Caster.Data];
                geoName = geoName.Substring(4);
                if (IDMonster == 244) // Cactuar (need to use idle_alternate)
                {
                    Animation animation = _v.Caster.Data.gameObject.GetComponent<Animation>();
                    if (animation.GetClip("ANH_" + geoName + "_001") == null)
                        AnimationFactory.AddAnimWithAnimatioName(_v.Caster.Data.gameObject, "ANH_" + geoName + "_001");
                    for (Int32 i = 0; i < 34; i++)
                        _v.Caster.Data.mot[i] = "ANH_" + geoName + "_001";
                }
                else
                {
                    Animation animation = _v.Caster.Data.gameObject.GetComponent<Animation>();
                    if (animation.GetClip("ANH_" + geoName + "_000") == null)
                        AnimationFactory.AddAnimWithAnimatioName(_v.Caster.Data.gameObject, "ANH_" + geoName + "_000");
                    for (Int32 i = 0; i < 34; i++)
                        _v.Caster.Data.mot[i] = "ANH_" + geoName + "_000";
                }          
                btl_mot.setMotion(_v.Caster.Data, BattlePlayerCharacter.PlayerMotionIndex.MP_IDLE_NORMAL);
                // No M-Sword => Multiple (ennemy) / Short => Multiple (ally) / No M-Sword + Short => Everyone
                NumberTargets[_v.Caster.Data] = (_v.Command.AbilityCategory & 4) != 0 && (_v.Command.AbilityCategory & 32) != 0 ? (BattleState.TargetCount(true) + BattleState.TargetCount(false)) : ((_v.Command.AbilityCategory & 4) != 0 ? BattleState.TargetCount(false) : (_v.Command.AbilityCategory & 32) != 0 ? BattleState.TargetCount(true) : 1);
                SummonStep[_v.Caster.Data] = 1;
            }
            else if (SummonStep[_v.Caster.Data] == 1) // Script for the damage
            {
                switch (_v.Command.AbilityId)
                {
                    // BLOOD SWORD WEAPON - Script 6
                    case (BattleAbilityId)1212: // Seeker Bat - Absorb even more
                    {
                        if (_v.Target.CanBeAttacked() && !_v.Target.TryKillFrozen())
                        {
                            _v.PhysicalAccuracy();
                            if (TranceSeekCustomAPI.TryPhysicalHit(_v))
                            {
                                _v.NormalPhysicalParams();
                                TranceSeekCustomAPI.EnemyTranceBonusAttack(_v);
                                TranceSeekCustomAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                                TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                                TranceSeekCustomAPI.BonusBackstabAndPenaltyLongDistance(_v);
                                TranceSeekCustomAPI.TryCriticalHit(_v);
                                TranceSeekCustomAPI.IpsenCastleMalus(_v);
                                _v.Target.Flags |= CalcFlag.HpAlteration;
                                _v.Caster.Flags |= CalcFlag.HpAlteration;
                                if (_v.Target.IsZombie)
                                {
                                    _v.Target.Flags |= CalcFlag.HpRecovery;
                                }
                                else
                                {
                                    _v.Caster.Flags |= CalcFlag.HpRecovery;
                                }
                                uint currentHp = _v.Target.CurrentHp;
                                _v.CalcPhysicalHpDamage();
                                if (_v.Caster.IsPlayer)
                                {
                                    _v.Caster.HpDamage = _v.Target.HpDamage / 4;
                                }
                                else
                                {
                                    _v.TryAlterMagicStatuses();
                                    if (_v.Target.HpDamage < currentHp)
                                    {
                                        _v.Caster.HpDamage = _v.Target.HpDamage;
                                    }
                                    else
                                    {
                                        _v.Caster.HpDamage = (int)currentHp;
                                    }
                                }
                            }
                        }
                        break;
                    }
                    // ENEMY PHYSICAL ATTACK - Script 8
                    case (BattleAbilityId)1184: // Ao
                    case (BattleAbilityId)1186: // Plant Spider
                    case (BattleAbilityId)1188: // Carve Spider
                    case (BattleAbilityId)1193: // Blazer Beetle
                    case (BattleAbilityId)1206: // Grand Dragon - Poison Claw
                    case (BattleAbilityId)1210: // Cerberus - Incandescent Claws
                    {
                        if (!_v.Target.TryKillFrozen())
                        {
                            _v.PhysicalAccuracy();
                            if (TranceSeekCustomAPI.TryPhysicalHit(_v))
                            {
                                _v.NormalPhysicalParams();
                                TranceSeekCustomAPI.CharacterBonusPassive(_v, "PhysicalAttack");
                                TranceSeekCustomAPI.EnemyTranceBonusAttack(_v);
                                TranceSeekCustomAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                                TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                                if (_v.Command.HitRate != 101)
                                {
                                    TranceSeekCustomAPI.BonusBackstabAndPenaltyLongDistance(_v);
                                }
                                TranceSeekCustomAPI.BonusElement(_v);
                                if (_v.CanAttackElementalCommand())
                                {
                                    if (_v.Command.HitRate == 224) // Contre-attaque avec Critique
                                    {
                                        _v.Context.Attack *= 2;
                                        _v.Target.Flags |= CalcFlag.Critical;
                                    }
                                    else
                                    {
                                        TranceSeekCustomAPI.TryCriticalHit(_v);
                                    }
                                    _v.CalcPhysicalHpDamage();
                                    TranceSeekCustomAPI.InfusedWeaponStatus(_v);
                                    TranceSeekCustomAPI.RaiseTrouble(_v);
                                    _v.TryAlterMagicStatuses();
                                }                               
                            }
                        }
                        break;
                    }
                    // MAGIC ATTACK - Script 9
                    case (BattleAbilityId)1172: // Agares
                    case (BattleAbilityId)1176: // Amanite
                    case (BattleAbilityId)1180: // Amduscias
                    case (BattleAbilityId)1182: // Anemone
                    case (BattleAbilityId)1187: // Plant Spider
                    case (BattleAbilityId)1192: // Blazer Beetle
                    case (BattleAbilityId)1194: // Axolotl
                    case (BattleAbilityId)1197: // Bandersnatch
                    case (BattleAbilityId)1200: // Mistodon
                    case (BattleAbilityId)1201: // Mistodon
                    case (BattleAbilityId)1205: // Armstrong - Tsunami
                    case (BattleAbilityId)1207: // Grand Dragon - Dragon Breath
                    case (BattleAbilityId)1209: // Shell Dragon - Disaster
                    case (BattleAbilityId)1211: // Cerberus - Flame
                    case (BattleAbilityId)1216: // Zuu - Aera
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
                            if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)102))
                                TranceSeekCustomAPI.TryCriticalHit(_v);
                            _v.CalcHpDamage();
                            TranceSeekCustomAPI.RaiseTrouble(_v);
                        }
                        _v.TryAlterMagicStatuses();
                        break;
                    }
                    // BAD STATUS - Script 11
                    case (BattleAbilityId)1175: // Ahriman
                    case (BattleAbilityId)1183: // Anemone
                    case (BattleAbilityId)1189: // Carve Spider
                    case (BattleAbilityId)1195: // Axolotl
                    case (BattleAbilityId)1196: // Bandersnatch
                    case (BattleAbilityId)1198: // Basilisk
                    case (BattleAbilityId)1199: // Basilisk
                    case (BattleAbilityId)1213: // Seeker Bat - Darkness
                    {
                        TranceSeekCustomAPI.MagicAccuracy(_v);
                        TranceSeekCustomAPI.ViviFocus(_v);
                        _v.Target.PenaltyShellHitRate();
                        _v.PenaltyCommandDividedHitRate();
                        if (_v.Command.AbilityId == (BattleAbilityId)1175) // Ahriman - Blaster
                        {
                            if (_v.Target.CheckUnsafetyOrGuard())
                            {
                                if (_v.TryMagicHit())
                                {
                                    TranceSeekCustomAPI.TryAlterCommandStatuses(_v);
                                }
                                else
                                {
                                    uint num = (uint)(1 + GameRandom.Next8() % 9);
                                    if (_v.Target.CurrentHp == 1U)
                                    {
                                        _v.Context.Flags |= BattleCalcFlags.Miss;
                                    }
                                    else
                                    {
                                        if (_v.Target.CurrentHp < num)
                                        {
                                            _v.Target.CurrentHp = (uint)(1L + GameRandom.Next8() % num);
                                        }
                                        else
                                        {
                                            _v.Target.CurrentHp = num;
                                        }
                                        _v.Context.Flags = 0;
                                    }
                                }
                            }
                            break;
                        }
                        if (_v.TryMagicHit() || _v.Command.HitRate == 255)
                            TranceSeekCustomAPI.TryAlterCommandStatuses(_v);
                        break;
                    }
                    // WHITE WIND - Script 30
                    case (BattleAbilityId)1217: // Zuu - White Wind
                    {
                        if (_v.Target.Data == _v.Caster.Data)
                        {
                            _v.Caster.HpDamage = (int)_v.Caster.CurrentHp;
                            if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)100)) // Medecin
                            {
                                _v.Caster.HpDamage += _v.Caster.HpDamage / 4;
                            }
                            else if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1100)) // Medecin +
                            {
                                _v.Caster.HpDamage += _v.Caster.HpDamage / 2;
                            }
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
                                if (unit.Data == _v.Caster.Data)
                                    SummonStep[_v.Caster.Data] = 2;
                            }
                            _v.Caster.Flags = 0;
                            _v.Caster.HpDamage = 0;
                            _v.PerformCalcResult = false;
                        }
                        return;
                    }
                    // ARMOR BREAK - Script 33
                    case (BattleAbilityId)1178: // Amazone
                    {
                        if (!_v.Target.TryKillFrozen())
                        {
                            if (_v.Target.PhysicalDefence == 255)
                            {
                                _v.Context.Flags |= BattleCalcFlags.Guard;
                                break;
                            }
                            if (_v.Target.IsUnderAnyStatus(BattleStatus.Vanish) || _v.Target.PhysicalEvade == 255)
                            {
                                _v.Context.Flags |= BattleCalcFlags.Miss;
                                break;
                            }

                            if (_v.Caster.IsPlayer)
                            {
                                _v.WeaponPhysicalParams();
                                TranceSeekCustomAPI.CharacterBonusPassive(_v, "LowPhysicalAttack");
                            }
                            else
                            {
                                _v.NormalPhysicalParams();
                            }
                            TranceSeekCustomAPI.MagicAccuracy(_v);
                            TranceSeekCustomAPI.EnemyTranceBonusAttack(_v);
                            TranceSeekCustomAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                            TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                            TranceSeekCustomAPI.BonusElement(_v);
                            _v.CalcHpDamage();
                            _v.Command.AbilityStatus |= TranceSeekCustomAPI.CustomStatus.ArmorBreak;
                            _v.TryAlterMagicStatuses();
                        }
                        break;
                    }
                    // MENTAL BREAK - Script 35
                    case (BattleAbilityId)1179: // Amazone
                    {
                        if (!_v.Target.TryKillFrozen())
                        {
                            if (_v.Target.PhysicalDefence == 255)
                            {
                                _v.Context.Flags |= BattleCalcFlags.Guard;
                                break;
                            }
                            if (_v.Target.IsUnderAnyStatus(BattleStatus.Vanish) || _v.Target.PhysicalEvade == 255)
                            {
                                _v.Context.Flags |= BattleCalcFlags.Miss;
                                break;
                            }

                            if (_v.Caster.IsPlayer)
                            {
                                _v.WeaponPhysicalParams();
                                TranceSeekCustomAPI.CharacterBonusPassive(_v, "LowPhysicalAttack");
                            }
                            else
                            {
                                _v.NormalPhysicalParams();
                            }
                            TranceSeekCustomAPI.MagicAccuracy(_v);
                            TranceSeekCustomAPI.EnemyTranceBonusAttack(_v);
                            TranceSeekCustomAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                            TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                            TranceSeekCustomAPI.BonusElement(_v);
                            _v.CalcHpDamage();
                            _v.Command.AbilityStatus |= TranceSeekCustomAPI.CustomStatus.MentalBreak;
                            _v.TryAlterMagicStatuses();
                        }
                        break;
                    }
                    // CANNON - Script 91
                    case (BattleAbilityId)1204: // Armstrong - Cannon
                    {
                        _v.PhysicalAccuracy();
                        if (TranceSeekCustomAPI.TryPhysicalHit(_v))
                        {
                            _v.NormalPhysicalParams();
                            TranceSeekCustomAPI.CharacterBonusPassive(_v, "PhysicalAttack");
                            TranceSeekCustomAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                            TranceSeekCustomAPI.EnemyTranceBonusAttack(_v);
                            TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                            if (Mathf.Abs((_v.Caster.Row - _v.Target.Row)) > 1)
                            {
                                _v.Context.Attack = _v.Context.Attack * 3 >> 1;
                            }
                            TranceSeekCustomAPI.BonusElement(_v);
                            if (_v.CanAttackElementalCommand())
                            {
                                _v.CalcPhysicalHpDamage();
                                TranceSeekCustomAPI.RaiseTrouble(_v);
                                _v.TryAlterMagicStatuses();
                            }
                        }
                        break;
                    }
                    // MAELSTORM - Script 93
                    case (BattleAbilityId)1181: // Amduscias
                    {
                        if (_v.Target.CheckUnsafetyOrGuard() && _v.Target.CanBeAttacked())
                        {
                            TranceSeekCustomAPI.MagicAccuracy(_v);
                            _v.Target.PenaltyShellHitRate();
                            _v.PenaltyCommandDividedHitRate();
                            _v.Command.HitRate += (byte)(_v.Caster.Level - _v.Target.Level);
                            if (TranceSeekCustomAPI.TryMagicHitWithoutBattleCalcFlag(_v) || _v.Command.Power == 255)
                            {
                                _v.Context.Flags |= BattleCalcFlags.DirectHP;
                                if (_v.Target.CurrentHp < 10U)
                                {
                                    _v.Target.CurrentHp = (uint)(1L + GameRandom.Next8() % _v.Target.CurrentHp);
                                }
                                else
                                {
                                    _v.Target.CurrentHp = (uint)(1 + GameRandom.Next8() % 9);
                                }
                                if (_v.Command.AbilityStatus > 0)
                                {
                                    if (_v.Command.HitRate == 255)
                                    {
                                        _v.Target.TryAlterStatuses(_v.Command.AbilityStatus, false, _v.Caster);
                                    }
                                    else
                                    {
                                        _v.TryAlterMagicStatuses();
                                    }
                                }
                            }
                            else
                            {
                                _v.Context.Flags |= BattleCalcFlags.Miss;
                            }
                        }
                        break;
                    }
                    // PRECISE PHYSICAL ATTACK - Script 100
                    case (BattleAbilityId)1173: // Agares
                    case (BattleAbilityId)1208: // Shell Dragon - Charge
                    {
                        if (!_v.Target.TryKillFrozen())
                        {
                            if (_v.Command.HitRate == 111) // Ignore physical defense
                            {
                                _v.SetCommandPower();
                                _v.Caster.SetPhysicalAttack();
                            }
                            else
                            {
                                _v.NormalPhysicalParams();
                                TranceSeekCustomAPI.CharacterBonusPassive(_v, "PhysicalAttack");
                            }
                            TranceSeekCustomAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                            TranceSeekCustomAPI.EnemyTranceBonusAttack(_v);
                            TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                            if (_v.Command.HitRate != 255)
                            {
                                TranceSeekCustomAPI.BonusBackstabAndPenaltyLongDistance(_v);
                            }
                            TranceSeekCustomAPI.BonusElement(_v);
                            if (_v.CanAttackElementalCommand())
                            {
                                TranceSeekCustomAPI.RaiseTrouble(_v);
                                _v.CalcHpDamage();
                                TranceSeekCustomAPI.InfusedWeaponStatus(_v);
                                _v.TryAlterMagicStatuses();
                            }
                        }
                        break;
                    }
                    // MAGIC APPLY POSITIVE - Script 103
                    case (BattleAbilityId)1185: // Ao
                    {
                        TranceSeekCustomAPI.TryAlterCommandStatuses(_v);
                        break;
                    }
                    // LOW RANDOM MAGIC - Script 116
                    case (BattleAbilityId)1174: // Ahriman
                    {
                        _v.Context.Attack = UnityEngine.Random.Range(((_v.Caster.Magic + _v.Caster.Level) / 3), (_v.Caster.Magic + _v.Caster.Level));
                        _v.SetCommandPower();
                        _v.Target.SetMagicDefense();
                        TranceSeekCustomAPI.CasterPenaltyMini(_v);
                        TranceSeekCustomAPI.EnemyTranceBonusAttack(_v);
                        TranceSeekCustomAPI.PenaltyShellAttack(_v);
                        TranceSeekCustomAPI.PenaltyCommandDividedAttack(_v);
                        TranceSeekCustomAPI.BonusElement(_v);
                        if (TranceSeekCustomAPI.CanAttackMagic(_v))
                        {
                            _v.CalcHpDamage();
                        }
                        _v.TryAlterMagicStatuses();
                        break;
                    }
                    // POISON MAGIC - Script 118
                    case (BattleAbilityId)1177: // Amanite
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
                            if (_v.Target.HasCategory(EnemyCategory.Humanoid))
                            {
                                _v.Context.Attack = _v.Context.Attack * 2;
                            }
                            if (_v.Target.IsZombie)
                            {
                                _v.Target.Flags |= CalcFlag.HpRecovery;
                            }
                            if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)102))
                                TranceSeekCustomAPI.TryCriticalHit(_v);
                            _v.CalcHpDamage();
                        }
                        _v.TryAlterMagicStatuses();
                        break;
                    }
                    case (BattleAbilityId)1243: // Cactuar
                    {
                        _v.Command.Power = 72;
                        _v.Command.Element = EffectElement.Thunder;
                        _v.NormalMagicParams();
                        TranceSeekCustomAPI.CharacterBonusPassive(_v, "MagicAttack");
                        TranceSeekCustomAPI.CasterPenaltyMini(_v);
                        TranceSeekCustomAPI.EnemyTranceBonusAttack(_v);
                        TranceSeekCustomAPI.PenaltyShellAttack(_v);
                        TranceSeekCustomAPI.PenaltyCommandDividedAttack(_v);
                        TranceSeekCustomAPI.BonusElement(_v);
                        if (TranceSeekCustomAPI.CanAttackMagic(_v))
                        {
                            if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)102))
                                TranceSeekCustomAPI.TryCriticalHit(_v);
                            _v.CalcHpDamage();
                            TranceSeekCustomAPI.RaiseTrouble(_v);
                        }
                        _v.TryAlterMagicStatuses();
                        break;
                    }
                    case (BattleAbilityId)1244: // Ozma
                    {
                        _v.Command.Power = 115;
                        _v.Context.Attack = GameRandom.Next16() % (_v.Caster.Magic + 99);
                        _v.SetCommandPower();
                        _v.Context.DefensePower = 0;
                        TranceSeekCustomAPI.CasterPenaltyMini(_v);
                        TranceSeekCustomAPI.EnemyTranceBonusAttack(_v);
                        TranceSeekCustomAPI.PenaltyShellAttack(_v);
                        TranceSeekCustomAPI.PenaltyCommandDividedAttack(_v);
                        TranceSeekCustomAPI.BonusElement(_v);
                        if (TranceSeekCustomAPI.CanAttackMagic(_v))
                        {
                            _v.CalcHpDamage();
                        }
                        _v.TryAlterMagicStatuses();
                        break;
                    }
                }

                NumberTargets[_v.Caster.Data]--;
                if (NumberTargets[_v.Caster.Data] <= 0)
                    SummonStep[_v.Caster.Data] = 2;
            }
            else if (SummonStep[_v.Caster.Data] == 2) // Turn back to the character
            {
                PLAYER player = _v.Caster.Player;
                BattlePlayerCharacter.PlayerMotionIndex motion = btl_mot.getMotion(_v.Caster);
                if (motion == BattlePlayerCharacter.PlayerMotionIndex.MP_MAX)
                    motion = BattlePlayerCharacter.PlayerMotionIndex.MP_IDLE_NORMAL;
                BattlePlayerCharacter.CreatePlayer(_v.Caster, player);
                btl_mot.SetPlayerDefMotion(_v.Caster, player.info.serial_no);
                BattlePlayerCharacter.InitAnimation(_v.Caster);
                if (_v.Caster.IsUnderAnyStatus(BattleStatus.Trance))
                    _v.Caster.Data.ChangeModel(_v.Caster.Data.tranceGo, btl_init.GetModelID(player.info.serial_no, true));
                else
                    _v.Caster.Data.ChangeModel(_v.Caster.Data.originalGo, btl_init.GetModelID(player.info.serial_no, false));
                btl_mot.setMotion(_v.Caster, motion);
                _v.Caster.Data.gameObject.transform.localPosition = InitPosition[_v.Caster.Data];
                btl_eqp.InitWeapon(_v.Caster.Player, _v.Caster.Data);
                _v.Caster.Data.weapon_geo.SetActive(true);
                _v.Caster.Data.flags &= (ushort)~geo.GEO_FLAGS_CLIP;
                _v.Caster.MaxDamageLimit = 9999;
                SummonStep[_v.Caster.Data]++;
            }

        }

        public static Dictionary<Int32, short> GeoMonsterWithAA = new Dictionary<Int32, short>()
        {
            { 1172, 328 }, // Agares
            { 1173, 328 },
            { 1174, 261 }, // Ahriman
            { 1175, 261 },
            { 1176, 143 }, // Amanite
            { 1177, 143 },
            { 1178, 591 }, // Amazone
            { 1179, 591 },
            { 1180, 325 }, // Amdusias
            { 1181, 325 },
            { 1182, 77 }, // Anemone
            { 1183, 77 },
            { 1184, 324 }, // Adamantoise
            { 1185, 324 },
            { 1186, 345 }, // Plant Spider
            { 1187, 345 },
            { 1188, 5460 }, // Carve Spider
            { 1189, 5460 },
            { 1190, 147 }, // Ash
            { 1191, 147 },
            { 1192, 84 }, // Blazer Beetle
            { 1193, 84 },
            { 1194, 86 }, // Axolotl
            { 1195, 86 },
            { 1196, 145 }, // Bandersnatch
            { 1197, 145 },
            { 1198, 5458 }, // Basilisk
            { 1199, 5458 },
            { 1200, 556 }, // Mistodon
            { 1201, 556 },
            { 1202, 57 }, // Yan
            { 1203, 57 },
            { 1204, 334 }, // Armstrong
            { 1205, 334 },
            { 1206, 355 }, // Grand Dragon
            { 1207, 355 },
            { 1208, 338 }, // Shell Dragon
            { 1209, 338 },
            { 1210, 89 }, // Cerberus
            { 1211, 89 },
            { 1212, 153 }, // Seeker Bat
            { 1213, 153 },
            { 1214, 337 }, // Chimera
            { 1215, 337 },
            { 1216, 87 }, // Zuu
            { 1217, 87 },
            { 1218, 451 }, // Crawler
            { 1219, 451 },
            { 1220, 159 }, // Ironite
            { 1221, 159 },
            { 1222, 165 }, // Dracozombie
            { 1223, 165 },
            { 1224, 330 }, // Dragonfly
            { 1225, 330 },
            { 1226, 343 }, // Iron Man
            { 1227, 343 },
            { 1228, 158 }, // Mu
            { 1229, 158 },
            { 1230, 93 }, // Catoblepas
            { 1231, 93 },
            { 1232, 78 }, // Whale Zombie
            { 1233, 78 },
            { 1234, 400 }, // Ghost
            { 1235, 400 },
            { 1236, 154 }, // Flan
            { 1237, 154 },
            { 1238, 350 }, // Antlion
            { 1239, 350 },
            { 1240, 83 }, // Gargoyle
            { 1241, 83 },
            { 1242, 82 }, // Garuda
            { 1243, 82 },
            { 1244, 146 }, // Land Worm
            { 1245, 146 },
            { 1246, 369 }, // Gimme cat
            { 1247, 369 },
            { 1248, 46 }, // Goblin Mage
            { 1249, 46 },
            { 1250, 152 }, // Goblin
            { 1251, 152 },
            { 1252, 339 }, // Sand Golem
            { 1253, 339 },
            { 1254, 333 }, // Grenade
            { 1255, 333 },
            { 1256, 90 }, // Griffin
            { 1257, 90 },
            { 1258, 379 }, // Grimlock
            { 1259, 379 },
            { 1260, 137 }, // Gigan Toad
            { 1261, 137 },
            { 1262, 166 }, // Hecteyes
            { 1263, 166 },
            { 1264, 341 }, // Abadon
            { 1265, 341 },
            { 1266, 332 }, // Jabberwock
            { 1267, 332 },
            { 1268, 135 }, // Vice
            { 1269, 135 },
            { 1270, 5459 }, // Lizard Man
            { 1271, 5459 },
            { 1272, 620 }, // Lich?
            { 1273, 620 },
            { 1274, 336 }, // Behemoth
            { 1275, 336 },
            { 1276, 339 }, // Core
            { 1277, 339 },
            { 1278, 618 }, // Kraken?
            { 1279, 618 },
            { 1280, 150 }, // Clipper
            { 1281, 150 },
            { 1282, 265 }, // Magic Vice
            { 1283, 265 },
            { 1284, 88 }, // Serpion
            { 1285, 88 },
            { 1286, 157 }, // Wyerd
            { 1287, 157 },
            { 1288, 161 }, // Mandragora
            { 1289, 161 },
            { 1290, 144 }, // Feather Circle
            { 1291, 144 },
            { 1292, 164 }, // Torama
            { 1293, 164 },
            { 1294, 151 }, // Fang
            { 1295, 151 },
            { 1296, 619 }, // Maliris?
            { 1297, 619 },
            { 1298, 266 }, // Mimic
            { 1299, 266 },
            { 1300, 136 }, // Ladybug
            { 1301, 136 },
            { 1302, 9 }, // Trick Sparrow
            { 1303, 9 },
            { 1304, 28 }, // Hornet
            { 1305, 28 },
            { 1306, 148 }, // Drakan
            { 1307, 148 },
            { 1308, 58 }, // Dendrobium
            { 1309, 58 },
            { 1310, 92 }, // Gnoll
            { 1311, 92 },
            { 1312, 548 }, // Nymph
            { 1313, 548 },
            { 1314, 327 }, // Ogre
            { 1315, 327 },
            { 1316, 244 }, // Cactuar
            { 1317, 244 },
            { 1318, 141 }, // Abomination
            { 1319, 141 },
            { 1320, 97 }, // Axe Beak
            { 1321, 97 },
            { 1322, 149 }, // Wraith (blue)
            { 1323, 149 },
            { 1324, 399 }, // Wraith (red)
            { 1325, 399 },
            { 1326, 163 }, // Zaghnol
            { 1327, 163 },
            { 1328, 79 }, // Gigan Octopus
            { 1329, 79 },
            { 1330, 160 }, // Carrion Worm
            { 1331, 160 },
            { 1332, 60 }, // Python
            { 1333, 60 },
            { 1334, 85 }, // Lamia
            { 1335, 85 },
            { 1336, 610 }, // Red Dragon
            { 1337, 610 },
            { 1338, 138 }, // Sahagin
            { 1339, 138 },
            { 1340, 81 }, // Sand Scorpion
            { 1341, 81 },
            { 1342, 94 }, // Ring Leader
            { 1343, 94 },
            { 1344, 546 }, // Cave Imp
            { 1345, 546 },
            { 1346, 162 }, // Skeleton
            { 1347, 162 },
            { 1348, 342 }, // Stilva
            { 1349, 342 },
            { 1350, 142 }, // Stroper
            { 1351, 142 },
            { 1352, 155 }, // Bomb
            { 1353, 155 },
            { 1354, 80 }, // Zemzelett
            { 1355, 80 },
            { 1356, 59 }, // Hedgehog Pie
            { 1357, 59 },
            { 1358, 621 }, // Tiamat?
            { 1359, 621 },
            { 1360, 184 }, // Tonberry
            { 1361, 184 },
            { 1362, 95 }, // Ochu
            { 1363, 95 },
            { 1364, 326 }, // Troll
            { 1365, 326 },
            { 1366, 331 }, // Vepal (ice)
            { 1367, 331 },
            { 1368, 676 }, // Vepal (fire)
            { 1369, 676 },
            { 1370, 340 }, // Worm Hydra
            { 1371, 340 },
            { 1372, 96 }, // Malboro
            { 1373, 96 },
            { 1374, 329 }, // Yeti
            { 1375, 329 },
        };
    }
}

