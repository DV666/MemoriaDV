using FF9;
using Memoria.Data;
using System;
using System.Collections.Generic;

namespace Memoria.Scripts.Battle
{
    [BattleScript(Id)]
    public sealed class MagicHammerScript : IBattleScript
    {
        public const Int32 Id = 0142;

        private readonly BattleCalculator _v;

        public MagicHammerScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Target.IsZombie)
            {
                _v.Target.Flags |= (CalcFlag.MpAlteration | CalcFlag.MpRecovery);
                _v.Caster.Flags |= CalcFlag.MpAlteration;
            }
            else
            {
                _v.Target.Flags |= CalcFlag.MpAlteration;
                _v.Caster.Flags |= (CalcFlag.MpAlteration | CalcFlag.MpRecovery);
            }
            if (_v.Target.CurrentMp > 0U)
            {
                int num = (_v.Caster.Magic + Comn.random16() % (1 + (_v.Caster.Level + _v.Caster.Magic) / 3));
                if (num > _v.Target.CurrentMp)
                {
                    num = (int)_v.Target.CurrentMp;
                }

                if (_v.Target.IsUnderStatus(BattleStatus.Shell))
                    num /= 2;

                if (_v.Command.IsManyTarget && !_v.Caster.HasSupportAbilityByIndex((SupportAbility)1126))
                    num /= 2;

                _v.Target.MpDamage = num;
                _v.Caster.MpDamage = num;
            }
            TranceSeekAPI.MagicAccuracy(_v);
            TranceSeekAPI.TryAlterMagicStatuses(_v);
        }
    }
}

