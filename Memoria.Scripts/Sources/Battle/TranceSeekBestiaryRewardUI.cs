using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Sources.Scripts.UI.Common;
using Memoria.Prime;
using UnityEngine;

namespace Memoria.Scripts.TranceSeek
{
    public class TranceSeekBestiaryRewardUI : MonoBehaviour
    {
        private const string FadeAnimPrefix = "[ANIM=TextRGBA,Sinus,1,Constant,2,Sinus,3,0,1,1,0]";
        private const float PageDuration = 3f;

        public Vector3 PopupPosition = new Vector3(0f, -120f, 0f);
        public int PopupHeight = 400;
        public Vector3 TextPosition = new Vector3(-40f, 120f, 0f);

        private bool _isShowing = false;
        private GameObject _clonedPanel = null;
        private UILabel _bodyLabel = null;
        private int _currentMessageIndex = 0;
        private float _pageTimer = 0f;

        private void Update()
        {
            UIManager uiManager = PersistenSingleton<UIManager>.Instance;
            if (uiManager == null)
                return;

            if (uiManager.State != UIManager.UIState.BattleResult)
            {
                if (_isShowing)
                    ClosePopup(null);
                return;
            }

            BattleResultUI brUI = uiManager.BattleResultScene;
            if (brUI == null || brUI.GilAndItemPhrasePanel == null)
                return;

            if (!_isShowing)
            {
                if (brUI.GilAndItemPhrasePanel.activeInHierarchy && TranceSeekBestiaryDB.PendingRewardMessages.Count > 0)
                {
                    ShowRewardPopup(brUI);
                }
            }
            else
            {
                if (!brUI.GilAndItemPhrasePanel.activeInHierarchy)
                {
                    ClosePopup(brUI);
                    return;
                }

                if (TranceSeekBestiaryDB.PendingRewardMessages.Count > 1 && _bodyLabel != null)
                {
                    _pageTimer -= RealTime.deltaTime;
                    if (_pageTimer <= 0f)
                    {
                        _currentMessageIndex = (_currentMessageIndex + 1) % TranceSeekBestiaryDB.PendingRewardMessages.Count;
                        UpdateDisplayedRewardMessage();
                    }
                }

                bool confirmPressed =
                    PersistenSingleton<HonoInputManager>.Instance.IsInputDown(Control.Confirm) ||
                    PersistenSingleton<HonoInputManager>.Instance.IsInputDown(Control.Cancel) ||
                    UIManager.Input.GetKeyTrigger(Control.Confirm) ||
                    UIManager.Input.GetKeyTrigger(Control.Cancel) ||
                    Input.GetKeyDown(KeyCode.Return) ||
                    Input.GetKeyDown(KeyCode.KeypadEnter) ||
                    Input.GetMouseButtonDown(0);

                if (confirmPressed)
                {
                    FF9Sfx.FF9SFX_Play(103);
                    ClosePopup(brUI);
                }
            }
        }

        private void ShowRewardPopup(BattleResultUI brUI)
        {
            HonoTweenClipping tweenComponent = brUI.TransitionPanel.transform.GetChild(4).GetComponent<HonoTweenClipping>();
            if (tweenComponent == null || tweenComponent.ClipGameObject == null)
                return;

            _isShowing = true;
            _currentMessageIndex = 0;

            brUI.Loading = true;

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

                _bodyLabel = bodyObj.GetComponent<UILabel>();
                if (_bodyLabel != null)
                {
                    _bodyLabel.multiLine = true;
                    _bodyLabel.overflowMethod = UILabel.Overflow.ResizeFreely;
                    _bodyLabel.alignment = NGUIText.Alignment.Center;
                    _bodyLabel.pivot = UIWidget.Pivot.Top;
                    _bodyLabel.depth = 51;
                    UpdateDisplayedRewardMessage();
                }
            }

            FF9Sfx.FF9SFX_Play(1043);
        }

        private void UpdateDisplayedRewardMessage()
        {
            if (_bodyLabel == null || TranceSeekBestiaryDB.PendingRewardMessages.Count == 0)
                return;

            string blockText = TranceSeekBestiaryDB.PendingRewardMessages[_currentMessageIndex];
            bool hasMultipleBlocks = TranceSeekBestiaryDB.PendingRewardMessages.Count > 1;

            if (hasMultipleBlocks)
                blockText = FadeAnimPrefix + blockText;

            _bodyLabel.SetText(blockText);
            _pageTimer = PageDuration;
        }

        private void ClosePopup(BattleResultUI brUI)
        {
            _isShowing = false;
            _bodyLabel = null;

            if (_clonedPanel != null)
            {
                _clonedPanel.SetActive(false);
                UnityEngine.Object.Destroy(_clonedPanel);
                _clonedPanel = null;
            }

            TranceSeekBestiaryDB.PendingRewardMessages.Clear();

            if (brUI != null)
            {
                StartCoroutine(UnlockBattleResultNextFrame(brUI));
            }
        }

        private IEnumerator UnlockBattleResultNextFrame(BattleResultUI brUI)
        {
            yield return null;
            if (brUI != null)
                brUI.Loading = false;
        }
    }
}
