using Memoria.Data;
using System;
using System.Collections.Generic;
using static SiliconStudio.Social.ResponseData;

namespace Memoria.Scripts.Battle
{
    [BattleScript(Id)]
    public sealed class BakuScript : IBattleScript
    {
        public const Int32 Id = 0139;

        private readonly BattleCalculator _v;

        public BakuScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            switch (_v.Command.AbilityId)
            {
                case (BattleAbilityId)1153: // You there!
                case (BattleAbilityId)1159: // Get over here!
                {
                    _v.Target.TryAlterStatuses(TranceSeekCustomAPI.CustomStatus.Provok, true, _v.Caster);
                    break;
                }
                case (BattleAbilityId)1154: // Get moving!
                {
                    _v.Target.RemoveStatus(BattleStatus.Sleep | BattleStatus.Slow | BattleStatus.Stop);
                    _v.Target.CurrentAtb = (short)Math.Max(_v.Target.CurrentAtb * 2, _v.Target.MaximumAtb);
                    break;
                }
                case (BattleAbilityId)1155: // Peuh!
                {
                    TranceSeekCustomAPI.SpecialSAEffect[_v.Target.Data][11] = 1;
                    break;
                }
                case (BattleAbilityId)1156: // A gift for you!
                {
                    List<BattleStatusId> TargetStatusList = new List<BattleStatusId>();

                    foreach (BattleStatusId statusId in (_v.Caster.Data.stat.cur & BattleStatusConst.AnyNegative).ToStatusList())
                        if ((_v.Target.Data.stat.invalid & statusId.ToBattleStatus()) == 0 && (_v.Target.Data.stat.cur & statusId.ToBattleStatus()) == 0)
                            TargetStatusList.Add(statusId);

                    if (TargetStatusList.Count == 0)
                    {
                        _v.Context.Flags |= BattleCalcFlags.Miss;
                        return;
                    }
                    BattleStatusId TargetStatusChoosen = TargetStatusList[GameRandom.Next16() % TargetStatusList.Count];
                    _v.Caster.RemoveStatus(TargetStatusChoosen);
                    _v.Target.AlterStatus(TargetStatusChoosen);
                    break;
                }
                case (BattleAbilityId)1157: // It's mine! Ahah!
                {
                    List<BattleStatusId> TargetStatusList = new List<BattleStatusId>();

                    foreach (BattleStatusId statusId in (_v.Target.Data.stat.cur & BattleStatusConst.AnyPositive).ToStatusList())
                        if ((_v.Caster.Data.stat.invalid & statusId.ToBattleStatus()) == 0 && (_v.Caster.Data.stat.cur & statusId.ToBattleStatus()) == 0)
                            TargetStatusList.Add(statusId);
                    
                    if (TargetStatusList.Count == 0)
                    {
                        _v.Context.Flags |= BattleCalcFlags.Miss;
                        return;
                    }
                    BattleStatusId TargetStatusChoosen = TargetStatusList[GameRandom.Next16() % TargetStatusList.Count];
                    _v.Target.RemoveStatus(TargetStatusChoosen);
                    _v.Caster.AlterStatus(TargetStatusChoosen);
                    break;
                }
                case (BattleAbilityId)1158: // That's all?
                {
                    if (TranceSeekCustomAPI.SpecialSAEffect[_v.Target.Data][12] != 0)
                    {
                        _v.Context.Flags |= BattleCalcFlags.Miss;
                        return;
                    }

                    uint VanillaMaxMP = _v.Target.MaximumHp;
                    _v.Target.MaximumHp *= 2;
                    TranceSeekCustomAPI.SpecialSAEffect[_v.Target.Data][12] = 1;
                    _v.Target.AddDelayedModifier(
                        target => !target.IsUnderAnyStatus(BattleStatus.Death),
                        target =>
                        {
                            target.MaximumHp = VanillaMaxMP;
                            TranceSeekCustomAPI.SpecialSAEffect[_v.Target.Data][12] = 0;
                        }
                    );
                    break;
                }
                case (BattleAbilityId)1160: // Tantalus!
                {
                    const BattleStatus cannotAttack = BattleStatus.Petrify | BattleStatus.Death | BattleStatus.Confuse | BattleStatus.Berserk
                                  | BattleStatus.Stop | BattleStatus.Sleep | BattleStatus.Freeze | BattleStatus.Jump;

                    Boolean canAttack = false;
                    foreach (BattleUnit unit in BattleState.EnumerateUnits())
                    {
                        if (unit.PlayerIndex != CharacterId.Zidane || unit.PlayerIndex != CharacterId.Marcus || unit.PlayerIndex != CharacterId.Blank
                            || unit.PlayerIndex != CharacterId.Cinna || unit.IsUnderAnyStatus(cannotAttack))
                            continue;

                        canAttack = true;
                        UInt16 randomEnemy = BattleState.GetRandomUnitId(isPlayer: false);
                        if (randomEnemy == 0)
                            return;

                        BattleState.EnqueueCounter(unit, BattleCommandId.RushAttack, BattleAbilityId.Attack, randomEnemy);
                    }
                    if (canAttack == false)
                    {
                        _v.Context.Flags |= BattleCalcFlags.Miss;
                        return;
                    }
                    break;
                }
            }
        }
    }
}

