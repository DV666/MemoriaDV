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
            _v.Context.Attack = 15;
            _v.Context.AttackPower = _v.Command.Item.Power;
            _v.Context.DefensePower = 0;
            if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1027) && (_v.Target.IsPlayer && BattleState.BattleUnitCount(true) > 1 || !_v.Target.IsPlayer && BattleState.BattleUnitCount(false) > 1))
            { // Herboriste +                    
                foreach (BattleUnit unit in BattleState.EnumerateUnits())
                {
                    int healing = 0;
                    if (_v.Target.IsPlayer)
                    {
                        if (!unit.IsPlayer || !unit.IsTargetable || unit.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump))
                            continue;

                        if (unit.Data == _v.Target.Data)
                        {
                            healing = _v.Context.AttackPower * _v.Context.Attack * 2;
                        }
                        else
                        {
                            healing = _v.Context.AttackPower * _v.Context.Attack;
                        }
                    }
                    else
                    {
                        if (unit.IsPlayer || !unit.IsTargetable || unit.IsUnderAnyStatus(BattleStatus.Death | BattleStatus.Petrify | BattleStatus.Jump))
                            continue;

                        if (unit.Data == _v.Target.Data)
                        {
                            healing = _v.Context.AttackPower * _v.Context.Attack * 2;
                        }
                        else
                        {
                            healing = _v.Context.AttackPower * _v.Context.Attack;
                        }
                    }
                    if (unit.IsZombie)
                    {
                        btl2d.Btl2dStatReq(unit, 0, healing);
                        btl_para.SetMpDamage(unit, (uint)healing);
                    }
                    else
                    {
                        btl2d.Btl2dStatReq(unit, 0, -healing);
                        btl_para.SetMpRecover(unit, (uint)healing);
                    }
                }
            }
            else
                _v.CalcMpMagicRecovery();

            if (_v.Caster.PlayerIndex == CharacterId.Blank && _v.Command.Id == BattleCommandId.Item)
                btl_stat.AlterStatus(_v.Caster, TranceSeekStatusId.Special, _v.Caster, true, "SoakedBlade", _v.Command.ItemId);
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
