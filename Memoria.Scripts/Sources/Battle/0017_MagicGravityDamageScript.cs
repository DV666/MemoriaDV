using Memoria.Data;
using Memoria.Prime;
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
            SB2_PATTERN sb2Pattern = FF9StateSystem.Battle.FF9Battle.btl_scene.PatAddr[FF9StateSystem.Battle.FF9Battle.btl_scene.PatNum];
            for (Int32 i = 0; i < ImmuneGravity.GetLength(0); i++)
            {
                if (FF9StateSystem.Battle.battleMapIndex == ImmuneGravity[i, 0] && sb2Pattern.Monster[_v.Target.Data.bi.slot_no].TypeNo == ImmuneGravity[i, 1])
                {
                    _v.Context.Flags = BattleCalcFlags.Guard;
                    return;
                }
            }

            _v.SetCommandAttack();
            TranceSeekAPI.PenaltyCommandDividedAttack(_v);
            if (_v.Target.IsUnderStatus(BattleStatus.Shell))
                _v.Context.DamageModifierCount -= 2;
            if (_v.Target.HasCategory(EnemyCategory.Stone) && !_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill))
                _v.Context.DamageModifierCount += 4;
            TranceSeekAPI.ViviFocus(_v);
            TranceSeekAPI.BonusElement(_v);
            if (TranceSeekAPI.CanAttackMagic(_v))
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
            if (_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill) || TranceSeekAPI.EliteMonster(_v.Target.Data))
            {
                _v.Target.HpDamage = Math.Max(1, (_v.Target.HpDamage / TranceSeekAPI.MonsterMechanic[_v.Target.Data][5]));
                TranceSeekAPI.MonsterMechanic[_v.Target.Data][5] = TranceSeekAPI.MonsterMechanic[_v.Target.Data][5] * 2;
            }
            TranceSeekAPI.TryAlterMagicStatuses(_v);

            if (TranceSeekAPI.AbsorbElement.TryGetValue(_v.Target.Data, out Int32 elementprotect))
                if (elementprotect == 256)
                    _v.Target.Flags |= CalcFlag.HpRecovery;
        }

        public static Int32[,] ImmuneGravity = new Int32[,]
        {
            { 930, 0 } // Lovecraft
        };
    }
}
