using System;
using UnityEngine;
using Memoria.Data;
using Object = System.Object;
using System.Collections.Generic;
using Unity.IO.Compression;
using Memoria.Scripts.Battle;

namespace Memoria.DefaultScripts
{
    public static class OverlapSHP
    {
        // Here, write the list of statuses with SHP that should overlap, ie. only one SHP will be displayed at a time
        private static List<BattleStatusId> OverlappingSHP = [BattleStatusId.CustomStatus1, BattleStatusId.CustomStatus2, BattleStatusId.CustomStatus3, BattleStatusId.CustomStatus4,
            BattleStatusId.CustomStatus5, BattleStatusId.CustomStatus6, BattleStatusId.CustomStatus7, BattleStatusId.CustomStatus8];
        private static Dictionary<BTL_DATA, Int32> OverlapIndex = new Dictionary<BTL_DATA, Int32>();

        public static void SetupOverlappingSHP(BattleUnit unit)
        {
            if (OverlapIndex.ContainsKey(unit.Data))
                return;
            OverlapIndex[unit.Data] = 0;
            unit.AddDelayedModifier(UpdateOverlappingSHP, null);
        }

        // That method should better be called on battle initialisation (IOverloadOnBattleInitScript)
        public static void ClearInBattleInit()
        {
            OverlapIndex.Clear();
        }

        private static Boolean UpdateOverlappingSHP(BattleUnit unit)
        {
            if (!OverlapIndex.TryGetValue(unit.Data, out Int32 currentIndex))
                return false;
            Boolean switchSHP = false;
            for (Int32 i = 0; i < OverlappingSHP.Count; i++)
            {
                BattleStatusId statusId = OverlappingSHP[currentIndex];
                if (unit.IsUnderAnyStatus(statusId))
                {
                    if (i != 0)
                        break;
                    SHPEffect shp = HonoluluBattleMain.battleSPS.GetBtlSHPObj(unit, statusId);
                    if (shp == null || !shp.IsCyclingFrame)
                        break;
                }
                currentIndex++;
                currentIndex %= OverlappingSHP.Count;
                switchSHP = i < OverlappingSHP.Count;
            }
            if (!unit.IsUnderAnyStatus(OverlappingSHP[currentIndex]))
            {
                OverlapIndex.Remove(unit.Data);
                return false;
            }
            OverlapIndex[unit.Data] = currentIndex;
            for (Int32 i = 0; i < OverlappingSHP.Count; i++)
            {
                BattleStatusId statusId = OverlappingSHP[i];
                SHPEffect shp = HonoluluBattleMain.battleSPS.GetBtlSHPObj(unit, statusId);
                if (shp == null)
                    continue;
                if (i == currentIndex)
                {
                    shp.attr &= unchecked((Byte)~SPSConst.ATTR_HIDDEN);
                    if (switchSHP)
                    {
                        shp.frame = 0;
                        StatusScriptBase effectScript = unit.Data.stat.effects[statusId];
                        // There, optionally run a code when the SHP is displayed
                        if (effectScript is PowerBreakStatusScript)
                        {
                            PowerBreakStatusScript PowerBreakScript = effectScript as PowerBreakStatusScript;
                            PowerBreakScript.OnSHPShow(true);
                        }
                        if (effectScript is MagicBreakStatusScript)
                        {
                            MagicBreakStatusScript MagicBreakScript = effectScript as MagicBreakStatusScript;
                            MagicBreakScript.OnSHPShow(true);
                        }
                        if (effectScript is ArmorBreakStatusScript)
                        {
                            ArmorBreakStatusScript ArmorBreakScript = effectScript as ArmorBreakStatusScript;
                            ArmorBreakScript.OnSHPShow(true);
                        }
                        if (effectScript is MentalBreakStatusScript)
                        {
                            MentalBreakStatusScript MentalBreakScript = effectScript as MentalBreakStatusScript;
                            MentalBreakScript.OnSHPShow(true);
                        }
                        if (effectScript is PowerUpStatusScript)
                        {
                            PowerUpStatusScript PowerUpScript = effectScript as PowerUpStatusScript;
                            PowerUpScript.OnSHPShow(true);
                        }
                        if (effectScript is MagicUpStatusScript)
                        {
                            MagicUpStatusScript MagicUpScript = effectScript as MagicUpStatusScript;
                            MagicUpScript.OnSHPShow(true);
                        }
                        if (effectScript is ArmorUpStatusScript)
                        {
                            ArmorUpStatusScript ArmorUpScript = effectScript as ArmorUpStatusScript;
                            ArmorUpScript.OnSHPShow(true);
                        }
                        if (effectScript is MentalUpStatusScript)
                        {
                            MentalUpStatusScript MentalUpScript = effectScript as MentalUpStatusScript;
                            MentalUpScript.OnSHPShow(true);
                        }
                    }
                }
                else if ((shp.attr & SPSConst.ATTR_HIDDEN) == 0)
                {
                    shp.attr |= SPSConst.ATTR_HIDDEN;
                    StatusScriptBase effectScript = unit.Data.stat.effects[statusId];
                    // There, optionally run a code when the SHP is hidden because of overlapping
                    if (effectScript is PowerBreakStatusScript)
                    {
                        PowerBreakStatusScript PowerBreakScript = effectScript as PowerBreakStatusScript;
                        PowerBreakScript.OnSHPShow(false);
                    }
                    if (effectScript is MagicBreakStatusScript)
                    {
                        MagicBreakStatusScript MagicBreakScript = effectScript as MagicBreakStatusScript;
                        MagicBreakScript.OnSHPShow(false);
                    }
                    if (effectScript is ArmorBreakStatusScript)
                    {
                        ArmorBreakStatusScript ArmorBreakScript = effectScript as ArmorBreakStatusScript;
                        ArmorBreakScript.OnSHPShow(false);
                    }
                    if (effectScript is MentalBreakStatusScript)
                    {
                        MentalBreakStatusScript MentalBreakScript = effectScript as MentalBreakStatusScript;
                        MentalBreakScript.OnSHPShow(false);
                    }
                    if (effectScript is PowerUpStatusScript)
                    {
                        PowerUpStatusScript PowerUpScript = effectScript as PowerUpStatusScript;
                        PowerUpScript.OnSHPShow(false);
                    }
                    if (effectScript is MagicUpStatusScript)
                    {
                        MagicUpStatusScript MagicUpScript = effectScript as MagicUpStatusScript;
                        MagicUpScript.OnSHPShow(false);
                    }
                    if (effectScript is ArmorUpStatusScript)
                    {
                        ArmorUpStatusScript ArmorUpScript = effectScript as ArmorUpStatusScript;
                        ArmorUpScript.OnSHPShow(false);
                    }
                    if (effectScript is MentalUpStatusScript)
                    {
                        MentalUpStatusScript MentalUpScript = effectScript as MentalUpStatusScript;
                        MentalUpScript.OnSHPShow(false);
                    }
                }
            }
            return true;
        }
    }
}
