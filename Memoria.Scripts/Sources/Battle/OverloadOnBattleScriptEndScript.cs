using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Data;
using Memoria.Database;
using Memoria.Prime;
using System;
using System.Collections.Generic;
using static Memoria.Scripts.Battle.TranceSeekAPI;

namespace Memoria.Scripts.Battle
{
    public class OverloadOnBattleScriptEndScript : IOverloadOnBattleScriptEndScript
    {
        public void OnBattleScriptEnd(BattleCalculator v)
        {
            SOS_SA(v);

            if (Configuration.Battle.Speed == 2)
            {
                EikoMougMechanic(v);
            }
            else // [TODO] Can be improved ?
            {
                Int32 counter = 15;
                v.Caster.AddDelayedModifier(
                    caster => (counter -= BattleState.ATBTickCount) > 0,
                    caster =>
                    {
                        EikoMougMechanic(v);
                    }
                );
            }

            if (v.Command.Id == BattleCommandId.BlackMagic || v.Command.Id == BattleCommandId.BlackMagic || v.Command.Id == BattleCommandId.SwordAct || v.Command.Id == TranceSeekBattleCommand.Witchcraft)
                ViviFocus(v);
        }

        public static void SOS_SA(BattleCalculator v)
        {
            Log.Message("v.Target = " + v.Target.Name);
            Log.Message("v.Caster = " + v.Caster.Name);
            Log.Message("SOS !");
            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Protect_Boosted) && (SpecialSAEffect[v.Target.Data][14] & 1) == 0 && v.Target.CurrentHp < (v.Target.MaximumHp / 2))
            {
                v.Target.AlterStatus(BattleStatus.Protect, v.Target);
                SpecialSAEffect[v.Target.Data][14] += 1;
            }
            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Protect) && (SpecialSAEffect[v.Target.Data][14] & 2) == 0 && v.Target.IsUnderAnyStatus(BattleStatus.LowHP) && !v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Protect_Boosted))
            {
                v.Target.AlterStatus(BattleStatus.Protect, v.Target);
                SpecialSAEffect[v.Target.Data][14] += 2;
            }
            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Shell_Boosted) && (SpecialSAEffect[v.Target.Data][14] & 4) == 0 && v.Target.CurrentHp < (v.Target.MaximumHp / 2))
            {
                v.Target.AlterStatus(BattleStatus.Shell, v.Target);
                SpecialSAEffect[v.Target.Data][14] += 4;
            }
            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Shell) && (SpecialSAEffect[v.Target.Data][14] & 8) == 0 && v.Target.IsUnderAnyStatus(BattleStatus.LowHP) && !v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Shell_Boosted))
            {
                v.Target.AlterStatus(BattleStatus.Shell, v.Target);
                SpecialSAEffect[v.Target.Data][14] += 8;
            }
            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Regen_Boosted) && (SpecialSAEffect[v.Target.Data][14] & 16) == 0 && v.Target.CurrentHp < (v.Target.MaximumHp / 2))
            {
                v.Target.AlterStatus(BattleStatus.Regen, v.Target);
                SpecialSAEffect[v.Target.Data][14] += 16;
            }
            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Regen) && (SpecialSAEffect[v.Target.Data][14] & 32) == 0 && v.Target.IsUnderAnyStatus(BattleStatus.LowHP) && !v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Regen_Boosted)) 
            {
                v.Target.AlterStatus(BattleStatus.Regen, v.Target);
                SpecialSAEffect[v.Target.Data][14] += 32;
            }
            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Haste_Boosted) && (SpecialSAEffect[v.Target.Data][14] & 64) == 0 && v.Target.CurrentHp < (v.Target.MaximumHp / 2))
            {
                v.Target.AlterStatus(BattleStatus.Haste, v.Target);
                SpecialSAEffect[v.Target.Data][14] += 64;
            }
            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Haste) && (SpecialSAEffect[v.Target.Data][14] & 128) == 0 && v.Target.IsUnderAnyStatus(BattleStatus.LowHP) && !v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Haste_Boosted))
            {
                v.Target.AlterStatus(BattleStatus.Haste, v.Target);
                SpecialSAEffect[v.Target.Data][14] += 128;
            }
            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Reflect_Boosted) && (SpecialSAEffect[v.Target.Data][14] & 256) == 0 && v.Target.CurrentHp < (v.Target.MaximumHp / 2))
            {
                v.Target.AlterStatus(BattleStatus.Reflect, v.Target);
                SpecialSAEffect[v.Target.Data][14] += 256;
            }
            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Reflect) && (SpecialSAEffect[v.Target.Data][14] & 512) == 0 && v.Target.IsUnderAnyStatus(BattleStatus.LowHP) && !v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Reflect_Boosted))
            {
                v.Target.AlterStatus(BattleStatus.Reflect, v.Target);
                SpecialSAEffect[v.Target.Data][14] += 512;
            }
            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Vanish_Boosted) && (SpecialSAEffect[v.Target.Data][14] & 1024) == 0 && v.Target.CurrentHp < (v.Target.MaximumHp / 2))
            {
                v.Target.AlterStatus(BattleStatus.Vanish, v.Target);
                SpecialSAEffect[v.Target.Data][14] += 1024;
            }
            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Vanish) && (SpecialSAEffect[v.Target.Data][14] & 2048) == 0 && v.Target.IsUnderAnyStatus(BattleStatus.LowHP) && !v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Vanish_Boosted))
            {
                v.Target.AlterStatus(BattleStatus.Vanish, v.Target);
                SpecialSAEffect[v.Target.Data][14] += 2048;
            }
        }

        public static void EikoMougMechanic(BattleCalculator v)
        {
            if (FF9StateSystem.Common.FF9.party.IsInParty(CharacterId.Eiko) && v.Command.ScriptId != 64 && v.Command.ScriptId != 164 && v.Command.Id != BattleCommandId.Counter && v.Command.Data.info.effect_counter == 1)
            {
                if (v.Caster.IsPlayer && v.Target.IsUnderAnyStatus(BattleStatus.Death) && (v.Command.ScriptId == 13 || v.Command.ScriptId == 72)) // Don't trigger if a player revive someone.
                    return;

                BattleUnit Eiko = BattleState.GetPlayerUnit(CharacterId.Eiko);

                if (StateMoug[Eiko.Data] == 0 && !Eiko.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump | BattleStatus.Heat) && v.Caster.Data != Eiko.Data)
                {
                    int ChanceMoug = (Eiko.HasSupportAbilityByIndex((SupportAbility)1224) ? 20 : (Eiko.HasSupportAbilityByIndex((SupportAbility)224) ? 15 : 10));

                    if (Comn.random16() % 100 > ChanceMoug)
                        return;

                    ushort TargetId = v.Caster.Id;
                    StateMoug[Eiko.Data] = 1;
                    List<BattleAbilityId> MougAAList = new List<BattleAbilityId>();
                    foreach (BattleAbilityId abilId in CharacterCommands.Commands[(BattleCommandId)1049].EnumerateAbilities()) // CMD Kupo (not used for Eiko)
                    {
                        Boolean AddAA = true;
                        switch (abilId)
                        {
                            case (BattleAbilityId)2000: // Mog Cure
                            {
                                if (Eiko.CurrentHp > Eiko.MaximumHp / 2)
                                    AddAA = false;
                                break;
                            }
                            case (BattleAbilityId)2001: // Mog Hug
                            {
                                if (Eiko.CurrentMp > Eiko.MaximumMp / 2)
                                    AddAA = false;
                                break;
                            }
                            case (BattleAbilityId)2002: // Mog Regen
                            {
                                if (Eiko.IsUnderAnyStatus(BattleStatus.Regen))
                                    AddAA = false;
                                break;
                            }
                            case (BattleAbilityId)2004: // Mog Mirror
                            {
                                if (Eiko.IsUnderAnyStatus(BattleStatus.Vanish))
                                    AddAA = false;
                                break;
                            }
                            case (BattleAbilityId)2005: // Mog AutoLife
                            {
                                if (Eiko.Level < 35 || Eiko.IsUnderAnyStatus(BattleStatus.AutoLife))
                                    AddAA = false;
                                break;
                            }
                            case (BattleAbilityId)2006: // Mog Esuna
                            {
                                if (!Eiko.IsUnderAnyStatus(BattleStatus.Heat | BattleStatus.Poison | BattleStatus.Venom | BattleStatus.Silence | BattleStatus.Blind | BattleStatus.Trouble | BattleStatus.Berserk | BattleStatus.Mini | TranceSeekStatus.Vieillissement))
                                    AddAA = false;
                                break;
                            }
                            case (BattleAbilityId)2007: // Mog Support
                            {
                                if (Eiko.Level < 30 || StackBreakOrUpStatus[Eiko.Data][1] >= 50 || StackBreakOrUpStatus[Eiko.Data][3] >= 50)
                                    AddAA = false;
                                break;
                            }
                            case (BattleAbilityId)2008: // Mog Life
                            {
                                AddAA = false; // Specific, handle aside.
                                break;
                            }
                            case (BattleAbilityId)2009: // Mog Flare
                            case (BattleAbilityId)2010: // Mog Holy
                            {
                                if (FF9StateSystem.EventState.ScenarioCounter < 9990) // The party finds Hilda
                                    AddAA = false;
                                else
                                {
                                    Boolean TargetAvailable = false;
                                    foreach (BattleUnit monster in BattleState.EnumerateUnits())
                                        if (!monster.IsPlayer && monster.IsTargetable && !monster.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump | BattleStatus.Reflect))
                                            TargetAvailable = true;

                                    if (!TargetAvailable)
                                        AddAA = false;
                                }
                                break;
                            }
                            case (BattleAbilityId)2011: // Moga Cure
                            {
                                if (Eiko.Level < 40)
                                    AddAA = false;
                                else
                                {
                                    uint CurrentHPTeam = 0;
                                    uint CurrentMaxHPTeam = 0;
                                    Boolean ZombiePresent = false;
                                    foreach (BattleUnit unit in BattleState.EnumerateUnits())
                                        if (unit.IsPlayer && unit.IsTargetable && !unit.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump))
                                        {
                                            CurrentHPTeam += unit.CurrentHp;
                                            CurrentMaxHPTeam += unit.MaximumHp;
                                            if (unit.IsZombie)
                                                ZombiePresent = true;
                                        }
                                    if (CurrentHPTeam > (CurrentMaxHPTeam / 2) || ZombiePresent)
                                        AddAA = false;
                                }
                                break;
                            }
                            case (BattleAbilityId)2012: // Moga Hug
                            {
                                if (Eiko.Level < 50)
                                    AddAA = false;
                                else
                                {
                                    uint CurrentMPTeam = 0;
                                    uint CurrentMaxMPTeam = 0;
                                    Boolean ZombiePresent = false;
                                    foreach (BattleUnit unit in BattleState.EnumerateUnits())
                                        if (unit.IsPlayer && unit.IsTargetable && !unit.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump))
                                        {
                                            CurrentMPTeam += unit.CurrentMp;
                                            CurrentMaxMPTeam += unit.MaximumMp;
                                            if (unit.IsZombie)
                                                ZombiePresent = true;
                                        }
                                    if (CurrentMPTeam > (CurrentMaxMPTeam / 4) || ZombiePresent)
                                        AddAA = false;
                                }
                                break;
                            }
                            case (BattleAbilityId)2014: // Moga Shield
                            {
                                if (Eiko.Level < 60)
                                    AddAA = false;
                                break;
                            }
                            case (BattleAbilityId)2013: // Moga Regen
                            {
                                if (Eiko.Level < 70)
                                    AddAA = false;
                                else
                                {
                                    Boolean StatusToApply = false;
                                    foreach (BattleUnit unit in BattleState.EnumerateUnits())
                                        if (unit.IsPlayer && unit.IsTargetable && !unit.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump | BattleStatus.Regen))
                                            StatusToApply = true;

                                    if (!StatusToApply)
                                        AddAA = false;
                                }
                                break;
                            }
                            case (BattleAbilityId)2017: // Moga Esuna
                            {
                                if (Eiko.Level < 75)
                                    AddAA = false;
                                else
                                {
                                    Boolean StatusToCure = false;
                                    foreach (BattleUnit unit in BattleState.EnumerateUnits())
                                        if (unit.IsPlayer && unit.IsTargetable && !unit.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump) && unit.IsUnderAnyStatus(BattleStatus.Poison | BattleStatus.Venom | BattleStatus.Silence | BattleStatus.Blind | BattleStatus.Trouble | BattleStatus.Mini | BattleStatus.Berserk | TranceSeekStatus.Vieillissement))
                                            StatusToCure = true;

                                    if (!StatusToCure)
                                        AddAA = false;
                                }
                                break;
                            }
                            case (BattleAbilityId)2015: // Moga Mirror
                            {
                                if (Eiko.Level < 80)
                                    AddAA = false;
                                else
                                {
                                    Boolean StatusToApply = false;
                                    foreach (BattleUnit unit in BattleState.EnumerateUnits())
                                        if (unit.IsPlayer && unit.IsTargetable && !unit.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump | BattleStatus.Vanish))
                                            StatusToApply = true;

                                    if (!StatusToApply)
                                        AddAA = false;
                                }
                                break;
                            }
                            case (BattleAbilityId)2018: // Moga Support
                            {
                                if (Eiko.Level < 85)
                                    AddAA = false;
                                else
                                {
                                    Boolean StatusToApply = false;
                                    foreach (BattleUnit unit in BattleState.EnumerateUnits())
                                        if (unit.IsPlayer && unit.IsTargetable && !unit.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump) && (StackBreakOrUpStatus[unit.Data][1] < 50 || StackBreakOrUpStatus[unit.Data][3] < 50))
                                            StatusToApply = true;

                                    if (!StatusToApply)
                                        AddAA = false;
                                }
                                break;
                            }
                            case (BattleAbilityId)2016: // Moga AutoLife
                            {
                                if (Eiko.Level < 90)
                                    AddAA = false;
                                else
                                {
                                    Boolean StatusToApply = false;
                                    foreach (BattleUnit unit in BattleState.EnumerateUnits())
                                        if (unit.IsPlayer && unit.IsTargetable && !unit.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump | BattleStatus.AutoLife))
                                            StatusToApply = true;

                                    if (!StatusToApply)
                                        AddAA = false;
                                }
                                break;
                            }
                            case (BattleAbilityId)2019: // Mouga Homing
                            {
                                if (Eiko.Level < 99)
                                    AddAA = false;
                                break;
                            }
                        }
                        if (AddAA)
                            MougAAList.Add(abilId);
                    }

                    BattleAbilityId MougAAChoosen = MougAAList[GameRandom.Next16() % MougAAList.Count];
                    TargetType TargetAA = FF9StateSystem.Battle.FF9Battle.aa_data[MougAAChoosen].Info.Target;
                    Boolean TargetDefaultAlly = FF9StateSystem.Battle.FF9Battle.aa_data[MougAAChoosen].Info.DefaultAlly;

                    if (TargetDefaultAlly)
                    {
                        if (TargetAA == TargetType.Self)
                            TargetId = Eiko.Id;
                        else
                            TargetId = 15;
                    }
                    else
                    {
                        if (TargetAA == TargetType.AllEnemy)
                            TargetId = 240;
                        else
                            TargetId = BattleState.GetRandomUnitId(isPlayer: false);
                    }

                    if (Comn.random16() % 100 <= ChanceMoug) // Mog Life
                    {
                        List<UInt16> candidates = new List<UInt16>(4);
                        for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next)
                            if (next.bi.player == 1 && btl_stat.CheckStatus(next, BattleStatus.Death) && next.bi.target != 0)
                                candidates.Add(next.btl_id);
                        if (candidates.Count > 0)
                        {
                            TargetId = candidates[UnityEngine.Random.Range(0, candidates.Count)];
                            MougAAChoosen = (BattleAbilityId)2008;
                        }
                    }
                    btl_cmd.SetCounter(Eiko, BattleCommandId.Counter, (int)MougAAChoosen, TargetId);
                }
            }
        }

        public static void ViviFocus(BattleCalculator v)
        {
            if (ViviPassive[v.Caster.Data][2] == 0)
            {
                ViviPassive[v.Caster.Data][2] = 1;
                Int32 counter = 25;
                v.Caster.AddDelayedModifier(
                    caster => (counter -= BattleState.ATBTickCount) > 0,
                    caster =>
                    {
                        ViviPassive[v.Caster.Data][1] = 0;
                        ViviPassive[v.Caster.Data][2] = 0;
                    }
                );
                if (v.Caster.PlayerIndex == CharacterId.Vivi)
                {
                    if ((v.Command.Id == BattleCommandId.BlackMagic || v.Command.Id == BattleCommandId.DoubleBlackMagic) || v.Command.Id == (BattleCommandId)1032)
                    {
                        if (ViviPassive[v.Caster.Data][1] == 0)
                        {
                            ViviPassive[v.Caster.Data][1] = (ushort)(v.Command.TargetCount);
                            if (FF9TextTool.ActionAbilityName(ViviPreviousSpell[v.Caster.Data]) != v.Command.AbilityName)
                            {
                                Int32 BonusFocusMax = 0;
                                switch (v.Caster.Weapon)
                                {
                                    case RegularItem.OakStaff:
                                        BonusFocusMax += 10;
                                        break;
                                    case RegularItem.CypressPile:
                                        BonusFocusMax += 10;
                                        break;
                                    case RegularItem.OctagonRod:
                                        BonusFocusMax += 15;
                                        break;
                                    case RegularItem.HighMageStaff:
                                        BonusFocusMax += 25;
                                        break;
                                    case RegularItem.MaceOfZeus:
                                        BonusFocusMax += 50;
                                        break;
                                }
                                if (ViviPassive[v.Caster.Data][0] < (50 + BonusFocusMax))
                                {
                                    ViviPassive[v.Caster.Data][0] += v.Caster.HasSupportAbilityByIndex((SupportAbility)207) ? 10 : 5; // SA Bobbin
                                }
                                Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                                {
                                    { "US", $"Focus +{ViviPassive[v.Caster.Data][0]}%!" },
                                    { "UK", $"Focus +{ViviPassive[v.Caster.Data][0]}%!" },
                                    { "JP", $"フォーカス +{ViviPassive[v.Caster.Data][0]}%!" },
                                    { "ES", $"¡Focus +{ViviPassive[v.Caster.Data][0]}%!" },
                                    { "FR", $"Focus +{ViviPassive[v.Caster.Data][0]}% !" },
                                    { "GR", $"Focus +{ViviPassive[v.Caster.Data][0]}%!" },
                                    { "IT", $"Focus +{ViviPassive[v.Caster.Data][0]}%!" },
                                };
                                btl2d.Btl2dReqSymbolMessage(v.Caster.Data, "[BA55D3]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 40);
                            }
                            else
                            {
                                if (v.Caster.HasSupportAbilityByIndex((SupportAbility)1207)) // SA Bobbin+
                                {
                                    ViviPassive[v.Caster.Data][0] /= 2;
                                    if (ViviPassive[v.Caster.Data][0] % 10U == 5)
                                        ViviPassive[v.Caster.Data][0] += 5;

                                    Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                                    {
                                        { "US", $"Focus +{ViviPassive[v.Caster.Data][0]}%!" },
                                        { "UK", $"Focus +{ViviPassive[v.Caster.Data][0]}%!" },
                                        { "JP", $"フォーカス +{ViviPassive[v.Caster.Data][0]}%!" },
                                        { "ES", $"¡Focus +{ViviPassive[v.Caster.Data][0]}%!" },
                                        { "FR", $"Focus +{ViviPassive[v.Caster.Data][0]}% !" },
                                        { "GR", $"Focus +{ViviPassive[v.Caster.Data][0]}%!" },
                                        { "IT", $"Focus +{ViviPassive[v.Caster.Data][0]}%!" },
                                    };
                                    btl2d.Btl2dReqSymbolMessage(v.Caster.Data, "[BA55D3]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 40);
                                }
                                else
                                {
                                    ViviPassive[v.Caster.Data][0] = 0;
                                    Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                                    {
                                        { "US", "- Focus!" },
                                        { "UK", "- Focus!" },
                                        { "JP", "- フォーカス!" },
                                        { "ES", "¡- Focus!" },
                                        { "FR", "- Focus !" },
                                        { "GR", "- Focus!" },
                                        { "IT", "- Focus!" },
                                    };
                                    btl2d.Btl2dReqSymbolMessage(v.Caster.Data, "[DC143C]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 40);
                                }
                            }
                            ViviPreviousSpell[v.Caster.Data] = v.Command.AbilityId;
                        }
                        ViviPassive[v.Caster.Data][1]--;
                        if (ViviPassive[v.Caster.Data][1] < 0)
                            ViviPassive[v.Caster.Data][1] = 0;

                        if (v.Command.ScriptId != 17) // Magic Gravity didn't get damage boost.
                        {
                            v.Context.Attack += (v.Context.Attack * ViviPassive[v.Caster.Data][0]) / 100;
                            v.Command.HitRate += (v.Command.HitRate * ViviPassive[v.Caster.Data][0]) / 100;
                        }
                    }
                }
                else if (v.Caster.PlayerIndex == CharacterId.Steiner && v.Command.Id == BattleCommandId.MagicSword && v.Command.AbilityId != (BattleAbilityId)1571 &&
                    v.Command.AbilityId != (BattleAbilityId)1572 && v.Command.AbilityId != (BattleAbilityId)1573 && v.Command.AbilityId != (BattleAbilityId)1574)
                {
                    if (ViviPassive[v.Caster.Data][1] == 0)
                    {
                        ViviPassive[v.Caster.Data][1] = (ushort)(v.Command.TargetCount);
                        foreach (BattleUnit Vivi in BattleState.EnumerateUnits())
                        {
                            if (Vivi.IsPlayer && Vivi.PlayerIndex == CharacterId.Vivi)
                            {
                                if (FF9TextTool.ActionAbilityName(ViviPreviousSpell[Vivi.Data]) != v.Command.AbilityName)
                                {
                                    Int32 BonusFocusMax = 0;
                                    switch (Vivi.Weapon)
                                    {
                                        case RegularItem.OakStaff:
                                            BonusFocusMax += 10;
                                            break;
                                        case RegularItem.CypressPile:
                                            BonusFocusMax += 10;
                                            break;
                                        case RegularItem.OctagonRod:
                                            BonusFocusMax += 15;
                                            break;
                                        case RegularItem.HighMageStaff:
                                            BonusFocusMax += 25;
                                            break;
                                        case RegularItem.MaceOfZeus:
                                            BonusFocusMax += 50;
                                            break;
                                    }
                                    if (ViviPassive[Vivi.Data][0] < (50 + BonusFocusMax))
                                    {
                                        ViviPassive[Vivi.Data][0] += Vivi.HasSupportAbilityByIndex((SupportAbility)207) ? 10 : 5; // SA Bobbin;
                                    }
                                    Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                                    {
                                        { "US", $"Focus +{ViviPassive[Vivi.Data][0]}%!" },
                                        { "UK", $"Focus +{ViviPassive[Vivi.Data][0]}%!" },
                                        { "JP", $"フォーカス +{ViviPassive[Vivi.Data][0]}%!" },
                                        { "ES", $"¡Focus +{ViviPassive[Vivi.Data][0]}%!" },
                                        { "FR", $"Focus +{ViviPassive[Vivi.Data][0]}% !" },
                                        { "GR", $"Focus +{ViviPassive[Vivi.Data][0]}%!" },
                                        { "IT", $"Focus +{ViviPassive[Vivi.Data][0]}%!" },
                                    };
                                    btl2d.Btl2dReqSymbolMessage(Vivi.Data, "[BA55D3]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 40);
                                    v.Context.Attack += v.Context.Attack * (ViviPassive[Vivi.Data][0] / 100);
                                    v.Command.HitRate += v.Command.HitRate * (ViviPassive[Vivi.Data][0] / 100);
                                }
                                else
                                {
                                    if (v.Caster.HasSupportAbilityByIndex((SupportAbility)1207)) // SA Bobbin+
                                    {
                                        ViviPassive[Vivi.Data][0] /= 2;
                                        if (ViviPassive[Vivi.Data][0] % 10U == 5)
                                            ViviPassive[Vivi.Data][0] += 5;

                                        Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                                        {
                                            { "US", $"Focus +{ViviPassive[Vivi.Data][0]}%!" },
                                            { "UK", $"Focus +{ViviPassive[Vivi.Data][0]}%!" },
                                            { "JP", $"フォーカス +{ViviPassive[Vivi.Data][0]}%!" },
                                            { "ES", $"¡Focus +{ViviPassive[Vivi.Data][0]}%!" },
                                            { "FR", $"Focus +{ViviPassive[Vivi.Data][0]}% !" },
                                            { "GR", $"Focus +{ViviPassive[Vivi.Data][0]}%!" },
                                            { "IT", $"Focus +{ViviPassive[Vivi.Data][0]}%!" },
                                        };
                                        btl2d.Btl2dReqSymbolMessage(Vivi.Data, "[BA55D3]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 40);
                                    }
                                    else
                                    {
                                        ViviPassive[Vivi.Data][0] = 0;
                                        Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                                        {
                                            { "US", "- Focus!" },
                                            { "UK", "- Focus!" },
                                            { "JP", "- フォーカス!" },
                                            { "ES", "¡- Focus!" },
                                            { "FR", "- Focus !" },
                                            { "GR", "- Focus!" },
                                            { "IT", "- Focus!" },
                                        };
                                        btl2d.Btl2dReqSymbolMessage(Vivi.Data, "[DC143C]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 40);
                                    }
                                }
                                ViviPreviousSpell[Vivi.Data] = v.Command.AbilityId;
                            }
                        }
                    }
                    ViviPassive[v.Caster.Data][1]--;
                    if (ViviPassive[v.Caster.Data][1] < 0)
                        ViviPassive[v.Caster.Data][1] = 0;
                }
            }
        }
    }
}
