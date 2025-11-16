using Memoria.Data;
using System;
using System.Collections.Generic;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Death
    /// </summary>
    [BattleScript(Id)]
    public sealed class DeathScript : IBattleScript
    {
        public const Int32 Id = 0014;

        private readonly BattleCalculator _v;

        public DeathScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Caster.Data.dms_geo_id == 401 && _v.Command.HitRate == 1) // Friendly Feather Circle - Mega Death
            {
                if (SFX.currentEffectID == SpecialEffect.LV5_Death)
                {
                    _v.Target.RemoveStatus(BattleStatus.AutoLife);
                    if (_v.Target.IsZombie)
                    {
                        if (_v.Target.CanBeAttacked())
                        {
                            _v.Target.CurrentHp = _v.Target.MaximumHp;
                        }
                    }
                    else
                    {
                        TranceSeekAPI.TryAlterCommandStatuses(_v);
                    }
                }
                else
                {
                    Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                    {
                        { "US", "B-Virus!" },
                        { "UK", "B-Virus!" },
                        { "JP", "B-Virus!" },
                        { "ES", "¡B-Virus!" },
                        { "FR", "Virus B !" },
                        { "GR", "B-Virus!" },
                        { "IT", "B-Virus!" },
                    };
                    btl2d.Btl2dReqSymbolMessage(_v.Target.Data, "[00FF00]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 5);
                    btl_stat.MakeStatusesPermanent(_v.Target, BattleStatus.Zombie, true);
                }
                return;
            }
            if (TranceSeekAPI.CheckUnsafetyOrGuard(_v))
            {
                if (_v.Target.IsZombie)
                {
                    if (_v.Target.CanBeAttacked())
                    {
                        _v.Target.CurrentHp = _v.Target.MaximumHp;
                    }
                }
                else
                {
                    TranceSeekAPI.MagicAccuracy(_v);
                    _v.Target.PenaltyShellHitRate();
                    _v.PenaltyCommandDividedHitRate();
                    if (TranceSeekAPI.TryMagicHit(_v) || _v.Command.HitRate == 255)
                    {
                        TranceSeekAPI.TryAlterCommandStatuses(_v);
                    }
                }
            }
        }
    }
}
