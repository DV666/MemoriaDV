using Memoria.Data;
using System;

namespace Memoria.Scripts.TranceSeek
{
    [BattleScript(Id)]
    public sealed class WillWeaponScript : BaseWeaponScript
    {
        public const Int32 Id = 0002;

        public WillWeaponScript(BattleCalculator v)
            : base(v, CalcAttackBonus.WillPower)
        {

        }
    }
}
