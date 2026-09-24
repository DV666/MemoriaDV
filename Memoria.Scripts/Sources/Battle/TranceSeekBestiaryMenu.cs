using System;
using System.Collections.Generic;
using System.Reflection;
using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Assets;
using Memoria.Data;
using Memoria.Prime;
using UnityEngine;

namespace Memoria.Scripts.TranceSeek
{
    public class TranceSeekBestiaryMenu : MonoBehaviour
    {
        private const string ConfigGroupName = "Config.Config";
        private const string BestiaryGroupName = "TranceSeek.Bestiary";
        private const string BestiaryButtonName = "Bestiary Panel - Button";

        private const Boolean UnlockAll = false;

        private static readonly FieldInfo ConfigFieldListField = typeof(ConfigUI).GetField("ConfigFieldList", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo ConfigScrollViewField = typeof(ConfigUI).GetField("configScrollView", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly Type ScrollItemKeyNavType = typeof(ConfigUI).Assembly.GetType("ScrollItemKeyNavigation");
        private static readonly FieldInfo NavIdField = ScrollItemKeyNavType?.GetField("ID", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        private static readonly Type SnapDragScrollViewType = typeof(ConfigUI).Assembly.GetType("SnapDragScrollView");
        private static readonly PropertyInfo MaxItemProp = SnapDragScrollViewType?.GetProperty("MaxItem", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly FieldInfo MaxItemField = SnapDragScrollViewType?.GetField("MaxItem", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        private int _infoWindowWidth = 680;
        private int _infoWindowHeight = 700;
        private float _infoWindowPosX = 360f;
        private float _infoWindowPosY = -25f;

        private int _modelWindowWidth = 550;
        private int _modelWindowHeight = 600;
        private float _modelWindowPosX = -340f;
        private float _modelWindowPosY = 25f;

        private float _buttonPosX = -340f;
        private float _buttonPosY = -340f;

        private float _modelPosX = -6f;
        private float _modelPosY = -250f;
        private float _modelPosZ = 500f;
        private float _modelScale = 0.5f;
        private float _modelRotX = -5f;
        private float _modelRotY = 340f;
        private float _modelRotZ = 180f;

        private GameObject _bbgModel;
        private string _currentBbgName = "";

        private float _bbgPosX = -6f;
        private float _bbgPosY = -250f;
        private float _bbgPosZ = 600f;
        private float _bbgScale = 0.5f;
        private float _bbgRotX = -185f;
        private float _bbgRotY = 340f;
        private float _bbgRotZ = 180f;

        private GameObject _bestiaryEntryButton;
        private UILabel _configTitleLabel;
        private string _originalTitleText = "Config.";

        private GameObject _bestiaryMenuRoot;
        private GameObject _bestiaryReturnButton;
        private bool _isBestiaryOpen = false;

        private UILabel _monsterInfoLabel;
        private GameObject _monsterModel;
        private RenderTexture _monsterRt;
        private GameObject _monsterCamObj;

        private int _currentPage = 0;
        private int _currentMonsterId = 1;
        private BestiaryDisplayEntry _currentDisplayEntry;
        private SB2_MON_PARM _currentMonsterParam;
        private string _currentMonsterName = "???";

        private List<string> _statusChunks = new List<string>();
        private int _statusChunkIndex = 0;
        private float _statusTimer = 0f;

        private void Update()
        {
            if (PersistenSingleton<UIManager>.Instance == null)
                return;

            if (PersistenSingleton<UIManager>.Instance.State != UIManager.UIState.Config)
            {
                if (_isBestiaryOpen)
                    ForceCloseBestiary();
                return;
            }

            ConfigUI configScene = PersistenSingleton<UIManager>.Instance.ConfigScene;
            if (configScene == null || configScene.ConfigList == null)
                return;

            if (_bestiaryEntryButton == null)
            {
                TryInjectBestiaryButton(configScene);
            }

            if (_isBestiaryOpen)
            {
                UpdateBestiaryMenuInput(configScene);
            }
            else
            {
                UpdateConfigMenuInput(configScene);
            }
        }

        private void TryInjectBestiaryButton(ConfigUI configUI)
        {
            Transform listParent = configUI.ConfigList.transform.GetChild(1).GetChild(0);
            if (listParent == null)
                return;

            Transform existing = listParent.Find(BestiaryButtonName);
            if (existing != null)
            {
                _bestiaryEntryButton = existing.gameObject;
                return;
            }

            if (ConfigFieldListField == null)
                return;

            List<ConfigField> fieldList = ConfigFieldListField.GetValue(configUI) as List<ConfigField>;
            if (fieldList == null || fieldList.Count == 0)
                return;

            int tutorialIndex = fieldList.FindIndex(f => f.Configurator == ConfigUI.Configurator.ControlTutorial);
            if (tutorialIndex == -1)
                return;

            ConfigField tutorialField = fieldList[tutorialIndex];
            GameObject templateGo = tutorialField.ConfigParent;
            if (templateGo == null)
                return;

            _bestiaryEntryButton = Instantiate(templateGo);
            _bestiaryEntryButton.name = BestiaryButtonName;
            _bestiaryEntryButton.transform.SetParent(listParent, false);

            int targetSiblingIndex = templateGo.transform.GetSiblingIndex();
            _bestiaryEntryButton.transform.SetSiblingIndex(targetSiblingIndex);

            UILabel label = _bestiaryEntryButton.GetComponentInChildren<UILabel>();
            if (label != null)
            {
                UILocalize localize = label.GetComponent<UILocalize>();
                if (localize != null)
                    Destroy(localize);

                label.rawText = "Bestiaire";
            }

            CreateDummyChoices(_bestiaryEntryButton);

            ConfigField bestiaryField = new ConfigField
            {
                ConfigParent = _bestiaryEntryButton,
                Button = _bestiaryEntryButton.GetComponent<ButtonGroupState>(),
                Configurator = (ConfigUI.Configurator)99,
                IsSlider = false,
                Value = 0f
            };

            bestiaryField.ConfigChoice.Add(_bestiaryEntryButton.transform.Find("DummyChoice1").gameObject);
            bestiaryField.ConfigChoice.Add(_bestiaryEntryButton.transform.Find("DummyChoice2").gameObject);

            if (bestiaryField.Button != null && bestiaryField.Button.Help != null)
                bestiaryField.Button.Help.TextKey = string.Empty;

            fieldList.Insert(tutorialIndex, bestiaryField);

            if (ScrollItemKeyNavType != null && NavIdField != null)
            {
                for (int i = 0; i < fieldList.Count; i++)
                {
                    Component nav = fieldList[i].ConfigParent.GetComponent(ScrollItemKeyNavType);
                    if (nav != null)
                        NavIdField.SetValue(nav, i);
                }
            }

            if (ConfigScrollViewField != null)
            {
                object scrollView = ConfigScrollViewField.GetValue(configUI);
                if (scrollView != null)
                {
                    if (MaxItemProp != null)
                        MaxItemProp.SetValue(scrollView, fieldList.Count, null);
                    else if (MaxItemField != null)
                        MaxItemField.SetValue(scrollView, fieldList.Count);
                }
            }

            RepositionList(listParent, templateGo, targetSiblingIndex);
            RebuildNavigationChain(fieldList);

            UIEventListener.Get(_bestiaryEntryButton).onClick = go => OpenBestiaryMenu(configUI);

            FindConfigTitleLabel(configUI);
        }

        private void RepositionList(Transform listParent, GameObject templateGo, int targetSiblingIndex)
        {
            UIGrid grid = listParent.GetComponent<UIGrid>();
            if (grid != null)
            {
                grid.sorting = UIGrid.Sorting.None;
                grid.repositionNow = true;
                grid.Reposition();
                return;
            }

            UITable table = listParent.GetComponent<UITable>();
            if (table != null)
            {
                table.repositionNow = true;
                table.Reposition();
                return;
            }

            float rowHeight = 44f;
            UIWidget widget = templateGo.GetComponent<UIWidget>();
            if (widget != null && widget.height > 0)
                rowHeight = widget.height;

            for (int i = targetSiblingIndex + 1; i < listParent.childCount; i++)
            {
                Transform child = listParent.GetChild(i);
                Vector3 pos = child.localPosition;
                child.localPosition = new Vector3(pos.x, pos.y - rowHeight, pos.z);
            }
        }

        private void CreateDummyChoices(GameObject parent)
        {
            for (int i = 1; i <= 2; i++)
            {
                GameObject dummy = new GameObject("DummyChoice" + i);
                dummy.transform.SetParent(parent.transform, false);
                dummy.SetActive(false);

                GameObject dummyChild = new GameObject("Label");
                dummyChild.transform.SetParent(dummy.transform, false);
                dummyChild.AddComponent<UILabel>();
            }
        }

        private void RebuildNavigationChain(List<ConfigField> fieldList)
        {
            for (int i = 0; i < fieldList.Count; i++)
            {
                UIKeyNavigation nav = fieldList[i].ConfigParent.GetComponent<UIKeyNavigation>();
                if (nav == null)
                    continue;

                nav.onUp = (i > 0) ? fieldList[i - 1].ConfigParent : null;

                if (i < fieldList.Count - 1)
                    nav.onDown = fieldList[i + 1].ConfigParent;
            }
        }

        private void FindConfigTitleLabel(ConfigUI configUI)
        {
            foreach (UILabel lbl in configUI.GetComponentsInChildren<UILabel>(true))
            {
                if (lbl.rawText != null && (lbl.rawText.Trim() == "Config." || lbl.rawText.Trim() == "Config"))
                {
                    _configTitleLabel = lbl;
                    _originalTitleText = lbl.rawText;
                    break;
                }
            }
        }

        private void CreateBestiaryMenuHierarchy(ConfigUI configUI)
        {
            if (_bestiaryMenuRoot != null)
                return;

            _bestiaryMenuRoot = new GameObject("TranceSeek_BestiaryMenuRoot");
            _bestiaryMenuRoot.transform.SetParent(configUI.transform, false);
            _bestiaryMenuRoot.transform.localPosition = Vector3.zero;
            _bestiaryMenuRoot.transform.localScale = Vector3.one;

            GameObject frameTemplate = configUI.ConfigList.transform.GetChild(2).gameObject;
            GameObject bestiaryFrame = Instantiate(frameTemplate);
            bestiaryFrame.name = "Bestiary Window Frame";
            bestiaryFrame.transform.SetParent(_bestiaryMenuRoot.transform, false);
            bestiaryFrame.transform.localPosition = frameTemplate.transform.localPosition;
            bestiaryFrame.transform.localScale = Vector3.one;

            foreach (UILabel lbl in bestiaryFrame.GetComponentsInChildren<UILabel>(true))
            {
                if (lbl.rawText != null && lbl.rawText.Contains("CONFIG"))
                {
                    UILocalize loc = lbl.GetComponent<UILocalize>();
                    if (loc != null) Destroy(loc);
                    lbl.rawText = "BESTIAIRE";
                }
            }

            _bestiaryReturnButton = Instantiate(_bestiaryEntryButton);
            _bestiaryReturnButton.name = "Bestiary Return Button";
            _bestiaryReturnButton.transform.SetParent(_bestiaryMenuRoot.transform, false);
            _bestiaryReturnButton.transform.localPosition = new Vector3(_buttonPosX, _buttonPosY, 0f);

            Transform d1 = _bestiaryReturnButton.transform.Find("DummyChoice1");
            if (d1 != null) Destroy(d1.gameObject);
            Transform d2 = _bestiaryReturnButton.transform.Find("DummyChoice2");
            if (d2 != null) Destroy(d2.gameObject);

            UIWidget buttonWidget = _bestiaryReturnButton.GetComponent<UIWidget>();
            if (buttonWidget != null)
            {
                buttonWidget.width = 400;
            }

            UILabel returnLabel = _bestiaryReturnButton.GetComponentInChildren<UILabel>();
            if (returnLabel != null)
            {
                UILocalize loc = returnLabel.GetComponent<UILocalize>();
                if (loc != null) Destroy(loc);
                returnLabel.rawText = Localization.GetWithDefault("BestiaryBack");
                returnLabel.alignment = NGUIText.Alignment.Center;
            }

            ButtonGroupState bgs = _bestiaryReturnButton.GetComponent<ButtonGroupState>();
            if (bgs != null)
            {
                bgs.GroupName = BestiaryGroupName;
                if (bgs.Help != null) bgs.Help.TextKey = string.Empty;
            }

            UIEventListener.Get(_bestiaryReturnButton).onClick = go => CloseBestiaryMenu(configUI);

            GameObject infoFrame = Instantiate(frameTemplate);
            infoFrame.name = "Bestiary Info Frame";
            infoFrame.transform.SetParent(_bestiaryMenuRoot.transform, false);
            infoFrame.transform.localPosition = new Vector3(_infoWindowPosX, _infoWindowPosY, 0f);

            UIWidget rootWidgetInfo = infoFrame.GetComponent<UIWidget>();
            if (rootWidgetInfo != null)
            {
                rootWidgetInfo.leftAnchor.target = null;
                rootWidgetInfo.rightAnchor.target = null;
                rootWidgetInfo.topAnchor.target = null;
                rootWidgetInfo.bottomAnchor.target = null;
                rootWidgetInfo.width = _infoWindowWidth;
                rootWidgetInfo.height = _infoWindowHeight;
            }

            UIWidget[] infoWidgets = infoFrame.GetComponentsInChildren<UIWidget>(true);
            foreach (UIWidget widget in infoWidgets)
            {
                widget.depth += 10;
            }

            foreach (UILabel lbl in infoFrame.GetComponentsInChildren<UILabel>(true))
            {
                if (lbl.rawText != null && lbl.rawText.Contains("CONFIG"))
                {
                    UILocalize loc = lbl.GetComponent<UILocalize>();
                    if (loc != null) Destroy(loc);
                    lbl.rawText = Localization.GetWithDefault("BestiaryInfo");
                }
            }

            UILabel templateLabel = _bestiaryEntryButton.GetComponentInChildren<UILabel>();
            GameObject infoTextObj = Instantiate(templateLabel.gameObject);
            infoTextObj.name = "Monster Info Text";
            infoTextObj.transform.SetParent(infoFrame.transform, false);

            _monsterInfoLabel = infoTextObj.GetComponent<UILabel>();
            UILocalize infoLoc = _monsterInfoLabel.GetComponent<UILocalize>();
            if (infoLoc != null) Destroy(infoLoc);

            _monsterInfoLabel.leftAnchor.target = null;
            _monsterInfoLabel.rightAnchor.target = null;
            _monsterInfoLabel.topAnchor.target = null;
            _monsterInfoLabel.bottomAnchor.target = null;

            _monsterInfoLabel.pivot = UIWidget.Pivot.TopLeft;
            _monsterInfoLabel.alignment = NGUIText.Alignment.Left;
            _monsterInfoLabel.width = _infoWindowWidth - 60;
            _monsterInfoLabel.height = _infoWindowHeight - 80;
            _monsterInfoLabel.depth += 15;

            float textPosX = -(_infoWindowWidth / 2f) + 30f;
            float textPosY = (_infoWindowHeight / 2f) - 40f;
            infoTextObj.transform.localPosition = new Vector3(textPosX, textPosY, 0f);

            GameObject modelFrame = Instantiate(frameTemplate);
            modelFrame.name = "Bestiary Model Frame";
            modelFrame.transform.SetParent(_bestiaryMenuRoot.transform, false);
            modelFrame.transform.localPosition = new Vector3(_modelWindowPosX, _modelWindowPosY, 0f);

            UIWidget rootWidgetModel = modelFrame.GetComponent<UIWidget>();
            if (rootWidgetModel != null)
            {
                rootWidgetModel.leftAnchor.target = null;
                rootWidgetModel.rightAnchor.target = null;
                rootWidgetModel.topAnchor.target = null;
                rootWidgetModel.bottomAnchor.target = null;
                rootWidgetModel.width = _modelWindowWidth;
                rootWidgetModel.height = _modelWindowHeight;
            }

            UIWidget[] modelWidgets = modelFrame.GetComponentsInChildren<UIWidget>(true);
            foreach (UIWidget widget in modelWidgets)
            {
                widget.depth += 10;
            }

            foreach (UILabel lbl in modelFrame.GetComponentsInChildren<UILabel>(true))
            {
                if (lbl.rawText != null && lbl.rawText.Contains("CONFIG"))
                {
                    UILocalize loc = lbl.GetComponent<UILocalize>();
                    if (loc != null) Destroy(loc);
                    lbl.rawText = Localization.GetWithDefault("BestiaryPreview");
                }
            }

            foreach (UISprite s in modelFrame.GetComponentsInChildren<UISprite>(true))
            {
                if (s.name != null && !s.name.ToLower().Contains("border"))
                {
                    Destroy(s.gameObject);
                }
            }

            _monsterRt = new RenderTexture(_modelWindowWidth, _modelWindowHeight, 24);
            _monsterRt.antiAliasing = 2;

            _monsterCamObj = new GameObject("MonsterRTCam");
            UnityEngine.Object.DontDestroyOnLoad(_monsterCamObj);
            _monsterCamObj.transform.position = new Vector3(20000f, 20000f, -1000f);
            Camera cam = _monsterCamObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = _modelWindowHeight / 2f;
            cam.targetTexture = _monsterRt;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.05f, 1f);
            cam.cullingMask = 1 << 31;
            cam.nearClipPlane = -5000f;
            cam.farClipPlane = 50000f;

            GameObject texObj = new GameObject("MonsterTexture");
            texObj.transform.SetParent(modelFrame.transform, false);
            texObj.transform.localPosition = Vector3.zero;
            UITexture uiTex = texObj.AddComponent<UITexture>();
            uiTex.mainTexture = _monsterRt;
            uiTex.width = _modelWindowWidth;
            uiTex.height = _modelWindowHeight;
            uiTex.depth = 12;

            _bestiaryMenuRoot.SetActive(false);
            if (_monsterCamObj != null)
                _monsterCamObj.SetActive(false);
        }

        private void LoadMonsterData()
        {
            if (!TranceSeekBestiaryDB.TryGetDisplayEntry(_currentMonsterId, out _currentDisplayEntry))
                return;

            BTL_SCENE scene = new BTL_SCENE();
            string battleSceneName = "";
            FF9BattleDB.SceneData.TryGetKey(_currentDisplayEntry.BattleId, out battleSceneName);
            battleSceneName = battleSceneName.Substring(4);
            scene.ReadBattleScene(battleSceneName);
            _currentMonsterParam = scene.MonAddr[_currentDisplayEntry.MonsterIndex];
            _currentBbgName = string.IsNullOrEmpty(scene.Info.BattleBackground) ? FF9BattleDB.MapModel["BSC_" + battleSceneName] : scene.Info.BattleBackground;

            try
            {
                int textId = FF9BattleDB.SceneData["BSC_" + battleSceneName];
                string[] texts = FF9TextTool.GetBattleText(textId);

                if (texts != null && _currentDisplayEntry.MonsterIndex < texts.Length)
                {
                    _currentMonsterName = texts[_currentDisplayEntry.MonsterIndex];
                }
                else
                {
                    _currentMonsterName = $"Monstre ID N°{_currentMonsterId}";
                }
            }
            catch (Exception ex)
            {
                _currentMonsterName = $"Monstre ID N°{_currentMonsterId}";
            }
        }

        private void UpdateMonsterModel()
        {
            if (_currentMonsterParam == null)
                return;

            if (_monsterModel != null)
            {
                Destroy(_monsterModel);
                _monsterModel = null;
            }

            if (_bbgModel != null)
            {
                Destroy(_bbgModel);
                _bbgModel = null;
            }

            if (!string.IsNullOrEmpty(_currentBbgName))
            {
                _bbgModel = ModelFactory.CreateModel($"BattleMap/BattleModel/battleMap_all/{_currentBbgName}/{_currentBbgName}", false, true, Configuration.Graphics.BattleSmoothTexture);
                if (_bbgModel != null)
                {
                    _bbgModel.SetActive(true);
                    _bbgModel.transform.SetParent(_monsterCamObj.transform, false);
                    NGUITools.SetLayer(_bbgModel, 31);
                    Int32.TryParse(_currentBbgName.Replace("BBG_B", ""), out battlebg.nf_BbgNumber);
                    battlebg.SetDefaultShader(_bbgModel);
                    if (String.Equals(_currentBbgName, "BBG_B171_OBJ2")) // Crystal World, Crystal
                        battlebg.SetMaterialShader(_bbgModel, "PSX/BattleMap_Cystal");

                    foreach (Renderer r in _bbgModel.GetComponentsInChildren<Renderer>(true))
                    {
                        r.enabled = true;
                        if (r is SkinnedMeshRenderer smr)
                        {
                            smr.updateWhenOffscreen = true;
                            smr.localBounds = new Bounds(Vector3.zero, new Vector3(10000f, 10000f, 10000f));
                        }
                    }

                    _bbgModel.transform.localPosition = new Vector3(_bbgPosX, _bbgPosY, _bbgPosZ);
                    _bbgModel.transform.localRotation = Quaternion.Euler(_bbgRotX, _bbgRotY, _bbgRotZ);
                    _bbgModel.transform.localScale = new Vector3(_bbgScale, _bbgScale, _bbgScale);
                }
            }

            Boolean BlackModel = false;
            int status = TranceSeekBestiaryDB.GetMonsterStatus(_currentMonsterId);
            if (status == TranceSeekBestiaryDB.StatusUndiscovered && !UnlockAll)
                BlackModel = true;

            string geoPath = FF9BattleDB.GEO.GetValue(_currentMonsterParam.Geo);
            if (string.IsNullOrEmpty(geoPath))
                return;

            _monsterModel = ModelFactory.CreateModel(geoPath, false);
            if (_monsterModel != null)
            {
                _monsterModel.SetActive(true);
                _monsterModel.transform.SetParent(_monsterCamObj.transform, false);

                NGUITools.SetLayer(_monsterModel, 31);

                btl_util.GeoSetABR(_monsterModel, "PSX/BattleMap_StatusEffect");

                if (BlackModel)
                    btl_util.GeoSetColor2DrawPacket(_monsterModel, 0, 0, 0, Byte.MaxValue);
                else
                    btl_util.GeoSetColor2DrawPacket(_monsterModel, 127, 127, 127, Byte.MaxValue);

                foreach (Renderer r in _monsterModel.GetComponentsInChildren<Renderer>(true))
                {
                    r.enabled = true;
                    if (r is SkinnedMeshRenderer smr)
                    {
                        smr.updateWhenOffscreen = true;
                        smr.localBounds = new Bounds(Vector3.zero, new Vector3(10000f, 10000f, 10000f));
                    }
                }

                _monsterModel.transform.localPosition = new Vector3(_modelPosX, _modelPosY, _modelPosZ);
                _monsterModel.transform.localRotation = Quaternion.Euler(_modelRotX, _modelRotY, _modelRotZ);
                _monsterModel.transform.localScale = new Vector3(_modelScale, _modelScale, _modelScale);

                if (_currentMonsterParam.Mot != null && _currentMonsterParam.Mot.Length > 0)
                {
                    string animName = FF9BattleDB.Animation[_currentMonsterParam.Mot[0]];
                    AnimationFactory.AddAnimWithAnimatioName(_monsterModel, animName);
                    Animation anim = _monsterModel.GetComponent<Animation>();
                    if (anim != null)
                    {
                        anim.wrapMode = WrapMode.Loop;
                        anim.Play(animName);
                    }
                }
            }
        }

        private string GetElementsString(byte flags)
        {
            if (flags == 0)
                return Localization.GetWithDefault("BestiaryNone");

            List<string> elems = new List<string>();
            for (int i = 0; i < 8; i++)
                if ((flags & (1 << i)) != 0)
                    elems.Add(UIManager.Battle.BtlGetAttrName(1 << i));

            return string.Join(", ", elems.ToArray());
        }

        private string GetCategoryString(byte category)
        {
            if (category == 0)
                return Localization.GetWithDefault("BestiaryNone");

            List<string> cats = new List<string>();
            for (int i = 0; i < 8; i++)
            {
                if ((category & (1 << i)) != 0)
                {
                    cats.Add(FF9TextTool.BattleLibraText(i));
                }
            }

            return string.Join(", ", cats.ToArray());
        }

        private List<string> GetStatusList(BattleStatus status)
        {
            List<string> statuses = new List<string>();

            if ((status & BattleStatus.Petrify) != 0) statuses.Add(Localization.GetWithDefault("BestiaryPetrify"));
            if ((status & BattleStatus.Venom) != 0) statuses.Add(Localization.GetWithDefault("BestiaryVenom"));
            if ((status & BattleStatus.Virus) != 0) statuses.Add(Localization.GetWithDefault("BestiaryVirus"));
            if ((status & BattleStatus.Silence) != 0) statuses.Add(Localization.GetWithDefault("BestiarySilence"));
            if ((status & BattleStatus.Blind) != 0) statuses.Add(Localization.GetWithDefault("BestiaryDarkness"));
            if ((status & BattleStatus.Trouble) != 0) statuses.Add(Localization.GetWithDefault("BestiaryTrouble"));
            if ((status & BattleStatus.Zombie) != 0) statuses.Add(Localization.GetWithDefault("BestiaryZombie"));
            if ((status & BattleStatus.Death) != 0) statuses.Add(Localization.GetWithDefault("BestiaryDeath"));
            if ((status & BattleStatus.Confuse) != 0) statuses.Add(Localization.GetWithDefault("BestiaryConfuse"));
            if ((status & BattleStatus.Berserk) != 0) statuses.Add(Localization.GetWithDefault("BestiaryBerserk"));
            if ((status & BattleStatus.Stop) != 0) statuses.Add(Localization.GetWithDefault("BestiaryStop"));
            if ((status & BattleStatus.Poison) != 0) statuses.Add(Localization.GetWithDefault("BestiaryPoison"));
            if ((status & BattleStatus.Sleep) != 0) statuses.Add(Localization.GetWithDefault("BestiarySleep"));
            if ((status & BattleStatus.Regen) != 0) statuses.Add(Localization.GetWithDefault("BestiaryRegen"));
            if ((status & BattleStatus.Haste) != 0) statuses.Add(Localization.GetWithDefault("BestiaryHaste"));
            if ((status & BattleStatus.Slow) != 0) statuses.Add(Localization.GetWithDefault("BestiarySlow"));
            if ((status & BattleStatus.Float) != 0) statuses.Add(Localization.GetWithDefault("BestiaryFloat"));
            if ((status & BattleStatus.Shell) != 0) statuses.Add(Localization.GetWithDefault("BestiaryShell"));
            if ((status & BattleStatus.Protect) != 0) statuses.Add(Localization.GetWithDefault("BestiaryProtect"));
            if ((status & BattleStatus.Heat) != 0) statuses.Add(Localization.GetWithDefault("BestiaryHeat"));
            if ((status & BattleStatus.Freeze) != 0) statuses.Add(Localization.GetWithDefault("BestiaryFreeze"));
            if ((status & BattleStatus.Vanish) != 0) statuses.Add(Localization.GetWithDefault("BestiaryVanish"));
            if ((status & BattleStatus.Doom) != 0) statuses.Add(Localization.GetWithDefault("BestiaryDoom"));
            if ((status & BattleStatus.Mini) != 0) statuses.Add(Localization.GetWithDefault("BestiaryMini"));
            if ((status & BattleStatus.Reflect) != 0) statuses.Add(Localization.GetWithDefault("BestiaryReflect"));
            if ((status & BattleStatus.GradualPetrify) != 0) statuses.Add(Localization.GetWithDefault("BestiaryGradualPetrify"));
            if ((status & TranceSeekStatus.PowerBreak) != 0) statuses.Add(Localization.GetWithDefault("BestiaryStrengthDown"));
            if ((status & TranceSeekStatus.MagicBreak) != 0) statuses.Add(Localization.GetWithDefault("BestiaryMagicDown"));
            if ((status & TranceSeekStatus.ArmorBreak) != 0) statuses.Add(Localization.GetWithDefault("BestiaryPDefenseDown"));
            if ((status & TranceSeekStatus.MentalBreak) != 0) statuses.Add(Localization.GetWithDefault("BestiaryMDefenseDown"));
            if ((status & TranceSeekStatus.PowerUp) != 0) statuses.Add(Localization.GetWithDefault("BestiaryStrengthUp"));
            if ((status & TranceSeekStatus.MagicUp) != 0) statuses.Add(Localization.GetWithDefault("BestiaryMagicUp"));
            if ((status & TranceSeekStatus.ArmorUp) != 0) statuses.Add(Localization.GetWithDefault("BestiaryDefenseUp"));
            if ((status & TranceSeekStatus.MentalUp) != 0) statuses.Add(Localization.GetWithDefault("BestiaryMDefenseUp"));
            if ((status & TranceSeekStatus.Dragon) != 0) statuses.Add(Localization.GetWithDefault("BestiaryDragon"));
            if ((status & TranceSeekStatus.PerfectCrit) != 0) statuses.Add(Localization.GetWithDefault("BestiaryPerfectCrit"));
            if ((status & TranceSeekStatus.PerfectDodge) != 0) statuses.Add(Localization.GetWithDefault("BestiaryPerfectDodge"));
            if ((status & TranceSeekStatus.Old) != 0) statuses.Add(Localization.GetWithDefault("BestiaryOld"));
            if ((status & TranceSeekStatus.Provok) != 0) statuses.Add(Localization.GetWithDefault("BestiaryProvok"));


            return statuses;
        }

        private string GetItemStringWithIcon(RegularItem itemId)
        {
            if (itemId == RegularItem.NoItem) return "---";
            string itemName = FF9TextTool.ItemName(itemId);

            try
            {
                FF9ITEM_DATA itemData = ff9item._FF9Item_Data[itemId];
                string spriteName = $"item{itemData.shape:0#}_{itemData.color:0#}";
                return $"[SPRT={spriteName},48,48] {itemName}";
            }
            catch
            {
                return itemName;
            }
        }

        private string GetCardName(TetraMasterCardId card)
        {
            if (card == TetraMasterCardId.NONE) return "---";
            return FF9TextTool.CardName(card);
        }

        private void InitializeMonsterInfo()
        {
            if (_currentMonsterParam == null)
                return;

            int status = TranceSeekBestiaryDB.GetMonsterStatus(_currentMonsterId);

            _statusTimer = 0.5f;
            _statusChunkIndex = 0;
            _statusChunks.Clear();

            if (status < TranceSeekBestiaryDB.StatusScannedPlus)
            {
                _statusChunks.Add("???");
            }
            else
            {
                List<string> statuses = GetStatusList(_currentMonsterParam.ResistStatus);
                if (statuses.Count == 0)
                {
                    _statusChunks.Add("Aucune");
                }
                else
                {
                    for (int i = 0; i < statuses.Count; i += 5)
                    {
                        int count = Mathf.Min(5, statuses.Count - i);
                        _statusChunks.Add(string.Join(", ", statuses.GetRange(i, count).ToArray()));
                    }
                }
            }

            RefreshPageText();
        }

        private void RefreshPageText()
        {
            if (_monsterInfoLabel == null || _currentMonsterParam == null)
                return;

            int status = TranceSeekBestiaryDB.GetMonsterStatus(_currentMonsterId);
            int kills = TranceSeekBestiaryDB.GetMonsterKills(_currentMonsterId);

            if (UnlockAll || kills >= TranceSeekBestiaryDB.RequiredKillsForMastery)
                status = TranceSeekBestiaryDB.StatusMastered;

            string text = "";
            string pageNav = $"\n\n[A0A0A0]< Page {_currentPage + 1}/3 >[FFFFFF]";

            string monsterName = status >= TranceSeekBestiaryDB.StatusDiscovered ? _currentMonsterName : "???";
            if (status >= TranceSeekBestiaryDB.StatusMastered)
                monsterName += " [SPRT=IconAtlas,item200_03,36,36]";

            int maxHp = (int)_currentMonsterParam.MaxHP;
            if ((_currentMonsterParam.Flags & 64) != 0)
                maxHp = Mathf.Max(0, maxHp - 10000);

            string TrueText = Localization.GetWithDefault("BestiaryTrue");
            string FalseText = Localization.GetWithDefault("BestiaryFalse");
            string sElite = ((_currentMonsterParam.Flags & 128) != 0) ? TrueText : FalseText;
            string sBoss = ((_currentMonsterParam.ResistStatus & BattleStatus.EasyKill) != 0) ? TrueText : FalseText;

            if (_currentDisplayEntry.MaxHP.HasValue)
                maxHp = _currentDisplayEntry.MaxHP.Value;

            int maxMp = _currentDisplayEntry.MaxMP ?? (int)_currentMonsterParam.MaxMP;
            int speed = _currentDisplayEntry.Speed ?? _currentMonsterParam.Element.Speed;
            int strength = _currentDisplayEntry.Strength ?? _currentMonsterParam.Element.Strength;
            int magic = _currentDisplayEntry.Magic ?? _currentMonsterParam.Element.Magic;
            int spirit = _currentDisplayEntry.Spirit ?? _currentMonsterParam.Element.Spirit;
            int pDef = _currentDisplayEntry.PhysicalDefense ?? _currentMonsterParam.PhysicalDefence;
            int pEvade = _currentDisplayEntry.PhysicalEvade ?? _currentMonsterParam.PhysicalEvade;
            int mDef = _currentDisplayEntry.MagicalDefense ?? _currentMonsterParam.MagicalDefence;
            int mEvade = _currentDisplayEntry.MagicalEvade ?? _currentMonsterParam.MagicalEvade;
            int winGil = _currentDisplayEntry.WinGil ?? (int)_currentMonsterParam.WinGil;
            int winExp = _currentDisplayEntry.WinExp ?? (int)_currentMonsterParam.WinExp;

            string sHp = status >= TranceSeekBestiaryDB.StatusScanned ? maxHp.ToString() : "???";
            string sMp = status >= TranceSeekBestiaryDB.StatusScanned ? maxMp.ToString() : "???";
            string sSpeed = status >= TranceSeekBestiaryDB.StatusScanned ? speed.ToString() : "???";
            string sStr = status >= TranceSeekBestiaryDB.StatusScanned ? strength.ToString() : "???";
            string sMag = status >= TranceSeekBestiaryDB.StatusScanned ? magic.ToString() : "???";
            string sSpr = status >= TranceSeekBestiaryDB.StatusScanned ? spirit.ToString() : "???";
            string sPDef = status >= TranceSeekBestiaryDB.StatusScanned ? pDef.ToString() : "???";
            string sPEvade = status >= TranceSeekBestiaryDB.StatusScanned ? pEvade.ToString() : "???";
            string sMDef = status >= TranceSeekBestiaryDB.StatusScanned ? mDef.ToString() : "???";
            string sMEvade = status >= TranceSeekBestiaryDB.StatusScanned ? mEvade.ToString() : "???";

            string sGil = status >= TranceSeekBestiaryDB.StatusDiscovered ? winGil.ToString() : "???";
            string sExp = status >= TranceSeekBestiaryDB.StatusDiscovered ? winExp.ToString() : "???";
            string sCategory = status >= TranceSeekBestiaryDB.StatusDiscovered ? GetCategoryString(_currentMonsterParam.Category) : "???";

            if (_currentPage == 0)
            {
                text = $"[FFCC00]{Localization.GetWithDefault("BestiaryEntry")} n°{_currentMonsterId}\n" +
                       $"[FFCC00]{Localization.GetWithDefault("BestiaryName")} :[FFFFFF] {monsterName}\n\n" +
                       $"[FFCC00]{Localization.GetWithDefault("HPCaption")} :[FFFFFF] {sHp}               [FFCC00]{Localization.GetWithDefault("MPCaption")} :[FFFFFF] {sMp}\n" +
                       $"[FFCC00]{Localization.GetWithDefault("Speed")} :[FFFFFF] {sSpeed}          [FFCC00]{Localization.GetWithDefault("Strength")} :[FFFFFF] {sStr}\n" +
                       $"[FFCC00]{Localization.GetWithDefault("Magic")} :[FFFFFF] {sMag}            [FFCC00]{Localization.GetWithDefault("Spirit")} :[FFFFFF] {sSpr}\n" +
                       $"[FFCC00]{Localization.GetWithDefault("DefenseStats")} :[FFFFFF] {sPDef}          [FFCC00]{Localization.GetWithDefault("Evade")} :[FFFFFF] {sPEvade}\n" +
                       $"[FFCC00]{Localization.GetWithDefault("MagicDef")} :[FFFFFF] {sMDef}        [FFCC00]{Localization.GetWithDefault("MagicEva")} :[FFFFFF] {sMEvade}\n\n" +
                       $"[FFCC00]{Localization.GetWithDefault("BattleResult04")} :[FFFFFF] {sGil}\n" +
                       $"[FFCC00]{Localization.GetWithDefault("EXP")} :[FFFFFF] {sExp}\n\n" +
                       $"[FFCC00]{Localization.GetWithDefault("BestiaryElite")} :[FFFFFF] {sElite}\n" +
                       $"[FFCC00]{Localization.GetWithDefault("BestiaryBoss")} :[FFFFFF] {sBoss}\n" + pageNav;
            }
            else if (_currentPage == 1)
            {
                string alphaTag = "";
                if (_statusChunks.Count > 1)
                {
                    float alpha = 1f;
                    if (_statusTimer < 0.5f) alpha = _statusTimer / 0.5f;
                    else if (_statusTimer > 2.0f) alpha = (2.5f - _statusTimer) / 0.5f;
                    alphaTag = "[" + NGUIText.EncodeAlpha(alpha) + "]";
                }

                string sAbsorb = status >= TranceSeekBestiaryDB.StatusScannedPlus ? GetElementsString(_currentMonsterParam.AbsorbElement) : "???";
                string sImmune = status >= TranceSeekBestiaryDB.StatusScannedPlus ? GetElementsString(_currentMonsterParam.GuardElement) : "???";
                string sHalf = status >= TranceSeekBestiaryDB.StatusScannedPlus ? GetElementsString(_currentMonsterParam.HalfElement) : "???";
                string sWeak = status >= TranceSeekBestiaryDB.StatusScannedPlus ? GetElementsString(_currentMonsterParam.WeakElement) : "???";

                text = $"[FFCC00]{Localization.GetWithDefault("BestiaryElemAbsorb")} :[FFFFFF]\n{sAbsorb}\n\n" +
                       $"[FFCC00]{Localization.GetWithDefault("BestiaryElemImmunity")} :[FFFFFF]\n{sImmune}\n\n" +
                       $"[FFCC00]{Localization.GetWithDefault("BestiaryElemReduction")} :[FFFFFF]\n{sHalf}\n\n" +
                       $"[FFCC00]{Localization.GetWithDefault("BestiaryElemWeakness")} :[FFFFFF]\n{sWeak}\n\n" +
                       $"[FFCC00]{Localization.GetWithDefault("BestiaryCategory")} :[FFFFFF] {sCategory}\n\n" +
                       $"[FFCC00]{Localization.GetWithDefault("BestiaryStatusResist")} :[FFFFFF]\n" +
                       alphaTag + _statusChunks[_statusChunkIndex] + "[FFFFFF]" + pageNav;
            }
            else if (_currentPage == 2)
            {
                string steal0 = status >= TranceSeekBestiaryDB.StatusScannedPlus ? GetItemStringWithIcon(_currentMonsterParam.StealItems[0]) : "???";
                string steal1 = status >= TranceSeekBestiaryDB.StatusScannedPlus ? GetItemStringWithIcon(_currentMonsterParam.StealItems[1]) : "???";
                string steal2 = status >= TranceSeekBestiaryDB.StatusScannedPlus ? GetItemStringWithIcon(_currentMonsterParam.StealItems[2]) : "???";
                string steal3 = status >= TranceSeekBestiaryDB.StatusScannedPlus ? GetItemStringWithIcon(_currentMonsterParam.StealItems[3]) : "???";

                string drop0 = status >= TranceSeekBestiaryDB.StatusScannedPlus ? GetItemStringWithIcon(_currentMonsterParam.WinItems[0]) : "???";
                string drop1 = status >= TranceSeekBestiaryDB.StatusScannedPlus ? GetItemStringWithIcon(_currentMonsterParam.WinItems[1]) : "???";
                string drop2 = status >= TranceSeekBestiaryDB.StatusScannedPlus ? GetItemStringWithIcon(_currentMonsterParam.WinItems[2]) : "???";
                string drop3 = status >= TranceSeekBestiaryDB.StatusScannedPlus ? GetItemStringWithIcon(_currentMonsterParam.WinItems[3]) : "???";

                string sCard = status >= TranceSeekBestiaryDB.StatusScannedPlus ? GetCardName(_currentMonsterParam.WinCard) : "???";

                text = $"[FFCC00]{Localization.GetWithDefault("BestiarySteal")} :[FFFFFF]\n" +
                       $"{steal0}\n{steal1}\n{steal2}\n{steal3}\n\n" +
                       $"[FFCC00]{Localization.GetWithDefault("BestiaryDrop")} :[FFFFFF]\n" +
                       $"{drop0}\n{drop1}\n{drop2}\n{drop3}\n\n" +
                       $"[FFCC00]{Localization.GetWithDefault("BestiaryCard")} :[FFFFFF] {sCard}" + pageNav;
            }

            _monsterInfoLabel.rawText = text;
        }

        private void CleanupMenu()
        {
            if (_bestiaryMenuRoot != null)
                _bestiaryMenuRoot.SetActive(false);

            if (_monsterCamObj != null)
                _monsterCamObj.SetActive(false);

            if (_monsterModel != null)
            {
                Destroy(_monsterModel);
                _monsterModel = null;
            }
            if (_bbgModel != null)
            {
                Destroy(_bbgModel);
                _bbgModel = null;
            }
        }

        private void OpenBestiaryMenu(ConfigUI configUI)
        {
            if (_isBestiaryOpen)
                return;

            FF9Sfx.FF9SFX_Play(103);

            CreateBestiaryMenuHierarchy(configUI);

            configUI.ConfigList.SetActive(false);
            if (configUI.BoosterPanel != null)
                configUI.BoosterPanel.SetActive(false);

            if (_configTitleLabel != null)
                _configTitleLabel.rawText = Localization.GetWithDefault("BestiaryTitle");

            _bestiaryMenuRoot.SetActive(true);

            if (_monsterCamObj != null)
            {
                _monsterCamObj.SetActive(true);
            }

            _currentMonsterId = 1;
            _currentPage = 0;
            LoadMonsterData();
            UpdateMonsterModel();
            InitializeMonsterInfo();

            _isBestiaryOpen = true;

            ButtonGroupState.SetPointerOffsetToGroup(new Vector2(30f, 0f), BestiaryGroupName);
            ButtonGroupState.SetPointerDepthToGroup(7, BestiaryGroupName);
            ButtonGroupState.SetCursorStartSelect(_bestiaryReturnButton, BestiaryGroupName);
            ButtonGroupState.ActiveGroup = BestiaryGroupName;
            ButtonGroupState.ActiveButton = _bestiaryReturnButton;
        }

        private void CloseBestiaryMenu(ConfigUI configUI)
        {
            if (!_isBestiaryOpen)
                return;

            FF9Sfx.FF9SFX_Play(101);

            if (_configTitleLabel != null)
                _configTitleLabel.rawText = _originalTitleText;

            configUI.ConfigList.SetActive(true);
            if (configUI.BoosterPanel != null)
                configUI.BoosterPanel.SetActive(true);

            _isBestiaryOpen = false;

            CleanupMenu();

            ButtonGroupState.ActiveGroup = ConfigGroupName;
            if (_bestiaryEntryButton != null)
            {
                ButtonGroupState.SetCursorStartSelect(_bestiaryEntryButton, ConfigGroupName);
                ButtonGroupState.ActiveButton = _bestiaryEntryButton;
            }
        }

        private void ForceCloseBestiary()
        {
            _isBestiaryOpen = false;

            if (_configTitleLabel != null)
                _configTitleLabel.rawText = _originalTitleText;

            CleanupMenu();
        }

        private void UpdateConfigMenuInput(ConfigUI configUI)
        {
            if (_bestiaryEntryButton == null)
                return;

            if (ButtonGroupState.ActiveGroup != ConfigGroupName)
                return;

            if (ButtonGroupState.ActiveButton != _bestiaryEntryButton)
                return;

            if (UIManager.Input.GetKeyTrigger(Control.Confirm) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OpenBestiaryMenu(configUI);
            }
        }

        private void UpdateBestiaryMenuInput(ConfigUI configUI)
        {
            if (_monsterModel != null && false)
            {
                float moveSpeed = 1f;
                float scaleSpeed = 0.05f;
                float rotSpeed = 5f;

                // Position
                if (Input.GetKey(KeyCode.H)) { _modelPosX += moveSpeed; _bbgPosX += moveSpeed; }
                if (Input.GetKey(KeyCode.F)) { _modelPosX -= moveSpeed; _bbgPosX -= moveSpeed; }
                if (Input.GetKey(KeyCode.T)) { _modelPosY += moveSpeed; _bbgPosY += moveSpeed; }
                if (Input.GetKey(KeyCode.G)) { _modelPosY -= moveSpeed; _bbgPosY -= moveSpeed; }
                if (Input.GetKey(KeyCode.R)) { _modelPosZ += moveSpeed; _bbgPosZ += moveSpeed; }
                if (Input.GetKey(KeyCode.Y)) { _modelPosZ -= moveSpeed; _bbgPosZ -= moveSpeed; }

                // Scale
                if (Input.GetKey(KeyCode.P)) { _modelScale += scaleSpeed; _bbgScale += scaleSpeed; }
                if (Input.GetKey(KeyCode.M)) { _modelScale -= scaleSpeed; _bbgScale -= scaleSpeed; }

                // Rotation
                if (Input.GetKey(KeyCode.U)) { _modelRotX += rotSpeed; _bbgRotX += rotSpeed; }
                if (Input.GetKey(KeyCode.J)) { _modelRotX -= rotSpeed; _bbgRotX -= rotSpeed; }
                if (Input.GetKey(KeyCode.I)) { _modelRotY += rotSpeed; _bbgRotY += rotSpeed; }
                if (Input.GetKey(KeyCode.K)) { _modelRotY -= rotSpeed; _bbgRotY -= rotSpeed; }
                if (Input.GetKey(KeyCode.O)) { _modelRotZ += rotSpeed; _bbgRotZ += rotSpeed; }
                if (Input.GetKey(KeyCode.L)) { _modelRotZ -= rotSpeed; _bbgRotZ -= rotSpeed; }

                _monsterModel.transform.localPosition = new Vector3(_modelPosX, _modelPosY, _modelPosZ);
                _monsterModel.transform.localRotation = Quaternion.Euler(_modelRotX, _modelRotY, _modelRotZ);
                _monsterModel.transform.localScale = new Vector3(_modelScale, _modelScale, _modelScale);

                if (_bbgModel != null)
                {
                    _bbgModel.transform.localPosition = new Vector3(_bbgPosX, _bbgPosY, _bbgPosZ);
                    _bbgModel.transform.localRotation = Quaternion.Euler(_bbgRotX, _bbgRotY, _bbgRotZ);
                    _bbgModel.transform.localScale = new Vector3(_bbgScale, _bbgScale, _bbgScale);
                }

                if (UnityXInput.Input.GetKeyDown(KeyCode.Space))
                {
                    Log.Message($"[DEBUG] _monsterModel => _modelPosX = {_modelPosX} ; _modelPosY = {_modelPosY} ; _modelPosZ = {_modelPosZ} ");
                    Log.Message($"[DEBUG] _monsterModel => _modelRotX = {_modelRotX} ; _modelRotY = {_modelRotY} ; _modelRotZ = {_modelRotZ} ");
                    Log.Message($"[DEBUG] _monsterModel => _modelScale = {_modelScale} ");

                    Log.Message($"[DEBUG] _bbgModel => _bbgPosX = {_bbgPosX} ; _bbgPosY = {_bbgPosY} ; _bbgPosZ = {_bbgPosZ} ");
                    Log.Message($"[DEBUG] _bbgModel => _bbgRotX = {_bbgRotX} ; _bbgRotY = {_bbgRotY} ; _bbgRotZ = {_bbgRotZ} ");
                    Log.Message($"[DEBUG] _bbgModel => _bbgScale = {_bbgScale} ");
                }
            }

            if (_currentPage == 1 && _statusChunks.Count > 1)
            {
                _statusTimer += Time.deltaTime;
                if (_statusTimer >= 2.5f)
                {
                    _statusTimer = 0f;
                    _statusChunkIndex = (_statusChunkIndex + 1) % _statusChunks.Count;
                }
                RefreshPageText();
            }

            if (UIManager.Input.GetKeyTrigger(Control.Cancel) || Input.GetKeyDown(KeyCode.Escape))
            {
                CloseBestiaryMenu(configUI);
                return;
            }

            if (UIManager.Input.GetKeyTrigger(Control.Left) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                _currentPage--;
                if (_currentPage < 0) _currentPage = 2;
                FF9Sfx.FF9SFX_Play(103);
                InitializeMonsterInfo();
            }
            else if (UIManager.Input.GetKeyTrigger(Control.Right) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                _currentPage++;
                if (_currentPage > 2) _currentPage = 0;
                FF9Sfx.FF9SFX_Play(103);
                InitializeMonsterInfo();
            }

            if (UIManager.Input.GetKeyTrigger(Control.Up) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (_currentMonsterId > 1)
                {
                    _currentMonsterId--;
                    FF9Sfx.FF9SFX_Play(103);
                    LoadMonsterData();
                    UpdateMonsterModel();
                    InitializeMonsterInfo();
                }
            }
            else if (UIManager.Input.GetKeyTrigger(Control.Down) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (_currentMonsterId < TranceSeekBestiaryDB.DisplayDatabase.Count)
                {
                    _currentMonsterId++;
                    FF9Sfx.FF9SFX_Play(103);
                    LoadMonsterData();
                    UpdateMonsterModel();
                    InitializeMonsterInfo();
                }
            }

            if (ButtonGroupState.ActiveGroup == BestiaryGroupName && ButtonGroupState.ActiveButton == _bestiaryReturnButton)
            {
                if (PersistenSingleton<HonoInputManager>.Instance.IsInputDown(Control.Confirm) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    CloseBestiaryMenu(configUI);
                }
            }
        }

        private void OnDestroy()
        {
            ForceCloseBestiary();

            if (_monsterRt != null)
            {
                _monsterRt.Release();
                Destroy(_monsterRt);
                _monsterRt = null;
            }

            if (_monsterCamObj != null)
            {
                Destroy(_monsterCamObj);
                _monsterCamObj = null;
            }
        }
    }
}
