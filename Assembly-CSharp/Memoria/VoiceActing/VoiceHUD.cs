using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Assets;
using Memoria.Data;
using Memoria.Prime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Memoria.Data
{
    public static class VoiceHUD
    {
        public static string CurrentObjectSelected;
        private static SoundProfile _lastPlayedSound;

        public static void UpdateHelpDialogAudio()
        {
            if (!Configuration.VoiceActing.Enabled)
                return;

            Log.Message("CurrentObjectSelected = " + CurrentObjectSelected);
            Log.Message("ButtonGroupState.ActiveGroup = " + ButtonGroupState.ActiveGroup);

            if (!Singleton<HelpDialog>.Instance.IsShown || string.IsNullOrEmpty(CurrentObjectSelected))
            {
                CurrentObjectSelected = "";
                return;
            }

            int idToPlay = GetId(CurrentObjectSelected);
            Log.Message("idToPlay = " + idToPlay);
            if (idToPlay == -1) return;

            HelpTextType type = HelpTextType.None;

            switch (ButtonGroupState.ActiveGroup)
            {
                case BattleHUD.CommandGroupButton:
                    type = HelpTextType.BattleCommand;
                    break;
                case BattleHUD.ItemGroupButton:
                    type = HelpTextType.BattleItem;
                    break;
                case "Ability.ActionAbility":
                    type = HelpTextType.ActiveAbility;
                    break;

                case "Ability.SupportAbility":
                    type = HelpTextType.SupportAbility;
                    break;

                case ItemUI.ItemGroupButton:
                    type = HelpTextType.MenuItem;
                    break;
                case ItemUI.KeyItemGroupButton:
                    type = HelpTextType.KeyItem;
                    break;
                case "Ability.SubMenu":
                case "MainMenu.SubMenu":
                case "MainMenu.Character":
                case "MainMenu.Order":
                case ItemUI.SubMenuGroupButton:
                case ItemUI.ItemArrangeGroupButton:
                case ItemUI.ArrangeMenuGroupButton: // Don't think it's a good idea to use this... x_x (or maybe juste invoke a sound when activating ?)
                    type = HelpTextType.MainMenu;
                    break;
                case "Card.Card":
                case "Card.Choice":
                    type = HelpTextType.TetraMaster;
                    break;
            }

            PlayHelpTextDialogAudio(type, idToPlay, GetCharId(CurrentObjectSelected));
        }

        public static void PlayHelpTextDialogAudio(HelpTextType type, Int32 Id, CharacterId character)
        {
            // Compile the list of candidate paths for the file name
            List<String> candidates = new List<String>();
            String lang = Localization.CurrentSymbol;
            string itemvalue = "";

            switch (type)
            {
                case HelpTextType.BattleItem:
                case HelpTextType.MenuItem:
                    itemvalue = ((RegularItem)Id).ToString();
                    break;
                case HelpTextType.KeyItem:
                    itemvalue = FF9TextTool.ImportantItemName(Id);
                    break;
                case HelpTextType.ActiveAbility:
                    itemvalue = ((BattleAbilityId)Id).ToString();
                    break;
                case HelpTextType.BattleCommand:
                    itemvalue = ((BattleCommandId)Id).ToString();
                    break;
                case HelpTextType.SupportAbility:
                    itemvalue = (ff9abil.GetSupportAbilityFromAbilityId(Id)).ToString();
                    break;

                case HelpTextType.TetraMaster:
                    itemvalue = FF9TextTool.CardName((TetraMasterCardId)Id);
                    break;
                default:
                    itemvalue = Id.ToString();
                    break;
            }

            // If a character is called (like in Battle or PlayerHUD)
            if (character != CharacterId.NONE)
            {
                candidates.Add($"Voices/{lang}/HelpText/{character}/{type}/va_{Id}");
                if (!string.IsNullOrEmpty(itemvalue))
                    candidates.Add($"Voices/{lang}/HelpText/{character}/{type}/va_{itemvalue}");
            }

            // Default path
            candidates.Add($"Voices/{lang}/HelpText/{type}/va_{Id}");

            // With the "name" of the value (RegularItem, AA, SA, etc...)
            if (!string.IsNullOrEmpty(itemvalue))
                candidates.Add($"Voices/{lang}/HelpText/{type}/va_{itemvalue}");

            // Try to find one of the candidates
            Boolean found = false;
            foreach (String path in candidates)
            {
                if (AssetManager.HasAssetOnDisc($"Sounds/{path}.akb", true, true) || AssetManager.HasAssetOnDisc($"Sounds/{path}.ogg", true, false))
                {
                    if (_lastPlayedSound != null)
                        ISdLibAPIProxy.Instance.SdSoundSystem_SoundCtrl_Stop(_lastPlayedSound.SoundID, 0);

                    int CustomID = path.GetHashCode(); // Generate a custom ID based on the path (otherwise, ID is same for an AA, with Eiko/Garnet for example)
                    SoundLib.VALog($"[READING] => type:{type}, Id:{Id}, CustomID:{CustomID}, value:{itemvalue}, path:{path}");
                    _lastPlayedSound = VoicePlayer.CreateLoadThenPlayVoice(CustomID, path);
                    found = true;
                    break;
                }
            }

            if (!found)
                SoundLib.VALog($"[NOT FOUND] => type:{type}, Id:{Id}, value:{itemvalue}, path(s):'{String.Join("', '", candidates.ToArray().Reverse().ToArray())}' (not found)");
        }

        private static int GetId(string input)
        {
            int lastUnderscoreIndex = input.LastIndexOf('_');
            if (lastUnderscoreIndex >= 0 && lastUnderscoreIndex < input.Length - 1)
            {
                string idString = input.Substring(lastUnderscoreIndex + 1);
                if (int.TryParse(idString, out int resultId))
                    return resultId;
            }
            return -1;
        }

        private static CharacterId GetCharId(string input)
        {
            if (string.IsNullOrEmpty(input)) return CharacterId.NONE;

            string key = "CHAR";
            int keyIndex = input.IndexOf(key);

            if (keyIndex != -1)
            {
                int startValueIndex = keyIndex + key.Length;
                int nextUnderscoreIndex = input.IndexOf('_', startValueIndex);

                if (nextUnderscoreIndex > startValueIndex)
                {
                    int length = nextUnderscoreIndex - startValueIndex;
                    string extractedValue = input.Substring(startValueIndex, length);

                    try
                    {
                        return (CharacterId)Enum.Parse(typeof(CharacterId), extractedValue, true);
                    }
                    catch
                    {
                        Log.Message("[VOICEHUD] Can't parse GetCharId ?");
                    }
                }
            }
            return CharacterId.NONE;
        }

        public enum HelpTextType
        {
            None,
            ActiveAbility,
            SupportAbility,
            BattleCommand,
            BattleItem,
            MenuItem,
            KeyItem,
            MainMenu,
            TetraMaster
        }
    }
}
