using System;
using System.Runtime.Remoting.Contexts;
using FF9;
using Memoria.Data;
using Memoria.Prime;

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
            if (TranceSeekCustomAPI.CanAttackMagic(_v))
            {
                switch (_v.Command.AbilityId)
                {
                    case BattleAbilityId.Shiva:
                    case BattleAbilityId.DiamondDust:
                    {
                        _v.Context.AttackPower += _v.Caster.Level;
                        _v.Context.HitRate += ((ff9item.FF9Item_GetCount(RegularItem.Opal) + 1)) / 2;
                        break;
                    }
                    case BattleAbilityId.Ifrit:
                    case BattleAbilityId.FlamesofHell:
                    {
                        _v.Context.AttackPower += _v.Caster.Level;
                        _v.Context.HitRate += ((ff9item.FF9Item_GetCount(RegularItem.Topaz) + 1)) / 2;
                        break;
                    }
                    case BattleAbilityId.Ramuh:
                    case BattleAbilityId.JudgementBolt:
                    {
                        _v.Context.AttackPower += _v.Caster.Level;
                        _v.Context.HitRate += ((ff9item.FF9Item_GetCount(RegularItem.Peridot) + 1)) / 2;
                        break;
                    }
                    case BattleAbilityId.Leviathan:
                    case BattleAbilityId.Tsunami:
                    {
                        _v.Context.AttackPower += _v.Caster.Level;
                        _v.Context.HitRate += ((ff9item.FF9Item_GetCount(RegularItem.Aquamarine) + 1)) / 2;
                        break;
                    }
                    case BattleAbilityId.Bahamut:
                    case BattleAbilityId.MegaFlare:
                    {
                        _v.Context.AttackPower += _v.Caster.Level;
                        if ((ff9item.FF9Item_GetCount(RegularItem.Garnet)) > Comn.random16() % 100)
                        {
                            _v.Target.RemoveStatus(BattleStatus.Protect);
                            _v.Target.RemoveStatus(BattleStatus.Shell);
                            _v.Target.RemoveStatus(BattleStatus.Reflect);
                        }
                        break;
                    }
                    case BattleAbilityId.Ark:
                    case BattleAbilityId.EternalDarkness:
                    {
                        _v.Context.AttackPower += _v.Caster.Level;
                        _v.Context.HitRate = ((ff9item.FF9Item_GetCount(RegularItem.LapisLazuli) + 1)) / 2;
                        break;
                    }
                    case BattleAbilityId.Carbuncle1:
                    case BattleAbilityId.Carbuncle2:
                    case BattleAbilityId.Carbuncle3:
                    case BattleAbilityId.Carbuncle4:
                    {  
                        byte will = _v.Caster.Will; // TODO - To improve ?
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
                _v.CalcHpDamage();

                // TODO - Create a new function for that ? Make it for MagicScript like for example ?                
                _v.Target.PenaltyShellHitRate();
                foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(_v.Caster))
                    saFeature.TriggerOnAbility(_v, "HitRateSetup", false);
                foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(_v.Target))
                    saFeature.TriggerOnAbility(_v, "HitRateSetup", true);

                if (_v.Context.HitRate > Comn.random16() % 100 && _v.Context.Evade <= Comn.random16() % 100)
                    _v.Target.TryAlterStatuses(_v.Command.AbilityStatus, true, _v.Caster);
            }
        }
    }
}
