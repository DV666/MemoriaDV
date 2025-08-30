using System;
using System.Collections.Generic;
using System.Linq;
using FF9;
using Memoria.Assets;
using Memoria.Data;
using Memoria.Database;
using Memoria.Prime.PsdFile;
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
            if (_v.Command.Id == (BattleCommandId)1053 || _v.Command.Id == (BattleCommandId)1054) // Metronome
            {
                UInt16 target = BattleState.GetRandomUnitId(isPlayer: false);

                List<BattleAbilityId> BlackAndWhiteMagic = new List<BattleAbilityId>();

                foreach (BattleAbilityId abilId in CharacterCommands.Commands[BattleCommandId.BlackMagic].EnumerateAbilities())
                {
                    if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1256))
                    {
                        if ((FF9StateSystem.Battle.FF9Battle.aa_data[abilId].Ref.Power > 20 || FF9StateSystem.Battle.FF9Battle.aa_data[abilId].Ref.Power == 00))
                            BlackAndWhiteMagic.Add(abilId);
                    }
                    else
                    {
                        BlackAndWhiteMagic.Add(abilId);

                    }
                }
                    
                foreach (BattleAbilityId abilId in CharacterCommands.Commands[BattleCommandId.WhiteMagicGarnet].EnumerateAbilities())
                {
                    if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1256))
                    {
                        if ((FF9StateSystem.Battle.FF9Battle.aa_data[abilId].Ref.Power > 20 || FF9StateSystem.Battle.FF9Battle.aa_data[abilId].Ref.Power == 00))
                            BlackAndWhiteMagic.Add(abilId);
                    }
                    else
                    {
                        BlackAndWhiteMagic.Add(abilId);

                    }
                }
                foreach (BattleAbilityId abilId in CharacterCommands.Commands[BattleCommandId.WhiteMagicEiko].EnumerateAbilities())
                {
                    if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1256))
                    {
                        if ((FF9StateSystem.Battle.FF9Battle.aa_data[abilId].Ref.Power > 20 || FF9StateSystem.Battle.FF9Battle.aa_data[abilId].Ref.Power == 00))
                            BlackAndWhiteMagic.Add(abilId);
                    }
                    else
                    {
                        BlackAndWhiteMagic.Add(abilId);

                    }
                }
                foreach (BattleAbilityId abilId in CharacterCommands.Commands[BattleCommandId.BlueMagic].EnumerateAbilities())
                {
                    if (ff9abil.FF9Abil_IsMaster(FF9StateSystem.Common.FF9.GetPlayer(CharacterId.Quina), ff9abil.GetAbilityIdFromActiveAbility((BattleAbilityId)abilId))) // If Quina learn the blue magic.
                        BlackAndWhiteMagic.Add(abilId);
                }

                if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1256)) // SA Metronome+
                {
                    foreach (BattleAbilityId abilId in CharacterCommands.Commands[BattleCommandId.SummonGarnet].EnumerateAbilities())
                        BlackAndWhiteMagic.Add(abilId);
                    foreach (BattleAbilityId abilId in CharacterCommands.Commands[BattleCommandId.SummonEiko].EnumerateAbilities())
                        BlackAndWhiteMagic.Add(abilId);          
                }

                BlackAndWhiteMagic.Distinct().ToList();

                BattleAbilityId AbilityChoosen = BlackAndWhiteMagic[GameRandom.Next16() % BlackAndWhiteMagic.Count];
                TargetType TargetAA = FF9StateSystem.Battle.FF9Battle.aa_data[AbilityChoosen].Info.Target;
                Boolean TargetDefaultAlly = FF9StateSystem.Battle.FF9Battle.aa_data[AbilityChoosen].Info.DefaultAlly;
                int ScriptAA = FF9StateSystem.Battle.FF9Battle.aa_data[AbilityChoosen].Ref.ScriptId;

                if (TargetAA == TargetType.SingleAny || TargetAA == TargetType.SingleAlly || TargetAA == TargetType.SingleEnemy)
                {
                    if (TargetDefaultAlly)
                        target = BattleState.GetRandomUnitId(isPlayer: true);
                    else
                        target = BattleState.GetRandomUnitId(isPlayer: false);
                }
                else if (TargetAA == TargetType.ManyAny || TargetAA == TargetType.ManyAlly || TargetAA == TargetType.ManyEnemy)
                {
                    if (GameRandom.Next16() % 2 == 0 || (BattleState.TargetCount(false) == 1 && TargetAA == TargetType.ManyEnemy) || (BattleState.TargetCount(true) == 1 && TargetAA == TargetType.ManyAlly))
                    {
                        if (TargetDefaultAlly)
                            target = BattleState.GetRandomUnitId(isPlayer: true);
                        else
                            target = BattleState.GetRandomUnitId(isPlayer: false);
                    }
                    else
                    {
                        if (TargetDefaultAlly)
                            target = 15;
                        else
                            target = 240;
                    }
                }
                else if (TargetAA == TargetType.All || TargetAA == TargetType.AllAlly || TargetAA == TargetType.AllEnemy)
                {
                    if (TargetDefaultAlly)
                        target = 15;
                    else
                        target = 240;
                }
                else if (TargetAA == TargetType.Self)
                {
                    target = _v.Caster.Id;
                }
                else if (TargetAA == TargetType.Everyone)
                {
                    target = 255;
                }

                if (ScriptAA == 10 && target != 15) // Heal
                {
                    Single RatioHP = 0;
                    foreach (BattleUnit playerunit in BattleState.EnumerateUnits())
                    {
                        if (playerunit.IsPlayer)
                        {
                            Single PlayerRatioHP = BattleScriptDamageEstimate.RateHpMp((Int32)playerunit.CurrentHp, (Int32)playerunit.MaximumHp);
                            if (PlayerRatioHP > RatioHP)
                            {
                                RatioHP = PlayerRatioHP;
                                target = playerunit.Id;
                            }
                        }
                    }
                }
                else if ((ScriptAA == 12 || ScriptAA == 103) && target != 15) // Cure Status
                {
                    Single RatioStatus = 0;
                    foreach (BattleUnit playerunit in BattleState.EnumerateUnits())
                    {
                        if (playerunit.IsPlayer)
                        {
                            Single PlayerRatioStatus = 0;
                            if (playerunit.IsUnderAnyStatus(TranceSeekStatus.Vieillissement) && AbilityChoosen == BattleAbilityId.Esuna)
                                PlayerRatioStatus = 20;

                            BattleStatus playerStatus = playerunit.CurrentStatus;
                            BattleStatus removeStatus = (FF9BattleDB.StatusSets.TryGetValue(FF9StateSystem.Battle.FF9Battle.aa_data[AbilityChoosen].AddStatusNo, out BattleStatusEntry stat) ? stat.Value : 0);
                            BattleStatus removedStatus = playerStatus & removeStatus;
                            Int32 rating = BattleScriptStatusEstimate.RateStatuses(removedStatus);

                            if (playerunit.IsPlayer)
                                PlayerRatioStatus = - 1 * rating;

                            if (PlayerRatioStatus > RatioStatus)
                            {
                                RatioStatus = PlayerRatioStatus;
                                target = playerunit.Id;
                            }
                        }
                    }
                }
                else if (ScriptAA == 13 && target != 15) // Life
                {
                    List<UInt16> candidates = new List<UInt16>(4);
                    for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next)
                        if (next.bi.player == 1 && btl_stat.CheckStatus(next, BattleStatus.Death) && next.bi.target != 0)
                            candidates.Add(next.btl_id);
                    if (candidates.Count > 0)
                        target = candidates[UnityEngine.Random.Range(0, candidates.Count)];
                }

                short OldMpCostFactor = _v.Caster.Player.mpCostFactor;
                _v.Caster.Player.mpCostFactor = 0; // Magic cost 0 MP.

                BattleState.EnqueueCounter(_v.Caster, BattleCommandId.RushAttack, AbilityChoosen, target);

                Int32 counter = 80;
                _v.Caster.AddDelayedModifier(
                    caster => (counter -= BattleState.ATBTickCount) > 0,
                    caster =>
                    {
                        _v.Caster.Player.mpCostFactor = OldMpCostFactor;
                    }
                );
                return;
            }
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
            else if (_v.Command.Power == 1 && _v.Command.HitRate == 1 && _v.Caster.Data.dms_geo_id == 326) // Frying from Jötunn
            {
                _v.Caster.Flags |= (CalcFlag.HpDamageOrHeal| CalcFlag.MpDamageOrHeal);
                _v.Target.Flags |= (CalcFlag.HpAlteration | CalcFlag.MpAlteration);
                int HPDamageMiam = (int)(_v.Target.CurrentHp - 10000);
                int MPDamageMiam = (int)(_v.Target.CurrentMp);
                _v.Caster.HpDamage = _v.Target.HpDamage = HPDamageMiam;
                _v.Caster.MpDamage = _v.Target.MpDamage = MPDamageMiam;
                int NumberBonus = Math.Max(1, HPDamageMiam / 1000);
                List<BattleStatusId> statuschoosen = new List<BattleStatusId>();
                foreach (BattleStatusId statusID in _v.Command.AbilityStatus.ToStatusList())
                    statuschoosen.Add(statusID);

                while (NumberBonus > 0 && statuschoosen.Count > 0)
                {
                    BattleStatusId statusselected = statuschoosen[GameRandom.Next16() % statuschoosen.Count];
                    _v.Caster.AlterStatus(statusselected, _v.Target);
                    statuschoosen.Remove(statusselected);
                    NumberBonus--;
                }
                BattleStatus statuspiaf = (_v.Target.CurrentStatus & ~BattleStatus.EasyKill);
                _v.Target.RemoveStatus(statuspiaf);
                statuspiaf = statuspiaf & ~_v.Caster.ResistStatus;
                _v.Caster.AlterStatus(statuspiaf, _v.Target);
                return;
            }
            else if (_v.Command.Power == 2 && _v.Command.HitRate == 2 && _v.Caster.Data.dms_geo_id == 326) // Troll Feast from Jötunn
            {
                int PiafTargetable = 0;
                foreach (BattleUnit unit in FF9StateSystem.Battle.FF9Battle.EnumerateBattleUnits())
                {
                    if (unit.Data.dms_geo_id == 9 && unit.Data.bi.target == 0)
                    {
                        unit.Data.bi.target = 1;
                        PiafTargetable++;
                    }
                }
                if (PiafTargetable > 0)
                    return;

                _v.Target.Flags |= (CalcFlag.HpDamageOrHeal | CalcFlag.MpDamageOrHeal);
                _v.Target.HpDamage = (int)(_v.Target.MaximumHp - 10000);
                _v.Target.MpDamage = (int)(_v.Target.MaximumMp);
                TranceSeekAPI.MonsterMechanic[_v.Target.Data][4] = 100;
                return;
            }
            else if (_v.Command.Power == 77 && _v.Command.HitRate == 177 && _v.Caster.Data.dms_geo_id == 546) // Mad Alchemist - Dragon Power
            {
                Dictionary<String, String> localizedMessage2 = new Dictionary<String, String>
                {
                    { "US", "Strength ↑↑↑" },
                    { "UK", "Strength ↑↑↑" },
                    { "JP", "ちから ↑↑↑" },
                    { "ES", "Forza ↑↑↑" },
                    { "FR", "Force ↑↑↑" },
                    { "GR", "Fuerza ↑↑↑" },
                    { "IT", "Stärke ↑↑↑" },
                };
                btl2d.Btl2dReqSymbolMessage(_v.Target.Data, "[FF040E]", localizedMessage2, HUDMessage.MessageStyle.DAMAGE, 10);
                Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                {
                    { "US", "Magic ↑↑↑" },
                    { "UK", "Magic ↑↑↑" },
                    { "JP", "まりょく ↑↑↑" },
                    { "ES", "POT magico ↑↑↑" },
                    { "FR", "Magie ↑↑↑" },
                    { "GR", "Magia ↑↑↑" },
                    { "IT", "Zauber ↑↑↑" },
                };
                btl2d.Btl2dReqSymbolMessage(_v.Target.Data, "[FF040E]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 15);
                return;
            }
            else if (_v.Command.Power == 1 && _v.Command.HitRate == 1 && _v.Caster.Data.dms_geo_id == 405) // Friendly Lady Bug - Wind mechanics
            {
                int ColorWing = GameRandom.Next16() % 5;
                _v.Caster.Data.gameObject.SetActive(false);
                _v.Caster.Data.gameObject = ModelFactory.CreateModel("GEO_MON_B3_159", true);
                if (ColorWing == 0)
                {
                    _v.Caster.WeakElement = EffectElement.Wind | EffectElement.Wind | EffectElement.Cold;
                    _v.Caster.AbsorbElement = EffectElement.Holy | EffectElement.Fire;
                    UIManager.Battle.SetBattleFollowMessage(3, Localization.GetWithDefault("LadyBugRed"));
                    ModelFactory.ChangeModelTexture(_v.Caster.Data.gameObject, new string[] { "CustomTextures/FriendlyLadyBug/FireWings/405_0.png", "CustomTextures/FriendlyLadyBug/FireWings/405_1.png", "CustomTextures/FriendlyLadyBug/FireWings/405_2.png" });
                }
                else if (ColorWing == 1)
                {
                    _v.Caster.WeakElement = EffectElement.Wind | EffectElement.Wind | EffectElement.Fire;
                    _v.Caster.AbsorbElement = EffectElement.Holy | EffectElement.Cold;
                    UIManager.Battle.SetBattleFollowMessage(3, Localization.GetWithDefault("LadyBugCyan"));
                    ModelFactory.ChangeModelTexture(_v.Caster.Data.gameObject, new string[] { "CustomTextures/FriendlyLadyBug/IceWings/405_0.png", "CustomTextures/FriendlyLadyBug/IceWings/405_1.png", "CustomTextures/FriendlyLadyBug/IceWings/405_2.png" });
                }
                else if (ColorWing == 2)
                {
                    _v.Caster.WeakElement = EffectElement.Wind | EffectElement.Wind | EffectElement.Aqua;
                    _v.Caster.AbsorbElement = EffectElement.Holy | EffectElement.Thunder;
                    UIManager.Battle.SetBattleFollowMessage(3, Localization.GetWithDefault("LadyBugYellow"));
                    ModelFactory.ChangeModelTexture(_v.Caster.Data.gameObject, new string[] { "CustomTextures/FriendlyLadyBug/ThunderWings/405_0.png", "CustomTextures/FriendlyLadyBug/ThunderWings/405_1.png", "CustomTextures/FriendlyLadyBug/ThunderWings/405_2.png" });
                }
                else if (ColorWing == 3)
                {
                    _v.Caster.WeakElement = EffectElement.Wind | EffectElement.Wind | EffectElement.Thunder;
                    _v.Caster.AbsorbElement = EffectElement.Holy | EffectElement.Aqua;
                    UIManager.Battle.SetBattleFollowMessage(3, Localization.GetWithDefault("LadyBugBlue"));
                    ModelFactory.ChangeModelTexture(_v.Caster.Data.gameObject, new string[] { "CustomTextures/FriendlyLadyBug/WaterWings/405_0.png", "CustomTextures/FriendlyLadyBug/WaterWings/405_1.png", "CustomTextures/FriendlyLadyBug/WaterWings/405_2.png" });
                }
                else
                {
                    _v.Caster.WeakElement = EffectElement.Wind | EffectElement.Wind;
                    _v.Caster.AbsorbElement = EffectElement.Holy;
                    UIManager.Battle.SetBattleFollowMessage(3, Localization.GetWithDefault("LadyBugWhite"));
                }
                _v.Caster.Data.gameObject.SetActive(true);
            }
            else if (_v.Command.Power == 25 && _v.Command.HitRate == 111 && _v.Caster.Data.dms_geo_id == 278) // Polarity (+) with SPS effect (Black Waltz 3)
            {
                _v.NormalMagicParams();
                TranceSeekAPI.CharacterBonusPassive(_v, "MagicAttack");
                TranceSeekAPI.CasterPenaltyMini(_v);
                TranceSeekAPI.EnemyTranceBonusAttack(_v);
                TranceSeekAPI.PenaltyShellAttack(_v);
                TranceSeekAPI.PenaltyCommandDividedAttack(_v);
                TranceSeekAPI.BonusElement(_v);
                if (TranceSeekAPI.CanAttackMagic(_v))
                {
                    _v.CalcHpDamage();
                    TranceSeekAPI.RaiseTrouble(_v);
                }
                TranceSeekAPI.TryAlterCommandStatuses(_v);

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
                TranceSeekAPI.PenaltyCommandDividedAttack(_v);
                if (_v.Target.IsUnderStatus(BattleStatus.Shell))
                    _v.Context.DamageModifierCount -= 2;
                TranceSeekAPI.BonusElement(_v);
                if (TranceSeekAPI.CanAttackMagic(_v))
                {
                    _v.CalcCannonProportionDamage();
                }
                TranceSeekAPI.TryAlterMagicStatuses(_v);

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
                if (TranceSeekAPI.CheckUnsafetyOrGuard(_v) && _v.Target.CanBeAttacked())
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
                    TranceSeekAPI.TryAlterCommandStatuses(_v);
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
                TranceSeekAPI.TryRemoveAbilityStatuses(_v);
                PolaritySPS[_v.Caster.Data].attr = 0;
                PolaritySPS[_v.Caster.Data].meshRenderer.enabled = false;
                return;
            }
            else if (_v.Command.Power == 133 && _v.Command.HitRate == 133 && _v.Caster.Data.dms_geo_id == 427) // Beatrix 3 - Finisher
            {
                _v.Target.CurrentHp = 1;
                _v.Target.CurrentMp = 1;
                btl_stat.MakeStatusesPermanent(_v.Caster, _v.Caster.PermanentStatus | BattleStatus.Trance, false);
                _v.Caster.RemoveStatus(BattleStatus.Trance);
                return;
            }
            else if (_v.Caster.Data.dms_geo_id == 63) // Golden Pidove
            {
                if (_v.Target.Data == _v.Caster.Data)
                {
                    PolaritySPS[_v.Caster.Data].attr = 0;
                    PolaritySPS[_v.Caster.Data].meshRenderer.enabled = false;
                    _v.Caster.AddDelayedModifier(
                        caster => caster.CurrentAtb >= caster.MaximumAtb,
                        caster =>
                        {
                            caster.CurrentHp = 10000;
                            foreach (BattleUnit player in BattleState.EnumerateUnits())
                            {
                                if (player.IsPlayer)
                                {
                                    player.CurrentHp = player.MaximumHp;
                                    player.CurrentMp = player.MaximumMp;
                                }
                            }
                        }
                    );
                }
                else
                {
                    int randomdamage = UnityEngine.Random.Range(0, 999999999);
                    btl2d.Btl2dReqSymbolMessage(_v.Target.Data, "[FFFFFF]", randomdamage.ToString(), HUDMessage.MessageStyle.DAMAGE, 0);
                    _v.Target.CurrentHp = (uint)Math.Max(1, _v.Target.CurrentHp - randomdamage);
                }
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
            else if (_v.Command.Power == 199 && _v.Command.HitRate == 199 && _v.Command.AbilityStatus == BattleStatus.Reflect) // AntiBoom from Invincible (CD3 Kuja)
            {
                _v.Target.Flags = CalcFlag.HpAlteration | CalcFlag.MpAlteration;
                _v.Target.HpDamage = (int)(_v.Target.CurrentHp - 1);
                _v.Target.MpDamage = (int)(_v.Target.CurrentMp - 1);
                if (_v.Target.IsUnderPermanentStatus(BattleStatus.Reflect))
                    _v.Target.Data.stat.permanent &= ~BattleStatus.Reflect;

                _v.Target.RemoveStatus(BattleStatus.Reflect);
            }
            else if (_v.Caster.Data.dms_geo_id == 146) // Quicksand mechanic
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
                        TranceSeekAPI.MonsterMechanic[_v.Caster.Data][2] = _v.Target.Id;
                        FF9StateSystem.EventState.gEventGlobal[1305] = (byte)_v.Target.Id;
                    }
                    else
                    {
                        foreach (BattleUnit unit in BattleState.EnumerateUnits())
                        {
                            if (TranceSeekAPI.MonsterMechanic[_v.Caster.Data][2] == unit.Id)
                            {
                                unit.Data.bi.target = 1;
                                btl_stat.MakeStatusesPermanent(unit, BattleStatus.Stop, false);
                                unit.RemoveStatus(BattleStatus.Stop);
                                FF9StateSystem.EventState.gEventGlobal[1305] = 0;
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
