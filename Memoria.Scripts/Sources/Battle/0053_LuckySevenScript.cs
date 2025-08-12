using Memoria.Assets;
using Memoria.Data;
using System;
using System.Collections.Generic;
using Assets.Sources.Scripts.UI.Common;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Lucky Seven
    /// </summary>
    [BattleScript(Id)]
    public sealed class LuckySevenScript : IBattleScript
    {
        public const Int32 Id = 0053;

        private readonly BattleCalculator _v;

        public LuckySevenScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Caster.PlayerIndex == CharacterId.Zidane)
            {
                if (_v.Command.AbilityId == (BattleAbilityId)1004) // Echauffement
                {
                    _v.Target.AlterStatus(TranceSeekStatus.PowerUp, _v.Caster);
                    _v.Target.AlterStatus(TranceSeekStatus.PerfectDodge, _v.Caster);
                    return;
                }
                else if (_v.Command.AbilityId == BattleAbilityId.LuckySeven) // Extorquer
                {
                    int extortgils = 0;
                    string extortitem = null;
                    BattleEnemy battleEnemy = BattleEnemy.Find(_v.Target);
                    for (int i = 0; i < 4; i++)
                    {
                        if (battleEnemy.StealableItems[i] != RegularItem.NoItem)
                        {
                            FF9ITEM_DATA item = ff9item._FF9Item_Data[battleEnemy.StealableItems[i]];
                            extortgils = (int)(_v.Command.Power * item.price) / 100;
                            extortitem = FF9TextTool.ItemName(battleEnemy.StealableItems[i]);
                            battleEnemy.StealableItems[i] = RegularItem.NoItem;
                            break;
                        }
                    }
                    if (extortgils != 0)
                    {
                        Dictionary<String, String> localizedMessage = new Dictionary<String, String>
                        {
                          { "US", $"+{extortgils} gils!" },
                          { "UK", $"+{extortgils} gils!" },
                          { "JP", $"+{extortgils} ギル!" },
                          { "ES", $"+{extortgils} guiles!" },
                          { "FR", $"+{extortgils} gils !" },
                          { "GR", $"+{extortgils} Gil!" },
                          { "IT", $"+{extortgils} Guil!" },
                        };
                        btl2d.Btl2dReqSymbolMessage(_v.Target.Data, NGUIText.FF9YellowColor, localizedMessage, HUDMessage.MessageStyle.DAMAGE, 5);
                        Dictionary<String, String> BattleMessagesExtort = new Dictionary<String, String>
                        {
                          { "US", $"{extortitem} has been destroyed !" },
                          { "UK", $"{extortitem} has been destroyed !" },
                          { "JP", $"{extortitem} が破壊された！" },
                          { "ES", $"¡{extortitem} ha sido destruido!" },
                          { "FR", $"{extortitem} a été détruit!" },
                          { "GR", $"{extortitem} wurde zerstört!" },
                          { "IT", $"{extortitem} è stato distrutto!" },
                        };
                        BattleMessagesExtort.TryGetValue(Localization.CurrentSymbol, out string CustomMessage);
                        UIManager.Battle.SetBattleFollowMessage(4, CustomMessage);
                        return;
                    }
                    _v.Context.Flags |= BattleCalcFlags.Miss;
                    return;
                }
            }
            else // Physical attack which cancel turn
            {
                if (!_v.Target.TryKillFrozen())
                {
                    _v.PhysicalAccuracy();
                    if (TranceSeekAPI.TryPhysicalHit(_v))
                    {
                        _v.NormalPhysicalParams();
                        TranceSeekAPI.CharacterBonusPassive(_v, "PhysicalAttack");
                        TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                        TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                        TranceSeekAPI.BonusBackstabAndPenaltyLongDistance(_v);
                        TranceSeekAPI.BonusElement(_v);
                        if (_v.CanAttackElementalCommand())
                        {
                            TranceSeekAPI.TryCriticalHit(_v);
                            _v.CalcPhysicalHpDamage();
                            TranceSeekAPI.RaiseTrouble(_v);
                            TranceSeekAPI.TryAlterMagicStatuses(_v);
                        }
                        if (_v.Target.Data.bi.player != 0)
                            UIManager.Battle.RemovePlayerFromAction(_v.Target.Data.btl_id, true);
                        if (!btl_cmd.KillMainCommand(_v.Target.Data))
                            return;
                        _v.Target.Data.bi.atb = 0;
                        if (_v.Target.Data.bi.player != 0 && !FF9StateSystem.Settings.IsATBFull)
                            _v.Target.Data.cur.at = 0;
                        _v.Target.Data.sel_mode = 0;
                    }
                }
            }
        }
    }
}
