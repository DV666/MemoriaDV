using System;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Phoenix
    /// </summary>
    [BattleScript(Id)]
    public sealed class PhoenixScript : IBattleScript
    {
        public const Int32 Id = 0049;

        private readonly BattleCalculator _v;

        public PhoenixScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            if (_v.Target.Accessory == (RegularItem)1213)
            {
                _v.Target.PermanentStatus &= ~BattleStatus.Doom;
                btl_stat.RemoveStatus(_v.Target, BattleStatus.Doom);
            }
            Boolean applyToPlayer = _v.Target.IsPlayer;
            MutableBattleCommand command = new MutableBattleCommand(_v.Caster, _v.Target.Id, _v.Command.Id, applyToPlayer ? BattleAbilityId.RebirthFlame : BattleAbilityId.Phoenix);
            command.IsShortSummon = _v.Command.IsShortSummon;
            command.ScriptId = (Byte)(applyToPlayer ? ReviveScript.Id : MagicAttackScript.Id);
            SBattleCalculator.CalcMain(_v.Caster, _v.Target, command);
            _v.PerformCalcResult = false;
            TranceSeekCustomAPI.SpecialSA(_v);
        }
    }
}