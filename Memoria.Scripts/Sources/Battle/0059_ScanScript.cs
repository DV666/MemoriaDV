using System;
using FF9;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Scan
    /// </summary>
    [BattleScript(Id)]
    public sealed class ScanScript : IBattleScript
    {
        public const Int32 Id = 0059;

        private readonly BattleCalculator _v;

        public ScanScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Caster.IsPlayer)
            {
                if (_v.Command.AbilityId == (BattleAbilityId)1075) // Lani - Predator's Eye
                {
                    _v.Target.Libra(BattleHUD.LibraInformation.NameLevel | BattleHUD.LibraInformation.Category | BattleHUD.LibraInformation.ElementalAffinities | BattleHUD.LibraInformation.StatusAffinities);
                }
                else if (_v.Target.IsUnderStatus(BattleStatus.EasyKill) && (btl_util.getEnemyPtr(_v.Target).info.flags & 128) == 0) // Si Unused(8) coché, affiche un scan normal
                {
                    _v.Target.Libra(BattleHUD.LibraInformation.Name | BattleHUD.LibraInformation.Level | BattleHUD.LibraInformation.Category | BattleHUD.LibraInformation.ItemSteal);
                }
                else
                {
                    _v.Target.Libra(BattleHUD.LibraInformation.Default | BattleHUD.LibraInformation.ItemSteal);
                }
            }
            else
            {
                if (_v.Command.Power == 1)
                {
                    _v.Target.Libra(BattleHUD.LibraInformation.Default);
                }
            }
        }
    }
}
