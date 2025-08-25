using Memoria.Data;
using System;
using System.Collections.Generic;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Detect
    /// </summary>
    [BattleScript(Id)]
    public sealed class DetectScript : IBattleScript
    {
        public const Int32 Id = 0060;

        private readonly BattleCalculator _v;

        public DetectScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            BattleEnemy battleEnemy = BattleEnemy.Find(_v.Target);
            if (!HasStealableItems(battleEnemy))
            {
                UiState.SetBattleFollowFormatMessage(BattleMesages.DoesNotHaveAnything, new object[0]);
            }
            else
            {
                if (_v.Target.PhysicalEvade == 255 || TranceSeekAPI.ZidanePassive[_v.Target.Data][2] > 0)
                {
                    _v.Context.Flags |= BattleCalcFlags.Miss;
                    return;
                }

                Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                {
                    { "US", "Thief's Eye!" },
                    { "UK", "Thief's Eye!" },
                    { "JP", "泥棒の目!" },
                    { "ES", "Ojo del ladrón!" },
                    { "FR", "Œil du voleur !" },
                    { "GR", "Auge des Diebes!" },
                    { "IT", "Occhio del ladro!" },
                };
                btl2d.Btl2dReqSymbolMessage(_v.Target.Data, "[FDEE00]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 5);
                TranceSeekAPI.ZidanePassive[_v.Target.Data][2] = 1;
            }
        }
        private static bool HasStealableItems(BattleEnemy enemy)
        {
            bool result = false;
            for (short num = 0; num < 4; num += 1)
            {
                if (enemy.StealableItems[num] != RegularItem.NoItem)
                {
                    result = true;
                }
            }
            return result;
        }
    }
}
