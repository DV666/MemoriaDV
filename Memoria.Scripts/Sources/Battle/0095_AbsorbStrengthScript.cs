using System;
using System.Collections.Generic;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Absorb Strength
    /// </summary>
    [BattleScript(Id)]
    public sealed class AbsorbStrengthScript : IBattleScript
    {
        public const Int32 Id = 0095;

        private readonly BattleCalculator _v;

        public AbsorbStrengthScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            int absorb = _v.Target.Strength / _v.Command.Power;
            _v.Target.Strength = (byte)Math.Max(0, (_v.Target.Strength - absorb));
            Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                    {
                        { "US", "↓ Strength ↓" },
                        { "UK", "↓ Strength ↓" },
                        { "JP", "↓ ちから！ ↓" },
                        { "ES", "↓ Forza ↓" },
                        { "FR", "↓ Force ↓" },
                        { "GR", "↓ Fuerza ↓" },
                        { "IT", "↓ Stärke ↓" },
                    };
            btl2d.Btl2dReqSymbolMessage(_v.Target.Data, "[FFA500]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 48);
            _v.Caster.Strength = (byte)Math.Min(99, (_v.Caster.Strength + absorb));
            Dictionary<String, String> localizedMessage2 = new Dictionary<String, String>
                    {
                        { "US", "↑ Strength ↑" },
                        { "UK", "↑ Strength ↑" },
                        { "JP", "↑ ちから！ ↑" },
                        { "ES", "↑ Forza ↑" },
                        { "FR", "↑ Force ↑" },
                        { "GR", "↑ Fuerza ↑" },
                        { "IT", "↑ Stärke ↑" },
                    };
            btl2d.Btl2dReqSymbolMessage(_v.Caster.Data, "[FFA500]", localizedMessage2, HUDMessage.MessageStyle.DAMAGE, 48);
            TranceSeekCustomAPI.SpecialSA(_v);
        }
    }
}
