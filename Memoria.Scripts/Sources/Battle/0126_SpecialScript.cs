using Memoria.Data;
using System;
using static Memoria.Scripts.Battle.TranceSeekAPI;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Regen, Shell, Protect, Haste, Reflect, Float, Carbuncle, Mighty Guard, Vanish, Auto-Life, Reis’s Wind, Luna, Aura, Defend
    /// </summary>
    [BattleScript(Id)]
    public sealed class SpecialScript : IBattleScript
    {
        public const Int32 Id = 0126;

        private readonly BattleCalculator _v;

        public SpecialScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Command.AbilityId == (BattleAbilityId)1102) // Sang Maudit
            {
                btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.Special, _v.Caster, true, "CursedBlood");
            }
            else if (_v.Caster.Data.dms_geo_id == 349 && _v.Command.Power == 10 && _v.Command.HitRate == 10) // Gisamark - Plongée
            {
                foreach (BattleStatusId statusid in (_v.Caster.Data.stat.cur).ToStatusList())
                {
                    if (statusid != BattleStatusId.EasyKill)
                        _v.Caster.RemoveStatus(statusid);
                }              
                return;
            }
            else if (_v.Caster.Data.dms_geo_id == 446 && _v.Command.Power == 111 && _v.Command.HitRate == 111) // Garland - Meteor Cast
            {
                _v.Caster.Data.mot[0] = "ANH_MON_B3_185_011";
                _v.Caster.Data.mot[1] = "ANH_MON_B3_185_000";
                _v.Caster.Data.mot[2] = "ANH_MON_B3_185_011";
                MonsterMechanic[_v.Caster.Data][2] = 1;
                return;
            }
            else if (_v.Caster.Data.dms_geo_id == 410 && _v.Command.Power == 10 && _v.Command.HitRate == 10) // Runic Lamie
            {
                _v.Caster.Data.mot[0] = "ANH_MON_B3_122_BTL_DEFENDIDLE";
                _v.Caster.Data.mot[2] = "ANH_MON_B3_122_BTL_DEFENDIDLE";
                btl_stat.MakeStatusesPermanent(_v.Caster, TranceSeekStatus.Runic, true);
                btl_stat.MakeStatusesPermanent(_v.Caster, BattleStatus.Defend, true);
            }

            //TranceSeekCustomAPI.TryAlterCommandStatuses(_v);
        }
    }
}
