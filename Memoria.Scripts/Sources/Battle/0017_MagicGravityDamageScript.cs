using Memoria.Data;
using System;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Demi, Aqua Breath, Demi Shock, Worm Hole
    /// </summary>
    [BattleScript(Id)]
    public sealed class MagicGravityDamageScript : IBattleScript
    {
        public const Int32 Id = 0017;

        private readonly BattleCalculator _v;

    public MagicGravityDamageScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            _v.SetCommandAttack();
            TranceSeekCustomAPI.PenaltyCommandDividedAttack(_v);
            if (_v.Target.IsUnderStatus(BattleStatus.Shell))
            {
                _v.Context.Attack = _v.Context.Attack / 2;
            }
            if (_v.Target.HasCategory(EnemyCategory.Stone) && !_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill))
            {
                _v.Context.Attack = _v.Context.Attack * 2;
            }
            TranceSeekCustomAPI.BonusElement(_v);
            if (TranceSeekCustomAPI.CanAttackMagic(_v))
            {
                if (_v.Command.HitRate == 255)
                {
                    _v.CalcDamageCommon();
                    if (_v.Context.Attack > 100)
                    {
                        _v.Context.Attack = 100;
                    }
                    int num = (int)(_v.Target.MaximumHp * _v.Context.Attack / 100U);
                    if (_v.Command.IsShortSummon)
                    {
                        num = num * 2 / 3;
                    }
                    _v.Target.HpDamage = num;             
                }
                else
                {
                    _v.CalcCannonProportionDamage();
                }
            }
            if (_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill))
            {
                _v.Target.HpDamage = Math.Max(1, (_v.Target.HpDamage / TranceSeekCustomAPI.MonsterMechanic[_v.Target.Data][5]));
                TranceSeekCustomAPI.MonsterMechanic[_v.Target.Data][5] = TranceSeekCustomAPI.MonsterMechanic[_v.Target.Data][5] * 2;
            }
            _v.TryAlterMagicStatuses();
        }
    }
}
