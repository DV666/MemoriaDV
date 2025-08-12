using Memoria.Data;
using System;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Pumpkin Head, Minus Strike, Chestnut
    /// </summary>
    [BattleScript(Id)]
    public sealed class DifferentCasterHpAttackScript : IBattleScript
    {
        public const Int32 Id = 0029;

        private readonly BattleCalculator _v;

        public DifferentCasterHpAttackScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Command.AbilityId == (BattleAbilityId)1105) // Blood Pact
            {
                _v.Caster.Flags |= CalcFlag.HpAlteration;
                _v.Caster.HpDamage = (Int32)(_v.Caster.CurrentHp);
                int TranceValue = (int)(_v.Caster.Trance + ((_v.Caster.CurrentHp * 255) / _v.Caster.MaximumHp) / 2);
                TranceSeekAPI.IncreaseTrance(_v.Caster.Data, TranceValue, 254);
                return;
            }

            _v.Target.Flags |= CalcFlag.HpAlteration;
            _v.Target.HpDamage = (Int32)(_v.Caster.MaximumHp - _v.Caster.CurrentHp);
        }
    }
}
