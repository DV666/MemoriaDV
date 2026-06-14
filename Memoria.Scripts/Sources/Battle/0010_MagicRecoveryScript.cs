using Memoria.Data;
using System;
using System.Collections.Generic;

namespace Memoria.Scripts.TranceSeek
{
    [BattleScript(Id)]
    public sealed class MagicRecoveryScript : IBattleScript
    {
        public const Int32 Id = 0010;

        private readonly BattleCalculator _v;

        public MagicRecoveryScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Command.Power == 1)
            {
                _v.Target.Flags |= CalcFlag.HpAlteration;
                if (!_v.Target.IsZombie)
                {
                    _v.Target.Flags |= CalcFlag.HpRecovery;
                }
                _v.Target.HpDamage = 1;
            }
            else
            {
                var Caster_TSVar = _v.CasterState();

                _v.NormalMagicParams();     
                TranceSeekAPI.CasterPenaltyMini(_v);
                TranceSeekAPI.EnemyTranceBonusAttack(_v);
                TranceSeekAPI.PenaltyCommandDividedAttack(_v);
                if (_v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Archmage))
                    TranceSeekAPI.TryCriticalHit(_v);
                _v.CalcHpMagicRecovery();

                if (Caster_TSVar.StackStatus.Magic != 0)
                    _v.Context.Attack += ((Caster_TSVar.StackStatus.Magic * _v.Context.Attack) / 100);

                if (_v.Command.HitRate == 255)
                {
                    _v.Target.TryRemoveStatuses(_v.Command.AbilityStatus);
                }
                else if (_v.Command.HitRate == 222)
                {
                    _v.Target.TryRemoveStatuses(BattleStatusConst.AnyNegative);
                }
                else if (_v.Command.HitRate == 111) // Still used ? (old Reconstruction Black Waltz 1)
                {
                    _v.Command.AbilityStatus |= BattleStatus.Regen;
                    TranceSeekAPI.TryAlterMagicStatuses(_v);
                }
                else if (_v.Command.HitRate == 99)
                {
                    _v.Command.AbilityStatus |= (TranceSeekStatus.ArmorUp | TranceSeekStatus.MentalUp);
                    TranceSeekAPI.TryAlterMagicStatuses(_v);
                }
                else
                {
                    TranceSeekAPI.TryAlterMagicStatuses(_v);
                }
                if (_v.Target.Data.dms_geo_id == 416) // Meltigemini
                {
                    int PreviousHP = (int)_v.Target.CurrentHp;
                    Int32 wait = 20;
                    _v.Caster.AddDelayedModifier(
                    caster => (wait -= BattleState.ATBTickCount) > 0,
                    caster =>
                        {
                            _v.TargetState().Monster.Special1 = Math.Min((int)(PreviousHP - _v.Target.CurrentHp), 9999);
                        }
                    );
                    _v.Target.TryAlterSingleStatus(TranceSeekStatusId.ZombieArmor, true, _v.Caster, _v.Target.HpDamage);
                }
            }
        }
    }
}

