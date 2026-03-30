using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UIManager;

namespace Memoria.Scripts.Battle
{
    public class OverloadedPlayerUI : IOverloadPlayerUIScript
    {
        public static PLAYER CurrentPlayer;
        private static bool _isMenuInjected = false;

        public IOverloadPlayerUIScript.Result UpdatePointStatus(PLAYER player)
        {
            if (!_isMenuInjected)
            {
                GameObject go = new GameObject("Mod_SAClearHandler");
                go.AddComponent<SAClearInputHandler>();
                UnityEngine.Object.DontDestroyOnLoad(go); // Pour qu'il survive aux changements de menus/scènes
                _isMenuInjected = true;
            }

            Boolean HPColored = Configuration.Mod.FolderNames.Contains("TranceSeek/ColoredHP");
            Boolean MPColored = Configuration.Mod.FolderNames.Contains("TranceSeek/ColoredMP");
            Boolean GemColored = Configuration.Mod.FolderNames.Contains("TranceSeek/ColoredGems");

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
                    {
                        result.ColorMagicStone = FF9TextTool.Gray;
                        return result;
                    }
                }

                if (player.cur.capa == 0)
                {
                    result.ColorMagicStone = FF9TextTool.White;
                    return result;
                }

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

            if (player.saExtended.Contains((SupportAbility)1132)) // SA Anastrophe+
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
            else if (player.saExtended.Contains((SupportAbility)132)) // SA Anastrophe
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

            return result;
        }

        public class SAClearInputHandler : MonoBehaviour
        {
            private bool _isActionTriggered = false;

            private void Update()
            {
                if (ButtonGroupState.ActiveGroup != "Ability.SupportAbility")
                    return;

                if (PersistenSingleton<HonoInputManager>.Instance.IsInputDown(Control.LeftTrigger) || UIManager.Input.GetKey(Control.LeftTrigger))
                {
                    if (!_isActionTriggered)
                    {
                        ClearSupportAbilities();
                        _isActionTriggered = true;
                    }
                }
                else
                {
                    _isActionTriggered = false;
                }
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

        // For Memoria ? In the Update fonction

        /*private void Update()
        {
            if (!this.isActiveAndEnabled)
                return;

            // Logique existante pour le trier (Sorter)
            if (_sortingSourceIndex != -1 && (UIManager.Input.L2Down || UIManager.Input.R2Down))
            {
                this.ResetSorter();
                return;
            }

            // NOTRE NOUVELLE LOGIQUE : Si on est dans le menu SA et qu'on fait L2
            if (ButtonGroupState.ActiveGroup == SupportAbilityGroupButton && UIManager.Input.L2Down)
            {
                this.ClearSupportAbilities();
            }
        }

        private void ClearSupportAbilities()
        {
            // On récupère le personnage actuellement affiché
            PLAYER player = FF9StateSystem.Common.FF9.party.member[this.currentPartyIndex];
            if (player == null) return;

            Boolean hasChanged = false;
            List<SupportAbility> toRemove = new List<SupportAbility>();

            // On liste les SA équipées, en ignorant celles "Forcées" par l'équipement
            foreach (SupportAbility sa in player.saExtended)
            {
                if (!player.saForced.Contains(sa))
                    toRemove.Add(sa);
            }

            // Optionnel : S'il n'y a rien à enlever, on fait un bruit d'erreur (Buzzer)
            if (toRemove.Count == 0)
            {
                FF9Sfx.FF9SFX_Play(102);
                return;
            }

            // On déséquipe les SA et on rembourse les gemmes manuellement (comme le fait déjà Memoria)
            foreach (SupportAbility sa in toRemove)
            {
                ff9abil.FF9Abil_SetEnableSA(player, sa, false);
                player.cur.capa += (UInt32)ff9abil.GetSAGemCostFromPlayer(player, sa);
                hasChanged = true;
            }

            if (hasChanged)
            {
                // Bruitage d'équipement
                FF9Sfx.FF9SFX_Play(107);

                // Mise à jour des stats du joueur
                ff9play.FF9Play_Update(player);

                // Rafraîchissement NOUVELLE GÉNÉRATION : Appels directs aux méthodes de la classe !
                this.DisplaySA();              // Recrée la liste et met à jour les icônes de pierres
                this.DisplayCharacter(true);   // Met à jour la jauge de gemmes du perso en haut à droite
                this.SetAbilityInfo(true);     // Met à jour l'entête
            }
        }*/
    }
}
