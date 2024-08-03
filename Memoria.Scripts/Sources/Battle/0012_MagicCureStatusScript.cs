using FF9;
using Memoria.Data;
using System;

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
                return;
            }
            if (_v.Command.AbilityId == BattleAbilityId.Esuna)
            {
                _v.Command.AbilityStatus |= TranceSeekCustomAPI.CustomStatus.Vieillissement;             
            }
            if (_v.Command.Power == 111)
            {
                for (Int32 i = 33; i < 63; i++)
                {
                    BattleStatusId statusId = (BattleStatusId)i;
                    _v.Target.RemoveStatus(statusId.ToBattleStatus());
                }
            }

            _v.TryRemoveAbilityStatuses();

            if (_v.Command.HitRate == 222)
            {
                _v.Context.Flags = 0;
            }
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
