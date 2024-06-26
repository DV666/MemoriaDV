using Memoria.Assets;
using Memoria.Data;
using System;
using System.Collections.Generic;
using Assets.Sources.Scripts.UI.Common;
using static Memoria.Scripts.Battle.TranceSeekCustomAPI;

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
            TranceSeekCustomAPI.InitCustomBTLDATA(_v);
            if (_v.Caster.PlayerIndex == CharacterId.Zidane)
            {
                if (_v.Command.AbilityId == (BattleAbilityId)1004) // Echauffement
                {
                    TranceSeekCustomAPI.SPSCumulative(_v.Target.Data, SPSStackable.SuperDodge);
                    TranceSeekCustomAPI.SpecialSA(_v);
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
                        BattleMessagesExtort.TryGetValue(Localization.GetSymbol(), out string CustomMessage);
                        UIManager.Battle.SetBattleFollowMessage(4, CustomMessage);
                        return;
                    }
                    _v.Context.Flags |= BattleCalcFlags.Miss;
                    TranceSeekCustomAPI.SpecialSA(_v);
                    return;
                }
            }
            else // Physical attack which cancel turn
            {
                if (!_v.Target.TryKillFrozen())
                {
                    _v.PhysicalAccuracy();
                    if (TranceSeekCustomAPI.TryPhysicalHit(_v))
                    {
                        _v.NormalPhysicalParams();
                        TranceSeekCustomAPI.CharacterBonusPassive(_v, "PhysicalAttack");
                        _v.Caster.PhysicalPenaltyAndBonusAttack();
                        TranceSeekCustomAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                        TranceSeekCustomAPI.BonusBackstabAndPenaltyLongDistanceTranceSeek(_v);
                        _v.BonusElement();
                        if (_v.CanAttackElementalCommand())
                        {
                            TranceSeekCustomAPI.TryCriticalHit(_v);
                            _v.CalcPhysicalHpDamage();
                            TranceSeekCustomAPI.RaiseTrouble(_v);
                            _v.TryAlterMagicStatuses();
                        }
                        TranceSeekCustomAPI.SpecialSA(_v);
                        if (_v.Target.Data.bi.player != 0)
                            UIManager.Battle.RemovePlayerFromAction(_v.Target.Data.btl_id, true);
                        if (!btl_cmd.KillCommand2(_v.Target.Data))
                            return;
                        _v.Target.Data.bi.atb = 0;
                        if (_v.Target.Data.bi.player != 0 && !FF9StateSystem.Settings.IsATBFull)
                            _v.Target.Data.cur.at = 0;
                        _v.Target.Data.sel_mode = 0;
                    }
                }
            }
            TranceSeekCustomAPI.SpecialSA(_v);
        }
    }
}
