using Memoria.Data;
using System;
using System.Collections.Generic;

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
                if (TranceSeekCustomAPI.BeatrixPassive[_v.Caster.Data][2] > 0)
                {
                    _v.Context.Flags |= BattleCalcFlags.Miss;
                    return;
                }

                if (_v.Caster.IsUnderAnyStatus(BattleStatus.Trance))
                {
                    TranceSeekCustomAPI.BeatrixPassive[_v.Caster.Data][2] = 2;
                    _v.Caster.AlterStatus(BattleStatus.Regen);
                    _v.Caster.AlterStatus(BattleStatus.AutoLife);
                }
                else
                {
                    TranceSeekCustomAPI.BeatrixPassive[_v.Caster.Data][2] = 1;
                }
                return;
            }

            _v.TryAlterMagicStatuses();
            _v.Target.AlterStatus(TranceSeekCustomAPI.CustomStatus.PowerUp, _v.Caster);
            _v.Target.AlterStatus(TranceSeekCustomAPI.CustomStatus.MagicUp, _v.Caster);
        }
    }
}
