using Memoria.Data;
using System;

namespace Memoria.Scripts.Battle
{
    [BattleScript(Id)]
    public sealed class MagicWeaponSpecialScript : IBattleScript
    {
        public const Int32 Id = 0141;

        private readonly BattleCalculator _v;

        public MagicWeaponSpecialScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            Boolean CantReflect = (_v.Caster.Weapon == (RegularItem)1031 && _v.Command.Id == (BattleCommandId)1052);
            if (_v.Target.IsUnderAnyStatus(BattleStatus.Reflect) && _v.Command.Data.info.effect_counter == 1 && !CantReflect)
                SFXChannel.PlayReflectEffect(_v.Target.Id, 5);

            if (_v.Command.Data.info.effect_counter >= 2)
            {
                int ScriptId = 0;
                switch (_v.Caster.Weapon)
                {
                    case RegularItem.StardustRod:
                    case RegularItem.FlameStaff:
                    case RegularItem.IceStaff:
                    case RegularItem.LightningStaff:
                    {
                        ScriptId = 9; // Script 0009_MagicAttackScript.cs
                        _v.Command.Power = _v.Command.Id == (BattleCommandId)1051 ? 29 : 14;
                        _v.Command.Element |= _v.Caster.WeaponElement;
                        if (_v.Caster.Weapon == RegularItem.StardustRod)
                            _v.Command.Element |= EffectElement.Darkness;
                        break;
                    }
                    case (RegularItem)1028: // Atomos' Scepter
                    {
                        ScriptId = 17; // Script 0017_MagicGravityDamageScript.cs
                        _v.Command.Power = _v.Command.Id == (BattleCommandId)1052 ? 75 : 25;
                        break;
                    }
                    case (RegularItem)1029: // Ivy's Scepter
                    {
                        ScriptId = 118; // Script 0118_PoisonMagicAttackScript.cs
                        _v.Command.Power = _v.Command.Id == (BattleCommandId)1052 ? 67 : 19;
                        _v.Command.HitRate = _v.Command.Id == (BattleCommandId)1052 ? 25 : 40;
                        _v.Command.AbilityStatus |= _v.Command.Id == (BattleCommandId)1052 ? BattleStatus.Venom : BattleStatus.Poison;
                        break;
                    }
                    case (RegularItem)1030: // Ankou's Scepter
                    {
                        ScriptId = 14; // Script 0014_DeathScript.cs
                        _v.Command.HitRate = 30;
                        _v.Command.AbilityStatus |= BattleStatus.Death;
                        break;
                    }
                    case (RegularItem)1031: // Stardust Scepter
                    {
                        ScriptId = 116; // Script 0116_LowRandomMagic.cs
                        _v.Command.Power = _v.Command.Id == (BattleCommandId)1052 ? 109 : 42;
                        break;
                    }
                }
                BattleScriptFactory factoryattack = SBattleCalculator.FindScriptFactory(ScriptId); 
                if (factoryattack != null)
                {
                    IBattleScript script = factoryattack(_v);
                    script.Perform();
                }
            }
            else
            {
                BattleScriptFactory factoryattack = SBattleCalculator.FindScriptFactory(1); // Script 0001_SimpleWeaponScript.cs
                if (factoryattack != null)
                {
                    IBattleScript script = factoryattack(_v);
                    script.Perform();
                }
            }
        }
    }
}

