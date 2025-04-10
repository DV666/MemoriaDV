using System;
using System.Runtime.Remoting.Contexts;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Lancer
    /// </summary>
    [BattleScript(Id)]
    public sealed class LowRandomMagic : IBattleScript
    {
        public const Int32 Id = 0116;

        private readonly BattleCalculator _v;

        public LowRandomMagic(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Caster.Data.dms_geo_id == 446 && TranceSeekCustomAPI.MonsterMechanic[_v.Caster.Data][2] == 1) // Garland - Final Meteor
            {            
                _v.Caster.Data.mot[0] = "ANH_MON_B3_185_008";
                _v.Caster.Data.mot[1] = "ANH_MON_B3_185_000";
                _v.Caster.Data.mot[2] = "ANH_MON_B3_185_000";
                _v.Caster.PhysicalEvade = 13;
                if (_v.Command.Power == 0 && _v.Command.HitRate == 111)
                {
                    _v.Context.Flags = BattleCalcFlags.Miss;
                    return;
                }
            }
            _v.Context.Attack = UnityEngine.Random.Range(((_v.Caster.Magic + _v.Caster.Level) / 3), (_v.Caster.Magic + _v.Caster.Level));
            _v.SetCommandPower();
            _v.Target.SetMagicDefense();
            TranceSeekCustomAPI.CasterPenaltyMini(_v);
            TranceSeekCustomAPI.EnemyTranceBonusAttack(_v);
            TranceSeekCustomAPI.PenaltyShellAttack(_v);
            TranceSeekCustomAPI.PenaltyCommandDividedAttack(_v);
            TranceSeekCustomAPI.BonusElement(_v);
            if (TranceSeekCustomAPI.CanAttackMagic(_v))
            {
                _v.CalcHpDamage();
            }
            TranceSeekCustomAPI.TryAlterMagicStatuses(_v);
        }
    }
}
