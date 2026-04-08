using System;

namespace Memoria.Scripts.TranceSeek
{
    [BattleScript(Id)]
    public sealed class LevelWeaponScript : BaseWeaponScript
    {
        public const Int32 Id = 0007;

        public LevelWeaponScript(BattleCalculator v)
            : base(v, CalcAttackBonus.Level)
        {
        }
    }
}
