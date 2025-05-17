using System;
using System.Collections.Generic;
using FF9;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Dark Matter
    /// </summary>
    [BattleScript(Id)]
    public sealed class ReiWindScript : IBattleScript
    {
        public const Int32 Id = 0079;

        private readonly BattleCalculator _v;

        public ReiWindScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
			if (_v.Caster.PlayerIndex == CharacterId.Freya || _v.Caster.Data.dms_geo_id == 297)
			{
                if (!_v.Target.CanBeAttacked())
                {
                    return;
                }
                uint HPratio = (_v.Target.CurrentHp / _v.Target.MaximumHp) * 100;
                _v.Target.Flags = CalcFlag.HpAlteration;
                _v.NormalMagicParams();
                TranceSeekAPI.CharacterBonusPassive(_v, "MagicAttack");
                TranceSeekAPI.CasterPenaltyMini(_v);
                _v.CalcHpMagicRecovery();
                if (!_v.Target.IsUnderAnyStatus(BattleStatus.Zombie))
                    _v.Target.Flags |= CalcFlag.HpRecovery;
                if (_v.Target.IsUnderAnyStatus(TranceSeekStatus.Dragon) || _v.Caster.IsUnderAnyStatus(BattleStatus.Trance))
                    _v.Target.HpDamage *= 2;
                if (_v.Target.IsUnderAnyStatus(BattleStatus.LowHP) || (_v.Target.CurrentHp <= _v.Target.MaximumHp / 4 && HPratio <= Comn.random16() % 100))
                {
                    BattleStatusId[] statuslist = { BattleStatusId.Regen, BattleStatusId.Haste, BattleStatusId.Float, BattleStatusId.Shell, BattleStatusId.Vanish,
                    BattleStatusId.Protect, BattleStatusId.AutoLife};

                    List<BattleStatusId> statuschoosen = new List<BattleStatusId>();

                    for (Int32 i = 0; i < statuslist.Length; i++)
                    {
                        if ((statuslist[i].ToBattleStatus() & _v.Target.CurrentStatus) == 0)
                        {
                            statuschoosen.Add(statuslist[i]);
                        }
                    }
                    if (statuschoosen.Count > 0)

                    {
                        BattleStatusId statusselected = statuschoosen[GameRandom.Next16() % statuschoosen.Count];
                        btl_stat.AlterStatus(_v.Target, statusselected, _v.Caster);
                        return;
                    }

                    _v.Target.Flags |= (CalcFlag.MpAlteration);
                    if (!_v.Target.IsUnderAnyStatus(BattleStatus.Zombie))
                        _v.Target.Flags |= CalcFlag.MpRecovery;
                    _v.Target.MpDamage = (int)(_v.Target.MaximumMp / 4);
                    return;
                }
            }
        }
    }
}
