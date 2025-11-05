using System;
using System.Collections.Generic;
using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Assets;
using Memoria.Data;
using UnityEngine;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Dark Matter
    /// </summary>
    [BattleScript(Id)]
    public sealed class DragonSkillScript : IBattleScript
    {
        public const Int32 Id = 0079;

        private readonly BattleCalculator _v;

        public DragonSkillScript(BattleCalculator v)
        {
            _v = v;
        }

        public static Dictionary<BTL_DATA, SPSEffect> ReiWrathSPS = new Dictionary<BTL_DATA, SPSEffect>();
        public static Dictionary<BTL_DATA, Int32> ReiWrathChance = new Dictionary<BTL_DATA, Int32>();

        public void Perform()
        {
            if (_v.Command.AbilityId == BattleAbilityId.Luna) // Luna
            {
                btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.Dragon, _v.Caster, parameters: "Add");
                return;
            }
            else if (_v.Command.AbilityId == (BattleAbilityId)1587) // Rei's Wrath
            {
                SPSEffect sps = HonoluluBattleMain.battleSPS.AddSequenceSPS(15, -1, 1, true);
                if (sps == null)
                    return;
                btl2d.GetIconPosition(_v.Target, btl2d.ICON_POS_ROOT, out Transform attachTransf, out Vector3 iconOff);
                sps.charTran = _v.Target.Data.gameObject.transform;
                sps.boneTran = attachTransf;
                sps.posOffset = new Vector3(0, -100, 0);
                sps.scale *= 2;
                ReiWrathSPS[_v.Target.Data] = sps;
                ReiWrathChance[_v.Target.Data] = (_v.Target.IsUnderAnyStatus(TranceSeekStatus.Dragon)) ? 100 : (_v.Target.Will / 2);
            }
            else if (_v.Command.AbilityId == (BattleAbilityId)1588) // Litany of Burmecia
            {
                _v.Target.AlterStatus(BattleStatus.Float, _v.Caster);
                _v.Target.AlterStatus(TranceSeekStatus.MentalUp, _v.Caster);
                if (_v.Target.IsUnderAnyStatus(TranceSeekStatus.Dragon) || _v.Caster.IsUnderAnyStatus(BattleStatus.Trance))
                {
                    _v.Target.Flags |= CalcFlag.MpDamageOrHeal;
                    _v.Target.MpDamage = (int)(_v.Target.MaximumMp / 4);
                }
            }
            else if (_v.Command.AbilityId == BattleAbilityId.ReisWind || _v.Caster.Data.dms_geo_id == 297) // Rei's Wind
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
            TranceSeekAPI.DragonMechanic(_v);
        }

        public static void ReiWrathTrigger(BattleCalculator v)
        {
            if (!ReiWrathSPS.TryGetValue(v.Caster.Data, out SPSEffect sps))
                ReiWrathSPS[v.Caster.Data] = null;

            if (sps != null)
            {
                int DragonStack = v.Target.IsUnderAnyStatus(TranceSeekStatus.Dragon) ? (int)v.Target.GetPropertyByName("StatusProperty CustomStatus9 Stack") : 0;
                int StackMaximum = v.Caster.HasSupportAbilityByIndex((SupportAbility)1218) ? 3 : (v.Caster.HasSupportAbilityByIndex((SupportAbility)218) ? 2 : 1);
                if ((Comn.random16() % 100) <= ReiWrathChance[v.Caster.Data] && DragonStack < StackMaximum)
                {
                    ReiWrathSPS[v.Caster.Data].attr = 0;
                    ReiWrathSPS[v.Caster.Data].meshRenderer.enabled = false;
                    v.Target.AlterStatus(TranceSeekStatus.Dragon, v.Caster);
                }
            }
        }
    }
}
