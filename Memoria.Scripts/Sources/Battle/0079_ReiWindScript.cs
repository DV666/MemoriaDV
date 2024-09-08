using System;
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
                TranceSeekCustomAPI.CharacterBonusPassive(_v, "MagicAttack");
                TranceSeekCustomAPI.CasterPenaltyMini(_v);
                _v.CalcHpMagicRecovery();
                if (!_v.Target.IsUnderAnyStatus(BattleStatus.Zombie))
                    _v.Target.Flags |= CalcFlag.HpRecovery;
                if (_v.Target.IsUnderAnyStatus(BattleStatus.LowHP) || (_v.Target.CurrentHp <= _v.Target.MaximumHp / 4 && HPratio <= Comn.random16() % 100) || _v.Target.IsUnderAnyStatus(TranceSeekCustomAPI.CustomStatus.Dragon) || _v.Caster.IsUnderAnyStatus(BattleStatus.Trance))
                {
                    int num4 = GameRandom.Next16() % 7;
                    if (num4 == 0)
                    {
                        if (!_v.Target.IsUnderStatus(BattleStatus.AutoLife))
                        {
                            _v.Target.AlterStatus(BattleStatus.AutoLife);
                        }
                        else
                        {
                            _v.Target.Flags |= (CalcFlag.MpDamageOrHeal);
                            _v.Target.MpDamage = (int)(_v.Target.MaximumMp / 4);
                        }
                        return;
                    }
                    if (num4 == 1 && !_v.Target.IsUnderStatus(BattleStatus.Regen))
                    {
                        _v.Target.AlterStatus(BattleStatus.Regen, _v.Caster);
                        return;
                    }
                    if (num4 == 2 && !_v.Target.IsUnderStatus(BattleStatus.Haste))
                    {
                        _v.Target.AlterStatus(BattleStatus.Haste, _v.Caster);
                        return;
                    }
                    if (num4 == 3 && !_v.Target.IsUnderStatus(BattleStatus.Float))
                    {
                        _v.Target.AlterStatus(BattleStatus.Float, _v.Caster);
                        return;
                    }
                    if (num4 == 4 && !_v.Target.IsUnderStatus(BattleStatus.Shell))
                    {
                        _v.Target.AlterStatus(BattleStatus.Shell, _v.Caster);
                        return;
                    }
                    if (num4 == 5 && !_v.Target.IsUnderStatus(BattleStatus.Vanish))
                    {
                        _v.Target.AlterStatus(BattleStatus.Vanish, _v.Caster);
                        return;
                    }
                    if (num4 == 6 && !_v.Target.IsUnderStatus(BattleStatus.Protect))
                    {
                        _v.Target.AlterStatus(BattleStatus.Protect, _v.Caster);
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
