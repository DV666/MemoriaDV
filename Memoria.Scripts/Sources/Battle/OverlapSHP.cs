using System;
using Memoria.Data;
using System.Collections.Generic;
using Memoria.Scripts.TranceSeek;

namespace Memoria.DefaultScripts
{
    /// <summary>
    /// Overlap System for "identical" SHP : idea by DV and code made by Tirlititi (thanks !)
    /// </summary>
    public static class OverlapSHP
    {
        // Here, write the list of statuses with SHP that should overlap, ie. only one SHP will be displayed at a time
        private static List<BattleStatusId> OverlappingSHP1 = [TranceSeekStatusId.PowerBreak, TranceSeekStatusId.MagicBreak, TranceSeekStatusId.ArmorBreak, TranceSeekStatusId.MentalBreak,
            TranceSeekStatusId.PowerUp, TranceSeekStatusId.MagicUp, TranceSeekStatusId.ArmorUp, TranceSeekStatusId.MentalUp];
        private static List<BattleStatusId> OverlappingSHP2 = [TranceSeekStatusId.Redemption, TranceSeekStatusId.MechanicalArmor, TranceSeekStatusId.Bulwark,
            TranceSeekStatusId.Rage, TranceSeekStatusId.PerfectCrit, TranceSeekStatusId.PerfectDodge];
        private static Dictionary<BTL_DATA, Int32> OverlapIndex1 = new Dictionary<BTL_DATA, Int32>();
        private static Dictionary<BTL_DATA, Int32> OverlapIndex2 = new Dictionary<BTL_DATA, Int32>();

        public static void SetupOverlappingSHP1(BattleUnit unit)
        {
            if (OverlapIndex1.ContainsKey(unit.Data))
                return;
            OverlapIndex1[unit.Data] = 0;
            unit.AddDelayedModifier(UpdateOverlappingSHP1, null);
        }

        public static void SetupOverlappingSHP2(BattleUnit unit)
        {
            if (OverlapIndex2.ContainsKey(unit.Data))
                return;
            OverlapIndex2[unit.Data] = 0;
            unit.AddDelayedModifier(UpdateOverlappingSHP2, null);
        }

        // That method should better be called on battle initialisation (IOverloadOnBattleInitScript)
        public static void ClearInBattleInit()
        {
            OverlapIndex1.Clear();
            OverlapIndex2.Clear();
        }

        private static Boolean UpdateOverlappingSHP1(BattleUnit unit)
        {
            if (!OverlapIndex1.TryGetValue(unit.Data, out Int32 currentIndex))
                return false;
            Boolean switchSHP = false;
            for (Int32 i = 0; i < OverlappingSHP1.Count; i++)
            {
                BattleStatusId statusId = OverlappingSHP1[currentIndex];
                if (unit.IsUnderAnyStatus(statusId))
                {
                    if (i != 0)
                        break;
                    SHPEffect shp = HonoluluBattleMain.battleSPS.GetBtlSHPObj(unit, statusId);
                    if (shp == null || !shp.IsCyclingFrame)
                        break;
                }
                currentIndex++;
                currentIndex %= OverlappingSHP1.Count;
                switchSHP = i < OverlappingSHP1.Count;
            }
            if (!unit.IsUnderAnyStatus(OverlappingSHP1[currentIndex]))
            {
                OverlapIndex1.Remove(unit.Data);
                return false;
            }
            OverlapIndex1[unit.Data] = currentIndex;
            for (Int32 i = 0; i < OverlappingSHP1.Count; i++)
            {
                BattleStatusId statusId = OverlappingSHP1[i];
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

        private static Boolean UpdateOverlappingSHP2(BattleUnit unit)
        {
            if (!OverlapIndex2.TryGetValue(unit.Data, out Int32 currentIndex))
                return false;
            Boolean switchSHP = false;
            for (Int32 i = 0; i < OverlappingSHP2.Count; i++)
            {
                BattleStatusId statusId = OverlappingSHP2[currentIndex];
                if (unit.IsUnderAnyStatus(statusId))
                {
                    if (i != 0)
                        break;
                    SHPEffect shp = HonoluluBattleMain.battleSPS.GetBtlSHPObj(unit, statusId);
                    if (shp == null || !shp.IsCyclingFrame)
                        break;
                }
                currentIndex++;
                currentIndex %= OverlappingSHP2.Count;
                switchSHP = i < OverlappingSHP2.Count;
            }
            if (!unit.IsUnderAnyStatus(OverlappingSHP2[currentIndex]))
            {
                OverlapIndex2.Remove(unit.Data);
                return false;
            }
            OverlapIndex2[unit.Data] = currentIndex;
            for (Int32 i = 0; i < OverlappingSHP2.Count; i++)
            {
                BattleStatusId statusId = OverlappingSHP2[i];
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
                        if (effectScript is RedemptionStatusScript)
                        {
                            RedemptionStatusScript RedemptionScript = effectScript as RedemptionStatusScript;
                            RedemptionScript.OnSHPShow(true);
                        }
                        if (effectScript is MechanicalArmorStatusScript)
                        {
                            MechanicalArmorStatusScript MechanicalArmorScript = effectScript as MechanicalArmorStatusScript;
                            MechanicalArmorScript.OnSHPShow(true);
                        }
                        if (effectScript is BulwarkStatusScript)
                        {
                            BulwarkStatusScript BulwarkScript = effectScript as BulwarkStatusScript;
                            BulwarkScript.OnSHPShow(true);
                        }
                        if (effectScript is RageStatusScript)
                        {
                            RageStatusScript RageScript = effectScript as RageStatusScript;
                            RageScript.OnSHPShow(true);
                        }
                        if (effectScript is PerfectCritStatusScript)
                        {
                            PerfectCritStatusScript PerfectCritScript = effectScript as PerfectCritStatusScript;
                            PerfectCritScript.OnSHPShow(true);
                        }
                        if (effectScript is PerfectDodgeStatusScript)
                        {
                            PerfectDodgeStatusScript PerfectCritScript = effectScript as PerfectDodgeStatusScript;
                            PerfectCritScript.OnSHPShow(true);
                        }
                    }
                }
                else if ((shp.attr & SPSConst.ATTR_HIDDEN) == 0)
                {
                    shp.attr |= SPSConst.ATTR_HIDDEN;
                    StatusScriptBase effectScript = unit.Data.stat.effects[statusId];
                    // There, optionally run a code when the SHP is hidden because of overlapping
                    if (effectScript is RedemptionStatusScript)
                    {
                        RedemptionStatusScript RedemptionScript = effectScript as RedemptionStatusScript;
                        RedemptionScript.OnSHPShow(false);
                    }
                    if (effectScript is MechanicalArmorStatusScript)
                    {
                        MechanicalArmorStatusScript MechanicalArmorScript = effectScript as MechanicalArmorStatusScript;
                        MechanicalArmorScript.OnSHPShow(false);
                    }
                    if (effectScript is BulwarkStatusScript)
                    {
                        BulwarkStatusScript BulwarkScript = effectScript as BulwarkStatusScript;
                        BulwarkScript.OnSHPShow(false);
                    }
                    if (effectScript is RageStatusScript)
                    {
                        RageStatusScript RageScript = effectScript as RageStatusScript;
                        RageScript.OnSHPShow(false);
                    }
                    if (effectScript is PerfectCritStatusScript)
                    {
                        PerfectCritStatusScript PerfectCritScript = effectScript as PerfectCritStatusScript;
                        PerfectCritScript.OnSHPShow(false);
                    }
                    if (effectScript is PerfectDodgeStatusScript)
                    {
                        PerfectDodgeStatusScript PerfectCritScript = effectScript as PerfectDodgeStatusScript;
                        PerfectCritScript.OnSHPShow(false);
                    }
                }
            }
            return true;
        }
    }
}
