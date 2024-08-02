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
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            BattleStatus status = _v.Caster.WeaponStatus;
            if (ff9item._FF9Item_Data[_v.Caster.Weapon].shape != 2 || status == 0) // Shape 1 => Dagger, Shape 2 => Thief Sword
            {
                _v.Context.Flags |= BattleCalcFlags.Miss;
                return;
            }

            if (!_v.Target.IsPlayer)
            {
                if ((status & BattleStatus.Death) == 0 || _v.Target.CheckUnsafetyOrGuard())
                    _v.Target.TryAlterStatuses(status, true, _v.Caster);
            }
            else
            {
                if (_v.Target.IsUnderStatus(status))
                {
                    _v.Target.RemoveStatus(status);
                }
                else
                {
                    _v.Context.Flags |= BattleCalcFlags.Miss;
                }
            }
            TranceSeekCustomAPI.SpecialSA(_v);
        }
    }
}
