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
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            if (_v.Caster.Data.dms_geo_id == 446 && _v.Command.HitRate == 111) // Garland - Meteor
            {
                _v.Caster.Data.mot[0] = "ANH_MON_B3_185_008";
                _v.Caster.Data.mot[1] = "ANH_MON_B3_185_000";
                _v.Caster.Data.mot[2] = "ANH_MON_B3_185_000";
                _v.Caster.PhysicalEvade = 13;
                _v.Context.Flags |= BattleCalcFlags.Miss;
                return;
            }
            if (_v.IsTargetLevelMultipleOfCommandRate() && _v.Target.CanBeAttacked())
            {
                _v.NormalMagicParams();
                TranceSeekCustomAPI.CharacterBonusPassive(_v, "MagicAttack");
                _v.Caster.EnemyTranceBonusAttack();
                _v.Caster.PenaltyMini();
                TranceSeekCustomAPI.PenaltyShellAttack(_v);
                _v.BonusElement();

                if (_v.CanAttackElementalCommand())
                    _v.CalcHpDamage();
            }
            TranceSeekCustomAPI.SpecialSA(_v);
        }
    }
}
