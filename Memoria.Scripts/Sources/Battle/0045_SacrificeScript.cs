using Memoria.Data;
using System;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Sacrifice
    /// </summary>
    [BattleScript(Id)]
    public sealed class SacrificeScript : IBattleScript
    {
        public const Int32 Id = 0045;

        private readonly BattleCalculator _v;

        public SacrificeScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Command.AbilityId == BattleAbilityId.FrogDrop || _v.Command.Power == 18) // Flash
            {
                if (_v.Target.Data == _v.Caster.Data)
                    foreach (BattleUnit unit in BattleState.EnumerateUnits())
                        if (unit.IsTargetable && !unit.IsUnderAnyStatus(BattleStatus.Petrify | BattleStatus.Death))
                            unit.AlterStatus(BattleStatus.Blind, _v.Caster);
            }
            else if (_v.Command.Power == 0 && _v.Command.HitRate == 255)
            {
                bool casterunderstatus = false;
                if (_v.Caster.IsUnderAnyStatus(_v.Command.AbilityStatus))
                {
                    casterunderstatus = true;
                }
                _v.Target.AlterStatus(_v.Command.AbilityStatus, _v.Caster);
                if (!casterunderstatus)
                {
                    _v.Caster.RemoveStatus(_v.Command.AbilityStatus);
                }
            }
            else
            {
                if (_v.Caster.PlayerIndex == CharacterId.Quina)
                {
                    if (_v.Command.AbilityId == BattleAbilityId.Vanish) // Transfert [TODO] => Need to improve this crappy code ?
                    {
                        byte b = 0;
                        if (_v.Caster.IsUnderAnyStatus(BattleStatus.Trance | BattleStatus.Death | BattleStatus.Petrify))
                        {
                            _v.Context.Flags |= BattleCalcFlags.Miss;
                        }
                        else
                        {
                            foreach (BattleUnit battleUnit in BattleState.EnumerateUnits())
                            {
                                if (battleUnit.IsPlayer && !battleUnit.IsUnderStatus(BattleStatus.Jump | BattleStatus.Trance | BattleStatus.Death | BattleStatus.Petrify))
                                {
                                    b += 1;
                                }
                            }
                            if (b <= 1)
                            {
                                _v.Context.Flags |= BattleCalcFlags.Miss;
                            }
                            else
                            {
                                byte b2 = (byte)(_v.Caster.Trance / (b - 1));
                                foreach (BattleUnit battleUnit2 in BattleState.EnumerateUnits())
                                {
                                    if (battleUnit2.IsPlayer && !battleUnit2.IsUnderAnyStatus(BattleStatus.Jump | BattleStatus.Trance | BattleStatus.Death | BattleStatus.Petrify) && battleUnit2.PlayerIndex != CharacterId.Quina)
                                    {
                                        if (battleUnit2.Trance + b2 < 255)
                                        {
                                            BattleUnit battleUnit3 = battleUnit2;
                                            BattleUnit battleUnit4 = battleUnit3;
                                            battleUnit4.Trance += b2;
                                        }
                                        else
                                        {
                                            battleUnit2.Trance = 255;
                                            battleUnit2.AlterStatus(BattleStatus.Trance);
                                        }
                                    }
                                }
                                _v.Caster.Trance = 0;
                            }
                        }
                    } 
                    else if (_v.Command.AbilityId == (BattleAbilityId)1033) // Sacrifice
                    {
                        _v.Target.CurrentHp = _v.Target.MaximumHp;
                        _v.Target.CurrentMp = _v.Target.MaximumMp;
                        TranceSeekAPI.TryRemoveAbilityStatuses(_v);
                        _v.Caster.CurrentMp = 0;
                        _v.Caster.Kill(_v.Caster);
                    }
                }
            }
        }
    }
}
