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
            if (_v.Command.HitRate == 255) // Nv. ? Sidéral
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
                    TranceSeekAPI.CharacterBonusPassive(_v, "MagicAttack");
                    TranceSeekAPI.CasterPenaltyMini(_v);
                    TranceSeekAPI.PenaltyShellAttack(_v);
                    TranceSeekAPI.PenaltyCommandDividedAttack(_v);
                    TranceSeekAPI.BonusElement(_v);
                    if (TranceSeekAPI.CanAttackMagic(_v))
                    {
                        _v.CalcHpDamage();
                    }
                    TranceSeekAPI.TryAlterMagicStatuses(_v);
                    return;
                }
            }
            if (_v.IsTargetLevelMultipleOfCommandRate() && _v.Target.CanBeAttacked())
            {
                if (_v.Command.Power == 90 && _v.Command.HitRate == 4) // Nv. 4 Gravité X
                {
                    if (TranceSeekAPI.CheckUnsafetyOrGuard(_v))
                    {
                        _v.SetCommandAttack();
                        TranceSeekAPI.PenaltyCommandDividedAttack(_v);
                        if (_v.Target.IsUnderStatus(BattleStatus.Shell))
                            _v.Context.DamageModifierCount -= 2;
                        TranceSeekAPI.BonusElement(_v);
                        if (TranceSeekAPI.CanAttackMagic(_v))
                            _v.CalcCannonProportionDamage();
                        TranceSeekAPI.TryAlterMagicStatuses(_v);
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
                                TranceSeekAPI.CharacterBonusPassive(_v, "MagicAttack");
                                TranceSeekAPI.CasterPenaltyMini(_v);
                                TranceSeekAPI.PenaltyShellAttack(_v);
                                TranceSeekAPI.PenaltyCommandDividedAttack(_v);
                                TranceSeekAPI.BonusElement(_v);
                                if (TranceSeekAPI.CanAttackMagic(_v))
                                {
                                    _v.CalcHpDamage();
                                }
                                TranceSeekAPI.TryAlterMagicStatuses(_v);
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
