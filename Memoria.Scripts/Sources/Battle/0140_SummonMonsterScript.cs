using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Assets;
using Memoria.Data;
using Memoria.Prime;
using Memoria.Prime.PsdFile;
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
            if (_v.Command.Id == (BattleCommandId)1029) // Kutrol
            {
                int AbilityId = GeoMonsterWithAA.FirstOrDefault(x => x.Value == _v.Target.Data.dms_geo_id).Key;
                AbilityId = ff9abil.GetAbilityIdFromActiveAbility((BattleAbilityId)AbilityId);

                if (_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill) || ff9abil.FF9Abil_IsMaster(_v.Caster.Player, AbilityId))
                {
                    _v.Context.Flags |= BattleCalcFlags.Miss;
                }
                else
                {

                    int RatioHP = (int)(100 - ((_v.Target.CurrentHp * 100) / _v.Target.MaximumHp));
                    int BonusStatus = 100;
                    foreach (BattleStatusId statusId in (_v.Target.Data.stat.cur & BattleStatusConst.AnyNegative).ToStatusList())
                    {
                        BonusStatus += 10;
                    }
                    RatioHP = (RatioHP * BonusStatus) / 100;
                    int CaptureRate = Math.Max(0, RatioHP - _v.Target.Level) / BattleState.TargetCount(false);
                    if (CaptureRate > Comn.random16() % 100)
                    {
                        ff9abil.FF9Abil_SetMaster(_v.Caster.Player, AbilityId);
                        UIManager.Battle.SetBattleFollowMessage(3, Localization.GetWithDefault("KutrolSucess"));
                        _v.Target.Kill(_v.Caster);
                    }
                    else
                    {
                        _v.Context.Flags |= BattleCalcFlags.Miss;
                    }
                }
                return;
            }

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
                    SummonStep[caster.Data] = 0;
                }
            );

            if (_v.Caster.CurrentHp == 0) // Reset model if character die from reflected spell for example.
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
                // _v.Caster.MaxDamageLimit = 9999;
                return;
            }

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
                Animation animation = _v.Caster.Data.gameObject.GetComponent<Animation>();

                if (IDMonster == 244 || _v.Command.AbilityId == (BattleAbilityId)1259 || _v.Command.AbilityId == (BattleAbilityId)1284)
                { // Cactuar, Grimlock (Blue for Stop), Serpion (Stab) (idle_alternate)
                    if (animation.GetClip("ANH_" + geoName + "_001") == null)
                        AnimationFactory.AddAnimWithAnimatioName(_v.Caster.Data.gameObject, "ANH_" + geoName + "_001");
                    for (Int32 i = 0; i < 34; i++)
                        _v.Caster.Data.mot[i] = "ANH_" + geoName + "_001";
                }
                else
                {
                    if (animation.GetClip("ANH_" + geoName + "_000") == null)
                        AnimationFactory.AddAnimWithAnimatioName(_v.Caster.Data.gameObject, "ANH_" + geoName + "_000");
                    for (Int32 i = 0; i < 34; i++)
                        _v.Caster.Data.mot[i] = "ANH_" + geoName + "_000";
                }
                btl_mot.setMotion(_v.Caster.Data, BattlePlayerCharacter.PlayerMotionIndex.MP_IDLE_NORMAL);
                // No M-Sword => Multiple (ennemy) / Short => Multiple (ally) / No M-Sword + Short => Everyone / Short + Returnable => Self
                NumberTargets[_v.Caster.Data] = ((_v.Command.AbilityCategory & 32) != 0 && (_v.Command.AbilityCategory & 128) != 0) ? 1 : ((_v.Command.AbilityCategory & 4) != 0 && (_v.Command.AbilityCategory & 32) != 0 ? (BattleState.TargetCount(true) + BattleState.TargetCount(false)) : ((_v.Command.AbilityCategory & 4) != 0 ? BattleState.TargetCount(false) : (_v.Command.AbilityCategory & 32) != 0 ? BattleState.TargetCount(true) : 1));
                SummonStep[_v.Caster.Data] = 1;

                if (FF9StateSystem.EventState.gScriptDictionary.TryGetValue(1030, out Dictionary<Int32, Int32> dict)) // Increase AA utilisation, for MPCost in AbilityFeatures.txt
                    dict[(int)_v.Command.AbilityId]++;

                if (_v.Command.AbilityId == (BattleAbilityId)1255) // Grenada - Fire balls
                    NumberTargets[_v.Caster.Data] = 6;
                else if (_v.Command.AbilityId == (BattleAbilityId)1263 || _v.Command.AbilityId == (BattleAbilityId)1377) // Hecteyes and Zombie - Roulette
                    NumberTargets[_v.Caster.Data] = 1;
                else if (_v.Command.AbilityId == (BattleAbilityId)1315) // Ogre - Double Knife
                    NumberTargets[_v.Caster.Data] = 2;

                // Custom Textures
                if (_v.Command.AbilityId == (BattleAbilityId)1378 || _v.Command.AbilityId == (BattleAbilityId)1379)
                {
                    //ModelFactory.ChangeModelTexture(_v.Caster.Data.gameObject, new string[] {"CustomTextures/OeilvertGuardian/342_0.png", "CustomTextures/OeilvertGuardian/342_1.png", "CustomTextures/OeilvertGuardian/342_2.png", "CustomTextures/OeilvertGuardian/342_3.png", "CustomTextures/ OeilvertGuardian/342_4.png", "CustomTextures/OeilvertGuardian/342_5.png"});
                }
            }
            else if (SummonStep[_v.Caster.Data] == 1) // Script for the damage
            {
                switch (_v.Command.AbilityId)
                {
                    // BLOOD SWORD WEAPON - Script 6
                    case (BattleAbilityId)1212: // Seeker Bat - Absorb even more
                    case (BattleAbilityId)1328: // Gigan Octopus - 6 Legs
                    {
                        if (_v.Target.CanBeAttacked() && !_v.Target.TryKillFrozen())
                        {
                            _v.PhysicalAccuracy();
                            if (TranceSeekAPI.TryPhysicalHit(_v))
                            {
                                _v.NormalPhysicalParams();
                                TranceSeekAPI.EnemyTranceBonusAttack(_v);
                                TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                                TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                                TranceSeekAPI.BonusBackstabAndPenaltyLongDistance(_v);
                                TranceSeekAPI.TryCriticalHit(_v);
                                TranceSeekAPI.IpsenCastleMalus(_v);
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
                                    TranceSeekAPI.TryAlterMagicStatuses(_v);
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
                    case (BattleAbilityId)1218: // Crawler - Claws
                    case (BattleAbilityId)1219: // Crawler - Stomach
                    case (BattleAbilityId)1222: // DracoZombie - Putrids claws
                    case (BattleAbilityId)1228: // Mu - Tail
                    case (BattleAbilityId)1230: // Catoblepas - Heave
                    case (BattleAbilityId)1236: // Flan - Head Attack
                    case (BattleAbilityId)1240: // Gargoyle - Charge
                    case (BattleAbilityId)1248: // Goblin Mage - Axe
                    case (BattleAbilityId)1250: // Goblin - Knife
                    case (BattleAbilityId)1252: // Golem - Counter
                    case (BattleAbilityId)1258: // Grimlock - The Drop
                    case (BattleAbilityId)1268: // Vice - Slice
                    case (BattleAbilityId)1274: // Behemoth - Heave
                    case (BattleAbilityId)1280: // Clipper - Crush
                    case (BattleAbilityId)1284: // Serpion - Stab
                    case (BattleAbilityId)1287: // Wyerd - Strike
                    case (BattleAbilityId)1290: // Feather Circle - Trouble Tail
                    case (BattleAbilityId)1294: // Fang - Rush
                    case (BattleAbilityId)1295: // Fang - Fang
                    case (BattleAbilityId)1300: // Ladybug - Spear
                    case (BattleAbilityId)1302: // Trick Sparrow - Beak
                    case (BattleAbilityId)1304: // Hornet - Stinger
                    case (BattleAbilityId)1311: // Gnoll - Gnoll Attack
                    case (BattleAbilityId)1312: // Nymph - Vicious root
                    case (BattleAbilityId)1314: // Ogre - Trouble Knife
                    case (BattleAbilityId)1315: // Ogre - Double Hit
                    case (BattleAbilityId)1318: // Abomination - Silent Slap
                    case (BattleAbilityId)1320: // Axe Beak - Beak
                    case (BattleAbilityId)1326: // Zaghnol - Heave
                    case (BattleAbilityId)1329: // Gigan Octopus - Ink
                    case (BattleAbilityId)1332: // Python - Fira
                    case (BattleAbilityId)1340: // Sand Scorpion - Claws
                    case (BattleAbilityId)1344: // Cave Imp - Rusty Knife
                    case (BattleAbilityId)1347: // Skeleton - Whirl Slash
                    case (BattleAbilityId)1356: // Hedgehog Pie - Ram
                    case (BattleAbilityId)1362: // Ochu - Thorn Whip
                    case (BattleAbilityId)1375: // Yeti - Blind Tail
                    case (BattleAbilityId)1378: // TEST
                    {
                        if (!_v.Target.TryKillFrozen())
                        {
                            _v.PhysicalAccuracy();
                            if (TranceSeekAPI.TryPhysicalHit(_v))
                            {
                                _v.NormalPhysicalParams();
                                TranceSeekAPI.CharacterBonusPassive(_v, "PhysicalAttack");
                                TranceSeekAPI.EnemyTranceBonusAttack(_v);
                                TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                                TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                                if (_v.Command.HitRate != 101)
                                {
                                    TranceSeekAPI.BonusBackstabAndPenaltyLongDistance(_v);
                                }
                                TranceSeekAPI.BonusElement(_v);
                                if (_v.CanAttackElementalCommand())
                                {
                                    if (_v.Command.HitRate == 224) // Contre-attaque avec Critique
                                    {
                                        _v.Context.DamageModifierCount += 4;
                                        _v.Target.Flags |= CalcFlag.Critical;
                                    }
                                    else
                                    {
                                        TranceSeekAPI.TryCriticalHit(_v);
                                    }
                                    _v.CalcPhysicalHpDamage();
                                    TranceSeekAPI.InfusedWeaponStatus(_v);
                                    TranceSeekAPI.RaiseTrouble(_v);
                                    TranceSeekAPI.TryAlterMagicStatuses(_v);
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
                    case (BattleAbilityId)1220: // Ironite - Fira (Multi)
                    case (BattleAbilityId)1224: // Dragonfly - Charge
                    case (BattleAbilityId)1231: // Catoblepas - Thundaga (Multi)
                    case (BattleAbilityId)1237: // Flan - Blizzard
                    case (BattleAbilityId)1242: // Garuda - Firaga
                    case (BattleAbilityId)1244: // Land Worm - Earthquake
                    case (BattleAbilityId)1254: // Grenada - Molotov
                    case (BattleAbilityId)1256: // Griffon - Aera
                    case (BattleAbilityId)1257: // Griffon - Tempest
                    case (BattleAbilityId)1260: // Gigan Toad - Watera
                    case (BattleAbilityId)1265: // Abadon - High Wind
                    case (BattleAbilityId)1286: // Wyerd - Blizzard
                    case (BattleAbilityId)1288: // Mandragora - Blizzara
                    case (BattleAbilityId)1292: // Torama - Thundara
                    case (BattleAbilityId)1301: // Ladybug - Rainbow
                    case (BattleAbilityId)1308: // Dendrobium - Pollen
                    case (BattleAbilityId)1309: // Dendrobium - Pollen
                    case (BattleAbilityId)1322: // Wraith (Ice) - Devil’s Candle
                    case (BattleAbilityId)1323: // Wraith (Ice) - Frost
                    case (BattleAbilityId)1324: // Wraith (Fire) - Devil’s Candle
                    case (BattleAbilityId)1325: // Wraith (Fire) - Mustard Bomb
                    case (BattleAbilityId)1327: // Zaghnol - Earthquake
                    case (BattleAbilityId)1330: // Carrion Worm - Fira
                    case (BattleAbilityId)1341: // Sand Scorpion - Mustard Bomb
                    case (BattleAbilityId)1343: // Ring Leader - Virus Powder
                    case (BattleAbilityId)1345: // Cave Imp - Sleeping Juice
                    case (BattleAbilityId)1346: // Skeleton - Thunder
                    case (BattleAbilityId)1352: // Bombo - Mustard Bomb
                    case (BattleAbilityId)1363: // Ochu - Floral dance
                    case (BattleAbilityId)1366: // Vepal (Ice) - Blizzaga
                    case (BattleAbilityId)1367: // Vepal (Ice) - Snowstorm
                    case (BattleAbilityId)1368: // Vepal (Fire) - Firaga
                    case (BattleAbilityId)1369: // Vepal (Fire) - Fire Breath
                    case (BattleAbilityId)1371: // Worm Hydra - Aero Breath
                    case (BattleAbilityId)1372: // Malboro - Bio
                    case (BattleAbilityId)1374: // Yeti - Blizzara
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
                            if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)102))
                                TranceSeekAPI.TryCriticalHit(_v);
                            _v.CalcHpDamage();
                            TranceSeekAPI.RaiseTrouble(_v);
                        }
                        TranceSeekAPI.TryAlterMagicStatuses(_v);
                        break;
                    }
                    // MAGIC RECOVERY - Script 10
                    case (BattleAbilityId)1246: // Gimme cat - Curaga
                    case (BattleAbilityId)1277: // Core - Cura
                    case (BattleAbilityId)1281: // Clipper - Armor
                    {
                        if (_v.Command.AbilityId == (BattleAbilityId)1281)
                        {
                            _v.Target.Flags |= CalcFlag.HpAlteration;
                            _v.Target.HpDamage = 9999;
                            return;
                        }
                        _v.NormalMagicParams();
                        TranceSeekAPI.CharacterBonusPassive(_v, "MagicAttack");
                        TranceSeekAPI.CasterPenaltyMini(_v);
                        TranceSeekAPI.EnemyTranceBonusAttack(_v);
                        TranceSeekAPI.PenaltyCommandDividedAttack(_v);
                        if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)102))
                            TranceSeekAPI.TryCriticalHit(_v);
                        _v.CalcHpMagicRecovery();
                        TranceSeekAPI.TryAlterMagicStatuses(_v);
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
                    case (BattleAbilityId)1225: // Dragonfly - Buzz
                    case (BattleAbilityId)1233: // Whale Zombie - Ultra Sound Wave
                    case (BattleAbilityId)1243: // Garuda - Stop
                    case (BattleAbilityId)1259: // Grimlock - Stop
                    case (BattleAbilityId)1261: // Gigan Toad - Glowing Eyes
                    case (BattleAbilityId)1313: // Nymph - Pollen
                    case (BattleAbilityId)1316: // Cactuar - Confuse
                    case (BattleAbilityId)1319: // Abomination - Sleep
                    case (BattleAbilityId)1342: // Ring Leader - Mini
                    case (BattleAbilityId)1351: // Stroper - Petrify
                    case (BattleAbilityId)1360: // Tonberry - Pain
                    case (BattleAbilityId)1373: // Malboro - Bad Breath
                    {
                        TranceSeekAPI.MagicAccuracy(_v);
                        _v.Target.PenaltyShellHitRate();
                        _v.PenaltyCommandDividedHitRate();
                        if (_v.Command.AbilityId == (BattleAbilityId)1175) // Ahriman - Blaster
                        {
                            if (TranceSeekAPI.CheckUnsafetyOrGuard(_v))
                            {
                                if (TranceSeekAPI.TryMagicHit(_v))
                                {
                                    TranceSeekAPI.TryAlterCommandStatuses(_v);
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
                        if (TranceSeekAPI.TryMagicHit(_v) || _v.Command.HitRate == 255)
                            TranceSeekAPI.TryAlterCommandStatuses(_v);
                        break;
                    }
                    // DEATH STATUS - Script 14
                    case (BattleAbilityId)1299: // Mimic - Death
                    case (BattleAbilityId)1306: // Drakan - Death
                    {
                        if (TranceSeekAPI.CheckUnsafetyOrGuard(_v))
                        {
                            if (_v.Target.IsZombie)
                            {
                                if (_v.Target.CanBeAttacked())
                                {
                                    _v.Target.CurrentHp = _v.Target.MaximumHp;
                                }
                            }
                            else
                            {
                                TranceSeekAPI.MagicAccuracy(_v);
                                if (TranceSeekAPI.TryMagicHit(_v) || _v.Command.HitRate == 255)
                                {
                                    TranceSeekAPI.TryAlterCommandStatuses(_v);
                                }
                            }
                        }
                        break;
                    }
                    // DRAIN MP - Script 15
                    case (BattleAbilityId)1235: // Ghost - Osmose
                    {
                        _v.Context.IsDrain = true;
                        if (!_v.IsCasterNotTarget() || !_v.Target.CanBeAttacked())
                            return;

                        _v.NormalMagicParams();
                        TranceSeekAPI.CharacterBonusPassive(_v, "MagicAttack");
                        TranceSeekAPI.CasterPenaltyMini(_v);
                        TranceSeekAPI.EnemyTranceBonusAttack(_v);
                        TranceSeekAPI.PenaltyShellAttack(_v);
                        _v.Target.Flags |= CalcFlag.MpAlteration;
                        _v.Caster.Flags |= CalcFlag.MpAlteration;
                        _v.Context.IsDrain = true;

                        _v.CalcMpDamage();
                        Int32 damage = _v.Target.MpDamage;

                        if (_v.Target.IsZombie)
                        {
                            damage = 0;
                        }
                        else
                        {
                            _v.Caster.Flags |= CalcFlag.MpRecovery;
                            if (damage > _v.Target.CurrentMp)
                                damage = (Int32)_v.Target.CurrentMp;
                        }

                        _v.Target.MpDamage = damage;
                        _v.Caster.MpDamage = damage;
                        break;
                    }
                    // DRAIN HP - Script 16
                    case (BattleAbilityId)1234: // Ghost - Drain
                    case (BattleAbilityId)1303: // Trick Sparrow - Drain
                    case (BattleAbilityId)1307: // Drakan - Mind Blast
                    {
                        if (_v.IsCasterNotTarget() && _v.Target.CanBeAttacked())
                        {
                            uint currentHp = _v.Target.CurrentHp;
                            _v.NormalMagicParams();
                            TranceSeekAPI.CharacterBonusPassive(_v, "MagicAttack");
                            TranceSeekAPI.EnemyTranceBonusAttack(_v);
                            TranceSeekAPI.BonusElement(_v);
                            TranceSeekAPI.CasterPenaltyMini(_v);
                            if (_v.Command.HitRate != 111)
                                TranceSeekAPI.PenaltyShellAttack(_v);
                            if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)102))
                                TranceSeekAPI.TryCriticalHit(_v);
                            if (_v.Command.HitRate == 254)
                            {
                                _v.Caster.Flags |= CalcFlag.HpDamageOrHeal | CalcFlag.MpDamageOrHeal;
                                _v.Target.Flags |= CalcFlag.HpAlteration | CalcFlag.MpAlteration;
                                _v.CalcMpDamage();
                                _v.CalcHpDamage();
                                int num = _v.Target.MpDamage / 2;
                                _v.Caster.HpDamage = _v.Target.HpDamage;
                                if (num > _v.Target.CurrentMp)
                                {
                                    num = (int)_v.Target.CurrentMp;
                                }
                                _v.Target.MpDamage = num;
                                _v.Caster.MpDamage = num;
                                TranceSeekAPI.TryAlterMagicStatuses(_v);
                            }
                            else
                            {
                                if (TranceSeekAPI.CanAttackMagic(_v))
                                {
                                    TranceSeekAPI.PrepareHpDraining(_v);
                                    if (_v.Context.PowerDifference >= 1 && TranceSeekAPI.CanAttackMagic(_v))
                                    {
                                        _v.CalcHpDamage();

                                        if (_v.Target.HpDamage > currentHp)
                                        {
                                            _v.Caster.HpDamage = (int)currentHp;
                                        }
                                        else
                                        {
                                            _v.Caster.HpDamage = _v.Target.HpDamage;
                                        }
                                    }
                                    TranceSeekAPI.TryAlterMagicStatuses(_v);                                
                                }
                            }
                        }
                        break;
                    }
                    // MAGIC GRAVITY - Script 17
                    case (BattleAbilityId)1239: // Antlion - Sandstorm
                    case (BattleAbilityId)1291: // Feather Circle - Trouble Tail
                    case (BattleAbilityId)1370: // Worm Hydra - Venom Breath
                    {
                        SB2_PATTERN sb2Pattern = FF9StateSystem.Battle.FF9Battle.btl_scene.PatAddr[FF9StateSystem.Battle.FF9Battle.btl_scene.PatNum];
                        for (Int32 i = 0; i < MagicGravityDamageScript.ImmuneGravity.GetLength(0); i++)
                        {
                            if (FF9StateSystem.Battle.battleMapIndex == MagicGravityDamageScript.ImmuneGravity[i, 0] && sb2Pattern.Monster[_v.Target.Data.bi.slot_no].TypeNo == MagicGravityDamageScript.ImmuneGravity[i, 1])
                            {
                                _v.Context.Flags = BattleCalcFlags.Guard;
                                return;
                            }
                        }

                        _v.SetCommandAttack();
                        TranceSeekAPI.PenaltyCommandDividedAttack(_v);
                        if (_v.Target.IsUnderStatus(BattleStatus.Shell))
                            _v.Context.DamageModifierCount -= 2;
                        if (_v.Target.HasCategory(EnemyCategory.Stone) && !_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill))
                            _v.Context.DamageModifierCount += 4;
                        TranceSeekAPI.BonusElement(_v);
                        if (TranceSeekAPI.CanAttackMagic(_v))
                        {
                            if (_v.Command.HitRate == 255)
                            {
                                _v.CalcDamageCommon();
                                if (_v.Context.Attack > 100)
                                {
                                    _v.Context.Attack = 100;
                                }
                                int num = (int)(_v.Target.MaximumHp * _v.Context.Attack / 100U);
                                if (_v.Command.IsShortSummon)
                                {
                                    num = num * 2 / 3;
                                }
                                _v.Target.HpDamage = num;
                            }
                            else
                            {
                                _v.CalcCannonProportionDamage();
                            }
                        }
                        if (_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill))
                        {
                            _v.Target.HpDamage = Math.Max(1, (_v.Target.HpDamage / TranceSeekAPI.MonsterMechanic[_v.Target.Data][5]));
                            TranceSeekAPI.MonsterMechanic[_v.Target.Data][5] = TranceSeekAPI.MonsterMechanic[_v.Target.Data][5] * 2;
                        }
                        TranceSeekAPI.TryAlterMagicStatuses(_v);

                        if (TranceSeekAPI.AbsorbElement.TryGetValue(_v.Target.Data, out Int32 elementprotect))
                            if (elementprotect == 256)
                                _v.Target.Flags |= CalcFlag.HpRecovery;
                        break;
                    }
                    // METEORITE (RANDOM MAGIC) - Script 18
                    case (BattleAbilityId)1255: // Grenada - Fire balls
                    case (BattleAbilityId)1275: // Behemoth - Meteor
                    case (BattleAbilityId)1337: // Red Dragon - Twister
                    {
                        _v.Context.Attack = GameRandom.Next16() % (_v.Caster.Magic + _v.Caster.Level);
                        _v.SetCommandPower();
                        _v.Context.DefensePower = 0;
                        TranceSeekAPI.CasterPenaltyMini(_v);
                        TranceSeekAPI.EnemyTranceBonusAttack(_v);
                        TranceSeekAPI.PenaltyShellAttack(_v);
                        TranceSeekAPI.PenaltyCommandDividedAttack(_v);
                        TranceSeekAPI.BonusElement(_v);
                        if (TranceSeekAPI.CanAttackMagic(_v))
                        {
                            _v.CalcHpDamage();
                        }
                        TranceSeekAPI.TryAlterMagicStatuses(_v);
                        break;
                    }
                    // GOBLIN PUNCH - Script 21
                    case (BattleAbilityId)1251: // Goblin - Goblin Punch
                    {
                        if (_v.Target.Level == _v.Caster.Level)
                        {
                            _v.Context.Attack += (int)_v.Caster.Level;
                            _v.Context.DefensePower = 0;
                        }
                        TranceSeekAPI.CasterPenaltyMini(_v);
                        TranceSeekAPI.PenaltyShellAttack(_v);
                        _v.CalcHpDamage();
                        break;
                    }
                    // LVL DIRECT HP DAMAGE - Script 22
                    case (BattleAbilityId)1262: // Hecteyes - LV5 Death
                    {
                        if (_v.IsTargetLevelMultipleOfCommandRate() && _v.Target.CanBeAttacked())
                        {
                            if (_v.Command.Power == 1 && _v.Command.HitRate == 1)
                            {
                                _v.TryDirectHPDamage();
                            }
                            else
                            {
                                if (_v.Command.Power > 0)
                                {
                                    if (_v.Target.MagicDefence == 255)
                                    {
                                        _v.Context.Flags |= BattleCalcFlags.Guard;
                                    }
                                    else
                                    {
                                        _v.NormalMagicParams();
                                        TranceSeekAPI.CharacterBonusPassive(_v, "MagicAttack");
                                        TranceSeekAPI.CasterPenaltyMini(_v);
                                        TranceSeekAPI.PenaltyShellAttack(_v);
                                        TranceSeekAPI.PenaltyCommandDividedAttack(_v);
                                        TranceSeekAPI.BonusElement(_v);
                                        if (TranceSeekAPI.CanAttackMagic(_v))
                                        {
                                            _v.CalcHpDamage();
                                        }
                                        TranceSeekAPI.TryAlterMagicStatuses(_v);
                                    }
                                }
                                else
                                {
                                    _v.TryDirectHPDamage();
                                }
                            }                       
                        }
                        break;
                    }
                    // LV REDUCE DEFENCE - Script 24
                    case (BattleAbilityId)1331: // Carrion Worm - Stomach Acid
                    {
                        if (_v.Command.Power == 0)
                        {
                            if (_v.IsTargetLevelMultipleOfCommandRate() && _v.Target.CanBeAttacked())
                            {
                                _v.Target.AlterStatus(TranceSeekStatus.Vieillissement);
                            }
                            else
                            {
                                _v.Context.Flags |= BattleCalcFlags.Miss;
                            }
                        }
                        else
                        {
                            if (_v.Caster.IsPlayer)
                            {
                                _v.OriginalMagicParams();
                            }
                            else
                            {
                                _v.NormalMagicParams();
                            }
                            TranceSeekAPI.CasterPenaltyMini(_v);
                            _v.Target.PenaltyShellAttack();
                            TranceSeekAPI.PenaltyCommandDividedAttack(_v);
                            TranceSeekAPI.BonusElement(_v);
                            if (TranceSeekAPI.CanAttackMagic(_v))
                            {
                                _v.CalcHpDamage();
                            }
                            TranceSeekAPI.TryAlterMagicStatuses(_v);
                            btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.ArmorBreak, parameters: "+2");
                            btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.MentalBreak, parameters: "+2");
                        }
                        break;
                    }
                    // DIRECT HP DAMAGE - Script 25
                    case (BattleAbilityId)1263: // Hecteyes - Roulette
                    case (BattleAbilityId)1377: // Zombie - Roulette
                    {
                        if (TranceSeekAPI.CheckUnsafetyOrGuard(_v) && _v.Target.CanBeAttacked())
                            _v.TryDirectHPDamage();
                        break;
                    }
                    // THOUNSAND NEEDLES - Script 26
                    case (BattleAbilityId)1317: // Cactuar - 1000 Needles
                    {
                        _v.Target.Flags |= CalcFlag.HpAlteration;
                        _v.Target.HpDamage = (_v.Command.Power * 100 + _v.Command.HitRate);
                        break;
                    }
                    // MINUS STRIKE - Script 29
                    case (BattleAbilityId)1289: // Mandragora - Chestnut
                    {
                        _v.Target.Flags |= CalcFlag.HpAlteration;
                        _v.Target.HpDamage = (Int32)(_v.Caster.MaximumHp - _v.Caster.CurrentHp);
                        break;
                    }
                    // WHITE WIND - Script 30
                    case (BattleAbilityId)1217: // Zuu - White Wind
                    case (BattleAbilityId)1267: // Jabberwock - Psychokinesis
                    case (BattleAbilityId)1354: // Zemzelett - Psychokinesis
                    case (BattleAbilityId)1355: // Zemzelett - White Wind
                    {
                        if (_v.Command.Power == 0)
                        {
                            if (_v.Target.Data == _v.Caster.Data)
                            {
                                int Heal = (int)_v.Caster.CurrentHp;
                                if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)100)) // Medecin
                                    Heal += Heal / (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1100) ? 2 : 4);

                                foreach (BattleUnit unit in BattleState.EnumerateUnits())
                                {
                                    if (!unit.IsPlayer || !unit.IsTargetable || unit.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump))
                                        continue;

                                    if (unit.IsUnderAnyStatus(BattleStatus.Zombie))
                                        Heal = -Heal;

                                    if (Heal > 0)
                                        unit.CurrentHp = (uint)Math.Min(unit.CurrentHp + Heal, unit.MaximumHp);
                                    else
                                        unit.CurrentHp = (uint)Math.Max(unit.CurrentHp + Heal, 0);

                                    btl2d.Btl2dStatReq(unit.Data, -Heal, 0);
                                }
                            }
                        }
                        else
                        {
                            if (_v.Target.MagicDefence == 255)
                            {
                                _v.Context.Flags |= BattleCalcFlags.Guard;
                            }
                            else
                            {
                                _v.NormalMagicParams();
                                TranceSeekAPI.CharacterBonusPassive(_v, "MagicAttack");
                                TranceSeekAPI.CasterPenaltyMini(_v);
                                TranceSeekAPI.PenaltyShellAttack(_v);
                                TranceSeekAPI.PenaltyCommandDividedAttack(_v);
                                TranceSeekAPI.BonusElement(_v);
                                if (TranceSeekAPI.CanAttackMagic(_v))
                                {
                                    if (_v.Target.IsLevitate)
                                        _v.Context.DamageModifierCount += 8;
                                    _v.CalcHpDamage();
                                }
                                TranceSeekAPI.TryAlterMagicStatuses(_v);
                            }
                        }
                        break;
                    }
                    // RANDOM MP DAMAGE - Script 31
                    case (BattleAbilityId)1283: // Magic Vice - Magic Hammer
                    {
                        _v.Target.Flags |= (CalcFlag.MpAlteration);

                        if (_v.Target.IsUnderStatus(BattleStatus.Shell))
                        {
                            _v.Target.MpDamage = (int)(Math.Min(9999, GameRandom.Next16() % (_v.Target.CurrentMp / 2U)));
                        }
                        else
                        {
                            _v.Target.MpDamage = (int)(Math.Min(9999, GameRandom.Next16() % _v.Target.CurrentMp));
                        }
                        break;
                    }
                    // ARMOR BREAK - Script 33
                    case (BattleAbilityId)1178: // Amazone
                    case (BattleAbilityId)1365: // Troll - Itching powder
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
                                TranceSeekAPI.CharacterBonusPassive(_v, "LowPhysicalAttack");
                            }
                            else
                            {
                                _v.NormalPhysicalParams();
                            }
                            TranceSeekAPI.MagicAccuracy(_v);
                            TranceSeekAPI.EnemyTranceBonusAttack(_v);
                            TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                            TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                            TranceSeekAPI.BonusElement(_v);
                            _v.CalcHpDamage();
                            _v.Command.AbilityStatus |= TranceSeekStatus.ArmorBreak;
                            TranceSeekAPI.TryAlterMagicStatuses(_v);
                        }
                        break;
                    }
                    // POWER BREAK - Script 34
                    case (BattleAbilityId)1364: // Troll - Chloroform
                    {
                        if (!_v.Target.TryKillFrozen())
                        {
                            if (_v.Target.PhysicalDefence == 255)
                            {
                                _v.Context.Flags |= BattleCalcFlags.Guard;
                                return;
                            }
                            if (_v.Target.IsUnderAnyStatus(BattleStatus.Vanish) || _v.Target.PhysicalEvade == 255)
                            {
                                _v.Context.Flags |= BattleCalcFlags.Miss;
                                return;
                            }

                            if (_v.Caster.IsPlayer)
                            {
                                _v.WeaponPhysicalParams();
                                TranceSeekAPI.CharacterBonusPassive(_v, "LowPhysicalAttack");
                            }
                            else
                            {
                                _v.NormalPhysicalParams();
                            }
                            TranceSeekAPI.MagicAccuracy(_v);
                            TranceSeekAPI.EnemyTranceBonusAttack(_v);
                            TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                            TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                            TranceSeekAPI.BonusElement(_v);
                            _v.CalcHpDamage();
                            _v.Command.AbilityStatus |= TranceSeekStatus.PowerBreak;
                            TranceSeekAPI.TryAlterMagicStatuses(_v);
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
                                TranceSeekAPI.CharacterBonusPassive(_v, "LowPhysicalAttack");
                            }
                            else
                            {
                                _v.NormalPhysicalParams();
                            }
                            TranceSeekAPI.MagicAccuracy(_v);
                            TranceSeekAPI.EnemyTranceBonusAttack(_v);
                            TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                            TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                            TranceSeekAPI.BonusElement(_v);
                            _v.CalcHpDamage();
                            _v.Command.AbilityStatus |= TranceSeekStatus.MentalBreak;
                            TranceSeekAPI.TryAlterMagicStatuses(_v);
                        }
                        break;
                    }
                    // CHAKRA - Script 37
                    case (BattleAbilityId)1334: // Lamia - Chakra
                    {
                        _v.Target.Flags |= (CalcFlag.HpDamageOrHeal | CalcFlag.MpDamageOrHeal);
                        _v.Target.HpDamage = (int)(_v.Target.MaximumHp * (uint)_v.Command.Power / 100U);
                        _v.Target.MpDamage = (int)(_v.Target.MaximumMp * (uint)_v.Command.Power / 100U);
                        TranceSeekAPI.TryAlterMagicStatuses(_v);
                        break;
                    }
                    // LANCER - Script 39
                    case (BattleAbilityId)2223: // DracoZombie - Zombie Breath
                    {
                        if (_v.Command.Power == 1)
                        {
                            if (_v.Target.CanBeAttacked())
                            {
                                _v.Target.CurrentHp = 1U;
                                _v.Target.CurrentMp = 1U;
                                TranceSeekAPI.TryAlterMagicStatuses(_v);
                            }
                        }
                        else
                        {
                            if (_v.IsCasterNotTarget() && _v.Target.CanBeAttacked() && !_v.Target.TryKillFrozen())
                            {
                                _v.PhysicalAccuracy();
                                if (TranceSeekAPI.TryPhysicalHit(_v))
                                {
                                    _v.NormalPhysicalParams();
                                    TranceSeekAPI.CharacterBonusPassive(_v, "PhysicalAttack");
                                    TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                                    TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                                    TranceSeekAPI.BonusBackstabAndPenaltyLongDistance(_v);
                                    TranceSeekAPI.BonusElement(_v);
                                    if (_v.CanAttackElementalCommand())
                                    {
                                        TranceSeekAPI.IpsenCastleMalus(_v);
                                        _v.Target.Flags |= (CalcFlag.HpAlteration | CalcFlag.MpAlteration);
                                        _v.CalcPhysicalHpDamage();
                                        int hpDamage3 = _v.Target.HpDamage;
                                        if ((_v.Target.Flags & CalcFlag.HpRecovery) != 0)
                                        {
                                            _v.Target.FaceTheEnemy();
                                        }
                                        _v.Target.MpDamage = hpDamage3 >> 4;
                                        BTL_DATA data = _v.Caster.Data;
                                        if (data.dms_geo_id == 297)
                                        {
                                            _v.Caster.Flags |= (CalcFlag.HpAlteration | CalcFlag.HpRecovery);
                                            _v.Caster.HpDamage = hpDamage3 / 2;
                                        }
                                        TranceSeekAPI.RaiseTrouble(_v);
                                        TranceSeekAPI.TryAlterMagicStatuses(_v);
                                    }
                                }
                            }
                        }
                        break;
                    }
                    // MIGHT SCRIPT - Script 43
                    case (BattleAbilityId)1335: // Lamia - Might
                    {
                        TranceSeekAPI.TryAlterMagicStatuses(_v);
                        btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.PowerUp, parameters: $"+{_v.Command.Power}");
                        btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.MagicUp, parameters: $"+{_v.Command.Power}");
                        break;
                    }
                    // STEAL SCRIPT - Script 58
                    case (BattleAbilityId)1282: // Magic Vice - Mug
                    {
                        BattleScriptFactory factorymug = SBattleCalculator.FindScriptFactory(58); // Script 0058_StealScript
                        BattleScriptFactory factoryattack = SBattleCalculator.FindScriptFactory(8); // Script 0008_EnemyPhysicalAttackScript
                        if (factorymug != null)
                        {
                            IBattleScript script = factorymug(_v);
                            script.Perform();
                        }
                        if (factoryattack != null)
                        {
                            IBattleScript script = factoryattack(_v);
                            script.Perform();
                        }
                        break;
                    }
                    // ITEM ETHER - Script 70
                    case (BattleAbilityId)1247: // Gimme cat - Ether
                    {
                        _v.Context.Attack = 15;
                        _v.Context.AttackPower = _v.Command.Item.Power;
                        _v.Context.DefensePower = 0;
                        _v.CalcMpMagicRecovery();
                        break;
                    }
                    // PUMPKIN HEAD - Script 78
                    case (BattleAbilityId)1357: // Hedgehog Pie - Pumpkin Head
                    {
                        uint num = Math.Min(((_v.Caster.MaximumHp - _v.Caster.CurrentHp) / 33), 100);
                        _v.NormalMagicParams();
                        _v.Context.AttackPower = (int)(_v.Command.Power + num);
                        TranceSeekAPI.CharacterBonusPassive(_v, "MagicAttack");
                        TranceSeekAPI.CasterPenaltyMini(_v);
                        TranceSeekAPI.PenaltyShellAttack(_v);
                        TranceSeekAPI.PenaltyCommandDividedAttack(_v);
                        TranceSeekAPI.BonusElement(_v);
                        if (TranceSeekAPI.CanAttackMagic(_v))
                        {
                            _v.Target.Flags = CalcFlag.HpAlteration;
                            if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)102))
                                TranceSeekAPI.TryCriticalHit(_v);
                            _v.CalcHpDamage();
                        }
                        TranceSeekAPI.TryAlterMagicStatuses(_v);
                        break;
                    }
                    // MELT - Script 88
                    case (BattleAbilityId)1353: // Bomb - Blowup
                    {
                        _v.Target.Flags |= CalcFlag.HpAlteration;
                        _v.Target.HpDamage = (Int32)_v.Caster.CurrentHp;
                        break;
                    }
                    // CANNON - Script 91
                    case (BattleAbilityId)1204: // Armstrong - Cannon
                    {
                        _v.PhysicalAccuracy();
                        if (TranceSeekAPI.TryPhysicalHit(_v))
                        {
                            _v.NormalPhysicalParams();
                            TranceSeekAPI.CharacterBonusPassive(_v, "PhysicalAttack");
                            TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                            TranceSeekAPI.EnemyTranceBonusAttack(_v);
                            TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                            if (Mathf.Abs((_v.Caster.Row - _v.Target.Row)) > 1)
                                ++_v.Context.DamageModifierCount;
                            TranceSeekAPI.BonusElement(_v);
                            if (_v.CanAttackElementalCommand())
                            {
                                _v.CalcPhysicalHpDamage();
                                TranceSeekAPI.RaiseTrouble(_v);
                                TranceSeekAPI.TryAlterMagicStatuses(_v);
                            }
                        }
                        break;
                    }
                    // MAELSTORM - Script 93
                    case (BattleAbilityId)1181: // Amduscias
                    case (BattleAbilityId)1293: // Torama - Blaster
                    {
                        if (TranceSeekAPI.CheckUnsafetyOrGuard(_v) && _v.Target.CanBeAttacked())
                        {
                            TranceSeekAPI.MagicAccuracy(_v);
                            _v.Target.PenaltyShellHitRate();
                            _v.PenaltyCommandDividedHitRate();
                            _v.Command.HitRate += _v.Caster.Level - _v.Target.Level;
                            TranceSeekAPI.ReduceAccuracyEliteMonsters(_v, true);
                            if (TranceSeekAPI.TryMagicHit(_v) || _v.Command.HitRate == 255)
                            {
                                _v.Context.Flags |= BattleCalcFlags.DirectHP;
                                if (_v.Caster.Data.dms_geo_id == 401) // Friendly Feather Circle - Heartless Angel
                                {
                                    _v.Target.CurrentHp = 1;
                                    return;
                                }
                                if (_v.Target.CurrentHp < 10U)
                                {
                                    _v.Target.CurrentHp = (uint)(1L + GameRandom.Next8() % _v.Target.CurrentHp);
                                }
                                else
                                {
                                    _v.Target.CurrentHp = (uint)(1 + GameRandom.Next8() % 9);
                                }
                                TranceSeekAPI.TryAlterCommandStatuses(_v);
                            }
                        }
                        break;
                    }
                    // PRECISE PHYSICAL ATTACK - Script 100
                    case (BattleAbilityId)1173: // Agares
                    case (BattleAbilityId)1208: // Shell Dragon - Charge
                    case (BattleAbilityId)1232: // Whale Zombie - Fin
                    case (BattleAbilityId)1238: // Antlion - Counter Horn
                    case (BattleAbilityId)1245: // Land Worm - Sandstorm
                    case (BattleAbilityId)1253: // Golem - Sandstorm
                    case (BattleAbilityId)1264: // Abadon - Mantis Reaper
                    case (BattleAbilityId)1270: // Lizard Man - Cleave
                    case (BattleAbilityId)1336: // Red Dragon - Vacuum
                    case (BattleAbilityId)1350: // Stroper - Sweep
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
                                TranceSeekAPI.CharacterBonusPassive(_v, "PhysicalAttack");
                            }
                            TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                            TranceSeekAPI.EnemyTranceBonusAttack(_v);
                            TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                            if (_v.Command.HitRate != 255)
                            {
                                TranceSeekAPI.BonusBackstabAndPenaltyLongDistance(_v);
                            }
                            TranceSeekAPI.BonusElement(_v);
                            if (_v.CanAttackElementalCommand())
                            {
                                TranceSeekAPI.RaiseTrouble(_v);
                                _v.CalcHpDamage();
                                TranceSeekAPI.InfusedWeaponStatus(_v);
                                TranceSeekAPI.TryAlterMagicStatuses(_v);
                            }
                        }
                        break;
                    }
                    // MAGIC APPLY POSITIVE - Script 103
                    case (BattleAbilityId)1185: // Ao
                    case (BattleAbilityId)1229: // Mu - Haste
                    case (BattleAbilityId)1241: // Gargoyle - Protect
                    case (BattleAbilityId)1249: // Goblin Mage - Vanish
                    case (BattleAbilityId)1266: // Jabberwock - Shell
                    case (BattleAbilityId)1269: // Vice - Trickery
                    case (BattleAbilityId)1271: // Lizard Man - Protect
                    case (BattleAbilityId)1276: // Core - Regen
                    case (BattleAbilityId)1285: // Serpion - Shell
                    case (BattleAbilityId)1298: // Mimic - Reflect
                    case (BattleAbilityId)1305: // Hornet - Vanish
                    case (BattleAbilityId)1310: // Gnoll - Haste
                    case (BattleAbilityId)1321: // Axe Beak - Flash
                    case (BattleAbilityId)1339: // Sahagin - Shell
                    {
                        TranceSeekAPI.TryAlterCommandStatuses(_v);
                        break;
                    }
                    // MAGIC APPLY POSITIVE - Script 104
                    case (BattleAbilityId)1361: // Tonberry - Everyone’s Grudge
                    {
                        _v.Target.Flags |= CalcFlag.HpAlteration;
                        _v.Target.HpDamage = 1 << Math.Min(20, GameState.Tonberies - 1);
                        break;
                    }
                    // MAGIC LANCER - Script 115
                    case (BattleAbilityId)1338: // Sahagin - Water-gun
                    {
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
                                _v.Target.Flags |= (CalcFlag.HpAlteration | CalcFlag.MpAlteration);
                                if (_v.Context.IsAbsorb)
                                {
                                    _v.Target.Flags = (CalcFlag.HpDamageOrHeal);
                                }
                                _v.CalcHpDamage();
                                int hpDamage2 = _v.Target.HpDamage;
                                if ((_v.Target.Flags & CalcFlag.HpRecovery) != 0)
                                {
                                    _v.Target.FaceTheEnemy();
                                }
                                _v.Target.MpDamage = hpDamage2 >> 4;
                                if (!_v.Target.IsZombie && !_v.Context.IsAbsorb)
                                    _v.Target.MpDamage = hpDamage2 >> 4;
                            }
                            TranceSeekAPI.TryAlterMagicStatuses(_v);
                        }
                        break;
                    }
                    // LOW RANDOM MAGIC - Script 116
                    case (BattleAbilityId)1174: // Ahriman
                    {
                        _v.Context.Attack = UnityEngine.Random.Range(((_v.Caster.Magic + _v.Caster.Level) / 3), (_v.Caster.Magic + _v.Caster.Level));
                        _v.SetCommandPower();
                        _v.Target.SetMagicDefense();
                        TranceSeekAPI.CasterPenaltyMini(_v);
                        TranceSeekAPI.EnemyTranceBonusAttack(_v);
                        TranceSeekAPI.PenaltyShellAttack(_v);
                        TranceSeekAPI.PenaltyCommandDividedAttack(_v);
                        TranceSeekAPI.BonusElement(_v);
                        if (TranceSeekAPI.CanAttackMagic(_v))
                        {
                            _v.CalcHpDamage();
                        }
                        TranceSeekAPI.TryAlterMagicStatuses(_v);
                        break;
                    }
                    // POISON MAGIC - Script 118
                    case (BattleAbilityId)1177: // Amanite
                    case (BattleAbilityId)1223: // DracoZombie - Zombie Breath
                    case (BattleAbilityId)1333: // Python - Poison
                    case (BattleAbilityId)1376: // Zombie - Poison
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
                            if (_v.Target.HasCategory(EnemyCategory.Humanoid))
                                _v.Context.DamageModifierCount += 4;
                            if (_v.Target.IsZombie)
                                _v.Target.Flags |= CalcFlag.HpRecovery;
                            if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)102))
                                TranceSeekAPI.TryCriticalHit(_v);
                            _v.CalcHpDamage();
                        }
                        TranceSeekAPI.TryAlterMagicStatuses(_v);
                        break;
                    }
                    // TRANCE SEEK SPECIAL - Script 164
                    case (BattleAbilityId)1221: // Ironite - Dragon Force
                    {
                        _v.Target.TryAlterSingleStatus(BattleStatusId.ChangeStat, true, _v.Caster, "PhysicalDefence", Math.Min(255, _v.Target.PhysicalDefence + 2));
                        Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                                {
                                    { "US", "Defence ↑" },
                                    { "UK", "Defence ↑" },
                                    { "JP", "ぼうぎょりょく↑" },
                                    { "ES", "DIF fisica ↑" },
                                    { "FR", "Défense ↑" },
                                    { "GR", "Defensa F ↑" },
                                    { "IT", "Abwehr ↑" },
                                };
                        btl2d.Btl2dReqSymbolMessage(_v.Target.Data, "[F9FF39]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 0);
                        _v.Target.TryAlterSingleStatus(BattleStatusId.ChangeStat, true, _v.Caster, "MagicDefence", Math.Min(255, _v.Target.MagicDefence + 2));
                        Dictionary<String, String> localizedMessage2 = new Dictionary<String, String>
                                {
                                    { "US", "Magic Def ↑" },
                                    { "UK", "Magic Def ↑" },
                                    { "JP", "まほうぼうぎょ ↑" },
                                    { "ES", "DIF magica ↑" },
                                    { "FR", "Protection ↑" },
                                    { "GR", "Defensa M ↑" },
                                    { "IT", "Z-Abwehr ↑" },
                                };
                        btl2d.Btl2dReqSymbolMessage(_v.Target.Data, "[F9FF39]", localizedMessage2, HUDMessage.MessageStyle.DAMAGE, 5);
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
                // _v.Caster.MaxDamageLimit = 9999;
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
            { 1234, 156 }, // Ghost
            { 1235, 156 },
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
            { 1376, 105 }, // Zombie
            { 1377, 105 },
            { 1378, 342 }, // TEST
            { 1379, 342 },
        };
    }
}

