using FF9;
using Memoria.Data;
using Memoria.Database;
using Memoria.Scripts.TranceSeek;
using System;
using UnityEngine;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.Confuse)]
    public class ConfuseStatusScript : StatusScriptBase, IAutoAttackStatusScript
    {
        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            if (!target.CanUseTheAttackCommand)
                return btl_stat.ALTER_RESIST;
            target.AddDelayedModifier(KeepRotating, null);
            TranceSeekAPI.SA_StatusApply(inflicter, false);
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            Target.CurrentOrientationAngle = Target.DefaultOrientationAngle;
            btl_stat.StatusCommandCancel(Target);
            return true;
        }

        public Boolean OnATB()
        {
            if (!Target.CanUseTheAttackCommand)
            {
                btl_stat.RemoveStatus(Target, BattleStatusId.Confuse);
                return false;
            }
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
                btl_cmd.SetCommand(Target.ATBCommand, CMDChoosen, (Int32)AAChoosen, btl_util.GetRandomBtlID((UInt32)(Comn.random8() & 1)), 0u);
            }
            else
                btl_cmd.SetEnemyCommand(Target, BattleCommandId.EnemyAtk, Target.EnemyType.p_atk_no, btl_util.GetRandomBtlID((UInt32)(Comn.random8() & 1)));
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
                BattleVoice.TriggerOnStatusChange(unit, BattleVoice.BattleMoment.Used, BattleStatusId.Confuse);
        }

        private Boolean KeepRotating(BattleUnit unit)
        {
            if (!unit.IsUnderAnyStatus(BattleStatus.Confuse))
                return false;
            if (FF9StateSystem.Battle.FF9Battle.btl_phase != FF9StateBattleSystem.PHASE_NORMAL)
                return true;
            if (btl_util.IsBtlUsingCommand(unit))
                return true;
            if (btl_mot.checkMotion(unit, BattlePlayerCharacter.PlayerMotionIndex.MP_IDLE_NORMAL) || btl_mot.checkMotion(unit, BattlePlayerCharacter.PlayerMotionIndex.MP_IDLE_DYING) || (unit.IsPlayer && btl_mot.checkMotion(unit, BattlePlayerCharacter.PlayerMotionIndex.MP_IDLE_CMD)) || (unit.IsPlayer && btl_mot.checkMotion(unit, BattlePlayerCharacter.PlayerMotionIndex.MP_DEFENCE)))
                unit.CurrentOrientationAngle += 11.25f;
            return true;
        }
    }
}
