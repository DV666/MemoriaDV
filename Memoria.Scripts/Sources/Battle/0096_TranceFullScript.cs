using System;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Trance Full, Transfer
    /// </summary>
    [BattleScript(Id)]
    public sealed class TranceFullScript : IBattleScript
    {
        public const Int32 Id = 0096;

        private readonly BattleCalculator _v;

        public TranceFullScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            if (!_v.Target.IsPlayer)
            {
                _v.Target.ResistStatus &= ~BattleStatus.Trance;
                btl_stat.MakeStatusesPermanent(_v.Target, _v.Target.PermanentStatus | BattleStatus.Trance, true);
            }
            else
            {
                if (!_v.Target.HasTrance)
                {
                    _v.Context.Flags |= BattleCalcFlags.Miss;
                    return;
                }
                _v.Target.Trance = Byte.MaxValue;
            }
            _v.Target.AlterStatus(BattleStatus.Trance);
            TranceSeekCustomAPI.SpecialSA(_v);
        }
    }
}
