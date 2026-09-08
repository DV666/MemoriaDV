using System;
using Memoria.Data;
using Memoria.Prime;

namespace Memoria.Scripts.TranceSeek
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
            if (TranceSeekCharacterMechanic.ZidaneDagger(_v.Caster) || _v.Command.AbilityStatus == 0)
            {
                _v.Context.Flags |= BattleCalcFlags.Miss;
                return;
            }

            if (!_v.Target.IsPlayer)
            {
                if ((_v.Command.AbilityStatus & BattleStatus.Death) == 0)
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
                    else if (_v.Target.CheckIsPlayer() && _v.Target.IsUnderStatus(BattleStatus.Death))
                        TranceSeekAPI.ReviveHeal(_v, (1 + GameRandom.Next8() % 10));
                }
                TranceSeekAPI.TryRemoveAbilityStatuses(_v);
            }              
        }
    }
}

