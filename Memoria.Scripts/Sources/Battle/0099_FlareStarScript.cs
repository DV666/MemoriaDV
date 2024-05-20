using System;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Flare Star
    /// </summary>
    [BattleScript(Id)]
    public sealed class FlareStarScript : IBattleScript
    {
        public const Int32 Id = 0099;

        private readonly BattleCalculator _v;

        public FlareStarScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            _v.MagicAccuracy();
            _v.Caster.EnemyTranceBonusAttack();
            _v.Target.PenaltyShellHitRate();
            _v.PenaltyCommandDividedHitRate();
            if (_v.TryMagicHit())
            {
                _v.Target.Flags |= CalcFlag.HpAlteration;
                _v.Target.HpDamage = _v.Target.Level * _v.Command.Power;
            }
            TranceSeekCustomAPI.SpecialSA(_v);
        }
    }
}