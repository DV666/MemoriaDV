using System;
using System.Collections.Generic;
using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Data;
using static Memoria.Assets.DataResources;
using static SiliconStudio.Social.ResponseData;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Special
    /// </summary>
    [BattleScript(Id)]
    public sealed class MixRemedyScript : IBattleScript
    {
        public const Int32 Id = 0201;

        private readonly BattleCalculator _v;

        public MixRemedyScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Command.ItemId == RegularItem.NoItem || _v.Caster.HasSupportAbilityByIndex((SupportAbility)251)) // SA Artificer
            {
                _v.Context.Flags = BattleCalcFlags.Miss;
                return;
            }

            int HPHeal = 0;
            int MPHeal = 0;
            switch (_v.Command.ItemId)
            {
                case (RegularItem)2039: // Sérum
                case (RegularItem)2049: // Echo
                case (RegularItem)2058: // Collyre
                case (RegularItem)2067: // Doux
                case (RegularItem)2076: // Calmant
                case (RegularItem)2085: // Eau bénite
                case (RegularItem)2094: // Anticorps
                {
                    HPHeal = 200;
                    TranceSeekAPI.TryRemoveItemStatuses(_v);
                    break;
                }
                case (RegularItem)2040: // Maxi Sérum
                case (RegularItem)2050: // Maxi Echo
                case (RegularItem)2059: // Maxi Collyre
                case (RegularItem)2068: // Maxi Doux
                case (RegularItem)2077: // Maxi Calmant
                case (RegularItem)2086: // Maxi Eau bénite
                case (RegularItem)2095: // Maxi Anticorps
                {
                    HPHeal = 500;
                    TranceSeekAPI.TryRemoveItemStatuses(_v);
                    break;
                }
                case (RegularItem)2041: // Ultra Sérum
                case (RegularItem)2051: // Ultra Echo
                case (RegularItem)2060: // Ultra Collyre
                case (RegularItem)2069: // Ultra Doux
                case (RegularItem)2078: // Ultra Calmant
                case (RegularItem)2087: // Ultra Eau bénite
                case (RegularItem)2096: // Ultra Anticorps
                {
                    HPHeal = 1250;
                    TranceSeekAPI.TryRemoveItemStatuses(_v);
                    break;
                }
                case (RegularItem)2103: // Médicament
                case (RegularItem)2104: // Maxi Médicament
                case (RegularItem)2105: // Ultra Médicament
                case (RegularItem)2113: // Médicament +
                case (RegularItem)2114: // Maxi Médicament +
                case (RegularItem)2115: // Ultra Médicament +
                {
                    HPHeal = (int)(_v.Target.MaximumHp * _v.Command.Power) / 100;
                    TranceSeekAPI.TryRemoveItemStatuses(_v);
                    break;
                }
                case (RegularItem)2106: // Traitement
                case (RegularItem)2107: // Traitement +
                case (RegularItem)2116: // Traitement X
                {
                    MPHeal = (int)(_v.Target.MaximumMp * _v.Command.Power) / 100;
                    TranceSeekAPI.TryRemoveItemStatuses(_v);
                    break;
                }
                case (RegularItem)2042: // Sérum amélioré
                case (RegularItem)2052: // Echo amélioré
                case (RegularItem)2061: // Collyre amélioré
                case (RegularItem)2070: // Doux amélioré
                case (RegularItem)2079: // Calmant amélioré
                case (RegularItem)2088: // Eau bénite amélioré
                case (RegularItem)2097: // Anticorps amélioré
                {
                    TranceSeekAPI.TryRemoveItemStatuses(_v);
                    if (TranceSeekAPI.ProtectStatus.TryGetValue(_v.Target.Data, out Dictionary<BattleStatus, Int32> statusprotect))
                    {
                        foreach (BattleStatusId statusID in _v.Command.Item.Status.ToStatusList())
                        {
                            BattleStatus status = statusID.ToBattleStatus();
                            statusprotect.Add(status, 2);
                        }
                    }
                    break;
                }
                case (RegularItem)2043: // Puissant sérum
                case (RegularItem)2053: // Puissant echo
                case (RegularItem)2062: // Puissant Collyre
                case (RegularItem)2071: // Puissant Doux
                case (RegularItem)2080: // Puissant Calmant
                case (RegularItem)2089: // Puissante Eau bénite
                case (RegularItem)2098: // Puissant Anticorps
                {
                    TranceSeekAPI.TryRemoveItemStatuses(_v);
                    if (TranceSeekAPI.ProtectStatus.TryGetValue(_v.Target.Data, out Dictionary<BattleStatus, Int32> statusprotect))
                    {
                        foreach (BattleStatusId statusID in _v.Command.Item.Status.ToStatusList())
                        {
                            BattleStatus status = statusID.ToBattleStatus();
                            statusprotect.Add(status, 3);
                        }
                    }
                    break;
                }
                case (RegularItem)2108: // Remontant
                case (RegularItem)2109: // Remontant +
                case (RegularItem)2110: // Secondie Vie
                case (RegularItem)2117: // Troisième Vie
                {
                    if (_v.Command.ItemId == (RegularItem)2108 || _v.Command.ItemId == (RegularItem)2109 || _v.Command.ItemId == (RegularItem)2110 || _v.Command.ItemId == (RegularItem)2117)
                    {
                        if (!_v.Target.CanBeRevived() || !_v.Target.IsUnderAnyStatus(BattleStatus.Death))
                        {
                            _v.Context.Flags |= BattleCalcFlags.Miss;
                            return;
                        }
                        if (HitRateForZombie() && !TranceSeekAPI.TryMagicHit(_v))
                            return;

                        if (_v.Target.IsZombie && !_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill))
                        {
                            _v.Target.Kill();
                            return;
                        }

                        if (!_v.Target.CheckIsPlayer())
                            return;

                        _v.Target.RemoveStatus(BattleStatus.Death);
                        if (_v.Command.ItemId == (RegularItem)2108 || _v.Command.ItemId == (RegularItem)2110) // Remontant
                            HPHeal = (int)(_v.Target.MaximumHp / 2);
                        else if (_v.Command.ItemId == (RegularItem)2109 || _v.Command.ItemId == (RegularItem)2117) // Remontant +
                            HPHeal = (int)(_v.Target.MaximumHp);
                    }

                    TranceSeekAPI.TryRemoveItemStatuses(_v);
                    if (TranceSeekAPI.ProtectStatus.TryGetValue(_v.Target.Data, out Dictionary<BattleStatus, Int32> statusprotect))
                    {
                        Boolean Message = false;
                        string ItemName = FF9TextTool.ItemName(_v.Command.ItemId);
                        foreach (BattleStatusId statusID in _v.Command.Item.Status.ToStatusList())
                        {
                            BattleStatus status = statusID.ToBattleStatus();
                            statusprotect.Add(status, 255);

                            Int32 wait = (short)((400 + (_v.Caster.Will * 3)) * 30);
                            _v.Target.AddDelayedModifier(
                            target => (wait -= target.Data.cur.at_coef * BattleState.ATBTickCount) > 0,
                            target =>
                            {
                                statusprotect.Remove(status);
                                if (!Message)
                                {
                                    Dictionary<String, String> localizedStatusProtect = new Dictionary<String, String>
                                    {
                                    { "US", $"- {ItemName}" },
                                    { "UK", $"- {ItemName}" },
                                    { "JP", $"- {ItemName}" },
                                    { "ES", $"- {ItemName}" },
                                    { "FR", $"- {ItemName}" },
                                    { "GR", $"- {ItemName}" },
                                    { "IT", $"- {ItemName}" },
                                    };
                                    btl2d.Btl2dReqSymbolMessage(target.Data, "[38FF1F]", localizedStatusProtect, HUDMessage.MessageStyle.DAMAGE, 5);
                                    Message = true;
                                }
                            }
                            );                         
                        }
                    }
                    break;
                }
                case (RegularItem)2044: // Sérum réversible
                case (RegularItem)2054: // Echo réversible
                case (RegularItem)2063: // Collyre réversible
                case (RegularItem)2072: // Doux réversible
                case (RegularItem)2081: // Calmant réversible
                case (RegularItem)2090: // Eau bénite réversible
                case (RegularItem)2099: // Anticorps réversible
                {
                    if (_v.Target.IsUnderAnyStatus(_v.Command.Item.Status))
                    {
                        TranceSeekAPI.TryRemoveItemStatuses(_v);
                        _v.Target.AlterStatus(BattleStatus.Regen, _v.Target);
                    }
                    break;
                }
                case (RegularItem)2146: // Remède P
                case (RegularItem)2147: // Remède P +
                case (RegularItem)2148: // Remède M
                case (RegularItem)2149: // Remède M +
                case (RegularItem)2150: // Remède C
                case (RegularItem)2151: // Remède C +
                case (RegularItem)2152: // Remède F
                case (RegularItem)2153: // Remède F +
                case (RegularItem)2154: // Remède S
                case (RegularItem)2155: // Remède S +
                case (RegularItem)2156: // Remède Z
                case (RegularItem)2157: // Remède Z +
                case (RegularItem)2158: // Remède V
                case (RegularItem)2159: // Remède V +
                {
                    TranceSeekAPI.TryRemoveItemStatuses(_v);
                    if (TranceSeekAPI.ProtectStatus.TryGetValue(_v.Target.Data, out Dictionary<BattleStatus, Int32> statusprotect))
                    {
                        Boolean Message = false;
                        string ItemName = FF9TextTool.ItemName(_v.Command.ItemId);
                        foreach (BattleStatusId statusID in _v.Command.Item.Status.ToStatusList())
                        {
                            BattleStatus status = statusID.ToBattleStatus();
                            statusprotect.Add(status, 255);

                            Int32 wait = (short)((400 + (_v.Caster.Will * 3)) * 30);
                            _v.Target.AddDelayedModifier(
                            target => (wait -= target.Data.cur.at_coef * BattleState.ATBTickCount) > 0,
                            target =>
                            {
                                statusprotect.Remove(status);
                                if (!Message)
                                {
                                    Dictionary<String, String> localizedStatusProtect = new Dictionary<String, String>
                                    {
                                    { "US", $"- {ItemName}" },
                                    { "UK", $"- {ItemName}" },
                                    { "JP", $"- {ItemName}" },
                                    { "ES", $"- {ItemName}" },
                                    { "FR", $"- {ItemName}" },
                                    { "GR", $"- {ItemName}" },
                                    { "IT", $"- {ItemName}" },
                                    };
                                    btl2d.Btl2dReqSymbolMessage(target.Data, "[38FF1F]", localizedStatusProtect, HUDMessage.MessageStyle.DAMAGE, 5);
                                    Message = true;
                                }
                            }
                            );
                        }
                    }
                    break;
                }
                case (RegularItem)2045: // Sérum sacré
                case (RegularItem)2055: // Echo sacré
                case (RegularItem)2064: // Collyre sacré
                case (RegularItem)2073: // Doux sacré
                case (RegularItem)2082: // Calmant sacré
                case (RegularItem)2091: // Eau bénite sacrée
                case (RegularItem)2100: // Anticorps sacré
                {
                    if (_v.Target.IsUnderAnyStatus(_v.Command.Item.Status))
                    {
                        TranceSeekAPI.TryRemoveItemStatuses(_v);
                        _v.Target.AlterStatus(BattleStatus.AutoLife, _v.Target);
                    }
                    break;
                }
                case (RegularItem)2046: // Sérum complet
                case (RegularItem)2047: // Sérum total
                case (RegularItem)2056: // Echo complet            
                case (RegularItem)2057: // Echo total
                case (RegularItem)2065: // Collyre complet
                case (RegularItem)2066: // Collyre total
                case (RegularItem)2074: // Doux complet
                case (RegularItem)2075: // Doux total
                case (RegularItem)2083: // Calmant complet
                case (RegularItem)2084: // Calmant total
                case (RegularItem)2092: // Eau bénite complete
                case (RegularItem)2093: // Eau bénite totale
                case (RegularItem)2101: // Anticorps complet
                case (RegularItem)2102: // Anticorps total
                case (RegularItem)2111: // Régénération
                case (RegularItem)2112: // Régénération totale
                case (RegularItem)2118: // Regénération puissante
                case (RegularItem)2119: // Regénération parfaite
                {
                    HPHeal = (int)(_v.Target.MaximumHp);
                    MPHeal = (int)(_v.Target.MaximumMp);
                    TranceSeekAPI.TryRemoveItemStatuses(_v);
                    break;
                }
                case (RegularItem)2120: // Boccidote
                case (RegularItem)2121: // Lasidote
                case (RegularItem)2122: // Défidote          
                case (RegularItem)2123: // Antidrouil
                case (RegularItem)2124: // Cachidote
                case (RegularItem)2125: // Vaccidote
                case (RegularItem)2126: // Boccik
                case (RegularItem)2127: // Déficca
                case (RegularItem)2128: // Bobrouil
                case (RegularItem)2129: // Vacca
                case (RegularItem)2130: // Lasijeur
                case (RegularItem)2131: // Désembrik
                case (RegularItem)2132: // Casik
                case (RegularItem)2133: // Vaccik
                case (RegularItem)2134: // Désemjeur
                case (RegularItem)2135: // Cafijeur
                case (RegularItem)2136: // Déficcin
                case (RegularItem)2137: // Cachembrouil
                case (RegularItem)2138: // Caccin
                {
                    Int32 statuspresent = 0;
                    foreach (BattleStatusId statusID in _v.Command.Item.Status.ToStatusList())
                    {
                        BattleStatus status = statusID.ToBattleStatus();
                        if ((_v.Target.CurrentStatus & status) != 0)
                            statuspresent++;
                    }
                    TranceSeekAPI.TryRemoveItemStatuses(_v);
                    if (statuspresent > 1)
                    {
                        HPHeal = (int)(_v.Target.MaximumHp) / 4;
                        MPHeal = (int)(_v.Target.MaximumMp) / 4;
                    }
                    break;
                }
                case (RegularItem)2139: // Mega Antidote
                case (RegularItem)2140: // Mega Bocca
                case (RegularItem)2141: // Mega Lasik          
                case (RegularItem)2142: // Mega Défijeur
                case (RegularItem)2143: // Mega Désembrouil
                case (RegularItem)2144: // Mega Cachet
                case (RegularItem)2145: // Mega Vaccin
                case (RegularItem)2160: // Remède X
                case (RegularItem)2161: // Mega Remède
                {
                    TranceSeekAPI.TryRemoveItemStatuses(_v);
                    return;
                }
            }
            _v.Context.Flags = 0;

            if (HPHeal > 0)
                _v.Target.Flags |= (_v.Target.IsZombie ? CalcFlag.HpAlteration : CalcFlag.HpDamageOrHeal);

            if (MPHeal > 0)
                _v.Target.Flags |= (_v.Target.IsZombie ? CalcFlag.MpAlteration : CalcFlag.MpDamageOrHeal);

            if ((_v.Target.Flags & CalcFlag.HpAlteration) != 0)
                _v.Target.HpDamage = HPHeal;

            if ((_v.Target.Flags & CalcFlag.MpAlteration) != 0)
                _v.Target.MpDamage = MPHeal;

            foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(_v.Caster))
                saFeature.TriggerOnAbility(_v, "CalcDamage", false);
            foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(_v.Target))
                saFeature.TriggerOnAbility(_v, "CalcDamage", true);
        }

        private Boolean HitRateForZombie()
        {
            if (_v.Target.IsZombie)
            {
                TranceSeekAPI.MagicAccuracy(_v);
                return true;
            }
            return false;
        }
    }
}
