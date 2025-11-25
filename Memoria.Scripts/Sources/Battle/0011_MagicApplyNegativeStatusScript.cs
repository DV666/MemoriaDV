using System;
using System.Collections.Generic;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Silence, Confuse, Berserk, Blind, Sleep, Slow, Stop, Poison, Break, Doom, Bad Breath, Night, Freeze, Mustard Bomb, Annoy, Countdown
    /// </summary>
    [BattleScript(Id)]
    public sealed class MagicApplyNegativeStatusScript : IBattleScript
    {
        public const Int32 Id = 0011;

        private readonly BattleCalculator _v;

        public MagicApplyNegativeStatusScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Command.AbilityId == BattleAbilityId.Doom)
            {
                if ((_v.Target.ResistStatus & BattleStatus.Doom) != 0)
                {
                    if (_v.Target.MagicDefence == 255)
                    {
                        _v.Context.Flags |= BattleCalcFlags.Guard;
                        return;
                    }
                    _v.Target.Flags |= CalcFlag.HpAlteration;
                    _v.Target.HpDamage = (int)_v.Caster.CurrentHp;
                    if (_v.Target.IsUnderAnyStatus(BattleStatus.Shell))
                        _v.Target.HpDamage /= 2;
                }
                else
                {
                    TranceSeekAPI.MagicAccuracy(_v);
                    _v.Target.PenaltyShellHitRate();
                    _v.PenaltyCommandDividedHitRate();
                    if (TranceSeekAPI.TryMagicHit(_v))
                        TranceSeekAPI.TryAlterCommandStatuses(_v);
                }
            }
            else
            {
                TranceSeekAPI.MagicAccuracy(_v);
                _v.Target.PenaltyShellHitRate();
                _v.PenaltyCommandDividedHitRate();
                if (_v.Caster.Data.dms_geo_id == 5 || _v.Caster.Data.dms_geo_id == 267) // Kuja (multiple target malus)
                {
                    if (_v.Context.sfxThread.targetId != 1 && _v.Context.sfxThread.targetId != 2 && _v.Context.sfxThread.targetId != 4 && _v.Context.sfxThread.targetId != 8)
                    {
                        _v.Context.Attack /= 2;
                        _v.Context.HitRate /= 2;
                    }
                }
                if (_v.Command.Power == 1) // Friendly Jabberwock - Reflect???
                {
                    if (_v.Target.IsUnderStatus(BattleStatus.Reflect))
                    {
                        _v.Command.AbilityStatus |= (BattleStatus.Silence | BattleStatus.Blind | BattleStatus.Slow);
                    }
                }
                else if (_v.Command.Power == 2) // Meltigemini - T Virus
                {
                    _v.Command.AbilityStatus |= (BattleStatus.Zombie | BattleStatus.Virus);
                }
                else if (_v.Command.Power == 7) // Ahriman - Blaster
                {
                    if (TranceSeekAPI.TryMagicHit(_v))
                    {
                        TranceSeekAPI.TryAlterCommandStatuses(_v);
                    }
                    else
                    {
                        uint num = (uint)(1 + GameRandom.Next8() % 9);
                        if (_v.Target.CurrentHp == 1U)
                        {
                            _v.Context.Flags |= BattleCalcFlags.Miss;
                        }
                        else
                        {
                            if (_v.Target.CurrentHp < num)
                            {
                                _v.Target.CurrentHp = (uint)(1L + GameRandom.Next8() % num);
                            }
                            else
                            {
                                _v.Target.CurrentHp = num;
                            }
                            _v.Context.Flags = 0;
                        }
                    }
                    return;
                }
                else if (_v.Command.Power == 10) // Garland - Regression
                {
                    _v.Command.AbilityStatus |= (TranceSeekStatus.Vieillissement);
                }
                else if (_v.Command.Power == 100) // Bass - Shackle Foe
                {
                    _v.Command.AbilityStatus |= (TranceSeekStatus.PowerBreak);
                }
                else if (_v.Command.Power == 101) // Bass - Armor Corrosive
                {
                    _v.Command.AbilityStatus |= (TranceSeekStatus.ArmorBreak);
                }
                else if (_v.Command.AbilityId == (BattleAbilityId)1108) // Nightmara
                {
                    _v.Command.AbilityStatus |= (BattleStatus.Confuse | BattleStatus.Slow);
                }
                else if (_v.Command.AbilityId == (BattleAbilityId)1109) // Nightmaga
                {
                    _v.Command.AbilityStatus |= (BattleStatus.Confuse | BattleStatus.Slow | BattleStatus.Trouble | BattleStatus.Mini);
                }
                else if (_v.Caster.Data.dms_geo_id == 142) // Cauchemard (from Dark Beatrix)
                {

                    if (_v.Command.Power >= 20) // Intimidation
                    {
                        if (_v.Target.MagicDefence == 255)
                        {
                            _v.Context.Flags |= BattleCalcFlags.Guard;
                        }
                        else
                        {
                            _v.NormalMagicParams();
                            TranceSeekAPI.CasterPenaltyMini(_v);
                            TranceSeekAPI.EnemyTranceBonusAttack(_v);
                            TranceSeekAPI.PenaltyShellAttack(_v);
                            TranceSeekAPI.PenaltyCommandDividedAttack(_v);
                            TranceSeekAPI.BonusElement(_v);
                            if (TranceSeekAPI.CanAttackMagic(_v))
                            {
                                _v.Target.Flags |= (CalcFlag.HpAlteration | CalcFlag.MpAlteration);
                                if (_v.Context.IsAbsorb)
                                {
                                    _v.Target.Flags |= (CalcFlag.HpRecovery | CalcFlag.MpRecovery);
                                }
                                _v.CalcHpDamage();
                                int hpDamage2 = _v.Target.HpDamage;
                                if ((_v.Target.Flags & CalcFlag.HpRecovery) != 0)
                                {
                                    _v.Target.FaceTheEnemy();
                                }
                                _v.Target.MpDamage = hpDamage2 >> 4;
                            }
                            _v.Command.AbilityStatus |= (BattleStatus.Stop | BattleStatus.Doom);
                            TranceSeekAPI.TryAlterCommandStatuses(_v);
                            return;
                        }
                    }
                    if (_v.Command.Power > 0)
                    {
                        _v.Target.Flags |= (CalcFlag.MpAlteration);

                        if (_v.Target.IsUnderStatus(BattleStatus.Shell))
                        {
                            _v.Target.MpDamage = (int)(Math.Min(9999, GameRandom.Next16() % (_v.Target.CurrentMp / 2U)));
                        }
                        else
                        {
                            _v.Target.MpDamage = (int)(Math.Min(9999, GameRandom.Next16() % _v.Target.CurrentMp));
                        }
                    }
                    if (TranceSeekAPI.TryMagicHit(_v))
                    {
                        if (_v.Command.Power == 3) // Cauchemar
                            _v.Command.AbilityStatus |= (BattleStatus.Sleep | BattleStatus.Confuse | BattleStatus.Berserk);
                        else if (_v.Command.Power == 4) // Cauchemar
                            _v.Command.AbilityStatus |= (BattleStatus.Trouble | BattleStatus.Confuse | BattleStatus.Berserk);
                        else if (_v.Command.Power == 5) // Cauchemar+
                            _v.Command.AbilityStatus |= (BattleStatus.Sleep | BattleStatus.Confuse | BattleStatus.Berserk | BattleStatus.Doom);
                        else if (_v.Command.Power == 6) // Cauchemar
                            _v.Command.AbilityStatus |= (BattleStatus.Sleep | BattleStatus.Silence | BattleStatus.Confuse);

                        TranceSeekAPI.TryAlterCommandStatuses(_v);
                    }
                    return;
                }
                else if (_v.Caster.Data.dms_geo_id == 166 && TranceSeekAPI.TryMagicHit(_v)) // Thousand Fears (from Dark Beatrix)
                {
                    if (_v.Command.Power == 77) // Eyes of Anguish
                    {
                        List<BattleStatus> statuschoosen = new List<BattleStatus>{ BattleStatus.Sleep, BattleStatus.Stop, BattleStatus.Blind, BattleStatus.Silence, BattleStatus.Doom,
                            BattleStatus.Confuse, BattleStatus.Freeze, BattleStatus.Petrify, BattleStatus.GradualPetrify };
                        int index = GameRandom.Next16() % 10;
                        if (index == 9)
                        {
                            _v.Target.Flags |= (CalcFlag.MpAlteration);
                            if (_v.Target.IsUnderStatus(BattleStatus.Shell))
                                _v.Target.MpDamage = (int)(Math.Min(9999, GameRandom.Next16() % (_v.Target.CurrentMp / 2U)));
                            else
                                _v.Target.MpDamage = (int)(Math.Min(9999, GameRandom.Next16() % _v.Target.CurrentMp));
                        }
                        else
                            _v.Command.AbilityStatus |= (BattleStatus.Sleep | BattleStatus.Silence | BattleStatus.Confuse);
                    }
                    TranceSeekAPI.TryAlterCommandStatuses(_v);
                    return;
                }
                if (_v.Command.HitRate == 255 || (_v.Caster.PlayerIndex == CharacterId.Amarant && _v.Caster.IsUnderStatus(BattleStatus.Trance) && _v.Command.AbilityId == BattleAbilityId.Revive2))
                { // 3 Plaies+
                    TranceSeekAPI.TryAlterCommandStatuses(_v);
                }
                else if (TranceSeekAPI.TryMagicHit(_v))
                {
                    TranceSeekAPI.TryAlterCommandStatuses(_v);
                }
            }
        }
    }
}
