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
            else if (_v.Command.AbilityId == (BattleAbilityId)1019) // Vent Blanc
            {
                _v.Target.Flags |= CalcFlag.HpDamageOrHeal;
                _v.Target.HpDamage = (Int32)(_v.Caster.MaximumHp / 3);
                _v.Target.RemoveStatus(BattleStatus.Poison | BattleStatus.Silence | BattleStatus.Blind);
            }
            else if (_v.Command.AbilityId == (BattleAbilityId)1020 || _v.Command.Power == 99 && _v.Command.HitRate == 99) // Arnica
            {
                _v.Target.Flags |= CalcFlag.HpAlteration;
                if (!_v.Target.IsZombie)
                    _v.Target.Flags |= CalcFlag.HpRecovery;
                _v.Target.HpDamage = 9999;

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
                                _v.Target.HpDamage += _v.Caster.HpDamage / (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1100) ? 2 : 4);

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
                        TranceSeekAPI.CharacterBonusPassive(_v, "MagicAttack");
                        TranceSeekAPI.CasterPenaltyMini(_v);
                        TranceSeekAPI.PenaltyShellAttack(_v);
                        TranceSeekAPI.PenaltyCommandDividedAttack(_v);
                        TranceSeekAPI.BonusElement(_v);
                        if (TranceSeekAPI.CanAttackMagic(_v))
                        {
                            if (_v.Target.IsLevitate) // Telekenesis (x 3 against Flying)
                               _v.Context.DamageModifierCount += 8;
                            _v.CalcHpDamage();
                        }
                        TranceSeekAPI.TryAlterMagicStatuses(_v);
                    }
                }
            }
        }
    }
}
