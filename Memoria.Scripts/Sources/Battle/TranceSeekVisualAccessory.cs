using FF9;
using Memoria.Data;
using Memoria.Prime;
using System;
using System.Collections.Generic;
using UnityEngine;
using static BTL_DATA;
using static Memoria.Scripts.TranceSeek.TranceSeekVisualAccessoryDB;

namespace Memoria.Scripts.TranceSeek
{
    public static class TranceSeekVisualAccessory
    {
        public static Boolean ProcessMascotRecast(BattleUnit caster)
        {
            var CasterTSVAR = caster.State();
            if (CasterTSVAR.MascotCooldown <= 0)
            {
                btl_cmd.SetCommand(caster.Data.cmd[3], BattleCommandId.SysPhantom, 2100, 254, 8u);
                //CasterTSVAR.MascotCooldown = (60 - caster.Will) * UnityEngine.Random.Range(1, 11) * 100;
                CasterTSVAR.MascotCooldown = (60 - caster.Will) * UnityEngine.Random.Range(1, 11);
            }
            else
            {
                CasterTSVAR.MascotCooldown -= caster.Data.cur.at_coef * BattleState.ATBTickCount;
            }
            return true;
        }

        public static void CheckCreateVisualAccessory(BattleUnit unit)
        {
            if (!unit.IsPlayer)
                return;

            CharacterId charId = unit.PlayerIndex;
            RegularItem equippedAccessory = unit.Accessory;

            if (VisualAccessoriesDict.TryGetValue(equippedAccessory, out AccessoryConfig accessoryConfig))
            {
                if (accessoryConfig.TryGetValue(charId, out CharacterTransform transformConfig))
                {
                    var unit_TSVar = unit.State();

                    WEAPON_MODEL AccessoryModel = new WEAPON_MODEL { geo = ModelFactory.CreateModel(accessoryConfig.ModelName, true) };
                    GameObject currentAttachedTarget = unit.Data.gameObject;

                    if (!string.IsNullOrEmpty(accessoryConfig.AnimIdle))
                    {
                        AnimationFactory.AddAnimWithAnimatioName(AccessoryModel.geo, accessoryConfig.AnimIdle);
                        Animation anim = AccessoryModel.geo.GetComponent<Animation>();
                        if (anim != null && anim.GetClip(accessoryConfig.AnimIdle) != null)
                        {
                            anim[accessoryConfig.AnimIdle].wrapMode = WrapMode.Loop;
                            anim[accessoryConfig.AnimIdle].speed = 0.5f;
                            anim.Play(accessoryConfig.AnimIdle);
                        }
                    }

                    GeoAttach(AccessoryModel.geo, currentAttachedTarget, transformConfig.BoneIndex);
                    AccessoryModel.geo.transform.localPosition = transformConfig.PositionOffset;
                    AccessoryModel.geo.transform.localRotation = Quaternion.Euler(transformConfig.RotationOffset);
                    AccessoryModel.geo.transform.localScale = transformConfig.ScaleOffset;
                    AccessoryModel.geo.SetActive(true);

                    unit_TSVar.AdditionalModel.Add(AccessoryModel);

                    List<BoneHider> cachedHiders = new List<BoneHider>();
                    if (transformConfig.BonesToHide != null)
                    {
                        foreach (int boneIndex in transformConfig.BonesToHide)
                        {
                            Transform boneToHide = currentAttachedTarget.transform.GetChildByName("bone" + boneIndex.ToString("D3"));
                            if (boneToHide != null)
                            {
                                BoneHider hider = boneToHide.GetComponent<BoneHider>();
                                if (hider == null)
                                    hider = boneToHide.gameObject.AddComponent<BoneHider>();

                                cachedHiders.Add(hider);
                            }
                        }
                    }

                    unit.AddDelayedModifier(
                        caster =>
                        {
                            if (AccessoryModel == null || AccessoryModel.geo == null || caster.Accessory != equippedAccessory)
                            {
                                if (AccessoryModel?.geo != null)
                                    UnityEngine.Object.Destroy(AccessoryModel.geo);

                                foreach (BoneHider hider in cachedHiders)
                                {
                                    if (hider != null)
                                        UnityEngine.Object.Destroy(hider);
                                }
                                cachedHiders.Clear();

                                return false;
                            }

                            if (currentAttachedTarget != caster.Data.gameObject)
                            {
                                currentAttachedTarget = caster.Data.gameObject;
                                GeoAttach(AccessoryModel.geo, currentAttachedTarget, transformConfig.BoneIndex);
                                AccessoryModel.geo.transform.localPosition = transformConfig.PositionOffset;
                                AccessoryModel.geo.transform.localRotation = Quaternion.Euler(transformConfig.RotationOffset);
                                AccessoryModel.geo.transform.localScale = transformConfig.ScaleOffset;

                                cachedHiders.Clear();
                                if (transformConfig.BonesToHide != null)
                                {
                                    foreach (int boneIndex in transformConfig.BonesToHide)
                                    {
                                        Transform boneToHide = currentAttachedTarget.transform.GetChildByName("bone" + boneIndex.ToString("D3"));
                                        if (boneToHide != null)
                                        {
                                            BoneHider hider = boneToHide.GetComponent<BoneHider>();
                                            if (hider == null)
                                                hider = boneToHide.gameObject.AddComponent<BoneHider>();

                                            cachedHiders.Add(hider);
                                        }
                                    }
                                }
                            }

                            bool areMeshesHidden = (caster.Data.meshflags & 0xFFFF) == 0xFFFF;
                            bool shouldShow = !areMeshesHidden && caster.Data.bi.disappear == 0;

                            if (AccessoryModel.geo.activeSelf != shouldShow)
                                AccessoryModel.geo.SetActive(shouldShow);

                            return true;
                        },
                        null
                    );
                }
            }
        }

        public class BoneHider : MonoBehaviour
        {
            private readonly Vector3 SCALE_INVISIBLE = new Vector3(0.01f, 0.01f, 0.01f);

            void LateUpdate()
            {
                transform.localScale = SCALE_INVISIBLE;
            }
        }

        private static void GeoAttach(GameObject sourceObject, GameObject targetObject, Int32 bone_index)
        {
            if (sourceObject == null || targetObject == null)
                return;
            Transform attachedTransform = targetObject.transform;
            Transform rootTransform = attachedTransform.GetChildByName("bone000").transform;
            Transform childByName = targetObject.transform.GetChildByName("bone" + bone_index.ToString("D3"));
            sourceObject.transform.parent = childByName;
            sourceObject.transform.localPosition = Vector3.zero;
            sourceObject.transform.localRotation = Quaternion.identity;
            sourceObject.transform.localScale = Vector3.one;
            rootTransform.localPosition = Vector3.zero;
            rootTransform.localRotation = Quaternion.identity;
            rootTransform.localScale = Vector3.one;
        }
    }
}
