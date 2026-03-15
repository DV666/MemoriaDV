using FF9;
using Memoria.Data;
using Memoria.Scripts.Battle;
using System;
using System.Collections.Generic;
using UnityEngine;
using static BTL_DATA;
using static Memoria.Scripts.Battle.TranceSeekBattleDictionary;
using Object = System.Object;

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
        public Int32 TimerEndTrance = 0;
        public Boolean TriggerTimerEndTrance = false;

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
            var Target_TSVar = target.State();
            if (!Target.IsPlayer && !Target.IsUnderAnyStatus(BattleStatus.EasyKill)) // +50% HP/MP Max if monster get under Trance
            {
                Target.MaximumHp += (Target.MaximumHp / 2);
                Target.MaximumMp += (Target.MaximumMp / 2);
                Target.CurrentHp = Target.MaximumHp;
                Target.CurrentMp = Target.MaximumMp;
                Target.RemoveStatus(BattleStatusConst.AnyNegative);
                Target.AlterStatus(BattleStatus.EasyKill);
            }

            if (target.HasSupportAbilityByIndex(TranceSeekSupportAbility.HighTide_Boosted))
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
                            ModelFactory.ChangeModelTexture(target.Data.gameObject, new string[] { "CustomTextures/Players/BeatrixTranceWings/427_0_trance.png", "CustomTextures/Players/BeatrixTranceWings/427_1_trance.png" });
                            WEAPON_MODEL GoldenWings = new WEAPON_MODEL { geo = ModelFactory.CreateModel("GEO_NPC_GoldenWings", true) };
                            GoldenWings.geo.SetActive(true);
                            GeoAttach(GoldenWings.geo, target.Data.gameObject, 11);
                            Target_TSVar.AdditionalModel.Add(GoldenWings);
                        }
                        );
                }
                else if (target.Data.dms_geo_id == 410)
                {
                    target.AddDelayedModifier(
                        target => !target.Data.enable_trance_glow,
                        target =>
                        {
                            ModelFactory.ChangeModelTexture(target.Data.gameObject, new string[] { "CustomTextures/Players/LaniTrance/410_0_trance.png", "CustomTextures/Players/LaniTrance/410_1_trance.png", "CustomTextures/Players/LaniTrance/410_2_trance.png", "CustomTextures/Players/LaniTrance/410_3_trance.png" });
                            WEAPON_MODEL FirstAxe = new WEAPON_MODEL { geo = ModelFactory.CreateModel("GEO_ACC_F0_LNW", true) };
                            GeoAttach(FirstAxe.geo, target.Data.gameObject, 14);
                            FirstAxe.geo.transform.localPosition = new Vector3(-130.5f, 87f, 106f);
                            FirstAxe.geo.transform.localRotation = Quaternion.Euler(new Vector3(17.35538f, 121.1880f, 247.1161f));
                            FirstAxe.geo.SetActive(true);
                            Target_TSVar.AdditionalModel.Add(FirstAxe);

                            WEAPON_MODEL SecondAxe = new WEAPON_MODEL { geo = ModelFactory.CreateModel("GEO_ACC_F0_LNW", true) };
                            GeoAttach(SecondAxe.geo, target.Data.gameObject, 14);
                            SecondAxe.geo.transform.localPosition = new Vector3(114.5f, 84.5f, 183f);
                            SecondAxe.geo.transform.localRotation = Quaternion.Euler(new Vector3(16.64425f, 212.1457f, 111.8135f));
                            SecondAxe.geo.SetActive(true);
                            Target_TSVar.AdditionalModel.Add(SecondAxe);
                        }
                    );
                }
                else if (target.Data.dms_geo_id == 5414)
                {
                    target.AddDelayedModifier(
                        target => !target.Data.enable_trance_glow,
                        target =>
                        {
                            Vector3 position = target.Data.gameObject.transform.position;
                            target.Data.gameObject.SetActive(false);
                            target.Data.gameObject = ModelFactory.CreateModel("GEO_MAIN_B0_022", true);
                            for (Int16 j = 0; j < target.Data.mot.Length; j++) // [DV] Check each anims if a clip exist, otherwise create them (if we don't that for custom anim, the battle is frozen).
                                AnimationFactory.AddAnimWithAnimatioName(target.Data.gameObject, target.Data.mot[j]);
                            target.Data.gameObject.transform.position = position;

                            WEAPON_MODEL Dagger = new WEAPON_MODEL { geo = ModelFactory.CreateModel("GEO_WEP_B1_012", true), bone = 13 };  
                            target.Data.weaponModels.Add(Dagger);
                            GeoAttach(Dagger.geo, target.Data.gameObject, Dagger.bone);
                        }
                    );
                }
            }
            else
            {
                if (target.PlayerIndex == CharacterId.Garnet)
                    target.AddDelayedModifier(ProcessPhantomRecast, ClearPhantomRecast);
                else if (target.PlayerIndex == CharacterId.Marcus)
                    target.AddDelayedModifier(
                        target => !target.Data.tranceGo.activeSelf,
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
                            WEAPON_MODEL DemonWings = new WEAPON_MODEL { geo = ModelFactory.CreateModel("GEO_NPC_DemonWings", true) };
                            DemonWings.geo.SetActive(true);
                            GeoAttach(DemonWings.geo, target.Data.gameObject, 1);
                            Target_TSVar.AdditionalModel.Add(DemonWings);
                        }
                    );
                else if (target.PlayerIndex == CharacterId.Beatrix)
                {
                    target.AddDelayedModifier(
                        target => !target.Data.tranceGo.activeSelf,
                        target =>
                        {
                            WEAPON_MODEL GoldenWings = new WEAPON_MODEL { geo = ModelFactory.CreateModel("GEO_NPC_GoldenWings", true) };
                            GoldenWings.geo.SetActive(true);
                            GeoAttach(GoldenWings.geo, target.Data.gameObject, 11);
                            Target_TSVar.AdditionalModel.Add(GoldenWings);
                        }
                    );
                }
                else if (target.PlayerIndex == (CharacterId)12) // Lani
                {
                    target.AddDelayedModifier(
                        target => !target.Data.tranceGo.activeSelf,
                        target =>
                        {
                            WEAPON_MODEL FirstAxe = new WEAPON_MODEL { geo = ModelFactory.CreateModel("GEO_ACC_F0_LNW", true) };
                            GeoAttach(FirstAxe.geo, target.Data.gameObject, 14);
                            FirstAxe.geo.transform.localPosition = new Vector3(-130.5f, 87f, 106f);
                            FirstAxe.geo.transform.localRotation = Quaternion.Euler(new Vector3(17.35538f, 121.1880f, 247.1161f));
                            FirstAxe.geo.SetActive(true);
                            Target_TSVar.AdditionalModel.Add(FirstAxe);

                            WEAPON_MODEL SecondAxe = new WEAPON_MODEL { geo = ModelFactory.CreateModel("GEO_ACC_F0_LNW", true) };
                            GeoAttach(SecondAxe.geo, target.Data.gameObject, 14);
                            SecondAxe.geo.transform.localPosition = new Vector3(114.5f, 84.5f, 183f);
                            SecondAxe.geo.transform.localRotation = Quaternion.Euler(new Vector3(16.64425f, 212.1457f, 111.8135f));
                            SecondAxe.geo.SetActive(true);
                            Target_TSVar.AdditionalModel.Add(SecondAxe);
                        }
                    );
                }
            }        

            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            Target.Trance = 0;
            var Target_TSVar = Target.State();
            if (Target.IsUnderAnyStatus(BattleStatus.Jump))
            {
                btl_stat.RemoveStatus(Target, BattleStatusId.Jump);
                Target.Data.SetDisappear(false, 2);
                btl_mot.setBasePos(Target);
                btl_mot.setMotion(Target, Target.Data.bi.def_idle);
                Target.Data.evt.animFrame = 0;
            }

            if (!Target.IsMonsterTransform && !Target_TSVar.PreventTranceSFX)
                btl_cmd.SetCommand(Target.Data.cmd[4], BattleCommandId.SysTrans, 0, Target.Id, 0u);

            if (!Target.IsPlayer)
            {
                if (Target.Data.dms_geo_id == 427)
                {
                    Int32 counter = 70;
                    Target.AddDelayedModifier(
                        target => (counter -= 1) > 0,
                        target =>
                        {
                            Vector3 position = target.Data.gameObject.transform.position;
                            foreach (var model in Target_TSVar.AdditionalModel)
                                if (model != null && model.geo != null)
                                    UnityEngine.Object.Destroy(model.geo);

                            target.Data.weaponModels[0].geo.SetActive(false);
                            target.Data.gameObject.SetActive(false);
                            target.Data.gameObject = ModelFactory.CreateModel("GEO_MON_B3_155", true);
                            target.Data.gameObject.transform.position = position;
                            target.Data.weaponModels[0].geo = ModelFactory.CreateModel("GEO_WEP_B1_037", true);
                            GeoAttach(target.Data.weaponModels[0].geo, target.Data.gameObject, 16);
                            target.Data.gameObject.SetActive(true);
                            target.Data.weaponModels[0].geo.SetActive(true);
                        }
                    );
                }
                else if (Target.Data.dms_geo_id == 410)
                {
                    Target.AddDelayedModifier(
                        target => target.Data.enable_trance_glow,
                        target =>
                        {
                            Vector3 position = target.Data.gameObject.transform.position;
                            foreach (var model in Target_TSVar.AdditionalModel)
                                if (model != null && model.geo != null)
                                    UnityEngine.Object.Destroy(model.geo);

                            target.Data.gameObject.SetActive(false);
                            target.Data.gameObject = ModelFactory.CreateModel("GEO_MON_B3_122", true);
                            target.Data.gameObject.transform.position = position;
                            target.Data.gameObject.SetActive(true);
                        }
                    );
                }
            }
            else if (Target.PlayerIndex == CharacterId.Marcus)
            {
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
            }
            else if (Target.Accessory == TranceSeekRegularItem.GhostScarf && !Target.IsUnderAnyStatus(BattleStatus.Vanish)) 
            // [TODO] Specific bug when mixing this Accessory when getting Trance at the start of the battle, like Vivi vs Black Waltz 3 on Disc 1.
            {
                TimerEndTrance = 10;
                Target.AddDelayedModifier(WaitTranceSFXEnd, ResetModel);
            }
            return true;
        }

        private Boolean WaitTranceSFXEnd(BattleUnit unit)
        {
            if (unit.IsDisappear)
                TriggerTimerEndTrance = true;

            if (TriggerTimerEndTrance)
                if (!unit.IsDisappear)
                {
                    if (TimerEndTrance > 0)
                        TimerEndTrance--;
                    else
                        return false;
                }

            return true;
        }

        private void ResetModel(BattleUnit unit)
        {
            Vector3 position = unit.Data.gameObject.transform.position;
            CharacterBattleParameter btlParam = btl_mot.BattleParameterList[unit.Player.info.serial_no];
            unit.Data.gameObject = ModelFactory.CreateModel(btlParam.ModelId, true, true, Configuration.Graphics.ElementsSmoothTexture);
            unit.Data.gameObject.transform.position = position;
            btl_eqp.InitWeapon(unit.Player, unit.Data);
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
