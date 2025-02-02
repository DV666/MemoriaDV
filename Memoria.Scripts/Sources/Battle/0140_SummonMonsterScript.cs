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
                    // ENEMY PHYSICAL ATTACK - Script 8
                    case (BattleAbilityId)1184: // Ao
                    case (BattleAbilityId)1186: // Plant Spider
                    case (BattleAbilityId)1188: // Carve Spider
                    case (BattleAbilityId)1193: // Blazer Beetle
                    case (BattleAbilityId)1206: // Grand Dragon - Poison Claw
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
                btl_mot.SetPlayerDefMotion(_v.Caster, player.info.serial_no, (UInt32)_v.Caster.GetIndex());
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
            { 1243, 244 }, // Cactuar
            { 1244, 1 }, // Ozma
        };
    }
}

