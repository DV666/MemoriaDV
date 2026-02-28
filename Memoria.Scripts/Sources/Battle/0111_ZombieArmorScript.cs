using Memoria.Data;
using System;

namespace Memoria.Scripts.Battle
{
    [BattleScript(Id)]
    public sealed class ZombieArmorScript : IBattleScript
    {
        public const Int32 Id = 0111;

        private readonly BattleCalculator _v;

        public ZombieArmorScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Caster.Data.dms_geo_id == 7 && _v.Command.Power == 66 && _v.Command.HitRate == 66) // Siamois - Zombie Armor
            {
                _v.Target.TryAlterSingleStatus(BattleStatusId.CustomStatus10, true, _v.Caster, -1);
            }
            else
            {
                var Target_TSVar = _v.TargetState();
                _v.Target.Flags |= (CalcFlag.HpAlteration);
                if (_v.Command.Power == 99)
                    _v.Target.HpDamage = 9999;
                else
                    _v.Target.HpDamage = Target_TSVar.Monster.Special1;
                Int32 wait = 20;
                _v.Caster.AddDelayedModifier(
                    caster => (wait -= BattleState.ATBTickCount) > 0,
                    caster =>
                    {
                        Target_TSVar.Monster.Special1 = 0;
                    }
                );
            }
        }
    }
}
