using Assets.Scripts.Common;
using Memoria.Data;
using Memoria.Prime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Memoria.Scripts.TranceSeek
{
    public class OverloadOnFieldScript : MonoBehaviour
    {
        private bool isFollowerFeatureEnabled;

        private static bool _lastBoosterState = false;
        private static bool _wasAccessoryEquipped = false;
        private static bool _isManualNoEncounterActivated = false;

        private void Awake()
        {
            isFollowerFeatureEnabled = Configuration.Mod.FolderNames.Contains("TranceSeek/Options/FollowersFeature");
        }

        private void CheckEncounterBooster()
        {
            if (FF9StateSystem.Settings == null || FF9StateSystem.Common.FF9 == null)
                return;

            if (PersistenSingleton<UIManager>.Instance != null && PersistenSingleton<UIManager>.Instance.State == UIManager.UIState.Title)
            {
                _lastBoosterState = false;
                _wasAccessoryEquipped = false;
                _isManualNoEncounterActivated = false;
                FF9StateSystem.Settings.IsBoosterButtonActive[4] = false;
                return;
            }

            bool isAccessoryEquipped = false;
            if (FF9StateSystem.Common.FF9.party != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    PLAYER p = FF9StateSystem.Common.FF9.party.member[i];
                    if (p != null && p.equip != null && p.equip.Accessory == TranceSeekRegularItem.MalboroIncense)
                    {
                        isAccessoryEquipped = true;
                        break;
                    }
                }
            }

            bool currentBoosterState = FF9StateSystem.Settings.IsBoosterButtonActive[4];

            if (currentBoosterState != _lastBoosterState)
            {
                _isManualNoEncounterActivated = currentBoosterState;
                _lastBoosterState = currentBoosterState;
            }

            if (isAccessoryEquipped != _wasAccessoryEquipped)
            {
                _wasAccessoryEquipped = isAccessoryEquipped;

                bool targetBoosterState = isAccessoryEquipped || _isManualNoEncounterActivated;

                if (currentBoosterState != targetBoosterState)
                {
                    FF9StateSystem.Settings.IsBoosterButtonActive[4] = targetBoosterState;
                    _lastBoosterState = targetBoosterState;
                    UpdateBoosterUI(targetBoosterState);
                }
            }
            else
            {
                if (isAccessoryEquipped && !currentBoosterState)
                {
                    FF9StateSystem.Settings.IsBoosterButtonActive[4] = true;
                    _lastBoosterState = true;
                    _isManualNoEncounterActivated = false;
                    UpdateBoosterUI(true);
                }
            }
        }
        private void UpdateBoosterUI(bool flag)
        {
            if (PersistenSingleton<UIManager>.Instance != null && PersistenSingleton<UIManager>.Instance.Booster != null)
            {
                PersistenSingleton<UIManager>.Instance.Booster.SetBoosterHudIcon(BoosterType.NoRandomEncounter, flag);
                PersistenSingleton<UIManager>.Instance.Booster.SetBoosterButton(BoosterType.NoRandomEncounter, flag);
            }
        }

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

            public GameObject ShadowObj;
            public Transform ShadowTransform;
            public MeshRenderer ShadowRenderer;

            public Transform RootBone;
            public Color LastAppliedColor = Color.clear;
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
            { CharacterId.Cinna, new FollowerData(107, "ANH_SUB_F0_CNA_IDLE", "ANH_SUB_F0_CNA_WALK", "ANH_SUB_F0_CNA_RUN", "ANH_SUB_F0_CNA_SIGN", new HashSet<Int32>(){39, 107, 661}) },
            { CharacterId.Marcus, new FollowerData(109, "ANH_SUB_F0_MRC_IDLE", "ANH_SUB_F0_MRC_WALK", "ANH_SUB_F0_MRC_RUN", "ANH_SUB_F0_MRC_TAN", new HashSet<Int32>(){45, 109, 660}) },
            { CharacterId.Blank, new FollowerData(5467, "ANH_SUB_F0_BLN_IDLE", "ANH_SUB_F0_BLN_WALK", "ANH_SUB_F0_BLN_RUN", "ANH_SUB_F0_BLN_TAN", new HashSet<Int32>(){42, 608, 639, 5467, 190, 659}) },
            { CharacterId.Beatrix, new FollowerData(204, "ANH_SUB_F0_BTX_IDLE", "ANH_SUB_F0_BTX_WALK", "ANH_SUB_F0_BTX_RUN", "ANH_SUB_F0_BTX_HAIR", new HashSet<Int32>(){427, 204, 358}) },
            { (CharacterId)12, new FollowerData(368, "ANH_SUB_F0_SBW_IDLE", "ANH_SUB_F0_SBW_WALK", "ANH_SUB_F0_SBW_RUN", "ANH_SUB_F0_SBW_GIVE_ME", new HashSet<Int32>(){427, 204, 368}) }
        };

        private Dictionary<CharacterId, Follower> followerPool = new Dictionary<CharacterId, Follower>();
        private List<Follower> activeFollowers = new List<Follower>();

        private Boolean init;
        private Vector3 lastLeaderLocalPos;
        private GameObject leader;
        private Actor actorleader;
        private Renderer leaderRenderer;
        private int leader_model_id;
        private int partynumber;
        private UIManager.UIState lastUiState;
        private Boolean IsWorldMap;
        private Boolean FollowersHidden;

        private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");
        private Renderer[] cachedLeaderRenderers;
        private HashSet<int> modelsOnFieldCache = new HashSet<int>();
        private Dialog cachedAteDialog;

        // For ATE system
        private bool isATEPending = false;
        private bool isPlayingATE = false;
        private bool wasATEMenuOpen = false;
        private int lastATEChoiceCount = 0;
        private int lastATESelectedChoice = -1;
        private int lastFieldMapNo = -1;

        private static readonly HashSet<Int32> BlackListAnimationId =
            new HashSet<Int32>(new[] {
                10539, // Climbing (Ladder)
                13055, // Climbing Up (Rope)
                13059, // Climbing Jump (Rope)
                13073, // Climbing Down (Rope)
                10633, // Mounting Gargant
                159 // Dagga climbing
            });

        private static readonly HashSet<Int32> BlackListFieldId =
            new HashSet<Int32>(new[] { 70, 152, 209, 260, 261, 453, 454, 606, 655, 767, 768, 769, 811, 813,
                814, 816, 954, 955, 1400, 1401, 1402, 1403, 1404, 1462, 1609, 1659, 1704, 1800, 2055, 2261, 2608, 2700, 2701, 2702, 2703, 2704, 2750, 2751, 2752, 2753, 2754, 2755, 2756, 2850, 2851, 2852, 2853, 2854, 2855, 2856, 2951, 2952, 2953,
            2928, 2929, 2930, 2931, 2932, 2933, 2934, 3000, 3001, 3002, 3003, 3004, 3005, 3006, 3007, 3008, 3009, 3010, 3011, 3012}); // End of the game

        private static readonly HashSet<Int32> ModelCantGetFollowers = new HashSet<Int32>(new[] { 317, 312, 320, 321, 308 });

        private static readonly HashSet<Int32> ActorAnimWalking = new HashSet<Int32>(new[] {
            203, 145, 2092, 2006, 2559, 3231, 7505, 8311, 473, 476, 464, 2982, 8347,
            38, 419, 2091, 2005, 2558, 3230, 7506, 8312, 105, 365, 5222, 2981, 8348
        });

        private static readonly HashSet<Int32> ActorAnimIdle = new HashSet<Int32>(new[]
        {
            3286, 3349, 5006, 5346, 12168, 6875, 6880, 7113, 7107, 6463,
            4533, 8245, 964, 10566, 4684, 4694, 7118, 3140, 8243, 1484,
            10245, 7503, 11795, 2556, 3724, 4988, 3728, 3731, 3735, 2094,
            2089, 3208, 3228, 8307, 8155, 8157, 3278, 2012, 6059, 2001,
            2732, 11736, 11740, 11738, 2469, 148, 11889, 8173, 2624, 2623,
            7922, 200, 7933, 2633, 3511, 8402, 8400, 2616, 8505, 10607,
            8853, 2670, 3602, 11107, 6565, 6562, 9786, 5275, 7033, 10372,
            11417, 7412, 7409, 7592, 7247, 11782, 760, 7659, 7249, 7466,
            7252, 3108, 7254, 7664, 8623, 11042, 896, 11774, 6908, 6477,
            454, 4974, 1426, 2800, 552, 555, 2095, 85, 560, 1500,
            2029, 1965, 567, 9851, 2229, 3174, 3190, 578, 2046, 2057,
            2171, 458, 11363, 2953, 3202, 11623, 7525, 10255, 580, 1511,
            2506, 2370, 1540, 1303, 1217, 1908, 6859, 7384, 7255, 7271,
            2886, 2884, 4459, 8070, 8068, 8056, 7275, 4662, 1531, 2783,
            10357, 1310, 800, 7148, 3201, 7173, 589, 7135, 1227, 926,
            617, 6535, 11058, 1235, 2904, 7347, 1921, 2099, 2107, 2494,
            4689, 4676, 8450, 641, 5762, 653, 664, 1121, 2968, 6188,
            1536, 691, 3430, 696, 687, 11139, 3410, 704, 706, 4478,
            1931, 712, 8263, 2204, 1259, 1257, 9859, 725, 2235, 2289,
            1937, 2516, 3250, 1645, 509, 729, 1285, 1292, 738, 9865,
            2785, 2526, 14667, 1555, 1125, 743, 2241, 2536, 14679, 1152,
            749, 2293, 6045, 780, 2068, 462, 1953, 1327, 1328, 13191,
            13199, 467, 4608, 2978, 5328, 2547, 8328, 11676, 12568, 12579,
            470, 1067, 1069, 1061, 12273, 12589, 1698, 5681, 5683, 5687,
            12891, 13075, 13079, 10202, 10206, 10210, 8492, 8478, 11127, 474,
            1094, 3311, 3324, 8085, 1093, 1758, 1742, 1717, 1722, 1774,
            1203, 8885, 8344, 10525, 521, 2269, 2266, 2277, 5957, 3249,
            526, 2758, 1192, 2819, 2814, 3268, 6622, 7295, 11533, 2490,
            124, 2147, 2213, 537, 548, 152, 4716, 4723, 6140, 6142,
            4707, 5144, 5134, 5127, 6150, 5114, 3132, 5131, 3598, 5110,
            5105, 5098, 3284, 3347, 5007, 5345, 12166, 6874, 6877, 7114,
            7108, 6464, 4531, 8246, 963, 10567, 4683, 4693, 7117, 3138,
            8242, 1483, 10244, 7504, 11793, 2557, 3722, 4961, 3726, 3730,
            3734, 2093, 2090, 3206, 3229, 8308, 8156, 8158, 3279, 2011,
            6060, 2002, 2733, 11737, 11741, 11739, 2468, 573, 11887, 8174,
            2625, 2622, 7920, 324, 7931, 2632, 3512, 8401, 8399, 2617,
            8506, 10606, 8852, 2671, 3600, 11104, 6566, 6561, 9787, 5260,
            7034, 10373, 11415, 7411, 7410, 7589, 7248, 11780, 759, 7660,
            7250, 7464, 7251, 3106, 7253, 7663, 8621, 11041, 895, 11772,
            6906, 6478, 453, 4981, 1425, 2801, 429, 596, 2096, 84,
            606, 1487, 2030, 1966, 610, 9850, 2230, 3175, 3191, 615,
            2045, 2058, 2172, 840, 11361, 2954, 3203, 11621, 7526, 10254,
            5242, 1510, 2507, 2371, 1537, 1298, 1212, 1907, 6858, 7383,
            7256, 7272, 2887, 2885, 4460, 8069, 8067, 8055, 7276, 4661,
            1527, 2784, 10356, 1305, 799, 7147, 3200, 7174, 623, 7136,
            1222, 921, 631, 6536, 11057, 1230, 2905, 7348, 1922, 2100,
            2108, 2495, 4690, 4675, 8449, 636, 5761, 29, 96, 1122,
            2177, 2967, 6185, 1535, 542, 3428, 644, 5291, 11137, 3407,
            651, 654, 4477, 1932, 663, 8264, 2203, 1242, 1240, 9858,
            665, 2236, 2290, 1938, 2517, 3251, 1643, 508, 5, 1274,
            1281, 75, 9864, 2786, 2527, 14666, 1550, 1126, 675, 2242,
            2537, 14678, 1153, 682, 2294, 6046, 773, 2067, 5221, 1954,
            1320, 1321, 13189, 13197, 351, 4605, 2977, 5326, 2546, 8327,
            11663, 12567, 12580, 101, 1058, 1060, 1052, 12272, 12590, 1695,
            5680, 5682, 5685, 12889, 13063, 13077, 10200, 10204, 10208, 8491,
            8477, 11125, 363, 1088, 3312, 3323, 8086, 1087, 1756, 1740,
            1716, 1719, 1772, 1197, 8883, 8343, 10524, 5276, 2270, 2265,
            2278, 5956, 3248, 5350, 2759, 1187, 2818, 2815, 3269, 6621,
            7296, 11531, 2491, 118, 2148, 2214, 5388, 5398, 149, 4715,
            4724, 6139, 6141, 4706, 5145, 5135, 5126, 6149, 5113, 3130,
            5130, 3596, 5109, 5106, 5097, 4047, 6508, 6515, 8306, 11836,
            8838, 13184
        });

        private int speedFactor => HonoBehaviorSystem.Instance.IsFastForwardModeActive() ? HonoBehaviorSystem.Instance.GetFastForwardFactor() : 1;

        private bool BlackListCondition
        {
            get
            {
                int scenario = GameState.ScenarioCounter;
                switch (FF9StateSystem.Common.FF9.fldMapNo)
                {
                    case 652: return scenario < 3700;
                    case 908: return scenario < 4400;
                    case 953: return scenario == 4530;
                    case 1014:
                        int animId = GetLeaderAnimID();
                        return animId == 581 || animId == 3519;
                    case 2550:
                    case 2551:
                    case 2552:
                    case 2553:
                    case 2554:
                        return scenario >= 10600 && scenario <= 10700;
                    case 2706:
                    case 2707:
                    case 2708:
                        return scenario == 10950 && FF9StateSystem.Common.FF9.party.MemberCount < 4; // You are not alone.
                    case 2711:
                        return isPlayingATE; // Elevator ATE
                    default: return false;
                }
            }
        }

        private bool ForceHidden
        {
            get
            {
                if (FF9StateSystem.EventState.gScriptDictionary.TryGetValue(1007, out Dictionary<Int32, Int32> dict))
                {
                    if (dict.TryGetValue(2, out int hideFollowersValue))
                        return hideFollowersValue > 0;
                }
                return false;
            }
        }

        private void CheckATEState()
        {
            int currentMap = FF9StateSystem.Common.FF9.fldMapNo;

            if (lastFieldMapNo != -1 && currentMap != lastFieldMapNo)
            {
                if (isATEPending)
                {
                    Log.Message("[OverloadOnFieldScript] Playing an ATE !");
                    isPlayingATE = true;
                    isATEPending = false;
                }
                else if (isPlayingATE)
                {
                    isPlayingATE = false;
                }
            }
            lastFieldMapNo = currentMap;

            Dialog activeATEDialog = FindActiveATEDialog();

            if (activeATEDialog != null)
            {
                wasATEMenuOpen = true;
                lastATEChoiceCount = activeATEDialog.ChoiceNumber;
                lastATESelectedChoice = DialogManager.SelectChoice;
            }
            else if (wasATEMenuOpen)
            {
                bool isCancelOption = (lastATEChoiceCount > 0) && (lastATESelectedChoice == lastATEChoiceCount - 1);
                if (!isCancelOption && lastATESelectedChoice >= 0)
                    isATEPending = true;

                wasATEMenuOpen = false;
            }
        }

        private Dialog FindActiveATEDialog()
        {
            if (cachedAteDialog != null && cachedAteDialog.gameObject.activeSelf && cachedAteDialog.CapType == Dialog.CaptionType.ActiveTimeEvent)
                return cachedAteDialog;

            if (Time.frameCount % 10 == 0) // Check for time to time.
            {
                Dialog[] dialogs = UnityEngine.Object.FindObjectsOfType<Dialog>();
                for (int i = 0; i < dialogs.Length; i++)
                {
                    Dialog d = dialogs[i];
                    if (d != null && d.gameObject.activeSelf && d.CapType == Dialog.CaptionType.ActiveTimeEvent)
                    {
                        cachedAteDialog = d;
                        return d;
                    }
                }
            }
            return null;
        }

        private void FixZidaneWorldMapWeapon()
        {
            if (!IsWorldMap || leader == null || leader_model_id != 310 || cachedLeaderRenderers == null)
                return;

            for (int i = 0; i < cachedLeaderRenderers.Length; i++)
            {
                Renderer rend = cachedLeaderRenderers[i];
                if (rend != null && (rend.gameObject.name == "battle_model0" || rend.gameObject.name == "battle_model1"))
                {
                    if (rend.enabled)
                        rend.enabled = false;
                }
            }
        }

        private void LateUpdate()
        {
            CheckEncounterBooster();

            if (!isFollowerFeatureEnabled)
                return;

            UIManager uiManager = PersistenSingleton<UIManager>.Instance;
            UIManager.UIState currentState = uiManager.State;

            CheckLeaderAndParty();
            CheckATEState();
            FixZidaneWorldMapWeapon();

            if (lastUiState == UIManager.UIState.PartySetting && (currentState == UIManager.UIState.FieldHUD || currentState == UIManager.UIState.WorldHUD))
                CheckSwapFollower();
            else if ((SceneDirector.IsFieldScene() || SceneDirector.IsWorldScene()) && !SceneDirector.Instance.IsFading)
            {
                UpdateModelsOnFieldCache();
                ProcessFollowers();
            }
            else
            {
                ClearFollowers();
            }

            if (actorleader != null && ((actorleader.flags & 1) == 0 || ForceHidden || ModelCantGetFollowers.Contains(leader_model_id) || BlackListFieldId.Contains(FF9StateSystem.Common.FF9.fldMapNo)
                || BlackListAnimationId.Contains(actorleader.anim) || MBG.Instance.IsPlaying() > 1 || BlackListCondition || isPlayingATE))
            {
                HideFollowers(true);
            }

            HandleAnimationPause(uiManager.IsPause);

            if (UnityXInput.Input.GetKeyDown(KeyCode.KeypadMultiply))
            {
                Log.Message("[Trance Seek] leader_model_id : " + leader_model_id);
                if (actorleader != null) Log.Message("[Trance Seek] actorleader.anim : " + actorleader.anim);
                if (IsWorldMap) Log.Message($"[Trance Seek] WM Actor Position {ff9.GetControlChar().pos}");
            }

            lastUiState = currentState;
        }

        private void CheckLeaderAndParty()
        {
            EventEngine engine = PersistenSingleton<EventEngine>.Instance;
            if (engine == null) return;

            GameObject oldLeader = leader;
            int CurrentMemberCounter = FF9StateSystem.Common.FF9.party.MemberCount;

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
                leader_model_id = actorleader.model;
                IsWorldMap = true;
                ApplyLeaderWorldMapShader();
            }
            else
            {
                leader = null;
                actorleader = null;
                IsWorldMap = false;
                leader_model_id = -1;
            }

            if (leader != oldLeader || CurrentMemberCounter != partynumber)
            {
                leaderRenderer = leader != null ? leader.GetComponentInChildren<Renderer>() : null;
                cachedLeaderRenderers = leader != null ? leader.GetComponentsInChildren<Renderer>(true) : null;

                partynumber = FF9StateSystem.Common.FF9.party.MemberCount;
                ClearHistoryFollowers();
                CheckSwapFollower();
            }
        }

        private void ApplyLeaderWorldMapShader()
        {
            if (!IsWorldMap || cachedLeaderRenderers == null) return;

            Shader wmShader = ShadersLoader.Find("WorldMap/Actor");
            if (wmShader == null) return;

            for (int i = 0; i < cachedLeaderRenderers.Length; i++)
            {
                Renderer renderer = cachedLeaderRenderers[i];
                if (renderer != null && renderer.sharedMaterial != null && renderer.sharedMaterial.shader != null && renderer.sharedMaterial.shader.name != "WorldMap/Actor")
                {
                    foreach (Material material in renderer.materials)
                    {
                        material.shader = wmShader;
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

                activeFollowers.Clear();
                int targetLayer = leaderRenderer != null ? leaderRenderer.gameObject.layer : leader.layer;
                int delay = 25;

                foreach (CharacterId id in expectedFollowers)
                {
                    Follower f = GetOrCreateFollower(id, targetLayer);
                    if (f == null) continue;

                    f.FramesBehind = delay;

                    if (f.Go != null)
                    {
                        if (IsWorldMap)
                            f.Go.transform.localPosition = ff9.GetControlChar().pos;
                        else
                            f.Go.transform.localPosition = leader.transform.localPosition;
                        f.Go.transform.localRotation = leader.transform.localRotation;
                        f.PositionHistory.Clear();
                        f.Go.SetActive(true);
                    }

                    activeFollowers.Add(f);
                    delay += 25;
                }
                partynumber = FF9StateSystem.Common.FF9.party.MemberCount;
                init = true;
            }
            catch (Exception ex)
            {
                Log.Error($"[Trance Seek] Error when creating the follower : {ex.Message}");
            }
        }

        private Follower GetOrCreateFollower(CharacterId id, int targetLayer)
        {
            if (followerPool.TryGetValue(id, out Follower existingFollower))
            {
                if (existingFollower.Go != null)
                {
                    existingFollower.Go.layer = targetLayer;
                    foreach (Renderer r in existingFollower.Go.GetComponentsInChildren<Renderer>())
                        r.gameObject.layer = targetLayer;
                }
                return existingFollower;
            }

            Follower f = new Follower();
            f.Id = id;
            f.AnimIdle = characterDB[id].AnimIdle;
            f.AnimWalk = characterDB[id].AnimWalk;
            f.AnimRun = characterDB[id].AnimRun;
            f.AnimInactive = characterDB[id].AnimInactive;
            ResetTimerInactiveAnimation(f);

            if (!FF9BattleDB.GEO.TryGetValue(characterDB[id].ModelId, out String modelName))
            {
                Log.Warning($"[Trance Seek] ERROR : can't load follower with ModelId : {characterDB[id].ModelId}...");
                return null;
            }

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
                        material.shader = ShadersLoader.Find("WorldMap/Actor");
                    else
                    {
                        material.shader = ShadersLoader.Find(Configuration.Shaders.FieldCharacterShader);
                        material.SetColor(ColorPropertyId, new Color32(128, 128, 128, 255));
                    }
                    if (material.HasProperty(ColorPropertyId))
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
            f.RootBone = f.Go.transform.FindChild("bone000");
            AnimationFactory.AddAnimWithAnimatioName(f.Go, f.AnimIdle);
            AnimationFactory.AddAnimWithAnimatioName(f.Go, f.AnimWalk);
            AnimationFactory.AddAnimWithAnimatioName(f.Go, f.AnimRun);
            AnimationFactory.AddAnimWithAnimatioName(f.Go, f.AnimInactive);

            CreateFollowerShadow(f);

            followerPool[id] = f;
            return f;
        }

        private void UpdateModelsOnFieldCache()
        {
            modelsOnFieldCache.Clear();
            for (ObjList objList = ff9.GetActiveObjList(); objList != null; objList = objList.next)
            {
                if (objList.obj != null && objList.obj.cid == 4)
                {
                    Actor actor = (Actor)objList.obj;
                    if (actor.go != null && actor.go != leader && (actor.flags & 1) != 0)
                        modelsOnFieldCache.Add(actor.model);
                }
            }
        }

        private bool IsCharacterModelPresentOnField(CharacterId id)
        {
            if (!characterDB.ContainsKey(id)) return false;

            HashSet<int> blackList = characterDB[id].BlackListModelId;
            foreach (int modelId in blackList)
            {
                if (modelsOnFieldCache.Contains(modelId))
                    return true;
            }
            return false;
        }

        private void ProcessFollowers()
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

            Vector3 currentLeaderPos;
            if (IsWorldMap)
            {
                leader_model_id = ff9.GetControlChar().originalActor.model;
                currentLeaderPos = ff9.GetControlChar().pos;
            }
            else
            {
                currentLeaderPos = leader.transform.localPosition;
            }

            float sqrDistanceMoved = (currentLeaderPos - lastLeaderLocalPos).sqrMagnitude;
            LeaderState leaderstate = new LeaderState();

            if (IsWorldMap && sqrDistanceMoved > 1024f)
            {
                Vector3 shiftDelta = currentLeaderPos - lastLeaderLocalPos;
                foreach (Follower f in activeFollowers)
                {
                    if (f.Go != null)
                        f.Go.transform.localPosition += shiftDelta;

                    int stateCount = f.PositionHistory.Count;
                    for (int i = 0; i < stateCount; i++)
                    {
                        LeaderState updatedState = f.PositionHistory.Dequeue();
                        updatedState.LocalPosition += shiftDelta;
                        f.PositionHistory.Enqueue(updatedState);
                    }
                }
                lastLeaderLocalPos = currentLeaderPos;
                sqrDistanceMoved = 0f;
            }
            else if (sqrDistanceMoved > 0.0001f)
            {
                leaderstate.LocalPosition = currentLeaderPos;
                leaderstate.LocalRotation = leader.transform.localRotation;
                leaderstate.IsMoving = true;
                leaderstate.IsRunning = !IsWorldMap && ActorAnimWalking.Contains(actorleader.anim);
                leaderstate.LightColor = GetLeaderColor();

                foreach (Follower f in activeFollowers)
                    f.PositionHistory.Enqueue(leaderstate);
            }

            foreach (Follower f in activeFollowers)
            {
                if (f.Go == null) continue;

                if (!IsWorldMap && (IsCharacterModelPresentOnField(f.Id) || !FF9StateSystem.Common.FF9.party.IsInParty(f.Id)))
                {
                    if (f.Go.activeSelf)
                        f.Go.SetActive(false);

                    if (f.ShadowObj != null)
                        f.ShadowObj.SetActive(false);

                    continue;
                }
                else
                {
                    if (!f.Go.activeSelf)
                        f.Go.SetActive(true);

                    if (f.ShadowObj != null && !f.ShadowObj.activeSelf)
                        f.ShadowObj.SetActive(true);
                }

                if (f.ShadowObj != null)
                {
                    if (IsWorldMap && leader != null)
                    {
                        PosObj leaderPosObj = ff9.GetControlChar_PosObj();
                        WMShadow leaderShadow = leaderPosObj != null ? Singleton<WMWorld>.Instance.GetShadow(leaderPosObj) : null;

                        if (leaderShadow != null && leaderShadow.gameObject.activeSelf)
                        {
                            f.ShadowRenderer.enabled = f.Go.activeSelf;
                            f.ShadowTransform.localScale = leaderShadow.transform.localScale;

                            if (leaderShadow.Material != null && f.ShadowRenderer.material != null)
                                f.ShadowRenderer.material.SetFloat("_Amp", leaderShadow.Material.GetFloat("_Amp"));

                            f.ShadowTransform.position = new Vector3(f.Go.transform.position.x, f.Go.transform.position.y + 0.1f, f.Go.transform.position.z);
                        }
                        else
                        {
                            f.ShadowRenderer.enabled = false;
                        }
                    }
                    else if (!IsWorldMap && actorleader != null)
                    {
                        int leaderUid = actorleader.uid;
                        if (FF9StateSystem.Field.FF9Field.loc.map.shadowArray.TryGetValue(leaderUid, out FF9Shadow leaderShadow))
                        {
                            f.ShadowRenderer.enabled = f.Go.activeSelf;
                            f.ShadowTransform.localScale = new Vector3(leaderShadow.xScale, 1f, leaderShadow.zScale);

                            Byte amp = (Byte)(leaderShadow.amp * 2);
                            f.ShadowRenderer.material.SetColor(ColorPropertyId, new Color32(amp, amp, amp, 255));
                            Vector3 basePos = f.RootBone != null ? f.RootBone.position : f.Go.transform.localPosition;
                            Vector3 shadowPos = new Vector3(basePos.x, f.Go.transform.localPosition.y, basePos.z);
                            f.ShadowTransform.localPosition = shadowPos + new Vector3(leaderShadow.xOffset, 5f, leaderShadow.zOffset);
                        }
                        else
                        {
                            f.ShadowRenderer.enabled = false;
                        }
                    }
                }

                if (f.PositionHistory.Count > f.FramesBehind)
                {
                    while (f.PositionHistory.Count > f.FramesBehind)
                        leaderstate = f.PositionHistory.Dequeue();

                    f.Go.transform.localPosition = leaderstate.LocalPosition;
                    f.Go.transform.localRotation = leaderstate.LocalRotation;
                    ApplyFollowerColor(f, leaderstate.LightColor);

                    if (leaderstate.IsMoving && ActorAnimIdle.Contains(GetLeaderAnimID()))
                        PlayAnimation(f, f.AnimIdle);
                    else if (IsWorldMap || leaderstate.IsRunning)
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

            lastLeaderLocalPos = currentLeaderPos;
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

            bool changeDetected = expectedFollowers.Count != activeFollowers.Count;
            if (!changeDetected)
            {
                for (int i = 0; i < activeFollowers.Count; i++)
                {
                    if (activeFollowers[i].Id != expectedFollowers[i])
                    {
                        changeDetected = true;
                        break;
                    }
                }
            }

            if (!changeDetected) return;

            foreach (Follower f in activeFollowers)
            {
                if (f.Go != null)
                    f.Go.SetActive(false);
                if (f.ShadowObj != null)
                    f.ShadowObj.SetActive(false);
            }

            activeFollowers.Clear();
            int targetLayer = leaderRenderer != null ? leaderRenderer.gameObject.layer : leader.layer;
            int delay = 25;

            foreach (CharacterId id in expectedFollowers)
            {
                Follower f = GetOrCreateFollower(id, targetLayer);
                if (f == null) continue;

                f.FramesBehind = delay;

                if (f.Go != null)
                {
                    if (IsWorldMap)
                        f.Go.transform.localPosition = ff9.GetControlChar().pos;
                    else
                        f.Go.transform.localPosition = leader.transform.localPosition;

                    f.Go.transform.localRotation = leader.transform.localRotation;
                    f.PositionHistory.Clear();
                    f.Go.SetActive(true);
                }

                activeFollowers.Add(f);
                delay += 25;
            }
        }

        private void CreateFollowerShadow(Follower f)
        {
            if (IsWorldMap)
            {
                GameObject original = Resources.Load<GameObject>("EmbeddedAsset/WorldMap_Local/Shadow/WMShadow");
                if (original != null)
                {
                    f.ShadowObj = UnityEngine.Object.Instantiate<GameObject>(original);
                    f.ShadowObj.name = f.Go.name + "_WMShadow";
                    if (Singleton<WMWorld>.Instance != null && Singleton<WMWorld>.Instance.WorldMapEffectRoot != null)
                        f.ShadowObj.transform.parent = Singleton<WMWorld>.Instance.WorldMapEffectRoot;
                    else
                        f.ShadowObj.transform.parent = f.Go.transform.parent;

                    f.ShadowTransform = f.ShadowObj.transform;
                    f.ShadowRenderer = f.ShadowObj.GetComponent<Renderer>() as MeshRenderer;
                }
            }
            else
            {
                List<Vector3> vertices = new List<Vector3>
                {
                    new Vector3(-1f, 0f, -1f), new Vector3(1f, 0f, -1f),
                    new Vector3(1f, 0f, 1f), new Vector3(-1f, 0f, 1f)
                };
                Color color = new Color(1f, 1f, 1f, 0.6f);
                List<Color> colors = new List<Color> { color, color, color, color };
                List<Vector2> uvs = new List<Vector2>
                {
                    new Vector2(0f, 0f), new Vector2(1f, 0f),
                    new Vector2(1f, 1f), new Vector2(0f, 1f)
                };
                List<int> triangles = new List<int> { 2, 1, 0, 3, 2, 0 };

                Mesh mesh = new Mesh { vertices = vertices.ToArray(), colors = colors.ToArray(), uv = uvs.ToArray(), triangles = triangles.ToArray() };
                f.ShadowObj = new GameObject(f.Go.name + "_FieldShadow");

                if (PersistenSingleton<EventEngine>.Instance != null && PersistenSingleton<EventEngine>.Instance.fieldmap != null)
                    f.ShadowObj.transform.parent = PersistenSingleton<EventEngine>.Instance.fieldmap.transform;
                else
                    f.ShadowObj.transform.parent = f.Go.transform.parent;

                f.ShadowRenderer = f.ShadowObj.AddComponent<MeshRenderer>();
                MeshFilter meshFilter = f.ShadowObj.AddComponent<MeshFilter>();
                meshFilter.mesh = mesh;

                Material material = new Material(ShadersLoader.Find("PSX/FieldMapActorShadow"));
                material.mainTexture = AssetManager.Load<Texture2D>("CommonAsset/Common/shadow_plate", false);
                f.ShadowRenderer.material = material;
                f.ShadowRenderer.material.color = Color.black;
                f.ShadowTransform = f.ShadowObj.transform;
                meshFilter.sharedMesh.bounds = new Bounds(Vector3.zero, Vector3.one * float.MaxValue * 0.01f);
            }
        }

        private void HideFollowers(Boolean hide)
        {
            if (activeFollowers.Count == 0 || FollowersHidden == hide)
                return;

            foreach (Follower f in activeFollowers)
            {
                if (f.Go != null)
                    f.Go.SetActive(!hide);
                if (f.ShadowObj != null)
                    f.ShadowObj.SetActive(!hide);
            }

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
            if (leaderRenderer.sharedMaterial.HasProperty(ColorPropertyId))
                return leaderRenderer.sharedMaterial.GetColor(ColorPropertyId);
            return Color.white;
        }

        private void ApplyFollowerColor(Follower f, Color color)
        {
            if (f.LastAppliedColor != color)
            {
                for (int i = 0; i < f.CachedMaterials.Count; i++)
                    f.CachedMaterials[i].SetColor(ColorPropertyId, color);

                f.LastAppliedColor = color;
            }
        }

        private void ResetTimerInactiveAnimation(Follower f)
        {
            f.IdleTimer = UnityEngine.Random.Range(2000, 8000);
        }

        private void ClearFollowers()
        {
            if (activeFollowers.Count == 0) return;

            foreach (Follower f in followerPool.Values)
            {
                if (f.Go != null)
                {
                    f.Go.SetActive(false);
                    Destroy(f.Go);
                }
                if (f.ShadowObj != null)
                {
                    if (f.ShadowRenderer != null && f.ShadowRenderer.material != null)
                        Destroy(f.ShadowRenderer.material);

                    f.ShadowObj.SetActive(false);
                    Destroy(f.ShadowObj);
                }
            }

            leader = null;
            actorleader = null;
            leaderRenderer = null;
            cachedLeaderRenderers = null;
            followerPool.Clear();
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
