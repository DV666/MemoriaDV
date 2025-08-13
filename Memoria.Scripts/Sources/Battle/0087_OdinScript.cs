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
                btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.PowerUp, parameters: "+2");
                if (_v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Divine_guidance) && _v.Target.IsPlayer)
                {
                    if (_v.Caster.HasSupportAbilityByIndex(TranceSeekSupportAbility.Divine_guidance_Boosted))
                    {
                        _v.CalcHpMagicRecovery();
                        _v.Target.HpDamage /= 3;
                    }
                }
            }
            else
            {
                if (TranceSeekAPI.CheckUnsafetyOrGuard(_v))
                {
                    TranceSeekAPI.MagicAccuracy(_v);
                    _v.Context.HitRate += (Int16)(ff9item.FF9Item_GetCount(RegularItem.Ore) >> 1);
                    if (TranceSeekAPI.TryMagicHit(_v))
                        TranceSeekAPI.TryAlterCommandStatuses(_v);
                }
            }
        }
    }
}
