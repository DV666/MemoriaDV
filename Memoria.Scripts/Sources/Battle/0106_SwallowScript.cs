using System;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Swallow, Snort
    /// </summary>
    [BattleScript(Id)]
    public sealed class SwallowScript : IBattleScript
    {
        public const Int32 Id = 0106;

        private readonly BattleCalculator _v;

        public SwallowScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            if (_v.Command.HitRate == 111)
            {
                _v.TryAlterMagicStatuses();
                _v.Target.Remove(false);
            }
            else
            {
                _v.Target.Remove();
                if (_v.Command.HitRate == 255)
                {
                    _v.Caster.Flags |= (CalcFlag.HpAlteration | CalcFlag.HpRecovery | CalcFlag.MpAlteration | CalcFlag.MpRecovery);
                    _v.Caster.HpDamage = (int)(_v.Target.MaximumHp * _v.Command.Power / 100U);
                    _v.Caster.MpDamage = (int)(_v.Target.MaximumMp * _v.Command.Power / 100U);
                }
            }
            TranceSeekCustomAPI.SpecialSA(_v);
        }
    }
}
