using Memoria.Data;
using System;
using static Memoria.Scripts.Battle.TranceSeekCustomAPI;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Regen, Shell, Protect, Haste, Reflect, Float, Carbuncle, Mighty Guard, Vanish, Auto-Life, Reis’s Wind, Luna, Aura, Defend
    /// </summary>
    [BattleScript(Id)]
    public sealed class SpecialStatusScript : IBattleScript
    {
        public const Int32 Id = 0126;

        private readonly BattleCalculator _v;

        public SpecialStatusScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Caster.Data.dms_geo_id == 410 && _v.Command.Power == 10 && _v.Command.HitRate == 10) // Runic Lamie
            {
                _v.Caster.Data.mot[0] = "ANH_MON_B3_122_BTL_DEFENDIDLE";
                _v.Caster.Data.mot[2] = "ANH_MON_B3_122_BTL_DEFENDIDLE";
                btl_stat.MakeStatusesPermanent(_v.Caster, CustomStatus.Runic, true);
                btl_stat.MakeStatusesPermanent(_v.Caster, BattleStatus.Defend, true);
            }

            //TranceSeekCustomAPI.TryAlterCommandStatuses(_v);
        }
    }
}
