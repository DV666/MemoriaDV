using FF9;
using Memoria.Data;
using System;
using System.Runtime.Remoting.Contexts;
using UnityEngine;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Osmose, Absorb MP
    /// </summary>
    [BattleScript(Id)]
    public sealed class DrainMpScript : IBattleScript
    {
        public const Int32 Id = 0015;

        private readonly BattleCalculator _v;

        public DrainMpScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            if (_v.Command.Id == BattleCommandId.Attack && _v.Caster.PlayerIndex == CharacterId.Quina)
            {
                _v.PhysicalAccuracy();
                if (!TranceSeekCustomAPI.TryPhysicalHit(_v))
                    return;

                Int32 baseDamage = Comn.random16() % (1 + (_v.Caster.Level + _v.Caster.Magic >> 3));
                _v.Context.AttackPower = _v.Caster.GetWeaponPower(_v.Command);
                _v.Target.SetMagicDefense();
                _v.Context.Attack = Comn.random16() % _v.Caster.Magic + baseDamage;
                _v.Caster.PhysicalPenaltyAndBonusAttack();
                TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                _v.BonusBackstabAndPenaltyLongDistance();
                _v.Caster.BonusWeaponElement();
                if (_v.CanAttackWeaponElementalCommand())
                {
                    if (_v.Context.IsAbsorb)
                    {
                        _v.Context.Flags |= BattleCalcFlags.Guard;
                    }
                    else
                    {
                        _v.TryCriticalHit();
                        _v.PenaltyReverseAttack();
                        _v.CalcMpDamage();
                        TranceSeekCustomAPI.RaiseTrouble(_v);
                    }
                }
                return;
            }
            if (!_v.IsCasterNotTarget() || !_v.Target.CanBeAttacked())
                return;

            _v.NormalMagicParams();
            TranceSeekCustomAPI.CharacterBonusPassive(_v, "MagicAttack");
            _v.Caster.PenaltyMini();
            _v.Caster.EnemyTranceBonusAttack();
            TranceSeekCustomAPI.PenaltyShellAttack(_v);
            _v.Target.Flags |= CalcFlag.MpAlteration;
            _v.Caster.Flags |= CalcFlag.MpAlteration;
            _v.Context.IsDrain = true;

            _v.CalcMpDamage();
            Int32 damage = _v.Target.MpDamage;

            if (_v.Target.IsZombie)
            {
                damage = 0;
            }
            else
            {
                _v.Caster.Flags |= CalcFlag.MpRecovery;
                if (damage > _v.Target.CurrentMp)
                    damage = (Int32)_v.Target.CurrentMp;
            }

            _v.Target.MpDamage = damage;
            _v.Caster.MpDamage = damage;
            TranceSeekCustomAPI.SpecialSA(_v);
        }
    }
}