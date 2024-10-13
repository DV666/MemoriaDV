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
            if (_v.Command.ItemId == RegularItem.NoItem)
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
                {
                    HPHeal = 200;
                    _v.TryRemoveItemStatuses();
                    break;
                }
                case (RegularItem)2040: // Maxi Sérum
                case (RegularItem)2050: // Maxi Echo
                {
                    HPHeal = 500;
                    _v.TryRemoveItemStatuses();
                    break;
                }
                case (RegularItem)2041: // Ultra Sérum
                case (RegularItem)2051: // Ultra Echo
                {
                    HPHeal = 1250;
                    _v.TryRemoveItemStatuses();
                    break;
                }
                case (RegularItem)2042: // Sérum amélioré
                case (RegularItem)2052: // Echo amélioré
                {
                    _v.TryRemoveItemStatuses();
                    TranceSeekCustomAPI.ProtectStatus[_v.Target.Data].Add(_v.Command.Item.Status, 1);
                    break;
                }
                case (RegularItem)2043: // Puissant sérum
                case (RegularItem)2053: // Puissant echo
                {
                    _v.TryRemoveItemStatuses();
                    TranceSeekCustomAPI.ProtectStatus[_v.Target.Data].Add(_v.Command.Item.Status, 2);
                    break;
                }
                case (RegularItem)2044: // Sérum réversible
                case (RegularItem)2054: // Echo réversible
                {
                    if (_v.Target.IsUnderAnyStatus(_v.Command.Item.Status))
                    {
                        _v.TryRemoveItemStatuses();
                        _v.Target.AlterStatus(BattleStatus.Regen, _v.Target);
                    }
                    break;
                }
                case (RegularItem)2045: // Sérum sacré
                case (RegularItem)2055: // Echo sacré
                {
                    if (_v.Target.IsUnderAnyStatus(_v.Command.Item.Status))
                    {
                        _v.TryRemoveItemStatuses();
                        _v.Target.AlterStatus(BattleStatus.AutoLife, _v.Target);
                    }
                    break;
                }
                case (RegularItem)2046: // Sérum complet
                case (RegularItem)2047: // Sérum total
                case (RegularItem)2056: // Echo complet            
                case (RegularItem)2057: // Echo total
                {
                    HPHeal = (int)(_v.Target.MaximumHp);
                    MPHeal = (int)(_v.Target.MaximumMp);
                    _v.TryRemoveItemStatuses();
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
