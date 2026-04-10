using System;
using System.Collections.Generic;
using System.Linq;
using FF9;
using Memoria.Assets;
using Memoria.Data;
using Memoria.Database;
using UnityEngine;

namespace Memoria.Scripts.TranceSeek
{
    /// <summary>
    /// Special
    /// </summary>
    [BattleScript(Id)]
    public sealed class MetronomeScript : IBattleScript
    {
        public const Int32 Id = 0144;

        private readonly BattleCalculator _v;
        private Transform _berthaCannonBone;

        public MetronomeScript(BattleCalculator v)
        {
            _v = v;
        }

        public static Dictionary<BTL_DATA, SPSEffect> PolaritySPS = new Dictionary<BTL_DATA, SPSEffect>();

        public void Perform()
        {
            UInt16 target = BattleState.GetRandomUnitId(isPlayer: false);

            List<BattleAbilityId> BlackAndWhiteMagic = new List<BattleAbilityId>();

            foreach (BattleAbilityId abilId in CharacterCommands.Commands[BattleCommandId.BlackMagic].EnumerateAbilities())
            {
                if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1256))
                {
                    if ((FF9StateSystem.Battle.FF9Battle.aa_data[abilId].Ref.Power > 20 || FF9StateSystem.Battle.FF9Battle.aa_data[abilId].Ref.Power == 00))
                        BlackAndWhiteMagic.Add(abilId);
                }
                else
                {
                    BlackAndWhiteMagic.Add(abilId);

                }
            }

            foreach (BattleAbilityId abilId in CharacterCommands.Commands[BattleCommandId.WhiteMagicGarnet].EnumerateAbilities())
            {
                if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1256))
                {
                    if ((FF9StateSystem.Battle.FF9Battle.aa_data[abilId].Ref.Power > 20 || FF9StateSystem.Battle.FF9Battle.aa_data[abilId].Ref.Power == 00))
                        BlackAndWhiteMagic.Add(abilId);
                }
                else
                {
                    BlackAndWhiteMagic.Add(abilId);

                }
            }
            foreach (BattleAbilityId abilId in CharacterCommands.Commands[BattleCommandId.WhiteMagicEiko].EnumerateAbilities())
            {
                if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1256))
                {
                    if ((FF9StateSystem.Battle.FF9Battle.aa_data[abilId].Ref.Power > 20 || FF9StateSystem.Battle.FF9Battle.aa_data[abilId].Ref.Power == 00))
                        BlackAndWhiteMagic.Add(abilId);
                }
                else
                {
                    BlackAndWhiteMagic.Add(abilId);

                }
            }
            foreach (BattleAbilityId abilId in CharacterCommands.Commands[BattleCommandId.BlueMagic].EnumerateAbilities())
            {
                if (ff9abil.FF9Abil_IsMaster(FF9StateSystem.Common.FF9.GetPlayer(CharacterId.Quina), ff9abil.GetAbilityIdFromActiveAbility((BattleAbilityId)abilId))) // If Quina learn the blue magic.
                    BlackAndWhiteMagic.Add(abilId);
            }

            if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1256)) // SA Metronome+
            {
                foreach (BattleAbilityId abilId in CharacterCommands.Commands[BattleCommandId.SummonGarnet].EnumerateAbilities())
                    BlackAndWhiteMagic.Add(abilId);
                foreach (BattleAbilityId abilId in CharacterCommands.Commands[BattleCommandId.SummonEiko].EnumerateAbilities())
                    BlackAndWhiteMagic.Add(abilId);
            }

            BlackAndWhiteMagic.Distinct().ToList();

            BattleAbilityId AbilityChoosen = BlackAndWhiteMagic[GameRandom.Next16() % BlackAndWhiteMagic.Count];
            TargetType TargetAA = FF9StateSystem.Battle.FF9Battle.aa_data[AbilityChoosen].Info.Target;
            Boolean TargetDefaultAlly = FF9StateSystem.Battle.FF9Battle.aa_data[AbilityChoosen].Info.DefaultAlly;
            int ScriptAA = FF9StateSystem.Battle.FF9Battle.aa_data[AbilityChoosen].Ref.ScriptId;

            if (TargetAA == TargetType.SingleAny || TargetAA == TargetType.SingleAlly || TargetAA == TargetType.SingleEnemy)
            {
                if (TargetDefaultAlly)
                    target = BattleState.GetRandomUnitId(isPlayer: true);
                else
                    target = BattleState.GetRandomUnitId(isPlayer: false);
            }
            else if (TargetAA == TargetType.ManyAny || TargetAA == TargetType.ManyAlly || TargetAA == TargetType.ManyEnemy)
            {
                if (GameRandom.Next16() % 2 == 0 || (BattleState.TargetCount(false) == 1 && TargetAA == TargetType.ManyEnemy) || (BattleState.TargetCount(true) == 1 && TargetAA == TargetType.ManyAlly))
                {
                    if (TargetDefaultAlly)
                        target = BattleState.GetRandomUnitId(isPlayer: true);
                    else
                        target = BattleState.GetRandomUnitId(isPlayer: false);
                }
                else
                {
                    if (TargetDefaultAlly)
                        target = 15;
                    else
                        target = 240;
                }
            }
            else if (TargetAA == TargetType.All || TargetAA == TargetType.AllAlly || TargetAA == TargetType.AllEnemy)
            {
                if (TargetDefaultAlly)
                    target = 15;
                else
                    target = 240;
            }
            else if (TargetAA == TargetType.Self)
            {
                target = _v.Caster.Id;
            }
            else if (TargetAA == TargetType.Everyone)
            {
                target = 255;
            }

            if (ScriptAA == 10 && target != 15) // Heal
            {
                Single RatioHP = 0;
                foreach (BattleUnit playerunit in BattleState.EnumerateUnits())
                {
                    if (playerunit.IsPlayer)
                    {
                        Single PlayerRatioHP = BattleScriptDamageEstimate.RateHpMp((Int32)playerunit.CurrentHp, (Int32)playerunit.MaximumHp);
                        if (PlayerRatioHP > RatioHP)
                        {
                            RatioHP = PlayerRatioHP;
                            target = playerunit.Id;
                        }
                    }
                }
            }
            else if ((ScriptAA == 12 || ScriptAA == 103) && target != 15) // Cure Status
            {
                Single RatioStatus = 0;
                foreach (BattleUnit playerunit in BattleState.EnumerateUnits())
                {
                    if (playerunit.IsPlayer)
                    {
                        Single PlayerRatioStatus = 0;
                        if (playerunit.IsUnderAnyStatus(TranceSeekStatus.Vieillissement) && AbilityChoosen == BattleAbilityId.Esuna)
                            PlayerRatioStatus = 20;

                        BattleStatus playerStatus = playerunit.CurrentStatus;
                        BattleStatus removeStatus = (FF9BattleDB.StatusSets.TryGetValue(FF9StateSystem.Battle.FF9Battle.aa_data[AbilityChoosen].AddStatusNo, out BattleStatusEntry stat) ? stat.Value : 0);
                        BattleStatus removedStatus = playerStatus & removeStatus;
                        Int32 rating = BattleScriptStatusEstimate.RateStatuses(removedStatus);

                        if (playerunit.IsPlayer)
                            PlayerRatioStatus = -1 * rating;

                        if (PlayerRatioStatus > RatioStatus)
                        {
                            RatioStatus = PlayerRatioStatus;
                            target = playerunit.Id;
                        }
                    }
                }
            }
            else if (ScriptAA == 13 && target != 15) // Life
            {
                List<UInt16> candidates = new List<UInt16>(4);
                for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next)
                    if (next.bi.player == 1 && btl_stat.CheckStatus(next, BattleStatus.Death) && next.bi.target != 0)
                        candidates.Add(next.btl_id);
                if (candidates.Count > 0)
                    target = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            }

            short OldMpCostFactor = _v.Caster.Player.mpCostFactor;
            _v.Caster.Player.mpCostFactor = 0; // Magic cost 0 MP.

            BattleState.EnqueueCounter(_v.Caster, BattleCommandId.RushAttack, AbilityChoosen, target);

            Int32 counter = 80;
            _v.Caster.AddDelayedModifier(
                caster => (counter -= BattleState.ATBTickCount) > 0,
                caster =>
                {
                    _v.Caster.Player.mpCostFactor = OldMpCostFactor;
                }
            );
        }

    }
}
