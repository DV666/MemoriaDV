using System;
using Memoria.Data;
using Memoria.Prime;
using Memoria.Scripts.Battle;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.Heat)]
    public class HeatStatusScript : StatusScriptBase, IFinishCommandScript, IOverloadOnCommandRunScript
    {
        public BattleUnit HeatInflicter = null;

        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            HeatInflicter = inflicter;
            TranceSeekAPI.SA_StatusApply(inflicter, false);
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            return true;
        }

        public void OnFinishCommand(CMD_DATA cmd, Int32 tranceDecrease)
        {
            if (Target.IsUnderAnyStatus(BattleStatus.Heat) && Target.CurrentHp > 1) // Heat Damage on monsters
            {
                UInt32 heat_damage = (uint)(Target.MaximumHp / (Target.IsUnderAnyStatus(BattleStatus.EasyKill) ? 128 : 32));
                if (Target.State().Monster.HPBoss10000)
                    heat_damage = (uint)((Target.MaximumHp - 10000) / (Target.IsUnderAnyStatus(BattleStatus.EasyKill) ? 128 : 32));
                if (heat_damage > 9999)
                    heat_damage = Math.Max(Target.CurrentHp, 9999);

                if (heat_damage > 0)
                {
                    if ((EffectElement.Fire & Target.AbsorbElement) != 0 || Target.Accessory == TranceSeekRegularItem.SolarCrown)
                    {
                        Target.Data.fig.info = FF9.Param.FIG_INFO_HP_RECOVER;
                        Target.CurrentHp = Math.Min(Target.CurrentHp + heat_damage, Target.MaximumHp);
                        btl2d.Btl2dStatReq(Target, -(Int32)heat_damage, 0);
                    }
                    else if (!Target.IsPlayer)
                    {
                        Target.Data.fig.info = FF9.Param.FIG_INFO_DISP_HP;
                        btl2d.Btl2dStatReq(Target, (Int32)heat_damage, 0);
                        if (Target.CurrentHp > heat_damage)
                        {
                            Target.CurrentHp -= heat_damage;
                        }
                        else if (btl_stat.AlterStatus(new BattleUnit(Target), BattleStatusId.Death) == btl_stat.ALTER_SUCCESS)
                        {
                            BattleVoice.TriggerOnStatusChange(Target, BattleVoice.BattleMoment.Used, BattleStatusId.Heat);
                            btl_cmd.KillCommand(cmd);
                        }
                    }
                }
            }
        }

        public Boolean OnCommandRun(BattleCommand cmd)
        {
            BTL_DATA btl = cmd.Data.regist;
            if (btl != null && (cmd.Data.cmd_no < BattleCommandId.EnemyReaction || cmd.Data.cmd_no > BattleCommandId.BoundaryUpperCheck) && btl_stat.CheckStatus(btl, BattleStatus.Heat))
            {
                BattleUnit unit = new BattleUnit(btl);
                Boolean DontKillUnderHeat = (unit.Accessory == TranceSeekRegularItem.SolarCrown || ((byte)EffectElement.Fire & btl.def_attr.absorb) != 0 && btl.bi.player == 0);

                if (!btl_stat.CheckStatus(btl, BattleStatus.EasyKill) && !DontKillUnderHeat)
                {
                    if (btl_stat.AlterStatus(new BattleUnit(btl), BattleStatusId.Death) == btl_stat.ALTER_SUCCESS)
                    {
                        BattleVoice.TriggerOnStatusChange(btl, BattleVoice.BattleMoment.Used, BattleStatusId.Heat);
                        btl_cmd.KillCommand(cmd);
                    }
                }
            }
            return false;
        }       
    }
}
