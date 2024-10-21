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
                TranceSeekCustomAPI.CharacterBonusPassive(_v, "PhysicalAttack");
                TranceSeekCustomAPI.EnemyTranceBonusAttack(_v);
                TranceSeekCustomAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                if (_v.Command.HitRate != 101)
                {
                    TranceSeekCustomAPI.BonusBackstabAndPenaltyLongDistance(_v);
                }
                TranceSeekCustomAPI.BonusElement(_v);
                if (_v.CanAttackElementalCommand())
                {
                    TranceSeekCustomAPI.TryCriticalHit(_v);
                    _v.CalcPhysicalHpDamage();
                    TranceSeekCustomAPI.InfusedWeaponStatus(_v);
                    TranceSeekCustomAPI.RaiseTrouble(_v);
                    if (_v.Command.HitRate == 222) // Motivation Gauche
                    {
                        _v.Command.AbilityStatus |= TranceSeekCustomAPI.CustomStatus.PowerUp;
                        TranceSeekCustomAPI.TryAlterCommandStatuses(_v);
                    }
                    else if (_v.Command.HitRate == 223) // Motivation droite
                    {
                        _v.Command.AbilityStatus |= TranceSeekCustomAPI.CustomStatus.ArmorUp;
                        TranceSeekCustomAPI.TryAlterCommandStatuses(_v);
                    }
                    else
                    {
                        _v.TryAlterMagicStatuses();

                    }
                }                    
            }
        }
    }
}
