using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Assets;
using Memoria.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static BTL_DATA;
using static UIManager;

namespace Memoria.Scripts.TranceSeek
{
    public class OverloadedPlayerUI : IOverloadPlayerUIScript
    {
        public static PLAYER CurrentPlayer;
        private static bool _isMenuInjected = false;

        private static List<PLAYER> PlayerPreventEncounter = new List<PLAYER>();
        private static bool _disableencounter = false;

        private Boolean HPColored = Configuration.Mod.FolderNames.Contains("TranceSeek/Options/ColoredHP");
        private Boolean MPColored = Configuration.Mod.FolderNames.Contains("TranceSeek/Options/ColoredMP");
        private Boolean GemColored = Configuration.Mod.FolderNames.Contains("TranceSeek/Options/ColoredGems");
        private Boolean BoostedSAIndicatorDisabled = Configuration.Mod.FolderNames.Contains("Options/BoostedSAIndicator/Disabled");

        public IOverloadPlayerUIScript.Result UpdatePointStatus(PLAYER player)
        {
            if (!_isMenuInjected)
            {
                GameObject go = new GameObject("Mod_SAClearHandler");
                go.AddComponent<SAClearInputHandler>();
                if (!BoostedSAIndicatorDisabled)
                    go.AddComponent<SABoostVisualizerHandler>();
                UnityEngine.Object.DontDestroyOnLoad(go);
                _isMenuInjected = true;
            }

            CurrentPlayer = player;

            IOverloadPlayerUIScript.Result result = new IOverloadPlayerUIScript.Result();
            result.ColorHP = (player.cur.hp == 0) ? FF9TextTool.Red
                           : (player.cur.hp <= player.max.hp / 6) ? FF9TextTool.Yellow : FF9TextTool.White;
            result.ColorMP = (player.cur.mp <= player.max.mp / 6) ? FF9TextTool.Yellow : FF9TextTool.White;

            if (player.cur.hp == player.max.hp && HPColored)
                result.ColorHP = FF9TextTool.Green;
            if (player.cur.mp == player.max.mp && MPColored)
                result.ColorMP = new Color(0.28104f, 0.43712f, 0.96821f);

            if (!GemColored)
            {
                result.ColorMagicStone = (player.cur.capa == 0) ? FF9TextTool.Yellow : FF9TextTool.White;
            }
            else
            {
                if (ff9abil._FF9Abil_PaData.TryGetValue(player.PresetId, out CharacterAbility[] paArray))
                {
                    Boolean NoSA = true;
                    foreach (CharacterAbility pa in paArray)
                        if (pa.IsPassive)
                            NoSA = false;

                    if (NoSA)
                        result.ColorMagicStone = FF9TextTool.Gray;
                }

                if (player.cur.capa == 0)
                    result.ColorMagicStone = FF9TextTool.White;

                float CurCapa = (float)player.cur.capa;
                float MaxCapa = (float)player.max.capa;
                float RatioCapa = CurCapa / MaxCapa;
                float red = 0.80f - (0.80f * RatioCapa);
                float green = 1.0f;
                float blue = 1.0f;

                result.ColorMagicStone = new Color(red, green, blue);
            }

            int IdDict = (int)(2000 + player.Index);
            if (!FF9StateSystem.EventState.gScriptDictionary.TryGetValue(IdDict, out Dictionary<Int32, Int32> dictbattle))
            {
                dictbattle = new Dictionary<Int32, Int32>();
                FF9StateSystem.EventState.gScriptDictionary.Add(IdDict, dictbattle);
                dictbattle[1] = 0;
                dictbattle[2] = 0;
                dictbattle[3] = 0;
            }

            if (player.saExtended.Contains(TranceSeekSupportAbility.Anastrophe_Boosted)) // SA Anastrophe+
            {
                if (dictbattle[3] != 2)
                {
                    dictbattle[1] = 0;
                    dictbattle[2] = 0;
                    dictbattle[3] = 2;
                    ff9play.FF9Play_Update(player);
                    dictbattle[1] = (int)(player.max.hp);
                    dictbattle[2] = (int)(player.max.mp);
                    ff9play.FF9Play_Update(player);
                }
            }
            else if (player.saExtended.Contains(TranceSeekSupportAbility.Anastrophe)) // SA Anastrophe
            {
                if (dictbattle[3] != 1)
                {
                    dictbattle[1] = 0;
                    dictbattle[2] = 0;
                    dictbattle[3] = 1;
                    ff9play.FF9Play_Update(player);
                    dictbattle[1] = (int)(player.max.hp / 2);
                    dictbattle[2] = (int)(player.max.mp / 2);
                    ff9play.FF9Play_Update(player);
                }
            }
            else
            {
                dictbattle[1] = 0;
                dictbattle[2] = 0;
                dictbattle[3] = 0;
            }

            if (!PlayerPreventEncounter.Contains(player) && player.equip.Accessory == TranceSeekRegularItem.MalboroIncense)
                PlayerPreventEncounter.Add(player);
            else if (PlayerPreventEncounter.Contains(player) && player.equip.Accessory != TranceSeekRegularItem.MalboroIncense)
                PlayerPreventEncounter.Remove(player);

            if (PlayerPreventEncounter.Count > 0)
                FF9StateSystem.Settings.IsBoosterButtonActive[4] = true;
            else
                FF9StateSystem.Settings.IsBoosterButtonActive[4] = false;

            return result;
        }

        public class SAClearInputHandler : MonoBehaviour
        {
            private bool _isActionTriggered = false;
            private Dialog _confirmDialog = null;

            private void Update()
            {
                if (_confirmDialog != null)
                    return;

                if (ButtonGroupState.ActiveGroup != "Ability.SupportAbility")
                    return;

                if (PersistenSingleton<HonoInputManager>.Instance.IsInputDown(Control.LeftTrigger) || UIManager.Input.GetKey(Control.LeftTrigger))
                {
                    if (!_isActionTriggered)
                    {
                        ShowConfirmDialog();
                        _isActionTriggered = true;
                    }
                }
                else
                {
                    _isActionTriggered = false;
                }
            }

            private void ShowConfirmDialog()
            {
                FF9Sfx.FF9SFX_Play(103);
                String lang = Localization.CurrentDisplaySymbol ?? "US";

                if (!ConfirmClearSADialogTexts.TryGetValue(lang, out String dialogText))
                    dialogText = ConfirmClearSADialogTexts["US"];

                _confirmDialog = Singleton<DialogManager>.Instance.AttachDialog(
                    dialogText,
                    0,
                    0,
                    Dialog.TailPosition.Center,
                    Dialog.WindowStyle.WindowStylePlain,
                    Vector2.zero,
                    Dialog.CaptionType.None
                );

                _confirmDialog.DefaultChoice = 1;
                _confirmDialog.CancelChoice = 1;
                _confirmDialog.AfterDialogHidden = OnConfirmDialogHidden;

                PersistenSingleton<UIManager>.Instance.IsWarningDialogEnable = true;

                ButtonGroupState.DisableAllGroup(true);
            }

            private void OnConfirmDialogHidden(Int32 choice)
            {
                _confirmDialog = null;

                PersistenSingleton<UIManager>.Instance.IsWarningDialogEnable = false;
                ButtonGroupState.ActiveGroup = "Ability.SupportAbility";

                if (choice == 0)
                    ClearSupportAbilities();
            }

            private void ClearSupportAbilities()
            {
                PLAYER player = OverloadedPlayerUI.CurrentPlayer;
                if (player == null) return;

                Boolean hasChanged = false;
                List<SupportAbility> toRemove = new List<SupportAbility>();

                foreach (SupportAbility sa in player.saExtended)
                    if (!player.saForced.Contains(sa))
                        toRemove.Add(sa);

                if (toRemove.Count == 0)
                    return;

                foreach (SupportAbility sa in toRemove)
                {
                    ff9abil.FF9Abil_SetEnableSA(player, sa, false, true);
                    hasChanged = true;
                }

                if (hasChanged)
                {
                    FF9Sfx.FF9SFX_Play(107);
                    ff9play.FF9Play_Update(player);

                    AbilityUI abilityScene = PersistenSingleton<UIManager>.Instance.AbilityScene;
                    if (abilityScene != null && abilityScene.isActiveAndEnabled)
                    {
                        Type abilityUiType = typeof(AbilityUI);
                        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

                        var displaySAMethod = abilityUiType.GetMethod("DisplaySA", flags);
                        displaySAMethod?.Invoke(abilityScene, null);

                        var displayCharMethod = abilityUiType.GetMethod("DisplayCharacter", flags);
                        displayCharMethod?.Invoke(abilityScene, new object[] { true });

                        var setAbilityInfoMethod = abilityUiType.GetMethod("SetAbilityInfo", flags);
                        setAbilityInfoMethod?.Invoke(abilityScene, new object[] { true });
                    }
                }
            }
        }

        public class SABoostVisualizerHandler : MonoBehaviour
        {
            private System.Reflection.FieldInfo _saIdListField;
            private System.Reflection.FieldInfo _scrollListField;
            private bool _isInitialized = false;

            private Boolean _pinkTextEnabled;
            private Boolean _fadingTextEnabled;

            private RecycleListItem[] _cachedItems = null;
            private Dictionary<RecycleListItem, UILabel> _cachedLabels = new Dictionary<RecycleListItem, UILabel>();

            private void Awake()
            {
                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                _saIdListField = typeof(AbilityUI).GetField("saIdList", flags);
                _scrollListField = typeof(AbilityUI).GetField("supportAbilityScrollList", flags);

                if (_saIdListField != null && _scrollListField != null)
                    _isInitialized = true;

                _pinkTextEnabled = Configuration.Mod.FolderNames.Contains("TranceSeek/Options/BoostedSAIndicator/PinkText");
                _fadingTextEnabled = Configuration.Mod.FolderNames.Contains("TranceSeek/Options/BoostedSAIndicator/Fading");
            }

            private void LateUpdate()
            {
                if (!_isInitialized)
                    return;

                AbilityUI abilityScene = PersistenSingleton<UIManager>.Instance.AbilityScene;
                if (abilityScene == null || !abilityScene.isActiveAndEnabled)
                    return;

                bool isInSAMenu = ButtonGroupState.ActiveGroup == "Ability.SupportAbility";
                bool isHoveringSA = ButtonGroupState.ActiveGroup == "Ability.SubMenu" && abilityScene.MagicStonePanel.activeInHierarchy;

                if (!isInSAMenu && !isHoveringSA)
                    return;

                PLAYER player = OverloadedPlayerUI.CurrentPlayer;
                if (player == null)
                    return;

                List<Int32> saIdList = _saIdListField.GetValue(abilityScene) as List<Int32>;
                RecycleListPopulator scrollList = _scrollListField.GetValue(abilityScene) as RecycleListPopulator;

                if (saIdList == null || scrollList == null)
                    return;

                if (_cachedItems == null || _cachedItems.Length == 0 || _cachedItems[0] == null)
                    _cachedItems = scrollList.GetComponentsInChildren<RecycleListItem>(true);

                foreach (RecycleListItem recycleItem in _cachedItems)
                {
                    if (!recycleItem.gameObject.activeInHierarchy)
                        continue;

                    Int32 itemIndex = recycleItem.ItemDataIndex;
                    if (itemIndex < 0 || itemIndex >= saIdList.Count)
                        continue;

                    UILabel nameLabel = null;
                    if (!_cachedLabels.TryGetValue(recycleItem, out nameLabel))
                    {
                        ItemListDetailWithIconHUD hud = new ItemListDetailWithIconHUD(recycleItem.gameObject, true);
                        nameLabel = hud.NameLabel;
                        if (nameLabel != null)
                            _cachedLabels[recycleItem] = nameLabel;
                    }

                    if (nameLabel == null)
                        continue;

                    Int32 abilityId = saIdList[itemIndex];
                    SupportAbility baseSupportId = ff9abil.GetSupportAbilityFromAbilityId(abilityId);

                    Int32 maxLevel = ff9abil.GetBoostedAbilityMaxLevel(player, baseSupportId);
                    Boolean highlight = false;
                    SupportAbility displaySupportId = baseSupportId;

                    Boolean isBaseEquipped = ff9abil.FF9Abil_IsEnableSA(player.saExtended, baseSupportId);

                    if (isBaseEquipped && maxLevel > 0)
                    {
                        Int32 boostLevel = Math.Min(maxLevel, ff9abil.GetBoostedAbilityLevel(player, baseSupportId));
                        List<SupportAbility> boostedList = ff9abil.GetBoostedAbilityList(baseSupportId);

                        if (boostLevel > 0)
                            displaySupportId = boostedList[boostLevel - 1];

                        if (boostLevel == maxLevel)
                        {
                            highlight = true;
                        }
                        else
                        {
                            SupportAbility nextBoostId = boostedList[boostLevel];
                            Int32 nextAbilityId = ff9abil.GetAbilityIdFromSupportAbility(nextBoostId);
                            Int32 nextCost = ff9abil.GetSAGemCostFromPlayer(player, nextBoostId);

                            Boolean hasAccess = player.saForced.Contains(nextBoostId) || ff9abil.FF9Abil_IsMaster(player, nextAbilityId);

                            if (!hasAccess)
                            {
                                for (Int32 i = 0; i < 5; ++i)
                                {
                                    RegularItem itemId = player.equip[i];
                                    if (itemId != RegularItem.NoItem && ff9item._FF9Item_Data[itemId].ability.Contains(nextAbilityId))
                                    {
                                        hasAccess = true;
                                        break;
                                    }
                                }
                            }

                            if (hasAccess && !player.saHidden.Contains(nextBoostId))
                            {
                                if (player.cur.capa >= nextCost)
                                {
                                    highlight = true;
                                }
                            }
                        }
                    }

                    String normalName = FF9TextTool.SupportAbilityName(displaySupportId);
                    String targetName = normalName;

                    if (highlight)
                    {
                        if (_fadingTextEnabled)
                            targetName = $"[ANIM=TextRGBA,Loop,Sinus,3.0,Sinus,6.0,0.784,0.784,0.784,0.968,0.118,0.968,0.784,0.784,0.784]{normalName}[383838][HSHD]";
                        else if (_pinkTextEnabled)
                            targetName = $"[b][ffc7ff][HSHD]{normalName}[383838][HSHD][/b]";
                    }

                    if (nameLabel.rawText != targetName)
                        nameLabel.rawText = targetName;
                }
            }
        }

        private static readonly Dictionary<String, String> ConfirmClearSADialogTexts = new Dictionary<String, String>
        {
            { "US", "[IMME]Remove all Support Abilities ?\n\n[CHOO][MOVE=18,0]Yes.\n[MOVE=18,0]No." },
            { "UK", "[IMME]Remove all Support Abilities ?\n\n[CHOO][MOVE=18,0]Yes.\n[MOVE=18,0]No." },
            { "FR", "[IMME]Retirer toutes les compétences de soutien ?\n\n[CHOO][MOVE=18,0]Oui.\n[MOVE=18,0]Non." },
            { "ES", "[IMME]¿Quitar todas las habilidades de apoyo?\n\n[CHOO][MOVE=18,0]Sí.\n[MOVE=18,0]No." },
            { "GR", "[IMME]Alle Hilfs-Abilities ablegen?\n\n[CHOO][MOVE=18,0]Ja.\n[MOVE=18,0]Nein." },
            { "IT", "[IMME]Rimuovere tutte le abilità di supporto?\n\n[CHOO][MOVE=18,0]Sì.\n[MOVE=18,0]No." },
            { "JP", "[IMME]すべてのアビリティを外しますか？\n\n[CHOO][MOVE=18,0]はい。\n[MOVE=18,0]いいえ。" }
        };

        // For Memoria ? In the Update fonction

        /*private void Update()
        {
            if (!this.isActiveAndEnabled)
                return;

            if (_sortingSourceIndex != -1 && (UIManager.Input.L2Down || UIManager.Input.R2Down))
            {
                this.ResetSorter();
                return;
            }

            if (ButtonGroupState.ActiveGroup == SupportAbilityGroupButton && UIManager.Input.L2Down)
            {
                this.ClearSupportAbilities();
            }
        }

        private void ClearSupportAbilities()
        {
            PLAYER player = FF9StateSystem.Common.FF9.party.member[this.currentPartyIndex];
            if (player == null) return;

            Boolean hasChanged = false;
            List<SupportAbility> toRemove = new List<SupportAbility>();

            foreach (SupportAbility sa in player.saExtended)
            {
                if (!player.saForced.Contains(sa))
                    toRemove.Add(sa);
            }

            if (toRemove.Count == 0)
            {
                FF9Sfx.FF9SFX_Play(102);
                return;
            }

            foreach (SupportAbility sa in toRemove)
            {
                ff9abil.FF9Abil_SetEnableSA(player, sa, false);
                player.cur.capa += (UInt32)ff9abil.GetSAGemCostFromPlayer(player, sa);
                hasChanged = true;
            }

            if (hasChanged)
            {
                FF9Sfx.FF9SFX_Play(107);
                ff9play.FF9Play_Update(player);
                this.DisplaySA();
                this.DisplayCharacter(true);
                this.SetAbilityInfo(true);
            }
        }*/
    }
}
