using System;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// LV5 Death, Smash, Climhazzard(Story), Stock Break
    /// </summary>
    [BattleScript(Id)]
    public sealed class InstantKillScript : IBattleScript
    {
        public const Int32 Id = 0022;

        private readonly BattleCalculator _v;

        public InstantKillScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Command.HitRate == 255) // Nv. ? Sid�ral
            {
                uint num = GameState.Gil % 10U;
                if (num == 0U)
                {
                    _v.Context.Flags |= BattleCalcFlags.Miss;
                    return;
                }
                if (_v.Target.Level % num == 0U)
                {
                    if (_v.Target.MagicDefence == 255)
                    {
                        _v.Context.Flags |= BattleCalcFlags.Guard;
                        return;
                    }
                    _v.NormalMagicParams();
                    TranceSeekCustomAPI.CharacterBonusPassive(_v, "MagicAttack");
                    TranceSeekCustomAPI.CasterPenaltyMini(_v);
                    TranceSeekCustomAPI.PenaltyShellAttack(_v);
                    TranceSeekCustomAPI.PenaltyCommandDividedAttack(_v);
                    TranceSeekCustomAPI.BonusElement(_v);
                    if (TranceSeekCustomAPI.CanAttackMagic(_v))
                    {
                        _v.CalcHpDamage();
                    }
                    TranceSeekCustomAPI.TryAlterMagicStatuses(_v);
                    return;
                }
            }
            if (_v.IsTargetLevelMultipleOfCommandRate() && _v.Target.CanBeAttacked())
            {
                if (_v.Command.Power == 90 && _v.Command.HitRate == 4) // Nv. 4 Gravit� X
                {
                    if (_v.Target.CheckUnsafetyOrGuard())
                    {
                        _v.SetCommandAttack();
                        TranceSeekCustomAPI.PenaltyCommandDividedAttack(_v);
                        if (_v.Target.IsUnderStatus(BattleStatus.Shell))
                        {
                            _v.Context.Attack = _v.Context.Attack / 2;
                        }
                        TranceSeekCustomAPI.BonusElement(_v);
                        if (TranceSeekCustomAPI.CanAttackMagic(_v))
                        {
                            _v.CalcCannonProportionDamage();
                        }
                        TranceSeekCustomAPI.TryAlterMagicStatuses(_v);
                    }
                }
                else
                {
                    if (_v.Command.Power == 1 && _v.Command.HitRate == 1)
                    {
                        _v.TryDirectHPDamage();
                    }
                    else
                    {
                        if (_v.Command.Power > 0)
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
                                    _v.CalcHpDamage();
                                }
                                TranceSeekCustomAPI.TryAlterMagicStatuses(_v);
                            }
                        }
                        else
                        {
                            _v.TryDirectHPDamage();
                        }
                    }
                }
            }
        }
    }
}
