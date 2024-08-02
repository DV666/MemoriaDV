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
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            if (_v.Command.Power == 0 && TranceSeekCustomAPI.RollBackStats[_v.Target.Data][0] == 0)
            {
                TranceSeekCustomAPI.RollBackStats[_v.Target.Data][0] = 1;
                TranceSeekCustomAPI.RollBackStats[_v.Target.Data][1] = (int)_v.Target.CurrentHp;
                TranceSeekCustomAPI.RollBackStats[_v.Target.Data][2] = (int)_v.Target.CurrentMp;
                TranceSeekCustomAPI.RollBackStats[_v.Target.Data][3] = _v.Target.Strength;
                TranceSeekCustomAPI.RollBackStats[_v.Target.Data][4] = _v.Target.Magic;
                TranceSeekCustomAPI.RollBackStats[_v.Target.Data][5] = _v.Target.Will;
                TranceSeekCustomAPI.RollBackStats[_v.Target.Data][6] = _v.Target.PhysicalDefence;
                TranceSeekCustomAPI.RollBackStats[_v.Target.Data][7] = _v.Target.PhysicalEvade;
                TranceSeekCustomAPI.RollBackStats[_v.Target.Data][8] = _v.Target.MagicDefence;
                TranceSeekCustomAPI.RollBackStats[_v.Target.Data][9] = _v.Target.MagicEvade;
                TranceSeekCustomAPI.RollBackStats[_v.Target.Data][10] = _v.Target.Trance;
                TranceSeekCustomAPI.RollBackBattleStatus[_v.Target.Data] = _v.Target.CurrentStatus;
            }
            else if (_v.Command.Power == 1 && TranceSeekCustomAPI.RollBackStats[_v.Target.Data][0] == 1)
            {
                TranceSeekCustomAPI.RollBackStats[_v.Target.Data][0] = 0;
                _v.Target.CurrentHp = (uint)TranceSeekCustomAPI.RollBackStats[_v.Target.Data][1];
                _v.Target.CurrentMp = (uint)TranceSeekCustomAPI.RollBackStats[_v.Target.Data][2];
                _v.Target.Strength = (byte)TranceSeekCustomAPI.RollBackStats[_v.Target.Data][3];
                _v.Target.Magic = (byte)TranceSeekCustomAPI.RollBackStats[_v.Target.Data][4];
                _v.Target.Will = (byte)TranceSeekCustomAPI.RollBackStats[_v.Target.Data][5];
                _v.Target.PhysicalDefence = (byte)TranceSeekCustomAPI.RollBackStats[_v.Target.Data][6];
                _v.Target.PhysicalEvade = (byte)TranceSeekCustomAPI.RollBackStats[_v.Target.Data][7];
                _v.Target.MagicDefence = (byte)TranceSeekCustomAPI.RollBackStats[_v.Target.Data][8];
                _v.Target.MagicEvade = (byte)TranceSeekCustomAPI.RollBackStats[_v.Target.Data][9];
                if ((TranceSeekCustomAPI.RollBackBattleStatus[_v.Target.Data] & BattleStatus.Trance) == 0 && _v.Target.IsUnderStatus(BattleStatus.Trance))
                    _v.Target.RemoveStatus(BattleStatus.Trance);
                _v.Target.AlterStatus(TranceSeekCustomAPI.RollBackBattleStatus[_v.Target.Data], _v.Caster);
                _v.Target.Trance = (byte)TranceSeekCustomAPI.RollBackStats[_v.Target.Data][10];
            }
            TranceSeekCustomAPI.SpecialSA(_v);
        }
    }
}
