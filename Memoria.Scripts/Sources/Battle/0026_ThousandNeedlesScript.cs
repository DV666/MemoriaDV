using System;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// 1,000 Needles, Pyro, Medeo, Poly
    /// </summary>
    [BattleScript(Id)]
    public sealed class ThousandNeedlesScript : IBattleScript
    {
        public const Int32 Id = 0026;

        private readonly BattleCalculator _v;

        public ThousandNeedlesScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Command.HitRate == 111 || _v.Caster.PlayerIndex == CharacterId.Quina && _v.Command.AbilityId == (BattleAbilityId)1029) // ?000 epines
            {
                short num = (short)(GameRandom.Next8() % (_v.Caster.Level / 10) + 1);
                _v.Target.Flags |= CalcFlag.HpAlteration;
                _v.Target.HpDamage = ((short)(_v.Command.Power * 100) * num);
                if (_v.Caster.Data.dms_geo_id == 553 && _v.Command.Power == 6 && _v.Command.HitRate == 66)
                {
                    TranceSeekAPI.RaiseTrouble(_v);
                }
            }
            else
            {
                _v.Target.Flags |= CalcFlag.HpAlteration;
                _v.Target.HpDamage = (_v.Command.Power * 100 + _v.Command.HitRate);
            }
        }
    }
}
