using Memoria.Data;
using System;

namespace Memoria.Scripts.Battle
{
    [BattleScript(Id)]
    public sealed class AlchemyScript : IBattleScript
    {
        public const Int32 Id = 0138;

        private readonly BattleCalculator _v;

        public AlchemyScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            switch (_v.Command.AbilityId)
            {
                case (BattleAbilityId)1145: // Bandage
                case (BattleAbilityId)1149: // Premiers soins
                {
                    _v.Target.Flags |= CalcFlag.HpDamageOrHeal;
                    _v.CalcDamageCommon();
                    _v.Target.HpDamage = (int)((_v.Target.MaximumHp * _v.Command.Power) / 100);
                    break;
                }
                case (BattleAbilityId)1146: // Ingrédient secret
                {
                    btl_stat.AlterStatus(_v.Target, TranceSeekCustomAPI.CustomStatusId.Special, parameters: "Secretingredient++");
                    break;
                }
            }
        }
    }
}

