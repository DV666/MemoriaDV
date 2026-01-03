using FF9;
using Memoria.Data;
using Memoria.Database;
using Memoria.Scripts.Battle;
using System;
using System.Collections.Generic;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.Berserk)]
    public class BerserkStatusScript : StatusScriptBase, IAutoAttackStatusScript
    {
        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            if (!target.CanUseTheAttackCommand)
                return btl_stat.ALTER_RESIST;
            TranceSeekAPI.SA_StatusApply(inflicter, false);
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            btl_stat.StatusCommandCancel(Target);
            return true;
        }

        public Boolean OnATB()
        {
            if (!Target.CanUseTheAttackCommand)
            {
                btl_stat.RemoveStatus(Target, BattleStatusId.Berserk);
                return false;
            }

            int TargetID = Target.Data.dms_geo_id == 341 ? 15 : btl_util.GetRandomBtlID(1); // Mantis Reaper from Abadon
            if (Target.IsPlayer)
            {
                BattleCommandId CMDChoosen = BattleCommandId.Attack;
                BattleAbilityId AAChoosen = BattleAbilityId.Attack;
                CMDChoosen = BattleCommandHelper.Patch(BattleCommandId.Attack, BattleCommandMenu.Attack, Target.Player, Target);

                if (CharacterCommands.Commands.TryGetValue(CMDChoosen, out CharacterCommand cmdData)) // For special attacks like Vivi's scepters
                {
                    BattleAbilityId abilId = cmdData.GetAbilityId(0);
                    AAChoosen = BattleAbilityHelper.Patch(abilId, Target.Player);
                }
                btl_cmd.SetCommand(Target.ATBCommand, CMDChoosen, (Int32)AAChoosen, btl_util.GetRandomBtlID(0), 0u);
            }
            else
                btl_cmd.SetEnemyCommand(Target, BattleCommandId.EnemyAtk, Target.EnemyType.p_atk_no, (ushort)TargetID);
            if (Configuration.VoiceActing.Enabled)
                Target.AddDelayedModifier(WaitForAutoAttack, TriggerUsageForBattleVoice);
            return true;
        }

        private Boolean WaitForAutoAttack(BattleUnit unit)
        {
            return btl_cmd.CheckCommandQueued(unit.ATBCommand);
        }

        private void TriggerUsageForBattleVoice(BattleUnit unit)
        {
            if (unit.ATBCommand.ExecutionStep != command_mode_index.CMD_MODE_INSPECTION)
                BattleVoice.TriggerOnStatusChange(unit, BattleVoice.BattleMoment.Used, BattleStatusId.Berserk);
        }
    }
}
