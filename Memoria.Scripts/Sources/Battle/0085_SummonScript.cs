using System;
using FF9;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Shiva, Ifrit, Ramuh, Leviathan, Bahamut, Ark, Fenrir, Madeen, Terra Homing
    /// </summary>
    [BattleScript(Id)]
    public sealed class SummonScript : IBattleScript
    {
        public const Int32 Id = 0085;

        private readonly BattleCalculator _v;

        public SummonScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            _v.NormalMagicParams();
            TranceSeekCustomAPI.CasterPenaltyMini(_v);
            TranceSeekCustomAPI.PenaltyShellAttack(_v);
            TranceSeekCustomAPI.BonusElement(_v);
            TranceSeekCustomAPI.MagicAccuracy(_v);
            switch (_v.Command.AbilityId)
            {
                case BattleAbilityId.Shiva:
                case BattleAbilityId.DiamondDust:
                case (BattleAbilityId)1529:
                {
                    _v.Context.HitRate += ((ff9item.FF9Item_GetCount(RegularItem.Opal) + 1)) / 2;
                    if (_v.Target.IsPlayer)
                    {
                        _v.Command.AbilityCategory -= 16; // Remove Magical effect to prevent Vanish to dissapear.
                        _v.Target.TryRemoveStatuses(_v.Command.AbilityStatus);
                        _v.Target.AlterStatus(TranceSeekCustomStatus.MentalUp);
                    }
                    else
                    {
                        _v.Context.AttackPower += _v.Caster.Level;
                    }
                    break;
                }
                case BattleAbilityId.Ifrit:
                case BattleAbilityId.FlamesofHell:
                case (BattleAbilityId)1530:
                {
                    _v.Context.HitRate += ((ff9item.FF9Item_GetCount(RegularItem.Topaz) + 1)) / 2;
                    if (_v.Target.IsPlayer)
                    {
                        _v.Command.AbilityCategory -= 16; // Remove Magical effect to prevent Vanish to dissapear.
                        _v.Target.TryRemoveStatuses(_v.Command.AbilityStatus);
                        _v.Target.AlterStatus(TranceSeekCustomStatus.ArmorUp);
                    }
                    else
                    {
                        _v.Context.AttackPower += _v.Caster.Level;
                    }
                    break;
                }
                case BattleAbilityId.Ramuh:
                case BattleAbilityId.JudgementBolt:
                case (BattleAbilityId)1531:
                {
                    _v.Context.HitRate += ((ff9item.FF9Item_GetCount(RegularItem.Peridot) + 1)) / 2;
                    if (_v.Target.IsPlayer)
                    {
                        _v.Command.AbilityCategory -= 16; // Remove Magical effect to prevent Vanish to dissapear.
                        _v.Target.TryRemoveStatuses(_v.Command.AbilityStatus);
                        _v.Target.AlterStatus(TranceSeekCustomStatus.MagicUp);
                    }
                    else
                    {
                        _v.Context.AttackPower += _v.Caster.Level;
                    }
                    break;
                }
                case BattleAbilityId.Leviathan:
                case BattleAbilityId.Tsunami:
                case (BattleAbilityId)1534:
                {
                    _v.Context.HitRate += ((ff9item.FF9Item_GetCount(RegularItem.Aquamarine) + 1)) / 2;
                    if (_v.Target.IsPlayer)
                    {
                        _v.Command.AbilityCategory -= 16; // Remove Magical effect to prevent Vanish to dissapear.
                        _v.Target.TryRemoveStatuses(_v.Command.AbilityStatus);
                        _v.Target.AlterStatus(BattleStatus.Regen);
                    }
                    else
                    {
                        _v.Context.AttackPower += _v.Caster.Level;
                    }
                    break;
                }
                case BattleAbilityId.Bahamut:
                case BattleAbilityId.MegaFlare:
                case (BattleAbilityId)1535:
                {
                    _v.Context.HitRate += ((ff9item.FF9Item_GetCount(RegularItem.Garnet) + 1)) / 2;
                    if (_v.Target.IsPlayer)
                    {
                        _v.Command.AbilityCategory -= 16; // Remove Magical effect to prevent Vanish to dissapear.
                        _v.Target.TryRemoveStatuses(_v.Command.AbilityStatus);
                        _v.Target.Flags = CalcFlag.MpDamageOrHeal;
                        _v.Target.MpDamage = _v.Caster.Magic + Comn.random16() % (1 + (_v.Caster.Level + _v.Caster.Magic) / 4);
                    }
                    else
                    {
                        _v.Context.AttackPower += _v.Caster.Level;
                    }
                    break;
                }
                case BattleAbilityId.Ark:
                case BattleAbilityId.EternalDarkness:
                case (BattleAbilityId)1536:
                {
                    _v.Context.HitRate = ((ff9item.FF9Item_GetCount(RegularItem.LapisLazuli) + 1)) / 2;
                    if (_v.Target.IsPlayer)
                    {
                        _v.Command.AbilityCategory -= 16; // Remove Magical effect to prevent Vanish to dissapear.
                        _v.Target.TryRemoveStatuses(_v.Command.AbilityStatus);
                        _v.Target.AlterStatus(TranceSeekCustomStatus.PowerUp | TranceSeekCustomStatus.MagicUp | TranceSeekCustomStatus.ArmorUp | TranceSeekCustomStatus.MentalUp);
                    }
                    else
                    {
                        _v.Context.AttackPower += _v.Caster.Level;
                    }
                    break;
                }
                case BattleAbilityId.Carbuncle1:
                case BattleAbilityId.Carbuncle2:
                case BattleAbilityId.Carbuncle3:
                case BattleAbilityId.Carbuncle4:
                {  
                    byte will = _v.Caster.Will; // TODO - To improve ? Move in Memoria.ini
                    if (ff9item.FF9Item_GetCount(RegularItem.Ruby) > 0)
                    {
                        _v.Caster.Will = (byte)(_v.Caster.Will + _v.Caster.Will * ff9item.FF9Item_GetCount(RegularItem.Ruby) / 100L);
                    }
                    if (!_v.Command.IsShortSummon && _v.Command.Id == BattleCommandId.SummonEiko)
                    {
                        _v.Caster.Will = (byte)(_v.Caster.Will * 2);
                    }
                    TranceSeekCustomAPI.TryAlterCommandStatuses(_v);
                    _v.Caster.Will = will;
                    return;
                }
                case BattleAbilityId.Fenrir1:
                case BattleAbilityId.Fenrir2:
                {
                    _v.Context.AttackPower += _v.Caster.Level;
                    _v.Context.HitRate += ((ff9item.FF9Item_GetCount(RegularItem.Sapphire) + 1)) / 2;
                    break;
                }
                case BattleAbilityId.Madeen:
                    _v.Context.AttackPower += _v.Caster.Level;
                    break;
            }
            if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)208) && _v.Target.IsPlayer)
            {
                if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1208))
                {
                    _v.CalcHpMagicRecovery();
                    _v.Target.HpDamage /= 3;
                }
            }
            else if (TranceSeekCustomAPI.CanAttackMagic(_v))
            {
                _v.CalcHpDamage();

                // TODO - Create a new function for that ? Make it for MagicScript like for example ?                
                _v.Target.PenaltyShellHitRate();
                if (_v.Command.IsShortSummon)
                    _v.Context.HitRate = _v.Context.HitRate * 2 / 3;

                foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(_v.Caster))
                    saFeature.TriggerOnAbility(_v, "HitRateSetup", false);
                foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(_v.Target))
                    saFeature.TriggerOnAbility(_v, "HitRateSetup", true);

                if (_v.Context.HitRate > Comn.random16() % 100 && _v.Context.Evade <= Comn.random16() % 100)
                {
                    if (_v.Command.AbilityId == BattleAbilityId.Bahamut || _v.Command.AbilityId == BattleAbilityId.MegaFlare)
                    {
                        _v.Target.RemoveStatus(BattleStatus.Protect);
                        _v.Target.RemoveStatus(BattleStatus.Shell);
                        _v.Target.RemoveStatus(BattleStatus.Reflect);
                    }
                    else
                    {
                        _v.Target.TryAlterStatuses(_v.Command.AbilityStatus, false, _v.Caster);
                    }
                }
            }
        }
    }
}
