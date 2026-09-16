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

        // Réflexion sur les champs privés de ConfigUI
        private static readonly FieldInfo ConfigFieldListField = typeof(ConfigUI).GetField("ConfigFieldList", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo ConfigScrollViewField = typeof(ConfigUI).GetField("configScrollView", BindingFlags.Instance | BindingFlags.NonPublic);

        // Réflexion sur les types internal d'Assembly-CSharp
        private static readonly Type ScrollItemKeyNavType = typeof(ConfigUI).Assembly.GetType("ScrollItemKeyNavigation");
        private static readonly FieldInfo NavIdField = ScrollItemKeyNavType?.GetField("ID", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        private static readonly Type SnapDragScrollViewType = typeof(ConfigUI).Assembly.GetType("SnapDragScrollView");
        private static readonly PropertyInfo MaxItemProp = SnapDragScrollViewType?.GetProperty("MaxItem", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly FieldInfo MaxItemField = SnapDragScrollViewType?.GetField("MaxItem", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        // Éléments du menu Config
        private GameObject _bestiaryEntryButton;
        private UILabel _configTitleLabel;
        private string _originalTitleText = "Config.";

        // Éléments du menu Bestiaire
        private GameObject _bestiaryMenuRoot;
        private GameObject _bestiaryReturnButton;
        private bool _isBestiaryOpen = false;

        private void Update()
        {
            if (PersistenSingleton<UIManager>.Instance == null)
                return;

            // Si le joueur quitte l'écran de configuration (ex: fondu, transition forcée)
            if (PersistenSingleton<UIManager>.Instance.State != UIManager.UIState.Config)
            {
                if (_isBestiaryOpen)
                    ForceCloseBestiary();
                return;
            }

            ConfigUI configScene = PersistenSingleton<UIManager>.Instance.ConfigScene;
            if (configScene == null || configScene.ConfigList == null)
                return;

            // 1. Injection du bouton dans la liste Config si ce n'est pas encore fait
            if (_bestiaryEntryButton == null)
            {
                TryInjectBestiaryButton(configScene);
            }

            // 2. Gestion des inputs selon le mode actif
            if (_isBestiaryOpen)
            {
                UpdateBestiaryMenuInput(configScene);
            }
            else
            {
                UpdateConfigMenuInput(configScene);
            }
        }

        #region Injection du bouton dans le menu Config

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

            // Cloner le bouton du tutoriel des commandes
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

            // Mise à jour des IDs de navigation NGUI
            if (ScrollItemKeyNavType != null && NavIdField != null)
            {
                for (int i = 0; i < fieldList.Count; i++)
                {
                    Component nav = fieldList[i].ConfigParent.GetComponent(ScrollItemKeyNavType);
                    if (nav != null)
                        NavIdField.SetValue(nav, i);
                }
            }

            // Mise à jour de la portée du ScrollView
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

            // Repositionnement NGUI
            RepositionList(listParent, templateGo, targetSiblingIndex);
            RebuildNavigationChain(fieldList);

            UIEventListener.Get(_bestiaryEntryButton).onClick = go => OpenBestiaryMenu(configUI);

            // Recherche du label de titre de la scène Config (badge en haut à droite)
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

        #endregion

        #region Création et Gestion du Menu Bestiaire

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

            // 1. Cadre principal cloné depuis ConfigList (garde le style, dégradé bleu et bordures exacts)
            GameObject frameTemplate = configUI.ConfigList.transform.GetChild(2).gameObject;
            GameObject bestiaryFrame = Instantiate(frameTemplate);
            bestiaryFrame.name = "Bestiary Window Frame";
            bestiaryFrame.transform.SetParent(_bestiaryMenuRoot.transform, false);
            bestiaryFrame.transform.localPosition = frameTemplate.transform.localPosition;
            bestiaryFrame.transform.localScale = Vector3.one;

            // Renommer le titre d'onglet dans le coin supérieur gauche ("CONFIG." -> "BESTIAIRE")
            foreach (UILabel lbl in bestiaryFrame.GetComponentsInChildren<UILabel>(true))
            {
                if (lbl.rawText != null && lbl.rawText.Contains("CONFIG"))
                {
                    UILocalize loc = lbl.GetComponent<UILocalize>();
                    if (loc != null) Destroy(loc);
                    lbl.rawText = "BESTIAIRE";
                }
            }

            // 2. Message / Contenu du Bestiaire
            UILabel templateLabel = _bestiaryEntryButton.GetComponentInChildren<UILabel>();
            GameObject textObj = Instantiate(templateLabel.gameObject);
            textObj.name = "Bestiary Content Label";
            textObj.transform.SetParent(_bestiaryMenuRoot.transform, false);
            textObj.transform.localPosition = new Vector3(0f, 40f, 0f);

            UILabel contentLabel = textObj.GetComponent<UILabel>();
            UILocalize textLoc = contentLabel.GetComponent<UILocalize>();
            if (textLoc != null) Destroy(textLoc);

            contentLabel.width = 720;
            contentLabel.height = 260;
            contentLabel.alignment = NGUIText.Alignment.Center;
            contentLabel.overflowMethod = UILabel.Overflow.ResizeHeight;
            contentLabel.rawText = "[FF9900]Menu Bestiaire[-]\n\n" +
                                   "[FFFFFF]Bienvenue dans le bestiaire de Trance Seek !\n\n" +
                                   "Ce panneau utilise les composants natifs du jeu et servira\n" +
                                   "de base pour afficher vos fiches de monstres et stats.[-]\n\n" +
                                   "[A0A0A0](Appuyez sur Annuler pour revenir)[-]";

            // 3. Bouton Retour (cloné pour conserver les colliders, hover et curseur NGUI)
            _bestiaryReturnButton = Instantiate(_bestiaryEntryButton);
            _bestiaryReturnButton.name = "Bestiary Return Button";
            _bestiaryReturnButton.transform.SetParent(_bestiaryMenuRoot.transform, false);
            _bestiaryReturnButton.transform.localPosition = new Vector3(0f, -150f, 0f);

            // Nettoyage des dummy choices
            Transform d1 = _bestiaryReturnButton.transform.Find("DummyChoice1");
            if (d1 != null) Destroy(d1.gameObject);
            Transform d2 = _bestiaryReturnButton.transform.Find("DummyChoice2");
            if (d2 != null) Destroy(d2.gameObject);

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

            _bestiaryMenuRoot.SetActive(false);
        }

        private void OpenBestiaryMenu(ConfigUI configUI)
        {
            if (_isBestiaryOpen)
                return;

            FF9Sfx.FF9SFX_Play(103); // Bruit de confirmation FFIX

            CreateBestiaryMenuHierarchy(configUI);

            // 1. Masquer les listes de la Configuration
            configUI.ConfigList.SetActive(false);
            if (configUI.BoosterPanel != null)
                configUI.BoosterPanel.SetActive(false);

            // 2. Mettre à jour le badge de titre
            if (_configTitleLabel != null)
                _configTitleLabel.rawText = "Bestiaire";

            // 3. Afficher notre nouveau menu
            _bestiaryMenuRoot.SetActive(true);
            _isBestiaryOpen = true;

            // 4. Configurer la main-curseur et le groupe d'input dédié
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

            FF9Sfx.FF9SFX_Play(101); // Bruit d'annulation FFIX

            // 1. Masquer notre menu Bestiaire
            if (_bestiaryMenuRoot != null)
                _bestiaryMenuRoot.SetActive(false);

            // 2. Restaurer le badge de titre
            if (_configTitleLabel != null)
                _configTitleLabel.rawText = _originalTitleText;

            // 3. Réactiver la liste du menu Config
            configUI.ConfigList.SetActive(true);
            if (configUI.BoosterPanel != null)
                configUI.BoosterPanel.SetActive(true);

            _isBestiaryOpen = false;

            // 4. Rétablir le groupe et replacer le curseur sur "Bestiaire"
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

            // Validation via manette ou clavier
            if (UIManager.Input.GetKeyTrigger(Control.Confirm) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OpenBestiaryMenu(configUI);
            }
        }

        private void UpdateBestiaryMenuInput(ConfigUI configUI)
        {
            // Touche Annuler (Manette Cercle/B, Échap, etc.)
            if (UIManager.Input.GetKeyTrigger(Control.Cancel) || Input.GetKeyDown(KeyCode.Escape))
            {
                CloseBestiaryMenu(configUI);
                return;
            }

            // Touche Confirmer sur le bouton Retour
            if (ButtonGroupState.ActiveGroup == BestiaryGroupName && ButtonGroupState.ActiveButton == _bestiaryReturnButton)
            {
                if (UIManager.Input.GetKeyTrigger(Control.Confirm) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    CloseBestiaryMenu(configUI);
                }
            }
        }

        #endregion

        private void OnDestroy()
        {
            ForceCloseBestiary();
            if (_bestiaryMenuRoot != null)
                Destroy(_bestiaryMenuRoot);
        }
    }
}
