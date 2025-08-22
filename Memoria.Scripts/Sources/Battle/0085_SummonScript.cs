using System;
using FF9;
using Memoria.Data;
using static SiliconStudio.Social.ResponseData;

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
            TranceSeekAPI.CasterPenaltyMini(_v);
            TranceSeekAPI.PenaltyShellAttack(_v);
            TranceSeekAPI.BonusElement(_v);
            TranceSeekAPI.MagicAccuracy(_v);
            int BonusTurbo = _v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Boost_Boosted) ? 3 : (_v.Caster.HasSupportAbilityByIndex(SupportAbility.Boost) ? 2 : 1);
            switch (_v.Command.AbilityId)
            {
                case BattleAbilityId.Shiva:
                case BattleAbilityId.DiamondDust:
                case (BattleAbilityId)1529:
                {
                    _v.Context.HitRate += ((ff9item.FF9Item_GetCount(RegularItem.Opal) + 1)) / 2;
                    if (_v.Target.IsPlayer)
                    {
                        _v.Command.AbilityCategory -= 16; // Remove Magical effect to prevent Vanish to disappear.
                        _v.Target.TryRemoveStatuses(_v.Command.AbilityStatus);              
                        btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.MentalUp, parameters: $"+{BonusTurbo}");
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
                        _v.Command.AbilityCategory -= 16; // Remove Magical effect to prevent Vanish to disappear.
                        _v.Target.TryRemoveStatuses(_v.Command.AbilityStatus);
                        btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.ArmorUp, parameters: $"+{BonusTurbo}");
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
                        _v.Command.AbilityCategory -= 16; // Remove Magical effect to prevent Vanish to disappear.
                        _v.Target.TryRemoveStatuses(_v.Command.AbilityStatus);
                        btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.MagicUp, parameters: $"+{BonusTurbo}");
                    }
                    break;
                }
                case BattleAbilityId.Odin:
                case BattleAbilityId.Zantetsuken:
                case (BattleAbilityId)1533:
                {
                    if (_v.Target.IsPlayer)
                    {
                        _v.Command.AbilityCategory -= 16; // Remove Magical effect to prevent Vanish to dissapear.
                        btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.PowerUp, parameters: $"+{BonusTurbo + 1}");
                    }
                    else if (!_v.Caster.HasSupportAbilityByIndex(SupportAbility.OdinSword))
                    {
                        _v.Context.HitRate += (Int16)(ff9item.FF9Item_GetCount(RegularItem.Ore) >> 1);
                        if (TranceSeekAPI.CheckUnsafetyOrGuard(_v))
                        {
                            TranceSeekAPI.MagicAccuracy(_v);                     
                            if (TranceSeekAPI.TryMagicHit(_v))
                                TranceSeekAPI.TryAlterCommandStatuses(_v);
                        }
                        return;
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
                        _v.Command.AbilityCategory -= 16; // Remove Magical effect to prevent Vanish to disappear.
                        _v.Target.TryRemoveStatuses(_v.Command.AbilityStatus);
                        _v.Target.AlterStatus(BattleStatus.Regen);
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
                        _v.Command.AbilityCategory -= 16; // Remove Magical effect to prevent Vanish to disappear.
                        _v.Target.Flags = CalcFlag.MpDamageOrHeal;
                        _v.Target.MpDamage = (_v.Caster.Magic + Comn.random16() % (1 + (_v.Caster.Level + _v.Caster.Magic) / 2)) / 3;
                        _v.Target.AlterStatus(BattleStatus.Float);
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
                        _v.Command.AbilityCategory -= 16; // Remove Magical effect to prevent Vanish to disappear.
                        _v.Target.TryRemoveStatuses(_v.Command.AbilityStatus);
                        btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.PowerUp, parameters: $"+{BonusTurbo}");
                        btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.MagicUp, parameters: $"+{BonusTurbo}");
                        btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.ArmorUp, parameters: $"+{BonusTurbo}");
                        btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.MentalUp, parameters: $"+{BonusTurbo}");
                    }
                    break;
                }
                case BattleAbilityId.Carbuncle1:
                case BattleAbilityId.Carbuncle2:
                case BattleAbilityId.Carbuncle3:
                case BattleAbilityId.Carbuncle4:
                case (BattleAbilityId)1578:
                case (BattleAbilityId)1579:
                case (BattleAbilityId)1580:
                case (BattleAbilityId)1581:
                {
                    int BonusFormula = (400 + _v.Caster.Will * 3);
                    TranceSeekAPI.TryAlterCommandStatuses(_v);

                    if (_v.Command.IsShortSummon)
                    {
                        if (_v.Caster.HasSupportAbilityByIndex(SupportAbility.Boost))
                            return;

                        BonusFormula = BonusFormula / 3;
                        TranceSeekAPI.AlterStatusDuration(_v, _v.Command.AbilityStatus, BonusFormula, false);
                    }
                    else if (_v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Boost_Boosted))
                        TranceSeekAPI.AlterStatusDuration(_v, _v.Command.AbilityStatus, BonusFormula, true);
                    else if (_v.Caster.HasSupportAbilityByIndex(SupportAbility.Boost))
                        TranceSeekAPI.AlterStatusDuration(_v, _v.Command.AbilityStatus, BonusFormula / 3, true);

                    return;
                }
                case BattleAbilityId.Fenrir1:
                case BattleAbilityId.Fenrir2:
                case (BattleAbilityId)1576:
                case (BattleAbilityId)1577:
                {
                    _v.Context.HitRate += ((ff9item.FF9Item_GetCount(RegularItem.Sapphire) + 1)) / 2;
                    break;
                }
            }
            if (_v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.DivineGuidance) && _v.Target.IsPlayer)
            {
                if (_v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.DivineGuidance_Boosted))
                {
                    _v.CalcHpMagicRecovery();
                    _v.Target.HpDamage /= (4 - BonusTurbo);
                }
            }
            else if (TranceSeekAPI.CanAttackMagic(_v))
            {
                int factor = _v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Boost_Boosted) ? 1 : (_v.Caster.HasSupportAbilityByIndex(SupportAbility.Boost) ? 2 : 6);
                _v.Context.AttackPower += _v.Caster.Level / factor;

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
