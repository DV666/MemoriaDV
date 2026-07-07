using Memoria.Data;
using System;
using System.Collections.Generic;

namespace Memoria.Scripts.TranceSeek
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
            if (_v.Caster.PlayerIndex == CharacterId.Beatrix && (_v.Command.AbilityId == TranceSeekBattleAbility.Braver || _v.Command.AbilityId == TranceSeekBattleAbility.Heroism))
            {
                var Caster_TSVar = _v.CasterState();
                if (Caster_TSVar.Beatrix.Braver > 0)
                {
                    _v.Context.Flags |= BattleCalcFlags.Miss;
                    return;
                }
                btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.PowerUp, parameters: $"+{_v.Command.Power}");
                btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.MagicUp, parameters: $"+{_v.Command.Power}");
                btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.Redemption, parameters: "MaxStack");
                if (_v.Caster.IsUnderAnyStatus(BattleStatus.Trance))
                {
                    _v.Command.AbilityStatus |= (BattleStatus.Regen | BattleStatus.AutoLife);
                    TranceSeekAPI.TryAlterCommandStatuses(_v);
                }
                Caster_TSVar.Beatrix.Braver = 1;
                TranceSeekCharacterMechanic.UpdateRedemptionHUD(_v.Target);
                return;
            }
            else if (_v.Caster.Data.dms_geo_id == 410 && _v.Command.Power == 2 || (_v.Caster.Data.dms_geo_id == 410 && _v.Command.Power == 4 || _v.Command.AbilityId == TranceSeekBattleAbility.SuperMuscles)) // [Lani] Adrénaline + Super Muscles
            {
                btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.PowerUp, parameters: $"+{_v.Command.Power}");
                btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.ArmorUp, parameters: $"+{_v.Command.Power}");
                return;
            }
            else if (_v.Command.HitRate == 77) // [Divinorum] Knowledge of the Elders
            {
                _v.Target.Magic += 10;
                _v.Target.Will += 10;
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


