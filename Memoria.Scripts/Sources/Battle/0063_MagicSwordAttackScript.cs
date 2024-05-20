using System;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Magic Sword
    /// </summary>
    [BattleScript(Id)]
    public sealed class MagicSwordAttackScript : IBattleScript
    {
        public const Int32 Id = 0063;

        private readonly BattleCalculator _v;

        public MagicSwordAttackScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            if ((_v.Caster.PlayerIndex == CharacterId.Steiner && _v.Command.AbilityId == BattleAbilityId.None7)) // Comet Sword + Meteor Sword
            {
                _v.Context.Attack = UnityEngine.Random.Range(((_v.Caster.Strength + _v.Caster.Level) / 3), (_v.Caster.Strength + _v.Caster.Level));
            }
            else
            {
                _v.Caster.SetLowPhysicalAttack();
            }
            _v.SetWeaponPowerSum();
            _v.Target.SetMagicDefense();
            _v.Caster.EnemyTranceBonusAttack();
            TranceSeekCustomAPI.PenaltyShellAttack(_v);
            _v.BonusElement();
            if (TranceSeekCustomAPI.CanAttackMagic(_v))
            {
                if (_v.Target.HasCategory(EnemyCategory.Humanoid) && (_v.Command.AbilityId == BattleAbilityId.None5 || _v.Command.AbilityId == BattleAbilityId.BioSword)) // Poison and Bio Sword
                {
                    _v.Context.Attack = _v.Context.Attack * 2;
                }
                _v.CalcHpDamage();
            }
            _v.TryAlterMagicStatuses();
            TranceSeekCustomAPI.SpecialSA(_v);
        }
    }
}