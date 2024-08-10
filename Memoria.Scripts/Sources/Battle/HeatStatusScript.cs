using System;
using Memoria.Data;
using Memoria.Scripts.Battle;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.Heat)]
    public class HeatStatusScript : StatusScriptBase, IFinishCommandScript
    {
        public BattleUnit HeatInflicter = null;

        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            HeatInflicter = inflicter;
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            return true;
        }

        public void OnFinishCommand(CMD_DATA cmd, Int32 tranceDecrease)
        {
            if (Target.IsUnderAnyStatus(BattleStatus.Heat) && Target.IsUnderAnyStatus(BattleStatus.EasyKill)) // Heat Damage
            {
                UInt32 heat_damage = Target.MaximumHp / 64;
                if (TranceSeekCustomAPI.MonsterMechanic[Target.Data][3] > 0)
                    heat_damage = (Target.MaximumHp - 10000) / 64;
                if (heat_damage > 9999)
                    heat_damage = Math.Max(Target.CurrentHp, 9999);

                if (heat_damage > 0)
                {
                    Target.AddDelayedModifier(
                    target => target.CurrentAtb >= target.MaximumAtb,
                    caster =>
                    {
                        if ((EffectElement.Fire & caster.AbsorbElement) != 0)
                            caster.Data.fig_info = FF9.Param.FIG_INFO_HP_RECOVER;
                        else
                            caster.Data.fig_info = FF9.Param.FIG_INFO_DISP_HP;

                        if (Target.CurrentHp > heat_damage)
                            Target.CurrentHp -= heat_damage;
                        else
                            Target.Kill(HeatInflicter);
                        btl2d.Btl2dStatReq(Target, (Int32)heat_damage, 0);
                    }
                );
                }
            }
        }
    }
}
