using Memoria.Data;
using System;

// ReSharper disable UseObjectOrCollectionInitializer

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
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            if (_v.Caster.PlayerIndex == CharacterId.Quina)
            {
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
                if (_v.Target.IsUnderAnyStatus(BattleStatus.Berserk) & _v.Target.HasSupportAbilityByIndex((SupportAbility)107))
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
            }
            TranceSeekCustomAPI.SpecialSA(_v);
        }
    }
}
