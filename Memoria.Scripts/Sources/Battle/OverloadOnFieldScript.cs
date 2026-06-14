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
using static Memoria.Scripts.TranceSeek.TranceSeekDebug;

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
            public int ModelId;
            public string AnimIdle;
            public string AnimWalk;
            public string AnimRun;
            public string AnimInactive;
            public HashSet<Int32> BlackListModelId;

            public FollowerData(int modelName, string animIdle, string animWalk, string animRun, string animInactive, HashSet<Int32> blackListModelId)
            {
                ModelId = modelName;
                AnimIdle = animIdle;
                AnimWalk = animWalk;
                AnimRun = animRun;
                AnimInactive = animInactive;
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
            public string AnimInactive;
            public int FramesBehind;
            public int IdleTimer;
            public Queue<LeaderState> PositionHistory = new Queue<LeaderState>();
            public List<Material> CachedMaterials = new List<Material>();
        }

        private Dictionary<CharacterId, FollowerData> characterDB = new Dictionary<CharacterId, FollowerData>()
        {
            { CharacterId.Zidane, new FollowerData(98, "ANH_MAIN_F0_ZDN_IDLE", "ANH_MAIN_F0_ZDN_WALK", "ANH_MAIN_F0_ZDN_RUN", "ANH_MAIN_F0_ZDN_BREAK1_XARM", new HashSet<Int32>(){98, 532, 203, 569, 310, 285, 5414}) },
            { CharacterId.Vivi, new FollowerData(8, "ANH_MAIN_F0_VIV_IDLE", "ANH_MAIN_F0_VIV_WALK", "ANH_MAIN_F0_VIV_RUN", "ANH_MAIN_F0_VIV_BREAK1", new HashSet<Int32>(){5415, 8, 662}) },
            { CharacterId.Garnet, new FollowerData(185, "ANH_MAIN_F0_GRN_IDLE", "ANH_MAIN_F0_GRN_WALK", "ANH_MAIN_F0_GRN_RUN", "ANH_MAIN_F0_GRN_BREAK_2", new HashSet<Int32>(){526, 532, 557, 202, 205, 666, 557, 671, 309, 281, 283, 287, 288, 185}) },
            { CharacterId.Steiner, new FollowerData(5489, "ANH_MAIN_F0_STN_IDLE", "ANH_MAIN_F0_STN_WALK", "ANH_MAIN_F0_STN_RUN", "ANH_MAIN_F0_STN_BREAK_1", new HashSet<Int32>(){286, 655, 5489, 658, 526}) },
            { CharacterId.Freya, new FollowerData(192, "ANH_MAIN_F0_FRJ_IDLE", "ANH_MAIN_F0_FRJ_WALK", "ANH_MAIN_F0_FRJ_RUN", "ANH_MAIN_F0_FRJ_DANCE_IDLE", new HashSet<Int32>(){290, 297, 192}) },
            { CharacterId.Quina, new FollowerData(273, "ANH_MAIN_F0_KUI_IDLE", "ANH_MAIN_F0_KUI_WALK", "ANH_MAIN_F0_KUI_RUN", "ANH_MAIN_F0_KUI_BERO_1", new HashSet<Int32>(){289, 273, 295}) },
            { CharacterId.Eiko, new FollowerData(443, "ANH_MAIN_F0_EIK_IDLE", "ANH_MAIN_F0_EIK_WALK", "ANH_MAIN_F0_EIK_RUN", "ANH_MAIN_F0_EIK_BREAK_2", new HashSet<Int32>(){284, 291, 443, 570}) },
            { CharacterId.Amarant, new FollowerData(509, "ANH_MAIN_F0_SLM_IDLE", "ANH_MAIN_F0_SLM_WALK", "ANH_MAIN_F0_SLM_RUN", "ANH_MAIN_F0_SLM_BYE", new HashSet<Int32>(){572, 509, 444}) },
            { CharacterId.Cinna, new FollowerData(107, "ANH_SSUB_F0_CNA_IDLE", "ANH_SUB_F0_CNA_WALK", "ANH_SUB_F0_CNA_RUN", "ANH_SUB_F0_CNA_SIGN", new HashSet<Int32>(){39, 107, 661}) },
            { CharacterId.Marcus, new FollowerData(109, "ANH_SUB_F0_MRC_IDLE", "ANH_SUB_F0_MRC_WALK", "ANH_SUB_F0_MRC_RUN", "ANH_SUB_F0_MRC_TAN", new HashSet<Int32>(){45, 109, 660}) },
            { CharacterId.Blank, new FollowerData(5467, "ANH_SUB_F0_BLN_IDLE", "ANH_SUB_F0_BLN_WALK", "ANH_SUB_F0_BLN_RUN", "ANH_SUB_F0_BLN_TAN", new HashSet<Int32>(){42, 608, 639, 5467, 190, 659}) },
            { CharacterId.Beatrix, new FollowerData(204, "ANH_SUB_F0_BTX_IDLE", "ANH_SUB_F0_BTX_WALK", "ANH_SUB_F0_BTX_RUN", "ANH_SUB_F0_BTX_HAIR", new HashSet<Int32>(){427, 204, 358}) },
            { (CharacterId)12, new FollowerData(368, "ANH_SUB_F0_SBW_IDLE", "ANH_SUB_F0_SBW_WALK", "ANH_SUB_F0_SBW_RUN", "ANH_SUB_F0_SBW_GIVE_ME", new HashSet<Int32>(){427, 204, 358}) }
        };

        private List<Follower> activeFollowers = new List<Follower>();
        private Vector3 lastLeaderLocalPos;
        private GameObject leader;
        private Actor actorleader;
        private Renderer leaderRenderer;
        private int leader_model_id;
        private Boolean init;
        private UIManager.UIState lastUiState;
        private Boolean IsWorldMap;
        private Boolean FollowersHidden;
        private static readonly HashSet<Int32> BlackListAnimationId =

            new HashSet<Int32>(new[] {
                10539, // Climbing (Ladder)
                13055, // Climbing Up (Rope)
                13059, // Climbing Jump (Rope)
                13073, // Climbing Down (Rope)
                10633 // Mounting Gargant
            });

        private static readonly HashSet<Int32> BlackListFieldId = 
            new HashSet<Int32>(new[] { 70, 152, 209, 260, 261, 453, 454, 606, 655, 767, 768, 769, 811, 813,
                814, 816, 954, 955, 1400, 1401, 1402, 1403, 1404, 1462, 1609, 1659, 1800, 2261, 2750, 2751, 2752, 2754, 2755, 2756, 2850, 2851, 2852, 2853, 2854, 2855, 2856, 2951, 2952, 2953,
            2928, 2929, 2930, 2931, 2932, 2933, 2934, 3000, 3001, 3002, 3003, 3004, 3005, 3006, 3007, 3008, 3009, 3010, 3011, 3012}); // End of the game
        private static readonly HashSet<Int32> ModelCantGetFollowers = new HashSet<Int32>(new[] { 317, 312, 320, 321, 308 });

        // I use this instead of checking if the player is running => In some scenes like Zidane running to save Eiko : he is running but player don't control it.
        private static readonly HashSet<Int32> ActorAnimWalking = new HashSet<Int32>(new[] {
            203, 145, 2092, 2006, 2559, 3231, 7505, 8311, 473, 476, 464, 2982, 8347,
            38, 419, 2091, 2005, 2558, 3230, 7506, 8312, 105, 365, 5222, 2981, 8348
        });

        private int speedFactor => HonoBehaviorSystem.Instance.IsFastForwardModeActive() ? HonoBehaviorSystem.Instance.GetFastForwardFactor() : 1;
        private Boolean BlackListCondition => ((FF9StateSystem.Common.FF9.fldMapNo == 908 && GameState.ScenarioCounter < 4400)
            || (FF9StateSystem.Common.FF9.fldMapNo == 953 && GameState.ScenarioCounter == 4530)
            || (FF9StateSystem.Common.FF9.fldMapNo == 1014 && (GetLeaderAnimID() == 581 || GetLeaderAnimID() == 3519)));

        // A FAIRE => Quand le groupe observe le rituel d'Eiko à Gulg.
        // Après avoir battu Siamois
        // Quand on libere Hilda

        private void LateUpdate()
        {
            UIManager uiManager = PersistenSingleton<UIManager>.Instance;
            UIManager.UIState currentState = uiManager.State;


            CheckLeader();
            if (lastUiState == UIManager.UIState.PartySetting && (currentState == UIManager.UIState.FieldHUD || currentState == UIManager.UIState.WorldHUD))
                CheckSwapFollower();
            else if (FF9StateSystem.Common.FF9.fldMapNo != -1 && SceneDirector.IsFieldScene() && !SceneDirector.Instance.IsFading)
                ProcessFieldFollowers();
            else if (FF9StateSystem.Common.FF9.wldMapNo != -1 && SceneDirector.IsWorldScene() && !SceneDirector.Instance.IsFading)
                ProcessWorldFollowers();
            else
                ClearFollowers();

            if (BlackListCondition || ModelCantGetFollowers.Contains(leader_model_id) || MBG.Instance.IsPlaying() > 1 ||
            BlackListAnimationId.Contains(actorleader.anim) || BlackListFieldId.Contains(FF9StateSystem.Common.FF9.fldMapNo) || (actorleader.flags & 1) == 0)
                HideFollowers(true);

            HandleAnimationPause(uiManager.IsPause);

            if (UnityXInput.Input.GetKeyDown(KeyCode.KeypadMultiply))
            {
                Log.Message("[Trance Seek] leader_model_id : " + leader_model_id);
                Log.Message("[Trance Seek] actorleader.anim : " + actorleader.anim);
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
                EnsureLeaderWorldMapShader();
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

        private void EnsureLeaderWorldMapShader()
        {
            if (!IsWorldMap || leader == null) return;

            foreach (Renderer renderer in leader.GetComponentsInChildren<Renderer>())
            {
                if (renderer.sharedMaterial != null && renderer.sharedMaterial.shader != null && renderer.sharedMaterial.shader.name != "WorldMap/Actor")
                {
                    Shader wmShader = ShadersLoader.Find("WorldMap/Actor");
                    if (wmShader != null)
                    {
                        foreach (Material material in renderer.materials)
                        {
                            material.shader = wmShader;
                        }
                    }
                }
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
                    f.AnimInactive = characterDB[id].AnimInactive;
                    ResetTimerInactiveAnimation(f);

                    if (!FF9BattleDB.GEO.TryGetValue(characterDB[id].ModelId, out String modelName))
                    {
                        Log.Warning($"[Trance Seek] ERROR : can't load follower with ModelId : {characterDB[id].ModelId}...");
                        continue;
                    }

                    //Log.Message($"[Trance Seek] New follower created : {f.Id}, using the model {modelName}...");
                    f.Go = ModelFactory.CreateModel(modelName, false, true, Configuration.Graphics.ElementsSmoothTexture);
                    GeoTexAnim.addTexAnim(f.Go, modelName);

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
                            if (IsWorldMap)
                            {
                                material.shader = ShadersLoader.Find("WorldMap/Actor");
                            }
                            else
                            {                              
                                material.shader = ShadersLoader.Find(Configuration.Shaders.FieldCharacterShader);
                                material.SetColor("_Color", new Color32(128, 128, 128, 255));
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
                    AnimationFactory.AddAnimWithAnimatioName(f.Go, f.AnimInactive);

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

            if (FollowersHidden)
                HideFollowers(false);

            float distanceMoved = Vector3.Distance(leader.transform.localPosition, lastLeaderLocalPos);

            if (distanceMoved > 0.01f)
            {
                LeaderState state = new LeaderState();
                state.LocalPosition = leader.transform.localPosition;
                state.LocalRotation = leader.transform.localRotation;
                state.IsMoving = true;
                state.IsRunning = ActorAnimWalking.Contains(actorleader.anim);
                state.LightColor = GetLeaderColor();

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

                int effectiveFramesBehind = Mathf.Max(1, f.FramesBehind / speedFactor);

                if (f.PositionHistory.Count > effectiveFramesBehind)
                {
                    LeaderState target = default;
                    while (f.PositionHistory.Count > effectiveFramesBehind)
                        target = f.PositionHistory.Dequeue();

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
                    f.IdleTimer -= speedFactor;
                    ApplyFollowerColor(f, GetLeaderColor());
                    if (f.IdleTimer < 0)
                    {
                        PlayAnimation(f, f.AnimInactive);
                        ResetTimerInactiveAnimation(f);
                    }
                    else if (!f.Anim.IsPlaying(f.AnimInactive))
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

                int effectiveFramesBehind = Mathf.Max(1, f.FramesBehind / speedFactor);

                if (f.PositionHistory.Count > effectiveFramesBehind)
                {
                    LeaderState target = default;
                    while (f.PositionHistory.Count > effectiveFramesBehind)
                        target = f.PositionHistory.Dequeue();

                    f.Go.transform.localPosition = target.LocalPosition;
                    f.Go.transform.localRotation = target.LocalRotation;
                    ApplyFollowerColor(f, target.LightColor);
                    PlayAnimation(f, f.AnimRun);
                }
                else
                {
                    f.IdleTimer -= speedFactor;
                    ApplyFollowerColor(f, GetLeaderColor());
                    if (f.IdleTimer < 0)
                    {
                        PlayAnimation(f, f.AnimInactive);
                        ResetTimerInactiveAnimation(f);
                    }
                    else if (!f.Anim.IsPlaying(f.AnimInactive))
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
                    f.AnimInactive = characterDB[newId].AnimInactive;
                    ResetTimerInactiveAnimation(f);

                    if (!FF9BattleDB.GEO.TryGetValue(characterDB[newId].ModelId, out String modelName))
                    {
                        Log.Warning($"[Trance Seek] ERROR : can't load follower with ModelId : {characterDB[newId].ModelId}...");
                        continue;
                    }

                    Vector3 SavePosition = f.Go.transform.localPosition;
                    Quaternion SaveRotation = f.Go.transform.localRotation;

                    f.Go = ModelFactory.CreateModel(modelName, false, true, Configuration.Graphics.ElementsSmoothTexture);
                    GeoTexAnim.addTexAnim(f.Go, modelName);

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
                            if (IsWorldMap)
                            {
                                material.shader = ShadersLoader.Find("WorldMap/Actor");
                            }
                            else
                            {
                                material.shader = ShadersLoader.Find(Configuration.Shaders.FieldCharacterShader);
                                material.SetColor("_Color", new Color32(128, 128, 128, 255));
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
                    AnimationFactory.AddAnimWithAnimatioName(f.Go, f.AnimInactive);
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
                    if (actor.go != null && actor.go != leader && (actor.flags & 1) != 0)
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
                        ApplyFollowerColor(f, GetLeaderColor());
                    }
                }
            }
        }

        private int GetLeaderModelID()
        {
            if (actorleader == null) return -1;

            return actorleader.model;
        }

        private int GetLeaderAnimID()
        {
            if (actorleader == null) return -1;

            return actorleader.anim;
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

        private void ResetTimerInactiveAnimation(Follower f)
        {
            f.IdleTimer = UnityEngine.Random.Range(2000, 8000);
        }

        private void ClearFollowers()
        {
            if (activeFollowers.Count == 0) return;

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
            activeFollowers.Clear();
            IsWorldMap = false;
            init = false;
        }

        private void PlayAnimation(Follower f, string animName)
        {
            if (f.Anim != null && f.Anim.GetClip(animName) != null)
            {
                if (!f.Anim.IsPlaying(animName))
                    f.Anim.Play(animName);

                float speedFactor = HonoBehaviorSystem.Instance.IsFastForwardModeActive()
                    ? (float)HonoBehaviorSystem.Instance.GetFastForwardFactor()
                    : 1f;

                AnimationState state = f.Anim[animName];
                if (state != null)
                    state.speed = speedFactor;
            }
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
}
