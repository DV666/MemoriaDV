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

            if (FF9StateSystem.EventState.gEventGlobal[1408] == 1 && CasterTSVAR.Mascot != null)
            {
                Animation anim = CasterTSVAR.Mascot.geo.GetComponent<Animation>();
                if (anim != null)
                {
                    string modelName = CasterTSVAR.Mascot.geo.name;

                    if (modelName.StartsWith("GEO_MON_B3"))
                    {
                        string idSuffix = modelName.Substring("GEO_MON_B3".Length);
                        string animCast1 = "ANH_MON_B3" + idSuffix + "_020";
                        string animCast2 = "ANH_MON_B3" + idSuffix + "_021";
                        string animCast3 = "ANH_MON_B3" + idSuffix + "_022";
                        string animIdle = "ANH_MON_B3" + idSuffix + "_000";

                        anim.PlayQueued(animCast1, QueueMode.PlayNow);
                        anim.PlayQueued(animCast2, QueueMode.CompleteOthers);
                        anim.PlayQueued(animCast3, QueueMode.CompleteOthers);
                        anim.PlayQueued(animIdle, QueueMode.CompleteOthers);
                    }
                }
                FF9StateSystem.EventState.gEventGlobal[1408] = 0;
            }

            if (CasterTSVAR.MascotCooldown <= 0)
            {
                btl_cmd.SetCommand(caster.Data.cmd[3], BattleCommandId.SysPhantom, 2100, 254, 8u);
                CasterTSVAR.MascotCooldown = (60 - caster.Will) * UnityEngine.Random.Range(1, 11) * 100;
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
                int accessoryShape = ff9item._FF9Item_Data[equippedAccessory].shape;
                string modelName = accessoryConfig.ModelName;
                string animIdle = accessoryConfig.AnimIdle;

                CharacterTransform transformConfig;
                bool hasTransform = accessoryConfig.TryGetValue(charId, out transformConfig);

                if (!hasTransform && accessoryShape == 60)
                {
                    hasTransform = MascotBaseTransforms.TryGetValue(charId, out transformConfig);
                    if (!SFXChannel.CHANNEL_TYPE.ContainsKey("Mascot"))
                        SFXChannel.CHANNEL_TYPE.Add("Mascot", new Int32[] { 1509, 1502, 1503 });
                }

                if (hasTransform)
                {
                    var unit_TSVar = unit.State();

                    WEAPON_MODEL AccessoryModel = new WEAPON_MODEL { geo = ModelFactory.CreateModel(modelName, true) };
                    AccessoryModel.geo.name = modelName;
                    GameObject currentAttachedTarget = unit.Data.gameObject;

                    if (accessoryShape == 60 && modelName.StartsWith("GEO_MON_B3"))
                    {
                        unit_TSVar.Mascot = AccessoryModel;
                        string idSuffix = modelName.Substring("GEO_MON_B3".Length);
                        if (string.IsNullOrEmpty(animIdle))
                            animIdle = "ANH_MON_B3" + idSuffix + "_000";

                        string cast1 = "ANH_MON_B3" + idSuffix + "_020";
                        string cast2 = "ANH_MON_B3" + idSuffix + "_021";
                        string cast3 = "ANH_MON_B3" + idSuffix + "_022";

                        AnimationFactory.AddAnimWithAnimatioName(AccessoryModel.geo, cast1);
                        AnimationFactory.AddAnimWithAnimatioName(AccessoryModel.geo, cast2);
                        AnimationFactory.AddAnimWithAnimatioName(AccessoryModel.geo, cast3);

                        Animation anim = AccessoryModel.geo.GetComponent<Animation>();
                        if (anim != null)
                        {
                            if (anim[animIdle] != null) anim[animIdle].speed = 0.75f;
                            if (anim[cast1] != null) anim[cast1].speed = 0.75f;
                            if (anim[cast2] != null) anim[cast2].speed = 0.75f;
                            if (anim[cast3] != null) anim[cast3].speed = 0.75f;
                        }
                    }

                    if (!string.IsNullOrEmpty(animIdle))
                    {
                        AnimationFactory.AddAnimWithAnimatioName(AccessoryModel.geo, animIdle);
                        Animation anim = AccessoryModel.geo.GetComponent<Animation>();
                        if (anim != null && anim.GetClip(animIdle) != null)
                        {
                            anim[animIdle].wrapMode = WrapMode.Loop;
                            anim[animIdle].speed = 0.5f;
                            anim.Play(animIdle);
                        }
                    }

                    Transform targetBone = currentAttachedTarget.transform.GetChildByName("bone" + transformConfig.BoneIndex.ToString("D3"));
                    AccessoryModel.geo.transform.parent = null;
                    MascotFollower follower = AccessoryModel.geo.AddComponent<MascotFollower>();
                    follower.TargetBone = targetBone;
                    follower.LocalPosition = transformConfig.PositionOffset;
                    follower.LocalRotation = transformConfig.RotationOffset;
                    follower.LocalScale = transformConfig.ScaleOffset;

                    AccessoryModel.geo.SetActive(true);

                    Transform rootTransform = currentAttachedTarget.transform.GetChildByName("bone000");
                    if (rootTransform != null)
                    {
                        rootTransform.localPosition = Vector3.zero;
                        rootTransform.localRotation = Quaternion.identity;
                        rootTransform.localScale = Vector3.one;
                    }

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

                            if (accessoryShape == 60)
                            {
                                Animation anim = AccessoryModel.geo.GetComponent<Animation>();
                                if (anim != null)
                                {
                                    foreach (AnimationState state in anim)
                                    {
                                        state.speed = 0.75f;
                                    }
                                }
                            }

                            if (currentAttachedTarget != caster.Data.gameObject)
                            {
                                currentAttachedTarget = caster.Data.gameObject;
                                Transform newBone = currentAttachedTarget.transform.GetChildByName("bone" + transformConfig.BoneIndex.ToString("D3"));
                                MascotFollower follower = AccessoryModel.geo.GetComponent<MascotFollower>();
                                if (follower != null)
                                    follower.TargetBone = newBone;

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
                            {
                                AccessoryModel.geo.SetActive(shouldShow);

                                if (shouldShow && accessoryShape == 60)
                                {
                                    Animation anim = AccessoryModel.geo.GetComponent<Animation>();
                                    if (anim != null && !string.IsNullOrEmpty(animIdle))
                                    {
                                        anim.enabled = true;
                                        anim.Play(animIdle);
                                    }
                                }
                            }

                            return true;
                        },
                        null
                    );
                }
            }
        }

        public class MascotFollower : MonoBehaviour
        {
            public Transform TargetBone;
            public Vector3 LocalPosition;
            public Vector3 LocalRotation;
            public Vector3 LocalScale = Vector3.one;

            private Animation _anim;

            void Start()
            {
                _anim = GetComponent<Animation>();
            }

            void LateUpdate()
            {
                if (TargetBone == null)
                    return;

                transform.position = TargetBone.TransformPoint(LocalPosition);
                transform.rotation = TargetBone.rotation * Quaternion.Euler(LocalRotation);
                transform.localScale = Vector3.Scale(TargetBone.lossyScale, LocalScale);

                if (_anim != null)
                {
                    bool isPaused = PersistenSingleton<UIManager>.Instance.IsPause;
                    if (_anim.enabled == isPaused)
                        _anim.enabled = !isPaused;
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
