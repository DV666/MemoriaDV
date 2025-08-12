using System;
using FF9;
using Memoria.Data;
using UnityEngine;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Special
    /// </summary>
    [BattleScript(Id)]
    public sealed class MechanicalArmorScript : IBattleScript
    {
        public const Int32 Id = 0122;

        private readonly BattleCalculator _v;

        public MechanicalArmorScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Caster.Data.dms_geo_id == 446) // Garland - Armor Mechanic
            {
                if (_v.Command.Power == 100 && _v.Command.HitRate == 100)
                {
                    TranceSeekAPI.MonsterMechanic[_v.Caster.Data][1] = 10;
                    _v.Target.Data.mot[2] = "ANH_MON_B3_185_000";
                    _v.Target.Flags |= CalcFlag.HpDamageOrHeal;
                    _v.Target.HpDamage = 5000;
                    _v.Target.TryAlterSingleStatus(TranceSeekStatusId.MechanicalArmor, true, _v.Caster, TranceSeekAPI.MonsterMechanic[_v.Caster.Data][1]);
                }
                else if (_v.Command.Power == 200 && _v.Command.HitRate == 200)
                {
                    TranceSeekAPI.MonsterMechanic[_v.Caster.Data][1] = 20;
                    _v.Target.Data.mot[2] = "ANH_MON_B3_185_000";
                    _v.Target.Flags |= CalcFlag.HpDamageOrHeal;
                    _v.Target.HpDamage = 9999;
                    _v.Target.PhysicalEvade = 0;
                    _v.Target.TryAlterSingleStatus(TranceSeekStatusId.MechanicalArmor, true, _v.Caster, TranceSeekAPI.MonsterMechanic[_v.Caster.Data][1]);
                }
                _v.Target.TryRemoveStatuses(_v.Command.AbilityStatus);
            }
        }
    }
}
