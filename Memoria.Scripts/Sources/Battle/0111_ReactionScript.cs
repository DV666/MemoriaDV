using System;
using System.Collections.Generic;
using FF9;

namespace Memoria.Scripts.Battle
{
    [BattleScript(Id)]
    public sealed class ReactionScript : IBattleScript
    {
        public const Int32 Id = 0111;

        private readonly BattleCalculator _v;

        public ReactionScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            _v.Target.Flags |= (CalcFlag.HpAlteration);
            _v.Target.HpDamage = TranceSeekCustomAPI.MonsterMechanic[_v.Caster.Data][1];
            Int32 wait = 200;
            _v.Caster.AddDelayedModifier(
                caster => (wait -= BattleState.ATBTickCount) > 0,
                caster =>
                {
                    TranceSeekCustomAPI.MonsterMechanic[_v.Caster.Data][1] = 0;
                }
            );

            TranceSeekCustomAPI.SPSCumulative(_v.Caster.Data, TranceSeekCustomAPI.SPSStackable.ZombieArmor);
            TranceSeekCustomAPI.SpecialSA(_v);
        }
    }
}