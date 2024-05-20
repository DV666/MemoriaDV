using System;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// White Wind
    /// </summary>
    [BattleScript(Id)]
    public sealed class WhiteWindScript : IBattleScript
    {
        public const Int32 Id = 0030;

        private readonly BattleCalculator _v;

        public WhiteWindScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            if (_v.Command.Power == 1)
            {
                _v.Target.Flags |= (CalcFlag.HpAlteration | CalcFlag.HpRecovery);
                _v.Target.HpDamage = (int)_v.Caster.CurrentHp;
            }
            else
            {
                if (_v.Command.Power == 0)
                {
                    if (_v.Caster.PlayerIndex == CharacterId.Quina)
                    {
                        if (_v.Target.PlayerIndex == CharacterId.Quina)
                        {
                            _v.Caster.HpDamage = (int)_v.Caster.CurrentHp;
                            foreach (BattleUnit unit in BattleState.EnumerateUnits())
                            {
                                if (!unit.IsPlayer || !unit.IsTargetable || unit.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump))
                                    continue;

                                _v.Caster.Flags = CalcFlag.HpAlteration;
                                if (!unit.IsUnderAnyStatus(BattleStatus.Zombie))
                                    _v.Caster.Flags = CalcFlag.HpDamageOrHeal;

                                _v.Caster.Change(unit);
                                SBattleCalculator.CalcResult(_v);
                                BattleState.Unit2DReq(unit);
                            }
                            _v.Caster.Flags = 0;
                            _v.Caster.HpDamage = 0;
                            _v.PerformCalcResult = false;
                        }
                    }
                    else
                    {
                        if (_v.Target.Data == _v.Caster.Data)
                        {
                            if (!_v.Target.CanBeHealed())
                                return;

                            _v.Caster.Flags = CalcFlag.HpAlteration | CalcFlag.HpRecovery;
                            _v.Caster.HpDamage = (int)_v.Caster.CurrentHp;

                            foreach (BattleUnit unit in BattleState.EnumerateUnits())
                            {
                                if (unit.IsPlayer)
                                    continue;

                                _v.Caster.Change(unit);
                                SBattleCalculator.CalcResult(_v);
                                BattleState.Unit2DReq(unit);
                            }
                            _v.Caster.Flags = 0;
                            _v.Caster.HpDamage = 0;
                            _v.PerformCalcResult = false;
                        }
                    }
                }
                else
                {
                    if (_v.Target.MagicDefence == 255)
                    {
                        _v.Context.Flags |= BattleCalcFlags.Guard;
                    }
                    else
                    {
                        _v.NormalMagicParams();
                        TranceSeekCustomAPI.CharacterBonusPassive(_v, "MagicAttack");
                        _v.Caster.PenaltyMini();
                        TranceSeekCustomAPI.PenaltyShellAttack(_v);
                        _v.PenaltyCommandDividedAttack();
                        _v.BonusElement();
                        if (TranceSeekCustomAPI.CanAttackMagic(_v))
                        {
                            if (_v.Target.IsLevitate)
                            {
                                _v.Context.Attack = _v.Context.Attack * 3;
                            }
                            _v.CalcHpDamage();
                        }
                        _v.TryAlterMagicStatuses();
                    }
                }
            }
            TranceSeekCustomAPI.SpecialSA(_v);
        }
    }
}