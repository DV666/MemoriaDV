using System;
using FF9;
using Memoria.Data;
using static SiliconStudio.Social.ResponseData;

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
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
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
            _v.Caster.PhysicalPenaltyAndBonusAttack();
            TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
            _v.Caster.EnemyTranceBonusAttack();
            TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
            _v.BonusElement();
            if (_v.CanAttackElementalCommand())
            {
                if (_v.Caster.PlayerIndex == CharacterId.Freya)
                {
                    if (_v.Command.AbilityId == BattleAbilityId.CherryBlossom)
                    {
                        _v.Target.Data.critical_rate_receive_bonus += 33;
                    }
                    TranceSeekCustomAPI.TryCriticalHitDragon(_v);
                    if (TranceSeekCustomAPI.MonsterMechanic[_v.Target.Data][5] > 0 || (_v.Caster.IsUnderStatus(BattleStatus.Trance) && _v.Command.AbilityId == BattleAbilityId.CherryBlossom))
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
                        _v.Target.Data.critical_rate_receive_bonus -= 33;
                    }
                    _v.CalcHpDamage();
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
            TranceSeekCustomAPI.SpecialSA(_v);
        }
    }
}
