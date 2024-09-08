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
            if (TranceSeekCustomAPI.CanAttackMagic(_v))
            {
                switch (_v.Command.AbilityId)
                {
                    case BattleAbilityId.Shiva:
                    case BattleAbilityId.DiamondDust:
                    {
                            _v.Context.AttackPower += _v.Caster.Level;
                            if ((ff9item.FF9Item_GetCount(RegularItem.Opal)) > Comn.random16() % 100)
                            {
                                _v.Target.AlterStatus(BattleStatus.Freeze, _v.Caster);
                            }
                            break;
                        }
                    case BattleAbilityId.Ifrit:
                    case BattleAbilityId.FlamesofHell:
                        {
                            _v.Context.AttackPower += _v.Caster.Level;
                            if ((ff9item.FF9Item_GetCount(RegularItem.Topaz)) > Comn.random16() % 100)
                            {
                                _v.Target.AlterStatus(BattleStatus.Heat, _v.Caster);
                            }
                            break;
                        }
                    case BattleAbilityId.Ramuh:
                    case BattleAbilityId.JudgementBolt:
                        {
                            _v.Context.AttackPower += _v.Caster.Level;
                            if ((ff9item.FF9Item_GetCount(RegularItem.Peridot)) > Comn.random16() % 100)
                            {
                                _v.Target.AlterStatus(BattleStatus.Slow, _v.Caster);
                            }
                            break;
                        }
                    case BattleAbilityId.Leviathan:
                    case BattleAbilityId.Tsunami:
                        {
                            _v.Context.AttackPower += _v.Caster.Level;
                            if ((ff9item.FF9Item_GetCount(RegularItem.Aquamarine)) > Comn.random16() % 100)
                            {
                                _v.Target.AlterStatus(BattleStatus.Silence);
                            }
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
                            if ((ff9item.FF9Item_GetCount(RegularItem.LapisLazuli)) > Comn.random16() % 100)
                            {
                                _v.Target.AlterStatus(BattleStatus.Death, _v.Caster);
                            }
                            break;
                        }
                    case BattleAbilityId.Carbuncle1:
                    case BattleAbilityId.Carbuncle2:
                    case BattleAbilityId.Carbuncle3:
                    case BattleAbilityId.Carbuncle4:
                        {  
                            byte will = _v.Target.Will;
                            if (ff9item.FF9Item_GetCount(RegularItem.Ruby) > 0)
                            {
                                _v.Target.Will = (byte)(_v.Target.Will + _v.Target.Will * ff9item.FF9Item_GetCount(RegularItem.Ruby) / 100L);
                            }
                            if (!_v.Command.IsShortSummon && _v.Command.Id == BattleCommandId.SummonEiko)
                            {
                                _v.Target.Will = (byte)(_v.Target.Will * 2);
                            }
                            TranceSeekCustomAPI.TryAlterCommandStatuses(_v);
                            _v.Target.Will = will;
                            return;
                        }
                    case BattleAbilityId.Fenrir1:
                        {
                            _v.Context.AttackPower += _v.Caster.Level;
                            if ((ff9item.FF9Item_GetCount(RegularItem.Sapphire)) > Comn.random16() % 100)
                            {
                                _v.Target.AlterStatus(BattleStatus.Confuse, _v.Caster);
                            }
                            break;
                        }
                    case BattleAbilityId.Fenrir2:
                        {
                            _v.Context.AttackPower += _v.Caster.Level;
                            if ((ff9item.FF9Item_GetCount(RegularItem.Sapphire)) > Comn.random16() % 100)
                            {
                                _v.Target.AlterStatus(BattleStatus.Float, _v.Caster);
                            }
                            break;
                        }
                    case BattleAbilityId.Madeen:
                        _v.Context.AttackPower += _v.Caster.Level;
                        break;
                }
                _v.CalcHpDamage();
            }
        }
    }
}
