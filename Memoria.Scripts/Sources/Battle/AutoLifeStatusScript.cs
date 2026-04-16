using Memoria.Data;
using Memoria.Scripts.TranceSeek;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.AutoLife)]
    public class AutoLifeStatusScript : StatusScriptBase, IDeathChangerStatusScript
    {
        public UInt32 HPRestore = 1;
        private SPSEffect _sps;

        private static readonly Dictionary<CharacterId, Vector3> CharacterOffsets = new Dictionary<CharacterId, Vector3>
        {
            { CharacterId.Steiner,new Vector3(0, 100, -10) },
            { CharacterId.Freya,  new Vector3(0, 100, -25) }
        };

        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            HPRestore = Math.Max(HPRestore, parameters.Length > 0 ? (UInt32)parameters[0] : 1);
            TranceSeekAPI.SA_StatusApply(inflicter, true);

            _sps = HonoluluBattleMain.battleSPS.AddSequenceSPS(9, -1, 1f, true);
            if (_sps != null)
            {
                btl2d.GetIconPosition(target, btl2d.ICON_POS_HEAD, out Transform attachTransf, out Vector3 iconOff);
                _sps.charTran = target.Data.gameObject.transform;
                _sps.boneTran = attachTransf;
                _sps.rotMode = 1;
                _sps.rot = new Vector3(90f, 0, 0);
                _sps.useBattleFactors = false;
                _sps.scale = 3072;
                _sps.frameRate = 32;

                Vector3 Offset = new Vector3(0, 100, 0);
                if (target.IsPlayer && CharacterOffsets.TryGetValue(target.PlayerIndex, out Vector3 FixedOffset))
                    Offset = FixedOffset;

                _sps.posOffset = Offset;
            }
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            UnloadSPS();
            return true;
        }

        public Boolean OnDeath()
        {
            btl_stat.RemoveStatus(Target, BattleStatusId.AutoLife);
            if (HPRestore > 0 && !Target.IsUnderAnyStatus(BattleStatusId.Zombie))
            {
                Target.CurrentHp = Math.Min(HPRestore, Target.MaximumHp);
                btl_stat.RemoveStatus(Target, BattleStatusId.Death);
            }
            BattleVoice.TriggerOnStatusChange(Target, BattleVoice.BattleMoment.Used, BattleStatusId.AutoLife);

            UnloadSPS();
            return true;
        }

        private void UnloadSPS()
        {
            if (_sps != null)
            {
                _sps.Unload();
                _sps = null;
            }
        }
    }
}
