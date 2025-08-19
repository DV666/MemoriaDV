using System;
using Memoria.Data;
using FF9;
using Object = System.Object;
using static Memoria.Scripts.Battle.TranceSeekAPI;
using Memoria.Scripts.Battle;

namespace Memoria.DefaultScripts
{
    // NOTE: this status is still specifically handled by the game in many aspects
    // You cannot recycle it for a completly different effect

    [StatusScript(BattleStatusId.Death)]
    public class DeathStatusScript : StatusScriptBase
    {
        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            if (target.CurrentHp > 0)
            {
                target.FigInfo |= Param.FIG_INFO_DEATH;
                target.Kill(inflicter);
            }
            else
            {
                target.CurrentHp = 0;
            }
            target.CurrentAtb = 0;

            if (!SpecialSAEffect.TryGetValue(target.Data, out var sa))
            {
                SpecialSAEffect[target.Data] = new int[18];
                SpecialSAEffect[target.Data][15] = (int)target.Player.max.hp;
                SpecialSAEffect[target.Data][16] = (int)target.Player.max.mp;
            }

            if (!target.IsPlayer)
            {
                if (target.Data.die_seq == 0)
                {
                    if (target.IsSlave)
                        target.Data.die_seq = 5;
                    else if (!target.Enemy.AttackOnDeath || !btl_util.IsBtlBusy(target, btl_util.BusyMode.CASTER | btl_util.BusyMode.QUEUED_CASTER))
                        target.Data.die_seq = 1;
                }
                btl_sys.CheckForecastMenuOff(target);
            }         
            if (target.IsUnderAnyStatus(BattleStatus.Trance) && btl_cmd.KillSpecificCommand(target, BattleCommandId.SysTrans))
            {
                SpecialSAEffect[target][3] = 1; // Fix SFX "Trance__Out" if character die in a combo attack
                btl_stat.RemoveStatus(target, BattleStatusId.Trance);
                SpecialSAEffect[target][3] = 0;
                target.Trance = 254;
            }          
            if (target.IsPlayer)
            {
                SpecialSAEffect[target.Data][14] = 0; // Reset SOS trigger

                // Reset stats
                target.MaximumHp = (uint)SpecialSAEffect[target.Data][15];
                target.MaximumMp = (uint)SpecialSAEffect[target.Data][16];
                // target.Level = target.Player.level; // Too sensible
                target.Dexterity = target.Player.elem.dex;
                target.Strength = target.Player.elem.str;
                target.Magic = target.Player.elem.mgc;
                target.Will = target.Player.elem.wpr;
                target.PhysicalDefence = target.Player.defence.PhysicalDefence;
                target.MagicDefence = target.Player.defence.MagicalDefence;
                target.PhysicalEvade = target.Player.defence.PhysicalEvade;
                target.MagicEvade = target.Player.defence.MagicalEvade;

                if (target.PlayerIndex == CharacterId.Beatrix)
                {
                    if (!BeatrixPassive.TryGetValue(target.Data, out Int32[] beatrixpassive))
                        BeatrixPassive[target.Data] = [0, 0, 0, 0];
                    BeatrixPassive[target.Data][2] = 0;
                }
            }
            if (!target.HasSupportAbilityByIndex((SupportAbility)1232)) // SA Expiation+
                btl_stat.RemoveStatus(target, TranceSeekStatusId.Redemption);
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            BTL_DATA btl = Target.Data;
            btl.die_seq = 0;
            //btl.bi.dmg_mot_f = 0;
            btl.bi.cmd_idle = 0;
            btl.bi.death_f = 0;
            btl.bi.stop_anim = 0;
            btl.escape_key = 0;
            btl.killer_track = null;

            if (Target.IsPlayer)
                SpecialSAEffect[Target.Data][14] = 0; // Reset SOS trigger

            if (btl_mot.checkMotion(btl, BattlePlayerCharacter.PlayerMotionIndex.MP_DISABLE) || btl_mot.checkMotion(btl, BattlePlayerCharacter.PlayerMotionIndex.MP_DOWN_DISABLE))
            {
                GeoTexAnim.geoTexAnimPlay(btl.texanimptr, 2);
                if (btl.bi.player != 0)
                    GeoTexAnim.geoTexAnimPlay(btl.tranceTexanimptr, 2);
            }
            if (!btl_util.IsBtlUsingCommand(btl, out CMD_DATA cmd) || !btl_util.IsCommandDeclarable(cmd.cmd_no))
                btl.sel_mode = 0;
            foreach (BattleStatusId oprStatus in (Target.PermanentStatus & BattleStatusConst.OprCount & BattleStatusId.Death.GetStatData().ClearOnApply).ToStatusList())
                btl_stat.SetOprStatusCount(Target, oprStatus);
            return true;
        }
    }
}
