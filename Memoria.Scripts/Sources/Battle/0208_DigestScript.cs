using FF9;
using Memoria.Data;
using System;
using UnityEngine;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// ???
    /// </summary>
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
