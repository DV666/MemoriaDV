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
                TranceSeekCustomAPI.CharacterBonusPassive(_v, "MagicAttack");
                _v.Caster.EnemyTranceBonusAttack();
                _v.Caster.PenaltyMini();
                TranceSeekCustomAPI.PenaltyShellAttack(_v);
                TranceSeekCustomAPI.BonusElement(_v);

                if (_v.CanAttackElementalCommand())
                    _v.CalcHpDamage();
            }
        }
    }
}
