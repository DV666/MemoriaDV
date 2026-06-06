using Assets.Scripts.Common;
using FF9;
using Memoria.Data;
using Memoria.Prime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using static Memoria.Configuration;

namespace Memoria.Scripts.TranceSeek
{
    /// <summary>
    /// [DV] This Overload didn't exist.
    /// Mostly made for the companions to follow the main character on the field and World Map too !
    /// Very experimental.
    /// </summary>

    public class OverloadOnFieldScript : MonoBehaviour
    {
        private struct LeaderState
        {
            public Vector3 LocalPosition;
            public Quaternion LocalRotation;
            public bool IsMoving;
            public bool IsRunning;
            public Color LightColor;
        }

        private struct FollowerData
        {
            public string ModelName;
            public string AnimIdle;
            public string AnimWalk;
            public string AnimRun;
            public HashSet<Int32> BlackListModelId;

            public FollowerData(string modelName, string animIdle, string animWalk, string animRun, HashSet<Int32> blackListModelId)
            {
                ModelName = modelName;
                AnimIdle = animIdle;
                AnimWalk = animWalk;
                AnimRun = animRun;
                BlackListModelId = blackListModelId;
            }
        }

        private class Follower
        {
            public CharacterId Id;
            public GameObject Go;
            public Animation Anim;
            public string AnimIdle;
            public string AnimWalk;
            public string AnimRun;
            public int FramesBehind;
            public Queue<LeaderState> PositionHistory = new Queue<LeaderState>();
            public List<Material> CachedMaterials = new List<Material>();
        }

        private Dictionary<CharacterId, FollowerData> characterDB = new Dictionary<CharacterId, FollowerData>()
        {
            { CharacterId.Zidane, new FollowerData("GEO_MAIN_F0_ZDN", "ANH_MAIN_F0_ZDN_IDLE", "ANH_MAIN_F0_ZDN_WALK", "ANH_MAIN_F0_ZDN_RUN", new HashSet<Int32>(){98, 532, 203, 569, 310, 285, 5414}) },
            { CharacterId.Vivi, new FollowerData("GEO_MAIN_F0_VIV", "ANH_MAIN_F0_VIV_IDLE", "ANH_MAIN_F0_VIV_WALK", "ANH_MAIN_F0_VIV_RUN", new HashSet<Int32>(){5415, 8, 662}) },
            { CharacterId.Garnet, new FollowerData("GEO_MAIN_F0_GRN", "ANH_MAIN_F0_GRN_IDLE", "ANH_MAIN_F0_GRN_WALK", "ANH_MAIN_F0_GRN_RUN", new HashSet<Int32>(){526, 532, 557, 202, 205, 666, 557, 671, 309, 281, 283, 287, 288, 185}) },
            { CharacterId.Steiner, new FollowerData("GEO_MAIN_F0_STN", "ANH_MAIN_F0_STN_IDLE", "ANH_MAIN_F0_STN_WALK", "ANH_MAIN_F0_STN_RUN", new HashSet<Int32>(){286, 655, 5489, 658, 526}) },
            { CharacterId.Freya, new FollowerData("GEO_MAIN_F0_FRJ", "ANH_MAIN_F0_FRJ_IDLE", "ANH_MAIN_F0_FRJ_WALK", "ANH_MAIN_F0_FRJ_RUN", new HashSet<Int32>(){290, 297, 192}) },
            { CharacterId.Quina, new FollowerData("GEO_MAIN_F0_KUI", "ANH_MAIN_F0_KUI_IDLE", "ANH_MAIN_F0_KUI_WALK", "ANH_MAIN_F0_KUI_RUN", new HashSet<Int32>(){289, 273, 295}) },
            { CharacterId.Eiko, new FollowerData("GEO_MAIN_F0_EIK", "ANH_MAIN_F0_EIK_IDLE", "ANH_MAIN_F0_EIK_WALK", "ANH_MAIN_F0_EIK_RUN", new HashSet<Int32>(){284, 291, 443, 570}) },
            { CharacterId.Amarant, new FollowerData("GEO_MAIN_F0_SLM", "ANH_MAIN_F0_SLM_IDLE", "ANH_MAIN_F0_SLM_WALK", "ANH_MAIN_F0_SLM_RUN", new HashSet<Int32>(){572, 509, 444}) },
            { CharacterId.Cinna, new FollowerData("GEO_MAIN_B0_013", "ANH_SSUB_F0_CNA_IDLE", "ANH_SUB_F0_CNA_WALK", "ANH_SUB_F0_CNA_RUN", new HashSet<Int32>(){39, 107, 661}) },
            { CharacterId.Marcus, new FollowerData("GEO_MAIN_B0_014", "ANH_SUB_F0_MRC_IDLE", "ANH_SUB_F0_MRC_WALK", "ANH_SUB_F0_MRC_RUN", new HashSet<Int32>(){45, 109, 660}) },
            { CharacterId.Blank, new FollowerData("GEO_MAIN_B0_015", "ANH_SUB_F0_BLN_IDLE", "ANH_SUB_F0_BLN_WALK", "ANH_SUB_F0_BLN_RUN", new HashSet<Int32>(){42, 608, 639, 5467, 190, 659}) },
            { CharacterId.Beatrix, new FollowerData("GEO_MAIN_B0_017", "ANH_SUB_F0_BTX_IDLE", "ANH_SUB_F0_BTX_WALK", "ANH_SUB_F0_BTX_RUN", new HashSet<Int32>(){427, 204, 358}) },
            { (CharacterId)12, new FollowerData("GEO_SUB_F0_SBW", "ANH_SUB_F0_SBW_IDLE", "ANH_SUB_F0_SBW_WALK", "ANH_SUB_F0_SBW_RUN", new HashSet<Int32>(){427, 204, 358}) }
        };

        private List<Follower> activeFollowers = new List<Follower>();
        private Vector3 lastLeaderLocalPos;
        private FieldInfo isRunningField;
        private GameObject leader;
        private Actor actorleader;
        private Renderer leaderRenderer;
        private int leader_model_id;
        private Boolean init;
        private UIManager.UIState lastUiState;
        private Boolean IsWorldMap;
        private Boolean FollowersHidden;

        // BlackListAnimationId is used to hide the followers when the character uses a very specific animations, like the one when mounting a Gargant.
        private static readonly HashSet<Int32> BlackListAnimationId = 
            new HashSet<Int32>(new[] {
                10633 // Mounting Gargant
            });
        private static readonly HashSet<Int32> BlackListFieldId = new HashSet<Int32>(new[] { 70, 655, 1400, 1401, 1402, 1403, 1404 });
        private static readonly HashSet<Int32> ModelCantGetFollowers = new HashSet<Int32>(new[] { 317, 312, 320, 321, 308 });

        private Boolean BlackListCondition => (FF9StateSystem.Common.FF9.fldMapNo == 908 && GameState.ScenarioCounter < 4400);

        private void LateUpdate()
        {
            UIManager uiManager = PersistenSingleton<UIManager>.Instance;
            UIManager.UIState currentState = uiManager.State;

            CheckLeader();
            if (lastUiState == UIManager.UIState.PartySetting && currentState == UIManager.UIState.FieldHUD)
                CheckSwapFollower();
            else if (BlackListCondition || ModelCantGetFollowers.Contains(leader_model_id) || BlackListAnimationId.Contains(actorleader.idle) || BlackListFieldId.Contains(FF9StateSystem.Common.FF9.fldMapNo)) // !PersistenSingleton<UIManager>.Instance.IsPlayerControlEnable
                HideFollowers(true);
            else if (FF9StateSystem.Common.FF9.fldMapNo != -1 && SceneDirector.IsFieldScene() && !SceneDirector.Instance.IsFading)
                ProcessFieldFollowers();
            else if (FF9StateSystem.Common.FF9.wldMapNo != -1 && SceneDirector.IsWorldScene() && !SceneDirector.Instance.IsFading)
                ProcessWorldFollowers();
            else
                ClearFollowers();

            HandleAnimationPause(uiManager.IsPause);

            if (UnityXInput.Input.GetKeyDown(KeyCode.KeypadMultiply))
            {
                Log.Message("[Trance Seek] leader_model_id : " + leader_model_id);
                if (IsWorldMap)
                    Log.Message($"[Trance Seek] WM Actor Position {ff9.GetControlChar().pos}");

                foreach (Follower f in activeFollowers)
                {
                    if (f.Go == null) continue;

                    Log.Message($"[Trance Seek] Follower {f.Id} at position {f.Go.transform.localPosition}, with {f.PositionHistory.Count} states in history.");
                }
            }

            lastUiState = currentState;
        }

        private void CheckLeader()
        {
            EventEngine engine = PersistenSingleton<EventEngine>.Instance;
            if (engine == null) return;

            GameObject oldLeader = leader;

            if (SceneDirector.IsFieldScene())
            {
                PosObj controlChar = engine.GetControlChar();
                if (controlChar == null || !(controlChar is Actor)) return;
                actorleader = (Actor)controlChar;
                leader = actorleader.go;
                leader_model_id = actorleader.model;
                IsWorldMap = false;
            }
            else if (SceneDirector.IsWorldScene())
            {
                leader = ff9.GetControlChar().gameObject;
                actorleader = ff9.GetControlChar().originalActor;
                leader_model_id = ff9.GetControlChar().originalActor.model;
                IsWorldMap = true;
            }
            else
            {
                leader = null;
                IsWorldMap = false;
                leader_model_id = -1;
            }

            if (leader != oldLeader)
            {
                leaderRenderer = leader != null ? leader.GetComponentInChildren<Renderer>() : null;
                ClearHistoryFollowers();
                CheckSwapFollower();
            }
        }

        private void InitFollower()
        {
            try
            {
                if (leader == null || leader_model_id == -1 || ModelCantGetFollowers.Contains(leader_model_id)) return;

                List<CharacterId> expectedFollowers = new List<CharacterId>();
                for (int i = 0; i < 4; i++)
                {
                    CharacterId id = FF9StateSystem.Common.FF9.party.GetCharacterId(i);
                    if (id != CharacterId.NONE && characterDB.ContainsKey(id))
                    {
                        if (characterDB[id].BlackListModelId.Contains(leader_model_id))
                            continue;

                        expectedFollowers.Add(id);
                    }
                }

                int targetLayer = leaderRenderer != null ? leaderRenderer.gameObject.layer : leader.layer;

                int delay = 25;
                foreach (CharacterId id in expectedFollowers)
                {
                    Follower f = new Follower();
                    f.Id = id;
                    f.FramesBehind = delay;
                    f.AnimIdle = characterDB[id].AnimIdle;
                    f.AnimWalk = characterDB[id].AnimWalk;
                    f.AnimRun = characterDB[id].AnimRun;

                    Log.Message($"[Trance Seek] New follower created : {f.Id}, using the model {characterDB[f.Id].ModelName}...");
                    f.Go = ModelFactory.CreateModel(characterDB[f.Id].ModelName, false, true, Configuration.Graphics.ElementsSmoothTexture);
                    GeoTexAnim.addTexAnim(f.Go, characterDB[f.Id].ModelName);

                    f.Go.transform.SetParent(leader.transform.parent, false);
                    f.Go.layer = targetLayer;

                    if (IsWorldMap)
                        f.Go.transform.localScale = new Vector3(-0.00390625f, -0.00390625f, 0.00390625f);
                    else
                        f.Go.transform.localScale = new Vector3(-1f, -1f, 1f);

                    foreach (Renderer renderer in f.Go.GetComponentsInChildren<Renderer>())
                    {
                        renderer.gameObject.layer = targetLayer;

                        foreach (Material material in renderer.materials)
                        {
                            if (!IsWorldMap)
                            {
                                material.shader = ShadersLoader.Find(Configuration.Shaders.FieldCharacterShader);
                                material.SetColor("_Color", new Color32(128, 128, 128, 255));
                            }
                            else if (IsWorldMap && leaderRenderer != null && leaderRenderer.sharedMaterial != null)
                            {
                                material.shader = leaderRenderer.sharedMaterial.shader;
                            }

                            if (material.HasProperty("_Color"))
                                f.CachedMaterials.Add(material);
                        }
                    }

                    foreach (MeshFilter meshFilter in f.Go.GetComponentsInChildren<MeshFilter>())
                        meshFilter.sharedMesh.bounds = new Bounds(Vector3.zero, Vector3.one * float.MaxValue * 0.01f);

                    foreach (SkinnedMeshRenderer skinnedRenderer in f.Go.GetComponentsInChildren<SkinnedMeshRenderer>())
                    {
                        skinnedRenderer.localBounds = new Bounds(Vector3.zero, Vector3.one * float.MaxValue * 0.01f);
                        if (IsWorldMap)
                            skinnedRenderer.updateWhenOffscreen = true;
                    }

                    f.Anim = f.Go.GetComponent<Animation>();
                    AnimationFactory.AddAnimWithAnimatioName(f.Go, f.AnimIdle);
                    AnimationFactory.AddAnimWithAnimatioName(f.Go, f.AnimWalk);
                    AnimationFactory.AddAnimWithAnimatioName(f.Go, f.AnimRun);

                    if (f.Go != null)
                    {
                        if (IsWorldMap)
                            f.Go.transform.localPosition = ff9.GetControlChar().pos;
                        else
                            f.Go.transform.localPosition = leader.transform.localPosition;
                        f.Go.transform.localRotation = leader.transform.localRotation;
                        ApplyFollowerColor(f, GetLeaderColor());
                        f.PositionHistory.Clear();
                    }

                    activeFollowers.Add(f);
                    delay += 25;
                }

                init = true;
            }
            catch (Exception ex)
            {
                Log.Error($"[Trance Seek] Error when creating the follower : {ex.Message}");
            }
        }

        private void ProcessFieldFollowers()
        {
            UIManager uiManager = PersistenSingleton<UIManager>.Instance;
            if (uiManager.IsLoading || uiManager.QuitScene.isShowQuitUI || uiManager.State == UIManager.UIState.Serialize || uiManager.IsPause)
                return;

            if (UIManager.IsUIStateMenu(uiManager.State))
                return;

            if (!init || leader == null)
                InitFollower();

            if (isRunningField == null)
                isRunningField = typeof(FieldMapActorController).GetField("isRunning", BindingFlags.NonPublic | BindingFlags.Instance);

            if (FollowersHidden)
                HideFollowers(false);

            float distanceMoved = Vector3.Distance(leader.transform.localPosition, lastLeaderLocalPos);

            if (distanceMoved > 0.01f)
            {
                LeaderState state = new LeaderState();
                state.LocalPosition = leader.transform.localPosition;
                state.LocalRotation = leader.transform.localRotation;
                state.IsMoving = true;
                state.IsRunning = false;
                state.LightColor = GetLeaderColor();

                FieldMapActorController fmac = leader.GetComponent<FieldMapActorController>();
                if (fmac != null && isRunningField != null)
                    state.IsRunning = (bool)isRunningField.GetValue(fmac);

                foreach (Follower f in activeFollowers)
                    f.PositionHistory.Enqueue(state);
            }

            foreach (Follower f in activeFollowers)
            {
                if (f.Go == null) continue;

                if (IsCharacterModelPresentOnField(f.Id) || !FF9StateSystem.Common.FF9.party.IsInParty(f.Id))
                {
                    if (f.Go.activeSelf) f.Go.SetActive(false);
                    continue;
                }
                else
                    if (!f.Go.activeSelf) f.Go.SetActive(true);

                if (f.PositionHistory.Count > f.FramesBehind)
                {
                    LeaderState target = f.PositionHistory.Dequeue();
                    f.Go.transform.localPosition = target.LocalPosition;
                    f.Go.transform.localRotation = target.LocalRotation;
                    ApplyFollowerColor(f, target.LightColor);

                    if (target.IsRunning || SceneDirector.IsWorldScene())
                        PlayAnimation(f, f.AnimRun);
                    else
                        PlayAnimation(f, f.AnimWalk);
                }
                else
                {
                    PlayAnimation(f, f.AnimIdle);
                }
            }

            lastLeaderLocalPos = leader.transform.localPosition;
        }

        private void ProcessWorldFollowers()
        {
            UIManager uiManager = PersistenSingleton<UIManager>.Instance;
            if (uiManager.IsLoading || uiManager.QuitScene.isShowQuitUI || uiManager.State == UIManager.UIState.Serialize || uiManager.IsPause)
                return;

            if (UIManager.IsUIStateMenu(uiManager.State))
                return;

            if (!init || leader == null)
                InitFollower();

            if (FollowersHidden)
                HideFollowers(false);

            leader_model_id = ff9.GetControlChar().originalActor.model;
            Vector3 MainCharacterPos = ff9.GetControlChar().pos;
            float distanceMoved = Vector3.Distance(MainCharacterPos, lastLeaderLocalPos);

            if (distanceMoved > 32f) // Crappy fix when followers teleporting under the floor (from WM Wrap fonction).
            {
                Vector3 shiftDelta = MainCharacterPos - lastLeaderLocalPos;
                foreach (Follower f in activeFollowers)
                {
                    if (f.Go != null)
                        f.Go.transform.localPosition += shiftDelta;

                    LeaderState[] arrayStates = f.PositionHistory.ToArray();
                    f.PositionHistory.Clear();
                    foreach (LeaderState state in arrayStates)
                    {
                        LeaderState updatedState = state;
                        updatedState.LocalPosition += shiftDelta;
                        f.PositionHistory.Enqueue(updatedState);
                    }
                }
                lastLeaderLocalPos = MainCharacterPos;
                distanceMoved = 0f;
            }
            else if (distanceMoved > 0.01f)
            {
                LeaderState state = new LeaderState();
                state.LocalPosition = MainCharacterPos;
                state.LocalRotation = leader.transform.localRotation;
                state.IsMoving = true;
                state.IsRunning = false;
                state.LightColor = GetLeaderColor();

                foreach (Follower f in activeFollowers)
                    f.PositionHistory.Enqueue(state);
            }

            foreach (Follower f in activeFollowers)
            {
                if (f.Go == null) continue;

                if (!f.Go.activeSelf) f.Go.SetActive(true);

                if (f.PositionHistory.Count > f.FramesBehind)
                {
                    LeaderState target = f.PositionHistory.Dequeue();
                    f.Go.transform.localPosition = target.LocalPosition;
                    f.Go.transform.localRotation = target.LocalRotation;
                    ApplyFollowerColor(f, target.LightColor);
                    PlayAnimation(f, f.AnimRun);
                }
                else
                {
                    PlayAnimation(f, f.AnimIdle);
                }
            }

            lastLeaderLocalPos = MainCharacterPos;
        }

        public void CheckSwapFollower()
        {
            if (!init || leader == null) return;

            List<CharacterId> expectedFollowers = new List<CharacterId>();
            for (int i = 0; i < 4; i++)
            {
                CharacterId id = FF9StateSystem.Common.FF9.party.GetCharacterId(i);
                if (id != CharacterId.NONE && characterDB.ContainsKey(id))
                {
                    if (characterDB[id].BlackListModelId.Contains(leader_model_id))
                        continue;

                    expectedFollowers.Add(id);
                }
            }

            int targetLayer = leaderRenderer != null ? leaderRenderer.gameObject.layer : leader.layer;

            for (int i = 0; i < activeFollowers.Count; i++)
            {
                if (activeFollowers[i].Id != expectedFollowers[i])
                {
                    Follower f = activeFollowers[i];
                    if (f.Go == null) continue;

                    CharacterId newId = expectedFollowers[i];

                    f.Go.SetActive(false);
                    f.Id = newId;
                    f.AnimIdle = characterDB[newId].AnimIdle;
                    f.AnimWalk = characterDB[newId].AnimWalk;
                    f.AnimRun = characterDB[newId].AnimRun;

                    Vector3 SavePosition = f.Go.transform.localPosition;
                    Quaternion SaveRotation = f.Go.transform.localRotation;

                    f.Go = ModelFactory.CreateModel(characterDB[newId].ModelName, false, true, Configuration.Graphics.ElementsSmoothTexture);
                    GeoTexAnim.addTexAnim(f.Go, characterDB[newId].ModelName);

                    f.Go.transform.SetParent(leader.transform.parent, false);
                    f.Go.layer = targetLayer;

                    if (IsWorldMap)
                        f.Go.transform.localScale = new Vector3(-0.00390625f, -0.00390625f, 0.00390625f);
                    else
                        f.Go.transform.localScale = new Vector3(-1f, -1f, 1f);

                    f.CachedMaterials.Clear();
                    foreach (Renderer renderer in f.Go.GetComponentsInChildren<Renderer>())
                    {
                        renderer.gameObject.layer = targetLayer;
                        foreach (Material material in renderer.materials)
                        {
                            if (!IsWorldMap)
                            {
                                material.shader = ShadersLoader.Find(Configuration.Shaders.FieldCharacterShader);
                                material.SetColor("_Color", new Color32(128, 128, 128, 255));
                            }
                            else if (IsWorldMap && leaderRenderer != null && leaderRenderer.sharedMaterial != null)
                            {
                                material.shader = leaderRenderer.sharedMaterial.shader;
                            }

                            if (material.HasProperty("_Color"))
                                f.CachedMaterials.Add(material);
                        }
                    }

                    foreach (MeshFilter meshFilter in f.Go.GetComponentsInChildren<MeshFilter>())
                        meshFilter.sharedMesh.bounds = new Bounds(Vector3.zero, Vector3.one * float.MaxValue * 0.01f);

                    foreach (SkinnedMeshRenderer skinnedRenderer in f.Go.GetComponentsInChildren<SkinnedMeshRenderer>())
                    {
                        skinnedRenderer.localBounds = new Bounds(Vector3.zero, Vector3.one * float.MaxValue * 0.01f);
                        if (IsWorldMap)
                            skinnedRenderer.updateWhenOffscreen = true;
                    }

                    f.Anim = f.Go.GetComponent<Animation>();
                    AnimationFactory.AddAnimWithAnimatioName(f.Go, f.AnimIdle);
                    AnimationFactory.AddAnimWithAnimatioName(f.Go, f.AnimWalk);
                    AnimationFactory.AddAnimWithAnimatioName(f.Go, f.AnimRun);
                    f.Go.transform.localPosition = SavePosition;
                    f.Go.transform.localRotation = SaveRotation;
                    f.Go.SetActive(true);
                }
            }
        }

        private bool IsCharacterModelPresentOnField(CharacterId id)
        {
            if (!characterDB.ContainsKey(id)) return false;

            HashSet<int> blackList = characterDB[id].BlackListModelId;

            for (ObjList objList = ff9.GetActiveObjList(); objList != null; objList = objList.next)
            {
                if (objList.obj != null && objList.obj.cid == 4)
                {
                    Actor actor = (Actor)objList.obj;
                    if (actor.go != null && actor.go != leader)
                    {
                        if (blackList.Contains(actor.model))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void HideFollowers(Boolean hide)
        {
            if (activeFollowers.Count == 0 || FollowersHidden == hide) return;

            foreach (Follower f in activeFollowers)
                if (f.Go != null)
                    f.Go.SetActive(!hide);

            if (hide)
                Log.Message("[Trance Seek] Hiding Followers...");
            else
                Log.Message("[Trance Seek] Showing Followers...");

            FollowersHidden = hide;

            if (!hide)
            {
                foreach (Follower f in activeFollowers)
                {
                    if (f.Go != null)
                    {
                        if (IsWorldMap)
                            f.Go.transform.localPosition = ff9.GetControlChar().pos;
                        else
                            f.Go.transform.localPosition = leader.transform.localPosition;
                        f.Go.transform.localRotation = leader.transform.localRotation;
                        f.PositionHistory.Clear();
                    }
                }
            }
        }

        private Color GetLeaderColor()
        {
            if (leaderRenderer == null || leaderRenderer.sharedMaterial == null) return Color.white;
            if (leaderRenderer.sharedMaterial.HasProperty("_Color"))
                return leaderRenderer.sharedMaterial.GetColor("_Color");
            return Color.white;
        }

        private void ApplyFollowerColor(Follower f, Color color)
        {
            for (int i = 0; i < f.CachedMaterials.Count; i++)
                f.CachedMaterials[i].SetColor("_Color", color);
        }

        private void ClearFollowers()
        {
            if (activeFollowers.Count == 0) return;

            Log.Message("[Trance Seek] Cleaning Followers...");
            foreach (Follower f in activeFollowers)
            {
                if (f.Go != null)
                {
                    f.Go.SetActive(false);
                    Destroy(f.Go);
                }
            }

            leader = null;
            leaderRenderer = null;
            isRunningField = null;
            activeFollowers.Clear();
            IsWorldMap = false;
            init = false;
        }

        private void PlayAnimation(Follower f, string animName)
        {
            if (f.Anim != null && f.Anim.GetClip(animName) != null)
                if (!f.Anim.IsPlaying(animName))
                    f.Anim.Play(animName);
        }

        private void HandleAnimationPause(bool isPaused)
        {
            foreach (Follower f in activeFollowers)
                if (f.Anim != null && f.Anim.enabled == isPaused)
                    f.Anim.enabled = !isPaused;
        }

        private void ClearHistoryFollowers()
        {
            foreach (Follower f in activeFollowers)
                if (f.Anim != null)
                    f.PositionHistory.Clear();
        }
    }

    public static class OverloadOnFieldScriptInitializer
    {
        [ModuleInitializer]
        public static void RunOnAssemblyLoad()
        {
            try
            {
                GameObject watcherObj = new GameObject("TranceSeek_FieldWatcher");
                GameObject.DontDestroyOnLoad(watcherObj);
                watcherObj.AddComponent<OverloadOnFieldScript>();
            }
            catch (Exception ex)
            {
                Log.Error($"[OverloadOnFieldScript] ERROR ON LOADING: {ex.Message}");
            }
        }
    }
}
