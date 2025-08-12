using Memoria.Data;
using System;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Angel’s Snack
    /// </summary>
    [BattleScript(Id)]
    public sealed class AngelSnackScript : IBattleScript
    {
        public const Int32 Id = 0052;

        private readonly BattleCalculator _v;

        public AngelSnackScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Caster.PlayerIndex == CharacterId.Quina)
            {
                BattleStatus PreviousTargetStatus = _v.Target.CurrentStatus;
                if (_v.Target.IsUnderAnyStatus(BattleStatus.Poison) & _v.Target.HasSupportAbility(SupportAbility2.Antibody))
                {
                    _v.Target.RemoveStatus(BattleStatus.Poison);
                }
                if (_v.Target.IsUnderAnyStatus(BattleStatus.Venom) & _v.Target.HasSupportAbility(SupportAbility2.Antibody))
                {
                    _v.Target.RemoveStatus(BattleStatus.Venom);
                }
                if (_v.Target.IsUnderAnyStatus(BattleStatus.Sleep) & _v.Target.HasSupportAbility(SupportAbility2.Insomniac))
                {
                    _v.Target.RemoveStatus(BattleStatus.Sleep);
                }
                if (_v.Target.IsUnderAnyStatus(BattleStatus.Silence) & _v.Target.HasSupportAbility(SupportAbility2.Loudmouth))
                {
                    _v.Target.RemoveStatus(BattleStatus.Silence);
                }
                if (_v.Target.IsUnderAnyStatus(BattleStatus.Freeze) & _v.Target.HasSupportAbility(SupportAbility2.BodyTemp))
                {
                    _v.Target.RemoveStatus(BattleStatus.Freeze);
                }
                if (_v.Target.IsUnderAnyStatus(BattleStatus.Heat) & _v.Target.HasSupportAbility(SupportAbility2.BodyTemp))
                {
                    _v.Target.RemoveStatus(BattleStatus.Heat);
                }
                if (_v.Target.IsUnderAnyStatus(BattleStatus.Blind) & _v.Target.HasSupportAbility(SupportAbility2.BrightEyes))
                {
                    _v.Target.RemoveStatus(BattleStatus.Blind);
                }
                if (_v.Target.IsUnderAnyStatus(BattleStatus.Petrify) & _v.Target.HasSupportAbility(SupportAbility2.Jelly))
                {
                    _v.Target.RemoveStatus(BattleStatus.Petrify);
                }
                if (_v.Target.IsUnderAnyStatus(BattleStatus.GradualPetrify) & _v.Target.HasSupportAbility(SupportAbility2.Jelly))
                {
                    _v.Target.RemoveStatus(BattleStatus.GradualPetrify);
                }
                if (_v.Target.IsUnderAnyStatus(BattleStatus.Confuse) & _v.Target.HasSupportAbility(SupportAbility2.ClearHeaded))
                {
                    _v.Target.RemoveStatus(BattleStatus.Confuse);
                }
                if (_v.Target.IsUnderAnyStatus(BattleStatus.Berserk) & _v.Target.HasSupportAbilityByIndex((SupportAbility)107)) // Inner Peace
                {
                    _v.Target.RemoveStatus(BattleStatus.Berserk);
                }
                if (_v.Target.IsUnderAnyStatus(BattleStatus.Stop) & _v.Target.HasSupportAbility(SupportAbility2.Locomotion))
                {
                    _v.Target.RemoveStatus(BattleStatus.Stop);
                }
                if (_v.Target.IsUnderAnyStatus(BattleStatus.Slow) & _v.Target.HasSupportAbility(SupportAbility2.Locomotion))
                {
                    _v.Target.RemoveStatus(BattleStatus.Slow);
                }
                if (_v.Target.IsUnderAnyStatus(BattleStatus.Virus) & _v.Target.HasSupportAbilityByIndex((SupportAbility)111)) // Pasteurized
                {
                    _v.Target.RemoveStatus(BattleStatus.Virus);
                }
                if (_v.Target.IsUnderAnyStatus(BattleStatus.Trouble) & _v.Target.HasSupportAbilityByIndex((SupportAbility)112)) // United
                {
                    _v.Target.RemoveStatus(BattleStatus.Trouble);
                }
                if (_v.Target.IsUnderAnyStatus(BattleStatus.Mini) & _v.Target.HasSupportAbilityByIndex((SupportAbility)113)) // Abundance
                {
                    _v.Target.RemoveStatus(BattleStatus.Mini);
                }
                if (_v.Target.IsUnderAnyStatus(BattleStatus.Zombie) & _v.Target.HasSupportAbilityByIndex((SupportAbility)114)) // Purity
                {
                    _v.Target.RemoveStatus(BattleStatus.Zombie);
                }
                if (_v.Target.IsUnderAnyStatus(BattleStatus.Doom) & _v.Target.HasSupportAbility(SupportAbility1.AutoRegen)) // Resilience
                {
                    _v.Target.RemoveStatus(BattleStatus.Doom);
                }
                if (PreviousTargetStatus == _v.Target.CurrentStatus)
                {
                    _v.Context.Flags |= BattleCalcFlags.Miss;
                }
            }
        }
    }
}
