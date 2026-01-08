using System;
using Memoria.Data;

namespace Memoria.EchoS
{
    public class BattleSpeakerEx : BattleVoice.BattleSpeaker
    {
        public bool Equals(BattleVoice.BattleSpeaker speaker)
        {
            if ((int)speaker.playerId >= 0 || (int)this.playerId >= 0)
            {
                return speaker.playerId == this.playerId;
            }

            return speaker.enemyModelId == this.enemyModelId &&
                   (speaker.enemyBattleId == -1 || this.enemyBattleId == -1 || speaker.enemyBattleId == this.enemyBattleId);
        }

        public override string ToString()
        {
            string statusText = (Status != BattleStatusId.None) ? $":{Status}" : "";
            string prefix = (!CheckCanTalk ? "$" : "") + (!CheckIsPlayer ? "!" : "") + (Without ? "\\" : "");

            if (playerId != CharacterId.NONE)
            {
                return $"{prefix}{playerId}{statusText}";
            }

            string enemyBId = (enemyBattleId >= 0) ? enemyBattleId.ToString() : "";
            string enemyMId = (enemyModelId >= 0) ? enemyModelId.ToString() : "";

            return $"{prefix}{enemyBId}:{enemyMId}{statusText}";
        }

        public BattleStatusId Status = BattleStatusId.None;

        public bool CheckCanTalk = true;

        public bool CheckIsPlayer = true;

        public bool Without;
    }
}
