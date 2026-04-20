using System;
using System.Collections.Generic;
using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Data;
using static Memoria.Assets.DataResources;
using static SiliconStudio.Social.ResponseData;

namespace Memoria.Scripts.TranceSeek
{
    /// <summary>
    /// Special
    /// </summary>
    [BattleScript(Id)]
    public sealed class MixRemedyScript : IBattleScript
    {
        public const Int32 Id = 0201;

        private readonly BattleCalculator _v;

        public MixRemedyScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Command.ItemId == RegularItem.NoItem || _v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Artificer)) // SA Artificer
            {
                _v.Context.Flags = BattleCalcFlags.Miss;
                return;
            }

            int HPHeal = 0;
            int MPHeal = 0;
            switch (_v.Command.ItemId)
            {
                case TranceSeekRegularItem.Serum: // Sérum
                case TranceSeekRegularItem.Echo: // Echo
                case TranceSeekRegularItem.Collyrium: // Collyre
                case TranceSeekRegularItem.Mild: // Doux
                case TranceSeekRegularItem.Sedative: // Calmant
                case TranceSeekRegularItem.BlessedWater: // Eau bénite
                case TranceSeekRegularItem.Antibodies: // Anticorps
                {
                    HPHeal = 200;
                    TranceSeekAPI.TryRemoveItemStatuses(_v);
                    break;
                }
                case TranceSeekRegularItem.HiSerum: // Maxi Sérum
                case TranceSeekRegularItem.HiEcho: // Maxi Echo
                case TranceSeekRegularItem.HiCollyrium: // Maxi Collyre
                case TranceSeekRegularItem.HiMild: // Maxi Doux
                case TranceSeekRegularItem.HiSedative: // Maxi Calmant
                case TranceSeekRegularItem.HiBlessedWater: // Maxi Eau bénite
                case TranceSeekRegularItem.HiAntibodies: // Maxi Anticorps
                {
                    HPHeal = 500;
                    TranceSeekAPI.TryRemoveItemStatuses(_v);
                    break;
                }
                case TranceSeekRegularItem.UltraSerum: // Ultra Sérum
                case TranceSeekRegularItem.UltraEcho: // Ultra Echo
                case TranceSeekRegularItem.UltraCollyrium: // Ultra Collyre
                case TranceSeekRegularItem.UltraMild: // Ultra Doux
                case TranceSeekRegularItem.UltraSedative: // Ultra Calmant
                case TranceSeekRegularItem.UltraBlessedWater: // Ultra Eau bénite
                case TranceSeekRegularItem.UltraAntibodies: // Ultra Anticorps
                {
                    HPHeal = 1250;
                    TranceSeekAPI.TryRemoveItemStatuses(_v);
                    break;
                }
                case TranceSeekRegularItem.Medication: // Médicament
                case TranceSeekRegularItem.HiMedication: // Maxi Médicament
                case TranceSeekRegularItem.UltraMedication: // Ultra Médicament
                case TranceSeekRegularItem.MedicationPlus: // Médicament +
                case TranceSeekRegularItem.HiMedicationPlus: // Maxi Médicament +
                case TranceSeekRegularItem.UltraMedicationPlus: // Ultra Médicament +
                {
                    HPHeal = (int)(_v.Target.MaximumHp * _v.Command.Power) / 100;
                    TranceSeekAPI.TryRemoveItemStatuses(_v);
                    break;
                }
                case TranceSeekRegularItem.Treatment: // Traitement
                case TranceSeekRegularItem.TreatmentPlus: // Traitement +
                case TranceSeekRegularItem.XTreatment: // Traitement X
                {
                    MPHeal = (int)(_v.Target.MaximumMp * _v.Command.Power) / 100;
                    TranceSeekAPI.TryRemoveItemStatuses(_v);
                    break;
                }
                case TranceSeekRegularItem.EnhancedSerum: // Sérum amélioré
                case TranceSeekRegularItem.EnhancedEcho: // Echo amélioré
                case TranceSeekRegularItem.EnhancedCollyrium: // Collyre amélioré
                case TranceSeekRegularItem.EnhancedMild: // Doux amélioré
                case TranceSeekRegularItem.EnhancedSedative: // Calmant amélioré
                case TranceSeekRegularItem.EnhancedBlessedWater: // Eau bénite amélioré
                case TranceSeekRegularItem.EnhancedAntibodies: // Anticorps amélioré
                {
                    TranceSeekAPI.TryRemoveItemStatuses(_v);
                    foreach (BattleStatusId statusID in _v.Command.Item.Status.ToStatusList())
                        _v.TargetState().ProtectStatus.Add(statusID.ToBattleStatus(), 2);

                    break;
                }
                case TranceSeekRegularItem.PowerfulSerum: // Puissant sérum
                case TranceSeekRegularItem.PowerfulEcho: // Puissant echo
                case TranceSeekRegularItem.PowerfulCollyrium: // Puissant Collyre
                case TranceSeekRegularItem.PowerfulMild: // Puissant Doux
                case TranceSeekRegularItem.PowerfulSedative: // Puissant Calmant
                case TranceSeekRegularItem.PowerfulBlessedWater: // Puissante Eau bénite
                case TranceSeekRegularItem.PowerfulAntibodies: // Puissant Anticorps
                {
                    TranceSeekAPI.TryRemoveItemStatuses(_v);
                    foreach (BattleStatusId statusID in _v.Command.Item.Status.ToStatusList())
                        _v.TargetState().ProtectStatus.Add(statusID.ToBattleStatus(), 3);

                    break;
                }
                case TranceSeekRegularItem.Tonic: // Remontant
                case TranceSeekRegularItem.TonicPlus: // Remontant +
                case TranceSeekRegularItem.SecondeLife: // Secondie Vie
                case TranceSeekRegularItem.ThirdLife: // Troisième Vie
                {
                    if (_v.Command.ItemId == TranceSeekRegularItem.Tonic || _v.Command.ItemId == TranceSeekRegularItem.TonicPlus || _v.Command.ItemId == TranceSeekRegularItem.SecondeLife || _v.Command.ItemId == TranceSeekRegularItem.ThirdLife)
                    {
                        if (!_v.Target.CanBeRevived() || !_v.Target.IsUnderAnyStatus(BattleStatus.Death))
                        {
                            _v.Context.Flags |= BattleCalcFlags.Miss;
                            return;
                        }
                        if (HitRateForZombie() && !TranceSeekAPI.TryMagicHit(_v))
                            return;

                        if (_v.Target.IsZombie && !_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill))
                        {
                            _v.Target.Kill();
                            return;
                        }

                        if (!_v.Target.CheckIsPlayer())
                            return;

                        _v.Target.RemoveStatus(BattleStatus.Death);
                        if (_v.Command.ItemId == TranceSeekRegularItem.Tonic || _v.Command.ItemId == TranceSeekRegularItem.SecondeLife) // Remontant
                            HPHeal = (int)(_v.Target.MaximumHp / 2);
                        else if (_v.Command.ItemId == TranceSeekRegularItem.TonicPlus || _v.Command.ItemId == TranceSeekRegularItem.ThirdLife) // Remontant +
                            HPHeal = (int)(_v.Target.MaximumHp);
                    }

                    TranceSeekAPI.TryRemoveItemStatuses(_v);
                    Boolean Message = false;
                    string ItemName = FF9TextTool.ItemName(_v.Command.ItemId);
                    foreach (BattleStatusId statusID in _v.Command.Item.Status.ToStatusList())
                    {
                        BattleStatus status = statusID.ToBattleStatus();
                        _v.TargetState().ProtectStatus.Add(status, 255);

                        Int32 wait = (short)((400 + (_v.Caster.Will * 3)) * 30);
                        _v.Target.AddDelayedModifier(
                        target => (wait -= target.Data.cur.at_coef * BattleState.ATBTickCount) > 0,
                        target =>
                        {
                            _v.TargetState().ProtectStatus.Remove(status);
                            if (!Message)
                            {
                                Dictionary<String, String> localizedStatusProtect = new Dictionary<String, String>
                                {
                                { "US", $"- {ItemName}" },
                                { "UK", $"- {ItemName}" },
                                { "JP", $"- {ItemName}" },
                                { "ES", $"- {ItemName}" },
                                { "FR", $"- {ItemName}" },
                                { "GR", $"- {ItemName}" },
                                { "IT", $"- {ItemName}" },
                                };
                                btl2d.Btl2dReqSymbolMessage(target.Data, "[38FF1F]", localizedStatusProtect, HUDMessage.MessageStyle.DAMAGE, 5);
                                Message = true;
                            }
                        }
                        );                         
                    }
                    break;
                }
                case TranceSeekRegularItem.ReversibleSerum: // Sérum réversible
                case TranceSeekRegularItem.ReversibleEcho: // Echo réversible
                case TranceSeekRegularItem.ReversibleCollyrium: // Collyre réversible
                case TranceSeekRegularItem.ReversibleMild: // Doux réversible
                case TranceSeekRegularItem.ReversibleSedative: // Calmant réversible
                case TranceSeekRegularItem.ReversibleBlessedWater: // Eau bénite réversible
                case TranceSeekRegularItem.ReversibleAntibodies: // Anticorps réversible
                {
                    if (_v.Target.IsUnderAnyStatus(_v.Command.Item.Status))
                    {
                        TranceSeekAPI.TryRemoveItemStatuses(_v);
                        _v.Target.AlterStatus(BattleStatus.Regen, _v.Target);
                    }
                    break;
                }
                case TranceSeekRegularItem.PRemedy: // Remède P
                case TranceSeekRegularItem.PRemedyPlus: // Remède P +
                case TranceSeekRegularItem.SRemedy: // Remède M
                case TranceSeekRegularItem.SRemedyPlus: // Remède M +
                case TranceSeekRegularItem.DRemedy: // Remède C
                case TranceSeekRegularItem.DRemedyPlus: // Remède C +
                case TranceSeekRegularItem.FRemedy: // Remède F
                case TranceSeekRegularItem.FRemedyPlus: // Remède F +
                case TranceSeekRegularItem.TRemedy: // Remède S
                case TranceSeekRegularItem.TRemedyPlus: // Remède S +
                case TranceSeekRegularItem.ZRemedy: // Remède Z
                case TranceSeekRegularItem.ZRemedyPlus: // Remède Z +
                case TranceSeekRegularItem.VRemedy: // Remède V
                case TranceSeekRegularItem.VRemedyPlus: // Remède V +
                {
                    TranceSeekAPI.TryRemoveItemStatuses(_v);
                    Boolean Message = false;
                    string ItemName = FF9TextTool.ItemName(_v.Command.ItemId);
                    foreach (BattleStatusId statusID in _v.Command.Item.Status.ToStatusList())
                    {
                        BattleStatus status = statusID.ToBattleStatus();
                        _v.TargetState().ProtectStatus.Add(status, 255);

                        Int32 wait = (short)((400 + (_v.Caster.Will * 3)) * 30);
                        _v.Target.AddDelayedModifier(
                        target => (wait -= target.Data.cur.at_coef * BattleState.ATBTickCount) > 0,
                        target =>
                        {
                            _v.TargetState().ProtectStatus.Remove(status);
                            if (!Message)
                            {
                                Dictionary<String, String> localizedStatusProtect = new Dictionary<String, String>
                                {
                                { "US", $"- {ItemName}" },
                                { "UK", $"- {ItemName}" },
                                { "JP", $"- {ItemName}" },
                                { "ES", $"- {ItemName}" },
                                { "FR", $"- {ItemName}" },
                                { "GR", $"- {ItemName}" },
                                { "IT", $"- {ItemName}" },
                                };
                                btl2d.Btl2dReqSymbolMessage(target.Data, "[38FF1F]", localizedStatusProtect, HUDMessage.MessageStyle.DAMAGE, 5);
                                Message = true;
                            }
                        }
                        );
                    }
                    break;
                }
                case TranceSeekRegularItem.HolySerum: // Sérum sacré
                case TranceSeekRegularItem.HolyEcho: // Echo sacré
                case TranceSeekRegularItem.HolyCollyrium: // Collyre sacré
                case TranceSeekRegularItem.HolyMild: // Doux sacré
                case TranceSeekRegularItem.HolySedative: // Calmant sacré
                case TranceSeekRegularItem.HolyBlessedWater: // Eau bénite sacrée
                case TranceSeekRegularItem.HolyAntibodies: // Anticorps sacré
                {
                    if (_v.Target.IsUnderAnyStatus(_v.Command.Item.Status))
                    {
                        TranceSeekAPI.TryRemoveItemStatuses(_v);
                        _v.Target.AlterStatus(BattleStatus.AutoLife, _v.Target);
                    }
                    break;
                }
                case TranceSeekRegularItem.CompleteSerum: // Sérum complet
                case TranceSeekRegularItem.TotalSerum: // Sérum total
                case TranceSeekRegularItem.CompleteEcho: // Echo complet            
                case TranceSeekRegularItem.TotalEcho: // Echo total
                case TranceSeekRegularItem.CompleteCollyrium: // Collyre complet
                case TranceSeekRegularItem.TotaleCollyrium: // Collyre total
                case TranceSeekRegularItem.CompleteMild: // Doux complet
                case TranceSeekRegularItem.TotaleMild: // Doux total
                case TranceSeekRegularItem.CompleteSedative: // Calmant complet
                case TranceSeekRegularItem.TotaleSedative: // Calmant total
                case TranceSeekRegularItem.CompleteBlessedWater: // Eau bénite complete
                case TranceSeekRegularItem.TotaleBlessedWater: // Eau bénite totale
                case TranceSeekRegularItem.CompleteAntibodies: // Anticorps complet
                case TranceSeekRegularItem.TotaleAntibodies: // Anticorps total
                case TranceSeekRegularItem.Rejuvenation: // Régénération
                case TranceSeekRegularItem.TotaleRejuvenation: // Régénération totale
                case TranceSeekRegularItem.PowerfulRejuvenation: // Regénération puissante
                case TranceSeekRegularItem.PerfectRejuvenation: // Regénération parfaite
                {
                    HPHeal = (int)(_v.Target.MaximumHp);
                    MPHeal = (int)(_v.Target.MaximumMp);
                    TranceSeekAPI.TryRemoveItemStatuses(_v);
                    break;
                }
                case TranceSeekRegularItem.Boccidote: // Boccidote
                case TranceSeekRegularItem.Collyridote: // Lasidote
                case TranceSeekRegularItem.Softidote: // Défidote          
                case TranceSeekRegularItem.Annoyntidote: // Antidrouil
                case TranceSeekRegularItem.Tagidote: // Cachidote
                case TranceSeekRegularItem.Vaccidote: // Vaccidote
                case TranceSeekRegularItem.Collyrecho: // Boccik
                case TranceSeekRegularItem.Echoft: // Déficca
                case TranceSeekRegularItem.Echotment: // Bobrouil
                case TranceSeekRegularItem.Vaccecho: // Vacca
                case TranceSeekRegularItem.SoftDrop: // Lasijeur
                case TranceSeekRegularItem.AnnoeyeDrop: // Désembrik
                case TranceSeekRegularItem.MagicDrop: // Casik
                case TranceSeekRegularItem.Vacceye: // Vaccik
                case TranceSeekRegularItem.Annoyft: // Désemjeur
                case TranceSeekRegularItem.Tagoft: // Cafijeur
                case TranceSeekRegularItem.Vasoft: // Déficcin
                case TranceSeekRegularItem.Tagoyntment: // Cachembrouil
                case TranceSeekRegularItem.Tagine: // Caccin
                {
                    Int32 statuspresent = 0;
                    foreach (BattleStatusId statusID in _v.Command.Item.Status.ToStatusList())
                    {
                        BattleStatus status = statusID.ToBattleStatus();
                        if ((_v.Target.CurrentStatus & status) != 0)
                            statuspresent++;
                    }
                    TranceSeekAPI.TryRemoveItemStatuses(_v);
                    if (statuspresent > 1)
                    {
                        HPHeal = (int)(_v.Target.MaximumHp) / 4;
                        MPHeal = (int)(_v.Target.MaximumMp) / 4;
                    }
                    break;
                }
                case TranceSeekRegularItem.MegaAntidote: // Mega Antidote
                case TranceSeekRegularItem.MegaEchoDrops: // Mega Bocca
                case TranceSeekRegularItem.MegaEyeDrops: // Mega Lasik          
                case TranceSeekRegularItem.MegaSoft: // Mega Défijeur
                case TranceSeekRegularItem.MegaAnnoyntment: // Mega Désembrouil
                case TranceSeekRegularItem.MegaMagicTag: // Mega Cachet
                case TranceSeekRegularItem.MegaVaccine: // Mega Vaccin
                case TranceSeekRegularItem.XRemedy: // Remède X
                case TranceSeekRegularItem.MegaRemedy: // Mega Remède
                {
                    TranceSeekAPI.TryRemoveItemStatuses(_v);
                    return;
                }
            }
            _v.Context.Flags = 0;

            if (HPHeal > 0)
                _v.Target.Flags |= (_v.Target.IsZombie ? CalcFlag.HpAlteration : CalcFlag.HpDamageOrHeal);

            if (MPHeal > 0)
                _v.Target.Flags |= (_v.Target.IsZombie ? CalcFlag.MpAlteration : CalcFlag.MpDamageOrHeal);

            if ((_v.Target.Flags & CalcFlag.HpAlteration) != 0)
                _v.Target.HpDamage = HPHeal;

            if ((_v.Target.Flags & CalcFlag.MpAlteration) != 0)
                _v.Target.MpDamage = MPHeal;

            foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(_v.Caster))
                saFeature.TriggerOnAbility(_v, "CalcDamage", false);
            foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(_v.Target))
                saFeature.TriggerOnAbility(_v, "CalcDamage", true);
        }

        private Boolean HitRateForZombie()
        {
            if (_v.Target.IsZombie)
            {
                TranceSeekAPI.MagicAccuracy(_v);
                return true;
            }
            return false;
        }
    }
}


