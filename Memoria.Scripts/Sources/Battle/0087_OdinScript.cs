using Memoria.Data;
using System;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Odin
    /// </summary>
    [BattleScript(Id)]
    public sealed class OdinScript : IBattleScript
    {
        public const Int32 Id = 0087;

        private readonly BattleCalculator _v;

        public OdinScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Target.IsPlayer && _v.Command.AbilityId == (BattleAbilityId)1533)
            {
                _v.Command.AbilityCategory -= 16; // Remove Magical effect to prevent Vanish to dissapear.
                btl_stat.AlterStatus(_v.Target, TranceSeekCustomAPI.CustomStatusId.PowerUp, parameters: "+2");
                if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)208))
                {
                    if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)1208))
                    {
                        _v.CalcHpMagicRecovery();
                        _v.Target.HpDamage /= 3;
                    }
                }
            }
            else
            {
                if (_v.Target.CheckUnsafetyOrGuard())
                {
                    TranceSeekCustomAPI.MagicAccuracy(_v);
                    _v.Context.HitRate += (Int16)(ff9item.FF9Item_GetCount(RegularItem.Ore) >> 1);
                    if (TranceSeekCustomAPI.TryMagicHit(_v))
                        TranceSeekCustomAPI.TryAlterCommandStatuses(_v);
                }
            }

            // The effect of OdinSword is now coded in AbilityFeatures.txt
            /*if ((_v.Context.Flags & (BattleCalcFlags.Guard | BattleCalcFlags.Miss)) != 0 && _v.Caster.HasSupportAbility(SupportAbility2.OdinSword))
            {
                _v.Context.Flags &= (BattleCalcFlags)65470;
                _v.Context.Attack = _v.Caster.Magic + GameRandom.Next16() % (1 + (_v.Caster.Level + _v.Caster.Magic >> 3));
                _v.Context.AttackPower = _v.Command.Power + 100 - ff9item.FF9Item_GetCount((Int16)GemItem.Ore);
                _v.Context.DefensePower = _v.Target.MagicDefence;
                _v.CalcHpDamage();
            }*/
        }
    }
}
