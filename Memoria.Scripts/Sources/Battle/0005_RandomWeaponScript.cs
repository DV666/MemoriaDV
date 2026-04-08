using System;

namespace Memoria.Scripts.TranceSeek
{
    [BattleScript(Id)]
    public sealed class RandomWeaponScript : BaseWeaponScript
    {
        public const Int32 Id = 0005;

        public RandomWeaponScript(BattleCalculator v)
            : base(v, CalcAttackBonus.Random)
        {
        }
    }
}
