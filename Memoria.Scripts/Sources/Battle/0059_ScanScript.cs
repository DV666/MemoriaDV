using System;
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
                if (!_v.Target.IsUnderStatus(BattleStatus.EasyKill))
                {
                    _v.Target.Libra(BattleHUD.LibraInformation.Default | BattleHUD.LibraInformation.ItemSteal);
                }
                else
                {
                    _v.Target.Libra(BattleHUD.LibraInformation.Name | BattleHUD.LibraInformation.Level | BattleHUD.LibraInformation.Category | BattleHUD.LibraInformation.ItemSteal);
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
