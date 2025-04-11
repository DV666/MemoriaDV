using System;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Soul Blade
    /// </summary>
    [BattleScript(Id)]
    public sealed class SoulBladeScript : IBattleScript
    {
        public const Int32 Id = 0046;

        private readonly BattleCalculator _v;

        public SoulBladeScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            _v.Command.AbilityStatus |= _v.Caster.WeaponStatus;
            if (ff9item._FF9Item_Data[_v.Caster.Weapon].shape != 2 || _v.Command.AbilityStatus == 0) // Shape 1 => Dagger, Shape 2 => Thief Sword
            {
                _v.Context.Flags |= BattleCalcFlags.Miss;
                return;
            }

            if (!_v.Target.IsPlayer)
            {
                if ((_v.Command.AbilityStatus & BattleStatus.Death) == 0 || TranceSeekCustomAPI.CheckUnsafetyOrGuard(_v))
                    TranceSeekCustomAPI.TryAlterCommandStatuses(_v);
            else
                if (_v.Target.IsUnderStatus(_v.Command.AbilityStatus))
                    _v.Target.RemoveStatus(_v.Command.AbilityStatus);
                else
                    _v.Context.Flags |= BattleCalcFlags.Miss;
            }
        }
    }
}
