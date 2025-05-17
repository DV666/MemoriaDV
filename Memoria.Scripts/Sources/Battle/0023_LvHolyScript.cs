using System;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// LV4 Holy
    /// </summary>
    [BattleScript(Id)]
    public sealed class LvHolyScript : IBattleScript
    {
        public const Int32 Id = 0023;

        private readonly BattleCalculator _v;

        public LvHolyScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.IsTargetLevelMultipleOfCommandRate() && _v.Target.CanBeAttacked())
            {
                _v.NormalMagicParams();
                TranceSeekAPI.CharacterBonusPassive(_v, "MagicAttack");
                TranceSeekAPI.EnemyTranceBonusAttack(_v);
                TranceSeekAPI.CasterPenaltyMini(_v);
                TranceSeekAPI.PenaltyShellAttack(_v);
                TranceSeekAPI.BonusElement(_v);

                if (_v.CanAttackElementalCommand())
                    _v.CalcHpDamage();
            }
        }
    }
}
