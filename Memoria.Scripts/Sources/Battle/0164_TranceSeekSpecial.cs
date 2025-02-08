using System;
using System.Collections.Generic;
using FF9;
using Memoria.Data;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Special
    /// </summary>
    [BattleScript(Id)]
    public sealed class TranceSeekSpecial : IBattleScript
    {
        public const Int32 Id = 0164;

        private readonly BattleCalculator _v;

        public TranceSeekSpecial(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Command.Power == 11 && _v.Command.HitRate == 11) // Ironite (Dragon Force)
            {
                _v.Target.TryAlterSingleStatus(BattleStatusId.ChangeStat, true, _v.Caster, "PhysicalDefence", Math.Min(255, _v.Target.PhysicalDefence + 2));
                Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                                {
                                    { "US", "Defence ↑" },
                                    { "UK", "Defence ↑" },
                                    { "JP", "ぼうぎょりょく↑" },
                                    { "ES", "DIF fisica ↑" },
                                    { "FR", "Défense ↑" },
                                    { "GR", "Defensa F ↑" },
                                    { "IT", "Abwehr ↑" },
                                };
                btl2d.Btl2dReqSymbolMessage(_v.Target.Data, "[F9FF39]", localizedMessage, HUDMessage.MessageStyle.DAMAGE, 0);
                _v.Target.TryAlterSingleStatus(BattleStatusId.ChangeStat, true, _v.Caster, "MagicDefence", Math.Min(255, _v.Target.MagicDefence + 2));
                Dictionary<String, String> localizedMessage2 = new Dictionary<String, String>
                                {
                                    { "US", "Magic Def ↑" },
                                    { "UK", "Magic Def ↑" },
                                    { "JP", "まほうぼうぎょ ↑" },
                                    { "ES", "DIF magica ↑" },
                                    { "FR", "Protection ↑" },
                                    { "GR", "Defensa M ↑" },
                                    { "IT", "Z-Abwehr ↑" },
                                };
                btl2d.Btl2dReqSymbolMessage(_v.Target.Data, "[F9FF39]", localizedMessage2, HUDMessage.MessageStyle.DAMAGE, 5);
            }
            else if (_v.Command.Power == 77 && _v.Command.HitRate == 77) // Giant Drink (Mad Alchemist)
            {
                _v.Target.RemoveStatus(BattleStatus.Mini);
                _v.Target.Data.geo_scale_default = 16384;
            }
            else if (_v.Caster.Data.dms_geo_id == 146)
            {
                if (_v.Command.Power == 10)
                {
                    if (_v.Command.HitRate == 1)
                    {
                        _v.Target.RemoveStatus(BattleStatus.Slow);
                        _v.Target.RemoveStatus(BattleStatus.Haste);
                        if (!_v.Target.IsUnderAnyStatus(BattleStatus.Death))
                        {
                            if (_v.Target.Data.bi.player != 0)
                                UIManager.Battle.RemovePlayerFromAction(_v.Target.Data.btl_id, true);
                            btl_cmd.KillMainCommand(_v.Target.Data);
                            _v.Target.Data.bi.atb = 0;
                            if (_v.Target.Data.bi.player != 0 && !FF9StateSystem.Settings.IsATBFull)
                                _v.Target.Data.cur.at = 0;
                            _v.Target.Data.sel_mode = 0;
                        }
                        btl_stat.MakeStatusesPermanent(_v.Target, BattleStatus.Stop, true);
                        _v.Target.Data.bi.target = 0;
                        TranceSeekCustomAPI.MonsterMechanic[_v.Caster.Data][2] = _v.Target.Id;
                    }
                    else
                    {
                        foreach (BattleUnit unit in BattleState.EnumerateUnits())
                        {
                            if (TranceSeekCustomAPI.MonsterMechanic[_v.Caster.Data][2] == unit.Id)
                            {
                                unit.Data.bi.target = 1;
                                btl_stat.MakeStatusesPermanent(unit, BattleStatus.Stop, false);
                                unit.RemoveStatus(BattleStatus.Stop);
                            }
                        }
                    }
                }

            }
            else
            {
                foreach (BattleStatusId statusId in _v.Target.Data.stat.cur.ToStatusList())
                    btl_stat.RemoveStatus(_v.Target, statusId);
                _v.Context.Flags = 0;
            }
        }
    }
}
