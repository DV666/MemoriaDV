using FF9;
using Memoria.Data;
using System;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Weapon: Blood Sword
    /// </summary>
    [BattleScript(Id)]
    public sealed class EnemyPhysicalBuffedAttackScript : IBattleScript
    {
        public const Int32 Id = 0127;

        private readonly BattleCalculator _v;

    public EnemyPhysicalBuffedAttackScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (!_v.Target.TryKillFrozen())
            {
                _v.NormalPhysicalParams();
                TranceSeekAPI.CharacterBonusPassive(_v, "PhysicalAttack");
                TranceSeekAPI.EnemyTranceBonusAttack(_v);
                TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                if (_v.Command.HitRate != 101)
                {
                    TranceSeekAPI.BonusBackstabAndPenaltyLongDistance(_v);
                }
                TranceSeekAPI.BonusElement(_v);
                if (_v.CanAttackElementalCommand())
                {
                    TranceSeekAPI.TryCriticalHit(_v);
                    _v.CalcPhysicalHpDamage();
                    TranceSeekAPI.RaiseTrouble(_v);
                    if (_v.Command.HitRate == 222) // Motivation Gauche
                    {
                        _v.Command.AbilityStatus |= TranceSeekStatus.PowerUp;
                        TranceSeekAPI.TryAlterCommandStatuses(_v);
                    }
                    else if (_v.Command.HitRate == 223) // Motivation droite
                    {
                        _v.Command.AbilityStatus |= TranceSeekStatus.ArmorUp;
                        TranceSeekAPI.TryAlterCommandStatuses(_v);
                    }
                    else
                    {
                        TranceSeekAPI.InfusedWeaponStatus(_v);
                        TranceSeekAPI.TryAlterMagicStatuses(_v);

                    }
                }                    
            }
        }
    }
}
