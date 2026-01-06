using Memoria.Data;
using Memoria.Prime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Memoria.EchoS
{
    public class BattleScript : IOverloadVABattleScript
    {
        public void Initialize()
        {
            Log.Message("[Echo-S] Initialize");

            BattleVoice.OnBattleInOut += new BattleVoice.BattleInOutDelegate(this.OnBattleInOut);
            BattleVoice.OnAct += new BattleVoice.BattleActDelegate(this.OnAct);
            BattleVoice.OnHit += new BattleVoice.BattleActDelegate(this.OnHit);
            BattleVoice.OnStatusChange += new BattleVoice.StatusChangeDelegate(this.OnStatusChange);
            BattleVoice.OnDialogAudioStart += new BattleVoice.BattleDialogDelegate(this.OnDialogAudioStart);
            BattleVoice.OnDialogAudioEnd += new BattleVoice.BattleDialogDelegate(this.OnDialogAudioEnd);

            BattleSystem.Lines = BattleScriptParser.LoadLines().ToArray();

            if (AssetManager.FolderHighToLow != null)
            {
                var folders = AssetManager.FolderHighToLow;
                for (int i = 0; i < folders.Length; i++)
                {
                    if (folders[i].FolderPath != null && folders[i].FolderPath.EndsWith("BattleSubtitles/"))
                    {
                        PersistenSingleton<BattleSubtitles>.Instance.Enabled = true;
                        return;
                    }
                }
            }
        }

        public bool OnBattleInOut(BattleVoice.BattleMoment when)
        {
            BattleSystem.HasFirstActHappened = false;
            if (BattleSystem.LinesQueue.Count > 1)
            {
                var item = BattleSystem.LinesQueue.Dequeue();
                BattleSystem.LinesQueue.Clear();
                BattleSystem.LinesQueue.Enqueue(item);
                Log.Message($"Cleared all lines but '{BattleSystem.Lines[item.Key].Path}'");
            }

            CharacterId focusChar = BattleVoice.VictoryFocusIndex;
            if ((int)when == 4 && focusChar != CharacterId.NONE && BattleState.BattleUnitCount(true) > 1)
            {
                bool onlyOneSurvivor = true;
                for (int j = 0; j < 4; j++)
                {
                    BattleUnit unit = new BattleUnit(FF9StateSystem.Battle.FF9Battle.btl_data[j]);
                    CharacterId pIndex = unit.PlayerIndex;
                    if (pIndex != CharacterId.NONE && pIndex != focusChar && unit.CurrentHp > 0U)
                    {
                        onlyOneSurvivor = false;
                        break;
                    }
                }
                if (onlyOneSurvivor) when = BattleMomentEx.VictoryPoseSurvivor;
            }

            if (focusChar == CharacterId.NONE)
            {
                int count = BattleState.BattleUnitCount(true);
                int rngIndex = UnityEngine.Random.Range(0, count);
                int current = rngIndex;
                for (BTL_DATA btl = FF9StateSystem.Battle.FF9Battle.btl_list.next; btl != null; btl = btl.next)
                {
                    if (btl.bi.player != 0 && current-- == 0)
                    {
                        focusChar = (CharacterId)btl.bi.slot_no;
                        break;
                    }
                }
                Log.Message($"OnBattleInOut {BattleMomentEx.ToString(when)} {focusChar} RNG: {rngIndex}/{count}");
            }
            else
            {
                Log.Message($"OnBattleInOut {BattleMomentEx.ToString(when)} {focusChar}");
            }

            if (!BattleSystem.CanPlayMoreLines) return true;

            if ((int)when == 1)
            {
                PersistenSingleton<BattleSubtitles>.Instance.ClearAll();
                string pNames = "";
                string eNames = "";
                for (BTL_DATA btl = FF9StateSystem.Battle.FF9Battle.btl_list.next; btl != null; btl = btl.next)
                {
                    BattleUnit unit = new BattleUnit(btl);
                    if (unit.IsPlayer)
                        pNames += $"[{unit.Name}] ";
                    else
                        eNames += $"[{StringExtension.RemoveTags(unit.Name)}({unit.Data.dms_geo_id})] ";
                }

                Log.Message($"BattleId: {FF9StateSystem.Battle.battleMapIndex} Players: {pNames}Enemies: {eNames}");
                BattleSystem.InTranceCharacters.Clear();
                BattleSystem.Flags = 0;

                if (BattleSystem.IsPreemptive) BattleSystem.Flags |= (uint)LineEntryFlag.Preemptive;
                else if (BattleSystem.IsBackAttack) BattleSystem.Flags |= (uint)LineEntryFlag.BackAttack;
                else BattleSystem.Flags |= (uint)LineEntryFlag.FrontAttack;

                BattleSystem.Flags |= (uint)((BattleState.BattleUnitCount(true) > 1) ? LineEntryFlag.PlayerTeam : LineEntryFlag.PlayerSolo);
                BattleSystem.Flags |= (uint)((BattleState.BattleUnitCount(false) > 1) ? LineEntryFlag.EnemyTeam : LineEntryFlag.EnemySolo);
                BattleSystem.Flags |= (uint)((BattleState.IsFriendlyBattle || BattleState.IsRagtimeBattle) ? LineEntryFlag.FriendlyBattle : LineEntryFlag.NonFriendlyBattle);

                if (!BattleState.IsRandomBattle) BattleSystem.Flags |= (uint)(LineEntryFlag.Serious | LineEntryFlag.Boss);
            }

            uint currentFlags = BattleSystem.Flags;
            BattleSystem.QueueLine(BattleSystem.GetRandomLine(when, (i, moment) =>
                BattleSystem.CommonChecks(i, moment, currentFlags, null, ((int)when == 2) ? BattleStatusId.Silence : (BattleStatusId)(-1)) &&
                (!BattleSystem.Lines[i].Speaker.CheckIsPlayer || focusChar == CharacterId.NONE || focusChar == BattleSystem.Lines[i].Speaker.playerId)), when);

            return true;
        }

        public bool OnAct(BattleUnit actingChar, BattleCalculator calc, BattleVoice.BattleMoment when)
        {
            BattleSystem.HasFirstActHappened = true;
            bool isFinished = false;

            if ((int)when == 11)
            {
                if (calc.Command.Id == BattleCommandId.Steal)
                {
                    if (calc.Context.ItemSteal == RegularItem.NoItem)
                    {
                        bool anyItemsLeft = calc.Target.Enemy.Data.steal_item.Any(p => p != RegularItem.NoItem);
                        when = anyItemsLeft ? BattleMomentEx.StealFail : BattleMomentEx.StealEmpty;
                    }
                    else when = BattleMomentEx.StealSuccess;
                }
                else if (calc.Command.Id == BattleCommandId.Eat || calc.Command.Id == BattleCommandId.Cook)
                {
                    switch (calc.Context.EatResult)
                    {
                        case EatResult.Failed: when = BattleMomentEx.EatFail; break;
                        case EatResult.CannotEat: when = BattleMomentEx.EatCannot; break;
                        case EatResult.Yummy: when = BattleMomentEx.EatSuccess; break;
                        case EatResult.TasteBad: when = BattleMomentEx.EatBad; break;
                    }
                }
                isFinished = true;
                BattleSystem.PerformingCalc = null;
            }

            if ((int)when == 9)
            {
                if (calc.Command.Id == (BattleCommandId)59)
                {
                    if (actingChar.InTrance)
                    {
                        when = BattleMomentEx.TranceEnter;
                        BattleSystem.InTranceCharacters.Add(actingChar.Data);
                    }
                    else
                    {
                        when = BattleMomentEx.TranceLeave;
                        BattleSystem.InTranceCharacters.Remove(actingChar.Data);
                    }
                }
                else if (calc.Command.Id == BattleCommandId.Change)
                {
                    when = (actingChar.Row == 0) ? BattleMomentEx.ChangeFront : BattleMomentEx.ChangeBack;
                }
                else
                {
                    BattleSystem.PerformingCalc = calc;
                }
            }

            BattleAbilityId abilityId = actingChar.IsPlayer ? calc.Command.AbilityId : (BattleAbilityId)calc.Command.RawIndex;
            string abilityName = StringExtension.RemoveTags(calc.Command.AbilityCastingName ?? "");
            if (string.IsNullOrEmpty(abilityName) && actingChar.IsPlayer) abilityName = calc.Command.AbilityId.ToString();

            Log.Message($"OnBattleAct {BattleMomentEx.ToString(when)} [{StringExtension.RemoveTags(actingChar.Name)}({actingChar.Id})] {calc.Command.Id} [{abilityName}({(int)abilityId})] {calc.Command.TargetType} {(calc.Target != null ? $"[{StringExtension.RemoveTags(calc.Target.Name)}({calc.Target.Id})]" : "")}");

            if (!BattleSystem.CanPlayMoreLines)
            {
                if (isFinished) ProcessStatuses(calc);
                return true;
            }

            uint actFlags = BattleSystem.Flags | (uint)BattleSystem.GetFlags(calc);
            if (calc.Caster.Data != calc.Target.Data)
            {
                actFlags |= (uint)(calc.Target.IsPlayer ? LineEntryFlag.Ally : LineEntryFlag.Enemy);
            }

            BattleSystem.LineEntryPredicate filter = (i, moment) =>
            {
                if (!BattleSystem.CommonChecks(i, moment, actFlags, actingChar, (BattleStatusId)(-1))) return false;
                if (!BattleSystem.CanPlayMoreLines && BattleSystem.Lines[i].IsVerbal) return false;
                if (BattleSystem.Lines[i].Abilities != null && !BattleSystem.Lines[i].Abilities.Contains(abilityId)) return false;
                if (BattleSystem.Lines[i].CommandId != null && !BattleSystem.Lines[i].CommandId.Contains(calc.Command.Id)) return false;
                return true;
            };

            BattleSystem.QueueLine(BattleSystem.GetRandomLine(when, filter), when);

            BattleVoice.BattleMoment extraMoment = 0;
            if ((int)when == 11)
            {
                if (calc.Target != null && calc.Target.CurrentHp <= 0U) extraMoment = BattleMomentEx.KillEffect;
                else if ((calc.Context.Flags & BattleCalcFlags.Dodge) != 0) extraMoment = BattleMomentEx.DodgeEffect;
                else if ((calc.Context.Flags & BattleCalcFlags.Miss) != 0) extraMoment = BattleMomentEx.MissEffect;
            }

            if ((int)extraMoment != 0)
            {
                Log.Message($"OnBattleAct additional When: {extraMoment}");
                int extraLine = BattleSystem.GetRandomLine(extraMoment, filter);
                if (extraLine >= 0) BattleSystem.QueueLine(extraLine, extraMoment);
            }

            if (isFinished) ProcessStatuses(calc);
            return true;
        }

        public bool OnHit(BattleUnit hitChar, BattleCalculator calc, BattleVoice.BattleMoment when)
        {
            BattleSystem.OnDeathCalc = null;
            if (hitChar.CurrentHp == 0U)
            {
                Log.Message($"OnHit {BattleMomentEx.ToString(when)} [{StringExtension.RemoveTags(hitChar.Name)}({hitChar.Id})] died");
                CheckDeathLowHP(calc);
                return true;
            }

            BattleAbilityId abilityId = calc.Caster.IsPlayer ? calc.Command.AbilityId : (BattleAbilityId)calc.Command.RawIndex;
            Log.Message($"OnHit {BattleMomentEx.ToString(when)} [{StringExtension.RemoveTags(hitChar.Name)}({hitChar.Id})] {calc.Command.Id} [{StringExtension.RemoveTags(calc.Command.AbilityCastingName ?? "")}({(int)abilityId})]");

            uint hitFlags = BattleSystem.Flags | (uint)BattleSystem.GetFlags(calc);
            if (calc.Caster.Data != calc.Target.Data)
            {
                hitFlags |= (uint)(calc.Caster.IsPlayer ? LineEntryFlag.Ally : LineEntryFlag.Enemy);
            }

            BattleSystem.QueueLine(BattleSystem.GetRandomLine(when, (i, moment) =>
            {
                if (!BattleSystem.CommonChecks(i, moment, hitFlags, hitChar, (BattleStatusId)(-1))) return false;
                if (BattleSystem.Lines[i].ContextFlags != 0 && (BattleSystem.Lines[i].ContextFlags & (BattleCalcFlags)calc.Context.Flags) == 0) return false;
                if (!BattleSystem.CanPlayMoreLines && BattleSystem.Lines[i].IsVerbal) return false;

                if (BattleSystem.Lines[i].Target != null)
                {
                    bool isCaster = BattleSystem.Lines[i].Target.CheckIsCharacter(calc.Caster);
                    if (BattleSystem.Lines[i].Target.Without ? isCaster : !isCaster) return false;
                }

                if (BattleSystem.Lines[i].Abilities != null && !BattleSystem.Lines[i].Abilities.Contains(abilityId)) return false;

                if (BattleSystem.Lines[i].CommandId != null)
                {
                    if (!BattleSystem.Lines[i].CommandId.Contains(calc.Command.Id)) return false;
                    if ((calc.Command.Id == BattleCommandId.Item || calc.Command.Id == BattleCommandId.AutoPotion) &&
                        BattleSystem.Lines[i].Items != null && !BattleSystem.Lines[i].Items.Contains(calc.Command.ItemId)) return false;
                }

                return !calc.Caster.IsPlayer || abilityId != (BattleAbilityId)177 || BattleSystem.Lines[i].Items == null || BattleSystem.Lines[i].Items.Contains(calc.Context.ItemSteal);
            }), when);

            CheckDeathLowHP(calc);
            return true;
        }

        private void CheckDeathLowHP(BattleCalculator calc)
        {
            BattleUnit target = calc.Target;
            if (target.HpDamage == 0) return;

            bool isDead = target.IsUnderStatus(BattleStatus.Death);
            if (!isDead && target.CurrentHp == 0U && calc.Command.AbilityId != (BattleAbilityId)106)
            {
                Log.Message($"Death added [{StringExtension.RemoveTags(target.Name)}({target.Id})]");
                OnStatusChangeEx(target, calc, BattleStatusId.Death, (BattleVoice.BattleMoment)18);
                return;
            }

            if (isDead && target.CurrentHp > 0U)
            {
                Log.Message($"Death removed [{StringExtension.RemoveTags(target.Name)}({target.Id})]");
                OnStatusChangeEx(target, calc, BattleStatusId.Death, (BattleVoice.BattleMoment)19);
                return;
            }

            float threshold = target.IsPlayer ? 6f : 4f;
            bool wasNotLow = (float)((ulong)target.CurrentHp + (ulong)((long)target.HpDamage)) * threshold <= target.MaximumHp;
            bool isNowLow = target.CurrentHp * threshold <= target.MaximumHp;
            bool applyEffect = !target.IsPlayer && calc.Target.CheckUnsafetyOrMiss() && calc.Target.CanBeAttacked() && !calc.Target.HasCategory(CharacterCategory.Male);

            if (!wasNotLow && isNowLow)
            {
                OnStatusChangeEx(target, calc, BattleStatusId.LowHP, (BattleVoice.BattleMoment)18);
                if (applyEffect) btl_stat.AddCustomGlowEffect(target.Data, 0, 1, new int[] { -5, -15, -20 });
            }
            else if (wasNotLow && !isNowLow)
            {
                OnStatusChangeEx(target, calc, BattleStatusId.LowHP, (BattleVoice.BattleMoment)19);
                if (applyEffect) btl_stat.ClearAllGlowEffect(target.Data);
            }
        }

        private void ProcessStatuses(BattleCalculator calc)
        {
            if (BattleSystem.StatusEvents.TryGetValue(calc.Command, out List<StatusEventData> events))
            {
                BattleSystem.StatusEvents.Remove(calc.Command);
                new Thread(() =>
                {
                    Thread.Sleep(1);
                    foreach (var data in events)
                    {
                        OnStatusChangeEx(data.statusedChar, data.calc, data.status, data.when);
                    }
                }).Start();
            }
        }

        public bool OnStatusChange(BattleUnit statusedChar, BattleCalculator calc, BattleStatusId status, BattleVoice.BattleMoment when)
        {
            if (status == BattleStatusId.Death || status == BattleStatusId.LowHP) return true;

            if (BattleSystem.PerformingCalc != null && calc != null)
            {
                string casterName = (calc.Caster != null) ? StringExtension.RemoveTags(calc.Caster.Name) : "null";
                Log.Message($"Enqueued OnStatusChange {BattleMomentEx.ToString(when)} [{StringExtension.RemoveTags(statusedChar.Name)}({statusedChar.Id})] {status} [{casterName}({calc.Caster?.Id})]");

                if (!BattleSystem.StatusEvents.ContainsKey(calc.Command)) BattleSystem.StatusEvents[calc.Command] = new List<StatusEventData>();
                BattleSystem.StatusEvents[calc.Command].Add(new StatusEventData { statusedChar = statusedChar, calc = calc, status = status, when = when });
                return true;
            }

            OnStatusChangeEx(statusedChar, calc, status, when);
            return true;
        }

        public bool OnStatusChangeEx(BattleUnit statusedChar, BattleCalculator calc, BattleStatusId status, BattleVoice.BattleMoment when)
        {
            if (!BattleSystem.HasFirstActHappened) return true;

            if (calc != null)
            {
                string casterName = (calc.Caster != null) ? StringExtension.RemoveTags(calc.Caster.Name) : "null";
                Log.Message($"OnStatusChange {BattleMomentEx.ToString(when)} [{StringExtension.RemoveTags(statusedChar.Name)}({statusedChar.Id})] {status} [{casterName}({calc.Caster?.Id})]");
            }
            else
            {
                Log.Message($"OnStatusChange {BattleMomentEx.ToString(when)} [{StringExtension.RemoveTags(statusedChar.Name)}({statusedChar.Id})] {status}");
            }

            if (!BattleSystem.CanPlayMoreLines) return true;

            BattleAbilityId abilityId = 0;
            uint statusFlags = BattleSystem.Flags;

            if (calc != null)
            {
                statusFlags |= (uint)BattleSystem.GetFlags(calc);
                statusFlags |= (uint)(calc.Caster.IsPlayer ? LineEntryFlag.Ally : LineEntryFlag.Enemy);
                if (status == BattleStatusId.Death && (int)when == 19) BattleSystem.OnDeathCalc = calc;
                if (status == BattleStatusId.LowHP && (int)when == 18 && BattleSystem.OnDeathCalc == calc)
                {
                    Log.Message("OnStatusChange LowHP after revive prevented");
                    BattleSystem.OnDeathCalc = null;
                    return true;
                }
                abilityId = calc.Caster.IsPlayer ? calc.Command.AbilityId : (BattleAbilityId)calc.Command.RawIndex;
            }
            else statusFlags |= (uint)LineEntryFlag.Self;

            BattleSystem.QueueLine(BattleSystem.GetRandomLine(when, (i, moment) =>
            {
                if (BattleSystem.Lines[i].Statuses == null || !BattleSystem.Lines[i].Statuses.Contains(status)) return false;
                if (!BattleSystem.CommonChecks(i, moment, statusFlags, statusedChar, status)) return false;
                if (calc == null) return true;
                if (BattleSystem.Lines[i].ContextFlags != 0 && (BattleSystem.Lines[i].ContextFlags & (BattleCalcFlags)calc.Context.Flags) == 0) return false;

                if (BattleSystem.Lines[i].Target != null)
                {
                    bool isCaster = BattleSystem.Lines[i].Target.CheckIsCharacter(calc.Caster);
                    if (BattleSystem.Lines[i].Target.Without ? isCaster : !isCaster) return false;
                }

                if (BattleSystem.Lines[i].Abilities != null && !BattleSystem.Lines[i].Abilities.Contains(abilityId)) return false;

                if (BattleSystem.Lines[i].CommandId != null)
                {
                    if (!BattleSystem.Lines[i].CommandId.Contains(calc.Command.Id)) return false;
                    if ((calc.Command.Id == BattleCommandId.Item || calc.Command.Id == BattleCommandId.AutoPotion) &&
                        BattleSystem.Lines[i].Items != null && !BattleSystem.Lines[i].Items.Contains(calc.Command.ItemId)) return false;
                }
                return true;
            }), when);

            return true;
        }

        public void OnDialogAudioStart(int voiceId, string text)
        {
            Log.Message($"OnBattleDialogAudioStart {voiceId} '{text}'");
            BattleSystem.CurrentPlayingDialog = voiceId;
            BattleSystem.LinesQueue.Clear();
            BattleVoice.StopAllVoices();
            PersistenSingleton<BattleSubtitles>.Instance.ClearAll();
        }

        public void OnDialogAudioEnd(int voiceId, string text)
        {
            Log.Message($"OnBattleDialogAudioEnd {voiceId} '{text}'");
            if (BattleSystem.CurrentPlayingDialog == voiceId) BattleSystem.CurrentPlayingDialog = -1;
        }
    }
}
