using Memoria.Data;
using Memoria.Prime;
using System;

namespace Memoria.Scripts.TranceSeek
{
    /// <summary>
    /// Ether
    /// </summary>
    [BattleScript(Id)]
    public sealed class ItemEtherScript : IBattleScript, IEstimateBattleScript
    {
        public const Int32 Id = 0070;

        private readonly BattleCalculator _v;

        public ItemEtherScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            _v.Context.Attack = 15;
            _v.Context.AttackPower = _v.Command.Item.Power;
            _v.Context.DefensePower = 0;

            _v.CalcMpMagicRecovery();

            if (_v.Caster.PlayerIndex == CharacterId.Blank && _v.Command.Id == BattleCommandId.Item)
                _v.CasterState().Blank.SoakedBlade = _v.Command.ItemId;
        }

        public Single RateTarget()
        {
            _v.Context.Attack = 15;
            _v.Context.AttackPower = _v.Command.Item.Power;
            _v.Context.DefensePower = 0;

            _v.CalcMpMagicRecovery();

            Single rate = _v.Target.MpDamage * BattleScriptDamageEstimate.RateHpMp((Int32)_v.Target.CurrentMp, (Int32)_v.Target.MaximumMp);

            if ((_v.Target.Flags & CalcFlag.MpRecovery) != CalcFlag.MpRecovery)
                rate *= -1;
            if (!_v.Target.IsPlayer)
                rate *= -1;

            return rate;
        }
    }
}

