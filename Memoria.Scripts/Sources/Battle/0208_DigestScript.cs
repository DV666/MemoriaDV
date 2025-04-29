using Memoria.Data;
using System;
using System.Collections.Generic;

namespace Memoria.Scripts.Battle
{

    [BattleScript(Id)]
    public sealed class DigestScript : IBattleScript
    {
        public const Int32 Id = 0208;

        private readonly BattleCalculator _v;

        public DigestScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform() // [TODO] Need to rework this and use Memoria Dictionnary instead.
        {
            if (!FF9StateSystem.EventState.gScriptDictionary.TryGetValue(20000, out Dictionary<Int32, Int32> dict)) // Init
            {
                dict = new Dictionary<Int32, Int32>();
                dict[0] = 0; // 0 = WhiteMagic
                dict[1] = 0; // 1 = BlackMagic
                dict[2] = 0; // 2 = PreviousBlackMagic element (for Zwerchhau & Redoublement)
                dict[3] = 0; // 3 = PreviousBlackMagic power (for Zwerchhau & Redoublement)
                dict[4] = 0; // 4 = Prevent trigger for multi target.
                FF9StateSystem.EventState.gScriptDictionary.Add(20000, dict); // Create the MemoriaDictionary with ID 20000
                _v.Caster.AddDelayedModifier(
                    caster => FF9StateSystem.Common.FF9.btl_result == 0, // Reset value when fight ends (i suppose that's works...)
                    caster =>
                    {
                        dict[0] = 0;
                        dict[1] = 0;
                        dict[2] = 0;
                        dict[3] = 0;
                        FF9StateSystem.EventState.gScriptDictionary.Remove(20000); // Remove the MemoriaDictionary with ID 20000, to refresh it for next battle.
                    }
                );
            }

            _v.Target.Flags |= (CalcFlag.HpDamageOrHeal | CalcFlag.MpDamageOrHeal);
            _v.Target.HpDamage = (FF9StateSystem.EventState.gEventGlobal[1320] * 256) + FF9StateSystem.EventState.gEventGlobal[1321];
            _v.Target.MpDamage = (FF9StateSystem.EventState.gEventGlobal[1322] * 256) + FF9StateSystem.EventState.gEventGlobal[1323];
            if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1223)) // SA Voracious +
            {
                _v.Target.HpDamage = (int)Math.Min(_v.Target.MaximumHp, _v.Target.HpDamage);
                _v.Target.MpDamage = (int)Math.Min(_v.Target.MaximumMp, _v.Target.MpDamage);
            }
            else
            {
                _v.Target.HpDamage = (int)Math.Min(_v.Target.MaximumHp / 2, _v.Target.HpDamage);  
                _v.Target.MpDamage = (int)Math.Min(_v.Target.MaximumMp / 2, _v.Target.MpDamage);
            }
            FF9StateSystem.EventState.gEventGlobal[1320] = 0;
            FF9StateSystem.EventState.gEventGlobal[1321] = 0;
            FF9StateSystem.EventState.gEventGlobal[1322] = 0;
            FF9StateSystem.EventState.gEventGlobal[1323] = 0;
        }
    }
}
