using Assets.Scripts.Common;
using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Memoria.Scripts.TranceSeek
{
    public static class TranceSeekDebug
    {
        public class DifficultyDebugMenu : MonoBehaviour
        {
            private bool _showMenu = false;
            private Rect _windowRect = new Rect(50, 50, 250, 550);
            public static bool _isDebugMenuCalled = false;
            public static int MegaCheat = 0;

            private bool _showDifficultyMenu = false;
            private Rect _difficultyWindowRect = new Rect(310, 50, 350, 450);

            private bool _showBattleMenu = false;
            private Rect _battleWindowRect = new Rect(310, 50, 520, 680);

            private bool _showEventMenu = false;
            private Rect _eventWindowRect = new Rect(310, 50, 600, 500);
            private int _eventMenuTab = 0;
            private int _selectedGlobalKey = 0;
            private Vector2 _eventScrollPos = Vector2.zero;
            private int _selectedOuterKey = -1;

            private bool _showAbilitiesMenu = false;
            private Rect _abilitiesWindowRect = new Rect(310, 50, 480, 390);

            private bool _showItemsMenu = false;
            private Rect _itemsWindowRect = new Rect(310, 50, 480, 250);

            private int _itemsMenuTab = 0;
            private int _currentImportantItemIndex = 0;
            private string _searchImportantItemIdStr = "";

            private bool _showLearningMenu = false;
            private Rect _learningWindowRect = new Rect(310, 50, 520, 800);
            private int _learningMenuTab = 0;
            private int _currentLearningCharIndex = 0;
            private Vector2 _learningScrollPos = Vector2.zero;

            private int _battleMenuTab = 0;
            private int _currentUnitIndex = 0;
            private int _currentAttackIndex = 0;
            private string _searchAttackIdStr = "";
            private int _selectedMonsterTypeIndex = -1;

            private int _forceTargetId = 15;
            private bool _showForceTargetDropdown = false;
            private Vector2 _forceTargetScrollPos = Vector2.zero;

            private btlseq.btlseqinstance _cachedSeqReader;
            private BTL_SCENE _cachedScene;
            private string _cachedBtlName;

            private int _currentAbilityIndex = 0;
            private string _searchAbilityIdStr = "";

            private int _currentItemIndex = 0;
            private int _itemQuantityToAdd = 1;
            private string _searchItemIdStr = "";

            private bool _showFieldMenu = false;
            private Rect _fieldWindowRect = new Rect(310, 50, 500, 550);
            private Vector2 _fieldScrollPos = Vector2.zero;
            private int _currentActorIndex = 0;
            private FieldMapActor[] _cachedActors = null;
            private Dictionary<Actor, ushort> _originalIdles = new Dictionary<Actor, ushort>();
            private FieldMapActor _lastTintedFma = null;
            private bool _enableTint = true;
            private Dictionary<Renderer, Color> _originalColors = new Dictionary<Renderer, Color>();
            private string _posX = "0", _posY = "0", _posZ = "0";
            private string _rotY = "0";
            private string _customAnimIdInput = "0";
            private string _lastPlayedAnimInfo = "Aucune";

            private int _fieldMenuTab = 0;
            private bool _isCameraDragMode = false;
            private float _targetCamX = 0f;
            private float _targetCamY = 0f;

            private string _camInputX = "0";
            private string _camInputY = "0";
            private string _camInputDuration = "30";
            private string _camInputType = "8";

            private bool _showLangMenu = false;
            private Rect _langWindowRect = new Rect(310, 50, 300, 200);
            private bool _showLangDropdown = false;
            private Vector2 _langScrollPos = Vector2.zero;

            private bool _showTriggerBattleMenu = false;
            private Rect _triggerBattleWindowRect = new Rect(310, 50, 300, 200);
            private string _battleIdStr = "0";
            private string _battleGroupStr = "0";
            private bool _randomBattleGroup = false;

            private int _selectedMapVarKey = 0;
            private int _selectedLocalVarKey = 0;
            private int _currentObjIndex = 0;

            private Dictionary<string, string> _statTextCache = new Dictionary<string, string>();

            private int _statusMode = 0;
            private readonly string[] _statusModeNames = { "<color=#55FF55>Current Status</color>", "<color=#FFFF55>Permanent Status</color>", "<color=#FF5555>Resist Status</color>" };
            private Vector2 _statusScrollPos = Vector2.zero;

            private static readonly KeyValuePair<string, BattleStatus>[] _statusList = new KeyValuePair<string, BattleStatus>[]
            {
        new KeyValuePair<string, BattleStatus>("Petrify", BattleStatus.Petrify),
        new KeyValuePair<string, BattleStatus>("Venom", BattleStatus.Venom),
        new KeyValuePair<string, BattleStatus>("Virus", BattleStatus.Virus),
        new KeyValuePair<string, BattleStatus>("Silence", BattleStatus.Silence),
        new KeyValuePair<string, BattleStatus>("Blind", BattleStatus.Blind),
        new KeyValuePair<string, BattleStatus>("Trouble", BattleStatus.Trouble),
        new KeyValuePair<string, BattleStatus>("Zombie", BattleStatus.Zombie),
        new KeyValuePair<string, BattleStatus>("Death", BattleStatus.Death),
        new KeyValuePair<string, BattleStatus>("EasyKill", BattleStatus.EasyKill),
        new KeyValuePair<string, BattleStatus>("Confuse", BattleStatus.Confuse),
        new KeyValuePair<string, BattleStatus>("Berserk", BattleStatus.Berserk),
        new KeyValuePair<string, BattleStatus>("Stop", BattleStatus.Stop),
        new KeyValuePair<string, BattleStatus>("AutoLife", BattleStatus.AutoLife),
        new KeyValuePair<string, BattleStatus>("Trance", BattleStatus.Trance),
        new KeyValuePair<string, BattleStatus>("Defend", BattleStatus.Defend),
        new KeyValuePair<string, BattleStatus>("Poison", BattleStatus.Poison),
        new KeyValuePair<string, BattleStatus>("Sleep", BattleStatus.Sleep),
        new KeyValuePair<string, BattleStatus>("Regen", BattleStatus.Regen),
        new KeyValuePair<string, BattleStatus>("Haste", BattleStatus.Haste),
        new KeyValuePair<string, BattleStatus>("Slow", BattleStatus.Slow),
        new KeyValuePair<string, BattleStatus>("Float", BattleStatus.Float),
        new KeyValuePair<string, BattleStatus>("Shell", BattleStatus.Shell),
        new KeyValuePair<string, BattleStatus>("Protect", BattleStatus.Protect),
        new KeyValuePair<string, BattleStatus>("Heat", BattleStatus.Heat),
        new KeyValuePair<string, BattleStatus>("Freeze", BattleStatus.Freeze),
        new KeyValuePair<string, BattleStatus>("Vanish", BattleStatus.Vanish),
        new KeyValuePair<string, BattleStatus>("Doom", BattleStatus.Doom),
        new KeyValuePair<string, BattleStatus>("Mini", BattleStatus.Mini),
        new KeyValuePair<string, BattleStatus>("Reflect", BattleStatus.Reflect),
        new KeyValuePair<string, BattleStatus>("Jump", BattleStatus.Jump),
        new KeyValuePair<string, BattleStatus>("GradualPetrify", BattleStatus.GradualPetrify),
        new KeyValuePair<string, BattleStatus>("PowerBreak", BattleStatus.CustomStatus1),
        new KeyValuePair<string, BattleStatus>("MagicBreak", BattleStatus.CustomStatus2),
        new KeyValuePair<string, BattleStatus>("ArmorBreak", BattleStatus.CustomStatus3),
        new KeyValuePair<string, BattleStatus>("MentalBreak", BattleStatus.CustomStatus4),
        new KeyValuePair<string, BattleStatus>("PowerUp", BattleStatus.CustomStatus5),
        new KeyValuePair<string, BattleStatus>("MagicUp", BattleStatus.CustomStatus6),
        new KeyValuePair<string, BattleStatus>("ArmorUp", BattleStatus.CustomStatus7),
        new KeyValuePair<string, BattleStatus>("MentalUp", BattleStatus.CustomStatus8),
        new KeyValuePair<string, BattleStatus>("Dragon", BattleStatus.CustomStatus9),
        new KeyValuePair<string, BattleStatus>("ZombieArmor", BattleStatus.CustomStatus10),
        new KeyValuePair<string, BattleStatus>("MechanicalArmor", BattleStatus.CustomStatus11),
        new KeyValuePair<string, BattleStatus>("Redemption", BattleStatus.CustomStatus12),
        new KeyValuePair<string, BattleStatus>("Bulwark", BattleStatus.CustomStatus13),
        new KeyValuePair<string, BattleStatus>("PerfectDodge", BattleStatus.CustomStatus14),
        new KeyValuePair<string, BattleStatus>("PerfectCrit", BattleStatus.CustomStatus15),
        new KeyValuePair<string, BattleStatus>("Vieillissement", BattleStatus.CustomStatus16),
        new KeyValuePair<string, BattleStatus>("SleepEasyKill", BattleStatus.CustomStatus17),
        new KeyValuePair<string, BattleStatus>("SilenceEasyKill", BattleStatus.CustomStatus18),
        new KeyValuePair<string, BattleStatus>("Rage", BattleStatus.CustomStatus19),
        new KeyValuePair<string, BattleStatus>("Runic", BattleStatus.CustomStatus20),
        new KeyValuePair<string, BattleStatus>("Special", BattleStatus.CustomStatus21),
        new KeyValuePair<string, BattleStatus>("Provok", BattleStatus.CustomStatus22),
        new KeyValuePair<string, BattleStatus>("Charm", BattleStatus.CustomStatus23)
            };

            private int _elementMode = 0;
            private readonly string[] _elementModeNames = { "<color=#FF5555>Weak Element</color>", "<color=#55FF55>Half Element</color>", "<color=#5555FF>Guard Element</color>", "<color=#55FFFF>Absorb Element</color>", "<color=#FF55FF>Bonus Element</color>" };
            private Vector2 _elementScrollPos = Vector2.zero;

            private static readonly KeyValuePair<string, EffectElement>[] _elementList = new KeyValuePair<string, EffectElement>[]
            {
        new KeyValuePair<string, EffectElement>("Fire", EffectElement.Fire),
        new KeyValuePair<string, EffectElement>("Cold", EffectElement.Cold),
        new KeyValuePair<string, EffectElement>("Thunder", EffectElement.Thunder),
        new KeyValuePair<string, EffectElement>("Earth", EffectElement.Earth),
        new KeyValuePair<string, EffectElement>("Aqua", EffectElement.Aqua),
        new KeyValuePair<string, EffectElement>("Wind", EffectElement.Wind),
        new KeyValuePair<string, EffectElement>("Holy", EffectElement.Holy),
        new KeyValuePair<string, EffectElement>("Darkness", EffectElement.Darkness)
            };

            void Update()
            {
                bool plusPressed = UnityEngine.Input.GetKey(KeyCode.KeypadPlus) || UnityEngine.Input.GetKey(KeyCode.Plus);
                bool minusPressed = UnityEngine.Input.GetKey(KeyCode.KeypadMinus) || UnityEngine.Input.GetKey(KeyCode.Minus);

                if (plusPressed && minusPressed && UnityEngine.Input.GetKeyDown(KeyCode.F11))
                {
                    _showMenu = true;
                    _isDebugMenuCalled = true;
                }

                bool plusDown = UnityEngine.Input.GetKeyDown(KeyCode.KeypadPlus) || UnityEngine.Input.GetKeyDown(KeyCode.Plus);

                if (plusDown && _isDebugMenuCalled)
                {
                    if (!minusPressed)
                    {
                        _showMenu = !_showMenu;
                        SoundLib.PlaySoundEffect(1362);
                    }
                    if (!_showMenu)
                        CloseAllSubMenus();
                }

                if (_isCameraDragMode)
                {
                    var fieldmap = PersistenSingleton<EventEngine>.Instance?.fieldmap;
                    if (fieldmap != null)
                    {
                        float speed = 5.0f;
                        if (UnityEngine.Input.GetKey(KeyCode.LeftShift) || UnityEngine.Input.GetKey(KeyCode.RightShift))
                            speed = 15.0f;

                        if (UnityEngine.Input.GetKey(KeyCode.LeftArrow) || UnityEngine.Input.GetKey(KeyCode.A) || UnityEngine.Input.GetKey(KeyCode.Q))
                            _targetCamX -= speed;

                        if (UnityEngine.Input.GetKey(KeyCode.RightArrow) || UnityEngine.Input.GetKey(KeyCode.D))
                            _targetCamX += speed;

                        if (UnityEngine.Input.GetKey(KeyCode.UpArrow) || UnityEngine.Input.GetKey(KeyCode.W) || UnityEngine.Input.GetKey(KeyCode.Z))
                            _targetCamY -= speed;

                        if (UnityEngine.Input.GetKey(KeyCode.DownArrow) || UnityEngine.Input.GetKey(KeyCode.S))
                            _targetCamY += speed;

                        fieldmap.EBG_scene2DScroll((short)_targetCamX, (short)_targetCamY, 1, 0);
                        fieldmap.curVRP.x = _targetCamX;
                        fieldmap.curVRP.y = _targetCamY;
                    }
                }
            }

            private void CloseAllSubMenus()
            {
                _showDifficultyMenu = false;
                _showBattleMenu = false;
                _showEventMenu = false;
                _showAbilitiesMenu = false;
                _showItemsMenu = false;
                _showLearningMenu = false;
                _showLangMenu = false;
                _showTriggerBattleMenu = false;
                if (_showFieldMenu)
                {
                    RestoreAllFieldAnimations();
                    RestoreTint();
                    if (_isCameraDragMode)
                    {
                        _isCameraDragMode = false;
                        PersistenSingleton<EventEngine>.Instance?.fieldmap?.EBG_scene2DScrollRelease(30, 0);
                    }
                }
                _showFieldMenu = false;
            }

            void OnGUI()
            {
                if (_showMenu)
                {
                    GUI.backgroundColor = Color.black;
                    _windowRect = GUI.Window(1403, _windowRect, DrawMenu, "DEBUG MENU TRANCE SEEK");
                }

                if (_showMenu && _showDifficultyMenu)
                {
                    GUI.backgroundColor = Color.black;
                    _difficultyWindowRect = GUI.Window(1412, _difficultyWindowRect, DrawDifficultyMenu, "Mod : Difficultés Trance Seek");
                }

                if (_showMenu && _showBattleMenu && SceneDirector.IsBattleScene())
                {
                    GUI.backgroundColor = Color.black;
                    _battleWindowRect = GUI.Window(1404, _battleWindowRect, DrawBattleMenu, "Mod : Debug Battle Stats");
                }

                if (_showMenu && _showEventMenu)
                {
                    GUI.backgroundColor = Color.black;
                    _eventWindowRect = GUI.Window(1405, _eventWindowRect, DrawEventMenu, "Mod : Event Variables");
                }

                if (_showMenu && _showAbilitiesMenu)
                {
                    GUI.backgroundColor = Color.black;
                    _abilitiesWindowRect = GUI.Window(1406, _abilitiesWindowRect, DrawAbilitiesMenu, "Mod : AA_Data Debug");
                }

                if (_showMenu && _showItemsMenu)
                {
                    GUI.backgroundColor = Color.black;
                    _itemsWindowRect = GUI.Window(1407, _itemsWindowRect, DrawItemsMenu, "Mod : Items Debug");
                }

                if (_showMenu && _showLearningMenu)
                {
                    GUI.backgroundColor = Color.black;
                    _learningWindowRect = GUI.Window(1408, _learningWindowRect, DrawLearningMenu, "Mod : Player Debug");
                }

                if (_showMenu && _showLangMenu)
                {
                    GUI.backgroundColor = Color.black;
                    _langWindowRect = GUI.Window(1409, _langWindowRect, DrawLangMenu, "Mod : Language Debug");
                }

                if (_showMenu && _showTriggerBattleMenu && SceneDirector.IsFieldScene())
                {
                    GUI.backgroundColor = Color.black;
                    _triggerBattleWindowRect = GUI.Window(1415, _triggerBattleWindowRect, DrawTriggerBattleMenu, "Mod : Trigger Battle");
                }

                if (_showMenu && _showFieldMenu && SceneDirector.IsFieldScene())
                {
                    GUI.backgroundColor = Color.black;
                    _fieldWindowRect = GUI.Window(1410, _fieldWindowRect, DrawFieldMenu, "Mod : Debug Field Animations");
                    if (_cachedActors != null && _cachedActors.Length > 0)
                    {
                        if (_currentActorIndex < _cachedActors.Length)
                        {
                            FieldMapActor selectedFma = _cachedActors[_currentActorIndex];
                            if (selectedFma != null && selectedFma.actor != null && selectedFma.actor.go != null)
                            {
                                Camera cam = Camera.main;
                                if (cam != null)
                                {
                                    Vector3 pos3D = selectedFma.actor.go.transform.position;
                                    Vector3 screenPos = cam.WorldToScreenPoint(pos3D);

                                    if (screenPos.z > 0)
                                    {
                                        Rect pointerRect = new Rect(screenPos.x - 20, Screen.height - screenPos.y - 150, 40, 40);
                                        GUI.Label(pointerRect, "<color=red><b>▼</b></color>", new GUIStyle(GUI.skin.label) { fontSize = 36, alignment = TextAnchor.MiddleCenter, richText = true });
                                    }
                                }
                            }
                        }
                    }
                }
            }

            void DrawMenu(int windowID)
            {
                GUILayout.Space(10);
                GUI.skin.button.richText = true;

                if (GUILayout.Button(_showDifficultyMenu ? "<color=orange><b>Fermer Difficultés Trance Seek</b></color>" : "<b>Ouvrir Difficultés Trance Seek</b>"))
                {
                    bool t = !_showDifficultyMenu;
                    _showDifficultyMenu = t;
                    SoundLib.PlaySoundEffect(1362);
                }

                GUILayout.Space(10);

                if (GUILayout.Button(FormatCheatText("Disable MegaCheat", 0))) { MegaCheat = 0; SoundLib.PlaySoundEffect(4505); }
                if (GUILayout.Button(FormatCheatText("Activate MegaCheat", 1))) { MegaCheat = 1; SoundLib.PlaySoundEffect(4504); }
                if (GUILayout.Button(FormatCheatText("Activate MegaCheatFULL", 2))) { MegaCheat = 2; SoundLib.PlaySoundEffect(4502); }

                GUILayout.Space(10);
                if (GUILayout.Button("<b>Reload AbilityFeatures</b>")) { ReloadAbilityFeatures(); GUI.FocusControl(null); }
                if (GUILayout.Button("<b>Reload All Texts</b>")) { ReloadAllTexts(); GUI.FocusControl(null); }
                GUILayout.Space(15);

                if (SceneDirector.IsBattleScene()) { if (GUILayout.Button(_showBattleMenu ? "<color=orange><b>Fermer Debug Battle</b></color>" : "<b>Ouvrir Debug Battle</b>")) { bool t = !_showBattleMenu; CloseAllSubMenus(); _showBattleMenu = t; SoundLib.PlaySoundEffect(1362); } }
                else _showBattleMenu = false;

                if (GUILayout.Button(_showEventMenu ? "<color=orange><b>Fermer Event Debug</b></color>" : "<b>Ouvrir Event Debug</b>")) { bool t = !_showEventMenu; CloseAllSubMenus(); _showEventMenu = t; SoundLib.PlaySoundEffect(1362); }
                if (GUILayout.Button(_showAbilitiesMenu ? "<color=orange><b>Fermer AA_Data Debug</b></color>" : "<b>Ouvrir AA_Data Debug</b>")) { bool t = !_showAbilitiesMenu; CloseAllSubMenus(); _showAbilitiesMenu = t; SoundLib.PlaySoundEffect(1362); }
                if (GUILayout.Button(_showLearningMenu ? "<color=orange><b>Fermer Player Debug</b></color>" : "<b>Ouvrir Player Debug</b>")) { bool t = !_showLearningMenu; CloseAllSubMenus(); _showLearningMenu = t; SoundLib.PlaySoundEffect(1362); }
                if (GUILayout.Button(_showItemsMenu ? "<color=orange><b>Fermer Items Debug</b></color>" : "<b>Ouvrir Items Debug</b>")) { bool t = !_showItemsMenu; CloseAllSubMenus(); _showItemsMenu = t; SoundLib.PlaySoundEffect(1362); }
                if (GUILayout.Button(_showLangMenu ? "<color=orange><b>Fermer Language Debug</b></color>" : "<b>Ouvrir Language Debug</b>")) { bool t = !_showLangMenu; CloseAllSubMenus(); _showLangMenu = t; SoundLib.PlaySoundEffect(1362); }
                if (SceneDirector.IsFieldScene())
                {
                    if (GUILayout.Button(_showFieldMenu ? "<color=orange><b>Fermer Debug Field</b></color>" : "<b>Ouvrir Debug Field</b>"))
                    {
                        bool t = !_showFieldMenu;
                        CloseAllSubMenus();
                        _showFieldMenu = t;
                        SoundLib.PlaySoundEffect(1362);
                    }

                    if (GUILayout.Button(_showTriggerBattleMenu ? "<color=orange><b>Fermer Trigger Battle</b></color>" : "<b>Ouvrir Trigger Battle</b>"))
                    {
                        bool t = !_showTriggerBattleMenu;
                        CloseAllSubMenus();
                        _showTriggerBattleMenu = t;
                        SoundLib.PlaySoundEffect(1362);
                    }
                }
                else
                {
                    _showFieldMenu = false;
                    _showTriggerBattleMenu = false;
                }

                GUILayout.Space(15);
                if (GUILayout.Button("Fermer le menu")) { _showMenu = false; SoundLib.PlaySoundEffect(1363); }
                GUI.DragWindow();
            }

            void DrawDifficultyMenu(int windowID)
            {
                GUILayout.BeginVertical("box");
                GUILayout.Label("<b>Système de Difficulté</b>", new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter });
                GUILayout.Space(10);

                if (GUILayout.Button(FormatDifficultyText("Zidane", 0))) SetDifficulty(0, 84);
                if (GUILayout.Button(FormatDifficultyText("Vivi", 1))) SetDifficulty(1, 82);
                if (GUILayout.Button(FormatDifficultyText("Eiko", 2))) SetDifficulty(2, 83);
                if (GUILayout.Button(FormatDifficultyText("Kuja", 3))) SetDifficulty(3, 85);
                if (GUILayout.Button(FormatDifficultyText("Necron", 4))) SetDifficulty(4, 86);
                if (GUILayout.Button(FormatDifficultyText("Beatrix", 5))) SetDifficulty(5, 87);
                if (GUILayout.Button(FormatDifficultyText("Ozma", 6))) SetDifficulty(6, 88);
                if (GUILayout.Button(FormatDifficultyText("Garland", 7))) SetDifficulty(7, 89);

                GUILayout.EndVertical();
                GUI.DragWindow();
            }

            void DrawLearningMenu(int windowID)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(_learningMenuTab == 0 ? "<color=orange><b>AA Learning</b></color>" : "AA Learning")) { _learningMenuTab = 0; _statTextCache.Clear(); GUI.FocusControl(null); }
                if (GUILayout.Button(_learningMenuTab == 1 ? "<color=orange><b>SA Learning</b></color>" : "SA Learning")) { _learningMenuTab = 1; _statTextCache.Clear(); GUI.FocusControl(null); }
                GUILayout.EndHorizontal();

                GUILayout.Space(10);
                var playersDict = FF9StateSystem.Common.FF9.player;
                if (playersDict == null) { GUILayout.Label("Aucun personnage."); GUI.DragWindow(); return; }
                var charList = playersDict.Keys.ToList();
                if (_currentLearningCharIndex >= charList.Count) _currentLearningCharIndex = 0;

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("<", GUILayout.Width(40), GUILayout.Height(30))) { _currentLearningCharIndex--; if (_currentLearningCharIndex < 0) _currentLearningCharIndex = charList.Count - 1; _statTextCache.Clear(); GUI.FocusControl(null); }
                CharacterId charId = charList[_currentLearningCharIndex];
                GUILayout.Label($"<b>👤 <color=#00FFFF>{charId}</color> 👤</b>\n{_currentLearningCharIndex + 1}/{charList.Count}", new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter });
                if (GUILayout.Button(">", GUILayout.Width(40), GUILayout.Height(30))) { _currentLearningCharIndex++; if (_currentLearningCharIndex >= charList.Count) _currentLearningCharIndex = 0; _statTextCache.Clear(); GUI.FocusControl(null); }
                GUILayout.EndHorizontal();

                GUILayout.Space(10);
                PLAYER p = playersDict[charId];
                if (!ff9abil._FF9Abil_PaData.ContainsKey(p.PresetId)) { GUILayout.Label("Données introuvables."); GUI.DragWindow(); return; }
                CharacterAbility[] charAbil = ff9abil._FF9Abil_PaData[p.PresetId];

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("<b>Tout Apprendre</b>\n<size=10>(AA & SA)</size>", new GUIStyle(GUI.skin.button) { richText = true }, GUILayout.Height(35)))
                {
                    for (int i = 0; i < charAbil.Length; i++)
                    {
                        if (charAbil[i].Id != 0 && i < p.pa.Length)
                            p.pa[i] = (byte)charAbil[i].Ap;
                    }
                    _statTextCache.Clear(); GUI.FocusControl(null); SoundLib.PlaySoundEffect(108);
                }
                if (GUILayout.Button("<b>Tout Oublier</b>\n<size=10>(AA & SA)</size>", new GUIStyle(GUI.skin.button) { richText = true }, GUILayout.Height(35)))
                {
                    for (int i = 0; i < charAbil.Length; i++)
                    {
                        if (charAbil[i].Id != 0 && i < p.pa.Length)
                            p.pa[i] = 0;
                    }
                    _statTextCache.Clear(); GUI.FocusControl(null); SoundLib.PlaySoundEffect(108);
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);

                _learningScrollPos = GUILayout.BeginScrollView(_learningScrollPos, GUILayout.Height(600));
                for (int i = 0; i < charAbil.Length; i++)
                {
                    if (charAbil[i].Id != 0 && i < p.pa.Length)
                    {
                        bool isPassive = charAbil[i].IsPassive;
                        if ((_learningMenuTab == 0 && !isPassive) || (_learningMenuTab == 1 && isPassive))
                        {
                            string abilName = ""; string icon = isPassive ? "🛡️" : "🔮"; int displayId = 0; string colorHex = "FFFFFF";
                            if (isPassive)
                            {
                                SupportAbility saId = charAbil[i].PassiveId; displayId = (int)saId;
                                abilName = SpecialFilesTranceSeek.RemoveTags(FF9TextTool.SupportAbilityName(saId));
                                if (displayId >= 1000 && displayId <= 1999) colorHex = "FF00FF"; else if (displayId >= 2000) colorHex = "FDFE0F";
                            }
                            else
                            {
                                BattleAbilityId aaId = charAbil[i].ActiveId; displayId = (int)aaId;
                                abilName = GetAbilityName(displayId);
                                if (abilName == "Unknown") abilName = SpecialFilesTranceSeek.RemoveTags(FF9TextTool.ActionAbilityName(aaId));
                            }

                            int maxAp = charAbil[i].Ap; int currentAp = p.pa[i];
                            GUILayout.BeginHorizontal("box");
                            GUILayout.Label($"<b>{icon} <color=#{colorHex}>{abilName}</color></b>\nID: {displayId}", new GUIStyle(GUI.skin.label) { richText = true }, GUILayout.Width(170));
                            int newAp = Mathf.Clamp(DrawStatUI($"Learn_{charId}_{i}_AP", $"AP (/{maxAp})", currentAp, 90), 0, maxAp);
                            if (newAp != currentAp) p.pa[i] = (byte)newAp;
                            if (GUILayout.Button("0 AP")) { p.pa[i] = 0; _statTextCache.Clear(); GUI.FocusControl(null); SoundLib.PlaySoundEffect(108); }
                            if (GUILayout.Button("Master")) { p.pa[i] = (byte)maxAp; _statTextCache.Clear(); GUI.FocusControl(null); SoundLib.PlaySoundEffect(108); }
                            GUILayout.EndHorizontal();
                        }
                    }
                }
                GUILayout.EndScrollView();
                GUI.DragWindow();
            }

            void DrawBattleMenu(int windowID)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(_battleMenuTab == 0 ? "<color=orange><b>Units</b></color>" : "Units")) { _battleMenuTab = 0; _statTextCache.Clear(); GUI.FocusControl(null); }
                if (GUILayout.Button(_battleMenuTab == 1 ? "<color=orange><b>Enemy Attacks</b></color>" : "Enemy Attacks")) { _battleMenuTab = 1; _statTextCache.Clear(); GUI.FocusControl(null); }
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
                if (_battleMenuTab == 0) DrawUnitsTab();
                else if (_battleMenuTab == 1) DrawEnemyAttacksTab();
                GUI.DragWindow();
            }

            private void DrawEnemyAttacksTab()
            {
                var battle = FF9StateSystem.Battle.FF9Battle;
                if (battle == null) return;

                string btlName = battle.btl_scene.nameIdentifier.Replace("BSC_", "");
                if (_cachedSeqReader == null || _cachedBtlName != btlName)
                {
                    _cachedBtlName = btlName;
                    _cachedScene = new BTL_SCENE();
                    _cachedScene.ReadBattleScene(btlName);
                    _cachedSeqReader = new btlseq.btlseqinstance();
                    btlseq.ReadBattleSequence(btlName, ref _cachedSeqReader);
                    _cachedSeqReader.FixBuggedAnimations(_cachedScene);
                }

                var activeUnits = BattleState.EnumerateUnits().Where(u => !u.IsPlayer).ToList();
                if (activeUnits.Count == 0) { GUILayout.Label("Aucun monstre actif."); return; }

                List<int> monsterAttacks = new List<int>();
                for (int i = 0; i < _cachedScene.header.AtkCount; i++)
                {
                    monsterAttacks.Add(i);
                }

                if (monsterAttacks.Count == 0) { GUILayout.Label($"Aucune attaque trouvée."); return; }

                GUILayout.BeginHorizontal();
                GUILayout.Label("Aller à l'ID :", GUILayout.Width(75));
                _searchAttackIdStr = GUILayout.TextField(_searchAttackIdStr, GUILayout.Width(50));

                if (GUILayout.Button("Go", GUILayout.Width(40)))
                {
                    if (int.TryParse(_searchAttackIdStr, out int parsedId))
                    {
                        int index = monsterAttacks.IndexOf(parsedId);
                        if (index != -1)
                        {
                            _currentAttackIndex = index;
                            _statTextCache.Clear();
                            GUI.FocusControl(null);
                            SoundLib.PlaySoundEffect(103);
                        }
                        else
                        {
                            SoundLib.PlaySoundEffect(102);
                        }
                    }
                    else
                    {
                        SoundLib.PlaySoundEffect(102);
                    }
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(5);

                if (_currentAttackIndex >= monsterAttacks.Count) _currentAttackIndex = 0;

                int curSeqId = monsterAttacks[_currentAttackIndex];
                AA_DATA atk = (battle.enemy_attack != null && curSeqId < battle.enemy_attack.Count) ? battle.enemy_attack[curSeqId] : null;
                if (atk == null) return;

                int curSeqEnemyType = _cachedSeqReader.GetEnemyIndexOfSequence(curSeqId);
                string atkOwnerName = $"Type {curSeqEnemyType}";
                var sampleUnit = activeUnits.FirstOrDefault(u => battle.btl_scene.PatAddr[battle.btl_scene.PatNum].Monster[u.Data.bi.slot_no].TypeNo == curSeqEnemyType);

                if (sampleUnit != null && battle.enemy[sampleUnit.Data.bi.slot_no] != null)
                {
                    atkOwnerName = SpecialFilesTranceSeek.RemoveTags(battle.enemy[sampleUnit.Data.bi.slot_no].et.name);
                }

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("<", GUILayout.Width(40), GUILayout.Height(30))) { _currentAttackIndex--; if (_currentAttackIndex < 0) _currentAttackIndex = monsterAttacks.Count - 1; _statTextCache.Clear(); GUI.FocusControl(null); }

                GUILayout.Label($"<b>⚔️ <color=#FFAA00>{SpecialFilesTranceSeek.RemoveTags(atk.Name)}</color> ⚔️</b>\n<color=#AADDFF>{atkOwnerName}</color> - Seq ID: {curSeqId} ({_currentAttackIndex + 1}/{monsterAttacks.Count})", new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter });

                if (GUILayout.Button(">", GUILayout.Width(40), GUILayout.Height(30))) { _currentAttackIndex++; if (_currentAttackIndex >= monsterAttacks.Count) _currentAttackIndex = 0; _statTextCache.Clear(); GUI.FocusControl(null); }
                GUILayout.EndHorizontal();

                GUILayout.BeginVertical("box"); GUILayout.BeginHorizontal();
                GUILayout.BeginVertical(GUILayout.Width(220));
                atk.Ref.Power = Mathf.Clamp(DrawStatUI($"Atk_{curSeqId}_Power", "Power", atk.Ref.Power), 0, 9999);
                atk.Ref.Rate = Mathf.Clamp(DrawStatUI($"Atk_{curSeqId}_Rate", "Hit Rate", atk.Ref.Rate), 0, 9999);
                atk.MP = Mathf.Clamp(DrawStatUI($"Atk_{curSeqId}_MP", "MP Cost", atk.MP), 0, 9999);
                atk.Ref.Elements = (byte)Mathf.Clamp(DrawStatUI($"Atk_{curSeqId}_Elem", "Elements", atk.Ref.Elements), 0, 255);
                atk.AddStatusNo = (StatusSetId)Mathf.Clamp(DrawStatUI($"Atk_{curSeqId}_Status", "Status Set", (int)atk.AddStatusNo), 0, 255);
                atk.Category = (byte)Mathf.Clamp(DrawStatUI($"Atk_{curSeqId}_Cat", "Category", atk.Category), 0, 255);
                atk.Ref.ScriptId = Mathf.Clamp(DrawStatUI($"Atk_{curSeqId}_Script", "Script ID", atk.Ref.ScriptId), 0, 9999);
                GUILayout.EndVertical();
                GUILayout.BeginVertical(GUILayout.Width(220));
                if (atk.Info != null)
                {
                    atk.Info.VfxIndex = (short)Mathf.Clamp(DrawStatUI($"Atk_{curSeqId}_Vfx", "Vfx Index", atk.Info.VfxIndex), -1, 9999);
                    atk.Info.Target = (TargetType)Mathf.Clamp(DrawStatUI($"Atk_{curSeqId}_Target", "Target", (int)atk.Info.Target), 0, 255);
                    atk.Info.DisplayStats = (TargetDisplay)Mathf.Clamp(DrawStatUI($"Atk_{curSeqId}_Disp", "Display", (int)atk.Info.DisplayStats), 0, 255);
                    GUILayout.Space(5); atk.Info.DefaultAlly = GUILayout.Toggle(atk.Info.DefaultAlly, "Default Ally"); atk.Info.ForDead = GUILayout.Toggle(atk.Info.ForDead, "For Dead"); atk.Info.DefaultCamera = GUILayout.Toggle(atk.Info.DefaultCamera, "Default Camera"); atk.Info.DefaultOnDead = GUILayout.Toggle(atk.Info.DefaultOnDead, "Default On Dead");
                }
                GUILayout.EndVertical(); GUILayout.EndHorizontal();
                if (atk.Info != null)
                {
                    GUILayout.BeginHorizontal(); GUILayout.Label("SEQ File:", GUILayout.Width(75)); string ck = $"Atk_{curSeqId}_Seq";
                    if (!_statTextCache.ContainsKey(ck)) _statTextCache[ck] = atk.Info.SequenceFile ?? "";
                    _statTextCache[ck] = GUILayout.TextField(_statTextCache[ck], GUILayout.Width(250)); atk.Info.SequenceFile = _statTextCache[ck];
                    if (GUILayout.Button("Reload")) { GUI.FocusControl(null); if (!string.IsNullOrEmpty(atk.Info.SequenceFile)) { string s = AssetManager.LoadString(atk.Info.SequenceFile); if (s != null) { atk.Info.VfxAction = new UnifiedBattleSequencer.BattleAction(s); SoundLib.PlaySoundEffect(108); } } }
                    GUILayout.EndHorizontal();
                }

                GUILayout.Space(10);
                GUILayout.BeginVertical("box");
                GUILayout.Label("<b>Force Trigger</b>", new GUIStyle(GUI.skin.label) { richText = true });

                List<KeyValuePair<string, int>> targetOptions = new List<KeyValuePair<string, int>>
            {
                new KeyValuePair<string, int>("All Players", 15),
                new KeyValuePair<string, int>("All Enemies", 240),
                new KeyValuePair<string, int>("Everyone", 255)
            };

                var allUnits = BattleState.EnumerateUnits().ToList();
                foreach (var unit in allUnits)
                {
                    string uName = unit.IsPlayer ? FF9TextTool.CharacterDefaultName(unit.PlayerIndex) : SpecialFilesTranceSeek.RemoveTags(unit.Name);
                    targetOptions.Add(new KeyValuePair<string, int>($"{uName}", unit.Id));
                }

                string currentTargetName = "Select Target";
                var currentSelection = targetOptions.FirstOrDefault(t => t.Value == _forceTargetId);
                if (currentSelection.Key != null) currentTargetName = currentSelection.Key;
                else _forceTargetId = 15;

                GUILayout.BeginHorizontal();

                if (GUILayout.Button($"Target: <b>{currentTargetName}</b>", new GUIStyle(GUI.skin.button) { richText = true }, GUILayout.Width(180)))
                {
                    _showForceTargetDropdown = !_showForceTargetDropdown;
                    SoundLib.PlaySoundEffect(103);
                }

                if (GUILayout.Button("<b>Lancer Séquence</b>", GUILayout.Width(120)))
                {
                    var launcher = activeUnits.FirstOrDefault(u => battle.btl_scene.PatAddr[battle.btl_scene.PatNum].Monster[u.Data.bi.slot_no].TypeNo == curSeqEnemyType);
                    if (launcher == null) launcher = activeUnits.FirstOrDefault(); // Fallback de sécurité

                    if (launcher != null)
                    {
                        btlseq.StartBtlSeq(launcher.Id, _forceTargetId, curSeqId);

                        int sfxToPlay = 103;
                        if (curSeqEnemyType >= 0 && curSeqEnemyType < battle.btl_scene.MonAddr.Length)
                            sfxToPlay = battle.btl_scene.MonAddr[curSeqEnemyType].StartSfx;

                        SoundLib.PlaySoundEffect(sfxToPlay);
                    }
                }
                GUILayout.EndHorizontal();

                if (_showForceTargetDropdown)
                {
                    GUILayout.BeginVertical("box");
                    _forceTargetScrollPos = GUILayout.BeginScrollView(_forceTargetScrollPos, GUILayout.Height(120));

                    foreach (var option in targetOptions)
                    {
                        bool isSelected = (_forceTargetId == option.Value);
                        string optionText = isSelected ? $"<color=orange>{option.Key}</color>" : option.Key;

                        if (GUILayout.Button(optionText, new GUIStyle(GUI.skin.button) { richText = true, alignment = TextAnchor.MiddleLeft }))
                        {
                            _forceTargetId = option.Value;
                            _showForceTargetDropdown = false;
                            SoundLib.PlaySoundEffect(103);
                        }
                    }
                    GUILayout.EndScrollView();
                    GUILayout.EndVertical();
                }

                GUILayout.EndVertical();
                GUILayout.EndVertical();
            }

            private void DrawUnitsTab()
            {
                List<BattleUnit> units = BattleState.EnumerateUnits().ToList();
                if (units.Count == 0) return;
                if (_currentUnitIndex >= units.Count) _currentUnitIndex = 0;

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("<", GUILayout.Width(40), GUILayout.Height(30))) { _currentUnitIndex--; if (_currentUnitIndex < 0) _currentUnitIndex = units.Count - 1; _statTextCache.Clear(); GUI.FocusControl(null); }
                BattleUnit u = units[_currentUnitIndex];
                string n = u.IsPlayer ? $"⭐ <color=#00FFFF>{FF9TextTool.CharacterDefaultName(u.PlayerIndex)}</color> ⭐" : $"👾 <color=#FF5555>{SpecialFilesTranceSeek.RemoveTags(u.Name)}</color> 👾";
                GUILayout.Label($"<b>{n}</b>\nID: {u.Id}", new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter });
                if (GUILayout.Button(">", GUILayout.Width(40), GUILayout.Height(30))) { _currentUnitIndex++; if (_currentUnitIndex >= units.Count) _currentUnitIndex = 0; _statTextCache.Clear(); GUI.FocusControl(null); }
                GUILayout.Space(10);
                if (GUILayout.Button("🔄 Refresh", GUILayout.Width(80), GUILayout.Height(30)))
                {
                    _statTextCache.Clear();
                    GUI.FocusControl(null);
                    SoundLib.PlaySoundEffect(103);
                }
                if (GUILayout.Button("✅ Apply", GUILayout.Width(90), GUILayout.Height(30)))
                {
                    ApplyUnitStats(u);
                    _statTextCache.Clear();
                    GUI.FocusControl(null);
                    SoundLib.PlaySoundEffect(104);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginVertical("box");
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical(GUILayout.Width(220));
                DrawUnitStatUI($"{u.Id}_HP", "HP", (int)u.CurrentHp, 0, 99999);
                DrawUnitStatUI($"{u.Id}_MaxHP", "Max HP", (int)u.MaximumHp, 1, 99999);
                DrawUnitStatUI($"{u.Id}_MP", "MP", (int)u.CurrentMp, 0, 9999);
                DrawUnitStatUI($"{u.Id}_MaxMP", "Max MP", (int)u.MaximumMp, 1, 9999);
                DrawUnitStatUI($"{u.Id}_Lvl", "Level", u.Level, 1, 99);
                DrawUnitStatUI($"{u.Id}_Trance", "Trance", u.Trance, 0, 255);
                GUILayout.EndVertical();
                GUILayout.BeginVertical(GUILayout.Width(220));
                DrawUnitStatUI($"{u.Id}_Str", "Strength", u.Strength, 0, 255);
                DrawUnitStatUI($"{u.Id}_Mag", "Magic", u.Magic, 0, 255);
                DrawUnitStatUI($"{u.Id}_Dex", "Dexterity", u.Dexterity, 0, 255);
                DrawUnitStatUI($"{u.Id}_Will", "Will", u.Will, 0, 255);
                DrawUnitStatUI($"{u.Id}_PDef", "Phys Def", u.PhysicalDefence, 0, 9999);
                DrawUnitStatUI($"{u.Id}_PEvd", "Phys Evd", u.PhysicalEvade, 0, 9999);
                DrawUnitStatUI($"{u.Id}_MDef", "Mag Def", u.MagicDefence, 0, 9999);
                DrawUnitStatUI($"{u.Id}_MEvd", "Mag Evd", u.MagicEvade, 0, 9999);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUILayout.Space(5);

                GUILayout.BeginVertical("box");
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("<", GUILayout.Width(30))) { _statusMode--; if (_statusMode < 0) _statusMode = 2; }
                GUILayout.Label(_statusModeNames[_statusMode], new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter });
                if (GUILayout.Button(">", GUILayout.Width(30))) { _statusMode++; if (_statusMode > 2) _statusMode = 0; }
                GUILayout.EndHorizontal();
                _statusScrollPos = GUILayout.BeginScrollView(_statusScrollPos, GUILayout.Height(150));
                int col = 3, c = 0; GUILayout.BeginHorizontal();
                foreach (var kv in _statusList)
                {
                    bool h = false;
                    if (_statusMode == 0) h = (u.CurrentStatus & kv.Value) != 0;
                    else if (_statusMode == 1) h = (u.PermanentStatus & kv.Value) != 0;
                    else h = (u.ResistStatus & kv.Value) != 0;

                    bool t = GUILayout.Toggle(h, kv.Key, GUILayout.Width(135));
                    if (t != h)
                    {
                        if (_statusMode == 0) { if (t) u.AlterStatus(kv.Value, u); else u.RemoveStatus(kv.Value); }
                        else if (_statusMode == 1) btl_stat.MakeStatusesPermanent(u, kv.Value, t);
                        else { if (t) u.ResistStatus |= kv.Value; else u.ResistStatus &= ~kv.Value; }
                    }
                    c++; if (c % col == 0) { GUILayout.EndHorizontal(); GUILayout.BeginHorizontal(); }
                }
                GUILayout.EndHorizontal(); GUILayout.EndScrollView();
                GUILayout.EndVertical();
                GUILayout.Space(5);

                GUILayout.BeginVertical("box");
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("<", GUILayout.Width(30))) { _elementMode--; if (_elementMode < 0) _elementMode = 4; }
                GUILayout.Label(_elementModeNames[_elementMode], new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter });
                if (GUILayout.Button(">", GUILayout.Width(30))) { _elementMode++; if (_elementMode > 4) _elementMode = 0; }
                GUILayout.EndHorizontal();
                _elementScrollPos = GUILayout.BeginScrollView(_elementScrollPos, GUILayout.Height(80));
                int colElem = 4, cElem = 0; GUILayout.BeginHorizontal();
                foreach (var kv in _elementList)
                {
                    bool hElem = false;
                    if (_elementMode == 0) hElem = (u.WeakElement & kv.Value) != 0;
                    else if (_elementMode == 1) hElem = (u.HalfElement & kv.Value) != 0;
                    else if (_elementMode == 2) hElem = (u.GuardElement & kv.Value) != 0;
                    else if (_elementMode == 3) hElem = (u.AbsorbElement & kv.Value) != 0;
                    else hElem = (u.BonusElement & kv.Value) != 0;

                    bool tElem = GUILayout.Toggle(hElem, kv.Key, GUILayout.Width(100));
                    if (tElem != hElem)
                    {
                        if (_elementMode == 0) { if (tElem) u.WeakElement |= kv.Value; else u.WeakElement &= ~kv.Value; }
                        else if (_elementMode == 1) { if (tElem) u.HalfElement |= kv.Value; else u.HalfElement &= ~kv.Value; }
                        else if (_elementMode == 2) { if (tElem) u.GuardElement |= kv.Value; else u.GuardElement &= ~kv.Value; }
                        else if (_elementMode == 3) { if (tElem) u.AbsorbElement |= kv.Value; else u.AbsorbElement &= ~kv.Value; }
                        else { if (tElem) u.BonusElement |= kv.Value; else u.BonusElement &= ~kv.Value; }
                    }
                    cElem++; if (cElem % colElem == 0) { GUILayout.EndHorizontal(); GUILayout.BeginHorizontal(); }
                }
                GUILayout.EndHorizontal(); GUILayout.EndScrollView();
                GUILayout.EndVertical();
                GUILayout.EndVertical();
            }

            private void DrawUnitStatUI(string k, string l, int liveValue, int min, int max, int w = 75)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(l, GUILayout.Width(w));

                if (!_statTextCache.ContainsKey(k))
                    _statTextCache[k] = liveValue.ToString();

                bool isModified = false;
                if (int.TryParse(_statTextCache[k], out int parsed) && parsed != liveValue)
                    isModified = true;

                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    if (int.TryParse(_statTextCache[k], out int v)) _statTextCache[k] = Mathf.Clamp(v - 1, min, max).ToString();
                    GUI.FocusControl(null);
                }

                GUI.SetNextControlName(k);

                Color defaultColor = GUI.contentColor;
                if (isModified) GUI.contentColor = Color.yellow;

                _statTextCache[k] = GUILayout.TextField(_statTextCache[k], GUILayout.Width(45));

                GUI.contentColor = defaultColor;

                if (GUILayout.Button("+", GUILayout.Width(20)))
                {
                    if (int.TryParse(_statTextCache[k], out int v)) _statTextCache[k] = Mathf.Clamp(v + 1, min, max).ToString();
                    GUI.FocusControl(null);
                }
                GUILayout.EndHorizontal();
            }

            private void ApplyUnitStats(BattleUnit u)
            {
                u.CurrentHp = (uint)GetCachedStat($"{u.Id}_HP", (int)u.CurrentHp, 0, 99999);
                u.MaximumHp = (uint)GetCachedStat($"{u.Id}_MaxHP", (int)u.MaximumHp, 1, 99999);
                u.CurrentMp = (uint)GetCachedStat($"{u.Id}_MP", (int)u.CurrentMp, 0, 9999);
                u.MaximumMp = (uint)GetCachedStat($"{u.Id}_MaxMP", (int)u.MaximumMp, 1, 9999);
                u.Level = (byte)GetCachedStat($"{u.Id}_Lvl", u.Level, 1, 99);
                u.Trance = (byte)GetCachedStat($"{u.Id}_Trance", u.Trance, 0, 255);
                u.Strength = (byte)GetCachedStat($"{u.Id}_Str", u.Strength, 0, 255);
                u.Magic = (byte)GetCachedStat($"{u.Id}_Mag", u.Magic, 0, 255);
                u.Dexterity = (byte)GetCachedStat($"{u.Id}_Dex", u.Dexterity, 0, 255);
                u.Will = (byte)GetCachedStat($"{u.Id}_Will", u.Will, 0, 255);
                u.PhysicalDefence = GetCachedStat($"{u.Id}_PDef", u.PhysicalDefence, 0, 9999);
                u.PhysicalEvade = GetCachedStat($"{u.Id}_PEvd", u.PhysicalEvade, 0, 9999);
                u.MagicDefence = GetCachedStat($"{u.Id}_MDef", u.MagicDefence, 0, 9999);
                u.MagicEvade = GetCachedStat($"{u.Id}_MEvd", u.MagicEvade, 0, 9999);
            }

            private int GetCachedStat(string key, int fallback, int min, int max)
            {
                if (_statTextCache.TryGetValue(key, out string valStr) && int.TryParse(valStr, out int val))
                    return Mathf.Clamp(val, min, max);
                return fallback;
            }

            void DrawAbilitiesMenu(int windowID)
            {
                var aa = FF9StateSystem.Battle.FF9Battle?.aa_data; if (aa == null) return;
                var list = aa.ToList(); GUILayout.BeginHorizontal(); GUILayout.Label("Search ID:", GUILayout.Width(75)); _searchAbilityIdStr = GUILayout.TextField(_searchAbilityIdStr, GUILayout.Width(50));
                if (GUILayout.Button("Go")) { if (int.TryParse(_searchAbilityIdStr, out int id)) { int f = list.FindIndex(k => (int)k.Key == id); if (f != -1) { _currentAbilityIndex = f; _statTextCache.Clear(); GUI.FocusControl(null); } } }
                GUILayout.FlexibleSpace(); if (GUILayout.Button("Reload DB")) { try { Type t = typeof(FF9BattleDB); MethodInfo m = t.GetMethod("LoadActions", BindingFlags.NonPublic | BindingFlags.Static); if (m != null) { var na = m.Invoke(null, null) as Dictionary<BattleAbilityId, AA_DATA>; if (na != null) { FF9StateSystem.Battle.FF9Battle.aa_data = na; list = FF9StateSystem.Battle.FF9Battle.aa_data.ToList(); _statTextCache.Clear(); SoundLib.PlaySoundEffect(108); } } } catch { } }
                GUILayout.EndHorizontal(); if (_currentAbilityIndex >= list.Count) _currentAbilityIndex = 0;
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("<", GUILayout.Width(40), GUILayout.Height(30))) { _currentAbilityIndex--; if (_currentAbilityIndex < 0) _currentAbilityIndex = list.Count - 1; _statTextCache.Clear(); GUI.FocusControl(null); }
                var kv = list[_currentAbilityIndex]; int aid = (int)kv.Key; AA_DATA a = kv.Value; string an = GetAbilityName(aid); if (an == "Unknown" && !string.IsNullOrEmpty(a.Name)) an = SpecialFilesTranceSeek.RemoveTags(a.Name);
                GUILayout.Label($"<b>🔮 <color=#FFFFFF>{an}</color> 🔮</b>\nID: {aid}", new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter });
                if (GUILayout.Button(">", GUILayout.Width(40), GUILayout.Height(30))) { _currentAbilityIndex++; if (_currentAbilityIndex >= list.Count) _currentAbilityIndex = 0; _statTextCache.Clear(); GUI.FocusControl(null); }
                GUILayout.EndHorizontal();
                GUILayout.BeginVertical("box"); GUILayout.BeginHorizontal();
                GUILayout.BeginVertical(GUILayout.Width(220)); a.Ref.Power = Mathf.Clamp(DrawStatUI($"PAb_{aid}_Power", "Power", a.Ref.Power), 0, 9999); a.Ref.Rate = Mathf.Clamp(DrawStatUI($"PAb_{aid}_Rate", "Hit Rate", a.Ref.Rate), 0, 9999); a.MP = Mathf.Clamp(DrawStatUI($"PAb_{aid}_MP", "MP Cost", a.MP), 0, 9999); a.Ref.Elements = (byte)Mathf.Clamp(DrawStatUI($"PAb_{aid}_Elem", "Elements", a.Ref.Elements), 0, 255); a.AddStatusNo = (StatusSetId)Mathf.Clamp(DrawStatUI($"PAb_{aid}_Status", "Status Set", (int)a.AddStatusNo), 0, 255); a.Category = (byte)Mathf.Clamp(DrawStatUI($"PAb_{aid}_Cat", "Category", a.Category), 0, 255); a.Ref.ScriptId = Mathf.Clamp(DrawStatUI($"PAb_{aid}_Script", "Script ID", a.Ref.ScriptId), 0, 9999); GUILayout.EndVertical();
                GUILayout.BeginVertical(GUILayout.Width(220)); if (a.Info != null) { a.Info.VfxIndex = (short)Mathf.Clamp(DrawStatUI($"PAb_{aid}_Vfx", "Vfx Index", a.Info.VfxIndex), -1, 9999); a.Info.Target = (TargetType)Mathf.Clamp(DrawStatUI($"PAb_{aid}_Target", "Target", (int)a.Info.Target), 0, 255); a.Info.DisplayStats = (TargetDisplay)Mathf.Clamp(DrawStatUI($"PAb_{aid}_Disp", "Display", (int)a.Info.DisplayStats), 0, 255); GUILayout.Space(5); a.Info.DefaultAlly = GUILayout.Toggle(a.Info.DefaultAlly, "Default Ally"); a.Info.ForDead = GUILayout.Toggle(a.Info.ForDead, "For Dead"); a.Info.DefaultCamera = GUILayout.Toggle(a.Info.DefaultCamera, "Default Camera"); a.Info.DefaultOnDead = GUILayout.Toggle(a.Info.DefaultOnDead, "Default On Dead"); }
                GUILayout.EndVertical(); GUILayout.EndHorizontal();
                if (a.Info != null) { GUILayout.BeginHorizontal(); GUILayout.Label("SEQ File:", GUILayout.Width(75)); string ck = $"PAb_{aid}_Seq"; if (!_statTextCache.ContainsKey(ck)) _statTextCache[ck] = a.Info.SequenceFile ?? ""; _statTextCache[ck] = GUILayout.TextField(_statTextCache[ck], GUILayout.Width(250)); a.Info.SequenceFile = _statTextCache[ck]; if (GUILayout.Button("Reload")) { GUI.FocusControl(null); if (!string.IsNullOrEmpty(a.Info.SequenceFile)) { string s = AssetManager.LoadString(a.Info.SequenceFile); if (s != null) { a.Info.VfxAction = new UnifiedBattleSequencer.BattleAction(s); SoundLib.PlaySoundEffect(108); } } } GUILayout.EndHorizontal(); }
                GUILayout.EndVertical(); GUI.DragWindow();
            }

            void DrawItemsMenu(int windowID)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(_itemsMenuTab == 0 ? "<color=orange><b>Regular Items</b></color>" : "Regular Items")) { _itemsMenuTab = 0; _statTextCache.Clear(); GUI.FocusControl(null); }
                if (GUILayout.Button(_itemsMenuTab == 1 ? "<color=orange><b>Key Items</b></color>" : "Key Items")) { _itemsMenuTab = 1; _statTextCache.Clear(); GUI.FocusControl(null); }
                GUILayout.EndHorizontal();

                GUILayout.Space(10);

                if (_itemsMenuTab == 0)
                    DrawRegularItemsTab();
                else
                    DrawImportantItemsTab();

                GUI.DragWindow();
            }

            private void DrawRegularItemsTab()
            {
                var itemDatabase = ff9item._FF9Item_Data;
                if (itemDatabase == null) return;
                var itemList = itemDatabase.Keys.ToList();

                GUILayout.BeginHorizontal();
                GUILayout.Label("ID:", GUILayout.Width(30));
                _searchItemIdStr = GUILayout.TextField(_searchItemIdStr, GUILayout.Width(45));

                if (GUILayout.Button("Go", GUILayout.Width(40)))
                {
                    if (int.TryParse(_searchItemIdStr, out int parsedId))
                    {
                        int index = itemList.FindIndex(k => (int)k == parsedId);
                        if (index != -1)
                        {
                            _currentItemIndex = index;
                            _statTextCache.Clear();
                            GUI.FocusControl(null);
                            SoundLib.PlaySoundEffect(103);
                        }
                        else
                        {
                            SoundLib.PlaySoundEffect(102);
                        }
                    }
                    else
                    {
                        SoundLib.PlaySoundEffect(102);
                    }
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Reload DB"))
                {
                    try
                    {
                        Type type = typeof(ff9item);
                        System.Reflection.MethodInfo method = type.GetMethod("LoadItems", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                        if (method != null)
                        {
                            var reloadedData = method.Invoke(null, null) as Dictionary<RegularItem, FF9ITEM_DATA>;
                            if (reloadedData != null)
                            {
                                ff9item._FF9Item_Data = reloadedData;
                                _statTextCache.Clear();
                                SoundLib.PlaySoundEffect(108);
                                Memoria.Prime.Log.Message("[DebugMenu] Items DB reloaded successfully.");
                            }
                            else
                            {
                                SoundLib.PlaySoundEffect(102);
                            }
                        }
                        else
                        {
                            SoundLib.PlaySoundEffect(102);
                        }
                    }
                    catch
                    {
                        SoundLib.PlaySoundEffect(102);
                    }
                    GUI.FocusControl(null);
                }

                if (GUILayout.Button("All x99"))
                {
                    foreach (RegularItem itemKey in itemDatabase.Keys)
                    {
                        ff9item.FF9Item_Add(itemKey, 99);
                    }
                    SoundLib.PlaySoundEffect(108);
                    GUI.FocusControl(null);
                }
                GUILayout.EndHorizontal();

                if (_currentItemIndex >= itemList.Count) _currentItemIndex = 0;

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("<", GUILayout.Width(40), GUILayout.Height(30)))
                {
                    _currentItemIndex--;
                    if (_currentItemIndex < 0) _currentItemIndex = itemList.Count - 1;
                    _statTextCache.Clear();
                    GUI.FocusControl(null);
                }

                RegularItem currentItem = itemList[_currentItemIndex];
                int itemId = (int)currentItem;
                string itemName = "Unknown";

                try
                {
                    string rawName = FF9TextTool.ItemName(currentItem);
                    if (!string.IsNullOrEmpty(rawName))
                    {
                        itemName = SpecialFilesTranceSeek.RemoveTags(rawName);
                    }
                }
                catch { }

                GUILayout.Label($"<b>🧪 <color=#00FF00>{itemName}</color> 🧪</b>\nID: {itemId}", new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter });

                if (GUILayout.Button(">", GUILayout.Width(40), GUILayout.Height(30)))
                {
                    _currentItemIndex++;
                    if (_currentItemIndex >= itemList.Count) _currentItemIndex = 0;
                    _statTextCache.Clear();
                    GUI.FocusControl(null);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginVertical("box");
                GUILayout.BeginHorizontal();
                _itemQuantityToAdd = Mathf.Clamp(DrawStatUI("Item_Qty", "Qté", _itemQuantityToAdd, 100), 1, 99);

                if (GUILayout.Button("Donner"))
                {
                    if (itemDatabase.ContainsKey(currentItem))
                    {
                        ff9item.FF9Item_Add(currentItem, _itemQuantityToAdd);
                        SoundLib.PlaySoundEffect(108);
                    }
                    else
                    {
                        SoundLib.PlaySoundEffect(102);
                    }
                }

                if (GUILayout.Button("Retirer"))
                {
                    if (itemDatabase.ContainsKey(currentItem))
                    {
                        ff9item.FF9Item_Remove(currentItem, _itemQuantityToAdd);
                        SoundLib.PlaySoundEffect(108);
                    }
                    else
                    {
                        SoundLib.PlaySoundEffect(102);
                    }
                }

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }

            private void DrawImportantItemsTab()
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("ID:", GUILayout.Width(50));
                _searchImportantItemIdStr = GUILayout.TextField(_searchImportantItemIdStr, GUILayout.Width(50));

                if (GUILayout.Button("Go"))
                {
                    if (int.TryParse(_searchImportantItemIdStr, out int id) && id >= 0 && id <= 255)
                    {
                        _currentImportantItemIndex = id; _statTextCache.Clear(); GUI.FocusControl(null);
                    }
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                if (_currentImportantItemIndex < 0) _currentImportantItemIndex = 255;
                if (_currentImportantItemIndex > 255) _currentImportantItemIndex = 0;

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("<", GUILayout.Width(40), GUILayout.Height(30))) { _currentImportantItemIndex--; if (_currentImportantItemIndex < 0) _currentImportantItemIndex = 255; _statTextCache.Clear(); GUI.FocusControl(null); }

                int iid = _currentImportantItemIndex;
                string itemName = "Unknown";
                try
                {
                    string rawName = FF9TextTool.ImportantItemName(iid);
                    if (!string.IsNullOrEmpty(rawName)) itemName = SpecialFilesTranceSeek.RemoveTags(rawName);
                }
                catch { }

                bool playerHasItem = ff9item.FF9Item_IsExistImportant(iid);
                string colorHex = playerHasItem ? "FFFF00" : "888888";

                GUILayout.Label($"<b>🔑 <color=#{colorHex}>{itemName}</color> 🔑</b>\nID: {iid}", new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter });

                if (GUILayout.Button(">", GUILayout.Width(40), GUILayout.Height(30))) { _currentImportantItemIndex++; if (_currentImportantItemIndex > 255) _currentImportantItemIndex = 0; _statTextCache.Clear(); GUI.FocusControl(null); }
                GUILayout.EndHorizontal();

                GUILayout.BeginVertical("box");
                GUILayout.BeginHorizontal();

                if (playerHasItem)
                {
                    if (GUILayout.Button("Retirer")) { ff9item.FF9Item_RemoveImportant(iid); SoundLib.PlaySoundEffect(108); }
                }
                else
                {
                    if (GUILayout.Button("Donner")) { ff9item.FF9Item_AddImportant(iid); SoundLib.PlaySoundEffect(108); }
                }

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }

            void DrawEventMenu(int windowID)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(_eventMenuTab == 0 ? "<color=orange><b>gEventGlobal</b></color>" : "gEventGlobal")) { _eventMenuTab = 0; _statTextCache.Clear(); GUI.FocusControl(null); }
                if (GUILayout.Button(_eventMenuTab == 1 ? "<color=orange><b>gScriptDictionary</b></color>" : "gScriptDictionary")) { _eventMenuTab = 1; _statTextCache.Clear(); GUI.FocusControl(null); }
                if (GUILayout.Button(_eventMenuTab == 2 ? "<color=orange><b>Map Vars (Global)</b></color>" : "Map Vars (Global)")) { _eventMenuTab = 2; _statTextCache.Clear(); GUI.FocusControl(null); }
                if (GUILayout.Button(_eventMenuTab == 3 ? "<color=orange><b>Local Vars (Obj)</b></color>" : "Local Vars (Obj)")) { _eventMenuTab = 3; _statTextCache.Clear(); GUI.FocusControl(null); }
                GUILayout.EndHorizontal();

                if (_eventMenuTab == 0)
                {
                    if (FF9StateSystem.EventState?.gEventGlobal == null) return;
                    byte[] g = FF9StateSystem.EventState.gEventGlobal;

                    GUILayout.BeginVertical("box");

                    GUILayout.BeginHorizontal();
                    _selectedGlobalKey = Mathf.Clamp(DrawStatUI("GlobalKey", "Index", _selectedGlobalKey, 100), 0, 2047);
                    if (GUILayout.Button("Refresh", GUILayout.Width(80))) { _statTextCache.Clear(); GUI.FocusControl(null); SoundLib.PlaySoundEffect(103); }
                    GUILayout.EndHorizontal();

                    int currentGlobalValue = g[_selectedGlobalKey];
                    DrawStatUI($"EvGlob_{_selectedGlobalKey}", "Valeur", currentGlobalValue, 100);

                    GUILayout.Space(10);
                    if (GUILayout.Button("<color=yellow><b>Appliquer</b></color>", new GUIStyle(GUI.skin.button) { richText = true }, GUILayout.Height(30)))
                    {
                        foreach (var kvp in _statTextCache)
                        {
                            if (kvp.Key.StartsWith("EvGlob_") && byte.TryParse(kvp.Value, out byte val))
                            {
                                if (int.TryParse(kvp.Key.Substring(7), out int idx) && idx >= 0 && idx < g.Length)
                                {
                                    g[idx] = val;
                                }
                            }
                        }
                        SoundLib.PlaySoundEffect(104);
                    }
                    GUILayout.EndVertical();
                }
                else if (_eventMenuTab == 1) DrawScriptDictionaryTab();
                else if (_eventMenuTab == 2) DrawMapVarsTab();
                else if (_eventMenuTab == 3) DrawLocalVarsTab();

                GUI.DragWindow();
            }

            private void DrawScriptDictionaryTab()
            {
                var d = FF9StateSystem.EventState?.gScriptDictionary;
                if (d == null || d.Count == 0) return;

                GUILayout.BeginHorizontal();
                foreach (int k in d.Keys)
                {
                    if (GUILayout.Button(k.ToString(), GUILayout.Width(50)))
                    {
                        _selectedOuterKey = k;
                        _statTextCache.Clear();
                        GUI.FocusControl(null);
                    }
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Refresh", GUILayout.Width(80))) { _statTextCache.Clear(); GUI.FocusControl(null); SoundLib.PlaySoundEffect(103); }
                GUILayout.EndHorizontal();

                if (_selectedOuterKey != -1 && d.ContainsKey(_selectedOuterKey))
                {
                    _eventScrollPos = GUILayout.BeginScrollView(_eventScrollPos, GUILayout.Height(250));
                    var inner = d[_selectedOuterKey];
                    int col = 2;
                    int c = 0;

                    GUILayout.BeginHorizontal();
                    foreach (int ik in inner.Keys.ToList())
                    {
                        GUILayout.BeginVertical("box", GUILayout.Width(250));
                        DrawStatUI($"Dict_{_selectedOuterKey}_{ik}", $"Key {ik}", inner[ik]);
                        GUILayout.EndVertical();

                        c++;
                        if (c % col == 0)
                        {
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                        }
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.EndScrollView();

                    GUILayout.Space(5);
                    if (GUILayout.Button("<color=yellow><b>Appliquer</b></color>", new GUIStyle(GUI.skin.button) { richText = true }, GUILayout.Height(30)))
                    {
                        foreach (var kvp in _statTextCache)
                        {
                            if (kvp.Key.StartsWith("Dict_") && int.TryParse(kvp.Value, out int val))
                            {
                                string[] parts = kvp.Key.Split('_');
                                if (parts.Length == 3 && int.TryParse(parts[1], out int outer) && int.TryParse(parts[2], out int innerK))
                                {
                                    if (d.ContainsKey(outer) && d[outer].ContainsKey(innerK))
                                    {
                                        d[outer][innerK] = val;
                                    }
                                }
                            }
                        }
                        SoundLib.PlaySoundEffect(104);
                    }
                }
            }

            private void DrawMapVarsTab()
            {
                var ee = PersistenSingleton<EventEngine>.Instance;
                if (ee == null) return;

                byte[] mapVars = ee.GetMapVar();
                if (mapVars == null || mapVars.Length == 0)
                {
                    GUILayout.Label("Aucune Map Variable (Combat Global) active.");
                    return;
                }

                GUILayout.BeginVertical("box");
                GUILayout.Label("<b>Variables Globales du Combat (Hades Workshop 'global')</b>", new GUIStyle(GUI.skin.label) { richText = true });

                GUILayout.BeginHorizontal();
                _selectedMapVarKey = Mathf.Clamp(DrawStatUI("MapVarKey", "Index (Offset)", _selectedMapVarKey, 110), 0, mapVars.Length - 1);

                if (GUILayout.Button("Refresh", GUILayout.Width(80)))
                {
                    _statTextCache.Clear();
                    GUI.FocusControl(null);
                    SoundLib.PlaySoundEffect(103);
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(10);

                int currentValue = mapVars[_selectedMapVarKey];
                DrawStatUI($"MapVarMod_{_selectedMapVarKey}", "Modifier UInt8", currentValue, 110);

                if (_selectedMapVarKey + 1 < mapVars.Length)
                {
                    byte b1 = (byte)GetCachedStat($"MapVarMod_{_selectedMapVarKey}", mapVars[_selectedMapVarKey], 0, 255);
                    byte b2 = (byte)GetCachedStat($"MapVarMod_{_selectedMapVarKey + 1}", mapVars[_selectedMapVarKey + 1], 0, 255);
                    ushort u16 = (ushort)(b1 | (b2 << 8));
                    GUILayout.Label($"<b>Valeur UInt16 (Lecture) :</b> <color=cyan>{u16}</color>", new GUIStyle(GUI.skin.label) { richText = true });
                }

                GUILayout.Space(10);

                if (GUILayout.Button("<color=yellow><b>Appliquer</b></color>", new GUIStyle(GUI.skin.button) { richText = true }, GUILayout.Height(30)))
                {
                    foreach (var kvp in _statTextCache)
                    {
                        if (kvp.Key.StartsWith("MapVarMod_") && byte.TryParse(kvp.Value, out byte val))
                        {
                            if (int.TryParse(kvp.Key.Substring(10), out int idx) && idx >= 0 && idx < mapVars.Length)
                            {
                                mapVars[idx] = val;
                            }
                        }
                    }
                    SoundLib.PlaySoundEffect(104);
                }

                GUILayout.EndVertical();
            }

            private void DrawLocalVarsTab()
            {
                var ee = PersistenSingleton<EventEngine>.Instance;
                if (ee == null) return;

                List<Obj> activeObjects = new List<Obj>();
                for (ObjList objList = ee.GetActiveObjList().next; objList != null; objList = objList.next)
                {
                    if (objList.obj != null)
                        activeObjects.Add(objList.obj);
                }

                if (activeObjects.Count == 0)
                {
                    GUILayout.Label("Aucun Objet actif.");
                    return;
                }

                if (_currentObjIndex >= activeObjects.Count) _currentObjIndex = 0;

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("<", GUILayout.Width(40), GUILayout.Height(30)))
                {
                    _currentObjIndex--;
                    if (_currentObjIndex < 0) _currentObjIndex = activeObjects.Count - 1;
                    _statTextCache.Clear();
                    GUI.FocusControl(null);
                }

                Obj currObj = activeObjects[_currentObjIndex];

                string objName = (currObj.go != null && !string.IsNullOrEmpty(currObj.go.name))
                                 ? currObj.go.name
                                 : $"Sans Nom (UID: {currObj.uid})";

                GUILayout.Label($"<b>Obj: <color=#FF5555>{objName}</color></b>\nUID: {currObj.uid} ({_currentObjIndex + 1}/{activeObjects.Count})", new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter });

                if (GUILayout.Button(">", GUILayout.Width(40), GUILayout.Height(30)))
                {
                    _currentObjIndex++;
                    if (_currentObjIndex >= activeObjects.Count) _currentObjIndex = 0;
                    _statTextCache.Clear();
                    GUI.FocusControl(null);
                }
                GUILayout.EndHorizontal();

                if (currObj.buffer == null || currObj.buffer.Length == 0)
                {
                    GUILayout.Label("Ce monstre/objet n'a aucune variable locale (buffer vide).");
                    return;
                }

                GUILayout.BeginVertical("box");
                GUILayout.Label("<b>Variables Locales du Monstre/Objet</b>", new GUIStyle(GUI.skin.label) { richText = true });

                int bufferOffset = currObj.vofs * 4;
                int maxHwOffset = currObj.buffer.Length - bufferOffset - 1;

                GUILayout.BeginHorizontal();
                _selectedLocalVarKey = Mathf.Clamp(DrawStatUI("LocalVarKey", "Index HW", _selectedLocalVarKey, 110), 0, maxHwOffset);

                if (GUILayout.Button("Refresh", GUILayout.Width(80)))
                {
                    _statTextCache.Clear();
                    GUI.FocusControl(null);
                    SoundLib.PlaySoundEffect(103);
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(10);

                int actualIndex = _selectedLocalVarKey + bufferOffset;
                int currentLocalValue = currObj.buffer[actualIndex];

                DrawStatUI($"LocVarMod_{currObj.uid}_{_selectedLocalVarKey}", "Modifier UInt8", currentLocalValue, 110);

                if (actualIndex + 1 < currObj.buffer.Length)
                {
                    byte b1 = (byte)GetCachedStat($"LocVarMod_{currObj.uid}_{_selectedLocalVarKey}", currObj.buffer[actualIndex], 0, 255);
                    byte b2 = (byte)GetCachedStat($"LocVarMod_{currObj.uid}_{_selectedLocalVarKey + 1}", currObj.buffer[actualIndex + 1], 0, 255);
                    ushort u16 = (ushort)(b1 | (b2 << 8));
                    GUILayout.Label($"<b>Valeur UInt16 (Lecture) :</b> <color=cyan>{u16}</color>", new GUIStyle(GUI.skin.label) { richText = true });
                }

                GUILayout.Space(10);
                if (GUILayout.Button("<color=yellow><b>Appliquer</b></color>", new GUIStyle(GUI.skin.button) { richText = true }, GUILayout.Height(30)))
                {
                    foreach (var kvp in _statTextCache)
                    {
                        if (kvp.Key.StartsWith($"LocVarMod_{currObj.uid}_") && byte.TryParse(kvp.Value, out byte val))
                        {
                            if (int.TryParse(kvp.Key.Substring($"LocVarMod_{currObj.uid}_".Length), out int offset))
                            {
                                int targetIndex = offset + bufferOffset;
                                if (targetIndex >= 0 && targetIndex < currObj.buffer.Length)
                                {
                                    currObj.buffer[targetIndex] = val;
                                }
                            }
                        }
                    }
                    SoundLib.PlaySoundEffect(104);
                }

                GUILayout.EndVertical();
            }

            private void ReloadAbilityFeatures()
            {
                try
                {
                    ff9abil._FeatureStats.Clear();
                    string inputPath = Memoria.Assets.DataResources.Characters.Abilities.PureDirectory + Memoria.Assets.DataResources.Characters.Abilities.SAFeaturesFile;
                    Dictionary<Memoria.Data.SupportAbility, Memoria.Data.SupportingAbilityFeature> result = new Dictionary<Memoria.Data.SupportAbility, Memoria.Data.SupportingAbilityFeature>();

                    foreach (AssetManager.AssetFolder folder in AssetManager.FolderLowToHigh)
                        if (folder.TryFindAssetInModOnDisc(inputPath, out String fullPath, AssetManagerUtil.GetStreamingAssetsPath() + "/"))
                            ff9abil.LoadAbilityFeatureFile(ref result, System.IO.File.ReadAllText(fullPath), fullPath);

                    if (result.Count > 0)
                    {
                        ff9abil._FF9Abil_SaFeature = result;
                        Memoria.Prime.Log.Message("[Trance Seek Debug] AbilityFeatures rechargé avec succès !");
                        SoundLib.PlaySoundEffect(108);
                    }
                }
                catch (Exception ex)
                {
                    Memoria.Prime.Log.Error(ex, "[Trance Seek Debug] Erreur lors du rechargement de AbilityFeatures.");
                }
            }

            private void ReloadAllTexts()
            {
                try
                {
                    FF9TextTool.UpdateTextLocalizationNow();

                    if (FF9TextTool.BattleZoneId != -1)
                        FF9TextTool.UpdateBattleTextNow(FF9TextTool.BattleZoneId);

                    if (FF9TextTool.FieldZoneId != -1)
                        FF9TextTool.UpdateFieldTextNow(FF9TextTool.FieldZoneId);

                    Memoria.Prime.Log.Message("[Trance Seek Debug] Textes rechargés avec succès !");
                    SoundLib.PlaySoundEffect(108);
                }
                catch (Exception ex)
                {
                    Memoria.Prime.Log.Error(ex, "[Trance Seek Debug] Erreur lors du rechargement des textes.");
                }
            }

            void DrawLangMenu(int windowID)
            {
                GUILayout.BeginVertical("box");
                GUILayout.Label("<b>Changer la langue du jeu</b>", new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter });
                GUILayout.Space(10);

                string currentLang = Memoria.Assets.Localization.CurrentLanguage;

                GUILayout.BeginHorizontal();
                if (GUILayout.Button($"Langue Actuelle : <b>{currentLang}</b>", new GUIStyle(GUI.skin.button) { richText = true }, GUILayout.Height(35)))
                {
                    _showLangDropdown = !_showLangDropdown;
                    SoundLib.PlaySoundEffect(103);
                }
                GUILayout.EndHorizontal();

                if (_showLangDropdown)
                {
                    GUILayout.BeginVertical("box");
                    _langScrollPos = GUILayout.BeginScrollView(_langScrollPos, GUILayout.Height(120));

                    foreach (string lang in Memoria.Assets.Localization.KnownLanguages)
                    {
                        bool isSelected = (currentLang == lang);
                        string optionText = isSelected ? $"<color=orange>{lang}</color>" : lang;

                        if (GUILayout.Button(optionText, new GUIStyle(GUI.skin.button) { richText = true, alignment = TextAnchor.MiddleLeft }))
                        {
                            _showLangDropdown = false;
                            SoundLib.PlaySoundEffect(103);

                            Memoria.Assets.Localization.SetCurrentLanguage(lang, null, null, false);
                            ReloadAllTexts();
                        }
                    }
                    GUILayout.EndScrollView();
                    GUILayout.EndVertical();
                }

                GUILayout.EndVertical();
                GUI.DragWindow();
            }

            void DrawFieldMenu(int windowID)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(_fieldMenuTab == 0 ? "<color=orange><b>Acteurs & Anim</b></color>" : "Acteurs & Anim")) { _fieldMenuTab = 0; GUI.FocusControl(null); }
                if (GUILayout.Button(_fieldMenuTab == 1 ? "<color=orange><b>Camera Field</b></color>" : "Camera Field")) { _fieldMenuTab = 1; GUI.FocusControl(null); }
                GUILayout.EndHorizontal();
                GUILayout.Space(10);

                if (_fieldMenuTab == 0) DrawFieldActorsTab();
                else if (_fieldMenuTab == 1) DrawCameraFieldTab();

                GUI.DragWindow();
            }

            private void DrawCameraFieldTab()
            {
                var fieldmap = PersistenSingleton<EventEngine>.Instance?.fieldmap;
                if (fieldmap == null)
                {
                    GUILayout.Label("Aucune carte active.");
                    return;
                }

                GUILayout.BeginVertical("box");
                GUILayout.Label("<b>Coordonnees Camera (VRP) Actuelles</b>", new GUIStyle(GUI.skin.label) { richText = true });
                GUILayout.Space(5);
                GUILayout.Label($"X : {fieldmap.curVRP.x:F2}");
                GUILayout.Label($"Y : {fieldmap.curVRP.y:F2}");
                GUILayout.EndVertical();

                GUILayout.Space(10);

                GUILayout.BeginVertical("box");
                GUILayout.Label("<b>Fonction : MoveCamera (EBG_scene2DScroll)</b>", new GUIStyle(GUI.skin.label) { richText = true });
                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                GUILayout.Label("X:", GUILayout.Width(20));
                _camInputX = GUILayout.TextField(_camInputX, GUILayout.Width(50));
                GUILayout.Label("Y:", GUILayout.Width(20));
                _camInputY = GUILayout.TextField(_camInputY, GUILayout.Width(50));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Durée:", GUILayout.Width(45));
                _camInputDuration = GUILayout.TextField(_camInputDuration, GUILayout.Width(40));
                GUILayout.Label("Type:", GUILayout.Width(40));
                _camInputType = GUILayout.TextField(_camInputType, GUILayout.Width(40));
                GUILayout.EndHorizontal();

                GUILayout.Space(5);
                if (GUILayout.Button("<color=yellow><b>Executer MoveCamera</b></color>", new GUIStyle(GUI.skin.button) { richText = true }))
                {
                    if (short.TryParse(_camInputX, out short x) &&
                        short.TryParse(_camInputY, out short y) &&
                        ushort.TryParse(_camInputDuration, out ushort duration) &&
                        byte.TryParse(_camInputType, out byte type))
                    {
                        _isCameraDragMode = false;
                        fieldmap.EBG_scene2DScroll(x, y, duration, type);
                        SoundLib.PlaySoundEffect(104);
                    }
                    GUI.FocusControl(null);
                }
                GUILayout.EndVertical();

                GUILayout.Space(15);

                string btnText = _isCameraDragMode ? "<color=red><b>Desactiver Mode Libre Camera</b></color>" : "<color=green><b>Activer Mode Libre Camera</b></color>";
                if (GUILayout.Button(btnText, new GUIStyle(GUI.skin.button) { richText = true }, GUILayout.Height(40)))
                {
                    _isCameraDragMode = !_isCameraDragMode;

                    if (_isCameraDragMode)
                    {
                        _targetCamX = fieldmap.curVRP.x;
                        _targetCamY = fieldmap.curVRP.y;
                    }
                    else
                    {
                        fieldmap.EBG_scene2DScrollRelease(30, 0);
                    }

                    SoundLib.PlaySoundEffect(103);
                    GUI.FocusControl(null);
                }

                if (_isCameraDragMode)
                {
                    GUILayout.Space(10);
                    GUILayout.Label("<i>Utilisez les <b>fleches du clavier</b> pour deplacer la camera.\nMaintenez <b>SHIFT</b> pour aller plus vite.</i>", new GUIStyle(GUI.skin.label) { richText = true });
                }
            }

            private void DrawFieldActorsTab()
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Rafraichir les Acteurs", GUILayout.Width(160), GUILayout.Height(30)))
                {
                    _cachedActors = UnityEngine.Object.FindObjectsOfType<FieldMapActor>();
                    _currentActorIndex = 0;
                    SoundLib.PlaySoundEffect(103);
                }
                if (GUILayout.Button("Restaurer Animations", GUILayout.Width(160), GUILayout.Height(30)))
                {
                    RestoreAllFieldAnimations();
                    SoundLib.PlaySoundEffect(103);
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);

                if (_cachedActors == null)
                    _cachedActors = UnityEngine.Object.FindObjectsOfType<FieldMapActor>();

                if (_cachedActors == null || _cachedActors.Length == 0)
                {
                    GUILayout.Label("Aucun acteur trouve sur cette carte.");
                    return;
                }

                if (_currentActorIndex >= _cachedActors.Length) _currentActorIndex = 0;

                FieldMapActor currentFma = _cachedActors[_currentActorIndex];
                Actor actor = currentFma?.actor;

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("<", GUILayout.Width(40), GUILayout.Height(30))) { _currentActorIndex--; if (_currentActorIndex < 0) _currentActorIndex = _cachedActors.Length - 1; GUI.FocusControl(null); }

                string actorName = actor != null && actor.go != null ? actor.go.name : "Acteur Inconnu";
                int modelId = actor != null ? actor.model : -1;

                GUILayout.Label($"<b><color=#00FFFF>{actorName}</color></b>\nModel ID: {modelId} ({_currentActorIndex + 1}/{_cachedActors.Length})", new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter });

                if (GUILayout.Button(">", GUILayout.Width(40), GUILayout.Height(30))) { _currentActorIndex++; if (_currentActorIndex >= _cachedActors.Length) _currentActorIndex = 0; GUI.FocusControl(null); }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);

                GUILayout.BeginVertical("box");
                bool prevTint = _enableTint;
                _enableTint = GUILayout.Toggle(_enableTint, " <b>Activer le ciblage visuel (Magenta)</b>", new GUIStyle(GUI.skin.toggle) { richText = true });

                if (prevTint != _enableTint && _lastTintedFma != null)
                {
                    foreach (var kvp in _originalColors)
                    {
                        if (kvp.Key != null) kvp.Key.material.color = _enableTint ? Color.magenta : kvp.Value;
                    }
                }
                GUILayout.EndVertical();
                GUILayout.Space(5);

                if (actor != null && actor.go != null)
                {
                    if (_lastTintedFma != currentFma)
                    {
                        RestoreTint();
                        if (currentFma != null && currentFma.actor != null && currentFma.actor.go != null)
                        {
                            _lastTintedFma = currentFma;
                            foreach (Renderer r in currentFma.actor.go.GetComponentsInChildren<Renderer>())
                            {
                                _originalColors[r] = r.material.color;
                                if (_enableTint) r.material.color = Color.magenta;
                            }
                        }
                    }

                    GUILayout.BeginVertical("box");
                    GUILayout.Label("<b>Position & Rotation de l'Acteur</b>", new GUIStyle(GUI.skin.label) { richText = true });

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("X:", GUILayout.Width(20));
                    _posX = GUILayout.TextField(_posX, GUILayout.Width(60));
                    GUILayout.Label("Y:", GUILayout.Width(20));
                    _posY = GUILayout.TextField(_posY, GUILayout.Width(60));
                    GUILayout.Label("Z:", GUILayout.Width(20));
                    _posZ = GUILayout.TextField(_posZ, GUILayout.Width(60));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Rot Y (0-255):", GUILayout.Width(85));
                    _rotY = GUILayout.TextField(_rotY, GUILayout.Width(60));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Lire Pos/Rot", GUILayout.Width(100)))
                    {
                        _posX = actor.go.transform.position.x.ToString("F0");
                        _posY = actor.go.transform.position.y.ToString("F0");
                        _posZ = actor.go.transform.position.z.ToString("F0");

                        _rotY = (Mathf.RoundToInt(actor.rotAngle[1] / 360f * 256f) % 256).ToString();

                        SoundLib.PlaySoundEffect(103);
                        GUI.FocusControl(null);
                    }

                    if (GUILayout.Button("<color=yellow><b>Appliquer</b></color>", new GUIStyle(GUI.skin.button) { richText = true }))
                    {
                        float posx = 0f, posy = 0f, posz = 0f, nRotY = 0f;

                        bool posParsed = float.TryParse(_posX.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out posx) &&
                                         float.TryParse(_posY.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out posy) &&
                                         float.TryParse(_posZ.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out posz);

                        bool rotParsed = float.TryParse(_rotY.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out nRotY);

                        if (posParsed)
                        {
                            actor.pos[0] = actor.lastx = posx;
                            actor.pos[1] = actor.lasty = posy;
                            actor.pos[2] = actor.lastz = posz;

                            actor.fieldMapActorController?.SetPosition(new Vector3(posx, posy, posz), true, true);
                        }

                        if (rotParsed)
                        {
                            Int32 angle = (Int32)nRotY;
                            Vector3 eulerAngles2 = actor.go.transform.localRotation.eulerAngles;
                            eulerAngles2.y = (float)(angle / 256.0 * 360.0);
                            actor.rotAngle[1] = eulerAngles2.y;
                        }

                        if (posParsed || rotParsed)
                            SoundLib.PlaySoundEffect(104);

                        GUI.FocusControl(null);
                    }

                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                    GUILayout.Space(5);

                    Animation animComp = actor.go.GetComponentInChildren<Animation>();
                    if (animComp != null)
                    {
                        string identifier = "";
                        foreach (AnimationState state in animComp)
                        {
                            string[] parts = state.name.Split('_');
                            if (parts.Length >= 4)
                            {
                                identifier = $"{parts[1]}_{parts[2]}_{parts[3]}";
                                break;
                            }
                        }

                        List<KeyValuePair<Int32, String>> anims = new List<KeyValuePair<Int32, String>>();

                        if (!string.IsNullOrEmpty(identifier))
                        {
                            foreach (KeyValuePair<Int32, String> anim in FF9DBAll.AnimationDB)
                            {
                                if (anim.Value.Length > 4 && anim.Value.Substring(4).StartsWith(identifier))
                                {
                                    anims.Add(anim);
                                }
                            }
                        }
                        else
                        {
                            foreach (AnimationState state in animComp)
                            {
                                foreach (var dbAnim in FF9DBAll.AnimationDB)
                                {
                                    if (dbAnim.Value == state.name)
                                    {
                                        anims.Add(dbAnim);
                                        break;
                                    }
                                }
                            }
                        }

                        GUILayout.BeginHorizontal("box");
                        GUILayout.Label("<b>Forcer Anim ID :</b>", new GUIStyle(GUI.skin.label) { richText = true }, GUILayout.Width(110));
                        _customAnimIdInput = GUILayout.TextField(_customAnimIdInput, GUILayout.Width(60));

                        GUILayout.Label($"  <i>(Anim. lue : <color=yellow>{_lastPlayedAnimInfo}</color>)</i>", new GUIStyle(GUI.skin.label) { richText = true });

                        if (GUILayout.Button("Jouer", GUILayout.Width(80)))
                        {
                            if (int.TryParse(_customAnimIdInput, out int customId))
                            {
                                if (FF9DBAll.AnimationDB.TryGetValue(customId, out string customAnimName))
                                {
                                    try
                                    {
                                        // NOUVEAU : Mise à jour de la variable
                                        _lastPlayedAnimInfo = customId.ToString();

                                        if (!_originalIdles.ContainsKey(actor)) _originalIdles[actor] = actor.idle;

                                        bool isLoaded = animComp[customAnimName] != null;
                                        if (!isLoaded) AnimationFactory.AddAnimWithAnimatioName(actor.go, customAnimName);

                                        actor.idle = (ushort)customId;
                                        actor.anim = (ushort)customId;

                                        animComp.Play(customAnimName);
                                        SoundLib.PlaySoundEffect(103);
                                    }
                                    catch (Exception ex)
                                    {
                                        Memoria.Prime.Log.Message($"[DebugMenu] Erreur lecture anim custom {customId} : {ex.Message}");
                                    }
                                }
                                else
                                {
                                    Memoria.Prime.Log.Message($"[DebugMenu] L'Anim ID {customId} n'existe pas dans le dictionnaire.");
                                }
                            }
                            GUI.FocusControl(null);
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.Space(5);

                        if (anims.Count > 0)
                        {
                            GUILayout.Label($"<b>Base de donnees : {anims.Count} animations trouvees</b>", new GUIStyle(GUI.skin.label) { richText = true });
                            _fieldScrollPos = GUILayout.BeginScrollView(_fieldScrollPos, GUILayout.Height(260));

                            int col = 2;
                            int c = 0;
                            GUILayout.BeginHorizontal();
                            foreach (KeyValuePair<Int32, String> animKvp in anims)
                            {
                                int animId = animKvp.Key;
                                string animName = animKvp.Value;

                                bool isLoaded = animComp[animName] != null;
                                string btnText = isLoaded ? $"<color=#AADDFF>{animName}</color>" : animName;

                                if (GUILayout.Button(btnText, new GUIStyle(GUI.skin.button) { richText = true }, GUILayout.Width(225)))
                                {
                                    try
                                    {
                                        _lastPlayedAnimInfo = animId.ToString();

                                        if (!_originalIdles.ContainsKey(actor))
                                        {
                                            _originalIdles[actor] = actor.idle;
                                        }

                                        if (!isLoaded) AnimationFactory.AddAnimWithAnimatioName(actor.go, animName);

                                        actor.idle = (ushort)animId;
                                        actor.anim = (ushort)animId;

                                        animComp.Play(animName);
                                        SoundLib.PlaySoundEffect(103);
                                    }
                                    catch (Exception ex)
                                    {
                                        Memoria.Prime.Log.Message($"[DebugMenu] Error reading anim {animName} : {ex.Message}");
                                    }
                                }

                                c++;
                                if (c % col == 0)
                                {
                                    GUILayout.EndHorizontal();
                                    GUILayout.BeginHorizontal();
                                }
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.EndScrollView();
                        }
                        else
                        {
                            GUILayout.Label("Aucune animation trouvee.");
                        }
                    }
                }
                else
                {
                    GUILayout.Label("L'acteur est invalide ou a ete detruit.");
                }
            }

            void DrawTriggerBattleMenu(int windowID)
            {
                GUILayout.BeginVertical("box");
                GUILayout.Label("<b>Lancer un combat</b>", new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter });
                GUILayout.Space(10);

                GUILayout.BeginHorizontal();
                GUILayout.Label("Battle ID:", GUILayout.Width(100));
                _battleIdStr = GUILayout.TextField(_battleIdStr, GUILayout.Width(100));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Group ID:", GUILayout.Width(100));
                _battleGroupStr = GUILayout.TextField(_battleGroupStr, GUILayout.Width(100));
                GUILayout.EndHorizontal();

                GUILayout.Space(15);

                if (GUILayout.Button("Lancer le combat", GUILayout.Height(40)))
                {
                    if (int.TryParse(_battleIdStr, out int battleId) && battleId >= 0 &&
                        sbyte.TryParse(_battleGroupStr, out sbyte groupId) && groupId >= -1)
                    {
                        if (!FF9BattleDB.SceneData.Any(kvp => kvp.Value == battleId))
                        {
                            Memoria.Prime.Log.Warning($"[DebugMenu] Impossible de lancer le combat : l'ID {battleId} n'existe pas dans SceneData !");
                            SoundLib.PlaySoundEffect(102);
                            return;
                        }

                        _showMenu = false;
                        SoundLib.PlaySoundEffect(103);
                        PersistenSingleton<EventEngine>.Instance.TriggerDebugBattle(battleId, groupId);
                    }
                    else
                    {
                        SoundLib.PlaySoundEffect(102);
                    }
                }

                GUILayout.EndVertical();
                GUI.DragWindow();
            }

            private void RestoreAllFieldAnimations()
            {
                foreach (var kvp in _originalIdles)
                {
                    Actor actor = kvp.Key;
                    if (actor != null && actor.go != null)
                    {
                        actor.idle = kvp.Value;
                        actor.anim = kvp.Value;

                        if (FF9DBAll.AnimationDB.TryGetValue(kvp.Value, out string origAnimName))
                        {
                            Animation animComp = actor.go.GetComponentInChildren<Animation>();
                            if (animComp != null && animComp[origAnimName] != null)
                            {
                                animComp.Play(origAnimName);
                            }
                        }
                    }
                }
                _originalIdles.Clear();
            }

            private void RestoreTint()
            {
                if (_lastTintedFma != null && _lastTintedFma.actor != null && _lastTintedFma.actor.go != null)
                {
                    foreach (var kvp in _originalColors)
                    {
                        if (kvp.Key != null) kvp.Key.material.color = kvp.Value;
                    }
                }
                _originalColors.Clear();
                _lastTintedFma = null;
            }

            private int DrawStatUI(string k, string l, int v, int w = 75) { GUILayout.BeginHorizontal(); GUILayout.Label(l, GUILayout.Width(w)); if (GUILayout.Button("-", GUILayout.Width(20))) { v--; _statTextCache[k] = v.ToString(); GUI.FocusControl(null); } if (!_statTextCache.ContainsKey(k)) _statTextCache[k] = v.ToString(); GUI.SetNextControlName(k); _statTextCache[k] = GUILayout.TextField(_statTextCache[k], GUILayout.Width(45)); if (GUILayout.Button("+", GUILayout.Width(20))) { v++; _statTextCache[k] = v.ToString(); GUI.FocusControl(null); } GUILayout.EndHorizontal(); if (int.TryParse(_statTextCache[k], out int p)) return p; return v; }
            private string GetAbilityName(int id) { var f = typeof(TranceSeekBattleAbility).GetFields(BindingFlags.Public | BindingFlags.Static); foreach (var fi in f) if ((int)(BattleAbilityId)fi.GetValue(null) == id) return fi.Name; if (Enum.IsDefined(typeof(BattleAbilityId), id)) return Enum.GetName(typeof(BattleAbilityId), id); return "Unknown"; }
            private string GetItemName(int id)
            {
                try
                {
                    var fields = typeof(TranceSeekRegularItem).GetFields(BindingFlags.Public | BindingFlags.Static);
                    foreach (var fi in fields)
                    {
                        if (fi.FieldType == typeof(RegularItem))
                        {
                            if ((int)(RegularItem)fi.GetValue(null) == id)
                                return fi.Name;
                        }
                    }

                    if (Enum.IsDefined(typeof(RegularItem), id))
                        return Enum.GetName(typeof(RegularItem), id);
                }
                catch (Exception)
                {
                }

                return "Unknown";
            }
            private string FormatDifficultyText(string n, int d) { if (FF9StateSystem.EventState?.gEventGlobal?[1403] == d) return $"<color=yellow><b>{n}</b></color>"; return n; }
            private string FormatCheatText(string n, int c) { if (MegaCheat == c) return $"<color=green><b>{n}</b></color>"; return n; }
            private void SetDifficulty(int g, int i) { try { for (int x = 82; x <= 89; x++) ff9item.FF9Item_RemoveImportant(x); ff9item.FF9Item_AddImportant(i); FF9StateSystem.EventState.gEventGlobal[1403] = (byte)g; FF9StateSystem.EventState.gEventGlobal[1407] = (byte)(g >= 4 && g <= 6 ? 1 : 0); SoundLib.PlaySoundEffect(108); } catch { } }
        }
    }
}
