using Memoria.Data;
using System;

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
            if (_v.Command.HitRate == 111)
            {
                _v.Target.Flags |= CalcFlag.MpDamageOrHeal;
                _v.Target.MpDamage = (int)(_v.Caster.CurrentMp / 10);
            }
            else if (_v.Command.Power == 1)
            {
                _v.Target.Flags |= CalcFlag.HpDamageOrHeal;
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
                            if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)100)) // Medecin
                            {
                                _v.Caster.HpDamage += _v.Caster.HpDamage / 4;
                            }
                            else if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1100)) // Medecin +
                            {
                                _v.Caster.HpDamage += _v.Caster.HpDamage / 2;
                            }
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
                        TranceSeekCustomAPI.CasterPenaltyMini(_v);
                        TranceSeekCustomAPI.PenaltyShellAttack(_v);
                        TranceSeekCustomAPI.PenaltyCommandDividedAttack(_v);
                        TranceSeekCustomAPI.BonusElement(_v);
                        if (TranceSeekCustomAPI.CanAttackMagic(_v))
                        {
                            if (_v.Target.IsLevitate)
                            {
                                _v.Context.Attack = _v.Context.Attack * 3;
                            }
                            _v.CalcHpDamage();
                        }
                        TranceSeekCustomAPI.TryAlterMagicStatuses(_v);
                    }
                }
            }
        }
    }
}
