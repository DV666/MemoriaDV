using Memoria.Data;
using System;
using System.Collections.Generic;
using Assets.Sources.Scripts.UI.Common;
using FF9;

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
                    _v.Target.TryAlterStatuses(TranceSeekStatus.Provok, true, _v.Caster);
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
                    TranceSeekAPI.SpecialSAEffect[_v.Target.Data][11] = 1;
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
                    if (TranceSeekAPI.SpecialSAEffect[_v.Target.Data][12] != 0)
                    {
                        _v.Context.Flags |= BattleCalcFlags.Miss;
                        return;
                    }

                    uint VanillaMaxMP = _v.Target.MaximumHp;
                    _v.Target.MaximumHp *= 2;
                    TranceSeekAPI.SpecialSAEffect[_v.Target.Data][12] = 1;
                    _v.Target.AddDelayedModifier(
                        target => !target.IsUnderAnyStatus(BattleStatus.Death),
                        target =>
                        {
                            target.MaximumHp = VanillaMaxMP;
                            TranceSeekAPI.SpecialSAEffect[_v.Target.Data][12] = 0;
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
                case (BattleAbilityId)1045: // Praise
                {
                    List<BattleStatus> PraiseStatus = new List<BattleStatus>{ TranceSeekStatus.PowerUp, TranceSeekStatus.MagicUp,
                    TranceSeekStatus.ArmorUp, TranceSeekStatus.MentalUp};

                    _v.Target.AlterStatus(PraiseStatus[Comn.random16() % PraiseStatus.Count]);
                    break;
                }
                case (BattleAbilityId)1164: // Cens
                {
                    Boolean FirstItem = false;
                    Boolean SecondItem = false;
                    Boolean ThirdItem = false;
                    Boolean FourthItem = false;
                    string CensText = null;

                    BattleEnemy battleEnemy = BattleEnemy.Find(_v.Target);
                    if (battleEnemy.StealableItems[0] != RegularItem.NoItem)
                    {
                        if (CensText == null)
                            CensText += $"{FF9TextTool.ItemName(battleEnemy.StealableItems[0])}";

                        _v.StealItem(battleEnemy, 0);
                        FirstItem = true;
                    }
                    if (battleEnemy.StealableItems[1] != RegularItem.NoItem)
                    {
                        if (CensText == null)
                            CensText += $"{FF9TextTool.ItemName(battleEnemy.StealableItems[1])}";
                        else
                            CensText += $" / {FF9TextTool.ItemName(battleEnemy.StealableItems[1])}";

                        _v.StealItem(battleEnemy, 1);
                        SecondItem = true;
                    }
                    if (battleEnemy.StealableItems[2] != RegularItem.NoItem)
                    {
                        if (CensText == null)
                            CensText += $"{FF9TextTool.ItemName(battleEnemy.StealableItems[2])}";
                        else
                            CensText += $" / {FF9TextTool.ItemName(battleEnemy.StealableItems[2])}";

                        _v.StealItem(battleEnemy, 2);
                        ThirdItem = true;
                    }
                    if (battleEnemy.StealableItems[3] != RegularItem.NoItem)
                    {
                        if (CensText == null)
                            CensText += $"{FF9TextTool.ItemName(battleEnemy.StealableItems[3])}";
                        else
                            CensText += $" / {FF9TextTool.ItemName(battleEnemy.StealableItems[3])}";

                        _v.StealItem(battleEnemy, 3);
                        FourthItem = true;
                    }
                    if (!FirstItem && !SecondItem && !ThirdItem && !FourthItem)
                    {
                        UiState.SetBattleFollowFormatMessage(BattleMesages.CouldNotStealAnything);
                    }

                    UiState.SetBattleFollowFormatMessage(BattleMesages.Stole, CensText);
                    break;
                }
                case (BattleAbilityId)1165: // Chosen
                {
                    BattleStatus GoodStatus = (BattleStatus.AutoLife | BattleStatus.Protect | BattleStatus.Shell | BattleStatus.Regen | BattleStatus.Float | BattleStatus.AutoLife |
                    BattleStatus.Haste | BattleStatus.Vanish | BattleStatus.Reflect | TranceSeekStatus.PowerUp | TranceSeekStatus.MagicUp | TranceSeekStatus.ArmorUp | TranceSeekStatus.MentalUp);
                    _v.Target.AlterStatus(GoodStatus);
                    _v.Caster.AddDelayedModifier(
                        caster => caster.IsUnderAnyStatus(BattleStatus.Trance),
                        caster =>
                        {
                            caster.RemoveStatus(GoodStatus);
                        }
                    );
                    break;
                }
                case (BattleAbilityId)1166: // Sanction
                case (BattleAbilityId)1168: // Retribution
                {
                    _v.SetWeaponPower();
                    _v.Caster.SetMagicAttack();
                    _v.Target.SetMagicDefense();
                    TranceSeekAPI.CasterPenaltyMini(_v);
                    TranceSeekAPI.PenaltyShellAttack(_v);
                    TranceSeekAPI.EnemyTranceBonusAttack(_v);
                    TranceSeekAPI.BonusElement(_v);
                    if (TranceSeekAPI.CanAttackMagic(_v))
                    {
                        _v.CalcHpDamage();
                    }
                    TranceSeekAPI.TryAlterMagicStatuses(_v);
                    break;
                }
                case (BattleAbilityId)1167: // Divine hand
                {
                    _v.Target.RemoveStatus(BattleStatusConst.AnyNegative);
                    _v.Target.Flags |= CalcFlag.HpDamageOrHeal;
                    _v.Target.HpDamage = (int)_v.Target.MaximumHp;
                    break;
                }
                case (BattleAbilityId)1169: // Sacred fire
                {
                    _v.Target.RemoveStatus(BattleStatus.Poison | BattleStatus.Venom | BattleStatus.Zombie);
                    _v.Command.AbilityStatus |= (BattleStatus.Regen | BattleStatus.AutoLife);
                    TranceSeekAPI.TryAlterCommandStatuses(_v);
                    break;
                }
            }
        }
    }
}

