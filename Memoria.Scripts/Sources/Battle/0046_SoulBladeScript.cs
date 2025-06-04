using System;
using Memoria.Data;
using Memoria.Prime;

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
            _v.Command.AbilityStatus = _v.Caster.WeaponStatus;
            if (ff9item._FF9Item_Data[_v.Caster.Weapon].shape != 2 || _v.Command.AbilityStatus == 0) // Shape 1 => Dagger, Shape 2 => Thief Sword
            {
                _v.Context.Flags |= BattleCalcFlags.Miss;
                return;
            }

            if (!_v.Target.IsPlayer)
            {
                if ((_v.Command.AbilityStatus & BattleStatus.Death) == 0 && TranceSeekAPI.CheckUnsafetyOrGuard(_v))
                    TranceSeekAPI.TryAlterCommandStatuses(_v);
                else
                    _v.Context.Flags |= BattleCalcFlags.Miss;
            }
            else
            {
                if  ((_v.Command.AbilityStatus & BattleStatus.Death) != 0)
                {
                    if (!_v.Target.CanBeRevived())
                        return;

                    if (_v.Target.IsZombie && !_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill))
                    {
                        if ((_v.Target.CurrentHp = (UInt32)(GameRandom.Next8() % 10)) == 0)
                            _v.Target.Kill();
                    }
                    else if (_v.Target.CheckIsPlayer())
                    {
                        if (_v.Target.IsUnderStatus(BattleStatus.Death))
                            if (_v.Target.HasSupportAbilityByIndex((SupportAbility)1004)) // Invincible+
                            {
                                _v.Target.Flags |= CalcFlag.HpAlteration | CalcFlag.HpRecovery | CalcFlag.MpAlteration | CalcFlag.MpRecovery;
                                _v.Target.HpDamage = (int)_v.Target.MaximumHp;
                                _v.Target.MpDamage = (int)_v.Target.MaximumMp;
                            }
                            else
                            {
                                _v.Target.CurrentHp = (UInt32)(1 + GameRandom.Next8() % 10);
                            }
                    }
                }
                TranceSeekAPI.TryRemoveAbilityStatuses(_v);
            }              
        }
    }
}
