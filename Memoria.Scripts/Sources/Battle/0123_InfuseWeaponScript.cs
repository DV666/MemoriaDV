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
            TranceSeekAPI.WeaponNewElement[_v.Target.Data] = _v.Command.Element;
            TranceSeekAPI.WeaponNewStatus[_v.Target.Data] = _v.Command.AbilityStatus;
            TranceSeekAPI.ViviPreviousSpell[_v.Target.Data] = _v.Command.AbilityId;
        }
    }
}
