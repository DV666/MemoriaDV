using System;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Special
    /// </summary>
    [BattleScript(Id)]
    public sealed class InfuseWeaponScript : IBattleScript
    {
        public const Int32 Id = 0123;

        private readonly BattleCalculator _v;

        public InfuseWeaponScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if ((_v.Target.WeaponElement & _v.Command.Element) != 0)
                TranceSeekCustomAPI.WeaponNewElement[_v.Target.Data] = _v.Command.Element;
            if ((_v.Target.WeaponStatus & _v.Command.AbilityStatus) != 0)
                TranceSeekCustomAPI.WeaponNewStatus[_v.Target.Data] = _v.Command.AbilityStatus;
        }
    }
}
