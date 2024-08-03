using System;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Dark Matter
    /// </summary>
    [BattleScript(Id)]
    public sealed class BanditScript : IBattleScript
    {
        public const Int32 Id = 0081;

        private readonly BattleCalculator _v;

        public BanditScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            SB2_PATTERN sb2Pattern = FF9StateSystem.Battle.FF9Battle.btl_scene.PatAddr[FF9StateSystem.Battle.FF9Battle.btl_scene.PatNum];
            if (_v.Caster.PlayerIndex == CharacterId.Amarant && _v.Command.AbilityId == BattleAbilityId.NoMercy1) // Intimidation
            {
                uint RatioPVMonsterInti = (_v.Target.CurrentHp * 100U / _v.Target.MaximumHp); // PV Monster
                for (Int32 i = 0; i < TranceSeekCustomAPI.BossBattleBonusHP.GetLength(0); i++)
                {
                    if (TranceSeekCustomAPI.BossBattleBonusHP[i, 0] == FF9StateSystem.Battle.battleMapIndex && TranceSeekCustomAPI.BossBattleBonusHP[i, 1] == sb2Pattern.Monster[_v.Target.Data.bi.slot_no].TypeNo)
                    {
                        RatioPVMonsterInti = ((_v.Target.CurrentHp - 10000U) * 100U / (_v.Target.MaximumHp - 10000U));
                    }
                }
                if ((GameRandom.Next8() % 100) < RatioPVMonsterInti)
                {
                    _v.Target.AlterStatus(BattleStatus.Stop | BattleStatus.Trouble, _v.Caster);
                }
                return;
            }
            if (_v.Target.IsUnderAnyStatus(BattleStatus.Vanish) | _v.Target.PhysicalEvade == 255)
            {
                _v.Context.Flags |= BattleCalcFlags.Miss;
                return;
            }
            if (_v.Caster.IsUnderAnyStatus(BattleStatus.Blind))
            {
                _v.Context.HitRate = (short)((_v.Caster.Level + _v.Caster.Will) / 2);
                _v.Context.Evade = (short)_v.Target.Level;
                if (GameRandom.Next16() % (int)_v.Context.HitRate < GameRandom.Next16() % (int)_v.Context.Evade)
                {
                    _v.Context.Flags |= BattleCalcFlags.Miss;
                    return;
                }
            }
            if (!_v.Target.TryKillFrozen())
            {
                TranceSeekCustomAPI.WeaponPhysicalParams(CalcAttackBonus.Simple, _v);
                _v.Caster.PhysicalPenaltyAndBonusAttack();
                TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                TranceSeekCustomAPI.BonusBackstabAndPenaltyLongDistanceTranceSeek(_v);
                TranceSeekCustomAPI.IpsenCastleMalus(_v);
                _v.CalcPhysicalHpDamage();
                TranceSeekCustomAPI.RaiseTrouble(_v);
            }
            BattleEnemy battleEnemy = BattleEnemy.Find(_v.Target);
            if (!HasStealableItems(battleEnemy))
            {
                UiState.SetBattleFollowFormatMessage(BattleMesages.DoesNotHaveAnything, new object[0]);
                return;
            }
            uint RatioPVMonster = (_v.Target.CurrentHp * 100U / _v.Target.MaximumHp); // PV Monster
            for (Int32 i = 0; i < TranceSeekCustomAPI.BossBattleBonusHP.GetLength(0); i++)
            {
                if (TranceSeekCustomAPI.BossBattleBonusHP[i, 0] == FF9StateSystem.Battle.battleMapIndex && TranceSeekCustomAPI.BossBattleBonusHP[i, 1] == sb2Pattern.Monster[_v.Target.Data.bi.slot_no].TypeNo)
                {
                    RatioPVMonster = ((_v.Target.CurrentHp - 10000U) * 100U / (_v.Target.MaximumHp - 10000U));
                }
            } 
            uint PVCharacter = (_v.Caster.CurrentHp * 100U / _v.Caster.MaximumHp); // Zidane or Markus
            uint PVMonster = (100 - RatioPVMonster);
            if (battleEnemy.StealableItems[3] != RegularItem.NoItem && (GameRandom.Next8() % 100) < (PVCharacter / 2) && (GameRandom.Next8() % 100) < (PVMonster / 2))
            {
                _v.StealItem(battleEnemy, 3);
            }
            else if (battleEnemy.StealableItems[2] != RegularItem.NoItem && (GameRandom.Next8() % 100) < (3 * PVCharacter) / 4 && (GameRandom.Next8() % 100) < (25 / 2) + (PVMonster / 2))
            {
                _v.StealItem(battleEnemy, 2);
            }
            else if (battleEnemy.StealableItems[1] != RegularItem.NoItem && (GameRandom.Next8() % 100) < PVCharacter && (GameRandom.Next8() % 100) < PVMonster)
            {
                _v.StealItem(battleEnemy, 1);
            }
            else if (battleEnemy.StealableItems[0] != RegularItem.NoItem)
            {
                _v.StealItem(battleEnemy, 0);
            }
            else
            {
                UiState.SetBattleFollowFormatMessage(BattleMesages.CouldNotStealAnything);
            }
        }

        private static bool HasStealableItems(BattleEnemy enemy)
        {
            bool result = false;
            for (short num = 0; num < 4; num += 1)
            {
                if (enemy.StealableItems[(int)num] != RegularItem.NoItem)
                {
                    result = true;
                }
            }
            return result;
        }
    }
}
