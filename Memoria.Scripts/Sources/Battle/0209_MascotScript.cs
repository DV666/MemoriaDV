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
            int ScriptId = 0;

            switch (_v.Caster.Accessory)
            {
                case TranceSeekRegularItem.Mini_FriendlyFeatherCircle:
                    _v.Target.AlterStatus(BattleStatus.Regen);
                break;
                case TranceSeekRegularItem.Mini_Dracozombie:
                    _v.Command.Power = 41;
                    _v.Command.HitRate = 35;
                    _v.Command.AbilityStatus = BattleStatus.Zombie;
                    ScriptId = 119;
                    break;
                case TranceSeekRegularItem.Mini_Grenade:
                    _v.Command.Power = 10;
                    _v.Command.Element = EffectElement.Fire;
                    ScriptId = 18;
                    break;
                case TranceSeekRegularItem.Mini_Mandragora:
                    ScriptId = 29;
                    break;
                case TranceSeekRegularItem.Mini_Clipper:
                    btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.ArmorUp, parameters: $"+2");
                    break;
            }

            if (ScriptId != 0)
            {
                BattleScriptFactory factoryattack = SBattleCalculator.FindScriptFactory(ScriptId);
                if (factoryattack != null)
                {
                    IBattleScript script = factoryattack(_v);
                    script.Perform();
                }
            }
        }
    }
}


