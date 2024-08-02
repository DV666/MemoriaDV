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
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            if (_v.Caster.Data.dms_geo_id == 446) // Garland - Armor Mechanic
            {
                if (_v.Command.Power == 100 && _v.Command.HitRate == 100)
                {
                    TranceSeekCustomAPI.MonsterMechanic[_v.Caster.Data][1] = 10;
                    _v.Target.Data.mot[2] = "ANH_MON_B3_185_000";
                    _v.Target.Flags |= CalcFlag.HpDamageOrHeal;
                    _v.Target.HpDamage = 5000;
                    _v.Target.TryAlterSingleStatus(TranceSeekCustomAPI.CustomStatusId.MechanicalArmor, true, _v.Caster, TranceSeekCustomAPI.MonsterMechanic[_v.Caster.Data][1]);
                }
                else if (_v.Command.Power == 111 && _v.Command.HitRate == 111)
                {
                    _v.Caster.Data.mot[0] = "ANH_MON_B3_185_011";
                    _v.Caster.Data.mot[1] = "ANH_MON_B3_185_000";
                    _v.Caster.Data.mot[2] = "ANH_MON_B3_185_011";
                    TranceSeekCustomAPI.MonsterMechanic[_v.Caster.Data][2] = 1;
                    return;
                }
                else if (_v.Command.Power == 200 && _v.Command.HitRate == 200)
                {
                    TranceSeekCustomAPI.MonsterMechanic[_v.Caster.Data][1] = 20;
                    _v.Target.Data.mot[2] = "ANH_MON_B3_185_000";
                    _v.Target.Flags |= CalcFlag.HpDamageOrHeal;
                    _v.Target.HpDamage = 9999;
                    _v.Target.TryAlterSingleStatus(TranceSeekCustomAPI.CustomStatusId.MechanicalArmor, true, _v.Caster, TranceSeekCustomAPI.MonsterMechanic[_v.Caster.Data][1]);
                }
                _v.Target.TryRemoveStatuses(_v.Command.AbilityStatus);
            }
            TranceSeekCustomAPI.SpecialSA(_v);
        }
    }
}
