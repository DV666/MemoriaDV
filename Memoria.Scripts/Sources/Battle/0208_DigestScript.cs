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

        public void Perform()
        {
            if (!FF9StateSystem.EventState.gScriptDictionary.TryGetValue(1035, out Dictionary<Int32, Int32> dict))
            {
                dict = new Dictionary<Int32, Int32>();
                FF9StateSystem.EventState.gScriptDictionary.Add(1035, dict);
            }

            _v.Target.Flags |= (CalcFlag.HpDamageOrHeal | CalcFlag.MpDamageOrHeal);
            _v.Target.HpDamage = dict[0];
            _v.Target.MpDamage = dict[1];
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
            dict[0] = 0;
            dict[1] = 0;
        }
    }
}
