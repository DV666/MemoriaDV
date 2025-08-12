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
            else if (_v.Caster.Data.dms_geo_id == 5 || _v.Caster.Data.dms_geo_id == 267) // Kuja (Double & Triple)
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
                            monster.CurrentHp = (monster.MaximumHp / 2);
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
                            monster.CurrentHp = (monster.MaximumHp / 2);
                        }
                    }
                }
            }
            else if (_v.Caster.Data.dms_geo_id == 92 && _v.Command.Power == 10) // Kelgar - Lupine Attack
            {
                List<BattleStatusId> statuschoosen = new List<BattleStatusId>{ BattleStatusId.Poison, BattleStatusId.Venom, BattleStatusId.Blind, BattleStatusId.Silence,
                    BattleStatusId.Trouble, BattleStatusId.Freeze, BattleStatusId.Heat, BattleStatusId.Doom, BattleStatusId.Mini, BattleStatusId.GradualPetrify,
                    BattleStatusId.Berserk, BattleStatusId.Confuse, BattleStatusId.Stop, BattleStatusId.Zombie, BattleStatusId.Slow, BattleStatusId.Haste,
                    BattleStatusId.Protect, BattleStatusId.Shell, BattleStatusId.Regen, BattleStatusId.Float, BattleStatusId.Vanish, TranceSeekStatusId.PowerBreak,
                TranceSeekStatusId.MagicBreak, TranceSeekStatusId.ArmorBreak, TranceSeekStatusId.MentalBreak, TranceSeekStatusId.PowerUp,
                TranceSeekStatusId.MagicUp, TranceSeekStatusId.ArmorUp, TranceSeekStatusId.MentalUp, TranceSeekStatusId.Vieillissement,
                TranceSeekStatusId.Dragon};

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
        }
    }
}
