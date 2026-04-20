using System;
using System.Collections.Generic;
using FF9;
using Memoria.Data;

namespace Memoria.Scripts.TranceSeek
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
            if (_v.Command.ItemId == RegularItem.NoItem || _v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Artificer)) // SA Artificer
            {
                _v.Context.Flags = BattleCalcFlags.Miss;
                return;
            }

            int HPHeal = 0;
            int MPHeal = 0;
            switch (_v.Command.ItemId)
            {
                case TranceSeekRegularItem.HiPotion2: // Maxi Potion²
                {
                    HPHeal = 750;
                    break;
                }
                case TranceSeekRegularItem.UltraPotion2: // Ultra Potion²
                case TranceSeekRegularItem.MegaPotion: // Mega Potion
                {
                    HPHeal = 2000;
                    break;
                }
                case TranceSeekRegularItem.XPotion: // Potion X
                {
                    HPHeal = (int)_v.Target.MaximumHp;
                    break;
                }
                case TranceSeekRegularItem.InvigoratingEther: // Ether revigorant
                {
                    HPHeal = 250;
                    MPHeal = 85;
                    break;
                }
                case TranceSeekRegularItem.InvigoratingEther2: // Ether revigorant²
                {
                    HPHeal = 250;
                    MPHeal = 250;
                    break;
                }
                case TranceSeekRegularItem.FabulPotion: // Potion de Fabul
                {
                    HPHeal = (int)(_v.Target.MaximumHp / 4);
                    MPHeal = (int)(_v.Target.MaximumMp / 4);
                    break;
                }
                case TranceSeekRegularItem.MiniElixir: // Mini Elixir
                {
                    HPHeal = 750;
                    MPHeal = 250;
                    break;
                }
                case TranceSeekRegularItem.SmallElixir: // Petit Elixir
                {
                    HPHeal = 1500;
                    MPHeal = 150;
                    break;
                }
                case TranceSeekRegularItem.MegaEther: // Mega Ether
                {
                    MPHeal = 200;
                    break;
                }
                case TranceSeekRegularItem.EtherX: // Ether X
                {
                    MPHeal = (int)_v.Target.MaximumMp;
                    break;
                }
                case TranceSeekRegularItem.DropOfLife: // Goutte de vie
                {
                    _v.Target.TryAlterStatuses(_v.Command.Item.Status, false);
                    break;
                }
                case TranceSeekRegularItem.LifeWater: // Eau de vie
                case TranceSeekRegularItem.FountainOfLife: // Fontaine de vie
                case TranceSeekRegularItem.QuetzalcoatlsFeather: // Plume de Quetzalcoatl
                case TranceSeekRegularItem.SoaringQuetzalcoatl: // Envol de Quetzalcoatl
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

                    if (_v.Command.ItemId == TranceSeekRegularItem.LifeWater || _v.Command.ItemId == TranceSeekRegularItem.GiftOfPhoenix) // Eau de vie + Bienfait de Phénix
                        HPHeal = (int)(_v.Target.MaximumHp / 2);
                    else if (_v.Command.ItemId == TranceSeekRegularItem.FountainOfLife) // Fontaine de vie
                        HPHeal = (int)(_v.Target.MaximumHp);
                    else
                        HPHeal = (int)(_v.Target.MaximumHp / 4);

                    if (!_v.Target.TryRemoveStatuses(BattleStatus.Death))
                    {
                        _v.Context.Flags |= BattleCalcFlags.Miss;
                        return;
                    }

                    if (_v.Command.ItemId == TranceSeekRegularItem.QuetzalcoatlsFeather) // Plume de Quetzalcoatl
                    {
                        _v.Target.AddDelayedModifier(
                        target => target.CurrentHp == 0,
                        target =>
                        {
                            target.AlterStatus(BattleStatus.Shell, target);
                        }
                        );
                    }
                    else if (_v.Command.ItemId == TranceSeekRegularItem.SoaringQuetzalcoatl)  // Envol de Quetzalcoatl
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
                case TranceSeekRegularItem.PhoenixsTear: // Larme de Phénix
                {
                    _v.Target.RemoveStatus(BattleStatus.Doom);
                    _v.Target.AlterStatus(BattleStatus.Regen, _v.Target);
                    break;
                }
                case TranceSeekRegularItem.GiftOfPhoenix: // Bienfait de Phénix
                case TranceSeekRegularItem.PhoenixBlessing: // Bénédiction de Phénix
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
                    if (_v.Command.ItemId == TranceSeekRegularItem.GiftOfPhoenix) // Bienfait de Phénix
                        HPHeal = (int)(_v.Target.MaximumHp / 2);
                    else // Bénédiction de Phénix
                        HPHeal = (int)(_v.Target.MaximumHp);
                    break;
                }
                case TranceSeekRegularItem.AngelPalm: // Paume d'Ange
                case TranceSeekRegularItem.AngelsTouch: // Toucher d'Ange
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

                    if (_v.Command.ItemId == TranceSeekRegularItem.AngelPalm) // Paume d'Ange
                        MPHeal = (int)(_v.Target.MaximumMp / 2);
                    else if (_v.Command.ItemId == TranceSeekRegularItem.AngelsTouch) // Toucher d'Ange
                        MPHeal = (int)(_v.Target.MaximumMp);

                    if (!_v.Target.TryRemoveStatuses(BattleStatus.Death))
                    {
                        _v.Context.Flags |= BattleCalcFlags.Miss;
                        return;
                    }
                    break;
                }
                case TranceSeekRegularItem.MegaPhoenixDown: // Méga Phénix
                case TranceSeekRegularItem.MegaPhoenixPinion: // Méga Résurex
                {
                    if (!_v.Target.CanBeRevived())
                        return;

                    if (_v.Target.Accessory == TranceSeekRegularItem.CursedRing) // Anneau Maudit
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
                case TranceSeekRegularItem.SuperElixir: // Super Elixir
                case TranceSeekRegularItem.HiElixir: // Maxi Elixir
                case TranceSeekRegularItem.UltraElixir: // Ultra Elixir
                case TranceSeekRegularItem.RefinedElixir: // Elixir raffiné
                case TranceSeekRegularItem.FabulousElixir: // Elixir fabuleux
                case TranceSeekRegularItem.SuperMegalixir: // Super Megalixir
                case TranceSeekRegularItem.HiMegalixir: // Maxi Megalixir
                case TranceSeekRegularItem.UltraMegalixir: // Ultra Megalixir
                case TranceSeekRegularItem.RefinedMegalixir: // Megalixir raffiné
                case TranceSeekRegularItem.FabulousMegalixir: // Megalixir fabuleux
                {
                    HPHeal = (int)(_v.Target.MaximumHp);
                    MPHeal = (int)(_v.Target.MaximumMp);
                    if (_v.Command.ItemId == TranceSeekRegularItem.UltraElixir || _v.Command.ItemId == TranceSeekRegularItem.UltraMegalixir) // Ultra Elixir + Ultra Megalixir
                        _v.Target.AlterStatus(TranceSeekStatus.ArmorUp, _v.Target);
                    else if (_v.Command.ItemId == TranceSeekRegularItem.FabulousElixir || _v.Command.ItemId == TranceSeekRegularItem.FabulousMegalixir) // Elixir fabuleux + Megalixir fabuleux
                        _v.Target.AlterStatus(TranceSeekStatus.MentalUp, _v.Target);

                    _v.Target.TryAlterStatuses(_v.Command.Item.Status, false);
                    break;
                }
                case TranceSeekRegularItem.Phoenixir: // Phénixir
                case TranceSeekRegularItem.Pinionixir: // Réselixir
                case TranceSeekRegularItem.MegaPhoenixir: // Méga Phénixir
                case TranceSeekRegularItem.MegaPinionixir: // Méga Réselixir
                case TranceSeekRegularItem.Reviviscence: // Reviviscence
                case TranceSeekRegularItem.TantalasMegalixir: // Megalixir des Tantalas
                {
                    if (HitRateForZombie() && !TranceSeekAPI.TryMagicHit(_v))
                        return;

                    if (_v.Target.IsZombie && !_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill))
                    {
                        _v.Target.Kill();
                        return;
                    }

                    if (_v.Command.ItemId == TranceSeekRegularItem.Reviviscence) // Reviviscence
                    {
                        BattleStatusId[] statuslist = { BattleStatusId.Protect, BattleStatusId.Shell, BattleStatusId.Regen, BattleStatusId.AutoLife,
                            TranceSeekStatusId.ArmorUp, TranceSeekStatusId.MentalUp};

                        BattleStatusId statusselected = statuslist[GameRandom.Next16() % statuslist.Length];
                        btl_stat.AlterStatus(_v.Target, statusselected, _v.Caster);
                    }
                    else if (_v.Command.ItemId == TranceSeekRegularItem.TantalasMegalixir) // Megalixir des Tantalas
                    {
                        _v.Target.AlterStatus(BattleStatus.Protect, _v.Target);
                        _v.Target.AlterStatus(BattleStatus.Shell, _v.Target);
                        _v.Target.AlterStatus(BattleStatus.AutoLife, _v.Target);
                        _v.Target.AlterStatus(BattleStatus.Regen, _v.Target);
                        _v.Target.AlterStatus(TranceSeekStatus.ArmorUp, _v.Target);
                        _v.Target.AlterStatus(TranceSeekStatus.MentalUp, _v.Target);
                    }
                    _v.Target.RemoveStatus(BattleStatus.Death);
                    if (_v.Command.ItemId == TranceSeekRegularItem.AngelPalm) // Réselixir
                        _v.Target.RemoveStatus(BattleStatus.Doom);

                    HPHeal = (int)(_v.Target.MaximumHp);
                    MPHeal = (int)(_v.Target.MaximumMp);
                    break;
                }
                case TranceSeekRegularItem.YellowElixir: // Elixir Jaune
                case TranceSeekRegularItem.RedElixir: // Elixir Rouge
                case TranceSeekRegularItem.CyanElixir: // Elixir Cyan
                case TranceSeekRegularItem.PurpleElixir: // Elixir Violet
                case TranceSeekRegularItem.BlueElixir: // Elixir Bleu
                case TranceSeekRegularItem.CrystallineElixir: // Elixir Cristallin
                case TranceSeekRegularItem.BrownElixir: // Elixir Marron
                case TranceSeekRegularItem.GreenElixir: // Elixir Vert
                case TranceSeekRegularItem.BlackElixir: // Elixir Noir
                case TranceSeekRegularItem.WhiteElixir: // Elixir Blanc
                case TranceSeekRegularItem.YellowMegalixir: // Megalixir Jaune
                case TranceSeekRegularItem.RedMegalixir: // Megalixir Rouge
                case TranceSeekRegularItem.CyanMegalixir: // Megalixir Cyan
                case TranceSeekRegularItem.PurpleMegalixir: // Megalixir Violet
                case TranceSeekRegularItem.BlueMegalixir: // Megalixir Bleu
                case TranceSeekRegularItem.CrystallineMegalixir: // Megalixir Cristallin
                case TranceSeekRegularItem.BrownMegalixir: // Megalixir Marron
                case TranceSeekRegularItem.GreenMegalixir: // Megalixir Vert
                case TranceSeekRegularItem.BlackMegalixir: // Megalixir Noir
                case TranceSeekRegularItem.WhiteMegalixir: // Megalixir Blanc
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
            { TranceSeekRegularItem.YellowElixir, EffectElement.Thunder },
            { TranceSeekRegularItem.RedElixir, EffectElement.Fire },
            { TranceSeekRegularItem.CyanElixir, EffectElement.Cold },
            { TranceSeekRegularItem.PurpleElixir, EffectElement.None }, // Gravity
            { TranceSeekRegularItem.BlueElixir, EffectElement.Aqua },
            { TranceSeekRegularItem.CrystallineElixir, EffectElement.None },
            { TranceSeekRegularItem.BrownElixir, EffectElement.Earth },
            { TranceSeekRegularItem.GreenElixir, EffectElement.Wind },
            { TranceSeekRegularItem.BlackElixir, EffectElement.Darkness },
            { TranceSeekRegularItem.WhiteElixir, EffectElement.Holy },
            { TranceSeekRegularItem.YellowMegalixir, EffectElement.Thunder },
            { TranceSeekRegularItem.RedMegalixir, EffectElement.Fire },
            { TranceSeekRegularItem.CyanMegalixir, EffectElement.Cold },
            { TranceSeekRegularItem.PurpleMegalixir, EffectElement.None }, // Gravity
            { TranceSeekRegularItem.BlueMegalixir, EffectElement.Aqua },
            { TranceSeekRegularItem.CrystallineMegalixir, EffectElement.None },
            { TranceSeekRegularItem.BrownMegalixir, EffectElement.Earth },
            { TranceSeekRegularItem.GreenMegalixir, EffectElement.Wind },
            { TranceSeekRegularItem.BlackMegalixir, EffectElement.Darkness },
            { TranceSeekRegularItem.WhiteMegalixir, EffectElement.Holy },
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


