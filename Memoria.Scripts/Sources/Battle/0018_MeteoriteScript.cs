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
            _v.Context.Attack = GameRandom.Next16() % (_v.Caster.Magic + _v.Caster.Level);
            _v.SetCommandPower();
            _v.Context.DefensePower = 0;
            TranceSeekAPI.CasterPenaltyMini(_v);
            TranceSeekAPI.EnemyTranceBonusAttack(_v);
            TranceSeekAPI.PenaltyShellAttack(_v);
            TranceSeekAPI.PenaltyCommandDividedAttack(_v);
            TranceSeekAPI.BonusElement(_v);
            if (TranceSeekAPI.CanAttackMagic(_v))
            {
                _v.CalcHpDamage();
            }
            TranceSeekAPI.TryAlterMagicStatuses(_v);
        }
    }
}
