using System;
using System.Collections.Generic;
using FF9;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Special
    /// </summary>
    [BattleScript(Id)]
    public sealed class MixPotionScript : IBattleScript
    {
        public const Int32 Id = 0200;

        private readonly BattleCalculator _v;

        public MixPotionScript(BattleCalculator v)
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
                case (RegularItem)2000: // Maxi Potion²
                {
                    HPHeal = 750;
                    break;
                }
                case (RegularItem)2001: // Ultra Potion²
                case (RegularItem)2002: // Mega Potion
                {
                    HPHeal = 2000;
                    break;
                }
                case (RegularItem)2003: // Potion X
                {
                    HPHeal = (int)_v.Target.MaximumHp;
                    break;
                }
                case (RegularItem)2004: // Ether revigorant
                {
                    HPHeal = 250;
                    MPHeal = 85;
                    break;
                }
                case (RegularItem)2005: // Ether revigorant²
                {
                    HPHeal = 250;
                    MPHeal = 250;
                    break;
                }
                case (RegularItem)2006: // Potion de Fabul
                {
                    HPHeal = (int)(_v.Target.MaximumHp / 4);
                    MPHeal = (int)(_v.Target.MaximumMp / 4);
                    break;
                }
                case (RegularItem)2007: // Mini Elixir
                {
                    HPHeal = 750;
                    MPHeal = 250;
                    break;
                }
                case (RegularItem)2008: // Petit Elixir
                {
                    HPHeal = 1500;
                    MPHeal = 150;
                    break;
                }
                case (RegularItem)2009: // Mega Ether
                {
                    MPHeal = 200;
                    break;
                }
                case (RegularItem)2010: // Ether X
                {
                    MPHeal = (int)_v.Target.MaximumMp;
                    break;
                }
                case (RegularItem)2011: // Goutte de vie
                {
                    _v.Target.TryAlterStatuses(_v.Command.Item.Status, false);
                    break;
                }
                case (RegularItem)2012: // Eau de vie
                case (RegularItem)2013: // Fontaine de vie
                case (RegularItem)2014: // Plume de Quetzalcoatl
                case (RegularItem)2015: // Envol de Quetzalcoatl
                {
                    if (!_v.Target.CanBeRevived())
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

                    if (_v.Command.ItemId == (RegularItem)2012 || _v.Command.ItemId == (RegularItem)2017) // Eau de vie + Bienfait de Phénix
                        HPHeal = (int)(_v.Target.MaximumHp / 2);
                    else if (_v.Command.ItemId == (RegularItem)2013) // Fontaine de vie
                        HPHeal = (int)(_v.Target.MaximumHp);
                    else
                        HPHeal = (int)(_v.Target.MaximumHp / 4);

                    if (!_v.Target.TryRemoveStatuses(BattleStatus.Death))
                    {
                        _v.Context.Flags |= BattleCalcFlags.Miss;
                        return;
                    }

                    if (_v.Command.ItemId == (RegularItem)2014) // Plume de Quetzalcoatl
                    {
                        _v.Target.AddDelayedModifier(
                        target => target.CurrentHp == 0,
                        target =>
                        {
                            target.AlterStatus(BattleStatus.Shell, target);
                        }
                        );
                    }
                    else if (_v.Command.ItemId == (RegularItem)2015)  // Envol de Quetzalcoatl
                    {
                        _v.Target.AddDelayedModifier(
                        target => target.CurrentHp == 0,
                        target =>
                        {
                            target.AlterStatus(BattleStatus.Protect, target);
                            target.AlterStatus(BattleStatus.Shell, target);
                            target.AlterStatus(BattleStatus.Regen, target);
                        }
                        );
                    }
                    break;
                }
                case (RegularItem)2016: // Larme de Phénix
                {
                    _v.Target.RemoveStatus(BattleStatus.Doom);
                    _v.Target.AlterStatus(BattleStatus.Regen, _v.Target);
                    break;
                }
                case (RegularItem)2017: // Bienfait de Phénix
                case (RegularItem)2018: // Bénédiction de Phénix
                {
                    if (HitRateForZombie() && !TranceSeekAPI.TryMagicHit(_v))
                        return;

                    if (_v.Target.IsZombie && !_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill))
                    {
                        _v.Target.Kill();
                        return;
                    }

                    _v.Target.RemoveStatus(BattleStatus.Doom);
                    _v.Target.RemoveStatus(BattleStatus.Death);
                    if (_v.Command.ItemId == (RegularItem)2017) // Bienfait de Phénix
                        HPHeal = (int)(_v.Target.MaximumHp / 2);
                    else // Bénédiction de Phénix
                        HPHeal = (int)(_v.Target.MaximumHp);
                    break;
                }
                case (RegularItem)2019: // Paume d'Ange
                case (RegularItem)2020: // Toucher d'Ange
                {
                    if (!_v.Target.CanBeRevived())
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

                    if (_v.Command.ItemId == (RegularItem)2019) // Paume d'Ange
                        MPHeal = (int)(_v.Target.MaximumMp / 2);
                    else if (_v.Command.ItemId == (RegularItem)2020) // Toucher d'Ange
                        MPHeal = (int)(_v.Target.MaximumMp);

                    if (!_v.Target.TryRemoveStatuses(BattleStatus.Death))
                    {
                        _v.Context.Flags |= BattleCalcFlags.Miss;
                        return;
                    }
                    break;
                }
                case (RegularItem)2021: // Méga Phénix
                case (RegularItem)2022: // Méga Résurex
                {
                    if (!_v.Target.CanBeRevived())
                        return;

                    if (_v.Target.Accessory == (RegularItem)1213) // Anneau Maudit
                    {
                        if (_v.Command.ItemId == RegularItem.PhoenixPinion && (_v.Target.Data.stat.permanent & BattleStatus.Doom) != 0 && !_v.Target.IsUnderAnyStatus(BattleStatus.Death))
                        {
                            _v.Context.Flags |= BattleCalcFlags.Miss;
                            return;
                        }
                    }

                    if (_v.Target.IsZombie && !_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill))
                    {
                        if ((_v.Target.CurrentHp = (UInt32)(GameRandom.Next8() % 10)) == 0)
                            _v.Target.Kill();
                    }
                    else if (_v.Target.CheckIsPlayer())
                    {
                        HPHeal = (1 + GameRandom.Next8() % 10);
                        TranceSeekAPI.TryRemoveItemStatuses(_v);
                    }
                    break;
                }
                case (RegularItem)2023: // Super Elixir
                case (RegularItem)2024: // Maxi Elixir
                case (RegularItem)2025: // Ultra Elixir
                case (RegularItem)2026: // Elixir raffiné
                case (RegularItem)2027: // Elixir fabuleux
                case (RegularItem)2030: // Super Megalixir
                case (RegularItem)2031: // Maxi Megalixir
                case (RegularItem)2032: // Ultra Megalixir
                case (RegularItem)2033: // Megalixir raffiné
                case (RegularItem)2034: // Megalixir fabuleux
                {
                    HPHeal = (int)(_v.Target.MaximumHp);
                    MPHeal = (int)(_v.Target.MaximumMp);
                    if (_v.Command.ItemId == (RegularItem)2025 || _v.Command.ItemId == (RegularItem)2032) // Ultra Elixir + Ultra Megalixir
                        _v.Target.AlterStatus(TranceSeekStatus.ArmorUp, _v.Target);
                    else if (_v.Command.ItemId == (RegularItem)2027 || _v.Command.ItemId == (RegularItem)2034) // Elixir fabuleux + Megalixir fabuleux
                        _v.Target.AlterStatus(TranceSeekStatus.MentalUp, _v.Target);

                    _v.Target.TryAlterStatuses(_v.Command.Item.Status, false);
                    break;
                }
                case (RegularItem)2028: // Phénixir
                case (RegularItem)2029: // Réselixir
                case (RegularItem)2035: // Méga Phénixir
                case (RegularItem)2036: // Méga Réselixir
                case (RegularItem)2037: // Reviviscence
                case (RegularItem)2038: // Megalixir des Tantalas
                {
                    if (HitRateForZombie() && !TranceSeekAPI.TryMagicHit(_v))
                        return;

                    if (_v.Target.IsZombie && !_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill))
                    {
                        _v.Target.Kill();
                        return;
                    }

                    if (_v.Command.ItemId == (RegularItem)2037) // Reviviscence
                    {
                        BattleStatusId[] statuslist = { BattleStatusId.Protect, BattleStatusId.Shell, BattleStatusId.Regen, BattleStatusId.AutoLife,
                            TranceSeekStatusId.ArmorUp, TranceSeekStatusId.MentalUp};

                        BattleStatusId statusselected = statuslist[GameRandom.Next16() % statuslist.Length];
                        btl_stat.AlterStatus(_v.Target, statusselected, _v.Caster);
                    }
                    else if (_v.Command.ItemId == (RegularItem)2038) // Megalixir des Tantalas
                    {
                        _v.Target.AlterStatus(BattleStatus.Protect, _v.Target);
                        _v.Target.AlterStatus(BattleStatus.Shell, _v.Target);
                        _v.Target.AlterStatus(BattleStatus.AutoLife, _v.Target);
                        _v.Target.AlterStatus(BattleStatus.Regen, _v.Target);
                        _v.Target.AlterStatus(TranceSeekStatus.ArmorUp, _v.Target);
                        _v.Target.AlterStatus(TranceSeekStatus.MentalUp, _v.Target);
                    }
                    _v.Target.RemoveStatus(BattleStatus.Death);
                    if (_v.Command.ItemId == (RegularItem)2019) // Réselixir
                        _v.Target.RemoveStatus(BattleStatus.Doom);

                    HPHeal = (int)(_v.Target.MaximumHp);
                    MPHeal = (int)(_v.Target.MaximumMp);
                    break;
                }
                case (RegularItem)2342: // Elixir Jaune
                case (RegularItem)2343: // Elixir Rouge
                case (RegularItem)2344: // Elixir Cyan
                case (RegularItem)2345: // Elixir Violet
                case (RegularItem)2346: // Elixir Bleu
                case (RegularItem)2347: // Elixir Cristallin
                case (RegularItem)2348: // Elixir Marron
                case (RegularItem)2350: // Elixir Vert
                case (RegularItem)2351: // Elixir Noir
                case (RegularItem)2352: // Elixir Blanc
                case (RegularItem)2353: // Megalixir Jaune
                case (RegularItem)2354: // Megalixir Rouge
                case (RegularItem)2355: // Megalixir Cyan
                case (RegularItem)2356: // Megalixir Violet
                case (RegularItem)2357: // Megalixir Bleu
                case (RegularItem)2358: // Megalixir Cristallin
                case (RegularItem)2359: // Megalixir Marron
                case (RegularItem)2361: // Megalixir Vert
                case (RegularItem)2362: // Megalixir Noir
                case (RegularItem)2363: // Megalixir Blanc
                {

                    break;
                }
            }

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

        public static Dictionary<RegularItem, EffectElement> ElementItem = new Dictionary<RegularItem, EffectElement>
        {
            { (RegularItem)2342, EffectElement.Thunder },
            { (RegularItem)2343, EffectElement.Fire },
            { (RegularItem)2344, EffectElement.Cold },
            { (RegularItem)2345, EffectElement.None }, // Gravity
            { (RegularItem)2346, EffectElement.Aqua },
            { (RegularItem)2347, EffectElement.None },
            { (RegularItem)2348, EffectElement.Earth },
            { (RegularItem)2350, EffectElement.Wind },
            { (RegularItem)2351, EffectElement.Darkness },
            { (RegularItem)2352, EffectElement.Holy },
            { (RegularItem)2353, EffectElement.Thunder },
            { (RegularItem)2354, EffectElement.Fire },
            { (RegularItem)2355, EffectElement.Cold },
            { (RegularItem)2356, EffectElement.None }, // Gravity
            { (RegularItem)2357, EffectElement.Aqua },
            { (RegularItem)2358, EffectElement.None },
            { (RegularItem)2359, EffectElement.Earth },
            { (RegularItem)2361, EffectElement.Wind },
            { (RegularItem)2362, EffectElement.Darkness },
            { (RegularItem)2363, EffectElement.Holy },
        };

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
