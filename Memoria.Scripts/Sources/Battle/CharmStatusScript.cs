using FF9;
using Memoria.Data;
using Memoria.Database;
using Memoria.Prime;
using Memoria.Scripts.Battle;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.CustomStatus23)]
    public class CharmStatusScript : StatusScriptBase, IAutoAttackStatusScript
    {
        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            if (!target.CanUseTheAttackCommand || !target.IsPlayer)
                return btl_stat.ALTER_RESIST;
            target.AddDelayedModifier(KeepRotating, null);
            TranceSeekAPI.SA_StatusApply(inflicter, false);
            FF9StateSystem.EventState.gEventGlobal[1544] |= (byte)target.Id;
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            btl_stat.StatusCommandCancel(Target);
            FF9StateSystem.EventState.gEventGlobal[1544] &= (byte)~Target.Id;
            return true;
        }

        public Boolean OnATB()
        {
            if (!Target.CanUseTheAttackCommand)
            {
                btl_stat.RemoveStatus(Target, BattleStatusId.CustomStatus23);
                return false;
            }

            List<BattleCommandId> CMDList = new List<BattleCommandId>{ BattleCommandId.Attack, BattleCommandId.BlackMagic, BattleCommandId.DoubleBlackMagic, BattleCommandId.Change};
            BattleCommandId CMDChoosen = CMDList[GameRandom.Next16() % CMDList.Count];
            BattleAbilityId AAChoosen = BattleAbilityId.Attack;
            ushort targetid = BattleState.GetRandomUnitId(true);

            if (CMDChoosen == BattleCommandId.Attack)
            {
                CMDChoosen = BattleCommandHelper.Patch(BattleCommandId.Attack, BattleCommandMenu.Attack, Target.Player, Target);

                if (CharacterCommands.Commands.TryGetValue(CMDChoosen, out CharacterCommand cmdData))
                {
                    BattleAbilityId abilId = cmdData.GetAbilityId(0);
                    AAChoosen = BattleAbilityHelper.Patch(abilId, Target.Player);
                }
            }
            else if (CMDChoosen == BattleCommandId.BlackMagic || CMDChoosen == BattleCommandId.DoubleBlackMagic)
            {
                CMDChoosen = AbilityCMD[Target.PlayerIndex];
                List<BattleAbilityId> AACandidates = new List<BattleAbilityId>();
                List<BattleAbilityId> AAFromStuff = new List<BattleAbilityId>();

                Dictionary<Int32, RegularItem> equipmentIdInAbilityDict = new Dictionary<Int32, RegularItem>();

                for (Int32 i = 0; i < 5; ++i)
                {
                    RegularItem itemId = Target.Player.equip[i];
                    if (itemId != RegularItem.NoItem)
                        foreach (Int32 abilId in ff9item._FF9Item_Data[itemId].ability)
                            AAFromStuff.Add((BattleAbilityId)abilId);
                }

                foreach (BattleAbilityId abilId in CharacterCommands.Commands[CMDChoosen].EnumerateAbilities())
                    if (FF9StateSystem.Battle.FF9Battle.aa_data[abilId].MP <= Target.CurrentMp && (ff9abil.FF9Abil_IsMaster(Target.Player, ff9abil.GetAbilityIdFromActiveAbility((BattleAbilityId)abilId)) || AAFromStuff.Contains(abilId)))
                        AACandidates.Add(abilId);

                if (AACandidates.Count > 0)
                {
                    AAChoosen = AACandidates[GameRandom.Next16() % AACandidates.Count];
                    AA_DATA AADATA = FF9StateSystem.Battle.FF9Battle.aa_data[AAChoosen];

                    if ((AADATA.Category & 2) != 0 && Target.IsUnderAnyStatus(BattleStatusConst.CannotUseMagic))
                    {
                        CMDChoosen = BattleCommandId.Item;
                        AAChoosen = (BattleAbilityId)241; // Echo Screen
                        targetid = Target.Data.btl_id;
                    }
                    else
                    {
                        if (AADATA.Info.Target == TargetType.AllAlly)
                            targetid = btl_scrp.GetBattleID(1U);
                        else if (AADATA.Info.Target == TargetType.AllEnemy)
                            targetid = btl_scrp.GetBattleID(0U);
                        else if (AADATA.Info.Target == TargetType.ManyAlly)
                            targetid = (Comn.random8() % 2 == 0) ? BattleState.GetRandomUnitId(false) : btl_scrp.GetBattleID(1U);
                        else if (AADATA.Info.Target == TargetType.ManyEnemy )
                            targetid = (Comn.random8() % 2 == 0) ? BattleState.GetRandomUnitId(true) : btl_scrp.GetBattleID(0U);
                        else if (AADATA.Info.Target == TargetType.RandomAlly || AADATA.Info.Target == TargetType.SingleAlly)
                            targetid = BattleState.GetRandomUnitId(false);
                        else if (AADATA.Info.Target == TargetType.RandomEnemy || AADATA.Info.Target == TargetType.SingleEnemy)
                            targetid =  BattleState.GetRandomUnitId(true);
                        else if (AADATA.Info.Target == TargetType.ManyAny)
                            if (AADATA.Info.DefaultAlly)
                                targetid = btl_scrp.GetBattleID(1U);
                            else
                                targetid = btl_scrp.GetBattleID(0U);
                        else if (AADATA.Info.Target == TargetType.SingleAny)
                            if (AADATA.Info.DefaultAlly)
                                targetid = BattleState.GetRandomUnitId(false);
                            else
                                targetid = BattleState.GetRandomUnitId(true);
                        else if (AADATA.Info.Target == TargetType.Self)
                            targetid = Target.Data.btl_id;
                    }
                }
                else
                {
                    CMDChoosen = BattleCommandId.Attack;
                    AAChoosen = BattleAbilityId.Attack;
                }

            }
            else if (CMDChoosen == BattleCommandId.Change)
            {
                AAChoosen = BattleAbilityId.Change;
                targetid = Target.Data.btl_id;
            }

            btl_cmd.SetCommand(Target.ATBCommand, CMDChoosen, (Int32)AAChoosen, targetid, 0u);

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
                BattleVoice.TriggerOnStatusChange(unit, BattleVoice.BattleMoment.Used, BattleStatusId.CustomStatus23);
        }

        private Boolean KeepRotating(BattleUnit unit)
        {
            if (!unit.IsUnderAnyStatus(BattleStatus.CustomStatus23))
                return false;
            if (FF9StateSystem.Battle.FF9Battle.btl_phase != FF9StateBattleSystem.PHASE_NORMAL)
                return true;
            if (btl_util.IsBtlUsingCommand(unit))
                return true;
            if (btl_mot.checkMotion(unit, BattlePlayerCharacter.PlayerMotionIndex.MP_IDLE_NORMAL) || btl_mot.checkMotion(unit, BattlePlayerCharacter.PlayerMotionIndex.MP_IDLE_DYING) || (unit.IsPlayer && btl_mot.checkMotion(unit, BattlePlayerCharacter.PlayerMotionIndex.MP_IDLE_CMD)))
                unit.CurrentOrientationAngle -= 11.25f;
            return true;
        }

        private static Dictionary<CharacterId, BattleCommandId> AbilityCMD = new Dictionary<CharacterId, BattleCommandId>
        {
            { CharacterId.Zidane, BattleCommandId.SecretTrick },
            { CharacterId.Vivi, BattleCommandId.BlackMagic },
            { CharacterId.Garnet, BattleCommandId.WhiteMagicGarnet },
            { CharacterId.Steiner, BattleCommandId.SwordAct },
            { CharacterId.Freya, BattleCommandId.DragonAct },
            { CharacterId.Quina, BattleCommandId.BlueMagic },
            { CharacterId.Eiko, BattleCommandId.WhiteMagicEiko },
            { CharacterId.Amarant, BattleCommandId.MasterTrick },
            { CharacterId.Cinna, TranceSeekBattleCommand.Engineer },
            { CharacterId.Marcus, TranceSeekBattleCommand.Oni },
            { CharacterId.Blank, TranceSeekBattleCommand.Alchemy },
            { CharacterId.Beatrix, BattleCommandId.HolySword1 },
            { (CharacterId)12, TranceSeekBattleCommand.Berserker }, // Lani
            { (CharacterId)13, BattleCommandId.Attack }, // Mikoto (unused)
            { (CharacterId)14, TranceSeekBattleCommand.Gyahahah }, // Bakku
            { (CharacterId)15, TranceSeekBattleCommand.Komrade } // Mog
        };
    }
}
