using System;
using System.Collections.Generic;
using Assets.Sources.Scripts.UI.Common;
using Assets.Scripts.Common;
using UnityEngine;

namespace Memoria.Assets
{
    public static class Localization
    {
        internal static readonly LanguageMap Provider;

        private static Boolean useSecondaryLanguage = false;
        private static String loadedSecondarySymbol = String.Empty;

        static Localization()
        {
            Provider = new LanguageMap();
            Provider.Broadcast();
        }

        public static ICollection<String> KnownLanguages => Provider.KnownLanguages;
        public static Dictionary<String, Dictionary<String, String>> DefaultDictionary => _defaultDictionary;

        /// <summary>The language name ("English(US)", "Japanese", "Spanish"...) of the currently displayed language</summary>
        public static String CurrentDisplayLanguage => useSecondaryLanguage ? Provider.SymbolToLanguage(Configuration.Lang.DualLanguage) : Provider.CurrentLanguage;
        /// <summary>The language symbol ("US", "JP", "ES"...) of the currently displayed language</summary>
        public static String CurrentDisplaySymbol => useSecondaryLanguage ? Configuration.Lang.DualLanguage : Provider.CurrentSymbol;
        /// <summary>The language name ("English(US)", "Japanese", "Spanish"...) of the current primary language</summary>
        public static String CurrentLanguage => Provider.CurrentLanguage;
        /// <summary>The language symbol ("US", "JP", "ES"...) of the current primary language</summary>
        public static String CurrentSymbol => Provider.CurrentSymbol;

        public static void SetCurrentLanguage(String lang, MonoBehaviour caller = null, Action callback = null, Boolean updateDatabase = true)
        {
            FF9StateSystem.Settings.CurrentLanguage = lang;
            Provider.SelectLanguage(lang);
            if (caller != null && updateDatabase)
            {
                UIManager.Field.InitializeATEText();
                caller.StartCoroutine(PersistenSingleton<FF9TextTool>.Instance.UpdateTextLocalization(callback));
            }
            UIRoot.Broadcast("OnLocalize");
        }

        public static Boolean UseSecondaryLanguage
        {
            get => useSecondaryLanguage;
            set
            {
                if (Configuration.Export.Enabled || PersistenSingleton<UIManager>.Instance.TitleScene?.IsJustLaunch == true)
                    value = false;
                if (useSecondaryLanguage == value)
                    return;
                useSecondaryLanguage = value;
                if (value && loadedSecondarySymbol != Configuration.Lang.DualLanguage)
                {
                    String symbol = Configuration.Lang.DualLanguage;
                    String lang = Provider.SymbolToLanguage(symbol);
                    loadedSecondarySymbol = symbol;
                    Provider.SelectSecondaryLanguage(lang);
                    FF9TextTool.LoadSecondaryLanguage(symbol);
                }
                if (Configuration.Lang.DualLanguageMode == 1)
                    UIRoot.Broadcast("OnLocalize");
            }
        }

        public static String Get(String key)
        {
            String str = Provider.Get(key, UseSecondaryLanguage);
            if (key == "StatusDetailHelp") // Fix: Status help in StatusUI
                str = str.Replace("[YADD=4]", "").Replace("[YSUB=4]", "");
            return str;
        }

        public static String ProcessEntryForCSVWriting(String entry)
        {
            entry = entry.Replace("\n", "\\n");
            if (entry.Contains("\"") || entry.Contains(","))
            {
                entry = entry.Replace("\"", "\"\"");
                entry = "\"" + entry + "\"";
            }
            return entry;
        }

        /// <summary>Might be useful in the future to base a translation mod on another language</summary>
        public static String GetFallbackSymbol()
        {
            return "US";
        }

        /// <summary>Deprecated: use "Localization.CurrentDisplaySymbol" or "Localization.CurrentSymbol" instead</summary>
        public static String GetSymbol()
        {
            return CurrentDisplaySymbol;
        }

        /// <summary>For keys not present in vanilla</summary>
        public static String GetWithDefault(String key)
        {
            String value = Provider.Get(key, UseSecondaryLanguage);
            if (!String.Equals(value, key))
                return value;
            if (_defaultDictionary.TryGetValue(key, out Dictionary<String, String> defaultValue))
            {
                if (defaultValue.TryGetValue(CurrentDisplaySymbol, out value))
                    return value;
                else if (defaultValue.TryGetValue(GetFallbackSymbol(), out value))
                    return value;
            }
            return key;
        }

        private static Dictionary<String, Dictionary<String, String>> _defaultDictionary = new Dictionary<String, Dictionary<String, String>>()
        {
            // The base reading direction of the language
            { LanguageMap.ReadingDirectionKey, new Dictionary<String, String>()
                {
                    { "US", UnicodeBIDI.DIRECTION_NAME_LEFT_TO_RIGHT },
                    { "UK", UnicodeBIDI.DIRECTION_NAME_LEFT_TO_RIGHT },
                    { "JP", UnicodeBIDI.DIRECTION_NAME_LEFT_TO_RIGHT },
                    { "ES", UnicodeBIDI.DIRECTION_NAME_LEFT_TO_RIGHT },
                    { "FR", UnicodeBIDI.DIRECTION_NAME_LEFT_TO_RIGHT },
                    { "GR", UnicodeBIDI.DIRECTION_NAME_LEFT_TO_RIGHT },
                    { "IT", UnicodeBIDI.DIRECTION_NAME_LEFT_TO_RIGHT }
                }
            },
            // The digit shapes of the language
            { LanguageMap.DigitShapesKey, new Dictionary<String, String>()
                {
                    { "US", UnicodeBIDI.DIGIT_SHAPES_LATIN },
                    { "UK", UnicodeBIDI.DIGIT_SHAPES_LATIN },
                    { "JP", UnicodeBIDI.DIGIT_SHAPES_LATIN },
                    { "ES", UnicodeBIDI.DIGIT_SHAPES_LATIN },
                    { "FR", UnicodeBIDI.DIGIT_SHAPES_LATIN },
                    { "GR", UnicodeBIDI.DIGIT_SHAPES_LATIN },
                    { "IT", UnicodeBIDI.DIGIT_SHAPES_LATIN }
                }
            },
            // Language name in the title menu's button
            { "NameShort", new Dictionary<String, String>()
                {
                    { "US", "US" },
                    { "UK", "UK" },
                    { "JP", "JP" },
                    { "ES", "ES" },
                    { "FR", "FR" },
                    { "GR", "GR" },
                    { "IT", "IT" }
                }
            },
            // Gil formatiing in multiple menu
            { "GilSymbol", new Dictionary<String, String>()
                {
                    { "US", "%[YSUB=1.3][sub]G" },
                    { "UK", "%[YSUB=1.3][sub]G" },
                    { "JP", "%[YSUB=1.3][sub]G" },
                    { "ES", "%[YSUB=1.3][sub]G" },
                    { "FR", "%[YSUB=1.3][sub]G" },
                    { "GR", "%[YSUB=1.3][sub]G" },
                    { "IT", "%[YSUB=1.3][sub]G" }
                }
            },
            // Number of points for Tetra Master
            { "CardPoints", new Dictionary<String, String>()
                {
                    { "US", "%p" },
                    { "UK", "%p" },
                    { "JP", "%p" },
                    { "ES", "%p" },
                    { "FR", "%p" },
                    { "GR", "%p" },
                    { "IT", "%p" }
                }
            },
            { "AutoDiscardHelp", new Dictionary<String, String>()
                {
                    { "US", "Discard all unnecessary cards." },
                    { "UK", "Discard all unnecessary cards." },
                    { "JP", "不要なカードをすべて捨てます。" },
                    { "ES", "Descarta todas las cartas innecesarias." },
                    { "FR", "Jeter les cartes superflues." },
                    { "GR", "Werfen Sie alle unnötigen Karten weg." },
                    { "IT", "Scarta tutte le carte non necessarie." }
                }
            },
            // Panel caption for magic stones (used to show "MP" in the Ability menu instead, since PSX)
            { "MagicStoneCaption", new Dictionary<String, String>()
                {
                    { "US", "MG ST" },
                    { "UK", "MG ST" },
                    { "JP", "魔石力" },
                    { "ES", "PIEDRAS" },
                    { "FR", "MAGIK" },
                    { "GR", "MG ST" },
                    { "IT", "PIETRE" }
                }
            },
            // New options in the config menu
            { "SoundVolume", new Dictionary<String, String>()
                {
                    { "US", "Sound Volume" },
                    { "UK", "Sound Volume" },
                    { "JP", "サウンド音量" },
                    { "ES", "Volumen de sonidos" },
                    { "FR", "Volume des sons" },
                    { "GR", "Lautstärke der Geräusche" },
                    { "IT", "Volume dei suoni" }
                }
            },
            { "MusicVolume", new Dictionary<String, String>()
                {
                    { "US", "Music Volume" },
                    { "UK", "Music Volume" },
                    { "JP", "音楽の音量" },
                    { "ES", "Volumen de la música" },
                    { "FR", "Volume de la musique" },
                    { "GR", "Musiklautstärke" },
                    { "IT", "Volume della musica" }
                }
            },
            { "MovieVolume", new Dictionary<String, String>()
                {
                    { "US", "Movie Volume" },
                    { "UK", "Movie Volume" },
                    { "JP", "動画の音量" },
                    { "ES", "Volumen de la película" },
                    { "FR", "Volume des films" },
                    { "GR", "Filmlautstärke" },
                    { "IT", "Volume di film" }
                }
            },
            { "VoiceVolume", new Dictionary<String, String>()
                {
                    { "US", "Voice Volume" },
                    { "UK", "Voice Volume" },
                    { "JP", "音声音量" },
                    { "ES", "Volumen de voz" },
                    { "FR", "Volume des voix" },
                    { "GR", "Stimmen Lautstärke" },
                    { "IT", "Volume della voce" }
                }
            },
            { "ATBModeNormal", new Dictionary<String, String>()
                {
                    { "US", "ATB: Normal" },
                    { "UK", "ATB: Normal" },
                    { "JP", "ATBモード: 通常" },
                    { "ES", "ATB: Normal" },
                    { "FR", "ATB : Normal" },
                    { "GR", "ATB: Normaler" },
                    { "IT", "ATB: Normale" }
                }
            },
            { "ATBModeFast", new Dictionary<String, String>()
                {
                    { "US", "ATB: Fast" },
                    { "UK", "ATB: Fast" },
                    { "JP", "ATBモード: 高速" },
                    { "ES", "ATB: Rápido" },
                    { "FR", "ATB : Rapide" },
                    { "GR", "ATB: Schneller" },
                    { "IT", "ATB: Veloce" }
                }
            },
            { "ATBModeTurnBased", new Dictionary<String, String>()
                {
                    { "US", "ATB: Turn-Based" },
                    { "UK", "ATB: Turn-Based" },
                    { "JP", "ATBモード: ターン制" },
                    { "ES", "ATB: Por Turnos" },
                    { "FR", "ATB : Par Tour" },
                    { "GR", "ATB: Turn" },
                    { "IT", "ATB: A Turni" }
                }
            },
            { "ATBModeDynamic", new Dictionary<String, String>()
                {
                    { "US", "ATB: Dynamic" },
                    { "UK", "ATB: Dynamic" },
                    { "JP", "ATBモード: 動的" },
                    { "ES", "ATB: Dinámico" },
                    { "FR", "ATB: Dynamique" },
                    { "GR", "ATB: Dynamischer" },
                    { "IT", "ATB: Dinamica" }
                }
            },
            { "AutoText", new Dictionary<String, String>()
                {
                    { "US", "Auto Text" },
                    { "UK", "Auto Text" },
                    { "JP", "自動テキスト" },
                    { "ES", "Texto Automático" },
                    { "FR", "Texte Automatique" },
                    { "GR", "Autotext" },
                    { "IT", "Testo Automatico" }
                }
            },
            // Black Jack labels
            { "BlackJackBankroll", new Dictionary<String, String>()
                {
                    { "US", "BANKROLL" },
                    { "UK", "BANKROLL" },
                    { "JP", "財源" },
                    { "ES", "EFECTIVO" },
                    { "FR", "FONDS" },
                    { "GR", "BANK" },
                    { "IT", "CONTANTE" }
                }
            },
            { "BlackJackWager", new Dictionary<String, String>()
                {
                    { "US", "WAGER" },
                    { "UK", "WAGER" },
                    { "JP", "賭け" },
                    { "ES", "APUESTA" },
                    { "FR", "MISE" },
                    { "GR", "WETTE" },
                    { "IT", "PUNTATA" }
                }
            },
            { "BlackJackStand", new Dictionary<String, String>()
                {
                    { "US", "STAND" },
                    { "UK", "STAND" },
                    { "JP", "スタンド" },
                    { "ES", "PLANTARSE" },
                    { "FR", "RESTE" },
                    { "GR", "STEHT" },
                    { "IT", "STARE" }
                }
            },
            { "BlackJackHit", new Dictionary<String, String>()
                {
                    { "US", "HIT" },
                    { "UK", "HIT" },
                    { "JP", "ヒット" },
                    { "ES", "PEDIR" },
                    { "FR", "CARTE" },
                    { "GR", "KARTE" },
                    { "IT", "CARTA" }
                }
            },
            { "BlackJackDouble", new Dictionary<String, String>()
                {
                    { "US", "DOUBLE" },
                    { "UK", "DOUBLE" },
                    { "JP", "ダブルダウン" },
                    { "ES", "DOBLAR" },
                    { "FR", "DOUBLE" },
                    { "GR", "VERDOPPELN" },
                    { "IT", "RADDOPPIO" }
                }
            },
            { "BlackJackSplit", new Dictionary<String, String>()
                {
                    { "US", "SPLIT" },
                    { "UK", "SPLIT" },
                    { "JP", "スプリット" },
                    { "ES", "SEPARAR" },
                    { "FR", "SPLIT" },
                    { "GR", "TEILEN" },
                    { "IT", "DIVISIONE" }
                }
            },
            // Manual trance command help
            { "TranceCommandHelp", new Dictionary<String, String>()
                {
                    { "US", "Activates the trance mode." },
                    { "UK", "Activates the trance mode." },
                    { "JP", "トランス状態を活性化する。" },
                    { "ES", "Activa el trance." },
                    { "FR", "Active la transe." },
                    { "GR", "Aktiviert die Trance." },
                    { "IT", "Attiva la trance." }
                }
            },
            // Mix battle message
            { "FailedMixMessage", new Dictionary<String, String>()
                {
                    { "US", "The combination failed!" },
                    { "UK", "The combination failed!" },
                    { "JP", "調合失敗" },
                    { "ES", "La mezcla falló!" },
                    { "FR", "Le mélange a échoué !" },
                    { "GR", "Mischung fehlgeschlagen!" },
                    { "IT", "La miscela non è riuscita!" }
                }
            },
            // New parameters possibly displayed by Scan in battles
            { "ElementWeak", new Dictionary<String, String>()
                {
                    { "US", "Weak to [FFCC00]%[FFFFFF]" },
                    { "UK", "Weak to [FFCC00]%[FFFFFF]" },
                    { "JP", "[FFCC00]%[FFFFFF]属性に弱い" },
                    { "ES", "Vulnerable al elemento [FFCC00]%[FFFFFF]" },
                    { "FR", "Sensible à [FFCC00]%[FFFFFF]" },
                    { "GR", "Anfällig gegen [FFCC00]%[FFFFFF]-Element" },
                    { "IT", "Debole all’elemento [FFCC00]%[FFFFFF]" }
                }
            },
            { "ElementResist", new Dictionary<String, String>()
                {
                    { "US", "Resistant to [FFCC00]%[FFFFFF]" },
                    { "UK", "Resistant to [FFCC00]%[FFFFFF]" },
                    { "JP", "[FFCC00]%[FFFFFF]属性を半減" },
                    { "ES", "Resistente al elemento [FFCC00]%[FFFFFF]" },
                    { "FR", "Résiste à [FFCC00]%[FFFFFF]" },
                    { "GR", "Resistent gegen [FFCC00]%[FFFFFF]-Element" },
                    { "IT", "Resistente all’elemento [FFCC00]%[FFFFFF]" }
                }
            },
            { "ElementImmune", new Dictionary<String, String>()
                {
                    { "US", "Immune to [FFCC00]%[FFFFFF]" },
                    { "UK", "Immune to [FFCC00]%[FFFFFF]" },
                    { "JP", "[FFCC00]%[FFFFFF]属性を無効" },
                    { "ES", "Inmune al elemento [FFCC00]%[FFFFFF]" },
                    { "FR", "Immunisé à [FFCC00]%[FFFFFF]" },
                    { "GR", "Immun gegen [FFCC00]%[FFFFFF]-Element" },
                    { "IT", "Immune all’elemento [FFCC00]%[FFFFFF]" }
                }
            },
            { "ElementAbsorb", new Dictionary<String, String>()
                {
                    { "US", "Absorb [FFCC00]%[FFFFFF]" },
                    { "UK", "Absorb [FFCC00]%[FFFFFF]" },
                    { "JP", "[FFCC00]%[FFFFFF]属性を吸収" },
                    { "ES", "Absorber el elemento [FFCC00]%[FFFFFF]" },
                    { "FR", "Absorbe l’élément [FFCC00]%[FFFFFF]" },
                    { "GR", "Absorbiert [FFCC00]%[FFFFFF]-Element" },
                    { "IT", "Assorbire l’elemento [FFCC00]%[FFFFFF]" }
                }
            },
            { "StatusImmune", new Dictionary<String, String>()
                {
                    { "US", "Immune: [FFCC00]%[FFFFFF]" },
                    { "UK", "Immune: [FFCC00]%[FFFFFF]" },
                    { "JP", "耐性 [FFCC00]%[FFFFFF]" },
                    { "ES", "Inmune: [FFCC00]%[FFFFFF]" },
                    { "FR", "Immunité : [FFCC00]%[FFFFFF]" },
                    { "GR", "Immunität [FFCC00]%[FFFFFF]" },
                    { "IT", "Immune: [FFCC00]%[FFFFFF]" }
                }
            },
            { "StatusAuto", new Dictionary<String, String>()
                {
                    { "US", "Auto: [FFCC00]%[FFFFFF]" },
                    { "UK", "Auto: [FFCC00]%[FFFFFF]" },
                    { "JP", "永続 [FFCC00]%[FFFFFF]" },
                    { "ES", "Auto: [FFCC00]%[FFFFFF]" },
                    { "FR", "Auto : [FFCC00]%[FFFFFF]" },
                    { "GR", "Auto [FFCC00]%[FFFFFF]" },
                    { "IT", "Sempre: [FFCC00]%[FFFFFF]" }
                }
            },
            { "StatusResist", new Dictionary<String, String>()
                {
                    { "US", "Resist: [FFCC00]%[FFFFFF]" },
                    { "UK", "Resist: [FFCC00]%[FFFFFF]" },
                    { "JP", "免疫 [FFCC00]%[FFFFFF]" },
                    { "ES", "Resistente: [FFCC00]%[FFFFFF]" },
                    { "FR", "Résistance : [FFCC00]%[FFFFFF]" },
                    { "GR", "Widersteht [FFCC00]%[FFFFFF]" },
                    { "IT", "Resistente: [FFCC00]%[FFFFFF]" }
                }
            },
            { "AttackList", new Dictionary<String, String>()
                {
                    { "US", "[b]Abilities[/b]" },
                    { "UK", "[b]Abilities[/b]" },
                    { "JP", "[b]アビリティ[/b]" },
                    { "ES", "[b]Habilidades[/b]" },
                    { "FR", "[b]Compétences[/b]" },
                    { "GR", "[b]Fähigkeiten[/b]" },
                    { "IT", "[b]Abilità[/b]" }
                }
            },
            { "StatusPetrify", new Dictionary<String, String>()
                {
                    { "US", "Petrify" },
                    { "UK", "Petrify" },
                    { "JP", "石化" },
                    { "ES", "Piedra" },
                    { "FR", "Fossile" },
                    { "GR", "Stein" },
                    { "IT", "Pietra" }
                }
            },
            { "StatusVenom", new Dictionary<String, String>()
                {
                    { "US", "Venom" },
                    { "UK", "Venom" },
                    { "JP", "猛毒" },
                    { "ES", "Veneno+" },
                    { "FR", "Toxique" },
                    { "GR", "Toxitus" },
                    { "IT", "Veleno" }
                }
            },
            { "StatusVirus", new Dictionary<String, String>()
                {
                    { "US", "Virus" },
                    { "UK", "Virus" },
                    { "JP", "ウイルス" },
                    { "ES", "Virus" },
                    { "FR", "Virus" },
                    { "GR", "Infektion" },
                    { "IT", "Virus" }
                }
            },
            { "StatusSilence", new Dictionary<String, String>()
                {
                    { "US", "Silence" },
                    { "UK", "Silence" },
                    { "JP", "沈黙" },
                    { "ES", "Mudez" },
                    { "FR", "Mutisme" },
                    { "GR", "Schweigen" },
                    { "IT", "Silenzio" }
                }
            },
            { "StatusBlind", new Dictionary<String, String>()
                {
                    { "US", "Darkness" },
                    { "UK", "Darkness" },
                    { "JP", "暗闇" },
                    { "ES", "Ceguera" },
                    { "FR", "Obscurité" },
                    { "GR", "Blind" },
                    { "IT", "Blind" }
                }
            },
            { "StatusTrouble", new Dictionary<String, String>()
                {
                    { "US", "Trouble" },
                    { "UK", "Trouble" },
                    { "JP", "迷惑" },
                    { "ES", "Molestia" },
                    { "FR", "Embrouilles" },
                    { "GR", "Neurose" },
                    { "IT", "Disturbo" }
                }
            },
            { "StatusZombie", new Dictionary<String, String>()
                {
                    { "US", "Zombie" },
                    { "UK", "Zombie" },
                    { "JP", "ゾンビ" },
                    { "ES", "Zombi" },
                    { "FR", "Zombie" },
                    { "GR", "Zombie" },
                    { "IT", "Zombie" }
                }
            },
            { "StatusEasyKill", new Dictionary<String, String>()
                {
                    { "US", "EasyKill" },
                    { "UK", "EasyKill" },
                    { "JP", "EasyKill" },
                    { "ES", "EasyKill" },
                    { "FR", "EasyKill" },
                    { "GR", "EasyKill" },
                    { "IT", "EasyKill" }
                }
            },
            { "StatusDeath", new Dictionary<String, String>()
                {
                    { "US", "Death" },
                    { "UK", "Death" },
                    { "JP", "死" },
                    { "ES", "Morte" },
                    { "FR", "Mort" },
                    { "GR", "Muerte" },
                    { "IT", "TOT" }
                }
            },
            { "CureDeath", new Dictionary<String, String>()
                {
                    { "US", "KO" },
                    { "UK", "KO" },
                    { "JP", "戦闘不能" },
                    { "ES", "Fuera de combate (K. O.)" },
                    { "FR", "KO" },
                    { "GR", "Kampfunfähigkeit" },
                    { "IT", "K.O." }
                }
            },
            { "StatusLowHP", new Dictionary<String, String>()
                {
                    { "US", "Low HP" },
                    { "UK", "Low HP" },
                    { "JP", "HPが低い" },
                    { "ES", "HP bajo" },
                    { "FR", "HP faibles" },
                    { "GR", "Niedrige HP" },
                    { "IT", "HP bassi" }
                }
            },
            { "StatusConfuse", new Dictionary<String, String>()
                {
                    { "US", "Confusion" },
                    { "UK", "Confusion" },
                    { "JP", "混乱" },
                    { "ES", "Confusión" },
                    { "FR", "Confusion" },
                    { "GR", "Konfus" },
                    { "IT", "Confusione" }
                }
            },
            { "StatusBerserk", new Dictionary<String, String>()
                {
                    { "US", "Berserk" },
                    { "UK", "Berserk" },
                    { "JP", "バーサク" },
                    { "ES", "Locura" },
                    { "FR", "Furie" },
                    { "GR", "Tobsucht" },
                    { "IT", "Berserk" }
                }
            },
            { "StatusStop", new Dictionary<String, String>()
                {
                    { "US", "Stop" },
                    { "UK", "Stop" },
                    { "JP", "ストップ" },
                    { "ES", "Paro" },
                    { "FR", "Stop" },
                    { "GR", "Stop" },
                    { "IT", "Stop" }
                }
            },
            { "StatusAutoLife", new Dictionary<String, String>()
                {
                    { "US", "AutoLife" },
                    { "UK", "AutoLife" },
                    { "JP", "リレイズ" },
                    { "ES", "AutoLázaro" },
                    { "FR", "Pakaho" },
                    { "GR", "Wiedergeburt" },
                    { "IT", "Risveglio" }
                }
            },
            { "StatusTrance", new Dictionary<String, String>()
                {
                    { "US", "Trance" },
                    { "UK", "Trance" },
                    { "JP", "トランス" },
                    { "ES", "Trance" },
                    { "FR", "Transe" },
                    { "GR", "Trance" },
                    { "IT", "Trance" }
                }
            },
            { "StatusDefend", new Dictionary<String, String>()
                {
                    { "US", "Defend" },
                    { "UK", "Defend" },
                    { "JP", "防御" },
                    { "ES", "Defensa" },
                    { "FR", "Défense" },
                    { "GR", "Abwehr" },
                    { "IT", "Difesa" }
                }
            },
            { "StatusPoison", new Dictionary<String, String>()
                {
                    { "US", "Poison" },
                    { "UK", "Poison" },
                    { "JP", "毒" },
                    { "ES", "Veneno" },
                    { "FR", "Poison" },
                    { "GR", "Gift" },
                    { "IT", "Fiele" }
                }
            },
            { "StatusSleep", new Dictionary<String, String>()
                {
                    { "US", "Sleep" },
                    { "UK", "Sleep" },
                    { "JP", "睡眠" },
                    { "ES", "Sueño" },
                    { "FR", "Morphée" },
                    { "GR", "Schlaf" },
                    { "IT", "Sonno" }
                }
            },
            { "StatusRegen", new Dictionary<String, String>()
                {
                    { "US", "Regen" },
                    { "UK", "Regen" },
                    { "JP", "リジェネ" },
                    { "ES", "Revitalia" },
                    { "FR", "Récup" },
                    { "GR", "Regena" },
                    { "IT", "Rigene" }
                }
            },
            { "StatusHaste", new Dictionary<String, String>()
                {
                    { "US", "Haste" },
                    { "UK", "Haste" },
                    { "JP", "ヘイスト" },
                    { "ES", "Prisa" },
                    { "FR", "Booster" },
                    { "GR", "Hast" },
                    { "IT", "Haste" }
                }
            },
            { "StatusSlow", new Dictionary<String, String>()
                {
                    { "US", "Slow" },
                    { "UK", "Slow" },
                    { "JP", "スロウ" },
                    { "ES", "Freno" },
                    { "FR", "Somni" },
                    { "GR", "Gemach" },
                    { "IT", "Lentezza" }
                }
            },
            { "StatusFloat", new Dictionary<String, String>()
                {
                    { "US", "Float" },
                    { "UK", "Float" },
                    { "JP", "レビテト" },
                    { "ES", "Lévita" },
                    { "FR", "Lévitation" },
                    { "GR", "Levitas" },
                    { "IT", "Levita" }
                }
            },
            { "StatusShell", new Dictionary<String, String>()
                {
                    { "US", "Shell" },
                    { "UK", "Shell" },
                    { "JP", "シェル" },
                    { "ES", "Coraza" },
                    { "FR", "Blindage" },
                    { "GR", "Shell" },
                    { "IT", "Shell" }
                }
            },
            { "StatusProtect", new Dictionary<String, String>()
                {
                    { "US", "Protect" },
                    { "UK", "Protect" },
                    { "JP", "プロテス" },
                    { "ES", "Escudo" },
                    { "FR", "Carapace" },
                    { "GR", "Protes" },
                    { "IT", "Protect" }
                }
            },
            { "StatusHeat", new Dictionary<String, String>()
                {
                    { "US", "Heat" },
                    { "UK", "Heat" },
                    { "JP", "ヒート" },
                    { "ES", "Ardor" },
                    { "FR", "Chaleur" },
                    { "GR", "Glut" },
                    { "IT", "Caldo" }
                }
            },
            { "StatusFreeze", new Dictionary<String, String>()
                {
                    { "US", "Freeze" },
                    { "UK", "Freeze" },
                    { "JP", "フリーズ" },
                    { "ES", "Gélido" },
                    { "FR", "Gel" },
                    { "GR", "Frost" },
                    { "IT", "Freddo" }
                }
            },
            { "StatusVanish", new Dictionary<String, String>()
                {
                    { "US", "Vanish" },
                    { "UK", "Vanish" },
                    { "JP", "消える" },
                    { "ES", "Invisibilidad" },
                    { "FR", "Invisibilité" },
                    { "GR", "Verschwinden" },
                    { "IT", "Invisibilità" }
                }
            },
            { "StatusDoom", new Dictionary<String, String>()
                {
                    { "US", "Doom" },
                    { "UK", "Doom" },
                    { "JP", "死の宣告" },
                    { "ES", "Condena" },
                    { "FR", "Châtiment" },
                    { "GR", "Todesurteil" },
                    { "IT", "Sentenza" }
                }
            },
            { "StatusMini", new Dictionary<String, String>()
                {
                    { "US", "Mini" },
                    { "UK", "Mini" },
                    { "JP", "ミニマム" },
                    { "ES", "Minimalia" },
                    { "FR", "Minimum" },
                    { "GR", "Wicht" },
                    { "IT", "Minimo" }
                }
            },
            { "StatusReflect", new Dictionary<String, String>()
                {
                    { "US", "Reflect" },
                    { "UK", "Reflect" },
                    { "JP", "リフレク" },
                    { "ES", "Espejo" },
                    { "FR", "Boomerang" },
                    { "GR", "Reflek" },
                    { "IT", "Reflex" }
                }
            },
            { "StatusJump", new Dictionary<String, String>()
                {
                    { "US", "Jump" },
                    { "UK", "Jump" },
                    { "JP", "ジャンプ" },
                    { "ES", "Salto" },
                    { "FR", "Sauter" },
                    { "GR", "Sprung" },
                    { "IT", "Salto" }
                }
            },
            { "StatusGradualPetrify", new Dictionary<String, String>()
                {
                    { "US", "Gradual Petrify" },
                    { "UK", "Gradual Petrify" },
                    { "JP", "徐々に石化" },
                    { "ES", "→Piedra" },
                    { "FR", "Pétra" },
                    { "GR", "Gips" },
                    { "IT", "Pietrificazione" }
                }
            },
            { "AADesc_Power", new Dictionary<String, String>()
                {
                    { "US", "Power" },
                    { "UK", "Power" },
                    { "JP", "力" },
                    { "ES", "Poder" },
                    { "FR", "Puissance" },
                    { "GR", "Power" },
                    { "IT", "Potere" }
                }
            },
            { "AADesc_HitRate", new Dictionary<String, String>()
                {
                    { "US", "Hitrate" },
                    { "UK", "Hitrate" },
                    { "JP", "正確さ" },
                    { "ES", "Precisión" },
                    { "FR", "Précision" },
                    { "GR", "Genauigkeit" },
                    { "IT", "Precisione" }
                }
            },
            { "AADesc_Physical", new Dictionary<String, String>()
                {
                    { "US", "physical" },
                    { "UK", "physical" },
                    { "JP", "物理的" },
                    { "ES", "físico" },
                    { "FR", "physique" },
                    { "GR", "physisch" },
                    { "IT", "fisico" }
                }
            },
            { "AADesc_Magical", new Dictionary<String, String>()
                {
                    { "US", "magical" },
                    { "UK", "magical" },
                    { "JP", "魔法の" },
                    { "ES", "mágico" },
                    { "FR", "magique" },
                    { "GR", "magisch" },
                    { "IT", "magico" }
                }
            },
            { "AADesc_SingleAlly", new Dictionary<String, String>()
                {
                    { "US", "an ally" },
                    { "UK", "an ally" },
                    { "JP", "味方" },
                    { "ES", "un aliado" },
                    { "FR", "un allié" },
                    { "GR", "Fläche (Verbündete)" },
                    { "IT", "un alleato" }
                }
            },
            { "AADesc_AllAlly", new Dictionary<String, String>()
                {
                    { "US", "the whole team" },
                    { "UK", "the whole team" },
                    { "JP", "チーム全体" },
                    { "ES", "todo el equipo" },
                    { "FR", "toute l'équipe" },
                    { "GR", "Fläche (Verbündete)" },
                    { "IT", "tutta gli alleati" }
                }
            },
            { "AADesc_SingleEnemy", new Dictionary<String, String>()
                {
                    { "US", "an enemy" },
                    { "UK", "an enemy" },
                    { "JP", "敵" },
                    { "ES", "un enemigo" },
                    { "FR", "un ennemi" },
                    { "GR", "Einzel (Gegner)" },
                    { "IT", "un nemico" }
                }
            },
            { "AADesc_AllEnemy", new Dictionary<String, String>()
                {
                    { "US", "all enemies" },
                    { "UK", "all enemies" },
                    { "JP", "全ての敵" },
                    { "ES", "todos los enemigos" },
                    { "FR", "tous les ennemis" },
                    { "GR", "Flächenvisier (Gegner)" },
                    { "IT", "tutti i nemici" }
                }
            },
            { "AADesc_Self", new Dictionary<String, String>()
                {
                    { "US", "self" },
                    { "UK", "self" },
                    { "JP", "自分自身" },
                    { "ES", "uno mismo" },
                    { "FR", "soi-même" },
                    { "GR", "Einzelvisier (Verbündete)" },
                    { "IT", "se stessi" }
                }
            },
            { "AADesc_Random", new Dictionary<String, String>()
                {
                    { "US", "a random target" },
                    { "UK", "a random target" },
                    { "JP", "ランダムな対象" },
                    { "ES", "un objetivo aleatorio" },
                    { "FR", "une cible aléatoire" },
                    { "GR", "Einzelvisier" },
                    { "IT", "un bersaglio casuale" }
                }
            },
            { "AADesc_RandomAlly", new Dictionary<String, String>()
                {
                    { "US", "a random ally" },
                    { "UK", "a random ally" },
                    { "JP", "ランダムな味方" },
                    { "ES", "un aliado aleatorio" },
                    { "FR", "un allié aléatoire" },
                    { "GR", "Einzelvisier (Verbündete)" },
                    { "IT", "un alleato casuale" }
                }
            },
            { "AADesc_RandomEnemy", new Dictionary<String, String>()
                {
                    { "US", "a random enemy" },
                    { "UK", "a random enemy" },
                    { "JP", "ランダムな敵" },
                    { "ES", "un enemigo aleatorio" },
                    { "FR", "un ennemi aléatoire" },
                    { "GR", "Einzelvisier (Gegner)" },
                    { "IT", "un nemico casuale" }
                }
            },
            { "AADesc_Everyone", new Dictionary<String, String>()
                {
                    { "US", "everyone" },
                    { "UK", "everybody" },
                    { "JP", "全員" },
                    { "ES", "todos" },
                    { "FR", "tout le monde" },
                    { "GR", "Flächenvisier (Gegner/Verbündete)." },
                    { "IT", "tutti" }
                }
            },
            { "AADesc_SingleAny", new Dictionary<String, String>()
                {
                    { "US", "an ally or an enemy" },
                    { "UK", "an ally or an enemy" },
                    { "JP", "味方または敵" },
                    { "ES", "un aliado o un enemigo" },
                    { "FR", "un allié ou un ennemi" },
                    { "GR", "Einzel (Gegner/Verbündete)" },
                    { "IT", "un alleato o un nemico" }
                }
            },
            { "AADesc_ManyAny", new Dictionary<String, String>()
                {
                    { "US", "allies or enemies" },
                    { "UK", "allies or enemies" },
                    { "JP", "味方または敵" },
                    { "ES", "aliados o enemigos" },
                    { "FR", "les alliés ou les ennemis" },
                    { "GR", "Einzel/Flächenvisier (Gegner/Verbündete)" },
                    { "IT", "alleati o nemici" }
                }
            },
            { "AADesc_HP", new Dictionary<String, String>()
                {
                    { "US", "HP" },
                    { "UK", "HP" },
                    { "JP", "HP" },
                    { "ES", "puntos de vitalidad" },
                    { "FR", "HP" },
                    { "GR", "HP" },
                    { "IT", "HP" }
                }
            },
            { "AADesc_MP", new Dictionary<String, String>()
                {
                    { "US", "MP" },
                    { "UK", "MP" },
                    { "JP", "MP" },
                    { "ES", "puntos mágicos" },
                    { "FR", "MP" },
                    { "GR", "MP" },
                    { "IT", "MP" }
                }
            },
            { "AADesc_NonElemental", new Dictionary<String, String>()
                {
                    { "US", "[A85038][HSHD]Non-elemental[383838][HSHD]" },
                    { "UK", "[A85038][HSHD]Non-elemental[383838][HSHD]" },
                    { "JP", "[A85038][HSHD]無[383838][HSHD]" },
                    { "ES", "[A85038][HSHD]no elemental[383838][HSHD]" },
                    { "FR", "[A85038][HSHD]Neutre[383838][HSHD]" },
                    { "GR", "[A85038][HSHD]neutrale[383838][HSHD]" },
                    { "IT", "[A85038][HSHD]Non-elementali[383838][HSHD]" }
                }
            },
            { "ClassicDamageScript", new Dictionary<String, String>()
                {
                    { "US", "Deals =TYPE= =ELEMENT= damage on =TARGET=." },
                    { "UK", "Deals =TYPE= =ELEMENT= damage on =TARGET=." },
                    { "JP", "=TARGET= に =ELEMENT= 属性の =TYPE= ダメージを与える。" },
                    { "ES", "Inflige daño =TYPE= de elemento =ELEMENT= a =TARGET=." },
                    { "FR", "Lance une attaque =TYPE= d'élement =ELEMENT= sur =TARGET=." },
                    { "GR", "=TARGET=. Fügt =TYPE= Schaden vom Element =ELEMENT= zu." },
                    { "IT", "Infligge danni =TYPE= di elemento =ELEMENT= a =TARGET=." }
                }
            },
            { "MPAttackScript", new Dictionary<String, String>()
                {
                    { "US", "Reduce MP on =TARGET=" },
                    { "UK", "Reduce MP on =TARGET=" },
                    { "JP", "=TARGET=からMPを除去する" },
                    { "ES", "Reduce puntos mágicos a =TARGET=" },
                    { "FR", "Retire des MP sur =TARGET=" },
                    { "GR", "=TARGET=. Entfernt MP." },
                    { "IT", "Rimuove MP di =TARGET=" }
                }
            },
            { "DarksideScript", new Dictionary<String, String>()
                {
                    { "US", "Reduces your HP to cause =ELEMENT= damage on =TARGET=." },
                    { "UK", "Reduces your HP to cause =ELEMENT= damage on =TARGET=." },
                    { "JP", "自分のHPを使って敵単体に =ELEMENT=属性のダメージを与えます。" },
                    { "ES", "Causa daño de elemento =ELEMENT= a costa de la vitalidad del atacante." },
                    { "FR", "Lance une attaque élémentaire =ELEMENT= grâce à vos HP." },
                    { "GR", "=TARGET=. Opfert eigene HP u. fügt =ELEMENT=- elementaren Schaden zu." },
                    { "IT", "Servendosi dei propri HP, provoca danni di elemento =ELEMENT= a =TARGET=." }
                }
            },
            { "HealScript", new Dictionary<String, String>()
                {
                    { "US", "Restores =FLAGS= on =TARGET=." },
                    { "UK", "Restores =FLAGS= on =TARGET=." },
                    { "JP", "=TARGET= の =FLAGS= を回復する。" },
                    { "ES", "Restaura =FLAGS= a =TARGET=." },
                    { "FR", "Restaure les =FLAGS= sur =TARGET=." },
                    { "GR", "=TARGET=. Heilt  =FLAGS=." },
                    { "IT", "Ripristina =FLAGS= su =TARGET=." }
                }
            },
            { "ApplyStatusScript", new Dictionary<String, String>()
                {
                    { "US", "Inflicts =STATUS= on =TARGET=." },
                    { "UK", "Inflicts =STATUS= to =TARGET=." },
                    { "JP", "=TARGET= に =STATUS= を付与する。" },
                    { "ES", "Causa =STATUS= a =TARGET=." },
                    { "FR", "Inflige =STATUS= sur =TARGET=." },
                    { "GR", "=TARGET=. Erteilt =STATUS=." },
                    { "IT", "Infligge =STATUS= su =TARGET=." }
                }
            },
            { "ApplyStatusBisScript", new Dictionary<String, String>()
                {
                    { "US", "Causes =STATUS=." },
                    { "UK", "Causes =STATUS=." },
                    { "JP", "さらに=STATUS=を付与する" },
                    { "ES", "Causa el estado =STATUS=." },
                    { "FR", "Provoque =STATUS=." },
                    { "GR", "Erteilt =STATUS=." },
                    { "IT", "Provoca status =STATUS=." }
                }
            },
            { "RemoveStatusScript", new Dictionary<String, String>()
                {
                    { "US", "Cures =STATUS=." },
                    { "UK", "Cures =STATUS=." },
                    { "JP", "=STATUS=を治します。" },
                    { "ES", "Cura el estado =STATUS=." },
                    { "FR", "Soigne =STATUS=." },
                    { "GR", "=TARGET=. Hebt Zustandsveränderung =STATUS= auf." },
                    { "IT", "Cura status =STATUS=." }
                }
            },
            { "DrainScript", new Dictionary<String, String>()
                {
                    { "US", "Drain =FLAGS= on =TARGET=." },
                    { "UK", "Drain =FLAGS= on =TARGET=." },
                    { "JP", "=TARGET=から=FLAGS=を吸収する" },
                    { "ES", "Absorbe =FLAGS= de =TARGET=." },
                    { "FR", "Absorbe des =FLAGS= sur =TARGET=." },
                    { "GR", "=TARGET=. Absorbiert =FLAGS=." },
                    { "IT", "Assorbe =FLAGS= da =TARGET=." }
                }
            },
            { "GravityScript", new Dictionary<String, String>()
                {
                    { "US", "Deals =TYPE= =ELEMENT= damage on =TARGET=.\nDamage depends on the target's max HP." },
                    { "UK", "Deals =TYPE= =ELEMENT= damage on =TARGET=.\nDamage depends on the target's max HP." },
                    { "JP", "=TARGET= に =ELEMENT= 属性の =TYPE= ダメージを与える。\nダメージは対象の最大HPに依存する" },
                    { "ES", "Inflige daño =TYPE= de elemento =ELEMENT= a =TARGET=.\nEl daño depende de los VIT máximos del objetivo" },
                    { "FR", "Lance une attaque =TYPE= d'élement =ELEMENT= sur =TARGET=.\nLes dégâts dépendent des HP maximum de la cible." },
                    { "GR", "=TARGET=. Fügt =TYPE= Schaden vom Element =ELEMENT= zu.\nSchaden hängt von den maximalen HP des Ziels ab." },
                    { "IT", "Infligge danni =TYPE= di elemento =ELEMENT= a =TARGET=.\nI danni dipendono dagli HP massimi del bersaglio." }
                }
            },
            { "ReviveScript", new Dictionary<String, String>()
                {
                    { "US", "Recover from [A85038][HSHD]=CUREKO=[383838][HSHD] and restores =POWER=% of max HP" },
                    { "UK", "Recover from [A85038][HSHD]=CUREKO=[383838][HSHD] and restores =POWER=% of max HP" },
                    { "JP", "[A85038][HSHD]=CUREKO=[383838][HSHD]を解除し、最大HPの=POWER=%を回復する" },
                    { "ES", "Cura el estado [A85038][HSHD]=CUREKO=[383838][HSHD] y restaura el =POWER=% de los HP máximos" },
                    { "FR", "Annule un [A85038][HSHD]=CUREKO=[383838][HSHD] et restaure =POWER=% des HP maximum." },
                    { "GR", "Kuriert [A85038][HSHD]=CUREKO=[383838][HSHD] auf und stellt =POWER=% der maximalen HP wieder her" },
                    { "IT", "Cura [A85038][HSHD]=CUREKO=[383838][HSHD] e ripristina il =POWER=% degli HP massimi" }
                }
            },
            { "LvDirectHPDamageScript", new Dictionary<String, String>()
                {
                    { "US", "Reduces HP to =POWER= on =TARGET= with level multiple of =HITRATE=." },
                    { "UK", "Reduces HP to =POWER= on =TARGET= with level multiple of =HITRATE=." },
                    { "JP", "=HITRATE=の倍数のレベルを持つ=TARGET=のHPを=POWER=に減らす" },
                    { "ES", "Reduce los Vit a =POWER= en =TARGET= con nivel múltiplo de =HITRATE=." },
                    { "FR", "Réduit les HP à =POWER= sur =TARGET= possèdant un niveau multiple de =HITRATE=." },
                    { "GR", "=TARGET=. Reduziert die HP auf =POWER= mit Level Vielfaches von =HITRATE=." },
                    { "IT", "Riduce gli HP a =POWER= su =TARGET= con livello multiplo di =HITRATE=." }
                }
            },
            { "LVHolyScript", new Dictionary<String, String>()
                {
                    { "US", "Deals a =TYPE= =ELEMENT= attack on =TARGET= with level multiple of =HITRATE=" },
                    { "UK", "Deals a =TYPE= =ELEMENT= attack on =TARGET= with level multiple of =HITRATE=" },
                    { "JP", "=HITRATE=の倍数のレベルを持つ=TARGET=に=TYPE=属性=ELEMENT=攻撃を仕掛ける" },
                    { "ES", "Lanza un ataque =TYPE= de elemento =ELEMENT= sobre =TARGET= con nivel múltiplo de =HITRATE=" },
                    { "FR", "Lance une attaque =TYPE= d'élement =ELEMENT= sur =TARGET= possédant un niveau multiple de =HITRATE=." },
                    { "GR", "=TARGET=. Startet eine =TYPE= Element =ELEMENT= Attacke mit Level Vielfaches von =HITRATE=" },
                    { "IT", "Lancia un attacco =TYPE= di elemento =ELEMENT= su =TARGET= con livello multiplo di =HITRATE=" }
                }
            },
            { "LvReduceDefence", new Dictionary<String, String>()
                {
                    { "US", "Reduces physical and magical defense on =TARGET= with level multiple of =HITRATE=" },
                    { "UK", "Reduces physical and magical defence on =TARGET= with level multiple of =HITRATE=" },
                    { "JP", "=HITRATE=の倍数のレベルを持つ=TARGET=の物理・魔法防御を減少させる" },
                    { "ES", "Reduce la defensa física y mágica en =TARGET= con nivel múltiplo de =HITRATE=" },
                    { "FR", "Réduit la défense physique et magique sur =TARGET= possédant un niveau multiple de =HITRATE=." },
                    { "GR", "=TARGET=. Reduziert die physische und magische Verteidigung mit Level Vielfaches von =HITRATE=" },
                    { "IT", "Riduce la difesa fisica e magica su =TARGET= con livello multiplo di =HITRATE=" }
                }
            },
            { "PreciseDirectHPDamageScript", new Dictionary<String, String>()
                {
                    { "US", "Reduces HP to =POWER= on =TARGET=" },
                    { "UK", "Reduces HP to =POWER= on =TARGET=" },
                    { "JP", "=TARGET=のHPを=POWER=に減らす" },
                    { "ES", "Reduce los Vit a =POWER= en =TARGET=" },
                    { "FR", "Réduit les HP à =POWER= sur =TARGET=." },
                    { "GR", "=TARGET=. Reduziert die HP auf =POWER=." },
                    { "IT", "Riduce gli HP a =POWER= su =TARGET=" }
                }
            },
            { "ThousandNeedlesScript", new Dictionary<String, String>()
                {
                    { "US", "Deals exactly =DAMAGE= HP on =TARGET=" },
                    { "UK", "Deals exactly =DAMAGE= HP on =TARGET=" },
                    { "JP", "=TARGET=に正確に=DAMAGE=のHPダメージを与える" },
                    { "ES", "Inflige exactamente =DAMAGE= de puntos de vitalidad a =TARGET=" },
                    { "FR", "Inflige exactement =DAMAGE= HP sur =TARGET=." },
                    { "GR", "=TARGET=. Zieht =DAMAGE= HP ab." },
                    { "IT", "Infligge esattamente =DAMAGE= HP a =TARGET=" }
                }
            },
            { "DifferentCasterHPScript", new Dictionary<String, String>()
                {
                    { "US", "Damages with the difference between your max HP and current HP." },
                    { "UK", "Damages with the difference between your max HP and current HP." },
                    { "JP", "敵味方単体に 自分の最大HPから今のHPを ひいた分のダメージを与えます。" },
                    { "ES", "Causa un daño igual a la vitalidad máxima menos la actual del atacante." },
                    { "FR", "Inflige des dégâts égaux à la différence entre vos HP max et vos HP actuels." },
                    { "GR", "=TARGET=. Max. HP - aktuelle HP = Schadensgröße." },
                    { "IT", "Toglie HP a un membro pari alla differenza fra il proprio HP max e l’attuale." }
                }
            },
            { "ArmourBreakScript", new Dictionary<String, String>()
                {
                    { "US", "Reduces [A85038][HSHD]Defense[383838][HSHD] on =TARGET=." },
                    { "UK", "Reduces [A85038][HSHD]Defence[383838][HSHD] on =TARGET=." },
                    { "JP", "=TARGET=の[A85038][HSHD]防御力[383838][HSHD]を減少させる" },
                    { "ES", "Reduce la [A85038][HSHD]capacidad defensiva[383838][HSHD] de =TARGET=" },
                    { "FR", "Réduit la [A85038][HSHD]Défense[383838][HSHD] sur =TARGET=." },
                    { "GR", "=TARGET=. Senkt [A85038][HSHD]Verteidigungskraft[383838][HSHD]." },
                    { "IT", "Diminuisce il [A85038][HSHD]potere di difesa[383838][HSHD] di =TARGET=" }
                }
            },
            { "PowerBreakScript", new Dictionary<String, String>()
                {
                    { "US", "Reduces [A85038][HSHD]Attack Pwr[383838][HSHD] on =TARGET=." },
                    { "UK", "Reduces [A85038][HSHD]Attack Pwr[383838][HSHD] on =TARGET=." },
                    { "JP", "=TARGET=の[A85038][HSHD]攻撃力[383838][HSHD]を減少させる" },
                    { "ES", "Reduce la [A85038][HSHD]poder de ataque[383838][HSHD] de =TARGET=" },
                    { "FR", "Réduit la [A85038][HSHD]Force de frappe[383838][HSHD] sur =TARGET=." },
                    { "GR", "=TARGET=. Senkt [A85038][HSHD]Angriffskraft[383838][HSHD]" },
                    { "IT", "Diminuisce il [A85038][HSHD]potere d’attacco[383838][HSHD] di =TARGET=" }
                }
            },
            { "MentalBreakScript", new Dictionary<String, String>()
                {
                    { "US", "Reduces [A85038][HSHD]Magic Def[383838][HSHD] on =TARGET=." },
                    { "UK", "Reduces [A85038][HSHD]Magic Def[383838][HSHD] on =TARGET=." },
                    { "JP", "=TARGET=の[A85038][HSHD]魔法防御力[383838][HSHD]を減少させる" },
                    { "ES", "Reduce la [A85038][HSHD]capacidad de defensa mágica[383838][HSHD] de =TARGET=" },
                    { "FR", "Réduit la [A85038][HSHD]Défense magique[383838][HSHD] sur =TARGET=." },
                    { "GR", "=TARGET=. Senkt [A85038][HSHD]Zauber-Abwehrkraft[383838][HSHD]" },
                    { "IT", "Diminuisce il [A85038][HSHD]potere di difesa magica[383838][HSHD] di =TARGET=" }
                }
            },
            { "MagicBreakScript", new Dictionary<String, String>()
                {
                    { "US", "Reduces [A85038][HSHD]Magic[383838][HSHD] on =TARGET=." },
                    { "UK", "Reduces [A85038][HSHD]Magic[383838][HSHD] on =TARGET=." },
                    { "JP", "=TARGET=の[A85038][HSHD]魔法攻撃力[383838][HSHD]を減少させる" },
                    { "ES", "Reduce la [A85038][HSHD]poder de ataque mágico[383838][HSHD] de =TARGET=" },
                    { "FR", "Réduit la [A85038][HSHD]Défense[383838][HSHD] sur =TARGET=." },
                    { "GR", "=TARGET=. Senkt [A85038][HSHD]Zauberkraft[383838][HSHD]." },
                    { "IT", "Diminuisce il [A85038][HSHD]potere di attacco magico[383838][HSHD] di =TARGET=" }
                }
            },
            { "SpareChangeScript", new Dictionary<String, String>()
                {
                    { "US", "Causes =ELEMENT= damage on =TARGET= by using Gil." },
                    { "UK", "Causes =ELEMENT= damage on =TARGET= by using Gil." },
                    { "JP", "=TARGET=の[A85038][HSHD]魔法攻撃力[383838][HSHD]を減少させる" },
                    { "ES", "Reduce la [A85038][HSHD]poder de ataque mágico[383838][HSHD] de =TARGET=" },
                    { "FR", "Réduit la [A85038][HSHD]Défense[383838][HSHD] sur =TARGET=." },
                    { "GR", "=TARGET=. Senkt [A85038][HSHD]Zauberkraft[383838][HSHD]." },
                    { "IT", "Diminuisce il [A85038][HSHD]potere di attacco magico[383838][HSHD] di =TARGET=" }
                }
            },
            { "MightScript", new Dictionary<String, String>()
                {
                    { "US", "Increases =STRENGTH= on =TARGET= (stackable)" },
                    { "UK", "Increases =STRENGTH= on =TARGET= (stackable)" },
                    { "JP", "=TARGET=の=STRENGTH=を増加させる（重複可）" },
                    { "ES", "Aumenta =STRENGTH= en =TARGET= (acumulable)" },
                    { "FR", "Augmente la =STRENGTH= sur =TARGET= (cumulable)." },
                    { "GR", "=TARGET=. Erhöht =STRENGTH= (stapelbar)" },
                    { "IT", "Aumenta =STRENGTH= su =TARGET= (cumulabile)" }
                }
            },
            { "FocusScript", new Dictionary<String, String>()
                {
                    { "US", "Increases =MAGIC= on =TARGET= (stackable)" },
                    { "UK", "Increases =MAGIC= on =TARGET= (stackable)" },
                    { "JP", "=TARGET=の=MAGIC=を増加させる（重複可）" },
                    { "ES", "Aumenta =MAGIC= en =TARGET= (acumulable)" },
                    { "FR", "Augmente la =MAGIC= sur =TARGET= (cumulable)." },
                    { "GR", "=TARGET=. Erhöht =MAGIC= (stapelbar)" },
                    { "IT", "Aumenta =MAGIC= su =TARGET= (cumulabile)" }
                }
            },
            { "SacrificeScript", new Dictionary<String, String>()
                {
                    { "US", "Sacrifice yourself to restore HP and MP on =TARGET=." },
                    { "UK", "Sacrifice yourself to restore HP and MP on =TARGET=." },
                    { "JP", "自分を犠牲にして=TARGET=のHPとMPを回復する" },
                    { "ES", "El guerrero se sacrifica para devolver VIT y PM en =TARGET=." },
                    { "FR", "Restaure les HP et MP sur =TARGET= grâce aux vôtres." },
                    { "GR", "=TARGET=. Zielobjekt opfert sich undheilt HP, MP der Gruppe." },
                    { "IT", "Recupera HP e MP a =TARGET= sacrificando i propri." }
                }
            },
            { "SixDragonsScript", new Dictionary<String, String>()
                {
                    { "US", "See for yourself." },
                    { "UK", "See for yourself." },
                    { "JP", "何がおこるかわかりません…。" },
                    { "ES", "Su efecto es desconocido..." },
                    { "FR", "Il faut le voir pour le savoir..." },
                    { "GR", "Probieren geht über Studieren." },
                    { "IT", "Effetto sconosciuto!" }
                }
            },
            { "CurseScript", new Dictionary<String, String>()
                {
                    { "US", "Makes =TARGET= weak against some elemental property." },
                    { "UK", "Makes =TARGET= weak against some elemental property." },
                    { "JP", "=TARGET=をなにかの属性に弱くします。" },
                    { "ES", "Hace a =TARGET= vulnerable a algún elemento." },
                    { "FR", "Fragilise =TARGET= vis-à-vis d’un élément." },
                    { "GR", "=TARGET=. Schwächt Elementarabwehr." },
                    { "IT", "Indebolisce un elemento di =TARGET=." }
                }
            },
            { "AngelSnackScript", new Dictionary<String, String>()
                {
                    { "US", "Uses =ITEM= on =TARGET=." },
                    { "UK", "Uses =ITEM= on =TARGET=." },
                    { "JP", "=TARGET=全体に=ITEM=を使います。" },
                    { "ES", "Usa =ITEM= en =TARGET=." },
                    { "FR", "Utilise =ITEM= sur =TARGET=." },
                    { "GR", "Verwendet =ITEM= bei =TARGET=." },
                    { "IT", "Usa =ITEM= su =TARGET=." }
                }
            },
            { "LuckySevenScript", new Dictionary<String, String>()
                {
                    { "US", "Deals =TYPE= damage by luck on =TARGET=." },
                    { "UK", "Deals =TYPE= damage by luck on =TARGET=." },
                    { "JP", "=TARGET=運による=TYPE=ダメージを与えます。" },
                    { "ES", "Causa un daño =TYPE= en =TARGET=, que depende de la suerte del atacante." },
                    { "FR", "Inflige des dégâts =TYPE= sur =TARGET= en fonction de la chance du personnage." },
                    { "GR", "=TARGET=. Mit viel Glück erteilt man =TYPE= Schaden." },
                    { "IT", "Provoca danni =TYPE= su =TARGET=, ma è un terno al lotto!" }
                }
            },
            { "WhatIsThatScript", new Dictionary<String, String>()
                {
                    { "US", "Allows back attack." },
                    { "UK", "Allows back attack." },
                    { "JP", "先制攻撃の状態にします。" },
                    { "ES", "Permite atacar primero." },
                    { "FR", "Permet d’attaquer le premier." },
                    { "GR", "Man ist im Angriffsvorteil." },
                    { "IT", "Permette attacco prioritario." }
                }
            },
            { "ChangeRowScript", new Dictionary<String, String>()
                {
                    { "US", "Toggle between front row and back row." },
                    { "UK", "Toggle between front row and back row." },
                    { "JP", "前列と後列がいれかわります。" },
                    { "ES", "Cambia entre vanguardia y retaguardia." },
                    { "FR", "Change la position avant/arrière." },
                    { "GR", "Charakter wechselt zw. vorderer/ hinterer Reihe." },
                    { "IT", "Inverte 1a e 2a linea." }
                }
            },
            { "FleeScript", new Dictionary<String, String>()
                {
                    { "US", "Escape from battle." },
                    { "UK", "Escape from battle." },
                    { "JP", "戦闘から脱出します。" },
                    { "ES", "Escapa de la batalla." },
                    { "FR", "Escape from battle." },
                    { "GR", "Tritt die Flucht nach hinten an." },
                    { "IT", "Scappa dalla battaglia." }
                }
            },
            { "StealScript", new Dictionary<String, String>()
                {
                    { "US", "Steal items from enemy." },
                    { "UK", "Steal items from enemy." },
                    { "JP", "相手の持ち物をぬすみます。" },
                    { "ES", "Roba objetos al enemigo." },
                    { "FR", "Vole des possessions à l’adversaire." },
                    { "GR", "Klaut gegnerische Items." },
                    { "IT", "Scappa dalla battaglia." }
                }
            },
            { "ScanScript", new Dictionary<String, String>()
                {
                    { "US", "Scan =TARGET= to determine HP, MP, and weaknesses." },
                    { "UK", "Scan =TARGET= to determine HP, MP, and weaknesses." },
                    { "JP", "=TARGET=のHP、MP、弱い属性がわかります。" },
                    { "ES", "Muestra la VIT, los PM y la debilidad elemental de =TARGET=" },
                    { "FR", "Donne les HP, MP et les points faibles =TARGET=" },
                    { "GR", "=TARGET=. Analysiert gegnerische Parameter." },
                    { "IT", "Individua HP, MP e elemento debole =TARGET=." }
                }
            },
            { "DetectScript", new Dictionary<String, String>()
                {
                    { "US", "See the enemy’s items." },
                    { "UK", "See the enemy’s items." },
                    { "JP", "敵単体の持っているアイテムがわかります。" },
                    { "ES", "Muestra los objetos en poder de un enemigo." },
                    { "FR", "Affiche les possessions d’un adversaire." },
                    { "GR", "=TARGET=. Manerfährt, was für Items der Gegner mit sich trägt." },
                    { "IT", "Individua gli oggetti posseduti da un nemico." }
                }
            },
            { "ChargeScript", new Dictionary<String, String>()
                {
                    { "US", "Makes all Near Death party members ‘Attack.’" },
                    { "UK", "Makes all Near Death party members ‘Attack.’." },
                    { "JP", "味方全体が『たたかう』を行います。" },
                    { "ES", "Todos los aliados atacan sucesivamente." },
                    { "FR", "Fait utiliser ”Attaquer” à toute l’équipe." },
                    { "GR", "=TARGET=. Alle rücken aus." },
                    { "IT", "Gli alleati attaccano tutti insieme." }
                }
            },
            { "EatScript", new Dictionary<String, String>()
                {
                    { "US", "Learn enemy skill." },
                    { "UK", "Learn enemy skill." },
                    { "JP", "敵のわざをおぼえます。" },
                    { "ES", "Aprende una técnica del enemigo." },
                    { "FR", "Apprend une technique de l’ennemi." },
                    { "GR", "Erlernt gegnerische Ability." },
                    { "IT", "Apprende una tecnica dell’avversario." }
                }
            },
            { "FrogDropScript", new Dictionary<String, String>()
                {
                    { "US", "Amount of damage depends on the number of frogs you have caught." },
                    { "UK", "Amount of damage depends on the number of frogs you have caught." },
                    { "JP", "カエルをとった分の物理ダメージを与えます。" },
                    { "ES", "Causa un daño físico proporcional al número de ranas atrapadas." },
                    { "FR", "Inflige des dégâts en fonction du nombre de grenouilles attrapées." },
                    { "GR", "Je nach Anzahl gefangener Frösche ändert sich die Schadensgröße." },
                    { "IT", "Provoca danni fisici in proporzione al livello della rana." }
                }
            },
            { "DoubleCastScript", new Dictionary<String, String>()
                {
                    { "US", "Casts =SPELL1= and =SPELL2= in the same turn" },
                    { "UK", "Casts =SPELL1= and =SPELL2= in the same turn" },
                    { "JP", "同じターンに=SPELL1=と=SPELL2=を使用する" },
                    { "ES", "Lanza =SPELL1= y =SPELL2= en el mismo turno" },
                    { "FR", "Lance =SPELL1= et =SPELL2= sur le même tour." },
                    { "GR", "Wirkt =SPELL1= und =SPELL2= in derselben Runde" },
                    { "IT", "Lancia =SPELL1= e =SPELL2= nello stesso turno" }
                }
            },
            { "KamikazeScript", new Dictionary<String, String>()
                {
                    { "US", "Sacrifices yourself to deal damage based on your current HP" },
                    { "UK", "Sacrifices yourself to deal damage based on your current HP" },
                    { "JP", "自身の現在のHPに応じたダメージを与えるために自分を犠牲にする" },
                    { "ES", "Te sacrificas para infligir daño según tu Vit actual" },
                    { "FR", "Vous sacrifie pour infliger des dégâts, basés sur vos HP actuels." },
                    { "GR", "Opfert sich, um Schaden basierend auf den aktuellen HP zu verursachen" },
                    { "IT", "Si sacrifica per infliggere danni in base agli HP attuali" }
                }
            },
            { "HPSwitchingScript", new Dictionary<String, String>()
                {
                    { "US", "Swaps your current HP with =TARGET=." },
                    { "UK", "Swaps your current HP with =TARGET=." },
                    { "JP", "自分の現在のHPと=TARGET=のHPを入れ替える." },
                    { "ES", "Intercambia tu Vit actual con =TARGET=." },
                    { "FR", "Inverse vos HP actuels avec =TARGET=." },
                    { "GR", "=TARGET=. Tauscht deine aktuellen HP." },
                    { "IT", "Scambia i tuoi HP attuali con =TARGET=." }
                }
            },
            { "HalfDefenceScript", new Dictionary<String, String>()
                {
                { "US", "Reduces physical and magical defense by 50% on =TARGET=." },
                { "UK", "Reduces physical and magical defence by 50% on =TARGET=." },
                { "JP", "=TARGET=の物理防御と魔法防御を50%下げる" },
                { "ES", "Reduce la defensa física y mágica en un 50% en =TARGET=." },
                { "FR", "Réduit la défense physique et magique de 50% sur =TARGET=." },
                { "GR", "=TARGET=. Reduziert physische und magische Abwehr um 50 %." },
                { "IT", "Riduce la difesa fisica e magica del 50% su =TARGET=." }
                }
            },
            { "CannonScript", new Dictionary<String, String>()
                {
                    { "US", "Deals =TYPE= =ELEMENT= damage on =TARGET=.\nDamage depends on the target's current HP." },
                    { "UK", "Deals =TYPE= =ELEMENT= damage on =TARGET=.\nDamage depends on the target's current HP." },
                    { "JP", "=TARGET= に =ELEMENT= 属性の =TYPE= ダメージを与える。\nダメージは対象の現在のHPに依存する" },
                    { "ES", "Inflige daño =TYPE= de elemento =ELEMENT= a =TARGET=.\nEl daño depende de los VIT actuales del objetivo" },
                    { "FR", "Lance une attaque =TYPE= d'élement =ELEMENT= sur =TARGET=.\nLes dégâts dépendent des HP actuels de la cible." },
                    { "GR", "=TARGET=. Fügt =TYPE= Schaden vom Element =ELEMENT= zu.\nSchaden hängt von den aktuellen HP des Ziels ab." },
                    { "IT", "Infligge danni =TYPE= di elemento =ELEMENT= a =TARGET=.\nI danni dipendono dagli HP attuali del bersaglio." }
                }
            },
            { "ItemAddScript", new Dictionary<String, String>()
                {
                    { "US", "Adds a =ITEM= to your inventory." },
                    { "UK", "Adds a =ITEM= to your inventory." },
                    { "JP", "=ITEM=をインベントリに追加する" },
                    { "ES", "Añade un =ITEM= a tu inventario." },
                    { "FR", "Ajoute un =ITEM= dans votre inventaire." },
                    { "GR", "Fügt ein =ITEM= dem Inventar hinzu." },
                    { "IT", "Aggiunge un =ITEM= al tuo inventario." }
                }
            },
            { "MaelstromScript", new Dictionary<String, String>()
                {
                    { "US", "Reduces HP to a single digit on =TARGET=" },
                    { "UK", "Reduces HP to a single digit on =TARGET=" },
                    { "JP", "=TARGET=のHPを一桁まで減らす" },
                    { "ES", "Reduce la Vit a un solo dígito en =TARGET=" },
                    { "FR", "Réduit les HP à un seul chiffre sur =TARGET=." },
                    { "GR", "=TARGET=. Reduziert die HP auf eine einzelne Ziffer" },
                    { "IT", "Riduce gli HP a una sola cifra su =TARGET=" }
                }
            },
            { "AbsorbMagicScript", new Dictionary<String, String>()
                {
                    { "US", "Absorbs magic from =TARGET=." },
                    { "UK", "Absorbs magic from =TARGET=." },
                    { "JP", "=TARGET=から魔力を吸収する" },
                    { "ES", "Absorbe magia de =TARGET=." },
                    { "FR", "Absorbe de la magie sur =TARGET=." },
                    { "GR", "=TARGET=. Absorbiert Magie." },
                    { "IT", "Assorbe magia da =TARGET=." }
                }
            },
            { "AbsorbStrengthScript", new Dictionary<String, String>()
                {
                    { "US", "Absorbs strength from =TARGET=." },
                    { "UK", "Absorbs strength from =TARGET=." },
                    { "JP", "=TARGET=から力を吸収する" },
                    { "ES", "Absorbe fuerza de =TARGET=." },
                    { "FR", "Absorbe de la force sur =TARGET=." },
                    { "GR", "=TARGET=. Absorbiert Stärke." },
                    { "IT", "Assorbe forza da =TARGET=." }
                }
            },
            { "TranceFullScript", new Dictionary<String, String>()
                {
                    { "US", "Forces Trance on =TARGET=." },
                    { "UK", "Forces Trance on =TARGET=." },
                    { "JP", "=TARGET=にトランスを強制する" },
                    { "ES", "Fuerza Trance en =TARGET=." },
                    { "FR", "Force la Trance sur =TARGET=." },
                    { "GR", "=TARGET=. Erzwingt Trance." },
                    { "IT", "Forza Trance su =TARGET=." }
                }
            },
            { "EnticeScript", new Dictionary<String, String>()
                {
                    { "US", "Guaranteed to inflict =STATUS= on =TARGET= (fails on females)." },
                    { "UK", "Guaranteed to inflict =STATUS= on =TARGET= (fails on females)." },
                    { "JP", "=TARGET=に=STATUS=を確実に付与する（女性には無効）" },
                    { "ES", "Inflige con seguridad =STATUS= a =TARGET= (falla en mujeres)." },
                    { "FR", "Inflige à coup sûr =STATUS= sur =TARGET= (échoue sur les femmes)." },
                    { "GR", "Verursacht garantiert =STATUS= bei =TARGET= (wirkt nicht bei Frauen)." },
                    { "IT", "Infligge automaticamente =STATUS= su =TARGET= (fallisce sulle donne)." }
                }
            },
            { "SimpleAttackGaiaScript", new Dictionary<String, String>()
                {
                    { "US", "Deals =TYPE= =ELEMENT= damage on =TARGET=. Ineffective against inhabitants of Terra." },
                    { "UK", "Deals =TYPE= =ELEMENT= damage on =TARGET=. Ineffective against inhabitants of Terra." },
                    { "JP", "=TARGET= に =ELEMENT= 属性の =TYPE= ダメージを与える（テラの住人には効果がない）" },
                    { "ES", "Inflige daño =TYPE= de elemento =ELEMENT= a =TARGET=. Ineficaz contra los habitantes de Terra." },
                    { "FR", "Lance une attaque =TYPE= d'élement =ELEMENT= sur =TARGET=. Inefficace contre les habitants de Terra." },
                    { "GR", "=TARGET=. Fügt =TYPE= Schaden vom Element =ELEMENT= zu. Keine Wirkung gegen Bewohner von Terra." },
                    { "IT", "Infligge danni =TYPE= di elemento =ELEMENT= a =TARGET=. Inefficace contro gli abitanti di Tera." }
                }
            },
            { "TonberryKarmaScript", new Dictionary<String, String>()
                {
                    { "US", "Deals damage based on the number of Tonberry defeated." },
                    { "UK", "Deals damage based on the number of Tonberry defeated." },
                    { "JP", "倒したトンベリの数に応じてダメージを与える。" },
                    { "ES", "Inflige daño según el número de Tomberi derrotados." },
                    { "FR", "Inflige des dégâts selon le nombre de Tomberry vaincus." },
                    { "GR", "Verursacht Schaden basierend auf der Anzahl besiegter Tombery." },
                    { "IT", "Infligge danni in base al numero di Tomberry sconfitti." }
                }
            },
            { "SwallowScript", new Dictionary<String, String>()
                {
                    { "US", "Removes =TARGET= from battle." },
                    { "UK", "Removes =TARGET= from battle." },
                    { "JP", "=TARGET=を戦闘から除外する" },
                    { "ES", "Elimina a =TARGET= del combate." },
                    { "FR", "Supprime =TARGET= du combat." },
                    { "GR", "=TARGET=. Entfernt aus dem Kampf." },
                    { "IT", "Rimuove =TARGET= dalla battaglia." }
                }
            },
            { "GeneralScript", new Dictionary<String, String>()
                {
                    { "US", "Uses skill =AA= on =TARGET=." },
                    { "UK", "Uses skill =AA= on =TARGET=." },
                    { "JP", "=TARGET=に=AA=を使用する" },
                    { "ES", "Usa la habilidad =AA= sobre =TARGET=." },
                    { "FR", "Utilise la compétence =AA= sur =TARGET=." },
                    { "GR", "=TARGET=. Verwendet Fähigkeit =AA=" },
                    { "IT", "Usa l'abilità =AA= su =TARGET=." }
                }
            }
        };
    }
}
