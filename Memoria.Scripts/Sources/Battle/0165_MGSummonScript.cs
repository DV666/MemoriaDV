using FF9;
using Memoria.Data;
using Memoria.Prime;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Memoria.Scripts.TranceSeek
{
    /// <summary>
    /// Special summon script for Mysterious Girl.
    /// </summary>
    [BattleScript(Id)]
    public sealed class MGSummonScript : IBattleScript
    {
        public const Int32 Id = 0165;

        private class SummonData
        {
            public String ModelName;
            public String BTLName;
            public byte TargetBone;
            public Byte[] IconBones;
            public Int32[] AnimIds;

            public SummonData(String modelName, String btlName, byte tarBone, Byte[] iconBones, Int32[] animIds)
            {
                ModelName = modelName;
                BTLName = btlName;
                TargetBone = tarBone;
                IconBones = iconBones;
                AnimIds = animIds;
            }
        }

        private static readonly Dictionary<Int32, SummonData> Summons = new Dictionary<Int32, SummonData>
        {
            { 1, new SummonData("GEO_MON_F9_ShivaMG", "Shiva", 2, new Byte[] { 5, 5, 5, 5, 5, 5 }, new Int32[] { 30013, 30013, 30013, 30013, 30014, 30014 }) },
            { 2, new SummonData("GEO_MON_F9_IfritMG", "Ifrit", 1, new Byte[] { 15, 15, 15, 15, 15, 15 }, new Int32[] { 30016, 30016, 30016, 30016, 30017, 30017 }) },
            { 3, new SummonData("GEO_MON_F9_RamuhMG", "Ramuh", 35, new Byte[] { 6, 6, 6, 6, 6, 6 }, new Int32[] { 30019, 30019, 30019, 30019, 30020, 30020 }) },
            { 4, new SummonData("GEO_MON_F9_AsuraMG", "Asura", 3, new Byte[] { 1, 8, 1, 1, 1, 1 }, new Int32[] { 30025, 30025, 30025, 30025, 30028, 30028 }) },
            { 5, new SummonData("GEO_MON_F9_LeviathanMG", "Leviathan", 18, new Byte[] { 24, 24, 25, 24, 24, 24 }, new Int32[] { 30022, 30022, 30022, 30022, 30023, 30023 }) },
            { 6, new SummonData("GEO_MON_F9_BahamutMG", "Bahamut", 2, new Byte[] { 20, 15, 20, 21, 20, 21 }, new Int32[] { 30038, 30041, 30038, 30041, 30039, 30039 }) }
        };

        private readonly BattleCalculator _v;

        public MGSummonScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Command.Power == 255 && _v.Command.HitRate == 255) // Unsummoning # NEED A FIX ON MEMORIA ? Need to fix btl_stat.GeoAddColor2DrawPacket (about the < 0 at the beginning)
            {
                if (_v.Target.Data.gameObject != null)
                {
                    Int32 fadeFrames = 12;

                    _v.Target.AddDelayedModifier(
                        btl =>
                        {
                            fadeFrames--;

                            Single progress = 1f - ((Single)fadeFrames / 12f);
                            Int16 rgbDrop = (Int16)(-128f * progress);

                            CustomGeoAddColor2DrawPacket(btl.Data.gameObject, rgbDrop, rgbDrop, rgbDrop);

                            if (fadeFrames < 15)
                                btl_util.GeoSetABR(btl.Data.gameObject, "GEO_POLYFLAGS_TRANS_100_PLUS_25", btl.Data);

                            return fadeFrames > 0;
                        },
                        btl =>
                        {
                            return;
                        }
                    );
                }
                return;
            }
            else if (_v.Command.Power == 99 && _v.Command.HitRate == 99)
            {
                if (_v.Command.Data.info.effect_counter == 1)
                {
                    _v.Target.TryAlterStatuses(BattleStatus.Reflect, false, _v.Target);
                }
                else if (_v.Command.Data.info.effect_counter == 2)
                {
                    _v.Target.MaximumHp = 100000;
                    _v.Target.CurrentHp = 100000;
                    _v.Target.Flags |= CalcFlag.HpAlteration;
                    _v.Caster.MaxDamageLimit = 99999;
                    _v.Target.HpDamage = 99999;
                }
                else
                {
                    if (_v.Target.Data.gameObject != null)
                    {
                        Int32 totalFrames = 15;
                        Int32 fadeFrames = totalFrames;

                        _v.Target.RemoveStatus(BattleStatusConst.AnyPositive);
                        _v.Target.AddDelayedModifier(
                            btl =>
                            {
                                fadeFrames--;

                                Single progress = 1f - ((Single)fadeFrames / (Single)totalFrames);
                                Int16 rgbDrop = (Int16)(-128f * progress);
                                CustomGeoAddColor2DrawPacket(btl.Data.gameObject, rgbDrop, rgbDrop, rgbDrop);

                                return fadeFrames > 0;
                            },
                            btl =>
                            {
                                return;
                            }
                        );
                    }
                }
                return;
            }
            else if(_v.Caster.Data.dms_geo_id == 1213 && _v.Command.Power == 6)
            {
                _v.Caster.Data.mot[2] = "ANH_MON_F9_BahamutMG_MEGAFLARECAST2";
                return;
            }

            if (_v.Command.Data.info.effect_counter == 1)
            {
                BattleUnit btl = btl_scrp.FindBattleUnit(32);
                btl.Data.bi.target = 1;
            }
            else
            {
                _v.Target.Data.SetDisappear(false, 3);
                btl_mot.ShowMesh(_v.Target.Data, _v.Target.Data.mesh_banish, false);
                _v.Target.Data.bi.shadow = 1;
                if (_v.Target.Data.getShadow() != null)
                    _v.Target.Data.getShadow().SetActive(true);

                Vector3 currentPos = _v.Target.Data.pos;
                Quaternion currentRot = _v.Target.Data.rot;

                if (_v.Target.Data.gameObject != null)
                    _v.Target.Data.gameObject.SetActive(false);

                Int32 Key = FF9StateSystem.EventState.gEventGlobal[1305];
                if (Summons.TryGetValue(Key, out SummonData summon))
                {
                    GameObject newModel;
                    bool isPreloaded = false;

                    if (_v.Target.Data.weaponModels.Count > 0 && _v.Target.Data.weaponModels[0].geo != null)
                    {
                        UnityEngine.Object.Destroy(_v.Target.Data.weaponModels[0].geo);
                        _v.Target.Data.weaponModels[0].geo = null;
                    }

                    if (PreloadedModels.TryGetValue(Key, out GameObject template) && template != null)
                    {
                        newModel = UnityEngine.Object.Instantiate(template);
                        isPreloaded = true;
                    }
                    else
                        newModel = ModelFactory.CreateModel(summon.ModelName, true);

                    if (battlebg.BattleRoot != null)
                        newModel.transform.SetParent(battlebg.BattleRoot.transform, false);

                    newModel.transform.localPosition = currentPos;
                    newModel.transform.localRotation = currentRot;
                    _v.Target.Data.gameObject = newModel;
                    _v.Target.Data.animation = newModel.GetComponent<Animation>();

                    _v.Target.Data.meshCount = 1;
                    _v.Target.Data.meshIsRendering = new Boolean[1] { true };
                    _v.Target.Data.meshflags = 0;
                    _v.Target.Data.weaponMeshCount = 0;

                    String[] newMot = new String[Mathf.Max(6, _v.Target.Data.mot.Length)];
                    for (Int16 i = 0; i < 6; i++)
                    {
                        String animName = FF9BattleDB.Animation[summon.AnimIds[i]];
                        newMot[i] = animName;
                        btl_util.getEnemyTypePtr(_v.Target.Data).mot[i] = animName;
                    }
                    _v.Target.Data.mot = newMot;

                    if (!isPreloaded)
                        for (Int16 j = 0; j < 6; j++)
                        {
                            String currentAnim = _v.Target.Data.mot[j];
                            if (!String.IsNullOrEmpty(currentAnim))
                                AnimationFactory.AddAnimWithAnimatioName(_v.Target.Data.gameObject, currentAnim);
                        }

                    OverloadOnBattleInitScript.InitModelAnimations(_v.Target);
                    _v.Target.Data.bi.stop_anim = 0;
                    String animToPlay = _v.Target.Data.mot[_v.Target.Data.bi.def_idle];
                    btl_mot.setMotion(_v.Target.Data, (Byte)_v.Target.Data.bi.def_idle);
                    _v.Target.Data.evt.animFrame = 0;
                    _v.Target.Data.tar_bone = summon.TargetBone;
                    _v.Target.Data.dms_geo_id = (short)FF9BattleDB.GEO.GetKey(summon.ModelName);
                    OverloadOnBattleInitScript.FixMonsterIconOffset(_v.Target);

                    ENEMY_TYPE et = FF9StateSystem.Battle.FF9Battle.enemy[_v.Target.Data.bi.slot_no].et;
                    et.name = summon.BTLName;
                    et.icon_bone = summon.IconBones;
                    SFX.InitBattleParty();

                    geo.geoScaleUpdate(_v.Target.Data, true);
                    _v.Target.Data.gameObject.SetActive(true);

                    if (Key == 3) // Ramuh weapon
                    {
                        String weaponName = "GEO_WEP_RamuhRod";
                        GameObject weaponGeo = ModelFactory.CreateModel("BattleMap/BattleModel/battle_weapon/" + weaponName + "/" + weaponName, true);

                        if (weaponGeo != null)
                        {
                            if (_v.Target.Data.weaponModels.Count == 0)
                                _v.Target.Data.weaponModels.Add(new BTL_DATA.WEAPON_MODEL());

                            BTL_DATA.WEAPON_MODEL weaponModel = _v.Target.Data.weaponModels[0];
                            weaponModel.geo = weaponGeo;
                            weaponModel.bone = 27;
                            weaponModel.scale = new Vector3(0.052098650f, 0.052098650f, 0.052098650f);
                            weaponModel.offset_pos = new Vector3(-2.0f, -0.5f, 0f);
                            weaponModel.builtin_mode = false;

                            geo.geoAttach(weaponGeo, newModel, weaponModel.bone);

                            weaponGeo.transform.localScale = weaponModel.scale;
                            weaponGeo.transform.localPosition = weaponModel.offset_pos;

                            MeshRenderer[] weaponRenderers = weaponGeo.GetComponentsInChildren<MeshRenderer>(true);
                            _v.Target.Data.weaponMeshCount = weaponRenderers.Length;
                            _v.Target.Data.weaponRenderer = new Renderer[weaponRenderers.Length];

                            for (Int32 i = 0; i < weaponRenderers.Length; i++)
                            {
                                _v.Target.Data.weaponRenderer[i] = weaponRenderers[i];
                            }
                        }
                    }
                    else if (Key == 6) // Better position for Bahamut
                    {
                        Vector3 newPos = new Vector3(-100f, 0, 900f);

                        _v.Target.Data.pos = newPos;
                        _v.Target.Data.base_pos = newPos;
                        _v.Target.Data.evt.posBattle = newPos;
                        _v.Target.Data.evt.pos[0] = newPos.x;
                        _v.Target.Data.evt.pos[1] = newPos.y;
                        _v.Target.Data.evt.pos[2] = newPos.z;

                        if (_v.Target.Data.gameObject != null)
                            _v.Target.Data.gameObject.transform.localPosition = newPos;
                    }

                }
                else
                {
                    _v.Context.Flags |= BattleCalcFlags.Miss;
                    return;
                }
            }
        }

        public static Dictionary<Int32, GameObject> PreloadedModels = new Dictionary<Int32, GameObject>();

        public static void InitPreload()
        {
            ClearPreload();
            foreach (var kvp in Summons)
            {
                GameObject template = ModelFactory.CreateModel(kvp.Value.ModelName, true);
                if (template != null)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        String animName = FF9BattleDB.Animation[kvp.Value.AnimIds[i]];
                        if (!String.IsNullOrEmpty(animName))
                        {
                            AnimationFactory.AddAnimWithAnimatioName(template, animName);
                        }
                    }

                    template.SetActive(false);
                    PreloadedModels[kvp.Key] = template;
                }
            }
        }

        public static void ClearPreload()
        {
            foreach (var model in PreloadedModels.Values)
            {
                if (model != null)
                    UnityEngine.Object.Destroy(model);
            }
            PreloadedModels.Clear();
        }

        private static void CustomGeoAddColor2DrawPacket(GameObject go, Int16 r, Int16 g, Int16 b)
        {
            BBGINFO bbgInfoPtr = battlebg.nf_GetBbgInfoPtr();

            Int32 finalR = bbgInfoPtr.chr_r + r;
            Int32 finalG = bbgInfoPtr.chr_g + g;
            Int32 finalB = bbgInfoPtr.chr_b + b;

            r = (Int16)Mathf.Clamp(finalR, 0, 255);
            g = (Int16)Mathf.Clamp(finalG, 0, 255);
            b = (Int16)Mathf.Clamp(finalB, 0, 255);

            foreach (SkinnedMeshRenderer renderer in go.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (r == 0 && g == 0 && b == 0)
                {
                    renderer.tag = "RGBZero";
                    renderer.enabled = false;
                }
                else
                {
                    if (!renderer.enabled && renderer.CompareTag("RGBZero"))
                    {
                        renderer.enabled = true;
                        renderer.tag = String.Empty;
                    }
                    renderer.material.SetColor("_Color", new Color32((Byte)r, (Byte)g, (Byte)b, Byte.MaxValue));
                }
            }

            foreach (MeshRenderer renderer in go.GetComponentsInChildren<MeshRenderer>())
            {
                if (r == 0 && g == 0 && b == 0)
                {
                    renderer.enabled = false;
                }
                else
                {
                    renderer.enabled = true;
                    foreach (Material material in renderer.materials)
                    {
                        material.SetColor("_Color", new Color32((Byte)r, (Byte)g, (Byte)b, Byte.MaxValue));
                    }
                }
            }
        }
    }
}
