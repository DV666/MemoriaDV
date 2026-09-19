using System;
using System.Collections.Generic;
using System.Reflection;
using Assets.Sources.Scripts.UI.Common;
using Memoria.Prime;
using UnityEngine;

namespace Memoria.Scripts.TranceSeek
{
    public class TranceSeekBestiaryMenu : MonoBehaviour
    {
        private const string ConfigGroupName = "Config.Config";
        private const string BestiaryGroupName = "TranceSeek.Bestiary";
        private const string BestiaryButtonName = "Bestiary Panel - Button";

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

        private float _modelPosX = 0f;
        private float _modelPosY = -150f;
        private float _modelPosZ = 100f;
        private float _modelScale = 0.5f;
        private float _modelRotX = 0f;
        private float _modelRotY = 340f;
        private float _modelRotZ = 180f;

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
        private int _currentMonsterId = 0;

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
                returnLabel.rawText = "Retour à la Configuration";
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
                    lbl.rawText = "INFORMATIONS";
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
                    lbl.rawText = "APERÇU";
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
            _monsterCamObj.transform.position = new Vector3(20000f, 20000f, -1000f);
            Camera cam = _monsterCamObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = _modelWindowHeight / 2f;
            cam.targetTexture = _monsterRt;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.05f, 1f);
            cam.cullingMask = 1 << 31;

            GameObject texObj = new GameObject("MonsterTexture");
            texObj.transform.SetParent(modelFrame.transform, false);
            texObj.transform.localPosition = Vector3.zero;
            UITexture uiTex = texObj.AddComponent<UITexture>();
            uiTex.mainTexture = _monsterRt;
            uiTex.width = _modelWindowWidth;
            uiTex.height = _modelWindowHeight;
            uiTex.depth = 12;

            _monsterModel = ModelFactory.CreateModel("GEO_MON_B3_001", false);
            if (_monsterModel != null)
            {
                _monsterModel.SetActive(true);
                _monsterModel.transform.SetParent(_monsterCamObj.transform, false);

                NGUITools.SetLayer(_monsterModel, 31);

                Shader unlitShader = ShadersLoader.Find("Unlit/Transparent Cutout");
                foreach (Renderer r in _monsterModel.GetComponentsInChildren<Renderer>(true))
                {
                    r.enabled = true;
                    if (r is SkinnedMeshRenderer smr)
                    {
                        smr.updateWhenOffscreen = true;
                        smr.localBounds = new Bounds(Vector3.zero, new Vector3(10000f, 10000f, 10000f));
                    }

                    foreach (Material m in r.materials)
                    {
                        if (m != null)
                        {
                            if (unlitShader != null)
                                m.shader = unlitShader;
                        }
                    }
                }

                _monsterModel.transform.localPosition = new Vector3(_modelPosX, _modelPosY, _modelPosZ);
                _monsterModel.transform.localRotation = Quaternion.Euler(_modelRotX, _modelRotY, _modelRotZ);
                _monsterModel.transform.localScale = new Vector3(_modelScale, _modelScale, _modelScale);

                AnimationFactory.AddAnimWithAnimatioName(_monsterModel, "ANH_MON_B3_001_000");
                Animation anim = _monsterModel.GetComponent<Animation>();
                if (anim != null)
                {
                    anim.wrapMode = WrapMode.Loop;
                }
            }

            _currentPage = 0;
            RefreshMonsterInfo();

            _bestiaryMenuRoot.SetActive(false);
            if (_monsterCamObj != null) _monsterCamObj.SetActive(false);
        }

        private void RefreshMonsterInfo()
        {
            if (_monsterInfoLabel == null)
                return;

            string text = "";

            if (_currentPage == 0)
            {
                text = "[FFCC00]Nom :[-] ???\n\n" +
                       "[FFCC00]HP :[-] ???               [FFCC00]MP :[-] ???\n" +
                       "[FFCC00]Vitesse :[-] ???          [FFCC00]Force :[-] ???\n" +
                       "[FFCC00]Magie :[-] ???            [FFCC00]Esprit :[-] ???\n" +
                       "[FFCC00]Défense :[-] ???          [FFCC00]Esquive :[-] ???\n" +
                       "[FFCC00]Déf. Mag. :[-] ???        [FFCC00]Esq. Mag. :[-] ???\n\n" +
                       "[FFCC00]Gils :[-] ???\n" +
                       "[FFCC00]Exp :[-] ???";
            }
            else if (_currentPage == 1)
            {
                text = "[FFCC00]Absorption Élémentaire :[-]\n???\n\n" +
                       "[FFCC00]Immunité Élémentaire :[-]\n???\n\n" +
                       "[FFCC00]Réduction Élémentaire :[-]\n???\n\n" +
                       "[FFCC00]Faiblesse Élémentaire :[-]\n???\n\n" +
                       "[FFCC00]Catégorie :[-] ???\n\n" +
                       "[FFCC00]Résistance aux altérations :[-]\n???";
            }
            else if (_currentPage == 2)
            {
                text = "[FFCC00]Objets à voler :[-]\n" +
                       "- ???\n- ???\n- ???\n- ???\n\n" +
                       "[FFCC00]Récompenses :[-]\n" +
                       "- ???\n- ???\n- ???\n- ???\n\n" +
                       "[FFCC00]Récompense carte :[-] ???";
            }

            text += $"\n\n[A0A0A0]< Page {_currentPage + 1}/3 >[-]";

            _monsterInfoLabel.rawText = text;
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
                _configTitleLabel.rawText = "Bestiaire";

            _bestiaryMenuRoot.SetActive(true);

            if (_monsterCamObj != null)
            {
                _monsterCamObj.SetActive(true);
            }

            if (_monsterModel != null)
            {
                Animation anim = _monsterModel.GetComponent<Animation>();
                if (anim != null)
                {
                    anim.Play("ANH_MON_B3_001_000");
                }
            }

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

            if (_bestiaryMenuRoot != null)
                _bestiaryMenuRoot.SetActive(false);

            if (_monsterCamObj != null)
                _monsterCamObj.SetActive(false);

            if (_configTitleLabel != null)
                _configTitleLabel.rawText = _originalTitleText;

            configUI.ConfigList.SetActive(true);
            if (configUI.BoosterPanel != null)
                configUI.BoosterPanel.SetActive(true);

            _isBestiaryOpen = false;

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
            if (_bestiaryMenuRoot != null)
                _bestiaryMenuRoot.SetActive(false);

            if (_monsterCamObj != null)
                _monsterCamObj.SetActive(false);

            if (_configTitleLabel != null)
                _configTitleLabel.rawText = _originalTitleText;
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
            if (_monsterModel != null)
            {
                float moveSpeed = 1f;
                float scaleSpeed = 0.05f;
                float rotSpeed = 5f;

                if (Input.GetKey(KeyCode.H)) _modelPosX += moveSpeed;
                if (Input.GetKey(KeyCode.F)) _modelPosX -= moveSpeed;
                if (Input.GetKey(KeyCode.T)) _modelPosY += moveSpeed;
                if (Input.GetKey(KeyCode.G)) _modelPosY -= moveSpeed;
                if (Input.GetKey(KeyCode.R)) _modelPosZ += moveSpeed;
                if (Input.GetKey(KeyCode.Y)) _modelPosZ -= moveSpeed;

                if (Input.GetKey(KeyCode.P)) _modelScale += scaleSpeed;
                if (Input.GetKey(KeyCode.M)) _modelScale -= scaleSpeed;

                if (Input.GetKey(KeyCode.U)) _modelRotX += rotSpeed;
                if (Input.GetKey(KeyCode.J)) _modelRotX -= rotSpeed;
                if (Input.GetKey(KeyCode.I)) _modelRotY += rotSpeed;
                if (Input.GetKey(KeyCode.K)) _modelRotY -= rotSpeed;
                if (Input.GetKey(KeyCode.O)) _modelRotZ += rotSpeed;
                if (Input.GetKey(KeyCode.L)) _modelRotZ -= rotSpeed;

                _monsterModel.transform.localPosition = new Vector3(_modelPosX, _modelPosY, _modelPosZ);
                _monsterModel.transform.localRotation = Quaternion.Euler(_modelRotX, _modelRotY, _modelRotZ);
                _monsterModel.transform.localScale = new Vector3(_modelScale, _modelScale, _modelScale);

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Log.Message($"[Debug RT Model] Pos: ({_modelPosX:F1}, {_modelPosY:F1}, {_modelPosZ:F1}) | Scale: {_modelScale:F3} | Rot: ({_modelRotX:F1}, {_modelRotY:F1}, {_modelRotZ:F1})");
                }
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
                RefreshMonsterInfo();
            }
            else if (UIManager.Input.GetKeyTrigger(Control.Right) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                _currentPage++;
                if (_currentPage > 2) _currentPage = 0;
                FF9Sfx.FF9SFX_Play(103);
                RefreshMonsterInfo();
            }

            if (ButtonGroupState.ActiveGroup == BestiaryGroupName && ButtonGroupState.ActiveButton == _bestiaryReturnButton)
            {
                if (UIManager.Input.GetKeyTrigger(Control.Confirm) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    CloseBestiaryMenu(configUI);
                }
            }
        }

        private void OnDestroy()
        {
            ForceCloseBestiary();
            if (_bestiaryMenuRoot != null)
                Destroy(_bestiaryMenuRoot);

            if (_monsterModel != null)
                Destroy(_monsterModel);

            if (_monsterRt != null)
                _monsterRt.Release();

            if (_monsterCamObj != null)
                Destroy(_monsterCamObj);
        }
    }
}
