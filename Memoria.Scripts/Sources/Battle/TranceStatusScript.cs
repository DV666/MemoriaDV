﻿using System;
using Memoria.Data;
using FF9;
using Object = System.Object;
using static Memoria.Scripts.Battle.TranceSeekCustomAPI;
using Memoria.Prime;
using UnityEngine;

namespace Memoria.DefaultScripts
{
    // NOTE: this status is still specifically handled by the game in many aspects
    // You cannot recycle it for a completly different effect

    [StatusScript(BattleStatusId.Trance)]
    public class TranceStatusScript : StatusScriptBase, IFinishCommandScript
    {
        public BattleAbilityId PhantomAbility = BattleAbilityId.Void;
        public Boolean PhantomCommandSent = false;
        public Int32 PhantomCountdown = 0;
        public Boolean MarcusAbsorbDarkness = false;
        public Boolean MarcusWeakLight = false;

        public static Int32 GetPhantomCount(BattleUnit btl)
        {
            return (60 - btl.Will) * 200;
        }

        public static BattleAbilityId EidolonToPhantom(BattleAbilityId eidolonId)
        {
            switch (eidolonId)
            {
                case BattleAbilityId.Shiva: return BattleAbilityId.DiamondDust;
                case BattleAbilityId.Ifrit: return BattleAbilityId.FlamesofHell;
                case BattleAbilityId.Ramuh: return BattleAbilityId.JudgementBolt;
                case BattleAbilityId.Atomos: return BattleAbilityId.WormHole;
                case BattleAbilityId.Odin: return BattleAbilityId.Zantetsuken;
                case BattleAbilityId.Leviathan: return BattleAbilityId.Tsunami;
                case BattleAbilityId.Bahamut: return BattleAbilityId.MegaFlare;
                case BattleAbilityId.Ark: return BattleAbilityId.EternalDarkness;
            }
            return BattleAbilityId.Void;
        }

        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            btl_cmd.SetCommand(target.Data.cmd[4], BattleCommandId.SysTrans, 0, target.Id, 0u);
            if (!Target.IsPlayer && MonsterMechanic[Target.Data][0] == 0 && !Target.IsUnderAnyStatus(BattleStatus.EasyKill)) // +50% HP/MP Max if monster get under Trance
            {
                MonsterMechanic[Target.Data][0] = 1;
                Target.MaximumHp += (Target.MaximumHp / 2);
                Target.MaximumMp += (Target.MaximumMp / 2);
                Target.CurrentHp = Target.MaximumHp;
                Target.CurrentMp = Target.MaximumMp;
            }
            if (target.PlayerIndex == CharacterId.Garnet)
                target.AddDelayedModifier(ProcessPhantomRecast, ClearPhantomRecast);

            if (target.PlayerIndex == CharacterId.Marcus)
                target.AddDelayedModifier(
                    target => !target.IsDisappear,
                    target =>
                    {
                        target.Data.weapon_geo.SetActive(false);
                        target.Player.trance = target.Trance;
                        target.MaximumMp *= 2;
                        target.CurrentMp = target.MaximumMp;
                        if ((target.AbsorbElement & EffectElement.Darkness) == 0)
                        {
                            MarcusAbsorbDarkness = true;
                            target.AbsorbElement |= EffectElement.Darkness;
                        }
                        if ((target.WeakElement & EffectElement.Holy) == 0)
                        {
                            MarcusWeakLight = true;
                            target.WeakElement |= EffectElement.Holy;
                        }

                    }
                );

            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            Target.Trance = 0;
            if (Target.IsUnderAnyStatus(BattleStatus.Jump))
            {
                btl_stat.RemoveStatus(Target, BattleStatusId.Jump);
                Target.Data.SetDisappear(false, 2);
                btl_mot.setBasePos(Target);
                btl_mot.setMotion(Target, Target.Data.bi.def_idle);
                Target.Data.evt.animFrame = 0;
            }

            if (!Target.IsMonsterTransform && SpecialSAEffect[Target][3] == 0)
                btl_cmd.SetCommand(Target.Data.cmd[4], BattleCommandId.SysTrans, 0, Target.Id, 0u);

            if (Target.PlayerIndex == CharacterId.Marcus)
                Target.AddDelayedModifier(
                    target => !target.IsDisappear,
                    target =>
                    {
                        target.Data.weapon_geo.SetActive(true);
                        target.MaximumMp /= 2;
                        if (MarcusAbsorbDarkness)
                        {
                            MarcusAbsorbDarkness = false;
                            target.AbsorbElement &= ~EffectElement.Darkness;
                        }
                        if (MarcusWeakLight)
                        {
                            MarcusWeakLight = false;
                            target.WeakElement &= ~EffectElement.Holy;
                        }
                    }
                );

            return true;
        }

        public void OnFinishCommand(CMD_DATA cmd, Int32 tranceDecrease)
        {
            if (Target.PlayerIndex != CharacterId.Garnet)
                return;
            if (cmd.cmd_no == BattleCommandId.Phantom)
            {
                PhantomAbility = EidolonToPhantom(btl_util.GetCommandMainActionIndex(cmd));
                if (PhantomAbility != BattleAbilityId.Void)
                    PhantomCountdown = GetPhantomCount(Target);
            }
            else if (cmd.cmd_no == BattleCommandId.SysPhantom)
            {
                PhantomCommandSent = false;
                PhantomCountdown = GetPhantomCount(Target);
            }
        }

        private Boolean ProcessPhantomRecast(BattleUnit garnet)
        {
            if (!garnet.IsUnderAnyStatus(BattleStatus.Trance))
                return false;
            if (FF9StateSystem.Battle.FF9Battle.btl_phase != FF9StateBattleSystem.PHASE_NORMAL)
            {
                ClearPhantomRecast(garnet);
                return true;
            }
            BattleAbilityId eidolon = (BattleAbilityId?)btl_util.GetBtlCurrentCommands(garnet).Find(cmd => cmd.cmd_no == BattleCommandId.Phantom)?.sub_no ?? BattleAbilityId.Void;
            if (eidolon != BattleAbilityId.Void && eidolon != PhantomAbility)
            {
                CMD_DATA phantomCmd = garnet.Data.cmd[3];
                if (PhantomCommandSent && phantomCmd.info.mode == command_mode_index.CMD_MODE_INSPECTION)
                {
                    PhantomAbility = EidolonToPhantom(eidolon);
                    phantomCmd.SetAAData(FF9StateSystem.Battle.FF9Battle.aa_data[PhantomAbility]);
                    phantomCmd.ScriptId = btl_util.GetCommandScriptId(phantomCmd);
                    phantomCmd.IsShortRange = btl_util.IsAttackShortRange(phantomCmd);
                }
            }
            if (!PhantomCommandSent && PhantomAbility != BattleAbilityId.Void)
            {
                if (PhantomCountdown <= 0)
                {
                    btl_cmd.SetCommand(garnet.Data.cmd[3], BattleCommandId.SysPhantom, (Int32)PhantomAbility, btl_util.GetStatusBtlID(1u, 0), 8u);
                    PhantomCommandSent = true;
                }
                else
                {
                    PhantomCountdown -= garnet.Data.cur.at_coef * BattleState.ATBTickCount;
                }
            }
            return true;
        }

        private void ClearPhantomRecast(BattleUnit garnet)
        {
            PhantomAbility = BattleAbilityId.Void;
            PhantomCommandSent = false;
            btl_cmd.KillSpecificCommand(garnet, BattleCommandId.SysPhantom);
        }
    }
}