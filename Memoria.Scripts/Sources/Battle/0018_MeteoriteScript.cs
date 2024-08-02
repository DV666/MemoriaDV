using System;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Comet, Meteor, Twister, Meteorite, Meteorite Counter
    /// </summary>
    [BattleScript(Id)]
    public sealed class MeteoriteScript : IBattleScript
    {
        public const Int32 Id = 0018;

        private readonly BattleCalculator _v;

        public MeteoriteScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            InitializeAttackParams();
            _v.Caster.PenaltyMini();
            _v.Caster.EnemyTranceBonusAttack();
            TranceSeekCustomAPI.PenaltyShellAttack(_v);
            _v.PenaltyCommandDividedAttack();
            _v.BonusElement();
            if (TranceSeekCustomAPI.CanAttackMagic(_v))
            {
                _v.CalcHpDamage();
            }
            _v.TryAlterMagicStatuses();
            TranceSeekCustomAPI.SpecialSA(_v);
        }

        private void InitializeAttackParams()
        {
            _v.Context.Attack = GameRandom.Next16() % (_v.Caster.Magic + _v.Caster.Level);
            _v.SetCommandPower();
            _v.Context.DefensePower = 0;
        }
    }
}
