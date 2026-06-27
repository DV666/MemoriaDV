using Assets.Scripts.Common;
using Assets.Sources.Scripts.UI.Common;
using Memoria;
using Memoria.Assets;
using Memoria.Prime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Memoria.Scripts.TranceSeek
{
    public class TranceSeekCheckBattleStat : MonoBehaviour
    {
        private class StatCache
        {
            public byte Level;
            public byte Strength;
            public byte Magic;
            public byte Dexterity;
            public byte Will;
            public short CriticalRateBonus;
            public short CriticalRateResistance;
            public int PhysicalDefence;
            public int PhysicalEvade;
            public int MagicDefence;
            public int MagicEvade;

            public bool WasOldStatus;
        }

        private Dictionary<BTL_DATA, StatCache> _unitCache = new Dictionary<BTL_DATA, StatCache>();
        private float _timer = 0f;
        private const float CheckInterval = 1f; // 1 second

        public static Boolean GreenRedColor_SubModCheck = Configuration.Mod.FolderNames.Contains("TranceSeek/Options/StatModifierIndicator/GreenRed");
        public static Boolean YellowPurple_SubModCheck = Configuration.Mod.FolderNames.Contains("TranceSeek/Options/StatModifierIndicator/YellowPurple");

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < CheckInterval) return;
            _timer = 0f;

            if (!SceneDirector.IsBattleScene())
            {
                Destroy(this);
                return;
            }

            if (FF9StateSystem.Battle.FF9Battle == null || FF9StateSystem.Battle.FF9Battle.btl_list == null)
                return;

            int invincibleTargetId = 0;
            int dodgeALLTargetId = 0;

            int immunestealTargetId = 0;
            int hidestatmodifTargetId = 0;
            if (FF9StateSystem.EventState.gScriptDictionary.TryGetValue(1000, out Dictionary<int, int> dictbattle))
            {
                dictbattle.TryGetValue(7, out invincibleTargetId);
                dictbattle.TryGetValue(8, out dodgeALLTargetId);
                dictbattle.TryGetValue(9, out immunestealTargetId);
                dictbattle.TryGetValue(10, out hidestatmodifTargetId);
            }

            foreach (BattleUnit unit in BattleState.EnumerateUnits())
            {
                if (unit.Data == null) continue;

                if (!_unitCache.TryGetValue(unit.Data, out StatCache cached))
                {
                    _unitCache[unit.Data] = new StatCache
                    {
                        Level = unit.Level,
                        Strength = unit.Strength,
                        Magic = unit.Magic,
                        Dexterity = unit.Dexterity,
                        Will = unit.Will,
                        CriticalRateBonus = unit.CriticalRateBonus,
                        CriticalRateResistance = unit.CriticalRateResistance,
                        PhysicalDefence = unit.PhysicalDefence,
                        PhysicalEvade = unit.PhysicalEvade,
                        MagicDefence = unit.MagicDefence,
                        MagicEvade = unit.MagicEvade,
                        WasOldStatus = unit.IsUnderAnyStatus(TranceSeekStatus.Old) // Trigger to much "popup" text.
                    };
                    continue;
                }

                if (invincibleTargetId != 0 && (invincibleTargetId & unit.Id) != 0)
                    unit.State().Invincible = true;
                else
                    unit.State().Invincible = false;

#if DEV_TS
                if (unit.IsPlayer)
                    unit.State().Invincible = TranceSeekDebug.TranceSeekDebugMenu.MegaCheat > 0;
#endif

                if (immunestealTargetId != 0 && (immunestealTargetId & unit.Id) != 0)
                    unit.State().ImmuneSteal = true;
                else
                    unit.State().ImmuneSteal = false;

                if (dodgeALLTargetId != 0 && (dodgeALLTargetId & unit.Id) != 0)
                    unit.State().DodgeALL = true;
                else
                    unit.State().DodgeALL = false;

                bool hidePopups = hidestatmodifTargetId != 0 && (hidestatmodifTargetId & unit.Id) != 0;
                int displayOffset = 0;

                bool isOld = unit.IsUnderAnyStatus(TranceSeekStatus.Old);
                if (isOld != cached.WasOldStatus)
                {
                    hidePopups = true;
                    cached.WasOldStatus = isOld;
                }

                if (unit.Level != cached.Level)
                {
                    RequestPopup(unit, unit.Level > cached.Level ? "Lvl_Up" : "Lvl_Down", unit.Level > cached.Level, Math.Abs(unit.Level - cached.Level), ref displayOffset, hidePopups);
                    cached.Level = unit.Level;
                }
                if (unit.Strength != cached.Strength)
                {
                    RequestPopup(unit, unit.Strength > cached.Strength ? "Str_Up" : "Str_Down", unit.Strength > cached.Strength, Math.Abs(unit.Strength - cached.Strength), ref displayOffset, hidePopups);
                    cached.Strength = unit.Strength;
                }
                if (unit.Magic != cached.Magic)
                {
                    RequestPopup(unit, unit.Magic > cached.Magic ? "Mag_Up" : "Mag_Down", unit.Magic > cached.Magic, Math.Abs(unit.Magic - cached.Magic), ref displayOffset, hidePopups);
                    cached.Magic = unit.Magic;
                }
                if (unit.Dexterity != cached.Dexterity)
                {
                    RequestPopup(unit, unit.Dexterity > cached.Dexterity ? "Dex_Up" : "Dex_Down", unit.Dexterity > cached.Dexterity, Math.Abs(unit.Dexterity - cached.Dexterity), ref displayOffset, hidePopups);
                    cached.Dexterity = unit.Dexterity;
                }
                if (unit.Will != cached.Will)
                {
                    RequestPopup(unit, unit.Will > cached.Will ? "Wil_Up" : "Wil_Down", unit.Will > cached.Will, Math.Abs(unit.Will - cached.Will), ref displayOffset, hidePopups);
                    cached.Will = unit.Will;
                }
                if (unit.CriticalRateBonus != cached.CriticalRateBonus)
                {
                    RequestPopup(unit, unit.CriticalRateBonus > cached.CriticalRateBonus ? "CritB_Up" : "CritB_Down", unit.CriticalRateBonus > cached.CriticalRateBonus, Math.Abs(unit.CriticalRateBonus - cached.CriticalRateBonus), ref displayOffset, hidePopups);
                    cached.CriticalRateBonus = unit.CriticalRateBonus;
                }
                if (unit.CriticalRateResistance != cached.CriticalRateResistance)
                {
                    RequestPopup(unit, unit.CriticalRateResistance > cached.CriticalRateResistance ? "CritR_Up" : "CritR_Down", unit.CriticalRateResistance > cached.CriticalRateResistance, Math.Abs(unit.CriticalRateResistance - cached.CriticalRateResistance), ref displayOffset, hidePopups);
                    cached.CriticalRateResistance = unit.CriticalRateResistance;
                }
                if (unit.PhysicalDefence != cached.PhysicalDefence)
                {
                    RequestPopup(unit, unit.PhysicalDefence > cached.PhysicalDefence ? "Def_Up" : "Def_Down", unit.PhysicalDefence > cached.PhysicalDefence, Math.Abs(unit.PhysicalDefence - cached.PhysicalDefence), ref displayOffset, hidePopups);
                    cached.PhysicalDefence = unit.PhysicalDefence;
                }
                if (unit.PhysicalEvade != cached.PhysicalEvade)
                {
                    RequestPopup(unit, unit.PhysicalEvade > cached.PhysicalEvade ? "PDev_Up" : "PDev_Down", unit.PhysicalEvade > cached.PhysicalEvade, Math.Abs(unit.PhysicalEvade - cached.PhysicalEvade), ref displayOffset, hidePopups);
                    cached.PhysicalEvade = unit.PhysicalEvade;
                }
                if (unit.MagicDefence != cached.MagicDefence)
                {
                    RequestPopup(unit, unit.MagicDefence > cached.MagicDefence ? "MDef_Up" : "MDef_Down", unit.MagicDefence > cached.MagicDefence, Math.Abs(unit.MagicDefence - cached.MagicDefence), ref displayOffset, hidePopups);
                    cached.MagicDefence = unit.MagicDefence;
                }
                if (unit.MagicEvade != cached.MagicEvade)
                {
                    RequestPopup(unit, unit.MagicEvade > cached.MagicEvade ? "MDev_Up" : "MDev_Down", unit.MagicEvade > cached.MagicEvade, Math.Abs(unit.MagicEvade - cached.MagicEvade), ref displayOffset, hidePopups);
                    cached.MagicEvade = unit.MagicEvade;
                }
            }
        }


        private void RequestPopup(BattleUnit unit, string dbKey, bool isUp, int delta, ref int offset, bool hidePopups)
        {
            if (hidePopups) return;

            if (LocDB.TryGetValue(dbKey, out Dictionary<string, string> baseMsg))
            {
                // ↑ or ↓ between 1 and 9
                // ↑↑ or ↓↓ between 10 and 19
                // ↑↑↑ or ↓↓↓ for >= 20
                if (!baseMsg.TryGetValue(Localization.CurrentDisplaySymbol, out string msg))
                    if (!baseMsg.TryGetValue(Localization.GetFallbackSymbol(), out msg))
                            return;

                int arrowCount = 1;
                if (delta >= 20) arrowCount = 3;
                else if (delta >= 10) arrowCount = 2;

                string arrowStr = new string(isUp ? '↑' : '↓', arrowCount);
                msg = msg.Replace(isUp ? "↑" : "↓", arrowStr);

                string themeKey = GreenRedColor_SubModCheck ? (isUp ? "Green" : "Red") : (isUp ? "Yellow" : "Purple");
                string hexColor = ColorThemes[themeKey][arrowCount - 1];

                Btl2dReqHeadSymbolMessage(unit.Data, hexColor, msg, HUDMessage.MessageStyle.DAMAGE, (byte)(offset * 5), 100);
                offset++;
            }
        }

        public static BTL2D_ENT Btl2dReqHeadSymbolMessage(BTL_DATA pBtl, String messageColor, Dictionary<String, String> multiLangMessage, HUDMessage.MessageStyle style, Byte pDelay, SByte customYofs = -16)
        {
            if (!multiLangMessage.TryGetValue(Localization.CurrentDisplaySymbol, out String msg))
                multiLangMessage.TryGetValue(Localization.GetFallbackSymbol(), out msg);
            return Btl2dReqHeadSymbolMessage(pBtl, messageColor, msg, style, pDelay, customYofs);
        }

        public static BTL2D_ENT Btl2dReqHeadSymbolMessage(BTL_DATA pBtl, String messageColor, String message, HUDMessage.MessageStyle style, Byte pDelay, SByte customYofs = -16)
        {
            BTL2D_ENT freeEntry = btl2d.GetFreeEntry(pBtl);

            freeEntry.Type = 3;
            freeEntry.Delay = pDelay;
            freeEntry.CustomColor = messageColor;
            freeEntry.CustomMessage = message;
            freeEntry.CustomStyle = style;

            btl2d.GetIconPosition(pBtl, btl2d.ICON_POS_DEFAULT, out Transform headTransform, out Vector3 _);
            if (headTransform != null)
                freeEntry.trans = headTransform;

            freeEntry.Yofs = customYofs;
            return freeEntry;
        }

        private void OnDestroy()
        {
            _unitCache.Clear();
        }

        private static readonly Dictionary<string, Dictionary<string, string>> LocDB = new Dictionary<string, Dictionary<string, string>>
        {
            { "Str_Up", new Dictionary<string, string> { { "US", "Strength ↑" }, { "UK", "Strength ↑" }, { "JP", "攻撃力 ↑" }, { "ES", "Ataque ↑" }, { "FR", "Force ↑" }, { "GR", "Angriff ↑" }, { "IT", "Forza ↑" } } },
            { "Str_Down", new Dictionary<string, string> { { "US", "Strength ↓" }, { "UK", "Strength ↓" }, { "JP", "攻撃力 ↓" }, { "ES", "Rompebrazo ↓" }, { "FR", "Force ↓" }, { "GR", "Kraft ↓" }, { "IT", "Forza ↓" } } },

            { "Mag_Up", new Dictionary<string, string> { { "US", "Magic ↑" }, { "UK", "Magic ↑" }, { "JP", "魔力 ↑" }, { "ES", "Magia ↑" }, { "FR", "Magie ↑" }, { "GR", "Magie ↑" }, { "IT", "Magia ↑" } } },
            { "Mag_Down", new Dictionary<string, string> { { "US", "Magic ↓" }, { "UK", "Magic ↓" }, { "JP", "魔力 ↓" }, { "ES", "Rompemagia ↓" }, { "FR", "Magie ↓" }, { "GR", "Magie ↓" }, { "IT", "Magia ↓" } } },

            { "Def_Up", new Dictionary<string, string> { { "US", "Defense ↑" }, { "UK", "Defense ↑" }, { "JP", "防御力 ↑" }, { "ES", "Defensa ↑" }, { "FR", "Défense ↑" }, { "GR", "Abwehr ↑" }, { "IT", "Difesa ↑" } } },
            { "Def_Down", new Dictionary<string, string> { { "US", "P.Defense ↓" }, { "UK", "P.Defense ↓" }, { "JP", "防御力 ↓" }, { "ES", "Rompecoraza ↓" }, { "FR", "Défense P. ↓" }, { "GR", "Abwehr ↓" }, { "IT", "Difesa P. ↓" } } },

            { "MDef_Up", new Dictionary<string, string> { { "US", "Magic Def ↑" }, { "UK", "Magic Def ↑" }, { "JP", "魔法防御 ↑" }, { "ES", "Def. M. ↑" }, { "FR", "Défense M. ↑" }, { "GR", "Z-Abwehr ↑" }, { "IT", "Dif. Mag. ↑" } } },
            { "MDef_Down", new Dictionary<string, string> { { "US", "M.Defense ↓" }, { "UK", "M.Defense ↓" }, { "JP", "魔法防御 ↓" }, { "ES", "Rompeespíritu ↓" }, { "FR", "Défense M. ↓" }, { "GR", "Zauber-Abwehr ↓" }, { "IT", "Dif. Mag. ↓" } } },

            { "Lvl_Up", new Dictionary<string, string> { { "US", "Level ↑" }, { "UK", "Level ↑" }, { "JP", "レベル ↑" }, { "ES", "Nivel ↑" }, { "FR", "Niveau ↑" }, { "GR", "Stufe ↑" }, { "IT", "Livello ↑" } } },
            { "Lvl_Down", new Dictionary<string, string> { { "US", "Level ↓" }, { "UK", "Level ↓" }, { "JP", "レベル ↓" }, { "ES", "Nivel ↓" }, { "FR", "Niveau ↓" }, { "GR", "Stufe ↓" }, { "IT", "Livello ↓" } } },

            { "Dex_Up", new Dictionary<string, string> { { "US", "Speed ↑" }, { "UK", "Speed ↑" }, { "JP", "素早さ ↑" }, { "ES", "Velocidad ↑" }, { "FR", "Vitesse ↑" }, { "GR", "Geschw. ↑" }, { "IT", "Velocità ↑" } } },
            { "Dex_Down", new Dictionary<string, string> { { "US", "Speed ↓" }, { "UK", "Speed ↓" }, { "JP", "素早さ ↓" }, { "ES", "Velocidad ↓" }, { "FR", "Vitesse ↓" }, { "GR", "Geschw. ↓" }, { "IT", "Velocità ↓" } } },

            { "Wil_Up", new Dictionary<string, string> { { "US", "Spirit ↑" }, { "UK", "Spirit ↑" }, { "JP", "気力 ↑" }, { "ES", "Espíritu ↑" }, { "FR", "Esprit ↑" }, { "GR", "Wille ↑" }, { "IT", "Zauber ↑" } } },
            { "Wil_Down", new Dictionary<string, string> { { "US", "Spirit ↓" }, { "UK", "Spirit ↓" }, { "JP", "気力 ↓" }, { "ES", "Espíritu ↓" }, { "FR", "Esprit ↓" }, { "GR", "Wille ↓" }, { "IT", "Zauber ↓" } } },

            { "CritB_Up", new Dictionary<string, string> { { "US", "Crit Rate ↑" }, { "UK", "Crit Rate ↑" }, { "JP", "会心率 ↑" }, { "ES", "Crítico ↑" }, { "FR", "Taux Crit. ↑" }, { "GR", "Krit ↑" }, { "IT", "Critico ↑" } } },
            { "CritB_Down", new Dictionary<string, string> { { "US", "Crit Rate ↓" }, { "UK", "Crit Rate ↓" }, { "JP", "会心率 ↓" }, { "ES", "Crítico ↓" }, { "FR", "Taux Crit. ↓" }, { "GR", "Krit ↓" }, { "IT", "Critico ↓" } } },

            { "CritR_Up", new Dictionary<string, string> { { "US", "Crit Res ↑" }, { "UK", "Crit Res ↑" }, { "JP", "会心耐性 ↑" }, { "ES", "Res. Crit. ↑" }, { "FR", "Rés. Crit. ↑" }, { "GR", "Krit-Res ↑" }, { "IT", "Res. Crit. ↑" } } },
            { "CritR_Down", new Dictionary<string, string> { { "US", "Crit Res ↓" }, { "UK", "Crit Res ↓" }, { "JP", "会心耐性 ↓" }, { "ES", "Res. Crit. ↓" }, { "FR", "Rés. Crit. ↓" }, { "GR", "Krit-Res ↓" }, { "IT", "Res. Crit. ↓" } } },

            { "PDev_Up", new Dictionary<string, string> { { "US", "Evade ↑" }, { "UK", "Evade ↑" }, { "JP", "回避率 ↑" }, { "ES", "Evasión ↑" }, { "FR", "Esquive ↑" }, { "GR", "Ausweichen ↑" }, { "IT", "Evasione ↑" } } },
            { "PDev_Down", new Dictionary<string, string> { { "US", "Evade ↓" }, { "UK", "Evade ↓" }, { "JP", "回避率 ↓" }, { "ES", "Evasión ↓" }, { "FR", "Esquive ↓" }, { "GR", "Ausweichen ↓" }, { "IT", "Evasione ↓" } } },

            { "MDev_Up", new Dictionary<string, string> { { "US", "M.Evade ↑" }, { "UK", "M.Evade ↑" }, { "JP", "魔法回避 ↑" }, { "ES", "Evasión M. ↑" }, { "FR", "Esquive M. ↑" }, { "GR", "M-Ausweichen ↑" }, { "IT", "Evasione M. ↑" } } },
            { "MDev_Down", new Dictionary<string, string> { { "US", "M.Evade ↓" }, { "UK", "M.Evade ↓" }, { "JP", "魔法回避 ↓" }, { "ES", "Evasión M. ↓" }, { "FR", "Esquive M. ↓" }, { "GR", "M-Ausweichen ↓" }, { "IT", "Evasione M. ↓" } } }
        };

        private static readonly Dictionary<string, string[]> ColorThemes = new Dictionary<string, string[]>
        {
            { "Green",  new[] { "[A6FF62]", "[78C840]", "[5CB81B]" } },
            { "Red",    new[] { "[FF8B88]", "[FF524F]", "[FF1E0E]" } },
            { "Yellow", new[] { "[FAFFAF]", "[F5FF54]", "[E1CC00]" } },
            { "Purple", new[] { "[F8B6F1]", "[F17CE5]", "[ED42DC]" } }
        };
    }
}
