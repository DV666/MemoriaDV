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
    public class OverloadOnBattleScriptEndScript //: IOverloadOnBattleScriptEndScript
    {
        public static void OnBattleScriptEnd(BattleCalculator v)
        {
            SOS_SA(v);

            //if (Configuration.Battle.Speed == 2)
            //{
                //EikoMougMechanic(v);
            //}
            //else // [TODO] Can be improved ?
            //{
            //    Int32 counter = 15;
            //    v.Caster.AddDelayedModifier(
            //        caster => (counter -= BattleState.ATBTickCount) > 0,
            //        caster =>
            //        {
            //            EikoMougMechanic(v);
            //        }
            //     );
            //}
        }

        public static void SOS_SA(BattleCalculator v)
        {
            if (v.Target.CurrentHp > (v.Target.MaximumHp / 2) && SpecialSAEffect[v.Target.Data][14] > 0 &&
                (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Protect_Boosted) || v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Shell_Boosted) ||
                v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Regen_Boosted) || v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Haste_Boosted) ||
                v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Reflect_Boosted) || v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Vanish_Boosted)))
            {
                if ((SpecialSAEffect[v.Target.Data][14] & 1) > 0)
                    SpecialSAEffect[v.Target.Data][14] -= 1;
                if ((SpecialSAEffect[v.Target.Data][14] & 4) > 0)
                    SpecialSAEffect[v.Target.Data][14] -= 4;
                if ((SpecialSAEffect[v.Target.Data][14] & 16) > 0)
                    SpecialSAEffect[v.Target.Data][14] -= 16;
                if ((SpecialSAEffect[v.Target.Data][14] & 64) > 0)
                    SpecialSAEffect[v.Target.Data][14] -= 64;
                if ((SpecialSAEffect[v.Target.Data][14] & 256) > 0)
                    SpecialSAEffect[v.Target.Data][14] -= 256;
                if ((SpecialSAEffect[v.Target.Data][14] & 1024) > 0)
                    SpecialSAEffect[v.Target.Data][14] -= 1024;
            }
            else if (!v.Target.IsUnderAnyStatus(BattleStatus.LowHP) && SpecialSAEffect[v.Target.Data][14] > 0 &&
                (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Protect) || v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Shell) ||
                v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Regen) || v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Haste) ||
                v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Reflect) || v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Vanish)))
            {
                if ((SpecialSAEffect[v.Target.Data][14] & 2) > 0)
                    SpecialSAEffect[v.Target.Data][14] -= 2;
                if ((SpecialSAEffect[v.Target.Data][14] & 8) > 0)
                    SpecialSAEffect[v.Target.Data][14] -= 8;
                if ((SpecialSAEffect[v.Target.Data][14] & 32) > 0)
                    SpecialSAEffect[v.Target.Data][14] -= 32;
                if ((SpecialSAEffect[v.Target.Data][14] & 128) > 0)
                    SpecialSAEffect[v.Target.Data][14] -= 128;
                if ((SpecialSAEffect[v.Target.Data][14] & 512) > 0)
                    SpecialSAEffect[v.Target.Data][14] -= 512;
                if ((SpecialSAEffect[v.Target.Data][14] & 2048) > 0)
                    SpecialSAEffect[v.Target.Data][14] -= 2048;
            }

            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Protect_Boosted) && (SpecialSAEffect[v.Target.Data][14] & 1) == 0 && v.Target.CurrentHp <= (v.Target.MaximumHp / 2))
            {
                v.Target.AlterStatus(BattleStatus.Protect, v.Target);
                SpecialSAEffect[v.Target.Data][14] += 1;
            }
            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Protect) && (SpecialSAEffect[v.Target.Data][14] & 2) == 0 && v.Target.IsUnderAnyStatus(BattleStatus.LowHP) && !v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Protect_Boosted))
            {
                v.Target.AlterStatus(BattleStatus.Protect, v.Target);
                SpecialSAEffect[v.Target.Data][14] += 2;
            }
            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Shell_Boosted) && (SpecialSAEffect[v.Target.Data][14] & 4) == 0 && v.Target.CurrentHp <= (v.Target.MaximumHp / 2))
            {
                v.Target.AlterStatus(BattleStatus.Shell, v.Target);
                SpecialSAEffect[v.Target.Data][14] += 4;
            }
            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Shell) && (SpecialSAEffect[v.Target.Data][14] & 8) == 0 && v.Target.IsUnderAnyStatus(BattleStatus.LowHP) && !v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Shell_Boosted))
            {
                v.Target.AlterStatus(BattleStatus.Shell, v.Target);
                SpecialSAEffect[v.Target.Data][14] += 8;
            }
            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Regen_Boosted) && (SpecialSAEffect[v.Target.Data][14] & 16) == 0 && v.Target.CurrentHp <= (v.Target.MaximumHp / 2))
            {
                v.Target.AlterStatus(BattleStatus.Regen, v.Target);
                SpecialSAEffect[v.Target.Data][14] += 16;
            }
            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Regen) && (SpecialSAEffect[v.Target.Data][14] & 32) == 0 && v.Target.IsUnderAnyStatus(BattleStatus.LowHP) && !v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Regen_Boosted)) 
            {
                v.Target.AlterStatus(BattleStatus.Regen, v.Target);
                SpecialSAEffect[v.Target.Data][14] += 32;
            }
            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Haste_Boosted) && (SpecialSAEffect[v.Target.Data][14] & 64) == 0 && v.Target.CurrentHp <= (v.Target.MaximumHp / 2))
            {
                v.Target.AlterStatus(BattleStatus.Haste, v.Target);
                SpecialSAEffect[v.Target.Data][14] += 64;
            }
            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Haste) && (SpecialSAEffect[v.Target.Data][14] & 128) == 0 && v.Target.IsUnderAnyStatus(BattleStatus.LowHP) && !v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Haste_Boosted))
            {
                v.Target.AlterStatus(BattleStatus.Haste, v.Target);
                SpecialSAEffect[v.Target.Data][14] += 128;
            }
            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Reflect_Boosted) && (SpecialSAEffect[v.Target.Data][14] & 256) == 0 && v.Target.CurrentHp <= (v.Target.MaximumHp / 2))
            {
                v.Target.AlterStatus(BattleStatus.Reflect, v.Target);
                SpecialSAEffect[v.Target.Data][14] += 256;
            }
            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Reflect) && (SpecialSAEffect[v.Target.Data][14] & 512) == 0 && v.Target.IsUnderAnyStatus(BattleStatus.LowHP) && !v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Reflect_Boosted))
            {
                v.Target.AlterStatus(BattleStatus.Reflect, v.Target);
                SpecialSAEffect[v.Target.Data][14] += 512;
            }
            if (v.Target.HasSupportAbilityByIndex(TranceSeekSupportAbility.SOS_Vanish_Boosted) && (SpecialSAEffect[v.Target.Data][14] & 1024) == 0 && v.Target.CurrentHp <= (v.Target.MaximumHp / 2))
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

                if (Eiko == null)
                    return;

                if (StateMoug[Eiko.Data] == 0 && !Eiko.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump | BattleStatus.Heat) && v.Caster.Data != Eiko.Data)
                {
                    float ChanceMoug = (float)(Eiko.HasSupportAbilityByIndex((SupportAbility)1224) ? 12 : (Eiko.HasSupportAbilityByIndex((SupportAbility)224) ? 10 : 8));

                    if (Comn.random16() % 100 > ChanceMoug || Eiko.HasSupportAbilityByIndex((SupportAbility)226)) // Synergy
                        return;

                    ushort TargetId = v.Caster.Id;
                    StateMoug[Eiko.Data] = 1;
                    List<BattleAbilityId> ClassicMougAAList = new List<BattleAbilityId>();
                    List<BattleAbilityId> SuperMougAAList = new List<BattleAbilityId>();
                    foreach (BattleAbilityId abilId in CharacterCommands.Commands[(BattleCommandId)1049].EnumerateAbilities()) // CMD Kupo (not used for Eiko)
                    {
                        Boolean AddAA = false;
                        switch (abilId)
                        {
                            case (BattleAbilityId)2000: // Mog Cure
                            {
                                if (Eiko.CurrentHp <= Eiko.MaximumHp / 2)
                                    AddAA = true;
                                break;
                            }
                            case (BattleAbilityId)2001: // Mog Hug
                            {
                                if (Eiko.CurrentMp <= Eiko.MaximumMp / 2)
                                    AddAA = true;
                                break;
                            }
                            case (BattleAbilityId)2002: // Mog Regen
                            {
                                if (!Eiko.IsUnderAnyStatus(BattleStatus.Regen))
                                    AddAA = true;
                                break;
                            }
                            case (BattleAbilityId)2004: // Mog Mirror
                            {
                                if (!Eiko.IsUnderAnyStatus(BattleStatus.Vanish))
                                    AddAA = true;
                                break;
                            }
                            case (BattleAbilityId)2005: // Mog AutoLife
                            {
                                if (Eiko.Level >= 35 && !Eiko.IsUnderAnyStatus(BattleStatus.AutoLife))
                                    AddAA = true;
                                break;
                            }
                            case (BattleAbilityId)2006: // Mog Esuna
                            {
                                if (Eiko.IsUnderAnyStatus(BattleStatus.Poison | BattleStatus.Venom | BattleStatus.Silence | BattleStatus.Blind | BattleStatus.Trouble | BattleStatus.Berserk | BattleStatus.Mini | TranceSeekStatus.Vieillissement))
                                    AddAA = true;
                                break;
                            }
                            case (BattleAbilityId)2007: // Mog Support
                            {
                                if (Eiko.Level >= 30 && (StackBreakOrUpStatus[Eiko.Data][1] < 50 || StackBreakOrUpStatus[Eiko.Data][3] < 50))
                                    AddAA = true;
                                break;
                            }
                            case (BattleAbilityId)2009: // Mog Flare
                            case (BattleAbilityId)2010: // Mog Holy
                            {
                                if (FF9StateSystem.EventState.ScenarioCounter >= 9990) // The party finds Hilda
                                {
                                    Boolean TargetAvailable = true;
                                    foreach (BattleUnit monster in BattleState.EnumerateUnits())
                                        if (!monster.IsPlayer && monster.IsTargetable && !monster.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump | BattleStatus.Reflect))
                                            TargetAvailable = true;

                                    if (TargetAvailable)
                                        AddAA = true;
                                }
                                break;
                            }
                            case (BattleAbilityId)2011: // Moga Cure
                            {
                                if (Eiko.Level >= 40)
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
                                    if (CurrentHPTeam <= (CurrentMaxHPTeam / 2) && !ZombiePresent)
                                        AddAA = true;
                                }
                                break;
                            }
                            case (BattleAbilityId)2012: // Moga Hug
                            {
                                if (Eiko.Level >= 50)
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

                                    if (CurrentMPTeam <= (CurrentMaxMPTeam / 4) && !ZombiePresent)
                                        AddAA = true;
                                }
                                break;
                            }
                            case (BattleAbilityId)2013: // Moga Regen
                            {
                                if (Eiko.Level >= 70)
                                {
                                    Boolean StatusToApply = false;
                                    foreach (BattleUnit unit in BattleState.EnumerateUnits())
                                        if (unit.IsPlayer && unit.IsTargetable && !unit.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump | BattleStatus.Regen))
                                            StatusToApply = true;

                                    if (StatusToApply)
                                        AddAA = true;
                                }
                                break;
                            }
                            case (BattleAbilityId)2014: // Moga Shield
                            {
                                if (Eiko.Level >= 60)
                                    AddAA = true;
                                break;
                            }
                            case (BattleAbilityId)2015: // Moga Mirror
                            {
                                if (Eiko.Level >= 80)
                                {
                                    Boolean StatusToApply = false;
                                    foreach (BattleUnit unit in BattleState.EnumerateUnits())
                                        if (unit.IsPlayer && unit.IsTargetable && !unit.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump | BattleStatus.Vanish))
                                            StatusToApply = true;

                                    if (StatusToApply)
                                        AddAA = true;
                                }
                                break;
                            }
                            case (BattleAbilityId)2017: // Moga Esuna
                            {
                                if (Eiko.Level >= 75)
                                {
                                    Boolean StatusToCure = false;
                                    foreach (BattleUnit unit in BattleState.EnumerateUnits())
                                        if (unit.IsPlayer && unit.IsTargetable && !unit.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump) && unit.IsUnderAnyStatus(BattleStatus.Poison | BattleStatus.Venom | BattleStatus.Silence | BattleStatus.Blind | BattleStatus.Trouble | BattleStatus.Mini | BattleStatus.Berserk | TranceSeekStatus.Vieillissement))
                                            StatusToCure = true;

                                    if (StatusToCure)
                                        AddAA = true;
                                }
                                break;
                            }
                            case (BattleAbilityId)2018: // Moga Support
                            {
                                if (Eiko.Level >= 85)
                                {
                                    Boolean StatusToApply = false;
                                    foreach (BattleUnit unit in BattleState.EnumerateUnits())
                                        if (unit.IsPlayer && unit.IsTargetable && !unit.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump) && (StackBreakOrUpStatus[unit.Data][1] < 50 || StackBreakOrUpStatus[unit.Data][3] < 50))
                                            StatusToApply = true;

                                    if (StatusToApply)
                                        AddAA = true;
                                }
                                break;
                            }
                        }
                        if (AddAA)
                        {
                            if ((int)abilId <= 2010)
                                ClassicMougAAList.Add(abilId);
                            else
                                SuperMougAAList.Add(abilId);
                        }

                    }

                    if (ClassicMougAAList.Count == 0)
                        return;

                    BattleAbilityId MougAAChoosen = ClassicMougAAList[GameRandom.Next16() % ClassicMougAAList.Count]; // Classic Mog spell
                    if (GameRandom.Next16() % 100 < 20 && SuperMougAAList.Count > 0)
                        MougAAChoosen = SuperMougAAList[GameRandom.Next16() % SuperMougAAList.Count];
                    if (GameRandom.Next16() % 100 < 5 && Eiko.Level >= 90) // Moga Autolife spell
                    {
                        Boolean StatusToApply = false;
                        foreach (BattleUnit unit in BattleState.EnumerateUnits())
                            if (unit.IsPlayer && unit.IsTargetable && !unit.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump | BattleStatus.AutoLife))
                                StatusToApply = true;

                        if (!StatusToApply)
                            MougAAChoosen = TranceSeekBattleAbility.MogAutoLife2;
                    }
                    if (GameRandom.Next16() % 100 == 0 && Eiko.Level >= 99) // MougaHoming
                        MougAAChoosen = TranceSeekBattleAbility.MougaHoming;

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
    }
}
