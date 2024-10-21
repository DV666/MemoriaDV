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
            TranceSeekCustomAPI.CharacterBonusPassive(_v, "PhysicalAttack");
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
            TranceSeekCustomAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
            TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
            TranceSeekCustomAPI.EnemyTranceBonusAttack(_v);
            TranceSeekCustomAPI.BonusElement(_v);
            if (_v.CanAttackElementalCommand())
            {
                if (_v.Caster.PlayerIndex == CharacterId.Freya)
                {
                    if (_v.Command.AbilityId == BattleAbilityId.CherryBlossom)
                    {
                        short CriticalBonus = _v.Caster.Data.critical_rate_deal_bonus;
                        _v.Caster.Data.critical_rate_deal_bonus += 33;
                        TranceSeekCustomAPI.TryCriticalHit(_v);
                        if (_v.Target.IsUnderAnyStatus(TranceSeekCustomAPI.CustomStatus.Dragon) || (_v.Caster.IsUnderStatus(BattleStatus.Trance)))
                        {
                            if (_v.Caster.Will > Comn.random16() % 100)
                            {
                                _v.Target.TryAlterStatuses(BattleStatus.Venom, false, _v.Caster);
                                _v.Target.TryAlterStatuses(BattleStatus.Poison, false, _v.Caster);
                            }
                            else
                            {
                                _v.Target.TryAlterStatuses(BattleStatus.Poison, false, _v.Caster);
                            }
                        }
                        _v.Caster.Data.critical_rate_deal_bonus = CriticalBonus;
                        _v.CalcHpDamage();
                    }
                }
                else
                {
                    _v.CalcHpDamage();
                    if (_v.Caster.PlayerIndex == CharacterId.Beatrix && _v.Command.AbilityId == (BattleAbilityId)1043) // Fury of the general
                    {
                        TranceSeekCustomAPI.TryCriticalHit(_v);
                    }
                    if (_v.Caster.PlayerIndex == CharacterId.Amarant && (_v.Command.AbilityId == BattleAbilityId.DemiShock1 || _v.Command.AbilityId == BattleAbilityId.DemiShock2)) // Tobigeri
                    {
                        if (_v.Caster.IsUnderAnyStatus(BattleStatus.Protect))
                            _v.Target.AlterStatus(BattleStatus.Blind, _v.Caster);
                        if (_v.Caster.IsUnderAnyStatus(BattleStatus.Shell))
                            _v.Target.AlterStatus(BattleStatus.Silence, _v.Caster);
                        if (_v.Caster.IsUnderAnyStatus(BattleStatus.Reflect))
                            _v.Target.AlterStatus(BattleStatus.Trouble, _v.Caster);
                        if (_v.Caster.IsUnderAnyStatus(BattleStatus.Regen))
                            _v.Target.AlterStatus(BattleStatus.Poison, _v.Caster);
                        if (_v.Caster.IsUnderAnyStatus(BattleStatus.AutoLife))
                            _v.Target.AlterStatus(BattleStatus.Doom, _v.Caster);
                        if (_v.Caster.IsUnderAnyStatus(BattleStatus.Vanish))
                            _v.Target.AlterStatus(BattleStatus.Confuse, _v.Caster);
                        if (_v.Caster.IsUnderAnyStatus(BattleStatus.Haste))
                            _v.Target.AlterStatus(BattleStatus.Slow, _v.Caster);
                    }
                    _v.TryAlterMagicStatuses();
                }
            }
        }
    }
}
