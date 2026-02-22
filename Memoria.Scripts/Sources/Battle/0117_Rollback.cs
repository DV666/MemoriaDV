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
            if (_v.Command.Power == 0 && TranceSeekBattleDictionary.RollBackStats[_v.Target.Data][0] == 0)
            {
                TranceSeekBattleDictionary.RollBackStats[_v.Target.Data][0] = 1;
                TranceSeekBattleDictionary.RollBackStats[_v.Target.Data][1] = (int)_v.Target.CurrentHp;
                TranceSeekBattleDictionary.RollBackStats[_v.Target.Data][2] = (int)_v.Target.CurrentMp;
                TranceSeekBattleDictionary.RollBackStats[_v.Target.Data][3] = _v.Target.Strength;
                TranceSeekBattleDictionary.RollBackStats[_v.Target.Data][4] = _v.Target.Magic;
                TranceSeekBattleDictionary.RollBackStats[_v.Target.Data][5] = _v.Target.Will;
                TranceSeekBattleDictionary.RollBackStats[_v.Target.Data][6] = _v.Target.PhysicalDefence;
                TranceSeekBattleDictionary.RollBackStats[_v.Target.Data][7] = _v.Target.PhysicalEvade;
                TranceSeekBattleDictionary.RollBackStats[_v.Target.Data][8] = _v.Target.MagicDefence;
                TranceSeekBattleDictionary.RollBackStats[_v.Target.Data][9] = _v.Target.MagicEvade;
                TranceSeekBattleDictionary.RollBackStats[_v.Target.Data][10] = _v.Target.Trance;
                TranceSeekBattleDictionary.RollBackBattleStatus[_v.Target.Data] = _v.Target.CurrentStatus;
            }
            else if (_v.Command.Power == 1 && TranceSeekBattleDictionary.RollBackStats[_v.Target.Data][0] == 1)
            {
                TranceSeekBattleDictionary.RollBackStats[_v.Target.Data][0] = 0;
                _v.Target.CurrentHp = (uint)TranceSeekBattleDictionary.RollBackStats[_v.Target.Data][1];
                _v.Target.CurrentMp = (uint)TranceSeekBattleDictionary.RollBackStats[_v.Target.Data][2];
                _v.Target.Strength = (byte)TranceSeekBattleDictionary.RollBackStats[_v.Target.Data][3];
                _v.Target.Magic = (byte)TranceSeekBattleDictionary.RollBackStats[_v.Target.Data][4];
                _v.Target.Will = (byte)TranceSeekBattleDictionary.RollBackStats[_v.Target.Data][5];
                _v.Target.PhysicalDefence = (byte)TranceSeekBattleDictionary.RollBackStats[_v.Target.Data][6];
                _v.Target.PhysicalEvade = (byte)TranceSeekBattleDictionary.RollBackStats[_v.Target.Data][7];
                _v.Target.MagicDefence = (byte)TranceSeekBattleDictionary.RollBackStats[_v.Target.Data][8];
                _v.Target.MagicEvade = (byte)TranceSeekBattleDictionary.RollBackStats[_v.Target.Data][9];
                if ((TranceSeekBattleDictionary.RollBackBattleStatus[_v.Target.Data] & BattleStatus.Trance) == 0 && _v.Target.IsUnderStatus(BattleStatus.Trance))
                    _v.Target.RemoveStatus(BattleStatus.Trance);
                _v.Target.AlterStatus(TranceSeekBattleDictionary.RollBackBattleStatus[_v.Target.Data], _v.Caster);
                _v.Target.Trance = (byte)TranceSeekBattleDictionary.RollBackStats[_v.Target.Data][10];
            }
        }
    }
}
