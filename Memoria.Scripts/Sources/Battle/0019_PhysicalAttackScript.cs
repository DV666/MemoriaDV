using System;
using FF9;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Free Energy, Tidal Flame, Scoop Art, Shift Break, Stellar Circle 5, Meo Twister, Solution 9, Grand Lethal, No Mercy, Stock Break, Shock
    /// </summary>
    [BattleScript(Id)]
    public sealed class PhysicalAttackScript : IBattleScript
    {
        public const Int32 Id = 0019;

        private readonly BattleCalculator _v;

        public PhysicalAttackScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Caster.IsPlayer)
            {
                _v.WeaponPhysicalParams();
            }
            else
            {
                _v.NormalPhysicalParams();
            }
            TranceSeekAPI.CharacterBonusPassive(_v, "PhysicalAttack");
            if ((_v.Command.AbilityId == (BattleAbilityId)1005 || _v.Command.AbilityId == (BattleAbilityId)1042) && _v.Command.IsATBCommand) // Attaque Eclair, Hikari
            {
                _v.Command.Element = _v.Caster.WeaponElement;
                _v.Caster.AddDelayedModifier(
                    caster => caster.CurrentAtb >= caster.MaximumAtb,
                    caster =>
                    {
                        if (!caster.IsUnderAnyStatus(BattleStatusConst.StopAtb) && caster.CurrentAtb < caster.MaximumAtb / 2)
                            caster.CurrentAtb += (Int16)(caster.MaximumAtb / 2 + caster.Dexterity);
                    }
                );
            }
            TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
            TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
            TranceSeekAPI.EnemyTranceBonusAttack(_v);
            TranceSeekAPI.BonusElement(_v);
            if (_v.CanAttackMagic())
            {
                if (_v.Command.AbilityId == BattleAbilityId.CherryBlossom)
                {
                    if (_v.Target.IsUnderAnyStatus(TranceSeekStatus.Dragon) || (_v.Caster.IsUnderStatus(BattleStatus.Trance)))
                    {
                        if (_v.Target.IsUnderAnyStatus(BattleStatus.Poison))
                            _v.Target.TryAlterStatuses(BattleStatus.Venom, false, _v.Caster);
                        else
                            TranceSeekAPI.TryAlterMagicStatuses(_v);
                    }
                    _v.CalcHpDamage();
                }
                else
                {
                    _v.CalcHpDamage();
                    if (_v.Command.AbilityId == (BattleAbilityId)1009) // Pluto Charge
                    {
                        int factorDefense = _v.Caster.PhysicalDefence + (_v.Caster.PhysicalDefence * (TranceSeekAPI.StackBreakOrUpStatus[_v.Caster.Data][2]) / 100);
                        _v.Target.HpDamage = (_v.Target.HpDamage * factorDefense) / 100;
                    }
                    else if (_v.Command.AbilityId == (BattleAbilityId)1043) // Fury of the general
                        TranceSeekAPI.TryCriticalHit(_v);
                    else if (_v.Command.AbilityId == BattleAbilityId.DemiShock1 || _v.Command.AbilityId == BattleAbilityId.DemiShock2) // Tobigeri
                    {
                        if (_v.Caster.IsUnderAnyStatus(BattleStatus.Protect))
                            _v.Command.AbilityStatus |= BattleStatus.Blind;
                        if (_v.Caster.IsUnderAnyStatus(BattleStatus.Shell))
                            _v.Command.AbilityStatus |= BattleStatus.Silence;
                        if (_v.Caster.IsUnderAnyStatus(BattleStatus.Reflect))
                            _v.Command.AbilityStatus |= BattleStatus.Trouble;
                        if (_v.Caster.IsUnderAnyStatus(BattleStatus.Regen))
                            _v.Command.AbilityStatus |= BattleStatus.Poison;
                        if (_v.Caster.IsUnderAnyStatus(BattleStatus.AutoLife))
                            _v.Command.AbilityStatus |= BattleStatus.Doom;
                        if (_v.Caster.IsUnderAnyStatus(BattleStatus.Vanish))
                            _v.Command.AbilityStatus |= BattleStatus.Confuse;
                        if (_v.Caster.IsUnderAnyStatus(BattleStatus.Haste))
                            _v.Command.AbilityStatus |= BattleStatus.Slow;
                        if (_v.Caster.IsUnderAnyStatus(BattleStatus.Float))
                            _v.Command.AbilityStatus |= BattleStatus.GradualPetrify;
                    }
                    else
                        TranceSeekAPI.TryAlterMagicStatuses(_v);
                }
            }
        }
    }
}
