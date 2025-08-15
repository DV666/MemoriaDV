using System;
using Memoria.Data;
using FF9;
using Object = System.Object;
using static Memoria.Scripts.Battle.TranceSeekAPI;
using Memoria.Prime;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Memoria.Scripts.Battle;

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
        public List<BattleStatusId> StatusResistOni = new List<BattleStatusId>();

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

            if (target.HasSupportAbilityByIndex(TranceSeekSupportAbility.High_Tide_Boosted))
            {
                Target.CurrentHp = Target.MaximumHp;
                Target.CurrentMp = Target.MaximumMp;
            }

            if (!target.IsPlayer)
            {
                ChangeLootMonster(target);
                if (target.Data.dms_geo_id == 427)
                {
                    target.AddDelayedModifier(
                        target => target.Data.bi.def_idle == 1,
                        target =>
                        {
                            if (ModelMoug[target.Data] == null) // TODO => Make new model for Beatrix wings.
                            {
                                ModelFactory.ChangeModelTexture(target.Data.gameObject, new string[] { "CustomTextures/Players/BeatrixTranceWings/427_0_trance.png", "CustomTextures/Players/BeatrixTranceWings/427_1_trance.png" });
                                ModelMoug[target.Data] = ModelFactory.CreateModel("GEO_MON_B3_115", true);
                                ModelMoug[target.Data].SetActive(true);
                                GeoAttach(ModelMoug[target.Data], target.Data.gameObject, 11);
                                ModelMoug[target.Data].transform.localPosition -= new Vector3(5f, -100f, 275f);
                                ModelMoug[target.Data].transform.localRotation = Quaternion.Euler(new Vector3(296f, 0, 0));
                                //ModelMoug[target.Data].transform.localScale = (new Vector3(0.85f, 0.85f, 0.85f));
                                ModelFactory.ChangeModelTexture(ModelMoug[target.Data], new string[] { "CustomTextures/Players/BeatrixTranceWings/Wings_0.png", "CustomTextures/Players/BeatrixTranceWings/Wings_1.png", "CustomTextures/Players/BeatrixTranceWings/Wings_2.png" });
                            }
                        }
                        );
                }
            }
            else
            {
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
                            if (target.HasSupportAbilityByIndex((SupportAbility)242))
                            {
                                foreach (BattleStatusId statusId in FF9BattleDB.StatusData.Keys)
                                {
                                    if (target.PartialResistStatus[statusId] == 0f && (statusId.ToBattleStatus() & BattleStatusConst.AnyNegative) != 0 && statusId != BattleStatusId.Death)
                                    {
                                        target.PartialResistStatus[statusId] = target.HasSupportAbilityByIndex((SupportAbility)1242) ? 0.75f : 0.50f;
                                        StatusResistOni.Add(statusId);
                                    }
                                }
                                if (target.HasSupportAbilityByIndex((SupportAbility)1242))
                                    target.AlterStatus(BattleStatus.EasyKill);
                            }
                        }
                    );

                if (target.PlayerIndex == CharacterId.Beatrix)
                {
                    target.AddDelayedModifier(
                        target => !target.Data.tranceGo.activeSelf,
                        target =>
                        {
                            if (ModelMoug[target.Data] == null) // TODO => Make new model for Beatrix wings.
                            {
                                ModelMoug[target.Data] = ModelFactory.CreateModel("GEO_MON_B3_115", true);
                                ModelMoug[target.Data].SetActive(true);
                                GeoAttach(ModelMoug[target.Data], target.Data.gameObject, 11);
                                ModelMoug[target.Data].transform.localPosition -= new Vector3(5f, -100f, 275f);
                                ModelMoug[target.Data].transform.localRotation = Quaternion.Euler(new Vector3(296f, 0, 0));
                                //ModelMoug[target.Data].transform.localScale = (new Vector3(0.85f, 0.85f, 0.85f));
                                ModelFactory.ChangeModelTexture(ModelMoug[target.Data], new string[] { "CustomTextures/Players/BeatrixTranceWings/Wings_0.png", "CustomTextures/Players/BeatrixTranceWings/Wings_1.png", "CustomTextures/Players/BeatrixTranceWings/Wings_2.png" });
                            }
                        }
                        );
                }
            }        

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
                        if (target.HasSupportAbilityByIndex((SupportAbility)242))
                        {
                            foreach (BattleStatusId statusId in StatusResistOni)
                            {
                                target.PartialResistStatus[statusId] = 0f;
                            }
                            if (target.HasSupportAbilityByIndex((SupportAbility)1242))
                                target.RemoveStatus(BattleStatus.EasyKill);
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

        private static void GeoAttach(GameObject sourceObject, GameObject targetObject, Int32 bone_index)
        {
            if (sourceObject == null || targetObject == null)
                return;
            Transform attachedTransform = targetObject.transform;
            Transform rootTransform = attachedTransform.GetChildByName("bone000").transform;
            Transform childByName = targetObject.transform.GetChildByName("bone" + bone_index.ToString("D3"));
            sourceObject.transform.parent = childByName;
            sourceObject.transform.localPosition = Vector3.zero;
            sourceObject.transform.localRotation = Quaternion.identity;
            sourceObject.transform.localScale = Vector3.one;
            rootTransform.localPosition = Vector3.zero;
            rootTransform.localRotation = Quaternion.identity;
            rootTransform.localScale = Vector3.one;
        }

        private void ChangeLootMonster(BattleUnit mob)
        {
            if (mob.IsPlayer)
                return;

            BattleEnemy battleEnemy = BattleEnemy.Find(mob);
            if (mob.Data.dms_geo_id == 92 && battleEnemy.StealableItems[3] == (RegularItem)1162) // Gnoll
                battleEnemy.StealableItems[3] = (RegularItem)1161;
            else if (mob.Data.dms_geo_id == 85 && battleEnemy.StealableItems[3] == (RegularItem)1158) // Lamia
                battleEnemy.StealableItems[3] = (RegularItem)1159;
            else if (mob.Data.dms_geo_id == 152 && battleEnemy.StealableItems[3] == (RegularItem)1152) // Goblin
                battleEnemy.StealableItems[3] = (RegularItem)1037;
            else if (mob.Data.dms_geo_id == 556 && battleEnemy.StealableItems[3] == RegularItem.Peridot) // Mistodon
                battleEnemy.StealableItems[3] = (RegularItem)1041;
            else if (mob.Data.dms_geo_id == 90 && battleEnemy.StealableItems[3] == RegularItem.Tent) // Griffin
                battleEnemy.StealableItems[3] = (RegularItem)1039;
        }
    }
}
