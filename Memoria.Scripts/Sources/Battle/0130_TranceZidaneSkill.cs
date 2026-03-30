using Memoria.Data;
using Memoria.Prime;
using System;
using System.Collections.Generic;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Armour Break
    /// </summary>
    [BattleScript(Id)]
    public sealed class TranceZidaneSkill : IBattleScript
    {
        public const Int32 Id = 0130;

        private readonly BattleCalculator _v;

    public TranceZidaneSkill(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            int LastDigitMP = (int)((_v.Caster.CurrentMp + FF9StateSystem.Battle.FF9Battle.aa_data[_v.Command.AbilityId].MP) % 10);
            Boolean reducemagique = _v.Command.AbilityId == BattleAbilityId.FreeEnergy || _v.Command.AbilityId == BattleAbilityId.Solution9 && LastDigitMP == 9;

            _v.SetWeaponPower();
            _v.Caster.SetMagicAttack();
            _v.Target.SetMagicDefense();
            if (reducemagique)
            {
                if (_v.Command.AbilityId == BattleAbilityId.FreeEnergy)
                {
                    if (LastDigitMP == 0)
                    {
                        int HealMP = (int)(_v.Caster.MaximumMp / 4);
                        _v.Caster.CurrentMp = Math.Min(_v.Caster.CurrentMp + (uint)HealMP, _v.Caster.MaximumMp);
                        btl2d.Btl2dStatReq(_v.Caster.Data, 0, -HealMP);

                    }
                    _v.Context.DefensePower = _v.Context.DefensePower - (_v.Context.DefensePower / 4);
                }
                else if (_v.Command.AbilityId == BattleAbilityId.Solution9)
                    _v.Context.DefensePower /= 2;
            }
            if (_v.Command.AbilityId == BattleAbilityId.MeoTwister && LastDigitMP == 7)
            {
                BattleStatusId[] statuslist = { BattleStatusId.Poison, BattleStatusId.Venom, BattleStatusId.Blind, BattleStatusId.Silence, BattleStatusId.Trouble,
                BattleStatusId.Sleep, BattleStatusId.Freeze, BattleStatusId.Heat, BattleStatusId.Doom, BattleStatusId.Mini, BattleStatusId.Petrify, BattleStatusId.GradualPetrify,
                BattleStatusId.Berserk, BattleStatusId.Confuse, BattleStatusId.Stop, BattleStatusId.Zombie, BattleStatusId.Slow, TranceSeekStatusId.Vieillissement,
                TranceSeekStatusId.ArmorBreak, TranceSeekStatusId.MagicBreak, TranceSeekStatusId.MentalBreak, TranceSeekStatusId.PowerBreak};

                List<BattleStatusId> statuschoosen = new List<BattleStatusId>();

                for (Int32 i = 0; i < statuslist.Length; i++)
                {
                    if ((statuslist[i].ToBattleStatus() & _v.Target.ResistStatus) == 0)
                    {
                        if (statuslist[i] == TranceSeekStatusId.Vieillissement && _v.Target.IsUnderAnyStatus(BattleStatus.EasyKill))
                            continue;

                        statuschoosen.Add(statuslist[i]);
                    }
                }
                BattleStatusId statusselected = statuschoosen[GameRandom.Next16() % statuschoosen.Count];
                btl_stat.AlterStatus(_v.Target, statusselected, _v.Caster);
            }
            TranceSeekAPI.CasterPenaltyMini(_v);
            TranceSeekAPI.PenaltyShellAttack(_v);
            TranceSeekAPI.PenaltyCommandDividedAttack(_v);
            TranceSeekAPI.EnemyTranceBonusAttack(_v);
            TranceSeekAPI.BonusElement(_v);
            if (TranceSeekAPI.CanAttackMagic(_v))
            {
                _v.CalcHpDamage();
                if (_v.Command.AbilityId == BattleAbilityId.ScoopArt)
                {
                    _v.Target.HpDamage /= 3;
                    if (LastDigitMP == 3)
                        TranceSeekAPI.TryCriticalHit(_v, 25);
                }
            }
            if (_v.Command.AbilityId == BattleAbilityId.TidalFlame && LastDigitMP == 2)
                _v.Target.TryAlterStatuses(BattleStatus.Heat, false, _v.Caster);
            else if (_v.Command.AbilityId == BattleAbilityId.ShiftBreak && LastDigitMP == 4)
            {
                _v.Target.TryAlterStatuses(TranceSeekStatus.MentalBreak, false, _v.Caster);
                _v.Context.DamageModifierCount++;
            }
            else if (_v.Command.AbilityId == BattleAbilityId.StellarCircle5 && LastDigitMP == 5)
            {
                _v.Context.DamageModifierCount++;
            }
            else if (_v.Command.AbilityId == BattleAbilityId.GrandLethal)
            {
                float TranceRatio = (255f - _v.Caster.Trance) / 255f;
                _v.Context.DamageModifierCount += (short)(10f * (TranceRatio * TranceRatio));
            }
            else
                TranceSeekAPI.TryAlterMagicStatuses(_v);
        }
    }
}
