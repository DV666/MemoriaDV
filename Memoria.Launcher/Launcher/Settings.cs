using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using Application = System.Windows.Application;

namespace Memoria.Launcher
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public readonly Object[][] SettingsList =
        {
            // Variable, _variable, variable_ini, category_ini, [value when off], [value when on], [default value (for toggle: 0 or 1)]
            
            // Checkboxes
            ["WidescreenSupport", "_iswidescreensupport", "WidescreenSupport", "Graphics", 0, 1, 1],
            ["SkipIntros", "_isskipintros", "SkipIntros", "Graphics", 0, 3, 1],
            ["MaxCardCount", "_maxcardcount", "MaxCardCount", "TetraMaster", 100, 1000, 0],
            ["HideCards", "_ishidecards", "HideCards", "Icons", 0, 1, 0],
            ["HideCards", "_ishidecards", "HideBeach", "Icons", 0, 1, 0],
            ["HideCards", "_ishidecards", "HideSteam", "Icons", 0, 1, 0],
            ["WorldmapBoost", "_worldmapboost", "FieldOfViewSpeedBoost", "Worldmap", 0, 100, 1],
            ["WorldmapShipTilt", "_worldmapshiptilt", "CameraTiltShip", "Worldmap", 0, 100, 1],
            ["SaveOnCloud", "_saveOnCloud", "SaveOnCloud", "SaveFile", 0, 1, 0],

            ["StealingAlwaysWorks", "_stealingalwaysworks", "StealingAlwaysWorks", "Hacks", 0, 2, 0],
            ["NoAutoTrance", "_noautotrance", "NoAutoTrance", "Battle", 0, 1, 0],
            ["ViviAutoAttack", "_viviautoattack", "ViviAutoAttack", "Battle", 0, 1, 0],
            ["BreakDamageLimit", "_breakDamageLimit", "BreakDamageLimit", "Battle", 0, 1, 0],
            //["AccessBattleMenuToggle", "_accessbattlemenutoggle", "AccessMenus", "Battle", 0, 3, 0],
            ["GarnetConcentrate", "_garnetconcentrate", "GarnetConcentrate", "Battle", 0, 1, 0],

            ["BattleAssistance", "_battleassistance", "BattleAssistance", "Cheats", 0, 1, 1],
            ["Attack9999", "_attack9999", "Attack9999", "Cheats", 0, 1, 1],
            ["NoRandomEncounter", "_norandomencounter", "NoRandomEncounter", "Cheats", 0, 1, 1],
            ["MasterSkill", "_masterskill", "MasterSkill", "Cheats", 0, 1, 0],
            ["MasterSkill", "_masterskill", "LvMax", "Cheats", 0, 1, 0],
            ["MasterSkill", "_masterskill", "GilMax", "Cheats", 0, 1, 0],

            ["EasyTetraMaster", "_easytetramaster", "EasyWin", "TetraMaster", 0, 1, 0],
            ["ExcaliburIINoTimeLimit", "_excaliburiinotimelimit", "ExcaliburIINoTimeLimit", "Hacks", 0, 1, 0],
            ["EasyJumpRopeMinigame", "_easyjumpropeminigame", "RopeJumpingIncrement", "Hacks", 1, 1000, 0],
            ["HippaulRacingViviSpeed", "_hippaulracingvivispeed", "HippaulRacingViviSpeed", "Hacks", 33, 100, 0],
            ["SwordplayAssistance", "_swordplayassistance", "SwordplayAssistance", "Hacks", 1, 2, 1],
            ["FrogCatchingIncrement", "_frogcatchingincrement", "FrogCatchingIncrement", "Hacks", 1, 100, 1],

            ["AudioBackend", "_audiobackend", "Backend", "Audio", 0, 1, 1],
            ["WorldSmoothTexture", "_worldsmoothtexture", "WorldSmoothTexture", "Graphics", 0, 1, 1],
            ["BattleSmoothTexture", "_battlesmoothtexture", "BattleSmoothTexture", "Graphics", 0, 1, 1],
            ["ElementsSmoothTexture", "_elementssmoothtexture", "ElementsSmoothTexture", "Graphics", 0, 1, 1],

            // Sliders
            ["CameraStabilizer", "_camerastabilizer", "CameraStabilizer", "Graphics", 0, 1, 85],
            ["BattleTPS", "_battletpsfactor", "BattleTPS", "Graphics", 0, 1, 15],
            ["SoundVolume", "_soundVolume", "SoundVolume", "Audio", 0, 1, 100],
            ["MusicVolume", "_musicVolume", "MusicVolume", "Audio", 0, 1, 100],
            ["MovieVolume", "_movieVolume", "MovieVolume", "Audio", 0, 1, 100],
            ["WorldmapTPS", "_worldmaptps", "WorldTPS", "Graphics", 0, 1, 20],
            ["WorldmapFOV", "_worldmapfov", "FieldOfView", "Worldmap", 0, 1, 44],
            ["WMCameraHeight", "_wmcameraheight", "CameraHeight", "Worldmap", 0, 1, 100],
        };

        #region Properties

        private Boolean _areThereModIncompatibilies;
        public Boolean AreThereModIncompatibilies
        {
            get => _areThereModIncompatibilies;
            set
            {
                _areThereModIncompatibilies = value;
                OnPropertyChanged(nameof(AreThereModIncompatibilies));
            }
        }

        private Boolean _areThereModUpdates;
        public Boolean AreThereModUpdates
        {
            get => _areThereModUpdates;
            set
            {
                _areThereModUpdates = value;
                OnPropertyChanged(nameof(AreThereModUpdates));
            }
        }


        ////////////////
        // CHECKBOXES //
        ////////////////

        private Int16 _iswidescreensupport;
        public Int16 WidescreenSupport
        {
            get => _iswidescreensupport;
            set => SetProperty(ref _iswidescreensupport, value);
        }
        private Int16 _isskipintros;
        public Int16 SkipIntros
        {
            get => _isskipintros;
            set => SetProperty(ref _isskipintros, value);
        }
        private Int16 _battleswirlframes;
        public Int16 BattleSwirlFrames
        {
            get => _battleswirlframes;
            set => SetProperty(ref _battleswirlframes, value);
        }
        private Int16 _maxcardcount;
        public Int16 MaxCardCount
        {
            get => _maxcardcount;
            set => SetProperty(ref _maxcardcount, value);
        }
        private Int16 _ishidecards;
        public Int16 HideCards
        {
            get => _ishidecards;
            set => SetProperty(ref _ishidecards, value);
        }
        private Int16 _worldmapboost;
        public Int16 WorldmapBoost
        {
            get => _worldmapboost;
            set => SetProperty(ref _worldmapboost, value);
        }
        private Int16 _worldmapshiptilt;
        public Int16 WorldmapShipTilt
        {
            get => _worldmapshiptilt;
            set => SetProperty(ref _worldmapshiptilt, value);
        }
        private Int16 _saveOnCloud;
        public Int16 SaveOnCloud
        {
            get => _saveOnCloud;
            set => SetProperty(ref _saveOnCloud, value);
        }

        // Cheats //

        private Int16 _stealingalwaysworks;
        public Int16 StealingAlwaysWorks
        {
            get => _stealingalwaysworks;
            set => SetProperty(ref _stealingalwaysworks, value);
        }
        private Int16 _noautotrance;
        public Int16 NoAutoTrance
        {
            get => _noautotrance;
            set => SetProperty(ref _noautotrance, value);
        }
        private Int16 _viviautoattack;
        public Int16 ViviAutoAttack
        {
            get => _viviautoattack;
            set => SetProperty(ref _viviautoattack, value);
        }
        private Int16 _breakDamageLimit;
        public Int16 BreakDamageLimit
        {
            get => _breakDamageLimit;
            set => SetProperty(ref _breakDamageLimit, value);
        }
        private Int16 _accessbattlemenutoggle;
        public Int16 AccessBattleMenuToggle
        {
            get => _accessbattlemenutoggle;
            set => SetProperty(ref _accessbattlemenutoggle, value);
        }
        private Int16 _garnetconcentrate;
        public Int16 GarnetConcentrate
        {
            get => _garnetconcentrate;
            set => SetProperty(ref _garnetconcentrate, value);
        }
        private Int16 _speedmode;
        public Int16 SpeedMode
        {
            get => _speedmode;
            set => SetProperty(ref _speedmode, value);
        }
        private Int16 _battleassistance;
        public Int16 BattleAssistance
        {
            get => _battleassistance;
            set => SetProperty(ref _battleassistance, value);
        }
        private Int16 _attack9999;
        public Int16 Attack9999
        {
            get => _attack9999;
            set => SetProperty(ref _attack9999, value);
        }
        private Int16 _norandomencounter;
        public Int16 NoRandomEncounter
        {
            get => _norandomencounter;
            set => SetProperty(ref _norandomencounter, value);
        }
        private Int16 _masterskill;
        public Int16 MasterSkill
        {
            get => _masterskill;
            set => SetProperty(ref _masterskill, value);
        }
        private Int16 _easytetramaster;
        public Int16 EasyTetraMaster
        {
            get => _easytetramaster;
            set => SetProperty(ref _easytetramaster, value);
        }
        private Int16 _excaliburiinotimelimit;
        public Int16 ExcaliburIINoTimeLimit
        {
            get => _excaliburiinotimelimit;
            set => SetProperty(ref _excaliburiinotimelimit, value);
        }
        private Int16 _easyjumpropeminigame;
        public Int16 EasyJumpRopeMinigame
        {
            get => _easyjumpropeminigame;
            set => SetProperty(ref _easyjumpropeminigame, value);
        }
        private Int16 _hippaulracingvivispeed;
        public Int16 HippaulRacingViviSpeed
        {
            get => _hippaulracingvivispeed;
            set => SetProperty(ref _hippaulracingvivispeed, value);
        }
        private Int16 _swordplayassistance;
        public Int16 SwordplayAssistance
        {
            get => _swordplayassistance;
            set => SetProperty(ref _swordplayassistance, value);
        }

        private Int16 _frogcatchingincrement;
        public Int16 FrogCatchingIncrement
        {
            get => _frogcatchingincrement;
            set => SetProperty(ref _frogcatchingincrement, value);
        }

        // advanced //

        private Int16 _audiobackend;
        public Int16 AudioBackend
        {
            get => _audiobackend;
            set => SetProperty(ref _audiobackend, value);
        }
        private Int16 _worldsmoothtexture;
        public Int16 WorldSmoothTexture
        {
            get => _worldsmoothtexture;
            set => SetProperty(ref _worldsmoothtexture, value);
        }
        private Int16 _battlesmoothtexture;
        public Int16 BattleSmoothTexture
        {
            get => _battlesmoothtexture;
            set => SetProperty(ref _battlesmoothtexture, value);
        }
        private Int16 _elementssmoothtexture;
        public Int16 ElementsSmoothTexture
        {
            get => _elementssmoothtexture;
            set => SetProperty(ref _elementssmoothtexture, value);
        }

        /////////////
        // SLIDERS //
        /////////////

        private Int16 _camerastabilizer;
        public Int16 CameraStabilizer
        {
            get => _camerastabilizer;
            set => SetProperty(ref _camerastabilizer, value);
        }
        private Int16 _battletpsfactor;
        public Int16 BattleTPS
        {
            get { return _battletpsfactor; }
            set
            {
                if (_battletpsfactor != value)
                {
                    _battletpsfactor = value;
                    BattleTPSDividedBy15 = (float)Math.Round((double)value / 15, 2);
                    OnPropertyChanged();
                }
            }
        }
        public Single BattleTPSDividedBy15
        {
            get { return (float)Math.Round((double)BattleTPS / 15, 2); }
            set { OnPropertyChanged(); }
        }
        private Int16 _soundVolume;
        public Int16 SoundVolume
        {
            get => _soundVolume;
            set => SetProperty(ref _soundVolume, value);
        }
        private Int16 _musicVolume;
        public Int16 MusicVolume
        {
            get => _musicVolume;
            set => SetProperty(ref _musicVolume, value);
        }

        private Int16 _movieVolume;
        public Int16 MovieVolume
        {
            get => _movieVolume;
            set => SetProperty(ref _movieVolume, value);
        }

        private Int16 _worldmaptps;
        public Int16 WorldmapTPS
        {
            get { return _worldmaptps; }
            set
            {
                if (_worldmaptps != value)
                {
                    _worldmaptps = value;
                    WorldmapTPSDividedby20 = (float)Math.Round((double)value / 20, 2);
                    OnPropertyChanged();
                }
            }
        }
        public Single WorldmapTPSDividedby20
        {
            get { return (float)Math.Round((double)WorldmapTPS / 20, 2); }
            set { OnPropertyChanged(); }
        }
        private Int16 _worldmapfov;
        public Int16 WorldmapFOV
        {
            get => _worldmapfov;
            set => SetProperty(ref _worldmapfov, value);
        }
        private Int16 _wmcameraheight;
        public Int16 WMCameraHeight
        {
            get => _wmcameraheight;
            set => SetProperty(ref _wmcameraheight, value);
        }
        private Int16 _speedfactor;
        public Int16 SpeedFactor
        {
            get { return _speedfactor; }
            set
            {
                if (_speedfactor != value)
                {
                    if (_speedfactor == 1)
                        _speedmode = 0;
                    else
                        _speedmode = 1;
                    _speedfactor = value;
                    OnPropertyChanged();
                }
            }
        }

        ////////////////
        // COMBOBOXES //
        ////////////////

        private Int16 _battleInterface;
        public Int16 BattleInterface
        {
            get => _battleInterface;
            set => SetProperty(ref _battleInterface, value);
        }
        private Int16 _fpsdropboxchoice;
        public Int16 FPSDropboxChoice
        {
            get => _fpsdropboxchoice;
            set => SetProperty(ref _fpsdropboxchoice, value);
        }
        private Int16 _shaderfieldchoice;
        public Int16 ShaderFieldChoice
        {
            get => _shaderfieldchoice;
            set => SetProperty(ref _shaderfieldchoice, value);
        }
        private Int16 _shaderbattlechoice;
        public Int16 ShaderBattleChoice
        {
            get => _shaderbattlechoice;
            set => SetProperty(ref _shaderbattlechoice, value);
        }
        private Int16 _uicolumnschoice;
        public Int16 UIColumnsChoice
        {
            get => _uicolumnschoice;
            set => SetProperty(ref _uicolumnschoice, value);
        }
        public Rect BattleInterfaceMenu
        {
            get
            {
                if (WidescreenSupport == 0)
                {
                    return BattleInterface switch
                    {
                        1 => new Rect(-400, -382, 530, 220),
                        2 => new Rect(-400, -360, 650, 280),
                        _ => new Rect(-400, -362, 630, 236),
                    };
                }
                else
                {
                    return BattleInterface switch
                    {
                        1 => new Rect(-500, -382, 530, 220),
                        2 => new Rect(-500, -360, 650, 280),
                        _ => new Rect(-400, -362, 630, 236),
                    };
                }
            }
        }
        public Rect BattleInterfaceDetail
        {
            get
            {
                if (WidescreenSupport == 0)
                {
                    return BattleInterface switch
                    {
                        1 => new Rect(345, -422, 672, 208),
                        2 => new Rect(345, -422, 672, 208),
                        _ => new Rect(345, -380, 796, 230),
                    };
                }
                else
                {
                    return BattleInterface switch
                    {
                        1 => new Rect(500, -422, 672, 208),
                        2 => new Rect(500, -422, 672, 208),
                        _ => new Rect(345, -380, 796, 230),
                    };
                }
            }
        }
        private Int16 _atbmodechoice;
        public Int16 ATBModeChoice
        {
            get => _atbmodechoice;
            set => SetProperty(ref _atbmodechoice, value);
        }
        private Int16 _tripleTriad;
        public Int16 TripleTriad
        {
            get => _tripleTriad;
            set => SetProperty(ref _tripleTriad, value);
        }
        private Int16 _usepsxfont;
        public Int16 UsePsxFont
        {
            get { return _usepsxfont; }
            set
            {
                if (_usepsxfont != value)
                {
                    _usepsxfont = value;
                    OnPropertyChanged();
                }
            }
        }
        protected String _fontChoice;
        public String FontChoice
        {
            get { return _fontChoice; }
            set
            {
                if (_fontChoice != value)
                {
                    _fontChoice = value;
                    OnPropertyChanged();
                    OnPropertyChanged("UsePsxFont");
                }
            }
        }
        protected String _fontDefaultPC = "Final Fantasy IX PC";
        protected String _fontDefaultPSX = "Final Fantasy IX PSX";

        private Int16 _worldmapmistpreset;
        public Int16 WorldmapMistPreset
        {
            get => _worldmapmistpreset;
            set => SetProperty(ref _worldmapmistpreset, value);
        }
        private Int16 _worldmapdistancepreset;
        public Int16 WorldmapDistancePreset
        {
            get => _worldmapdistancepreset;
            set => SetProperty(ref _worldmapdistancepreset, value);
        }
        private Int16 _accessBattleMenu;
        public Int16 AccessBattleMenu
        {
            get => _accessBattleMenu;
            set => SetProperty(ref _accessBattleMenu, value);
        }
        private Int16 _autoSave;
        public Int16 AutoSave
        {
            get => _autoSave;
            set => SetProperty(ref _autoSave, value);
        }
        public String AvailableBattleMenus => AccessBattleMenu < 3 ? " \"Equip\", \"SupportingAbility\"" : "";
        #endregion

        #region Write ini
        public event PropertyChangedEventHandler PropertyChanged;
        private async void OnPropertyChanged([CallerMemberName] String propertyName = null)
        {
            try
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

                IniFile iniFile = new IniFile(IniFile.IniPath);

                foreach (Object[] item in SettingsList)
                {
                    if (item[0] is String property && property == propertyName && item[2] is String name_ini && item[3] is String category && item[4] is Int32 valueZero && item[5] is Int32 valueOne) //  
                    {
                        Object propValue = this.GetType().GetProperty(property)?.GetValue(this);
                        if (propValue != null && Int16.TryParse(propValue.ToString(), out Int16 varValue))
                        {
                            if (varValue == 0)
                            {
                                iniFile.WriteValue(category, name_ini, $"{valueZero}");
                            }
                            else if (varValue == 1)
                            {
                                iniFile.WriteValue(category, name_ini, $"{valueOne}");
                                iniFile.WriteValue(category, "Enabled", "1");
                            }
                            else
                            {
                                iniFile.WriteValue(category, name_ini, $"{varValue}");
                                iniFile.WriteValue(category, "Enabled", "1");
                            }
                        }
                    }
                }
                Int32 var0 = 0; Int32 var1 = 0; Int32 var2 = 0; Int32 var3 = 0; Int32 var4 = 0; Int32 var5 = 0;
                switch (propertyName)
                {
                    case nameof(BattleSwirlFrames):
                        var0 = BattleSwirlFrames;
                        if (var0 == 1)
                        {
                            iniFile.WriteValue("Graphics", "Enabled", "1");
                            iniFile.WriteValue("Graphics", "BattleSwirlFrames", "0");
                        }
                        else if (var0 == 0)
                        {
                            iniFile.WriteValue("Graphics", "BattleSwirlFrames", "70");
                        }
                        break;
                    case nameof(SpeedFactor):
                        var0 = SpeedFactor;
                        iniFile.WriteValue("Cheats", "SpeedFactor", $"{var0}");
                        if (var0 < 2)
                            iniFile.WriteValue("Cheats", "SpeedMode", "0");
                        else
                        {
                            iniFile.WriteValue("Cheats", "SpeedMode", "1");
                            iniFile.WriteValue("Cheats", "Enabled", "1");
                        }
                        break;
                    case nameof(FPSDropboxChoice):
                        var0 = FPSDropboxChoice;
                        if (var0 >= 0)
                        {
                            iniFile.WriteValue("Graphics", "Enabled", "1");
                            if (var0 == 0) { var1 = 30; var2 = 15; var3 = 20; }
                            if (var0 == 1) { var1 = 30; var2 = 30; var3 = 30; }
                            if (var0 == 2) { var1 = 60; var2 = 60; var3 = 60; }
                            if (var0 == 3) { var1 = 90; var2 = 90; var3 = 90; }
                            if (var0 == 4) { var1 = 120; var2 = 120; var3 = 120; }
                            iniFile.WriteValue("Graphics", "FieldFPS", $"{var1}");
                            iniFile.WriteValue("Graphics", "BattleFPS", $"{var2}");
                            iniFile.WriteValue("Graphics", "WorldFPS", $"{var3}");
                        }
                        break;
                    case nameof(ShaderFieldChoice):
                        var0 = ShaderFieldChoice;
                        if (var0 >= 0)
                        {
                            iniFile.WriteValue("Shaders", "Enabled", "1");
                            if (var0 == 1) { var2 = 1; var3 = 1; }
                            if (var0 == 2) { var2 = 1; }
                            if (var0 == 3) { var1 = 1; var3 = 1; }
                            if (var0 == 4) { var1 = 1; }
                            if (var0 == 5) { var3 = 1; }
                            iniFile.WriteValue("Shaders", "Shader_Field_Realism", $"{var1}");
                            iniFile.WriteValue("Shaders", "Shader_Field_Toon", $"{var2}");
                            iniFile.WriteValue("Shaders", "Shader_Field_Outlines", $"{var3}");
                        }
                        break;
                    case nameof(ShaderBattleChoice):
                        var0 = ShaderBattleChoice;
                        if (var0 >= 0)
                        {
                            iniFile.WriteValue("Shaders", "Enabled", "1");
                            if (var0 == 1) { var2 = 1; var3 = 1; }
                            if (var0 == 2) { var2 = 1; }
                            if (var0 == 3) { var1 = 1; var3 = 1; }
                            if (var0 == 4) { var1 = 1; }
                            if (var0 == 5) { var3 = 1; }
                            iniFile.WriteValue("Shaders", "Shader_Battle_Realism", $"{var1}");
                            iniFile.WriteValue("Shaders", "Shader_Battle_Toon", $"{var2}");
                            iniFile.WriteValue("Shaders", "Shader_Battle_Outlines", $"{var3}");
                        }
                        break;
                    case nameof(UsePsxFont):
                        if (UsePsxFont == 1)
                        {
                            iniFile.WriteValue("Font", "Enabled", "1");
                            iniFile.WriteValue("Font", "Names", "\"Alexandria\", \"Garnet\"");
                            FontChoice = _fontDefaultPSX;
                        }
                        else if (UsePsxFont == 0)
                        {
                            _usepsxfont = 0;
                            if (FontChoice.CompareTo(_fontDefaultPSX) == 0) FontChoice = _fontDefaultPC;
                        }
                        break;
                    case nameof(FontChoice):
                        if (FontChoice.CompareTo(_fontDefaultPC) == 0)
                        {
                            iniFile.WriteValue("Font", "Enabled", "0");
                            _usepsxfont = 0;
                        }
                        else if (FontChoice.CompareTo(_fontDefaultPSX) == 0)
                        {
                            iniFile.WriteValue("Font", "Enabled", "1");
                            iniFile.WriteValue("Font", "Names", "\"Alexandria\", \"Garnet\"");
                            _usepsxfont = 1;
                        }
                        else
                        {
                            iniFile.WriteValue("Font", "Enabled", "1");
                            iniFile.WriteValue("Font", "Names", "\"" + FontChoice + "\", \"Times Bold\"");
                            _usepsxfont = 0;
                        }
                        break;
                    case nameof(UIColumnsChoice):
                        var0 = UIColumnsChoice;
                        if (var0 >= 0)
                        {
                            iniFile.WriteValue("Interface", "Enabled", "1");
                            if (var0 == 0) { var1 = 8; var2 = 6; var3 = 5; }
                            if (var0 == 1) { var1 = 12; var2 = 9; var3 = 7; }
                            if (var0 == 2) { var1 = 16; var2 = 12; var3 = 8; }
                            iniFile.WriteValue("Interface", "MenuItemRowCount", $"{var1}");
                            iniFile.WriteValue("Interface", "MenuAbilityRowCount", $"{var2}");
                            iniFile.WriteValue("Interface", "MenuEquipRowCount", $"{var3}");
                            iniFile.WriteValue("Interface", "MenuChocographRowCount", $"{var3}");
                        }
                        break;
                    case nameof(BattleInterface):
                        iniFile.WriteValue("Interface", "BattleMenuPosX", $"{(Int32)BattleInterfaceMenu.X}");
                        iniFile.WriteValue("Interface", "BattleMenuPosY", $"{(Int32)BattleInterfaceMenu.Y}");
                        iniFile.WriteValue("Interface", "BattleMenuWidth", $"{(Int32)BattleInterfaceMenu.Width}");
                        iniFile.WriteValue("Interface", "BattleMenuHeight", $"{(Int32)BattleInterfaceMenu.Height}");
                        iniFile.WriteValue("Interface", "BattleDetailPosX", $"{(Int32)BattleInterfaceDetail.X}");
                        iniFile.WriteValue("Interface", "BattleDetailPosY", $"{(Int32)BattleInterfaceDetail.Y}");
                        iniFile.WriteValue("Interface", "BattleDetailWidth", $"{(Int32)BattleInterfaceDetail.Width}");
                        iniFile.WriteValue("Interface", "BattleDetailHeight", $"{(Int32)BattleInterfaceDetail.Height}");
                        iniFile.WriteValue("Interface", "BattleRowCount", $"{(BattleInterface == 2 ? 4 : 5)}");
                        iniFile.WriteValue("Interface", "BattleColumnCount", $"{(BattleInterface == 2 ? 1 : 1)}");
                        iniFile.WriteValue("Interface", "PSXBattleMenu", $"{(BattleInterface == 2 ? 1 : 0)}");
                        break;
                    case nameof(ATBModeChoice):
                        if (ATBModeChoice != 0)
                            iniFile.WriteValue("Battle", "Enabled", "1");
                        if (ATBModeChoice == 3)
                            iniFile.WriteValue("Battle", "Speed", "5");
                        else
                            iniFile.WriteValue("Battle", "Speed", $"{ATBModeChoice}");
                        break;
                    case nameof(TripleTriad):
                        var0 = TripleTriad;
                        if (var0 >= 0)
                        {
                            iniFile.WriteValue("TetraMaster", "Enabled", "1");
                            if (var0 == 0) { var1 = 0; var2 = 0; }
                            if (var0 == 1) { var1 = 0; var2 = 1; }
                            if (var0 == 2) { var1 = 1; var2 = 0; }
                            if (var0 == 3) { var1 = 2; var2 = 0; }
                            iniFile.WriteValue("TetraMaster", "TripleTriad", $"{var1}");
                            iniFile.WriteValue("TetraMaster", "ReduceRandom", $"{var2}");
                        }
                        break;
                    case nameof(WorldmapMistPreset):
                        var0 = WorldmapMistPreset;
                        if (var0 >= 0)
                        {
                            iniFile.WriteValue("Worldmap", "Enabled", "1");
                            if (var0 == 0) { var1 = 100; var2 = 55; var3 = 27; var4 = 80;  var5 = 7; }
                            if (var0 == 1) { var1 = 250; var2 = 10; var3 = 30; var4 = 200; var5 = 7; }
                            if (var0 == 2) { var1 = 400; var2 = 10; var3 = 50; var4 = 300; var5 = 4; }
                            if (var0 == 3) { var1 = 450; var2 = 0; var3 = 250; var4 = 330; var5 = 0; }
                            iniFile.WriteValue("Worldmap", "MistViewDistance", $"{var1}");
                            iniFile.WriteValue("Worldmap", "MistStartDistance_base", $"{var2}");
                            iniFile.WriteValue("Worldmap", "MistStartDistance", $"{var3}");
                            iniFile.WriteValue("Worldmap", "MistEndDistance", $"{var4}");
                            iniFile.WriteValue("Worldmap", "MistDensity", $"{var5}");
                        }
                        break;
                    case nameof(WorldmapDistancePreset):
                        var0 = WorldmapDistancePreset;
                        if (var0 >= 0)
                        {
                            iniFile.WriteValue("Worldmap", "Enabled", "1");
                            if (var0 == 0) { var1 = 100; var2 = 86; var3 = 142; }
                            if (var0 == 1) { var1 = 200; var2 = 150; var3 = 250; }
                            if (var0 == 2) { var1 = 300; var2 = 250; var3 = 350; }
                            if (var0 == 3) { var1 = 450; var2 = 400; var3 = 490; }
                            iniFile.WriteValue("Worldmap", "NoMistViewDistance", $"{var1}");
                            iniFile.WriteValue("Worldmap", "FogStartDistance", $"{var2}");
                            iniFile.WriteValue("Worldmap", "FogEndDistance", $"{var3}");
                        }
                        break;
                    case nameof(AccessBattleMenu):
                        iniFile.WriteValue("Battle", "AccessMenus", $"{AccessBattleMenu}");
                        iniFile.WriteValue("Battle", "AvailableMenus", AvailableBattleMenus);
                        if (AccessBattleMenu > 0)
                            iniFile.WriteValue("Battle", "Enabled", "1");
                        break;
                    case nameof(AutoSave):
                        iniFile.WriteValue("SaveFile", "DisableAutoSave", AutoSave == 1 ? "1" : "0");
                        iniFile.WriteValue("SaveFile", "AutoSaveOnlyAtMoogle", AutoSave == 2 ? "1" : "0");
                        break;
                }
            }
            catch (Exception ex)
            {
                UiHelper.ShowError(Application.Current.MainWindow, ex);
            }
        }

        #endregion

        #region LoadSettings

        public void LoadSettings()
        {
            try
            {
                //IniFile iniFile = new(IniFile.IniPath);
                IniReader iniReader = new IniReader(IniFile.IniPath);

                foreach (Object[] item in SettingsList)
                {
                    if (item[0] is String property && item[1] is String _property && item[2] is String name_ini && item[3] is String category && item[4] is Int32 valueZero && item[5] is Int32 valueOne && item[6] is Int32 defaultVal) //  
                    {
                        String INIvalueString = iniReader.GetSetting(category, name_ini);
                        PropertyInfo field = this.GetType().GetProperty(property);
                        if (field != null)
                        {
                            if (String.IsNullOrEmpty(INIvalueString))
                                INIvalueString = defaultVal.ToString();
                            if (!Int16.TryParse(INIvalueString, out Int16 val3))
                                val3 = (Int16)defaultVal;
                            if (val3 == valueZero)
                                field.SetValue(this, (Int16)0);
                            else if (val3 == valueOne)
                                field.SetValue(this, (Int16)1);
                            else
                                field.SetValue(this, val3);

                            Refresh(property);
                        }
                    }
                }

                String value = iniReader.GetSetting("Worldmap", "MistViewDistance");
                Boolean value1isInt = Int16.TryParse(value, out Int16 value1);
                value = iniReader.GetSetting("Worldmap", "MistStartDistance_base");
                Boolean value2isInt = Int16.TryParse(value, out Int16 value2);
                value = iniReader.GetSetting("Worldmap", "MistStartDistance");
                Boolean value3isInt = Int16.TryParse(value, out Int16 value3);
                value = iniReader.GetSetting("Worldmap", "MistEndDistance");
                Boolean value4isInt = Int16.TryParse(value, out Int16 value4);
                value = iniReader.GetSetting("Worldmap", "MistDensity");
                Boolean value5isInt = Int16.TryParse(value, out Int16 value5);
                if (value1 == 100 && value2 == 55 && value3 == 27 && value4 == 80 && value5 == 7)
                    _worldmapmistpreset = 0;
                else if (value1 == 250 && value2 == 10 && value3 == 30 && value4 == 200 && value5 == 7)
                    _worldmapmistpreset = 1;
                else if (value1 == 400 && value2 == 10 && value3 == 50 && value4 == 300 && value5 == 4)
                    _worldmapmistpreset = 2;
                else if (value1 == 450 && value2 == 0 && value3 == 250 && value4 == 330 && value5 == 0)
                    _worldmapmistpreset = 3;
                else
                    _worldmapmistpreset = -1;
                Refresh(nameof(WorldmapMistPreset));

                value = iniReader.GetSetting("Graphics", "BattleSwirlFrames");
                if (String.IsNullOrEmpty(value))
                    value = " 0";
                value1isInt = Int16.TryParse(value, out value1);
                if (value1isInt)
                {
                    BattleSwirlFrames = (value1 == 0) ? (Int16)1 : (Int16)0; // activates BattleSwirlFrames only when frames are zero
                }
                Refresh(nameof(BattleSwirlFrames));



                value = iniReader.GetSetting("Cheats", "SpeedMode");
                if (String.IsNullOrEmpty(value))
                {
                    value = " 1";
                }
                value1isInt = Int16.TryParse(value, out value1);
                if (value1isInt && value1 == 0)
                {
                    SpeedMode = 0;
                    SpeedFactor = 1;
                }
                else
                {
                    SpeedMode = 1;
                    value = iniReader.GetSetting("Cheats", "SpeedFactor");
                    if (String.IsNullOrEmpty(value))
                    {
                        value = " 3";
                    }
                    if (!Int16.TryParse(value, out _speedfactor))
                        _speedfactor = 3;
                }
                Refresh(nameof(SpeedMode));
                Refresh(nameof(SpeedFactor));

                value = iniReader.GetSetting("Graphics", "FieldFPS");
                value1isInt = Int16.TryParse(value, out value1);
                value = iniReader.GetSetting("Graphics", "BattleFPS");
                value2isInt = Int16.TryParse(value, out value2);
                value = iniReader.GetSetting("Graphics", "WorldFPS");
                value3isInt = Int16.TryParse(value, out value3);
                if (value1isInt && value2isInt && value3isInt)
                {
                    if (value1 == 30 && value2 == 15 && value3 == 20)
                        _fpsdropboxchoice = 0;
                    else if (value1 == 30 && value2 == 30 && value3 == 30)
                        _fpsdropboxchoice = 1;
                    else if (value1 == 60 && value2 == 60 && value3 == 60)
                        _fpsdropboxchoice = 2;
                    else if (value1 == 90 && value2 == 90 && value3 == 90)
                        _fpsdropboxchoice = 3;
                    else if (value1 == 120 && value2 == 120 && value3 == 120)
                        _fpsdropboxchoice = 4;
                    else
                        _fpsdropboxchoice = -1;
                }
                Refresh(nameof(FPSDropboxChoice));

                String valueMenuPos = iniReader.GetSetting("Interface", "BattleMenuPosX");
                String valuePSXMenu = iniReader.GetSetting("Interface", "PSXBattleMenu");
                Int32 menuPosX = -400;
                Int32 psxMenu = 0;
                if (!String.IsNullOrEmpty(valueMenuPos))
                    if (!Int32.TryParse(valueMenuPos, out menuPosX))
                        menuPosX = -400;
                if (!String.IsNullOrEmpty(valuePSXMenu))
                    if (!Int32.TryParse(valuePSXMenu, out psxMenu))
                        psxMenu = 0;
                if (psxMenu > 0)
                    _battleInterface = 2;
                else if (menuPosX != -400)
                    _battleInterface = 1;
                else
                    _battleInterface = 0;
                Refresh(nameof(BattleInterface));

                value = iniReader.GetSetting("Interface", "MenuItemRowCount");
                value1isInt = Int16.TryParse(value, out value1);
                value = iniReader.GetSetting("Interface", "MenuAbilityRowCount");
                value2isInt = Int16.TryParse(value, out value2);
                value = iniReader.GetSetting("Interface", "MenuEquipRowCount");
                value3isInt = Int16.TryParse(value, out value3);
                value = iniReader.GetSetting("Interface", "MenuChocographRowCount");
                value4isInt = Int16.TryParse(value, out value4);
                if (value1 == 8 && value2 == 6 && value3 == 5 && value4 == 5)
                    _uicolumnschoice = 0;
                else if (value1 == 12 && value2 == 9 && value3 == 7 && value4 == 7)
                    _uicolumnschoice = 1;
                else if (value1 == 16 && value2 == 12 && value3 == 8 && value4 == 8)
                    _uicolumnschoice = 2;
                else
                    _uicolumnschoice = -1;
                Refresh(nameof(UIColumnsChoice));

                value = iniReader.GetSetting("Battle", "Speed");
                if (String.IsNullOrEmpty(value))
                {
                    value = " 0";
                    OnPropertyChanged("ATBModeChoice");
                }
                if (!Int16.TryParse(value, out _atbmodechoice))
                    _atbmodechoice = 0;
                else if (_atbmodechoice > 3)
                    _atbmodechoice = 3;

                value = iniReader.GetSetting("TetraMaster", "TripleTriad");
                value1isInt = Int16.TryParse(value, out value1);
                value = iniReader.GetSetting("TetraMaster", "ReduceRandom");
                value2isInt = Int16.TryParse(value, out value2);
                if (value1 == 0 && value2 == 0)
                    _tripleTriad = 0;
                else if (value1 == 0 && value2 > 0)
                    _tripleTriad = 1;
                else if (value1 == 1)
                    _tripleTriad = 2;
                else if (value1 == 2)
                    _tripleTriad = 3;
                else
                    _tripleTriad = -1;
                Refresh(nameof(TripleTriad));

                value = iniReader.GetSetting("Font", "Enabled");
                if (String.IsNullOrEmpty(value) || !Int16.TryParse(value, out Int16 enabledFont) || enabledFont == 0)
                {
                    _fontChoice = _fontDefaultPC;
                    _usepsxfont = 0;
                }
                else
                {
                    value = iniReader.GetSetting("Font", "Names");
                    if (String.IsNullOrEmpty(value) || value.Length < 2)
                    {
                        _fontChoice = _fontDefaultPC;
                        _usepsxfont = 0;
                    }
                    else
                    {
                        String[] fontList = value.Trim('"').Split(new[] { "\", \"" }, StringSplitOptions.None);
                        _fontChoice = fontList[0];
                        if (_fontChoice.CompareTo("Alexandria") == 0 || _fontChoice.CompareTo("Garnet") == 0)
                        {
                            _fontChoice = _fontDefaultPSX;
                            _usepsxfont = 1;
                        }
                        else
                        {
                            _usepsxfont = 0;
                        }
                    }
                }
                Refresh(nameof(UsePsxFont));
                Refresh(nameof(FontChoice));

                value = iniReader.GetSetting("Shaders", "Shader_Field_Realism");
                value1isInt = Int16.TryParse(value, out value1);
                value = iniReader.GetSetting("Shaders", "Shader_Field_Toon");
                value2isInt = Int16.TryParse(value, out value2);
                value = iniReader.GetSetting("Shaders", "Shader_Field_Outlines");
                value3isInt = Int16.TryParse(value, out value3);
                if (value1 == 0 && value2 == 0 && value3 == 0)
                    _shaderfieldchoice = 0;
                else if (value1 == 0 && value2 == 1 && value3 == 1)
                    _shaderfieldchoice = 1;
                else if (value1 == 0 && value2 == 1 && value3 == 0)
                    _shaderfieldchoice = 2;
                else if (value1 == 1 && value2 == 0 && value3 == 1)
                    _shaderfieldchoice = 3;
                else if (value1 == 1 && value2 == 0 && value3 == 0)
                    _shaderfieldchoice = 4;
                else if (value1 == 0 && value2 == 0 && value3 == 1)
                    _shaderfieldchoice = 5;
                else
                    _shaderfieldchoice = -1;
                Refresh(nameof(ShaderFieldChoice));

                value = iniReader.GetSetting("Shaders", "Shader_Battle_Realism");
                value1isInt = Int16.TryParse(value, out value1);
                value = iniReader.GetSetting("Shaders", "Shader_Battle_Toon");
                value2isInt = Int16.TryParse(value, out value2);
                value = iniReader.GetSetting("Shaders", "Shader_Battle_Outlines");
                value3isInt = Int16.TryParse(value, out value3);
                if (value1 == 0 && value2 == 0 && value3 == 0)
                    _shaderbattlechoice = 0;
                else if (value1 == 0 && value2 == 1 && value3 == 1)
                    _shaderbattlechoice = 1;
                else if (value1 == 0 && value2 == 1 && value3 == 0)
                    _shaderbattlechoice = 2;
                else if (value1 == 1 && value2 == 0 && value3 == 1)
                    _shaderbattlechoice = 3;
                else if (value1 == 1 && value2 == 0 && value3 == 0)
                    _shaderbattlechoice = 4;
                else if (value1 == 0 && value2 == 0 && value3 == 1)
                    _shaderbattlechoice = 5;
                else
                    _shaderbattlechoice = -1;
                Refresh(nameof(ShaderBattleChoice));

                value = iniReader.GetSetting("Worldmap", "NoMistViewDistance");
                value1isInt = Int16.TryParse(value, out value1);
                value = iniReader.GetSetting("Worldmap", "FogStartDistance");
                value2isInt = Int16.TryParse(value, out value2);
                value = iniReader.GetSetting("Worldmap", "FogEndDistance");
                value3isInt = Int16.TryParse(value, out value3);
                if (value1 == 100 && value2 == 86 && value3 == 142)
                    _worldmapdistancepreset = 0;
                else if (value1 == 200 && value2 == 150 && value3 == 250)
                    _worldmapdistancepreset = 1;
                else if (value1 == 300 && value2 == 250 && value3 == 350)
                    _worldmapdistancepreset = 2;
                else if (value1 == 450 && value2 == 400 && value3 == 490)
                    _worldmapdistancepreset = 3;
                else
                    _worldmapdistancepreset = -1;
                Refresh(nameof(WorldmapDistancePreset));


                value = iniReader.GetSetting("Battle", "AccessMenus");
                if (String.IsNullOrEmpty(value))
                {
                    value = " 0";
                    OnPropertyChanged(nameof(AccessBattleMenu));
                }
                if (!Int16.TryParse(value, out _accessBattleMenu))
                    _accessBattleMenu = 0;
                Refresh(nameof(AccessBattleMenu));


                value = iniReader.GetSetting("SaveFile", "DisableAutoSave");
                value1isInt = Int16.TryParse(value, out value1);
                value = iniReader.GetSetting("SaveFile", "AutoSaveOnlyAtMoogle");
                value2isInt = Int16.TryParse(value, out value2);
                if (!value1isInt) value1 = 0;
                if (!value2isInt) value2 = 0;
                _autoSave = (short)(value1 == 1 ? 1 : value2 == 0 ? 0 : 2);
                Refresh(nameof(AutoSave));
            }
            catch (Exception ex)
            {
                UiHelper.ShowError(Application.Current.MainWindow, ex);
            }
        }
        #endregion

        #region generic func
        private async void Refresh([CallerMemberName] String propertyName = null)
        {
            try
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            catch (Exception ex)
            {
                UiHelper.ShowError(Application.Current.MainWindow, ex);
            }
        }
        protected bool SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                OnPropertyChanged(propertyName);
                return true;
            }
            return false;
        }

        #endregion

    }
}
