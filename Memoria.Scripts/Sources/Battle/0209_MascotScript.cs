using FF9;
using Memoria.Data;
using Memoria.Database;
using System;
using System.Collections.Generic;

namespace Memoria.Scripts.TranceSeek
{
    /// <summary>
    /// Special
    /// </summary>
    [BattleScript(Id)]
    public sealed class MascotScript : IBattleScript
    {
        public const Int32 Id = 0209;

        private readonly BattleCalculator _v;

        public MascotScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            switch (_v.Caster.Accessory)
            {
                case TranceSeekRegularItem.Mini_FriendlyFeatherCircle:
                    _v.Target.AlterStatus(BattleStatus.Regen);
                    break;
            }
        }
    }
}


