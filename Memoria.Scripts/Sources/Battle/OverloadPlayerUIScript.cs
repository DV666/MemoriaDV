using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Assets;
using Memoria.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Memoria.Scripts.TranceSeek
{
    public class OverloadedPlayerUI : IOverloadPlayerUIScript
    {
        public static PLAYER CurrentPlayer;
        private static bool _isMenuInjected = false;

        private Boolean HPColored = Configuration.Mod.FolderNames.Contains("TranceSeek/Options/ColoredHP");
        private Boolean MPColored = Configuration.Mod.FolderNames.Contains("TranceSeek/Options/ColoredMP");
        private Boolean GemColored = Configuration.Mod.FolderNames.Contains("TranceSeek/Options/ColoredGems");
        private Boolean BoostedSAIndicatorDisabled = Configuration.Mod.FolderNames.Contains("Options/BoostedSAIndicator/Disabled");

        private static Boolean _pinkTextEnabled = Configuration.Mod.FolderNames.Contains("TranceSeek/Options/BoostedSAIndicator/PinkText");
        private static Boolean _fadingTextEnabled = Configuration.Mod.FolderNames.Contains("TranceSeek/Options/BoostedSAIndicator/Fading");

        public static Dictionary<SupportAbility, String> BoostedSATargetNames = new Dictionary<SupportAbility, String>();

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
                    FF9Play_UpdateFromOverload(player);
                    dictbattle[1] = (int)(player.max.hp);
                    dictbattle[2] = (int)(player.max.mp);
                    FF9Play_UpdateFromOverload(player);
                }
            }
            else if (player.saExtended.Contains(TranceSeekSupportAbility.Anastrophe)) // SA Anastrophe
            {
                if (dictbattle[3] != 1)
                {
                    dictbattle[1] = 0;
                    dictbattle[2] = 0;
                    dictbattle[3] = 1;
                    FF9Play_UpdateFromOverload(player);
                    dictbattle[1] = (int)(player.max.hp / 2);
                    dictbattle[2] = (int)(player.max.mp / 2);
                    FF9Play_UpdateFromOverload(player);
                }
            }
            else
            {
                dictbattle[1] = 0;
                dictbattle[2] = 0;
                dictbattle[3] = 0;
            }

            ValidateBoostedSupportAbilities(player);

            if (!BoostedSAIndicatorDisabled)
                CheckFadeNextBoostedSA(player);

            return result;
        }

        public static void CheckFadeNextBoostedSA(PLAYER player)
        {
            BoostedSATargetNames.Clear();

            if (player == null || player.saExtended == null)
                return;

            AbilityUI abilityScene = PersistenSingleton<UIManager>.Instance.AbilityScene;
            if (abilityScene == null || !abilityScene.isActiveAndEnabled)
                return;

            bool isInSAMenu = ButtonGroupState.ActiveGroup == "Ability.SupportAbility";
            bool isHoveringSA = ButtonGroupState.ActiveGroup == "Ability.SubMenu" && abilityScene.MagicStonePanel.activeInHierarchy;

            if (!isInSAMenu && !isHoveringSA)
                return;

            foreach (SupportAbility equippedSA in player.saExtended)
            {
                SupportAbility baseSA = ff9abil.GetBaseAbilityFromBoostedAbility(equippedSA);

                if (BoostedSATargetNames.ContainsKey(baseSA))
                    continue;

                Int32 currentLevel = ff9abil.GetBoostedAbilityLevel(player, baseSA);
                List<SupportAbility> boostedList = ff9abil.GetBoostedAbilityList(baseSA);

                if (currentLevel >= boostedList.Count)
                    continue;

                if (currentLevel == 0 || currentLevel == 1)
                {
                    SupportAbility nextSA = boostedList[currentLevel];
                    Int32 nextAbilityId = ff9abil.GetAbilityIdFromSupportAbility(nextSA);
                    Int32 nextCost = ff9abil.GetSAGemCostFromPlayer(player, nextSA);

                    Boolean hasAccess = player.saForced.Contains(nextSA) || ff9abil.FF9Abil_IsMaster(player, nextAbilityId);

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

                    if (hasAccess && !player.saHidden.Contains(nextSA) && player.cur.capa >= nextCost)
                    {
                        Int32 highlightTier = (currentLevel == 0) ? 1 : 2;

                        Int32 boostLevelToDisplay = Math.Min(ff9abil.GetBoostedAbilityMaxLevel(player, baseSA), currentLevel);
                        SupportAbility displaySupportId = baseSA;
                        if (boostLevelToDisplay > 0)
                            displaySupportId = boostedList[boostLevelToDisplay - 1];

                        String rawName = FF9TextTool.SupportAbilityName(displaySupportId);
                        String cleanName = System.Text.RegularExpressions.Regex.Replace(rawName, @"\[/?b\]|\[[a-fA-F0-9]{6}\]|\[HSHD\]", string.Empty);
                        String targetName = cleanName;

                        if (highlightTier == 2)
                        {
                            if (_fadingTextEnabled)
                                targetName = $"[ANIM=TextRGBA,Loop,Sinus,3.0,Sinus,6.0,0.968,0.118,0.968,1.000,0.843,0.000,0.968,0.118,0.968][HSHD]{cleanName}[383838][HSHD]";
                            else if (_pinkTextEnabled)
                                targetName = $"[b][ffd700][HSHD]{cleanName}[383838][HSHD][/b]";
                        }
                        else if (highlightTier == 1)
                        {
                            if (_fadingTextEnabled)
                                targetName = $"[ANIM=TextRGBA,Loop,Sinus,3.0,Sinus,6.0,0.784,0.784,0.784,0.968,0.118,0.968,0.784,0.784,0.784][HSHD]{cleanName}[383838][HSHD]";
                            else if (_pinkTextEnabled)
                                targetName = $"[b][ffc7ff][HSHD]{cleanName}[383838][HSHD][/b]";
                        }

                        BoostedSATargetNames[baseSA] = targetName;
                    }
                }
            }
        }

        public class SABoostVisualizerHandler : MonoBehaviour
        {
            private System.Reflection.FieldInfo _saIdListField;
            private System.Reflection.FieldInfo _scrollListField;
            private bool _isInitialized = false;

            private RecycleListItem[] _cachedItems = null;
            private Dictionary<RecycleListItem, UILabel> _cachedLabels = new Dictionary<RecycleListItem, UILabel>();

            private void Awake()
            {
                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                _saIdListField = typeof(AbilityUI).GetField("saIdList", flags);
                _scrollListField = typeof(AbilityUI).GetField("supportAbilityScrollList", flags);

                if (_saIdListField != null && _scrollListField != null)
                    _isInitialized = true;
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

                    if (BoostedSATargetNames.TryGetValue(baseSupportId, out String targetName))
                    {
                        if (nameLabel.rawText != targetName)
                            nameLabel.rawText = targetName;
                    }
                    else
                    {
                        Int32 boostLevel = Math.Min(ff9abil.GetBoostedAbilityMaxLevel(player, baseSupportId), ff9abil.GetBoostedAbilityLevel(player, baseSupportId));
                        SupportAbility displaySupportId = baseSupportId;
                        if (boostLevel > 0)
                            displaySupportId = ff9abil.GetBoostedAbilityList(baseSupportId)[boostLevel - 1];

                        String defaultName = FF9TextTool.SupportAbilityName(displaySupportId);

                        if (nameLabel.rawText != defaultName)
                            nameLabel.rawText = defaultName;
                    }
                }
            }
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
                    FF9Play_UpdateFromOverload(player);

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

        private static void FF9Play_UpdateFromOverload(PLAYER play, Boolean IsPreview = false)
        {
            uint PlayGemsPreview = 0;
            if (IsPreview)
                PlayGemsPreview = play.cur.capa;

            play.max.hp = play.basis.max_hp;
            play.max.mp = play.basis.max_mp;
            play.max.capa = play.basis.max_capa;
            play.cur.capa = play.basis.capa;
            play.elem.dex = play.basis.dex;
            play.elem.str = play.basis.str;
            play.elem.mgc = play.basis.mgc;
            play.elem.wpr = play.basis.wpr;
            play.defence.PhysicalDefence = 0;
            play.defence.PhysicalEvade = 0;
            play.defence.MagicalDefence = 0;
            play.defence.MagicalEvade = 0;
            for (Int32 i = 0; i < 5; ++i)
            {
                RegularItem itemId = play.equip[i];
                if (itemId != RegularItem.NoItem)
                {
                    if (ff9item.HasItemArmor(itemId))
                    {
                        ItemDefence defParams = ff9item.GetItemArmor(itemId);
                        play.defence.PhysicalDefence += defParams.PhysicalDefence;
                        play.defence.PhysicalEvade += defParams.PhysicalEvade;
                        play.defence.MagicalDefence += defParams.MagicalDefence;
                        play.defence.MagicalEvade += defParams.MagicalEvade;
                    }
                    ItemStats equipPrivilege = ff9equip.ItemStatsData[ff9item._FF9Item_Data[itemId].bonus];
                    play.elem.dex += equipPrivilege.dex;
                    play.elem.str += equipPrivilege.str;
                    play.elem.mgc += equipPrivilege.mgc;
                    play.elem.wpr += equipPrivilege.wpr;
                }
            }
            if (play.elem.dex > ff9play.FF9PLAY_STAT_MAX[0])
                play.elem.dex = ff9play.FF9PLAY_STAT_MAX[0];
            if (play.elem.str > ff9play.FF9PLAY_STAT_MAX[1])
                play.elem.str = ff9play.FF9PLAY_STAT_MAX[1];
            if (play.elem.mgc > ff9play.FF9PLAY_STAT_MAX[2])
                play.elem.mgc = ff9play.FF9PLAY_STAT_MAX[2];
            if (play.elem.wpr > ff9play.FF9PLAY_STAT_MAX[3])
                play.elem.wpr = ff9play.FF9PLAY_STAT_MAX[3];
            if (play.defence.PhysicalDefence > ff9play.FF9PLAY_DEFPARAM_VAL_MAX)
                play.defence.PhysicalDefence = ff9play.FF9PLAY_DEFPARAM_VAL_MAX;
            if (play.defence.PhysicalEvade > ff9play.FF9PLAY_DEFPARAM_VAL_MAX)
                play.defence.PhysicalEvade = ff9play.FF9PLAY_DEFPARAM_VAL_MAX;
            if (play.defence.MagicalDefence > ff9play.FF9PLAY_DEFPARAM_VAL_MAX)
                play.defence.MagicalDefence = ff9play.FF9PLAY_DEFPARAM_VAL_MAX;
            if (play.defence.MagicalEvade > ff9play.FF9PLAY_DEFPARAM_VAL_MAX)
                play.defence.MagicalEvade = ff9play.FF9PLAY_DEFPARAM_VAL_MAX;
            play.mpCostFactor = 100;
            play.maxHpLimit = ff9play.FF9PLAY_HP_MAX;
            play.maxMpLimit = ff9play.FF9PLAY_MP_MAX;
            play.maxDamageLimit = ff9play.FF9PLAY_DAMAGE_MAX;
            play.maxMpDamageLimit = ff9play.FF9PLAY_MPDAMAGE_MAX;

            ff9play.FF9Play_SAFeature_Update(play, IsPreview);

            foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(play))
                saFeature.TriggerOnEnable(play);

            EquipmentHelper.TriggerOnEnable(play);

            if (IsPreview)
                play.cur.capa = PlayGemsPreview;
            else
                ff9abil.CalculateGemsPlayer(play);

            if (play.max.hp > play.maxHpLimit)
                play.max.hp = play.maxHpLimit;
            if (play.max.mp > play.maxMpLimit)
                play.max.mp = play.maxMpLimit;
            if (play.cur.hp > play.max.hp)
                play.cur.hp = play.max.hp;
            if (play.cur.mp > play.max.mp)
                play.cur.mp = play.max.mp;
        }

        public static void ValidateBoostedSupportAbilities(PLAYER player)
        {
            if (player == null || player.saExtended == null)
                return;

            List<SupportAbility> toDisable = new List<SupportAbility>();

            foreach (SupportAbility sa in player.saExtended)
            {
                if (player.saForced.Contains(sa))
                    continue;

                List<SupportAbility> hierarchy = ff9abil.GetHierarchyFromAnySA(sa);
                if (hierarchy != null && hierarchy.Count > 0)
                {
                    Int32 currentIndex = hierarchy.IndexOf(sa);
                    Boolean isChainValid = true;

                    for (Int32 i = 0; i <= currentIndex; i++)
                    {
                        SupportAbility reqSA = hierarchy[i];
                        if (!HasSupportAbilityAccess(player, reqSA))
                        {
                            if (!toDisable.Contains(reqSA))
                                toDisable.Add(reqSA);
                            isChainValid = false;
                            break;
                        }
                    }

                    if (!isChainValid)
                    {
                        for (Int32 j = currentIndex; j < hierarchy.Count; j++)
                        {
                            SupportAbility saToDisable = hierarchy[j];
                            if (!toDisable.Contains(saToDisable))
                                toDisable.Add(saToDisable);
                        }
                    }
                }
            }

            if (toDisable.Count > 0)
            {
                foreach (SupportAbility sa in toDisable)
                {
                    Memoria.Prime.Log.Message($"[Trance Seek] Disabling Support Ability: {sa} for player {player.Name} since hierarchy not respected anymore.");
                    if (player.saExtended.Contains(sa))
                        ff9abil.FF9Abil_SetEnableSA(player, sa, false);
                }

                ff9abil.CalculateGemsPlayer(player);
            }
        }

        private static Boolean HasSupportAbilityAccess(PLAYER player, SupportAbility sa)
        {
            if (player.saForced.Contains(sa))
                return true;

            Int32 abilityId = ff9abil.GetAbilityIdFromSupportAbility(sa);

            if (ff9abil.FF9Abil_IsMaster(player, abilityId))
                return true;

            for (Int32 i = 0; i < 5; ++i)
            {
                RegularItem itemId = player.equip[i];
                if (itemId != RegularItem.NoItem && ff9item._FF9Item_Data[itemId].ability.Contains(abilityId))
                    return true;
            }

            return false;
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
