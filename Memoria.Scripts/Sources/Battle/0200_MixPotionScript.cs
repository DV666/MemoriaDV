using System;
using System.Runtime.Remoting.Contexts;
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
            if (_v.Command.ItemId == RegularItem.NoItem)
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
                    TranceSeekCustomAPI.TryAlterCommandStatuses(_v);
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
                    if (HitRateForZombie() && !_v.TryMagicHit())
                        return;

                    if (_v.Target.IsZombie && !_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill))
                    {
                        _v.Target.Kill();
                        return;
                    }

                    if (!_v.Target.CheckIsPlayer())
                        return;

                    if (_v.Command.ItemId == (RegularItem)2012) // Eau de vie
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
            }

            if (HPHeal > 0)
                _v.Target.Flags |= (_v.Target.IsZombie ? CalcFlag.HpAlteration : CalcFlag.HpDamageOrHeal);

            if (MPHeal > 0)
                _v.Target.Flags |= (_v.Target.IsZombie ? CalcFlag.MpAlteration : CalcFlag.MpDamageOrHeal);

            if ((_v.Target.Flags & CalcFlag.HpAlteration) != 0)
                _v.Target.HpDamage = HPHeal;

            if ((_v.Target.Flags & CalcFlag.MpAlteration) != 0)
                _v.Target.MpDamage = MPHeal;

            _v.CalcDamageCommon();
        }

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
