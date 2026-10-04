using Memoria.Data;
using Memoria.Prime;
using System;

namespace Memoria.Scripts.TranceSeek
{
    /// <summary>
    /// White Wind
    /// </summary>
    [BattleScript(Id)]
    public sealed class WhiteWindScript : IBattleScript
    {
        public const Int32 Id = 0030;

        private readonly BattleCalculator _v;

        public WhiteWindScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform() // [TODO] Maybe redoing the White Wind script, by using some TS dict var ?
        {
            if (_v.Command.HitRate == 111)
            {
                _v.Target.Flags |= CalcFlag.MpDamageOrHeal;
                _v.Target.MpDamage = (int)(_v.Caster.CurrentMp / 10);
            }
            else if (_v.Command.AbilityId == TranceSeekBattleAbility.Recover || _v.Command.Power == 99 && _v.Command.HitRate == 99) // Arnica
            {
                _v.Target.RemoveStatus(BattleStatusConst.AnyNegative);
                _v.Target.Flags |= CalcFlag.HpAlteration;
                if (!_v.Target.IsZombie)
                    _v.Target.Flags |= CalcFlag.HpRecovery;
                _v.Target.HpDamage = (int)_v.Target.MaximumHp;

            }
            else if (_v.Command.Power == 1)
            {
                if (_v.Target.IsZombie)
                    _v.Target.Flags |= CalcFlag.HpAlteration;
                else
                    _v.Target.Flags |= CalcFlag.HpDamageOrHeal;

                _v.Target.HpDamage = (int)_v.Caster.CurrentHp;
            }
            else
            {
                var CasterState = _v.Caster.State();
                if (CasterState.CasterHP_WhiteWind == 0)
                    CasterState.CasterHP_WhiteWind = _v.Caster.CurrentHp;

                if (_v.Target.IsZombie)
                    _v.Target.Flags |= CalcFlag.HpAlteration;
                else
                    _v.Target.Flags |= CalcFlag.HpDamageOrHeal;

                _v.Target.HpDamage = (int)CasterState.CasterHP_WhiteWind;
            }
        }
    }
}


