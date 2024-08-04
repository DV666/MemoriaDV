using System;
using FF9;
using Memoria.Data;
using UnityEngine;
using System.Collections.Generic;
using Memoria.Assets;

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
            if (_v.Caster.Data.dms_geo_id == 573) // Duel - Le Rouge
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
                List<BattleStatusId> statuschoosen = new List<BattleStatusId>{ BattleStatusId.Poison, BattleStatusId.Venom, BattleStatusId.Blind, BattleStatusId.Silence,
                    BattleStatusId.Trouble, BattleStatusId.Freeze, BattleStatusId.Heat, BattleStatusId.Doom, BattleStatusId.Mini, BattleStatusId.GradualPetrify,
                    BattleStatusId.Berserk, BattleStatusId.Confuse, BattleStatusId.Stop, BattleStatusId.Zombie, BattleStatusId.Slow, BattleStatusId.Haste,
                    BattleStatusId.Protect, BattleStatusId.Shell, BattleStatusId.Regen, BattleStatusId.Float, BattleStatusId.Vanish, TranceSeekCustomAPI.CustomStatusId.PowerBreak,
                TranceSeekCustomAPI.CustomStatusId.MagicBreak, TranceSeekCustomAPI.CustomStatusId.ArmorBreak, TranceSeekCustomAPI.CustomStatusId.MentalBreak, TranceSeekCustomAPI.CustomStatusId.PowerUp,
                TranceSeekCustomAPI.CustomStatusId.MagicUp, TranceSeekCustomAPI.CustomStatusId.ArmorUp, TranceSeekCustomAPI.CustomStatusId.MentalUp, TranceSeekCustomAPI.CustomStatusId.Vieillissement,
                TranceSeekCustomAPI.CustomStatusId.Dragon};

                for (Int32 i = 0; i < (statuschoosen.Count - 1); i++)
                {
                    if ((statuschoosen[i].ToBattleStatus() & _v.Caster.CurrentStatus) != 0)
                    {
                        btl_stat.RemoveStatus(_v.Target, statuschoosen[i]);
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
        }
    }
}
