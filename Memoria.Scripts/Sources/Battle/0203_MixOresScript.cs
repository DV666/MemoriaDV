using System;
using System.Collections.Generic;
using FF9;
using Memoria.Data;
using Memoria.Prime;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Special
    /// </summary>
    [BattleScript(Id)]
    public sealed class MixOresScript : IBattleScript
    {
        public const Int32 Id = 0203;

        private readonly BattleCalculator _v;

        public MixOresScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Command.ItemId == RegularItem.NoItem)
            {
                _v.Context.Flags = BattleCalcFlags.Miss;
                return;
            }

            int HPHeal = 0;
            int MPHeal = 0;
            int PowerGemVanilla = _v.Command.Item.Power * (ff9item.FF9Item_GetCount(_v.Command.ItemId) + 1);
            _v.Command.Power = _v.Command.Item.Power;
            switch (_v.Command.ItemId)
            {
                case (RegularItem)2349: // Mother G
                case (RegularItem)2360: // Super Mother G
                {
                    _v.Command.AbilityStatus = _v.Command.ItemStatus;
                    TranceSeekCustomAPI.TryAlterCommandStatuses(_v);
                    break;
                }
                case (RegularItem)2364: // Super Péridot
                case (RegularItem)2365: // Super Topaze
                case (RegularItem)2366: // Super Opale
                case (RegularItem)2367: // Super Améthyst
                case (RegularItem)2368: // Super Beryl
                case (RegularItem)2369: // Super Grenat
                case (RegularItem)2370: // Super Saphir
                case (RegularItem)2371: // Super Rubis
                case (RegularItem)2372: // Super Emeraude
                case (RegularItem)2373: // Super Lapis Lazuli
                case (RegularItem)2374: // Super Lunalithe
                {
                    HPHeal = PowerGemVanilla * 2;
                    break;
                }
                case (RegularItem)2375: // Péridotopaze
                case (RegularItem)2376: // Péridotopale
                case (RegularItem)2377: // Péridothyste
                case (RegularItem)2378: // Péridoryl
                case (RegularItem)2379: // Péridenat
                case (RegularItem)2380: // Péridaphir
                case (RegularItem)2381: // Périderaude
                case (RegularItem)2382: // Péridazuli
                case (RegularItem)2383: // Péridalithe
                case (RegularItem)2384: // Topale
                case (RegularItem)2385: // Topaméthyste
                case (RegularItem)2386: // Toperyl
                case (RegularItem)2387: // Topenat
                case (RegularItem)2388: // Topaphir
                case (RegularItem)2389: // Toperaude
                case (RegularItem)2390: // Topazuli
                case (RegularItem)2391: // Topalithe
                case (RegularItem)2392: // Opalèthyste
                case (RegularItem)2393: // Operyl
                case (RegularItem)2394: // Openat
                case (RegularItem)2395: // Opaphir
                case (RegularItem)2396: // Operaude
                case (RegularItem)2397: // Opazuli
                case (RegularItem)2398: // Opalithe
                case (RegularItem)2399: // Berhyste
                case (RegularItem)2400: // Grenéthyst
                case (RegularItem)2401: // Améphir
                case (RegularItem)2402: // Améraude
                case (RegularItem)2403: // Améthazuli
                case (RegularItem)2404: // Lunathyst
                case (RegularItem)2405: // Greryl
                case (RegularItem)2406: // Beryphir
                case (RegularItem)2407: // Berylaude
                case (RegularItem)2408: // Berylazuli
                case (RegularItem)2409: // Berylithe
                case (RegularItem)2410: // Grenaphir
                case (RegularItem)2411: // Greraude
                case (RegularItem)2412: // Grenazuli
                case (RegularItem)2413: // Grenalithe
                case (RegularItem)2414: // Saphemaude
                case (RegularItem)2415: // Lapis Laphir
                case (RegularItem)2416: // Lunaphir
                case (RegularItem)2417: // Lazeraude
                case (RegularItem)2418: // Luneraude
                case (RegularItem)2419: // Lapis Lunazuli
                {
                    RegularItem[] ingredients = new RegularItem[2];
                    EffectElement element1 = EffectElement.None;
                    EffectElement element2 = EffectElement.None;
                    foreach (MixItems mixCandidate in ff9mixitem.MixItemsData.Values)
                    {
                        if (mixCandidate.Id < 0)
                            continue;
                        if (mixCandidate.Result == _v.Command.ItemId)
                        {
                            for (int i = 0; i < mixCandidate.Ingredients.Count; i++)
                            {
                                ingredients[i] = mixCandidate.Ingredients[i];
                            }
                        }                     
                    }
                    HPHeal = PowerGemVanilla;
                    if (ElementItem.ContainsKey(ingredients[0]))
                        element1 = ElementItem[ingredients[0]];
                    if (ElementItem.ContainsKey(ingredients[1]))
                        element2 = ElementItem[ingredients[1]];

                    if (ingredients[0] == RegularItem.Garnet || ingredients[1] == RegularItem.Garnet)
                    {
                        if ((element1 > 0 && _v.Target.AbsorbElement == element1) || (element2 > 0 && _v.Target.AbsorbElement == element2))
                            HPHeal *= 4;
                    }
                    else
                    {
                        if ((_v.Target.AbsorbElement & element1) != 0)
                            HPHeal *= 2;
                        if ((_v.Target.AbsorbElement & element2) != 0)
                            HPHeal *= 2;
                    }
                    break;
                }
                case (RegularItem)2420: // Pluie de Gemme
                case (RegularItem)2421: // Pluie de Péridot
                case (RegularItem)2422: // Pluie de Topaze
                case (RegularItem)2423: // Pluie d'Opale
                case (RegularItem)2424: // Pluie d'Améthyste
                case (RegularItem)2425: // Pluie de Beryl
                case (RegularItem)2426: // Pluie de Grenat
                case (RegularItem)2427: // Pluie de Saphir
                case (RegularItem)2428: // Pluie de Rubis
                case (RegularItem)2429: // Pluie d'Emeraude
                case (RegularItem)2430: // Pluie de Lapis Lazuli
                case (RegularItem)2431: // Pluie de Lunalithe
                {
                    HPHeal = PowerGemVanilla;
                    break;
                }
                case (RegularItem)2446: // Globe brilliant de foudre
                case (RegularItem)2447: // Globe brilliant de feu
                case (RegularItem)2448: // Globe brilliant de glace
                case (RegularItem)2449: // Globe brilliant de gravité
                case (RegularItem)2450: // Globe brilliant d'eau
                case (RegularItem)2451: // Globe brilliant flambloyant
                case (RegularItem)2452: // Globe brilliant de terre
                case (RegularItem)2453: // Globe brilliant de vent
                case (RegularItem)2454: // Globe brilliant de ténèbres
                case (RegularItem)2455: // Globe brilliant de lumière
                {
                    _v.Command.Power = _v.Command.Item.Power;
                    _v.NormalMagicParams();
                    if (ElementItem.ContainsKey(_v.Command.ItemId))
                        _v.Command.Element = ElementItem[_v.Command.ItemId];

                    if (_v.Target.IsUnderAnyStatus(BattleStatus.Reflect))
                        _v.Command.Power *= 2;

                    TranceSeekCustomAPI.CasterPenaltyMini(_v);
                    TranceSeekCustomAPI.EnemyTranceBonusAttack(_v);
                    TranceSeekCustomAPI.PenaltyShellAttack(_v);
                    TranceSeekCustomAPI.PenaltyCommandDividedAttack(_v);
                    TranceSeekCustomAPI.BonusElement(_v);
                    if (TranceSeekCustomAPI.CanAttackMagic(_v))
                    {
                        _v.CalcHpDamage();
                        TranceSeekCustomAPI.RaiseTrouble(_v);
                    }
                    return;
                }
            }

            if (HPHeal > 0)
                _v.Target.Flags |= CalcFlag.HpDamageOrHeal;

            if (MPHeal > 0)
                _v.Target.Flags |= CalcFlag.MpDamageOrHeal;

            if ((_v.Target.Flags & CalcFlag.HpAlteration) != 0)
                _v.Target.HpDamage = HPHeal;

            if ((_v.Target.Flags & CalcFlag.MpAlteration) != 0)
                _v.Target.MpDamage = MPHeal;

            foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(_v.Caster))
                saFeature.TriggerOnAbility(_v, "CalcDamage", false);
            foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(_v.Target))
                saFeature.TriggerOnAbility(_v, "CalcDamage", true);
        }

        public static Dictionary<RegularItem, EffectElement> ElementItem = new Dictionary<RegularItem, EffectElement>
        {
            { RegularItem.Peridot, EffectElement.Thunder },
            { RegularItem.Topaz, EffectElement.Fire },
            { RegularItem.Opal, EffectElement.Cold },
            { RegularItem.Aquamarine, EffectElement.Aqua },
            { RegularItem.Sapphire, EffectElement.Earth },
            { RegularItem.Garnet, EffectElement.None },
            { RegularItem.Emerald, EffectElement.Wind },
            { RegularItem.LapisLazuli, EffectElement.Darkness },
            { (RegularItem)2446, EffectElement.Thunder },
            { (RegularItem)2447, EffectElement.Fire },
            { (RegularItem)2448, EffectElement.Cold },
            { (RegularItem)2450, EffectElement.Aqua },
            { (RegularItem)2452, EffectElement.Earth },
            { (RegularItem)2453, EffectElement.Wind },
            { (RegularItem)2454, EffectElement.Darkness },
            { (RegularItem)2455, EffectElement.Holy }
        };

        private Boolean HitRateForZombie()
        {
            if (_v.Target.IsZombie)
            {
                TranceSeekCustomAPI.MagicAccuracy(_v);
                return true;
            }
            return false;
        }
    }
}
