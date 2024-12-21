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
            _v.SetCommandAttack();
            TranceSeekCustomAPI.BonusElement(_v);
            if (!TranceSeekCustomAPI.CanAttackMagic(_v))
                return;

            _v.CalcCannonProportionDamage();
            if (_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill))
            {
                _v.Target.HpDamage = Math.Max(1, (_v.Target.HpDamage / TranceSeekCustomAPI.MonsterMechanic[_v.Target.Data][5]));
                TranceSeekCustomAPI.MonsterMechanic[_v.Target.Data][5] = TranceSeekCustomAPI.MonsterMechanic[_v.Target.Data][5] * 2;
            }
            if ((ff9item.FF9Item_GetCount(RegularItem.Amethyst)) > Comn.random16() % 100)
            {
                _v.Target.AlterStatus(BattleStatus.Mini, _v.Caster);
            }
            if (_v.Command.IsShortSummon)
                _v.Target.HpDamage = _v.Target.HpDamage * 2 / 3;
        }
    }
}
