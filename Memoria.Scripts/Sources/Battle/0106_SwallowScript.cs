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
            if (_v.Command.HitRate == 111)
            {
                TranceSeekAPI.TryAlterMagicStatuses(_v);
                _v.Target.Remove(false);
            }
            else
            {               
                if (_v.Command.HitRate == 255)
                {
                    _v.Caster.Flags |= (CalcFlag.HpDamageOrHeal | CalcFlag.MpDamageOrHeal);
                    _v.Caster.HpDamage = (int)((_v.Target.MaximumHp * _v.Command.Power) / 100U);
                    _v.Caster.MpDamage = (int)((_v.Target.MaximumMp * _v.Command.Power) / 100U);
                    _v.Caster.AddDelayedModifier(
                        caster => caster.CurrentAtb >= caster.MaximumAtb,
                        caster =>
                        {
                            btl2d.Btl2dStatReq(caster, -_v.Caster.HpDamage, -_v.Caster.MpDamage);
                        }
                    );
                }
                _v.Target.Remove();
            }
        }
    }
}
