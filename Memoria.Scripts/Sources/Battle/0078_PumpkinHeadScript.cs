using Memoria.Data;
using System;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Dark Matter
    /// </summary>
    [BattleScript(Id)]
    public sealed class PumpkinHeadScript : IBattleScript
    {
        public const Int32 Id = 0078;

        private readonly BattleCalculator _v;

        public PumpkinHeadScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            uint num = Math.Min(((_v.Caster.MaximumHp - _v.Caster.CurrentHp) / 33), 100);
            _v.NormalMagicParams();
            _v.Context.AttackPower = (int)(_v.Command.Power + num);
            TranceSeekAPI.CharacterBonusPassive(_v, "MagicAttack");
            TranceSeekAPI.CasterPenaltyMini(_v);
            TranceSeekAPI.PenaltyShellAttack(_v);
            TranceSeekAPI.PenaltyCommandDividedAttack(_v);
            TranceSeekAPI.BonusElement(_v);
            if (TranceSeekAPI.CanAttackMagic(_v))
            {
                _v.Target.Flags = CalcFlag.HpAlteration;
                if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)102))
                    TranceSeekAPI.TryCriticalHit(_v);
                _v.CalcHpDamage();
            }
            TranceSeekAPI.TryAlterMagicStatuses(_v);
        }
    }
}
