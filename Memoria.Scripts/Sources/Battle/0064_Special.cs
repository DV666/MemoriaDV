using System;
using FF9;
using Memoria.Data;
using UnityEngine;
using System.Collections.Generic;
using Memoria.Assets;
using static Memoria.Scripts.Battle.TranceSeekCustomAPI;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Special
    /// </summary>
    [BattleScript(Id)]
    public sealed class Special : IBattleScript
    {
        public const Int32 Id = 0064;

        private readonly BattleCalculator _v;

        public Special(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            if (_v.Caster.PlayerIndex == CharacterId.Zidane)
            {
                switch (_v.Caster.Weapon)
                {
                    case RegularItem.Dagger:
                    case RegularItem.MageMasher:
                    case RegularItem.MythrilDagger:
                        _v.Context.Flags |= BattleCalcFlags.Miss;
                        return;
                    default:
                        break;
                }

                string ModelZidane = null;
                string ModelTranceZidane = null;
                if (btl_util.getSerialNumber(_v.Caster.Data) == CharacterSerialNumber.ZIDANE_DAGGER)
                {
                    ModelZidane = "GEO_MAIN_B0_001"; // Model Zidane_Sword
                    ModelTranceZidane = "GEO_MAIN_B0_023";
                }
                else
                {
                    ModelZidane = "GEO_MAIN_B0_000"; // Model Zidane_Dagger
                    ModelTranceZidane = "GEO_MAIN_B0_022";
                }
                
                _v.Caster.Data.gameObject.SetActive(false);
                _v.Caster.Data.weapon_geo.SetActive(false);
                CharacterBattleParameter btlParam = btl_mot.BattleParameterList[ModelZidane == "GEO_MAIN_B0_001" ? CharacterSerialNumber.ZIDANE_SWORD : CharacterSerialNumber.ZIDANE_DAGGER];
                FF9StateSystem.Common.FF9.player[CharacterId.Zidane].info.serial_no = ModelZidane == "GEO_MAIN_B0_001" ? CharacterSerialNumber.ZIDANE_SWORD : CharacterSerialNumber.ZIDANE_DAGGER;         
                if (_v.Caster.IsUnderAnyStatus(BattleStatus.Trance))
                {
                    _v.Caster.Data.gameObject = ModelFactory.CreateModel(ModelTranceZidane, true);
                    _v.Caster.Data.originalGo = ModelFactory.CreateModel(ModelZidane, true);
                    RefreshTranceModel(_v.Caster.Data);
                    _v.Caster.Data.originalGo.SetActive(false);
                }
                else
                {
                    _v.Caster.Data.gameObject = ModelFactory.CreateModel(ModelZidane, true);
                }
                RegularItem weaponchoose = (btl_util.getWeaponNumber(_v.Caster.Data) > (RegularItem)1000) ? (btl_util.getWeaponNumber(_v.Caster.Data) - 1000) : (btl_util.getWeaponNumber(_v.Caster.Data) + 1000);
                if (weaponchoose < 0)
                {
                    weaponchoose = btl_util.getWeaponNumber(_v.Caster.Data);
                    _v.Context.Flags |= BattleCalcFlags.Miss;
                }
                else
                {
                    FF9StateSystem.Common.FF9.player[(CharacterId)_v.Caster.Data.bi.slot_no].equip[0] = weaponchoose;
                }
                _v.Caster.Data.weapon = ff9item.GetItemWeapon(weaponchoose);
                String modelName = _v.Caster.Data.weapon.ModelName;
                SFX.InitBattleParty();
                _v.Caster.Data.weapon_geo = ModelFactory.CreateModel("BattleMap/BattleModel/battle_weapon/" + modelName + "/" + modelName, true);
                MeshRenderer[] componentsInChildren = _v.Caster.Data.weapon_geo.GetComponentsInChildren<MeshRenderer>();
                _v.Caster.Data.weaponMeshCount = componentsInChildren.Length;
                _v.Caster.Data.weaponRenderer = new Renderer[_v.Caster.Data.weaponMeshCount];
                for (Int32 i = 0; i < _v.Caster.Data.weaponMeshCount; i++)
                {
                    _v.Caster.Data.weaponRenderer[i] = componentsInChildren[i].GetComponent<Renderer>();
                    if (_v.Caster.Data.weapon.CustomTexture.Length > 0)
                    {
                        if (!String.IsNullOrEmpty(_v.Caster.Data.weapon.CustomTexture[i]))
                        {
                            _v.Caster.Data.weaponRenderer[i].material.mainTexture = AssetManager.Load<Texture2D>(_v.Caster.Data.weapon.CustomTexture[i], false);
                        }
                    }
                }
                btl_util.SetBBGColor(_v.Caster.Data.weapon_geo);
                geo.geoAttach(_v.Caster.Data.weapon_geo, _v.Caster.Data.gameObject, 13);
                for (Int32 i = 0; i < 34; i++)
                    _v.Caster.Data.mot[i] = btlParam.AnimationId[i];
                _v.Caster.Data.gameObject.SetActive(true);
                _v.Caster.Data.weapon_geo.SetActive(true);
                btl_mot.setMotion(_v.Caster.Data, BattlePlayerCharacter.PlayerMotionIndex.MP_MAGIC);
                _v.Caster.Data.evt.animFrame = 0;
                geo.geoScaleUpdate(_v.Caster.Data, true);
                _v.Caster.AddDelayedModifier(
                    caster => caster.CurrentAtb >= caster.MaximumAtb,
                    caster =>
                    {
                        if (!caster.IsUnderAnyStatus(BattleStatusConst.StopAtb) && caster.CurrentAtb < (4 * caster.MaximumAtb / 5))
                            caster.CurrentAtb += (Int16)(4 * caster.MaximumAtb / 5);
                        caster.RemoveStatus(BattleStatus.Vanish);
                    }
                );
            }
            else if (_v.Caster.PlayerIndex == CharacterId.Eiko)
            {
                if (!TranceSeekCustomAPI.ModelEiko.TryGetValue(_v.Caster.Data, out String ModelEiko))
                    TranceSeekCustomAPI.ModelEiko[_v.Caster.Data] = "";

                if (SFX.currentEffectID == SpecialEffect.Chakra__Multi) // AA:1017 - Soin Moug
                {
                    _v.Target.Flags |= (CalcFlag.HpDamageOrHeal);
                    _v.Target.HpDamage = (int)(_v.Target.MaximumHp / 2);
                    return;
                }
                else if (SFX.currentEffectID == SpecialEffect.Ether) // AA:1018 - Calin Moug
                {
                    _v.Target.Flags |= (CalcFlag.MpDamageOrHeal);
                    _v.Target.MpDamage = (int)(_v.Target.MaximumMp / 4);
                    _v.Target.Data.special_status_old = false;
                    _v.Target.RemoveStatus(BattleStatus.Poison | BattleStatus.Venom | BattleStatus.Silence | BattleStatus.Blind | BattleStatus.Trouble | BattleStatus.Mini | BattleStatus.Berserk);
                    return;
                }
                else if (SFX.currentEffectID == SpecialEffect.Regen) // AA:1019 - Récup Moug
                {
                    _v.Target.AlterStatus(BattleStatus.Regen, _v.Caster);
                    _v.Target.AlterStatus(BattleStatus.Haste, _v.Caster);
                    return;
                }
                else if (SFX.currentEffectID == SpecialEffect.Mighty_Guard) // AA:1020 - Barrière Moug
                {
                    if (GameRandom.Next8() % 2 != 0)
                    {
                        _v.Target.AlterStatus(BattleStatus.Protect, _v.Caster);
                    }
                    else
                    {
                        _v.Target.AlterStatus(BattleStatus.Shell, _v.Caster);
                    }
                    return;
                }
                else if (SFX.currentEffectID == SpecialEffect.Vanish) // AA:1021 - Mirroir Moug
                {
                    _v.Target.AlterStatus(BattleStatus.Vanish, _v.Caster);
                    return;
                }
                else if (SFX.currentEffectID == SpecialEffect.Auto_Life) // AA:1022 - Pakaho Moug
                {
                    _v.Target.AlterStatus(BattleStatus.AutoLife, _v.Caster);
                    return;
                }
                else if (SFX.currentEffectID == SpecialEffect.Nanoflare || SFX.currentEffectID == SpecialEffect.Poly) // AA:1023 + AA:1024 - Atomoug + Sidémoug
                {
                    if (_v.Target.MagicDefence == 255)
                    {
                        _v.Context.Flags |= BattleCalcFlags.Guard;
                    }
                    else
                    {
                        _v.NormalMagicParams();
                        TranceSeekCustomAPI.CharacterBonusPassive(_v, "MagicAttack");
                        _v.Caster.PenaltyMini();
                        _v.Caster.EnemyTranceBonusAttack();
                        TranceSeekCustomAPI.PenaltyShellAttack(_v);
                        _v.PenaltyCommandDividedAttack();
                        _v.BonusElement();
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
                    return;
                }
                if (TranceSeekCustomAPI.ModelEiko[_v.Caster.Data] == "")
                {
                    if (btl_util.getSerialNumber(_v.Caster.Data) == CharacterSerialNumber.EIKO_FLUTE)
                    {
                        TranceSeekCustomAPI.ModelEiko[_v.Caster.Data] = "GEO_MAIN_B0_009"; 
                    }
                    else
                    {
                        TranceSeekCustomAPI.ModelEiko[_v.Caster.Data] = "GEO_MAIN_B0_010";
                    }
                    _v.Caster.Data.gameObject.SetActive(false);
                    _v.Caster.Data.weapon_geo.SetActive(false);
                    _v.Caster.Data.gameObject = ModelFactory.CreateModel("GEO_NPC_F4_MOG", true);
                    for (Int32 i = 0; i < 34; i++)
                        _v.Caster.Data.mot[i] = "ANH_NPC_F4_MOG_IDLE";
                    _v.Caster.Data.gameObject.SetActive(true);
                    btl_mot.setMotion(_v.Caster.Data, _v.Caster.Data.mot[0]);
                }
                else
                {
                    _v.Caster.Data.gameObject.SetActive(false);
                    CharacterBattleParameter btlParam = btl_mot.BattleParameterList[TranceSeekCustomAPI.ModelEiko[_v.Caster.Data] == "GEO_MAIN_B0_009" ? CharacterSerialNumber.EIKO_FLUTE : CharacterSerialNumber.EIKO_KNIFE];
//                    _v.Caster.Data.gameObject = ModelFactory.CreateModel(TranceSeekCustomAPI.ModelEiko[_v.Caster.Data], true);
                    _v.Caster.Data.gameObject = _v.Caster.Data.originalGo;
                    for (Int32 i = 0; i < 34; i++)
                        _v.Caster.Data.mot[i] = btlParam.AnimationId[i];
                    btl_eqp.InitWeapon(_v.Caster.Player.Data, _v.Caster);
                    btl_mot.setMotion(_v.Caster.Data, _v.Caster.Data.mot[0]);
                    geo.geoScaleUpdate(_v.Caster.Data, true);
                    btl_vfx.SetTranceModel(_v.Caster.Data, _v.Caster.IsUnderAnyStatus(BattleStatus.Trance));
                    _v.Caster.Data.gameObject.SetActive(true);
                    _v.Caster.Data.weapon_geo.SetActive(true);
                    if (_v.Caster.IsUnderAnyStatus(BattleStatus.Vanish))
                        btl_mot.HideMesh(_v.Caster.Data, _v.Caster.Data.mesh_banish, true);
                    TranceSeekCustomAPI.ModelEiko[_v.Caster.Data] = "";
                }
            }
            else if (_v.Caster.Data.dms_geo_id == 573) // Duel - Le Rouge
            {
                if (_v.Command.Power == 1)
                {
                    _v.Caster.Data.mot[0] = "ANH_MAIN_B0_012_401";
                    _v.Caster.Data.mot[2] = "ANH_MAIN_B0_012_401";
                    _v.Caster.AlterStatus(BattleStatus.Defend, _v.Caster);

                }
                else if (_v.Command.Power == 2)
                {
                    _v.Caster.Data.mot[0] = "ANH_MON_B3_182_000";
                    _v.Caster.Data.mot[2] = "ANH_MON_B3_182_003";
                }
            }
            else if (_v.Caster.Data.dms_geo_id == 146)
            {
                if (_v.Command.Power == 10)
                {
                    if (_v.Command.HitRate == 1)
                    {
                        _v.Target.AlterStatus(BattleStatus.Stop, _v.Caster);
                        _v.Target.Data.bi.target = 0;
                    }
                    else
                    {
                        foreach (BattleUnit unit in BattleState.EnumerateUnits())
                        {
                            if (unit.IsPlayer && unit.Data.bi.target == 0 && !unit.IsUnderAnyStatus(BattleStatus.Jump))
                            {
                                unit.Data.bi.target = 1;
                                unit.RemoveStatus(BattleStatus.Stop);
                            }
                        }
                    }
                }
   
            }
            else if (_v.Caster.Data.dms_geo_id == 5 || _v.Caster.Data.dms_geo_id == 267)
            {
                if (_v.Command.Power == 1)
                {
                    Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                    {
                        { "US", "Double!" },
                        { "UK", "Double!" },
                        { "JP", "ダブル!" },
                        { "ES", "¡Doble!" },
                        { "FR", "Double !" },
                        { "GR", "Doppelt!" },
                        { "IT", "Doppio !" },
                    };
                    btl2d.Btl2dReqSymbolMessage(_v.Target.Data, "[EE82EE]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 5);

                }
                else if (_v.Command.Power == 2)
                {
                    Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                    {
                        { "US", "Triple!" },
                        { "UK", "Triple!" },
                        { "JP", "トリプル！" },
                        { "ES", "¡Triple!" },
                        { "FR", "Triple !" },
                        { "GR", "Verdreifachen!" },
                        { "IT", "Triplicare!" },
                    };
                    btl2d.Btl2dReqSymbolMessage(_v.Target.Data, "[00FFFF]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 5);
                }
                else if (_v.Command.Power == 11)
                {
                    _v.Caster.SummonCount = 2;
                }
            }
            else if (_v.Caster.Data.dms_geo_id == 221) // Epitaf - Add weapon to Zidane
            {
                foreach (BattleUnit monster in BattleState.EnumerateUnits())
                {
                    if (!monster.IsPlayer && monster.Data.btl_id > 4 && monster.CurrentHp > 0 && monster.Data.dms_geo_id == 5414)
                    {
                        monster.Data.weapon_geo = ModelFactory.CreateModel("BattleMap/BattleModel/battle_weapon/GEO_WEP_B1_012/GEO_WEP_B1_012", true);
                        MeshRenderer[] componentsInChildren = monster.Data.weapon_geo.GetComponentsInChildren<MeshRenderer>();
                        monster.Data.weaponMeshCount = componentsInChildren.Length;
                        monster.Data.weaponRenderer = new Renderer[monster.Data.weaponMeshCount];
                        for (Int32 i = 0; i < monster.Data.weaponMeshCount; i++)
                            monster.Data.weaponRenderer[i] = componentsInChildren[i].GetComponent<Renderer>();
                        geo.geoAttach(monster.Data.weapon_geo, monster.Data.gameObject, 13);
                        return;
                    }

                }
            }
            else if (_v.Caster.Data.dms_geo_id == 530) // Gardienne du feu
            {
                if (_v.Command.Power == 10)
                {
                    foreach (BattleUnit monster in BattleState.EnumerateUnits()) 
                    {
                        if (!monster.IsPlayer && monster.Data.btl_id == 64 && monster.Data.dms_geo_id == 427) // Sabreur + 1
                        {
                            btl_mot.ShowMesh(monster.Data, 65535, false);
                            monster.Data.mesh_current = (UInt16)(monster.Data.mesh_current & (UInt16)monster.Data.mesh_banish);
                            monster.Data.weapon_geo = ModelFactory.CreateModel("BattleMap/BattleModel/battle_weapon/GEO_WEP_B1_034/GEO_WEP_B1_034", true);
                            MeshRenderer[] componentsInChildren = monster.Data.weapon_geo.GetComponentsInChildren<MeshRenderer>();
                            monster.Data.weaponMeshCount = componentsInChildren.Length;
                            monster.Data.weaponRenderer = new Renderer[monster.Data.weaponMeshCount];
                            for (Int32 i = 0; i < monster.Data.weaponMeshCount; i++)
                                monster.Data.weaponRenderer[i] = componentsInChildren[i].GetComponent<Renderer>();
                            geo.geoAttach(monster.Data.weapon_geo, monster.Data.gameObject, 16);
                            btl_mot.HideMesh(monster.Data, 7, false);
                            monster.MaximumHp = 1000;
                            monster.CurrentHp = 1000;
                        }
                    }
                }
                if (_v.Command.Power == 11)
                {
                    foreach (BattleUnit monster in BattleState.EnumerateUnits())
                    {
                        if (!monster.IsPlayer && monster.Data.dms_geo_id == 427) // Sabreur + 2
                        {
                            btl_mot.ShowMesh(monster.Data, 65535, false);
                            monster.Data.mesh_current = (UInt16)(monster.Data.mesh_current & (UInt16)monster.Data.mesh_banish);
                            monster.Data.weapon_geo = ModelFactory.CreateModel("BattleMap/BattleModel/battle_weapon/GEO_WEP_B1_034/GEO_WEP_B1_034", true);
                            MeshRenderer[] componentsInChildren = monster.Data.weapon_geo.GetComponentsInChildren<MeshRenderer>();
                            monster.Data.weaponMeshCount = componentsInChildren.Length;
                            monster.Data.weaponRenderer = new Renderer[monster.Data.weaponMeshCount];
                            for (Int32 i = 0; i < monster.Data.weaponMeshCount; i++)
                                monster.Data.weaponRenderer[i] = componentsInChildren[i].GetComponent<Renderer>();
                            geo.geoAttach(monster.Data.weapon_geo, monster.Data.gameObject, 16);
                            btl_mot.HideMesh(monster.Data, 7, false);
                            monster.MaximumHp = 1000;
                            monster.CurrentHp = 1000;
                        }
                    }
                }
            }
            else if (_v.Caster.Data.dms_geo_id == 92 && _v.Command.Power == 10) // Kelgar - Lupine Attack
            {
                List<BattleStatus> statuschoosen = new List<BattleStatus>{ BattleStatus.Poison, BattleStatus.Venom, BattleStatus.Blind, BattleStatus.Silence, BattleStatus.Trouble,
                    BattleStatus.Freeze, BattleStatus.Heat, BattleStatus.Doom, BattleStatus.Mini, BattleStatus.GradualPetrify,
                    BattleStatus.Berserk, BattleStatus.Confuse, BattleStatus.Stop, BattleStatus.Zombie, BattleStatus.Slow, BattleStatus.Haste, 
                    BattleStatus.Protect, BattleStatus.Shell, BattleStatus.Regen, BattleStatus.Float, BattleStatus.Vanish };

                for (Int32 i = 0; i < (statuschoosen.Count - 1); i++)
                {
                    if ((statuschoosen[i] & _v.Caster.CurrentStatus) != 0)
                    {
                        btl_stat.AlterStatus(_v.Target, statuschoosen[i]);
                    }
                }
                if (_v.Caster.Data.special_status_old)
                {
                    _v.Target.Data.special_status_old = true;
                }
                for (Int32 i = 0; i < (TranceSeekCustomAPI.SPSSpecialStatus[_v.Caster.Data].Length - 1); i++)
                {
                    if (TranceSeekCustomAPI.SPSSpecialStatus[_v.Caster.Data][i] >= 0)
                    {
                        TranceSeekCustomAPI.AddSpecialSPS(_v.Target.Data, (uint)i, -1, 1.0f);
                    }
                }
            }
            else if (_v.Caster.Data.dms_geo_id == 36) // Silver Dragon - Counter Stance
            {
                if (_v.Command.Power == 1 && _v.Command.HitRate == 1)
                {
                    _v.Caster.Data.mot[0] = "ANH_MON_B3_136_041";
                    _v.Caster.Data.mot[2] = "ANH_MON_B3_136_041";
                    _v.Caster.AlterStatus(BattleStatus.Defend);
                }
                else if (_v.Command.Power == 2 && _v.Command.HitRate == 2)
                {
                    _v.Caster.Data.mot[0] = "ANH_MON_B3_136_000";
                    _v.Caster.Data.mot[2] = "ANH_MON_B3_136_003";
                    _v.Caster.RemoveStatus(BattleStatus.Defend);
                }
            }
            else if (_v.Caster.Data.dms_geo_id == 446) // Garland - Armor Mechanic
            {
                if (_v.Command.Power == 100 && _v.Command.HitRate == 100)
                {
                    TranceSeekCustomAPI.MonsterMechanic[_v.Caster.Data][1] = 100;
                    _v.Target.Data.mot[2] = "ANH_MON_B3_185_000";
                    _v.Target.Flags |= CalcFlag.HpDamageOrHeal;
                    _v.Target.HpDamage = 5000;
                    TranceSeekCustomAPI.SPSCumulative(_v.Target.Data, SPSStackable.MechanicalArmor);
                }
                else if (_v.Command.Power == 111 && _v.Command.HitRate == 111)
                {
                    _v.Caster.Data.mot[0] = "ANH_MON_B3_185_011";
                    _v.Caster.Data.mot[1] = "ANH_MON_B3_185_000";
                    _v.Caster.Data.mot[2] = "ANH_MON_B3_185_011";
                    TranceSeekCustomAPI.MonsterMechanic[_v.Caster.Data][2] = 1;                  
                    return;
                }
                else if (_v.Command.Power == 200 && _v.Command.HitRate == 200)
                {
                    TranceSeekCustomAPI.MonsterMechanic[_v.Caster.Data][1] = 200;
                    _v.Target.Data.mot[2] = "ANH_MON_B3_185_000";
                    _v.Target.Flags |= CalcFlag.HpDamageOrHeal;
                    _v.Target.HpDamage = 9999;
                    TranceSeekCustomAPI.SPSCumulative(_v.Target.Data, SPSStackable.MechanicalArmor);
                }
                _v.Target.TryRemoveStatuses(_v.Command.AbilityStatus);
            }
            else if (_v.Caster.Data.dms_geo_id == 7 && _v.Command.Power == 66 && _v.Command.HitRate == 66) // Siamois - Zombie Armor
            {
                _v.Target.PhysicalDefence += 4;
                _v.Target.MagicDefence += 4;
                SPSCumulative(_v.Target.Data, SPSStackable.ZombieArmor);
            }
            else if (_v.Caster.Data.dms_geo_id == 405 && _v.Caster.Level == 28 && _v.Command.Power == 1 && _v.Command.HitRate == 1) // Friendly Lady Bug - Wind mechanics
            {
                int ColorWing = GameRandom.Next16() % 5;
                _v.Caster.Data.gameObject.SetActive(false);
                _v.Caster.Data.gameObject = ModelFactory.CreateModel("GEO_MON_B3_159", true);
                if (ColorWing == 0)
                {
                    _v.Caster.WeakElement = EffectElement.Wind | EffectElement.Wind | EffectElement.Cold;
                    _v.Caster.AbsorbElement = EffectElement.Holy | EffectElement.Fire;
                    if ((EmbadedTextResources.CurrentSymbol ?? Localization.GetSymbol()) == "FR")
                    {
                        UIManager.Battle.SetBattleMessage("[STRT=270,1]Les ailes Miskoxy brillent d'une couleur [FF0000][HSHD]rouge[FFFFFF][HSHD].", 3);
                    }
                    else
                    {
                        UIManager.Battle.SetBattleMessage("[STRT=246,1]The Miskoxy wings shine with a [FF0000][HSHD]red[FFFFFF][HSHD] color.", 3);
                    }
                    ModelFactory.ChangeModelTexture(_v.Caster.Data.gameObject, new string[] { "CustomTextures/FriendlyLadyBug/FireWings/405_0.png", "CustomTextures/FriendlyLadyBug/FireWings/405_1.png", "CustomTextures/FriendlyLadyBug/FireWings/405_2.png" });
                }
                else if (ColorWing == 1)
                {
                    _v.Caster.WeakElement = EffectElement.Wind | EffectElement.Wind | EffectElement.Fire;
                    _v.Caster.AbsorbElement = EffectElement.Holy | EffectElement.Cold;
                    if ((EmbadedTextResources.CurrentSymbol ?? Localization.GetSymbol()) == "FR")
                    {
                        UIManager.Battle.SetBattleMessage("[STRT=265,1]Les ailes Miskoxy brillent d'une couleur [00d5fe][HSHD]cyan[FFFFFF][HSHD].", 3);
                    }
                    else
                    {
                        UIManager.Battle.SetBattleMessage("[STRT=255,1]The Miskoxy wings shine with a color [00d5fe][HSHD]cyan[FFFFFF][HSHD].", 3);
                    }
                    ModelFactory.ChangeModelTexture(_v.Caster.Data.gameObject, new string[] { "CustomTextures/FriendlyLadyBug/IceWings/405_0.png", "CustomTextures/FriendlyLadyBug/IceWings/405_1.png", "CustomTextures/FriendlyLadyBug/IceWings/405_2.png" });
                }
                else if (ColorWing == 2)
                {
                    _v.Caster.WeakElement = EffectElement.Wind | EffectElement.Wind | EffectElement.Aqua;
                    _v.Caster.AbsorbElement = EffectElement.Holy | EffectElement.Thunder;
                    if ((EmbadedTextResources.CurrentSymbol ?? Localization.GetSymbol()) == "FR")
                    {
                        UIManager.Battle.SetBattleMessage("[STRT=269,1]Les ailes Miskoxy brillent d'une couleur [fdff36][HSHD]jaune[FFFFFF][HSHD].", 3);
                    }
                    else
                    {
                        UIManager.Battle.SetBattleMessage("[STRT=263,1]The Miskoxy wings shine with a color [fdff36][HSHD]yellow[FFFFFF][HSHD].", 3);
                    }
                    ModelFactory.ChangeModelTexture(_v.Caster.Data.gameObject, new string[] { "CustomTextures/FriendlyLadyBug/ThunderWings/405_0.png", "CustomTextures/FriendlyLadyBug/ThunderWings/405_1.png", "CustomTextures/FriendlyLadyBug/ThunderWings/405_2.png" });
                }
                else if (ColorWing == 3)
                {
                    _v.Caster.WeakElement = EffectElement.Wind | EffectElement.Wind | EffectElement.Thunder;
                    _v.Caster.AbsorbElement = EffectElement.Holy | EffectElement.Aqua;
                    if ((EmbadedTextResources.CurrentSymbol ?? Localization.GetSymbol()) == "FR")
                    {
                        UIManager.Battle.SetBattleMessage("[STRT=268,1]Les ailes Miskoxy brillent d'une couleur [000fe0][HSHD]bleue[FFFFFF][HSHD].", 3);
                    }
                    else
                    {
                        UIManager.Battle.SetBattleMessage("[STRT=224,1]Miskoxy wings shine with a color [000fe0][HSHD]blue[FFFFFF][HSHD].", 3);
                    }
                    ModelFactory.ChangeModelTexture(_v.Caster.Data.gameObject, new string[] { "CustomTextures/FriendlyLadyBug/WaterWings/405_0.png", "CustomTextures/FriendlyLadyBug/WaterWings/405_1.png", "CustomTextures/FriendlyLadyBug/WaterWings/405_2.png" });
                }
                else
                {
                    _v.Caster.WeakElement = EffectElement.Wind | EffectElement.Wind;
                    _v.Caster.AbsorbElement = EffectElement.Holy;
                    if ((EmbadedTextResources.CurrentSymbol ?? Localization.GetSymbol()) == "FR")
                    {
                        UIManager.Battle.SetBattleMessage("[STRT=288,1]Les ailes Miskoxy sont totalement transparentes.", 3);
                    }
                    else
                    {
                        UIManager.Battle.SetBattleMessage("[STRT=223,1]Miskoxy wings are totally transparent.", 3);
                    }
                }
                _v.Caster.Data.gameObject.SetActive(true);
            }
            TranceSeekCustomAPI.SpecialSA(_v);
        }

        public static void RefreshTranceModel(BTL_DATA btl)
        {
            CharacterSerialNumber serialNo = btl_util.getSerialNumber(btl);
            btl.battleModelIsRendering = true;
            GeoTexAnim.geoTexAnimPlay(btl.tranceTexanimptr, 2);
            btl.meshCount = 0;
            foreach (System.Object obj in btl.gameObject.transform)
            {
                Transform transform = (Transform)obj;
                if (transform.name.Contains("mesh"))
                    btl.meshCount++;
            }
            btl.meshIsRendering = new Boolean[btl.meshCount];
            for (Int32 i = 0; i < btl.meshCount; i++)
                btl.meshIsRendering[i] = true;
            btl_util.GeoSetABR(btl.gameObject, "PSX/BattleMap_StatusEffect");
            BattlePlayerCharacter.InitAnimation(btl);
            AnimationFactory.AddAnimToGameObject(btl.gameObject, btl_mot.BattleParameterList[serialNo].ModelId, true);
        }
    }
}
