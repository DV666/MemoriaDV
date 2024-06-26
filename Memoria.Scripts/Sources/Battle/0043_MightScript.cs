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
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
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
                TranceSeekCustomAPI.SpecialSA(_v);
                return;
            }
            _v.Target.Strength = (byte)Math.Min(99, (_v.Target.Strength + (_v.Target.Strength / _v.Command.Power)));
            _v.Target.Magic = (byte)Math.Min(99, (_v.Target.Magic + (_v.Target.Magic / _v.Command.Power)));
            _v.TryAlterMagicStatuses();
            Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                    {
                        { "US", "↑ Strength ↑" },
                        { "UK", "↑ Strength ↑" },
                        { "JP", "↑ ちから！ ↑" },
                        { "ES", "↑ Forza ↑" },
                        { "FR", "↑ Force ↑" },
                        { "GR", "↑ Fuerza ↑" },
                        { "IT", "↑ Stärke ↑" },
                    };
            btl2d.Btl2dReqSymbolMessage(_v.Target.Data, "[FFA500]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 15);
            Dictionary<String, String> localizedMessage2 = new Dictionary<String, String>
                    {
                        { "US", "↑ Magic ↑" },
                        { "UK", "↑ Magic ↑" },
                        { "JP", "↑ まりょく！ ↑" },
                        { "ES", "↑ POT magico ↑" },
                        { "FR", "↑ Magie ↑" },
                        { "GR", "↑ Magia ↑" },
                        { "IT", "↑ Zauber ↑" },
                    };
            btl2d.Btl2dReqSymbolMessage(_v.Target.Data, "[FFA500]", localizedMessage2, HUDMessage.MessageStyle.DAMAGE, 20);
            TranceSeekCustomAPI.SpecialSA(_v);
        }
    }
}
