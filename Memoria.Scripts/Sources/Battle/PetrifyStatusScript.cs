using Memoria.Data;
using Memoria.Scripts.TranceSeek;
using System;
using UnityEngine;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.Petrify)]
    public class PetrifyStatusScript : StatusScriptBase
    {
        public Int32 DiffPhysicalEvade;
        public Int32 DiffMagicalEvade;

        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            if (inflicter == null)
                inflicter = target;
            base.Apply(target, inflicter, parameters);
            btl_stat.RemoveStatus(target, BattleStatusId.GradualPetrify);
            var Statufication_State = target.State().Monster.Statufication;
            if (target.IsUnderAnyStatus(BattleStatus.EasyKill) && !Statufication_State)
            {
                DiffPhysicalEvade = Math.Max(1, (3 * target.PhysicalEvade) / 4);
                target.PhysicalEvade = Math.Max(0, target.PhysicalEvade - DiffPhysicalEvade);
                DiffMagicalEvade = Math.Max(1, target.MagicEvade / 4);
                target.MagicEvade = Math.Max(0, target.MagicEvade - DiffMagicalEvade);

                AlterPetrifyTexture(target, true);
                Statufication_State = true;

                BattleStatusDataEntry statusData = FF9StateSystem.Battle.FF9Battle.status_data[BattleStatusId.Poison];
                Int32 wait = (short)(((400 + (inflicter.Will * 2) - target.Will) * statusData.ContiCnt) * (inflicter.HasSupportAbilityByIndex(TranceSeekSupportAbility.Persistence_Boosted) ? (150 / 100) : inflicter.HasSupportAbilityByIndex(TranceSeekSupportAbility.Persistence) ? (125 / 100) : 1));
                Target.AddDelayedModifier(
                target => (wait -= target.Data.cur.at_coef * BattleState.ATBTickCount) > 0,
                target =>
                {
                    if (DiffPhysicalEvade > 0)
                        Target.PhysicalEvade = Math.Min(255, Target.PhysicalEvade + DiffPhysicalEvade);
                    if (DiffMagicalEvade > 0)
                        Target.MagicEvade = Math.Min(255, Target.MagicEvade + DiffMagicalEvade);

                    AlterPetrifyTexture(Target, false);
                    Statufication_State = false;
                }
                );

                return btl_stat.ALTER_SUCCESS_NO_SET;
            }
            else if (!btl_cmd.CheckUsingCommand(target.PetrifyCommand) && FF9StateSystem.Battle.FF9Battle.btl_phase > FF9StateBattleSystem.PHASE_ENTER && Configuration.Battle.Speed < 3)
            {
                btl_cmd.SetCommand(target.PetrifyCommand, BattleCommandId.SysStone, 0, target.Id, 0);
                return btl_stat.ALTER_SUCCESS_NO_SET;
            }
            target.CurrentAtb = 0;
            btl_cmd.KillSpecificCommand(target, BattleCommandId.SysStone);
            TranceSeekAPI.SA_StatusApply(inflicter, false);
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            btl_cmd.KillSpecificCommand(Target, BattleCommandId.SysStone);
            return true;
        }

        private static void AlterPetrifyTexture(BTL_DATA btl, Boolean sw)
        {
            Int32 petrifyFlag = sw ? 1 : 0;
            foreach (Renderer renderers in btl.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
                renderers.material.SetFloat("_IsPetrify", petrifyFlag);
            foreach (Renderer renderers in btl.gameObject.GetComponentsInChildren<MeshRenderer>())
                foreach (Material material in renderers.materials)
                    material.SetFloat("_IsPetrify", petrifyFlag);
        }
    }
}
