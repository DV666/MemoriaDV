using Memoria.Data;
using System;
using System.Collections.Generic;
using static Memoria.Scripts.Battle.TranceSeekAPI;

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
                btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.PowerUp, parameters: $"+{_v.Command.Power}");
                btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.MagicUp, parameters: $"+{_v.Command.Power}");
                _v.Target.AlterStatus(TranceSeekStatus.Redemption, _v.Caster);
                _v.Target.AlterStatus(TranceSeekStatus.Redemption, _v.Caster);
                if (_v.Target.HasSupportAbilityByIndex((SupportAbility)232)) // SA Expiation
                    _v.Target.AlterStatus(TranceSeekStatus.Redemption, _v.Caster);
                if (_v.Caster.IsUnderAnyStatus(BattleStatus.Trance))
                {
                    BeatrixPassive[_v.Caster.Data][2] = 2;
                    _v.Command.AbilityStatus |= (BattleStatus.Regen | BattleStatus.AutoLife);
                    TranceSeekAPI.TryAlterCommandStatuses(_v);
                }
                else
                {
                    BeatrixPassive[_v.Caster.Data][2] = 1;
                }
                return;
            }
            else if (_v.Caster.Data.dms_geo_id == 410 && _v.Command.Power == 2 || (_v.Caster.Data.dms_geo_id == 410 && _v.Command.Power == 4 || _v.Command.AbilityId == (BattleAbilityId)1081)) // [Lani] Adrénaline + Super Muscles
            {
                btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.PowerUp, parameters: $"+{_v.Command.Power}");
                btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.ArmorUp, parameters: $"+{_v.Command.Power}");
                return;
            }
            else if (_v.Command.HitRate == 77) // [Divinorum] Knowledge of the Elders
            {
                _v.Target.Magic += 10;
                _v.Target.Will += 10;
                Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                                {
                                    { "US", "Magic ↑" },
                                    { "UK", "Magic ↑" },
                                    { "JP", "まりょく ↑" },
                                    { "ES", "POT magico ↑" },
                                    { "FR", "Magie ↑" },
                                    { "GR", "Magia ↑" },
                                    { "IT", "Zauber ↑" },
                                };
                btl2d.Btl2dReqSymbolMessage(_v.Target.Data, "[F9FF39]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 0);
                Dictionary<String, String> localizedMessage2 = new Dictionary<String, String>
                                {
                                    { "US", "Spirit ↑" },
                                    { "UK", "Spirit ↑" },
                                    { "JP", "きりょく ↑" },
                                    { "ES", "POT spirito ↑" },
                                    { "FR", "Esprit ↑" },
                                    { "GR", "Espíritu ↑" },
                                    { "IT", "Wille ↑" },
                                };
                btl2d.Btl2dReqSymbolMessage(_v.Target.Data, "[F9FF39]", localizedMessage2, HUDMessage.MessageStyle.DAMAGE, 5);
                _v.Target.Flags |= CalcFlag.MpDamageOrHeal;
                _v.Target.MpDamage = (int)(_v.Target.MaximumMp);
                return;
            }

            TranceSeekAPI.TryAlterMagicStatuses(_v);
            btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.PowerUp, parameters: $"+{_v.Command.Power}");
            btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.MagicUp, parameters: $"+{_v.Command.Power}");
        }
    }
}
