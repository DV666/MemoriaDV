using System;
using System.Runtime.Remoting.Contexts;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Lancer
    /// </summary>
    [BattleScript(Id)]
    public sealed class Rollback : IBattleScript
    {
        public const Int32 Id = 0117;

        private readonly BattleCalculator _v;

        public Rollback(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Command.Power == 0 && TranceSeekAPI.RollBackStats[_v.Target.Data][0] == 0)
            {
                TranceSeekAPI.RollBackStats[_v.Target.Data][0] = 1;
                TranceSeekAPI.RollBackStats[_v.Target.Data][1] = (int)_v.Target.CurrentHp;
                TranceSeekAPI.RollBackStats[_v.Target.Data][2] = (int)_v.Target.CurrentMp;
                TranceSeekAPI.RollBackStats[_v.Target.Data][3] = _v.Target.Strength;
                TranceSeekAPI.RollBackStats[_v.Target.Data][4] = _v.Target.Magic;
                TranceSeekAPI.RollBackStats[_v.Target.Data][5] = _v.Target.Will;
                TranceSeekAPI.RollBackStats[_v.Target.Data][6] = _v.Target.PhysicalDefence;
                TranceSeekAPI.RollBackStats[_v.Target.Data][7] = _v.Target.PhysicalEvade;
                TranceSeekAPI.RollBackStats[_v.Target.Data][8] = _v.Target.MagicDefence;
                TranceSeekAPI.RollBackStats[_v.Target.Data][9] = _v.Target.MagicEvade;
                TranceSeekAPI.RollBackStats[_v.Target.Data][10] = _v.Target.Trance;
                TranceSeekAPI.RollBackBattleStatus[_v.Target.Data] = _v.Target.CurrentStatus;
            }
            else if (_v.Command.Power == 1 && TranceSeekAPI.RollBackStats[_v.Target.Data][0] == 1)
            {
                TranceSeekAPI.RollBackStats[_v.Target.Data][0] = 0;
                _v.Target.CurrentHp = (uint)TranceSeekAPI.RollBackStats[_v.Target.Data][1];
                _v.Target.CurrentMp = (uint)TranceSeekAPI.RollBackStats[_v.Target.Data][2];
                _v.Target.Strength = (byte)TranceSeekAPI.RollBackStats[_v.Target.Data][3];
                _v.Target.Magic = (byte)TranceSeekAPI.RollBackStats[_v.Target.Data][4];
                _v.Target.Will = (byte)TranceSeekAPI.RollBackStats[_v.Target.Data][5];
                _v.Target.PhysicalDefence = (byte)TranceSeekAPI.RollBackStats[_v.Target.Data][6];
                _v.Target.PhysicalEvade = (byte)TranceSeekAPI.RollBackStats[_v.Target.Data][7];
                _v.Target.MagicDefence = (byte)TranceSeekAPI.RollBackStats[_v.Target.Data][8];
                _v.Target.MagicEvade = (byte)TranceSeekAPI.RollBackStats[_v.Target.Data][9];
                if ((TranceSeekAPI.RollBackBattleStatus[_v.Target.Data] & BattleStatus.Trance) == 0 && _v.Target.IsUnderStatus(BattleStatus.Trance))
                    _v.Target.RemoveStatus(BattleStatus.Trance);
                _v.Target.AlterStatus(TranceSeekAPI.RollBackBattleStatus[_v.Target.Data], _v.Caster);
                _v.Target.Trance = (byte)TranceSeekAPI.RollBackStats[_v.Target.Data][10];
            }
        }
    }
}
