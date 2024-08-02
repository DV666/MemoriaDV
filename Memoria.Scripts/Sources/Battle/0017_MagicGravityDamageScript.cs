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
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            if (_v.Target.CheckUnsafetyOrMiss())
            {
                _v.SetCommandAttack();
                _v.PenaltyCommandDividedAttack();
                if (_v.Target.IsUnderStatus(BattleStatus.Shell))
                {
                    _v.Context.Attack = _v.Context.Attack / 2;
                }
                if (_v.Target.HasCategory(EnemyCategory.Stone) && _v.Command.AbilityId == BattleAbilityId.Demi)
                {
                    _v.Context.Attack = _v.Context.Attack * 2;
                }
                _v.BonusElement();
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
                _v.TryAlterMagicStatuses();
                TranceSeekCustomAPI.SpecialSA(_v);
            }
        }
    }
}
