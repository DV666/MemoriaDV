using System;
using System.Collections.Generic;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Absorb Magic
    /// </summary>
    [BattleScript(Id)]
    public sealed class AbsorbMagicScript : IBattleScript
    {
        public const Int32 Id = 0094;

        private readonly BattleCalculator _v;

        public AbsorbMagicScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            int absorb = _v.Target.Magic / _v.Command.Power;
            _v.Target.Magic = (byte)Math.Max(0, (_v.Target.Magic - absorb));
            Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                    {
                        { "US", "↓ Magic ↓" },
                        { "UK", "↓ Magic ↓" },
                        { "JP", "↓ まりょく！ ↓" },
                        { "ES", "↓ POT magico ↓" },
                        { "FR", "↓ Magie ↓" },
                        { "GR", "↓ Magia ↓" },
                        { "IT", "↓ Zauber ↓" },
                    };
            btl2d.Btl2dReqSymbolMessage(_v.Target.Data, "[FFA500]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 48);
            _v.Caster.Magic = (byte)Math.Min(99, (_v.Caster.Magic + absorb));
            Dictionary<String, String> localizedMessage2 = new Dictionary<String, String>
                    {
                        { "US", "↑ Magic ↑" },
                        { "UK", "↑ Magic ↑" },
                        { "JP", "↑ まりょく！ ↑" },
                        { "ES", "↑ POT magico ↑" },
                        { "FR", "↑ Magie ↑" },
                        { "GR", "↑ Magia ↑" },
                        { "IT", "↑ Zauber ↑" },
                    };
            btl2d.Btl2dReqSymbolMessage(_v.Caster.Data, "[FFA500]", localizedMessage2, HUDMessage.MessageStyle.DAMAGE, 48);
            TranceSeekCustomAPI.SpecialSA(_v);
        }
    }
}