using System;
using Memoria.Data;
using Memoria.Scripts.Battle;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.Venom)]
    public class VenomStatusScript : StatusScriptBase, IOprStatusScript
    {
        public BattleUnit VenomInflicter = null;
        public Int32 SpeedTick = 0;

        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            VenomInflicter = inflicter;
            if (target.IsUnderAnyStatus(BattleStatus.EasyKill))
            {
                BattleStatusDataEntry statusData = FF9StateSystem.Battle.FF9Battle.status_data[BattleStatusId.Poison];
                Int32 wait = (short)(((400 + (inflicter.Will * 2) - target.Will) * statusData.ContiCnt) * (inflicter.HasSupportAbilityByIndex((SupportAbility)1124) ? (150 / 100) : inflicter.HasSupportAbilityByIndex((SupportAbility)124) ? (125 / 100) : 1));
                target.AddDelayedModifier(
                target => (wait -= target.Data.cur.at_coef * BattleState.ATBTickCount) > 0,
                target =>
                {
                    target.RemoveStatus(BattleStatus.Venom);
                }
                );
            }
            TranceSeekAPI.SA_StatusApply(inflicter, false);
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            return true;
        }

        public IOprStatusScript.SetupOprMethod SetupOpr => SetupVenomOpr;
        public Int32 SetupVenomOpr()
        {
            if (Target.IsUnderAnyStatus(BattleStatus.EasyKill))
                return 600;
            else
                return FF9StateSystem.EventState.gEventGlobal[1407] > 0 ? 100 : Math.Max(100, (600 - SpeedTick));
        }
        public Boolean OnOpr()
        {
            if (Target.IsUnderAnyStatus(BattleStatus.Petrify))
                return false;
            uint TargetMaxHP = TranceSeekBattleDictionary.MonsterMechanic[Target.Data][3] != 0 ? (Target.MaximumHp - 10000) : Target.MaximumHp;
            UInt32 HPdamage = (UInt32)Math.Round(Target.IsUnderAnyStatus(BattleStatus.EasyKill) ? (TargetMaxHP / 128.0) : (TargetMaxHP / 16.0));
            UInt32 MPdamage = 0;
            if (!Target.IsUnderAnyStatus(BattleStatus.EasyKill))
            {
                MPdamage = Target.MaximumMp >> 5;
                if (Target.IsZombie)
                    MPdamage /= 2;
                if (Target.CurrentMp > MPdamage)
                    Target.CurrentMp -= MPdamage;
                else
                    Target.CurrentMp = 0;
            }
            if (Target.IsZombie)
                HPdamage /= 2;
            if (Target.CurrentHp > HPdamage)
                Target.CurrentHp -= HPdamage;
            else
                Target.Kill(VenomInflicter);
            btl2d.Btl2dStatReq(Target, (Int32)HPdamage, (Int32)MPdamage);
            SpeedTick += 100;
            BattleVoice.TriggerOnStatusChange(Target, BattleVoice.BattleMoment.Used, BattleStatusId.Venom);
            return false;
        }
    }
}
