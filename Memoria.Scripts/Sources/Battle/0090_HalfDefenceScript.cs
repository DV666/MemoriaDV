using System;
using static Memoria.Scripts.Battle.TranceSeekCustomAPI;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// ???
    /// </summary>
    [BattleScript(Id)]
    public sealed class HalfDefenceScript : IBattleScript
    {
        public const Int32 Id = 0090;

        private readonly BattleCalculator _v;

        public HalfDefenceScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            btl_stat.AlterStatus(_v.Target, CustomStatusId.ArmorBreak, parameters: "+4");
            btl_stat.AlterStatus(_v.Target, CustomStatusId.MentalBreak, parameters: "+4");
        }
    }
}
