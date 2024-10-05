using System;
using System.Collections.Generic;
using FF9;
using Memoria.Data;
using UnityEngine;
using UnityEngine.Networking.Types;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Mental Break
    /// </summary>
    [BattleScript(Id)]
    public sealed class SummonMonsterScript : IBattleScript
    {
        public const Int32 Id = 0140;

        private readonly BattleCalculator _v;

        public static Dictionary<BTL_DATA, Int32> SummonStep = new Dictionary<BTL_DATA, Int32>();

        public SummonMonsterScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (!SummonStep.TryGetValue(_v.Caster.Data, out Int32 IDStep)) // Init
                SummonStep[_v.Caster.Data] = 0;

            if (SummonStep[_v.Caster.Data] == 0)
            {
                _v.Caster.Data.gameObject.SetActive(false);
                _v.Caster.Data.weapon_geo.SetActive(false);
                if (_v.Caster.IsUnderAnyStatus(BattleStatus.Trance))
                {
                    _v.Caster.Data.gameObject = ModelFactory.CreateModel("GEO_MON_B3_070", true);
                    _v.Caster.Data.originalGo = ModelFactory.CreateModel("GEO_MON_B3_070", true);
                }
                else
                {
                    _v.Caster.Data.gameObject = ModelFactory.CreateModel("GEO_MON_B3_070", true);
                }
                for (Int32 i = 0; i < 34; i++)
                    _v.Caster.Data.mot[i] = "GEO_MON_B3_070_000";

                _v.Caster.Data.gameObject.SetActive(true);
                SummonStep[_v.Caster.Data]++;
            }
            else if (SummonStep[_v.Caster.Data] == 1)
            {
                _v.Command.Power = 72;
                _v.Command.Element = EffectElement.Thunder;
                TranceSeekCustomAPI.CharacterBonusPassive(_v, "MagicAttack");
                TranceSeekCustomAPI.CasterPenaltyMini(_v);
                TranceSeekCustomAPI.EnemyTranceBonusAttack(_v);
                TranceSeekCustomAPI.PenaltyShellAttack(_v);
                TranceSeekCustomAPI.PenaltyCommandDividedAttack(_v);
                TranceSeekCustomAPI.BonusElement(_v);
                if (TranceSeekCustomAPI.CanAttackMagic(_v))
                {
                    if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)102))
                        TranceSeekCustomAPI.TryCriticalHit(_v);
                    _v.CalcHpDamage();
                    TranceSeekCustomAPI.RaiseTrouble(_v);
                }
                _v.TryAlterMagicStatuses();
                SummonStep[_v.Caster.Data]++;
            }
            else if (SummonStep[_v.Caster.Data] == 2)
            {
                _v.Caster.Data.gameObject.SetActive(false);
                CharacterBattleParameter btlParam = btl_mot.BattleParameterList[btl_util.getSerialNumber(_v.Caster.Data)];
                if (_v.Caster.IsUnderAnyStatus(BattleStatus.Trance))
                {
                    _v.Caster.Data.gameObject = ModelFactory.CreateModel(btlParam.TranceModelId, true);
                    _v.Caster.Data.originalGo = ModelFactory.CreateModel(btlParam.ModelId, true);
                }
                else
                {
                    _v.Caster.Data.gameObject = ModelFactory.CreateModel(btlParam.ModelId, true);
                }

                for (Int32 i = 0; i < 34; i++)
                    _v.Caster.Data.mot[i] = btlParam.AnimationId[i];

                _v.Caster.Data.gameObject.SetActive(true);
                _v.Caster.Data.weapon_geo.SetActive(true);

                SummonStep[_v.Caster.Data] = 0;
            }
        }
    }
}

