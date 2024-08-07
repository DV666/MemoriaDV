﻿namespace Memoria.Data
{
    // Warning: these must never be renamed for a correct reading of the .seq files
    // If required, add an alias to an existing entry
    public enum SpecialEffect
    {
        Special_No_Effect = -1,
        Fire__Single = 0,
        Fire__Multi = 70,
        Fira__Single = 2,
        Fira__Multi = 71,
        Firaga__Single = 7,
        Firaga__Multi = 72,
        Sleep__Single = 26,
        Sleep__Multi = 78,
        Blizzard__Single = 6,
        Blizzard__Multi = 62,
        Blizzara__Single = 4,
        Blizzara__Multi = 77,
        Blizzaga__Single = 151,
        Blizzaga__Multi = 152,
        Slow = 126, // Special case: in vanilla (Steam), a clock sprite and a screenshot twist are hidden; also there are more lights in the PSX version
        Thunder__Single = 3,
        Thunder__Multi = 66,
        Thundera__Single = 5,
        Thundara__Multi = 73,
        Thundaga__Single = 24,
        Thundaga__Multi = 133,
        Stop = 149, // Special case: in vanilla (Steam), a black sprite is hidden
        Aero = 180,
        Aera_1 = 502, // Griffin, Zuu
        Aera_2 = 205, // Lani, Jabberwock, Gimme Cat, Yan
        Poison__Single = 21,
        Poison__Multi = 202,
        Bio__Single = 93,
        Bio__Multi = 120,
        Osmose = 57,
        Drain = 132,
        Demi__Single = 98,
        Demi__Multi = 99,
        Comet = 56,
        Death = 94,
        Break = 153,
        Water__Single = 22,
        Water__Multi = 95,
        Waterga = 427,
        Meteor__Success = 134,
        Meteor__Fail = 143,
        Flare = 125,
        Flare_Star = 436,
        Doomsday = 308,
        Focus = 199,
        Cure__Single = 8,
        Cure__Multi = 9,
        Cura__Single = 141,
        Cura__Multi = 162,
        Curaga__Single = 142,
        Curaga__Multi = 144,
        Regen = 32,
        Life = 17,
        Full_Life = 15,
        Scan = 131,
        Panacea = 12,
        Stona = 13,
        Esuna = 11,
        Shell = 63,
        Protect = 123,
        Haste = 89,
        Silence__Single = 42,
        Silence__Multi = 127,
        Mini__Single = 25,
        Mini__Multi = 124,
        Reflect = 79,
        Confuse__Single = 96,
        Confuse__Multi = 97,
        Berserk__Single = 14,
        Berserk__Multi = 140,
        Blind__Single = 158,
        Blind__Multi = 157,
        Float__Single = 27,
        Float__Multi = 76,
        Dispel = 128,
        Jewel = 201,
        Might = 90,
        Holy = 87,
        Shiva__Short = 407,
        Shiva__Full = 38,
        Ifrit__Short = 445,
        Ifrit__Full = 276,
        Ramuh__Short = 415,
        Ramuh__Full = 186,
        Atomos__Short = 446,
        Atomos__Full = 184,
        Odin__Short = 424,
        Odin__Full = 261,
        Leviathan__Short = 406,
        Leviathan__Full = 179,
        Bahamut__Short = 405,
        Bahamut__Full = 227,
        Ark__Short = 447,
        Ark__Full = 381, // Special case: render priority of blend modes (Opa, Sub, Add) is switched during animation; also the sounds are a mess
        Carbuncle_Ruby__Short = 504,
        Carbuncle_Ruby__Full = 177,
        Carbuncle_Pearl__Short = 505,
        Carbuncle_Pearl__Full = 493,
        Carbuncle_Emerald__Short = 506,
        Carbuncle_Emerald__Full = 494,
        Carbuncle_Diamond__Short = 507,
        Carbuncle_Diamond__Full = 495,
        Fenrir_Earth__Short = 508,
        Fenrir_Earth__Full = 210,
        Fenrir_Wind__Short = 509,
        Fenrir_Wind__Full = 226,
        Phoenix__Short = 510,
        Phoenix__Full = 211,
        Phoenix_Rebirth_Flame = 225,
        Madeen__Short = 378,
        Madeen__Full = 251,
        Eat = 254,
        Cook = 255,
        Goblin_Punch = 74,
        LV5_Death = 154,
        LV4_Holy = 129,
        LV3_Def_less = 392,
        Doom = 236,
        Roulette = 237,
        Aqua_Breath = 28,
        Mighty_Guard = 81,
        Matra_Magic = 88,
        Bad_Breath = 44,
        Limit_Glove = 82,
        Thousand_Needles = 30,
        Pumpkin_Head = 297,
        Night = 501,
        Twister = 178,
        Earth_Shake = 390,
        Angels_Snack = 45,
        Frog_Drop = 228,
        White_Wind = 41,
        Vanish = 500,
        Frost = 150,
        Mustard_Bomb = 187,
        Magic_Hammer = 130,
        Auto_Life = 138,
        Darkside_1 = 188,
        Darkside_2 = 409,
        Minus_Strike_1 = 189,
        Minus_Strike_2 = 410,
        Iai_Strike_1 = 190,
        Iai_Strike_2 = 411,
        Power_Break = 208,
        Armor_Break = 92,
        Mental_Break = 209,
        Magic_Break = 46,
        Charge = 398,
        Thunder_Slash_1 = 191,
        Thunder_Slash_2 = 412,
        Thunder_Slash_3 = 457,
        Stock_Break_1 = 207,
        Stock_Break_2 = 414,
        Stock_Break_3 = 459,
        Climhazzard_1 = 397,
        Climhazzard_2 = 417,
        Climhazzard_3 = 443,
        Shock_1 = 192,
        Shock_2 = 413,
        Shock_3 = 458,
        Fire_Sword = 212,
        Fira_Sword = 213,
        Firaga_Sword = 214,
        Thunder_Sword = 215,
        Thundara_Sword = 216,
        Thundaga_Sword = 217,
        Blizzard_Sword = 218,
        Blizzara_Sword = 219,
        Blizzaga_Sword = 220,
        Bio_Sword = 221,
        Water_Sword = 222,
        Flare_Sword = 223,
        Doomsday_Sword = 224,
        Steal_Zidane_Dagger = 200,
        Steal_Zidane_Sword = 273,
        Steal_Cinna = 19,
        Steal_Marcus = 119,
        Steal_Blank = 20,
        Flee_Skill = 249,
        Detect = 250, // Special case: UV is moved during animation to open the eye texture
        Whats_That = 399,
        Soul_Blade = 229,
        Annoy = 233,
        Sacrifice = 234,
        Lucky_Seven = 312,
        Thievery = 231,
        Free_Energy = 241,
        Tidal_Flame = 242,
        Scoop_Art = 243,
        Shift_Break = 244,
        Stellar_Circle_5 = 245,
        Meo_Twister = 246,
        Solution_9 = 247,
        Grand_Lethal = 248,
        Jump = 393,
        Spear = 388,
        Spear_Trance = 389,
        Lancer = 401,
        Reis_Wind = 168,
        Dragon_Breath = 296,
        White_Draw = 83,
        Luna = 61,
        Six_Dragons = 491,
        Cherry_Blossom = 387,
        Dragons_Crest = 490,
        Throw_Racket = 265,
        Throw_Thief_Sword = 266,
        Throw_Sword = 267,
        Throw_Spear = 268,
        Throw_Claw = 269,
        Throw_Rod = 270,
        Throw_Fork = 271,
        Throw_Dagger = 272,
        Throw_Disc = 277,
        Chakra__Single = 85,
        Chakra__Multi = 86,
        Spare_Change__Single = 135,
        Spare_Change__Multi = 136,
        No_Mercy__Single = 193,
        No_Mercy__Multi = 194,
        Aura__Single = 197,
        Aura__Multi = 198,
        Curse__Single = 195,
        Curse__Multi = 196,
        Revive__Single = 137,
        Revive__Multi = 421,
        Demi_Shock__Single = 121,
        Demi_Shock__Multi = 122,
        Countdown__Single = 385,
        Countdown__Multi = 386,
        Ether = 278,
        Elixir = 40,
        Echo_Screen = 280,
        Eye_Drops = 279,
        Magic_Tag = 281,
        Vaccine = 282,
        Annoyntment = 283,
        Dark_Matter = 287,
        Gysahl_Greens = 289,
        Dead_Pepper = 288,
        Tent = 290,
        Ore = 292,
        Pyro = 159,
        Poly = 293,
        Medeo = 377,
        Instant = 16,
        Venom_Powder = 10,
        High_Wind = 284,
        Melt = 383,
        Darkness = 55,
        Snowflakes = 23, // Devil's Candle (Ice)
        Burning_Oil = 53, // Devil's Candle (Fire)
        Explosion = 31, // Friendly Yan's attack
        Smoke = 33, // Tantarian's "Open"
        Red_Smoke = 307, // Tantarian's "Close" and Tonberry's disappear
        Green_Smoke = 306, // Cactuar's Hide/Appearance
        Flame_1 = 47, // Grenade, Ogre
        Flame_2 = 311, // Cerberus, Ironite, Worm Hydra
        Pollen = 48,
        Spore = 165,
        Gradual_Petrify = 49,
        Glowing_Eyes = 50,
        Blowup = 54,
        Earthquake = 58,
        Trouble_Juice = 59,
        Rainbow_Storm = 60, // Dendrobium's Wind
        Bubbles = 64,
        Whirl_Slash = 65,
        Ultra_Sound_Wave = 68,
        Sandstorm = 185,
        Sandstorm_Antlion = 75,
        Aerial_Slash_Silver_Dragon = 69,
        Aerial_Slash_Gargoyle = 29,
        Aerial_Slash_Garuda = 36,
        Aerial_Slash_Red_Dragon = 43,
        Aerial_Slash_Nova_Dragon = 139,
        Entice = 145,
        Hypnotize = 146,
        Cannon = 147,
        Web = 160,
        Blaster = 161,
        Electrocute = 163,
        Buzz = 164,
        Sweep = 166,
        Gnoll_Attack = 156,
        Delta_Attack = 148,
        Snowstorm = 170,
        Maelstrom = 171,
        Sleeping_Juice = 172,
        Lightning = 169,
        Cold_Breath = 52,
        Aero_Breath = 173,
        Venom_Breath = 174,
        Zombie_Breath = 175,
        Leaf_Swirl = 176,
        Rainbow_Wind = 181,
        Scorch = 182,
        Heavy = 183,
        Nanoflare = 203,
        Meteorite = 204,
        Screech = 206,
        Stone = 230,
        Psychokinesis = 235,
        Psychokinesis_Garland = 35,
        Mirror = 238,
        Red_Clipper = 239,
        Zombie_Powder = 240,
        Solution = 256,
        Rise = 258,
        Mist = 259,
        Everyones_Grudge = 260,
        Mucus = 262,
        Devour = 274, // Mimic's. Special case: a texture of the target is generated by taking a screenshot (camera-dependant)
        Virus_Combo = 275,
        Absorb_Prison_Cage = 285,
        Raining_Swords = 291,
        Boomerang = 294,
        Wave = 295,
        Jet_Fire = 298,
        Photon = 299,
        Earthquake_Strong = 300,
        Sinkhole = 301, // Antlion's death. Special case: instead of disappearing, the caster sinks and the ground texture becomes opaque
        Paper_Storm = 303,
        Propeller_Wind = 304,
        Accumulate = 305, // Sand Golem's Revival
        Dust_Storm = 433, // Sand Golem's Death
        Curse = 309, // Also Bad Breath (Malboro)
        Absorb = 310,
        Chestnut = 382,
        Fire_Blades = 391,
        Silent_Voice = 395, // Special case: in vanilla (Steam), a wave effect is missing
        Light_Float = 396,
        Viral_Smoke = 400,
        Counter_Horn = 403,
        Shockwave = 404,
        Tsunami = 408,
        Flaming_Sword = 416,
        Trouble_Mucus = 419,
        Ink = 67,
        Water_Gun = 167,
        Water_Gun_x3 = 422,
        Lava_Gun_x6 = 155,
        Mind_Blast = 423,
        Virus_Fly = 286,
        Virus_Powder = 425,
        Virus_Crunch = 428,
        Octopus_Suction = 429, // Gigan Octopus's 6 Legs
        Tidal_Wave = 434,
        Grand_Cross = 498,
        Neutron_Ring = 499,
        Blue_Shockwave = 503,
        Ragtime_Correct = 486,
        Ragtime_Wrong = 487,
        Snort = 51,
        Swallow = 302,
        Escape = 232,
        Dummy = 437, // Smoke bomb saying "Dummy"
        Special_Prepare_Attack = 253,
        Special_Flee = 252,
        Special_Trance_Activate = 257,
        Special_Trance_End = 489,
        Special_Trance_Kuja = 394,
        Special_Curaga = 1,
        Special_Steiner_VS_Blank = 418,
        Special_Prison_Cage_Death = 402,
        Special_Sealion_Engage = 460,
        Special_Gizamaluke_Engage = 441,
        Special_Gizamaluke_Death = 440,
        Special_Valia_Pira_Death = 431,
        Special_Krakens_Arm_Death = 34,
        Special_Ultima_Memoria = 384,
        Special_Ultima_Terra = 492,
        Special_Silver_Dragon_Death = 438,
        Special_Nova_Dragon_Death = 439,
        Special_Deathguise_Death = 420,
        Special_Necron_Death = 435, // Special case: custom textures are loaded (PSXTextureMgr.SpEff435)
        Special_Necron_Engage = 496,
        Special_Necron_Teleport = 497,
        Special_Ozma_Death = 432,
        Player_Attack_Zidane_Dagger = 100,
        Player_Attack_Zidane_Sword,
        Player_Attack_Vivi,
        Player_Attack_Garnet_LH_Rod,
        Player_Attack_Garnet_LH_Racket,
        Player_Attack_Garnet_SH_Rod,
        Player_Attack_Garnet_SH_Racket,
        Player_Attack_Steiner,
        Player_Attack_Steiner_2,
        Player_Attack_Quina,
        Player_Attack_Eiko_Flute,
        Player_Attack_Eiko_Racket,
        Player_Attack_Freya,
        Player_Attack_Amarant,
        Player_Attack_Cinna,
        Player_Attack_Marcus,
        Player_Attack_Blank,
        Player_Attack_Blank_Pluto,
        Player_Attack_Beatrix,
        SFX_Attack_Rod = 313, // Knock (Drakan, Nymph), Call (Mimic), Garnet mirror
        SFX_Attack_Mythril_Rod,
        SFX_Attack_Stardust_Rod,
        SFX_Attack_Healing_Rod, // Power up (Zorn & Thorn)
        SFX_Attack_Asura_Rod,
        SFX_Attack_Wizard_Rod, // Hit (Black Waltz 3)
        SFX_Attack_Whale_Whisker,
        SFX_Attack_Golem_Flute, // Charge (Fang, Hedgehog Pie), Eiko mirror
        SFX_Attack_Lamia_Flute,
        SFX_Attack_Fairy_Flute, // Spear (Troll, Stilva, Ralvurahva, Red Vepal, Amdusias)
        SFX_Attack_Hamelin,
        SFX_Attack_Siren_Flute, // Body Ram (Green Vepal)
        SFX_Attack_Angel_Flute, // Wings (Meltigemini)
        SFX_Attack_Mage_Staff, // Strike (Bandersnatch, Feather Circle, Black Waltz 1 & 2, Type A, Magic Vice, Hilgigars, Prison Cage, Plant Spider, Wyerd, Zombie, Axolotl, Mu, Grimlock, Blazer Beetle), Vivi mirror
        SFX_Attack_Flame_Staff, // Stab (Ralvuimago), Ragtime Mouse's death
        SFX_Attack_Ice_Staff, // Stomach (Crawler)
        SFX_Attack_Lightning_Staff, // Fang (Fang)
        SFX_Attack_Oak_Staff, // Hit (Sand Golem, Malboro, Cactuar)
        SFX_Attack_Cypress_Pile, // Hiphop (Hilgigars)
        SFX_Attack_Octagon_Rod, // Edge (Tantarian)
        SFX_Attack_High_Mage_Staff, // Strike (Nova Dragon)
        SFX_Attack_Mace_Of_Zeus,
        SFX_Attack_Fork, // Thrust (Armodullahan, Sand Scorpion, Hornet, Trick Sparrow, Ogre, Serpion, Hedgehog Pie, Axe Beak), Quina mirror
        SFX_Attack_Needle_Fork, // Rolling Attack (Quale)
        SFX_Attack_Mythril_Fork,
        SFX_Attack_Silver_Fork,
        SFX_Attack_Bistro_Fork,
        SFX_Attack_Gastro_Fork,
        SFX_Attack_Air_Racket,
        SFX_Attack_Multina_Racket,
        SFX_Attack_Magic_Racket,
        SFX_Attack_Mythril_Racket,
        SFX_Attack_Priest_Racket,
        SFX_Attack_Tiger_Racket,
        SFX_Attack_Dagger, // Claw (Sand Scorpion, -Neros), Zidane mirror
        SFX_Attack_Mage_Masher,
        SFX_Attack_Mythril_Dagger,
        SFX_Attack_Gladius,
        SFX_Attack_Zorlin_Shape,
        SFX_Attack_Orichalcon, // Death Cutter (Lich)
        SFX_Attack_Butterfly_Sword, // Vertical cut (Ironite, Goblin, Skeleton, Sahagin, Carve Spider)
        SFX_Attack_The_Ogre,
        SFX_Attack_Exploda,
        SFX_Attack_Rune_Tooth,
        SFX_Attack_Angel_Bless,
        SFX_Attack_Sargatanas,
        SFX_Attack_Masamune,
        SFX_Attack_The_Tower,
        SFX_Attack_Ultima_Weapon,
        SFX_Attack_Broadsword, // Sword (Soldier, Baku, Veteran, Crawler, Zuu, Dragonfly, Red Dragon, Cave Imp), Steiner mirror
        SFX_Attack_Iron_Sword, // Vertical hit (Lani, Seeker Bat)
        SFX_Attack_Mythril_Sword,
        SFX_Attack_Blood_Sword, // Suck (Seeker Bat)
        SFX_Attack_Ice_Brand,
        SFX_Attack_Coral_Sword,
        SFX_Attack_Diamond_Sword, // Verical blade (Iron Man, Ogre)
        SFX_Attack_Flame_Saber,
        SFX_Attack_Rune_Blade,
        SFX_Attack_Defender, // Judgment Sword (Hades)
        SFX_Attack_Save_The_Queen, // Hit (Bandersnatch, Beatrix, Baku, Iron Man, Behemoth, Plant Brain, Griffin, Abomination, Gizamaluke, Sealion, Stroper, Taharka, Gargoyle, Shell Dragon, Catoblepas, Yeti, Bomb, Adamantoise)
        SFX_Attack_Ultima_Sword, // Slam (Mistodon, Kraken)
        SFX_Attack_Excalibur,
        SFX_Attack_Ragnarok,
        SFX_Attack_Excalibur2,
        SFX_Attack_Javelin = 461, // Short blade (Lamia, Vice, Blazer Beetle), Freya mirror
        SFX_Attack_Mythril_Spear,
        SFX_Attack_Partisan,
        SFX_Attack_Ice_Lance,
        SFX_Attack_Trident, // Heave (Zaghnol)
        SFX_Attack_Heavy_Lance, // Blade (Abadon)
        SFX_Attack_Obelisk,
        SFX_Attack_Holy_Lance,
        SFX_Attack_Kain_Lance, // Taste steel! (King Leo)
        SFX_Attack_Dragon_Hair,
        SFX_Attack_Cat_Claws, // Strike (Cerberus, Behemoth, Dracozombie, Silver Dragon), Amarant mirror
        SFX_Attack_Poison_Knuckles,
        SFX_Attack_Mythril_Claws,
        SFX_Attack_Scissor_Fangs,
        SFX_Attack_Dragon_Claws, // Poison Claw (Grand Dragon)
        SFX_Attack_Tiger_Fangs,
        SFX_Attack_Avenger,
        SFX_Attack_Kaiser_Knuckles, // Demon's Claw (Deathguise)
        SFX_Attack_Duel_Claws, // Silent Claw (Tiamat)
        SFX_Attack_Rune_Claws,
        SFX_Attack_Hammer, // Hammer
        SFX_Attack_Charge_All, // Charge on full party (Shell Dragon, Whale Zombie, Deathguise)
        SFX_Attack_Cleave, // Cleave (Iron Man, Hades)
        SFX_Attack_Charge_And_Strike, // Charge (Flan, Myconid, Python, Yeti, Basilisk, Ochu)
        SFX_Attack_Blade_Long, // Blade (Maliris, Earth Guardian, Taharka, Clipper, Lizard Man)
        // These mostly bug when tried; better not play them
        Unused_18 = 18, // Would apply effect instantly
        Unused_37 = 37, // Would never end
        Unused_39 = 39, // Would never end
        Unused_80 = 80, // Would run casting animation & apply effect
        Unused_84 = 84, // Would run casting animation & apply effect
        Unused_91 = 91, // Would run casting animation & apply effect
        Unused_263 = 263, // Would rerun last effect used
        Unused_264 = 264, // Would rerun last effect used
        Unused_379 = 379, // Would rerun last effect used
        Unused_380 = 380, // Would rerun last effect used
        Unused_426 = 426, // Would rerun last effect used
        Unused_430 = 430, // Would rerun last effect used
        Unused_442 = 442, // Would rerun last effect used
        Unused_444 = 444, // Would rerun last effect used
        Unused_448 = 448, // Would rerun last effect used
        Unused_449 = 449, // Would rerun last effect used
        Unused_450 = 450, // Would rerun last effect used
        Unused_451 = 451, // Would rerun last effect used
        Unused_452 = 452, // Would rerun last effect used
        Unused_453 = 453, // Would rerun last effect used
        Unused_454 = 454, // Would rerun last effect used
        Unused_455 = 455, // Would rerun last effect used
        Unused_456 = 456, // Would rerun last effect used
        Unused_488 = 488, // Would rerun last effect used
    }
}
