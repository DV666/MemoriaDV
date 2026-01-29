using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Assets;
using Memoria.Data;
using Memoria.Prime;
using System;
using System.Collections.Generic;
using System.Linq;
using static Memoria.Assets.DataResources;

namespace Memoria.Data
{
    public static class VoiceHUD
    {
        private static SoundProfile _lastPlayedSound; // To prevent spamming
        private static Boolean CanReplay = false;

        private class SoundInfo
        {
            public string Type;
            public string Subtype;
            public Int32 Id;
            public CharacterId Character;
        }

        private static SoundInfo SoundData = null;

        public static void PlayHelpTextDialogAudio(string type, string subtype, Int32 Id, CharacterId character = CharacterId.NONE)
        {
            SoundData = new SoundInfo // Create the data for a potential incoming sound, when activating the help text (then invoked in ReplayHelpTextDialogAudio)
            {
                Type = type,
                Subtype = subtype,
                Id = Id,
                Character = character
            };

            if (_lastPlayedSound != null)
                ISdLibAPIProxy.Instance.SdSoundSystem_SoundCtrl_Stop(_lastPlayedSound.SoundID, 0);

            if (!Singleton<HelpDialog>.Instance.IsShown || !Configuration.VoiceActing.Enabled)
            {
                _lastPlayedSound = null;
                CanReplay = false;
                return;
            }

            // Compile the list of candidate paths for the file name
            List<String> candidates = new List<String>();
            String lang = Localization.CurrentSymbol;
            string fulltype = type;
            if (!string.IsNullOrEmpty(subtype))
                fulltype += $"_{subtype}";

            //Log.Message("fulltype = " + fulltype);

            string itemvalue = "";

            switch (fulltype)
            {
                case "Battle_RegularItem":
                case "ItemMenu_RegularItem":
                    itemvalue = ((RegularItem)Id).ToString();
                    break;
                case "ItemMenu_KeyItem":
                    itemvalue = FF9TextTool.ImportantItemName(Id);
                    break;
                case "MenuActiveAbility":
                case "Battle_ActiveAbility":
                    itemvalue = ((BattleAbilityId)Id).ToString();
                    break;
                case "Battle_Command":
                    itemvalue = ((BattleCommandId)Id).ToString();
                    break;
                case "MenuSupportAbility":
                    itemvalue = (ff9abil.GetSupportAbilityFromAbilityId(Id)).ToString();
                    break;
                case "MenuTetraMaster":
                    itemvalue = FF9TextTool.CardName((TetraMasterCardId)Id);
                    break;
            }

            // If a character is called (like in Battle or PlayerHUD)
            if (character != CharacterId.NONE)
            {
                if (!string.IsNullOrEmpty(subtype))
                    candidates.Add($"Voices/{lang}/HelpText/{character}/{type}/{subtype}/va_{Id}");
                else
                    candidates.Add($"Voices/{lang}/HelpText/{character}/{type}/va_{Id}");

                if (!string.IsNullOrEmpty(itemvalue))
                {
                    if (!string.IsNullOrEmpty(subtype))
                        candidates.Add($"Voices/{lang}/HelpText/{character}/{type}/{subtype}/va_{itemvalue}");
                    else
                        candidates.Add($"Voices/{lang}/HelpText/{character}/{type}/va_{itemvalue}");
                }
            }

            // With the "name" of the value (RegularItem, AA, SA, etc...)
            if (!string.IsNullOrEmpty(itemvalue))
            {
                if (!string.IsNullOrEmpty(subtype))
                    candidates.Add($"Voices/{lang}/HelpText/{type}/{subtype}/va_{itemvalue}");
                else
                    candidates.Add($"Voices/{lang}/HelpText/{type}/va_{itemvalue}");
            }

            // Default path
            if (!string.IsNullOrEmpty(subtype))
                candidates.Add($"Voices/{lang}/HelpText/{type}/{subtype}/va_{Id}");
            else
                candidates.Add($"Voices/{lang}/HelpText/{type}/va_{Id}");

            // Try to find one of the candidates
            Boolean found = false;
            foreach (String path in candidates)
            {
                if (AssetManager.HasAssetOnDisc($"Sounds/{path}.akb", true, true) || AssetManager.HasAssetOnDisc($"Sounds/{path}.ogg", true, false))
                {
                    int CustomID = path.GetHashCode(); // Generate a custom ID based on the path (otherwise, ID is same for an AA, with Eiko/Garnet for example)
                    SoundLib.VALog($"[READING] => type:{fulltype}, Id:{Id}, value:{itemvalue}, CustomID:{CustomID}, path:{path}");
                    _lastPlayedSound = VoicePlayer.CreateLoadThenPlayVoice(CustomID, path);
                    found = true;
                    CanReplay = true;
                    break;
                }
            }

            if (!found)
            {
                SoundLib.VALog($"[NOT FOUND] => type:{fulltype}, Id:{Id}, value:{itemvalue}, path(s):'{String.Join("', '", candidates.ToArray().Reverse().ToArray())}' (not found)");
                _lastPlayedSound = null;
                CanReplay = false;
            }
        }

        public static void ReplayHelpTextDialogAudio() // To replay the sound when disable/enable the help text dialog
        {
            if (!Singleton<HelpDialog>.Instance.IsShown || !Configuration.VoiceActing.Enabled)
                return;

            if (_lastPlayedSound == null)
            {
                if (SoundData != null)
                    PlayHelpTextDialogAudio(SoundData.Type, SoundData.Subtype, SoundData.Id, SoundData.Character);
            }
            else if (CanReplay && _lastPlayedSound != null)
            {
                ISdLibAPIProxy.Instance.SdSoundSystem_SoundCtrl_Stop(_lastPlayedSound.SoundID, 0);
                SoundLib.VoicePlayer.CreateSound(_lastPlayedSound);
                SoundLib.VoicePlayer.StartSound(_lastPlayedSound, null);
            }
        }
    }
}
