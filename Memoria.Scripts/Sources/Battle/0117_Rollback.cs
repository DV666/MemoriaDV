using System;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Lancer (Rollback)
    /// </summary>
    [BattleScript(Id)]
    public sealed class Rollback : IBattleScript
    {
        public const Int32 Id = 0117;

        private readonly BattleCalculator _v;

        public Rollback(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            var targetState = _v.TargetState();

            if (_v.Command.Power == 0 && !targetState.Rollback.IsSaved)
            {
                targetState.Rollback.IsSaved = true;

                targetState.Rollback.CurrentHp = _v.Target.CurrentHp;
                targetState.Rollback.CurrentMp = _v.Target.CurrentMp;
                targetState.Rollback.Strength = _v.Target.Strength;
                targetState.Rollback.Magic = _v.Target.Magic;
                targetState.Rollback.Will = _v.Target.Will;
                targetState.Rollback.PhysicalDefence = _v.Target.PhysicalDefence;
                targetState.Rollback.PhysicalEvade = _v.Target.PhysicalEvade;
                targetState.Rollback.MagicDefence = _v.Target.MagicDefence;
                targetState.Rollback.MagicEvade = _v.Target.MagicEvade;
                targetState.Rollback.Trance = _v.Target.Trance;

                targetState.Rollback.SavedStatus = _v.Target.CurrentStatus;
            }
            else if (_v.Command.Power == 1 && targetState.Rollback.IsSaved)
            {
                targetState.Rollback.IsSaved = false; // On reset la sauvegarde

                _v.Target.CurrentHp = targetState.Rollback.CurrentHp;
                _v.Target.CurrentMp = targetState.Rollback.CurrentMp;
                _v.Target.Strength = targetState.Rollback.Strength;
                _v.Target.Magic = targetState.Rollback.Magic;
                _v.Target.Will = targetState.Rollback.Will;
                _v.Target.PhysicalDefence = targetState.Rollback.PhysicalDefence;
                _v.Target.PhysicalEvade = targetState.Rollback.PhysicalEvade;
                _v.Target.MagicDefence = targetState.Rollback.MagicDefence;
                _v.Target.MagicEvade = targetState.Rollback.MagicEvade;

                if ((targetState.Rollback.SavedStatus & BattleStatus.Trance) == 0 && _v.Target.IsUnderStatus(BattleStatus.Trance))
                    _v.Target.RemoveStatus(BattleStatus.Trance);

                _v.Target.AlterStatus(targetState.Rollback.SavedStatus, _v.Caster);
                _v.Target.Trance = targetState.Rollback.Trance;
            }
        }
    }
}
