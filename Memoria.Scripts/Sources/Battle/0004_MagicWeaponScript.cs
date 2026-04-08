using System;

namespace Memoria.Scripts.TranceSeek
{
    [BattleScript(Id)]
    public sealed class MagicWeaponScript : BaseWeaponScript
    {
        public const Int32 Id = 0004;

        public MagicWeaponScript(BattleCalculator v)
            : base(v, CalcAttackBonus.Magic)
        {
        }
    }
}
