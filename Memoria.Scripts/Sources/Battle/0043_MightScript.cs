using Memoria.Data;
using System;
using static Memoria.Scripts.Battle.TranceSeekCustomAPI;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Might
    /// </summary>
    [BattleScript(Id)]
    public sealed class MightScript : IBattleScript
    {
        public const Int32 Id = 0043;

        private readonly BattleCalculator _v;

        public MightScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Caster.PlayerIndex == CharacterId.Beatrix && (_v.Command.AbilityId == (BattleAbilityId)1012 || _v.Command.AbilityId == (BattleAbilityId)1054)) // Bravoure
            {
                if (BeatrixPassive[_v.Caster.Data][2] > 0)
                {
                    _v.Context.Flags |= BattleCalcFlags.Miss;
                    return;
                }
                _v.Target.AlterStatus(CustomStatus.PowerUp, _v.Caster);
                _v.Target.AlterStatus(CustomStatus.MagicUp, _v.Caster);
                _v.Target.AlterStatus(CustomStatus.Redemption, _v.Caster);
                _v.Target.AlterStatus(CustomStatus.Redemption, _v.Caster);
                if (_v.Caster.IsUnderAnyStatus(BattleStatus.Trance))
                {
                    BeatrixPassive[_v.Caster.Data][2] = 2;
                    _v.Caster.AlterStatus(BattleStatus.Regen);
                    _v.Caster.AlterStatus(BattleStatus.AutoLife);
                }
                else
                {
                    BeatrixPassive[_v.Caster.Data][2] = 1;
                }
                return;
            }
            else if (_v.Caster.Data.dms_geo_id == 410 && _v.Command.Power == 2) // [Lani] Adrénaline
            {
                btl_stat.AlterStatus(_v.Target, CustomStatusId.PowerUp, parameters: "+2");
                btl_stat.AlterStatus(_v.Target, CustomStatusId.ArmorUp, parameters: "+2");
                return;
            }
            else if (_v.Caster.Data.dms_geo_id == 410 && _v.Command.Power == 4 || _v.Command.AbilityId == (BattleAbilityId)1081) // [Lani] Super Muscles
            {
                btl_stat.AlterStatus(_v.Target, CustomStatusId.PowerUp, parameters: "+4");
                btl_stat.AlterStatus(_v.Target, CustomStatusId.ArmorUp, parameters: "+4");
                return;
            }

            _v.TryAlterMagicStatuses();
            _v.Target.AlterStatus(CustomStatus.PowerUp, _v.Caster);
            _v.Target.AlterStatus(CustomStatus.MagicUp, _v.Caster);
        }
    }
}
