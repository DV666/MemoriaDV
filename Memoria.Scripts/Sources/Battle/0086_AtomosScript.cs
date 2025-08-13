using FF9;
using Memoria.Data;
using System;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Atomos
    /// </summary>
    [BattleScript(Id)]
    public sealed class AtomosScript : IBattleScript
    {
        public const Int32 Id = 0086;

        private readonly BattleCalculator _v;

        public AtomosScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Target.IsPlayer && _v.Command.AbilityId == (BattleAbilityId)1532)
            {
                _v.Command.AbilityCategory -= 16; // Remove Magical effect to prevent Vanish to dissapear.
                _v.Target.TryRemoveStatuses(_v.Command.AbilityStatus);
                _v.Target.AlterStatus(TranceSeekStatus.PerfectDodge);
                if (_v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Divine_guidance) && _v.Target.IsPlayer)
                {
                    if (_v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Divine_guidance_Boosted))
                    {
                        _v.CalcHpMagicRecovery();
                        _v.Target.HpDamage /= 3;
                    }
                }
                return;
            }

            _v.SetCommandAttack();
            TranceSeekAPI.BonusElement(_v);
            if (!TranceSeekAPI.CanAttackMagic(_v))
                return;

            _v.CalcCannonProportionDamage();
            if (_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill))
            {
                _v.Target.HpDamage = Math.Max(1, (_v.Target.HpDamage / TranceSeekAPI.MonsterMechanic[_v.Target.Data][5]));
                TranceSeekAPI.MonsterMechanic[_v.Target.Data][5] = TranceSeekAPI.MonsterMechanic[_v.Target.Data][5] * 2;
            }
            if ((ff9item.FF9Item_GetCount(RegularItem.Amethyst)) > Comn.random16() % 100)
                TranceSeekAPI.TryAlterCommandStatuses(_v);
            if (_v.Command.IsShortSummon)
                _v.Target.HpDamage = _v.Target.HpDamage * 2 / 3;
        }
    }
}
