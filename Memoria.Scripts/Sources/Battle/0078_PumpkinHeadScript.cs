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
            uint num = ((_v.Caster.MaximumHp - _v.Caster.CurrentHp) / 33);
            _v.NormalMagicParams();
            _v.Context.AttackPower = (int)(_v.Command.Power + num);
            TranceSeekCustomAPI.CharacterBonusPassive(_v, "MagicAttack");
            _v.Caster.PenaltyMini();
            TranceSeekCustomAPI.PenaltyShellAttack(_v);
            TranceSeekCustomAPI.PenaltyCommandDividedAttack(_v);
            TranceSeekCustomAPI.BonusElement(_v);
            if (TranceSeekCustomAPI.CanAttackMagic(_v))
            {
                _v.Target.Flags = CalcFlag.HpAlteration;
                if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)102))
                    TranceSeekCustomAPI.TryCriticalHit(_v);
                _v.CalcHpDamage();
            }
            _v.TryAlterMagicStatuses();
        }
    }
}
