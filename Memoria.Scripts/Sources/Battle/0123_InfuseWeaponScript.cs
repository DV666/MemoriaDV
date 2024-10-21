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
            TranceSeekCustomAPI.WeaponNewElement[_v.Target.Data] = _v.Command.Element;
            TranceSeekCustomAPI.WeaponNewStatus[_v.Target.Data] = _v.Command.AbilityStatus;
        }
    }
}
