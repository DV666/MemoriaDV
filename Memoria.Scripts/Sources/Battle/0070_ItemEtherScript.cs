using Memoria.Data;
using System;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Ether
    /// </summary>
    [BattleScript(Id)]
    public sealed class ItemEtherScript : IBattleScript, IEstimateBattleScript
    {
        public const Int32 Id = 0070;

        private readonly BattleCalculator _v;

        public ItemEtherScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            _v.Context.Attack = 15;
            _v.Context.AttackPower = _v.Command.Item.Power;
            _v.Context.DefensePower = 0;
            if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1027))
            {
                foreach (BattleUnit unit in BattleState.EnumerateUnits())
                {
                    _v.Caster.Flags = CalcFlag.MpAlteration;
                    if (_v.Target.IsPlayer)
                    {
                        if (!unit.IsPlayer || !unit.IsTargetable || unit.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump))
                            continue;

                        if (!unit.IsZombie)
                            _v.Caster.Flags |= CalcFlag.MpRecovery;

                        if (unit.Data == _v.Target.Data)
                        {
                            _v.Caster.MpDamage = _v.Context.AttackPower * _v.Context.Attack * 2;
                        }
                        else
                        {
                            _v.Caster.MpDamage = _v.Context.AttackPower * _v.Context.Attack;
                        }
                    }
                    else
                    {
                        if (unit.IsPlayer || !unit.IsTargetable || unit.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump))
                            continue;

                        if (!unit.IsZombie)
                            _v.Caster.Flags |= CalcFlag.MpRecovery;

                        if (unit.Data == _v.Target.Data)
                        {
                            _v.Caster.MpDamage = _v.Context.AttackPower * _v.Context.Attack * 2;
                        }
                        else
                        {
                            _v.Caster.MpDamage = _v.Context.AttackPower * _v.Context.Attack;
                        }
                    }
                    _v.Caster.Change(unit);
                    SBattleCalculator.CalcResult(_v);
                    BattleState.Unit2DReq(unit);
                }
                _v.Caster.Flags = 0;
                _v.Caster.MpDamage = 0;
                _v.PerformCalcResult = false;
                return;
            }
            _v.CalcMpMagicRecovery();
            TranceSeekCustomAPI.SpecialSA(_v);
        }

        public Single RateTarget()
        {
            _v.Context.Attack = 15;
            _v.Context.AttackPower = _v.Command.Item.Power;
            _v.Context.DefensePower = 0;

            _v.CalcMpMagicRecovery();

            Single rate = _v.Target.MpDamage * BattleScriptDamageEstimate.RateHpMp((Int32)_v.Target.CurrentMp, (Int32)_v.Target.MaximumMp);

            if ((_v.Target.Flags & CalcFlag.MpRecovery) != CalcFlag.MpRecovery)
                rate *= -1;
            if (!_v.Target.IsPlayer)
                rate *= -1;

            return rate;
        }
    }
}
