using System;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Panacea, Stona, Esuna, Dispel
    /// </summary>
    [BattleScript(Id)]
    public sealed class MagicCureStatusScript : IBattleScript, IEstimateBattleScript
    {
        public const Int32 Id = 0012;

        private readonly BattleCalculator _v;

        public MagicCureStatusScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            _v.TryRemoveAbilityStatuses();     
            if (_v.Caster.Data.dms_geo_id == 410) // Lamie
            {
                if (_v.Command.Power == 99)
                {
                    switch (_v.Command.AbilityStatus)
                    {
                        case BattleStatus.Blind:
                            {
                                TranceSeekCustomAPI.MonsterMechanic[_v.Caster.Data][2] = 16;
                                break;
                            }
                        case BattleStatus.Silence:
                            {
                                TranceSeekCustomAPI.MonsterMechanic[_v.Caster.Data][2] = 8;
                                break;
                            }
                        case BattleStatus.Poison:
                            {
                                TranceSeekCustomAPI.MonsterMechanic[_v.Caster.Data][2] = 65536;
                                break;
                            }
                        case BattleStatus.Venom:
                            {
                                TranceSeekCustomAPI.MonsterMechanic[_v.Caster.Data][2] = 2;
                                break;
                            }
                        case BattleStatus.Petrify:
                            {
                                TranceSeekCustomAPI.MonsterMechanic[_v.Caster.Data][2] = 1;
                                break;
                            }
                        case BattleStatus.Slow:
                            {
                                TranceSeekCustomAPI.MonsterMechanic[_v.Caster.Data][2] = 1048576;
                                break;
                            }
                    }
                    _v.Context.Flags = 0;
                }
                TranceSeekCustomAPI.SpecialSA(_v);
                return;
            }
            if (_v.Command.AbilityId == BattleAbilityId.Esuna)
            {
                if (_v.Target.Data.special_status_old)
                {
                    _v.Target.Data.special_status_old = false;
                    _v.Context.Flags = 0;
                }
            }
            if (_v.Command.AbilityId == BattleAbilityId.Dispel)
            {
                for (Int32 i = 0; i < 31; i++)
                {
                    if (TranceSeekCustomAPI.SPSSpecialStatus[_v.Caster.Data][i] >= 0)
                    {
                        TranceSeekCustomAPI.RemoveSpecialSPS(_v.Target.Data, (uint)i);
                        _v.Context.Flags = 0;
                    }
                }
            }
            if (_v.Command.Power == 111)
            {
                for (Int32 i = 0; i < (TranceSeekCustomAPI.SPSSpecialStatus[_v.Caster.Data].Length - 1); i++)
                {
                    if (TranceSeekCustomAPI.SPSSpecialStatus[_v.Caster.Data][i] >= 0)
                    {
                        TranceSeekCustomAPI.RemoveSpecialSPS(_v.Target.Data, (uint)i);
                        _v.Context.Flags = 0;
                    }
                }
            }
            if (_v.Command.HitRate == 222)
            {
                _v.Context.Flags = 0;
            }
            TranceSeekCustomAPI.SpecialSA(_v);
        }

        public Single RateTarget()
        {
            BattleStatus playerStatus = _v.Target.CurrentStatus;
            BattleStatus removeStatus = _v.Command.AbilityStatus;
            BattleStatus removedStatus = playerStatus & removeStatus;
            Int32 rating = BattleScriptStatusEstimate.RateStatuses(removedStatus);

            if (_v.Target.IsPlayer)
                return -1 * rating;

            return rating;
        }
    }
}