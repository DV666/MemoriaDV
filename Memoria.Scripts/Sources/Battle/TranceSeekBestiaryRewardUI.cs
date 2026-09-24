using System;
using System.Collections.Generic;
using Assets.Sources.Scripts.UI.Common;
using Memoria.Prime;
using UnityEngine;

namespace Memoria.Scripts.TranceSeek
{
    public class TranceSeekBestiaryRewardUI : MonoBehaviour
    {
        public Vector3 PopupPosition = new Vector3(0f, -120f, 0f);
        public int PopupHeight = 400;
        public Vector3 TextPosition = new Vector3(-40f, 120f, 0f);

        private bool _isShowing = false;
        private GameObject _clonedPanel = null;

        private void Update()
        {
            UIManager uiManager = PersistenSingleton<UIManager>.Instance;
            if (uiManager == null)
                return;

            if (uiManager.State == UIManager.UIState.BattleResult)
            {
                BattleResultUI brUI = uiManager.BattleResultScene;

                if (brUI != null && brUI.GilAndItemPhrasePanel != null && brUI.GilAndItemPhrasePanel.activeInHierarchy)
                {
                    if (!_isShowing && TranceSeekBestiaryDB.PendingRewardMessages.Count > 0)
                    {
                        ShowRewardPopup(brUI);
                    }
                    else if (_isShowing)
                    {
                        if (PersistenSingleton<HonoInputManager>.Instance.IsInputDown(Control.Confirm) || Input.GetKeyDown(KeyCode.KeypadEnter))
                        {
                            ClosePopup();
                        }
                    }
                }
            }
            else
            {
                if (_isShowing)
                {
                    ClosePopup();
                }
            }
        }

        private void ShowRewardPopup(BattleResultUI brUI)
        {
            _isShowing = true;

            HonoTweenClipping tweenComponent = brUI.TransitionPanel.transform.GetChild(4).GetComponent<HonoTweenClipping>();
            if (tweenComponent == null || tweenComponent.ClipGameObject == null)
                return;

            GameObject originalPanel = tweenComponent.ClipGameObject;

            _clonedPanel = Instantiate(originalPanel) as GameObject;
            _clonedPanel.transform.parent = originalPanel.transform.parent;
            _clonedPanel.transform.localPosition = PopupPosition;
            _clonedPanel.transform.localScale = Vector3.one;

            _clonedPanel.name = "TranceSeek_BestiaryRewardPopup";
            _clonedPanel.SetActive(true);

            foreach (Behaviour t in _clonedPanel.GetComponentsInChildren<Behaviour>(true))
            {
                if (t.GetType().Name.Contains("Tween"))
                    Destroy(t);
            }

            UILocalize[] localizes = _clonedPanel.GetComponentsInChildren<UILocalize>(true);
            foreach (UILocalize loc in localizes)
            {
                Destroy(loc);
            }

            UIWidget[] widgets = _clonedPanel.GetComponentsInChildren<UIWidget>(true);
            foreach (UIWidget w in widgets)
            {
                w.gameObject.SetActive(true);
                w.alpha = 1f;

                if (w is UISprite && w.height > 50)
                {
                    w.topAnchor.target = null;
                    w.bottomAnchor.target = null;
                    w.height = PopupHeight;
                }
            }

            UIPanel panel = _clonedPanel.GetComponent<UIPanel>();
            if (panel != null)
            {
                panel.depth = 100;
                panel.clipping = UIDrawCall.Clipping.None;
                panel.alpha = 1f;
            }

            UILabel[] labels = _clonedPanel.GetComponentsInChildren<UILabel>(true);
            if (labels.Length > 0)
            {
                UILabel templateLabel = labels[0];

                foreach (UILabel lbl in labels)
                {
                    lbl.rawText = string.Empty;
                }

                GameObject bodyObj = Instantiate(templateLabel.gameObject) as GameObject;
                bodyObj.transform.parent = _clonedPanel.transform;
                bodyObj.transform.localPosition = TextPosition;
                bodyObj.transform.localScale = Vector3.one;

                UILabel bodyLabel = bodyObj.GetComponent<UILabel>();
                if (bodyLabel != null)
                {
                    bodyLabel.multiLine = true;
                    bodyLabel.overflowMethod = UILabel.Overflow.ResizeFreely;
                    bodyLabel.alignment = NGUIText.Alignment.Center;
                    bodyLabel.pivot = UIWidget.Pivot.Top;
                    bodyLabel.rawText = string.Join("\n", TranceSeekBestiaryDB.PendingRewardMessages.ToArray());
                    bodyLabel.depth = 51;
                }
            }

            FF9Sfx.FF9SFX_Play(1043);
        }

        private void ClosePopup()
        {
            _isShowing = false;

            if (_clonedPanel != null)
            {
                _clonedPanel.SetActive(false);
                UnityEngine.Object.Destroy(_clonedPanel);
                _clonedPanel = null;
            }

            TranceSeekBestiaryDB.PendingRewardMessages.Clear();
        }
    }
}
