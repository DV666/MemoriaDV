using Memoria.Data;
using System;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Elexir
    /// </summary>
    [BattleScript(Id)]
    public sealed class ItemElixirScript : IBattleScript, IEstimateBattleScript
    {
        public const Int32 Id = 0071;

        private readonly BattleCalculator _v;

        public ItemElixirScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            if (!_v.Target.CanBeAttacked())
                return;

            if (_v.Target.IsZombie)
            {
                if (_v.Target.Data.dms_geo_id == 416)
                {
                    TranceSeekCustomAPI.MonsterMechanic[_v.Target.Data][1] = 9999;
                    _v.Target.CurrentHp = 1;
                    return;
                }
                if (_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill))
                {
                    _v.Target.Flags |= (CalcFlag.HpAlteration | CalcFlag.MpAlteration);
                    _v.Target.HpDamage = 9999;
                    _v.Target.MpDamage = 999;
                    return;
                }
                _v.Target.CurrentMp = 0;
                _v.Target.Kill();
            }
            else
            {
                if (_v.Target.IsPlayer || _v.Command.Power == 250)
                {
                    _v.Target.Flags |= (CalcFlag.HpAlteration | CalcFlag.HpRecovery | CalcFlag.MpAlteration | CalcFlag.MpRecovery);
                    _v.Target.HpDamage = (int)_v.Target.MaximumHp;
                    _v.Target.MpDamage = (int)_v.Target.MaximumMp;
                }
                else
                {
                    _v.Target.CurrentHp = _v.Target.MaximumHp;
                    _v.Target.CurrentMp = _v.Target.MaximumMp;
                }
                if (!_v.Caster.IsPlayer)
                {
                    _v.TryAlterMagicStatuses();
                }
            }
            TranceSeekCustomAPI.SpecialSA(_v);
        }

        public Single RateTarget()
        {
            if (!_v.Target.CanBeAttacked())
                return 0;

            if (_v.Target.IsZombie)
            {
                Int32 rate = BattleScriptStatusEstimate.RateStatus(BattleStatus.Death);
                if (!_v.Target.IsPlayer)
                    rate *= -1;

                return rate;
            }
            else
            {
                Single rate = 0;

                rate += _v.Target.MaximumHp * BattleScriptDamageEstimate.RateHpMp((Int32)_v.Target.CurrentHp, (Int32)_v.Target.MaximumHp);
                rate += _v.Target.MaximumMp * BattleScriptDamageEstimate.RateHpMp((Int32)_v.Target.CurrentMp, (Int32)_v.Target.MaximumMp);

                if (!_v.Target.IsPlayer)
                    rate *= -1;

                return rate;
            }
        }
    }
}
