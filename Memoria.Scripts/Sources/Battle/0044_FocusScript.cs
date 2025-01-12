﻿using System;
using System.Collections.Generic;
using Memoria.Data;
using Memoria.Prime;

namespace Memoria.Scripts.Battle
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
                if (_v.Caster.IsUnderStatus(BattleStatus.Trance)) // AA Mana Well
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
                else
                {
                    if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)204)) // SA Transcendent
                        _v.Target.Flags |= CalcFlag.MpDamageOrHeal;
                    else
                        _v.Target.Flags |= (CalcFlag.HpAlteration | CalcFlag.MpDamageOrHeal);

                    uint num;
                    uint num2;
                    uint factor = 4;
                    if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1032)) // SA Overload+
                        factor = 2;
                    else if (_v.Caster.HasSupportAbility(SupportAbility2.MagElemNull)) // SA Overload
                        factor = 3;

                    if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)204))
                    {
                        num = 255 / factor;
                        num2 = _v.Target.MaximumMp / factor;
                        int num3 = (int)(_v.Target.Trance - num);

                        if (_v.Caster.Trance == 0)
                        {
                            _v.Context.Flags |= BattleCalcFlags.Miss;
                        }
                        else
                        {
                            if (num3 <= 0)
                            {
                                _v.Target.Trance = 0;
                                _v.Target.MpDamage = (int)(num2 * _v.Target.Trance / num);
                            }
                            else
                            {
                                _v.Target.Trance -= (byte)num;
                                _v.Target.MpDamage = (int)num2;
                            }
                        }
                    }
                    else
                    {
                        num = _v.Target.MaximumHp / factor;
                        num2 = _v.Target.MaximumMp / factor;
                        uint num3 = _v.Target.CurrentHp - num;

                        if (_v.Caster.CurrentHp == 1U)
                        {
                            _v.Context.Flags |= BattleCalcFlags.Miss;
                        }
                        else
                        {
                            if (num3 <= 0)
                            {
                                _v.Target.HpDamage = (int)(_v.Target.CurrentHp - 1U);
                                _v.Target.MpDamage = (int)(num2 * _v.Target.CurrentHp / num);
                            }
                            else
                            {
                                _v.Target.HpDamage = (int)num;
                                _v.Target.MpDamage = (int)num2;
                            }
                        }
                    }
                }
            }
            else
            {
                _v.Target.AlterStatus(TranceSeekCustomAPI.CustomStatus.MagicUp, _v.Caster);
            }
        }
    }
}
