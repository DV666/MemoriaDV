using System;
using System.Collections.Generic;
using Memoria.Data;

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
                if (_v.Caster.IsUnderStatus(BattleStatus.Trance))
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
                    _v.Target.Flags |= (CalcFlag.HpAlteration | CalcFlag.MpAlteration | CalcFlag.MpRecovery); ;
                    short num;
                    short num2;
                    if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1032))
                    {
                        num = (short)(_v.Target.MaximumHp / 2);
                        num2 = (short)(_v.Target.MaximumMp / 2);
                    }
                    else if (_v.Caster.HasSupportAbility(SupportAbility2.MagElemNull))
                    {
                        num = (short)(_v.Target.MaximumHp / 3);
                        num2 = (short)(_v.Target.MaximumMp / 3);
                    }
                    else
                    {
                        num = (short)(_v.Target.MaximumHp / 4);
                        num2 = (short)(_v.Target.MaximumMp / 4);
                    }
                    short num3 = (short)(_v.Target.CurrentHp - (uint)((ushort)num));
                    if (_v.Caster.CurrentHp == 1U)
                    {
                        _v.Context.Flags |= BattleCalcFlags.Miss;
                    }
                    else
                    {
                        if (num3 <= 0)
                        {
                            _v.Target.HpDamage = ((short)(_v.Target.CurrentHp - 1U));
                            _v.Target.MpDamage = (num2 * (short)_v.Target.CurrentHp / num);
                        }
                        else
                        {
                            _v.Target.HpDamage = num;
                            _v.Target.MpDamage = num2;
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
