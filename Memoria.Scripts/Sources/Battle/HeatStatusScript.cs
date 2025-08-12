using System;
using Memoria.Data;
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
            if (Target.IsUnderAnyStatus(BattleStatus.Heat) && Target.IsUnderAnyStatus(BattleStatus.EasyKill) && Target.CurrentHp > 1) // Heat Damage
            {
                UInt32 heat_damage = Target.MaximumHp / 128;
                if (TranceSeekAPI.MonsterMechanic[Target.Data][3] > 0)
                    heat_damage = (Target.MaximumHp - 10000) / 128;
                if (heat_damage > 9999)
                    heat_damage = Math.Max(Target.CurrentHp, 9999);

                if (heat_damage > 0)
                {
                    if ((EffectElement.Fire & Target.AbsorbElement) != 0)
                        Target.Data.fig.info = FF9.Param.FIG_INFO_HP_RECOVER;
                    else
                        Target.Data.fig.info = FF9.Param.FIG_INFO_DISP_HP;

                    if (Target.CurrentHp > heat_damage)
                        Target.CurrentHp -= heat_damage;
                    else
                        Target.CurrentHp = 1;

                    btl2d.Btl2dStatReq(Target, (Int32)heat_damage, 0);
                }
            }
        }

        public Boolean OnCommandRun(BattleCommand cmd)
        {
            if (cmd.Data.regist != null && (cmd.Data.cmd_no < BattleCommandId.EnemyReaction || cmd.Data.cmd_no > BattleCommandId.BoundaryUpperCheck))
            {
                BTL_DATA btl = cmd.Data.regist;

                if (!btl_stat.CheckStatus(btl, BattleStatus.EasyKill))
                {
                    if (btl_stat.CheckStatus(btl, BattleStatus.Heat))
                    {
                        if (btl_stat.AlterStatus(new BattleUnit(btl), BattleStatusId.Death) == btl_stat.ALTER_SUCCESS)
                        {
                            BattleVoice.TriggerOnStatusChange(btl, BattleVoice.BattleMoment.Used, BattleStatusId.Heat);
                            btl_cmd.KillCommand(cmd);
                        }
                    }
                }
            }
            return false;
        }       
    }
}
