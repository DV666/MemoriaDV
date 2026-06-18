using FF9;
using Memoria.Data;
using Memoria.Scripts.TranceSeek;
using System;
using System.Linq;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.Slow)]
    public class SlowStatusScript : StatusScriptBase
    {
        private Boolean SlowAnimFeature = Configuration.Mod.FolderNames.Contains("TranceSeek/Options/HasteSlowFeature");

        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            if (target.IsUnderStatus(BattleStatus.Haste))
            {
                if (btl_stat.RemoveStatus(target, BattleStatusId.Haste) == 2)
                    return btl_stat.ALTER_SUCCESS_NO_SET;
                return btl_stat.ALTER_RESIST;
            }
            btl_para.SetupATBCoef(target, btl_para.GetATBCoef() * 2 / 3);
            target.UISpriteATB = BattleHUD.ATEGray;

            if (SlowAnimFeature)
                target.AddDelayedModifier(HalfSpeedAnimation, null);

            TranceSeekAPI.SA_StatusApply(inflicter, false);
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            btl_para.SetupATBCoef(Target, btl_para.GetATBCoef());
            Target.UISpriteATB = Target.IsUnderAnyStatus(BattleStatus.Stop) ? BattleHUD.ATEGray : Target.IsUnderAnyStatus(BattleStatus.Haste) ? BattleHUD.ATEOrange : BattleHUD.ATENormal;
            return true;
        }

        private Boolean HalfSpeedAnimation(BattleUnit target)
        {
            if (!target.IsUnderAnyStatus(BattleStatus.Slow))
            {
                target.Data.animSpeed = 1f;
                return false;
            }

            BattlePlayerCharacter.PlayerMotionIndex currentAnim = btl_mot.getMotion(target.Data);

            if (btl_mot.IsLoopingMotion(currentAnim))
                target.Data.animSpeed = 0.5f;
            else
                target.Data.animSpeed = 1f;

            return true;
        }
    }
}
