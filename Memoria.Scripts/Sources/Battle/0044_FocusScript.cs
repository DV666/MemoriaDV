using System;
using System.Collections.Generic;
using Memoria.Data;
using Memoria.Prime;

namespace Memoria.Scripts.TranceSeek
{
    /// <summary>
    /// Focus
    /// </summary>
    [BattleScript(Id)]
    public sealed class FocusScript : IBattleScript
    {
        public const Int32 Id = 0044;

        private readonly BattleCalculator _v;

        public FocusScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Caster.PlayerIndex == CharacterId.Vivi)
            {
                if (_v.Command.Id == TranceSeekBattleCommand.Manawell) // AA Mana Well
                {
                    _v.Target.Flags |= (CalcFlag.MpAlteration | CalcFlag.MpRecovery);
                    short mpDamage = (short)(_v.Target.MaximumMp / 2U);
                    if (_v.Caster.CurrentMp == _v.Caster.MaximumMp)
                    {
                        _v.Context.Flags |= BattleCalcFlags.Miss;
                    }
                    else
                    {
                        _v.Target.MpDamage = mpDamage;
                    }
                }
                else if (_v.Command.Id == (BattleCommandId)1056) // Transcendent
                {
                    _v.Target.Flags |= CalcFlag.MpDamageOrHeal;

                    uint factor = 4;
                    if (_v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Overload_Boosted)) // SA Overload+
                        factor = 2;
                    else if (_v.Caster.HasSupportAbilityByIndex(SupportAbility.MagElemNull)) // SA Overload
                        factor = 3;

                    uint TranceFactor = 255 / (factor * 3);
                    uint MPFactor = _v.Caster.MaximumMp / factor;
                    int RemainingTrance = (int)(_v.Caster.Trance - TranceFactor);

                    if (_v.Caster.Trance == 0)
                    {
                        _v.Context.Flags |= BattleCalcFlags.Miss;
                    }
                    else
                    {
                        if (RemainingTrance <= 0)
                        {
                            _v.Caster.MpDamage = (int)(MPFactor * _v.Caster.Trance / TranceFactor);
                            _v.Caster.Trance = 0;
                        }
                        else
                        {
                            _v.Caster.Trance -= (byte)TranceFactor;
                            _v.Caster.MpDamage = (int)MPFactor;
                        }
                    }
                }
                else if (_v.Command.Id == BattleCommandId.Accumulate)
                {
                    _v.Caster.Flags |= (CalcFlag.HpAlteration | CalcFlag.MpDamageOrHeal);

                    uint factor = 4;
                    if (_v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Overload_Boosted)) // SA Overload+
                        factor = 2;
                    else if (_v.Caster.HasSupportAbilityByIndex(SupportAbility.MagElemNull)) // SA Overload
                        factor = 3;

                    uint HPFactor = _v.Caster.MaximumHp / factor;
                    uint MPFactor = _v.Caster.MaximumMp / factor;
                    int RemainingHP = (int)(_v.Caster.CurrentHp - HPFactor);

                    if (_v.Caster.CurrentHp == 1)
                    {
                        _v.Context.Flags |= BattleCalcFlags.Miss;
                    }
                    else
                    {
                        if (RemainingHP <= 0)
                        {
                            _v.Caster.HpDamage = (int)(_v.Caster.CurrentHp - 1);
                            _v.Caster.MpDamage = (int)(MPFactor * _v.Caster.CurrentHp / HPFactor);
                        }
                        else
                        {
                            _v.Caster.HpDamage = (int)HPFactor;
                            _v.Caster.MpDamage = (int)MPFactor;
                        }
                    }
                }
                else if (_v.Command.Id == TranceSeekBattleCommand.Absorb || _v.Command.Id == TranceSeekBattleCommand.Absorb2) // SA Absorb
                {
                    _v.Target.Flags |= CalcFlag.HpAlteration;
                    _v.Caster.Flags |= CalcFlag.MpDamageOrHeal;

                    uint HPAbsorbed = (uint)(_v.Target.MaximumHp / _v.Command.Power);
                    uint MPRestored = (uint)(_v.Target.MaximumMp / _v.Command.Power);
                    uint RemainingHP = _v.Target.CurrentHp - HPAbsorbed;

                    if (RemainingHP <= 0)
                    {
                        _v.Target.HpDamage = (int)(_v.Target.CurrentHp);
                        _v.Caster.MpDamage = (int)(MPRestored * _v.Target.CurrentHp / HPAbsorbed);
                    }
                    else
                    {
                        _v.Target.HpDamage = (int)HPAbsorbed;
                        _v.Caster.MpDamage = (int)MPRestored;
                    }
                }
            }
            else
            {
                btl_stat.AlterStatus(_v.Caster, TranceSeekStatusId.MagicUp, parameters: $"+{_v.Command.Power}");
            }
        }
    }
}


