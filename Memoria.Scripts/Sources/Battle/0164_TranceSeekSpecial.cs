using System;
using FF9;
using Memoria.Data;
using UnityEngine;
using System.Collections.Generic;
using Memoria.Assets;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Special
    /// </summary>
    [BattleScript(Id)]
    public sealed class TranceSeekSpecial : IBattleScript
    {
        public const Int32 Id = 0164;

        private readonly BattleCalculator _v;

        public TranceSeekSpecial(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            foreach (BattleStatusId statusId in _v.Target.Data.stat.cur.ToStatusList())
                btl_stat.RemoveStatus(_v.Target, statusId);
            _v.Context.Flags = 0;
        }
    }
}
