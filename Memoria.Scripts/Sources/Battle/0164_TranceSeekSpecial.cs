using System;
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
            if (_v.Caster.Data.dms_geo_id == 146)
            {
                if (_v.Command.Power == 10)
                {
                    if (_v.Command.HitRate == 1)
                    {
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
