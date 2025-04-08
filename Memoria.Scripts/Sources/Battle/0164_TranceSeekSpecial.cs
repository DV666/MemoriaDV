using System;
using System.Collections.Generic;
using FF9;
using Memoria.Data;
using UnityEngine;
using UnityEngine.Networking;
using static SiliconStudio.Social.ResponseData;

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

        public static Dictionary<BTL_DATA, SPSEffect> PolaritySPS = new Dictionary<BTL_DATA, SPSEffect>();

        public void Perform()
        {
            if (_v.Command.Power == 11 && _v.Command.HitRate == 11) // Ironite (Dragon Force)
            {
                _v.Target.TryAlterSingleStatus(BattleStatusId.ChangeStat, true, _v.Caster, "PhysicalDefence", Math.Min(255, _v.Target.PhysicalDefence + 2));
                Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                                {
                                    { "US", "Defence ↑" },
                                    { "UK", "Defence ↑" },
                                    { "JP", "ぼうぎょりょく↑" },
                                    { "ES", "DIF fisica ↑" },
                                    { "FR", "Défense ↑" },
                                    { "GR", "Defensa F ↑" },
                                    { "IT", "Abwehr ↑" },
                                };
                btl2d.Btl2dReqSymbolMessage(_v.Target.Data, "[F9FF39]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 0);
                _v.Target.TryAlterSingleStatus(BattleStatusId.ChangeStat, true, _v.Caster, "MagicDefence", Math.Min(255, _v.Target.MagicDefence + 2));
                Dictionary<String, String> localizedMessage2 = new Dictionary<String, String>
                                {
                                    { "US", "Magic Def ↑" },
                                    { "UK", "Magic Def ↑" },
                                    { "JP", "まほうぼうぎょ ↑" },
                                    { "ES", "DIF magica ↑" },
                                    { "FR", "Protection ↑" },
                                    { "GR", "Defensa M ↑" },
                                    { "IT", "Z-Abwehr ↑" },
                                };
                btl2d.Btl2dReqSymbolMessage(_v.Target.Data, "[F9FF39]", localizedMessage2, HUDMessage.MessageStyle.DAMAGE, 5);
            }
            else if (_v.Command.Power == 25 && _v.Command.HitRate == 111 && _v.Caster.Data.dms_geo_id == 278) // Polarity (+) with SPS effect (Black Waltz 3)
            {
                _v.NormalMagicParams();
                TranceSeekCustomAPI.CharacterBonusPassive(_v, "MagicAttack");
                TranceSeekCustomAPI.CasterPenaltyMini(_v);
                TranceSeekCustomAPI.EnemyTranceBonusAttack(_v);
                TranceSeekCustomAPI.PenaltyShellAttack(_v);
                TranceSeekCustomAPI.PenaltyCommandDividedAttack(_v);
                TranceSeekCustomAPI.BonusElement(_v);
                if (TranceSeekCustomAPI.CanAttackMagic(_v))
                {
                    _v.CalcHpDamage();
                    TranceSeekCustomAPI.RaiseTrouble(_v);
                }
                TranceSeekCustomAPI.TryAlterCommandStatuses(_v);

                if (!PolaritySPS.TryGetValue(_v.Caster.Data, out SPSEffect sps)) // Init
                    PolaritySPS[_v.Caster.Data] = null;

                sps = HonoluluBattleMain.battleSPS.AddSequenceSPS(11, -1, 1, true); // Bone 28
                if (sps == null)
                    return;
                sps.charTran = _v.Target.Data.gameObject.transform;
                sps.boneTran = _v.Target.Data.gameObject.transform.GetChildByName("bone028");
                sps.posOffset = Vector3.zero;
                PolaritySPS[_v.Caster.Data] = sps;
                return;
                //sps.scale = (Int32)(sps.scale * tmpSingle);
            }
            else if (_v.Command.HitRate == 133 && _v.Caster.Data.dms_geo_id == 593) // Polarity (+) with SPS effect (Black Waltz 3 broken)
            {
                _v.SetCommandAttack();
                TranceSeekCustomAPI.PenaltyCommandDividedAttack(_v);
                if (_v.Target.IsUnderStatus(BattleStatus.Shell))
                    _v.Context.Attack = _v.Context.Attack / 2;
                TranceSeekCustomAPI.BonusElement(_v);
                if (TranceSeekCustomAPI.CanAttackMagic(_v))
                {
                    _v.CalcCannonProportionDamage();
                }
                _v.TryAlterMagicStatuses();

                if (!PolaritySPS.TryGetValue(_v.Target.Data, out SPSEffect sps)) // Init
                    PolaritySPS[_v.Target.Data] = null;

                sps = HonoluluBattleMain.battleSPS.AddSequenceSPS(11, -1, 1, true);
                if (sps == null)
                    return;
                btl2d.GetIconPosition(_v.Target, btl2d.ICON_POS_HEAD, out Transform attachTransf, out Vector3 iconOff);
                sps.charTran = _v.Target.Data.gameObject.transform;
                sps.boneTran = attachTransf;
                sps.posOffset = Vector3.zero;
                PolaritySPS[_v.Target.Data] = sps;
                FF9StateSystem.EventState.gEventGlobal[1305] = (byte)_v.Target.Id;
                return;
            }
            else if (_v.Command.Power == 25 && _v.Command.HitRate == 222 && (_v.Caster.Data.dms_geo_id == 278 || _v.Caster.Data.dms_geo_id == 593)) // Polarized staff (-) with SPS effect (Black Waltz 3)
            {
                if (_v.Target.CheckUnsafetyOrGuard() && _v.Target.CanBeAttacked())
                {
                    _v.Context.Flags |= BattleCalcFlags.DirectHP;
                    if (_v.Target.CurrentHp < 10U)
                    {
                        _v.Target.CurrentHp = (uint)(1L + GameRandom.Next8() % _v.Target.CurrentHp);
                    }
                    else
                    {
                        _v.Target.CurrentHp = (uint)(1 + GameRandom.Next8() % 9);
                    }
                    TranceSeekCustomAPI.TryAlterCommandStatuses(_v);
                }

                if (!PolaritySPS.TryGetValue(_v.Target.Data, out SPSEffect sps)) // Init
                    PolaritySPS[_v.Target.Data] = null;

                sps = HonoluluBattleMain.battleSPS.AddSequenceSPS(12, -1, 1, true);
                if (sps == null)
                    return;
                btl2d.GetIconPosition(_v.Target, btl2d.ICON_POS_HEAD, out Transform attachTransf, out Vector3 iconOff);
                sps.charTran = _v.Target.Data.gameObject.transform;
                sps.boneTran = attachTransf;
                sps.posOffset = Vector3.zero;
                PolaritySPS[_v.Target.Data] = sps;
                //sps.scale = (Int32)(sps.scale * tmpSingle);

                if (_v.Caster.Data.dms_geo_id == 278)
                {
                    _v.Target.AddDelayedModifier(
                        target => PolaritySPS[_v.Caster.Data].attr != 0,
                        target =>
                        {
                            PolaritySPS[_v.Target.Data].attr = 0;
                            PolaritySPS[_v.Target.Data].meshRenderer.enabled = false;
                            btl_stat.RemoveStatus(_v.Target, BattleStatusId.Slow);
                        }
                    );
                }
                return;
            }
            else if (_v.Command.Power == 25 && _v.Command.HitRate == 77 && _v.Caster.Data.dms_geo_id == 278) // Barrier OFF (Black Waltz 3)
            {
                TranceSeekCustomAPI.TryRemoveAbilityStatuses(_v);
                PolaritySPS[_v.Caster.Data].attr = 0;
                PolaritySPS[_v.Caster.Data].meshRenderer.enabled = false;
                return;
            }
            else if (_v.Caster.Data.dms_geo_id == 349) // Gizamaluke (preparation to dive)
            {
                if (_v.Command.Power == 11 && _v.Command.HitRate == 111)
                {
                    _v.Caster.Data.mot[1] = "ANH_MON_B3_114_043";
                    _v.Caster.Data.mot[3] = "ANH_MON_B3_114_043";
                    return;
                }
                else if (_v.Command.Power == 22 && _v.Command.HitRate == 222)
                {
                    _v.Caster.Data.mot[1] = "ANH_MON_B3_114_000";
                    _v.Caster.Data.mot[3] = "ANH_MON_B3_114_003";
                    return;
                }                  
            }
            if (FF9StateSystem.Battle.battleMapIndex == 303)
            {
                SB2_PATTERN sb2Pattern = FF9StateSystem.Battle.FF9Battle.btl_scene.PatAddr[FF9StateSystem.Battle.FF9Battle.btl_scene.PatNum];
                if (sb2Pattern.Monster[_v.Caster.Data.bi.slot_no].TypeNo == 0 && (_v.Command.AbilityStatus & BattleStatus.Heat) != 0) // Buzz - Blambourine
                {
                    BattleStatusDataEntry statusData = FF9StateSystem.Battle.FF9Battle.status_data[BattleStatusId.Heat];
                    Int32 wait = (short)(((400 + (_v.Caster.Will * 2) - _v.Target.Will) * statusData.ContiCnt) / 4);
                    _v.Target.AddDelayedModifier(
                    target => (wait -= target.Data.cur.at_coef * BattleState.ATBTickCount) > 0,
                    target =>
                    {
                        target.RemoveStatus(BattleStatus.Heat);
                    }
                    );
                }
            }
            else if (_v.Command.Power == 77 && _v.Command.HitRate == 77) // Giant Drink (Mad Alchemist)
            {
                _v.Target.RemoveStatus(BattleStatus.Mini);
                _v.Target.Data.geo_scale_default = 16384;
            }
            else if (_v.Caster.Data.dms_geo_id == 146)
            {
                if (_v.Command.Power == 10)
                {
                    if (_v.Command.HitRate == 1)
                    {
                        _v.Target.RemoveStatus(BattleStatus.Slow);
                        _v.Target.RemoveStatus(BattleStatus.Haste);
                        if (!_v.Target.IsUnderAnyStatus(BattleStatus.Death))
                        {
                            if (_v.Target.Data.bi.player != 0)
                                UIManager.Battle.RemovePlayerFromAction(_v.Target.Data.btl_id, true);
                            btl_cmd.KillMainCommand(_v.Target.Data);
                            _v.Target.Data.bi.atb = 0;
                            if (_v.Target.Data.bi.player != 0 && !FF9StateSystem.Settings.IsATBFull)
                                _v.Target.Data.cur.at = 0;
                            _v.Target.Data.sel_mode = 0;
                        }
                        btl_stat.MakeStatusesPermanent(_v.Target, BattleStatus.Stop, true);
                        _v.Target.Data.bi.target = 0;
                        TranceSeekCustomAPI.MonsterMechanic[_v.Caster.Data][2] = _v.Target.Id;
                    }
                    else
                    {
                        foreach (BattleUnit unit in BattleState.EnumerateUnits())
                        {
                            if (TranceSeekCustomAPI.MonsterMechanic[_v.Caster.Data][2] == unit.Id)
                            {
                                unit.Data.bi.target = 1;
                                btl_stat.MakeStatusesPermanent(unit, BattleStatus.Stop, false);
                                unit.RemoveStatus(BattleStatus.Stop);
                            }
                        }
                    }
                }

            }
            else
            {
                foreach (BattleStatusId statusId in _v.Target.Data.stat.cur.ToStatusList())
                    btl_stat.RemoveStatus(_v.Target, statusId);
                _v.Context.Flags = 0;
            }
        }
    }
}
