using FF9;
using Memoria.Data;
using Memoria.Scripts.TranceSeek;
using System;
using System.Linq;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.Haste)]
    public class HasteStatusScript : StatusScriptBase
    {
        private Boolean HasteAnimFeature = Configuration.Mod.FolderNames.Contains("TranceSeek/Options/HasteSlowFeature");

        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            if (target.IsUnderStatus(BattleStatus.Slow))
            {
                if (btl_stat.RemoveStatus(target, BattleStatusId.Slow) == 2)
                    return btl_stat.ALTER_SUCCESS_NO_SET;
                return btl_stat.ALTER_RESIST;
            }
            btl_para.SetupATBCoef(target, btl_para.GetATBCoef() * 3 / 2);
            target.UISpriteATB = Target.IsUnderAnyStatus(BattleStatus.Stop) ? BattleHUD.ATEGray : BattleHUD.ATEOrange;

            if (HasteAnimFeature)
                target.AddDelayedModifier(DoubleSpeedAnimation, null);

            TranceSeekAPI.SA_StatusApply(inflicter, true);
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            btl_para.SetupATBCoef(Target, btl_para.GetATBCoef());
            Target.UISpriteATB = Target.IsUnderAnyStatus(BattleStatus.Stop | BattleStatus.Slow) ? BattleHUD.ATEGray : BattleHUD.ATENormal;
            return true;
        }

        private Boolean DoubleSpeedAnimation(BattleUnit target)
        {
            if (!target.IsUnderAnyStatus(BattleStatus.Haste))
            {
                target.Data.animSpeed = 1f;
                return false;
            }

            BattlePlayerCharacter.PlayerMotionIndex currentAnim = btl_mot.getMotion(target.Data);

            if (btl_mot.IsLoopingMotion(currentAnim))
                target.Data.animSpeed = 2F;
            else
                target.Data.animSpeed = 1f;

            return true;
        }
    }
}
