using System;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Aerial Slash, Whirlwind, Flame Slash, Fire Blades, Jet Fire, Virus Crunch, Psychokinesis, Curse, Sandstorm, High Wind, Virus Fly, !!!, Leaf Swirl, Sweep, Fin, Boomerang, Paper Storm, Spin, Shockwave, Cleave, Raining Swords, Neutron Ring
    /// </summary>
    [BattleScript(Id)]
    public sealed class PreciseEnemyPhysicalAttackScript : IBattleScript
    {
        public const Int32 Id = 0100;

        private readonly BattleCalculator _v;

        public PreciseEnemyPhysicalAttackScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Target.IsUnderStatus(Data.BattleStatus.Mini) && _v.Command.HitRate == 1) // Yeti Friendly - Nom nom nom
            {
                _v.Target.Remove();
            }
            else
            {
                if (!_v.Target.TryKillFrozen())
                {
                    if (_v.Command.HitRate == 111)
                    {
                        _v.SetCommandPower();
                        _v.Caster.SetPhysicalAttack();
                    }
                    else
                    {
                        _v.NormalPhysicalParams();
                        TranceSeekCustomAPI.CharacterBonusPassive(_v, "PhysicalAttack");
                    }
                    TranceSeekCustomAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                    TranceSeekCustomAPI.EnemyTranceBonusAttack(_v);
                    TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                    if (_v.Command.HitRate != 255)
                    {
                        TranceSeekCustomAPI.BonusBackstabAndPenaltyLongDistanceTranceSeek(_v);
                    }
                    TranceSeekCustomAPI.BonusElement(_v);
                    if (_v.CanAttackElementalCommand())
                    {
                        TranceSeekCustomAPI.RaiseTrouble(_v);
                        _v.CalcHpDamage();
                        TranceSeekCustomAPI.InfusedWeaponStatus(_v);
                        _v.TryAlterMagicStatuses();
                    }
                }
            }
        }
    }
}
