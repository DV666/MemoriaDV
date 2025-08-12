using Assets.Sources.Scripts.UI.Common;
using System;
using Memoria.Data;
using Memoria.Assets;
using System.Text;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// What’s That!?
    /// </summary>
    [BattleScript(Id)]
    public sealed class WhatIsThatScript : IBattleScript
    {
        public const Int32 Id = 0054;

        private readonly BattleCalculator _v;

        public WhatIsThatScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            short num = 0;
            string text = string.Empty;
            string text2 = string.Empty;
            string text3 = string.Empty;
            string text4 = string.Empty;
            foreach (BattleUnit battleUnit in BattleState.EnumerateUnits())
            {
                if (battleUnit.IsPlayer || !battleUnit.IsUnderStatus(BattleStatus.EasyKill))
                {
                    if (!battleUnit.IsPlayer && battleUnit.PhysicalEvade != 255 && !battleUnit.IsUnderStatus(BattleStatus.EasyKill) && !battleUnit.IsUnderStatus(BattleStatus.Vanish))
                    {
                        battleUnit.FaceAsUnit(_v.Caster);
                        BattleEnemy battleEnemy = BattleEnemy.Find(battleUnit);
                        if (HasStealableItems(battleEnemy))
                        {
                            if (battleEnemy.StealableItems[3] != RegularItem.NoItem && GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[3], _v.Caster))
                            {
                                if (num == 0)
                                {
                                    text += FF9TextTool.ItemName(battleEnemy.StealableItems[3]);
                                }
                                else if (num == 1)
                                {
                                    text2 += FF9TextTool.ItemName(battleEnemy.StealableItems[3]);
                                }
                                else if (num == 2)
                                {
                                    text3 += FF9TextTool.ItemName(battleEnemy.StealableItems[3]);
                                }
                                else if (num == 3)
                                {
                                    text4 += FF9TextTool.ItemName(battleEnemy.StealableItems[3]);
                                }
                                _v.StealItem(battleEnemy, 3);
                            }
                            else if (battleEnemy.StealableItems[2] != RegularItem.NoItem && GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[2], _v.Caster))
                            {
                                if (num == 0)
                                {
                                    text += FF9TextTool.ItemName(battleEnemy.StealableItems[2]);
                                }
                                else if (num == 1)
                                {
                                    text2 += FF9TextTool.ItemName(battleEnemy.StealableItems[2]);
                                }
                                else if (num == 2)
                                {
                                    text3 += FF9TextTool.ItemName(battleEnemy.StealableItems[2]);
                                }
                                else if (num == 3)
                                {
                                    text4 += FF9TextTool.ItemName(battleEnemy.StealableItems[2]);
                                }
                                _v.StealItem(battleEnemy, 2);
                            }
                            else if (battleEnemy.StealableItems[1] != RegularItem.NoItem && GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[1], _v.Caster))
                            {
                                if (num == 0)
                                {
                                    text += FF9TextTool.ItemName(battleEnemy.StealableItems[1]);
                                }
                                else if (num == 1)
                                {
                                    text2 += FF9TextTool.ItemName(battleEnemy.StealableItems[1]);
                                }
                                else if (num == 2)
                                {
                                    text3 += FF9TextTool.ItemName(battleEnemy.StealableItems[1]);
                                }
                                else if (num == 3)
                                {
                                    text4 += FF9TextTool.ItemName(battleEnemy.StealableItems[1]);
                                }
                                _v.StealItem(battleEnemy, 1);
                            }
                            else if (battleEnemy.StealableItems[0] != RegularItem.NoItem)
                            {
                                if (num == 0)
                                {
                                    text += FF9TextTool.ItemName(battleEnemy.StealableItems[0]);
                                }
                                else if (num == 1)
                                {
                                    text2 += FF9TextTool.ItemName(battleEnemy.StealableItems[0]);
                                }
                                else if (num == 2)
                                {
                                    text3 += FF9TextTool.ItemName(battleEnemy.StealableItems[0]);
                                }
                                else if (num == 3)
                                {
                                    text4 += FF9TextTool.ItemName(battleEnemy.StealableItems[0]);
                                }
                                _v.StealItem(battleEnemy, 0);
                            }
                            else if (TranceSeekAPI.ZidanePassive[battleUnit.Data][2] > 0) // Oeil de voleur
                            {
                                if (battleEnemy.StealableItems[3] != RegularItem.NoItem && GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[3], _v.Caster))
                                {
                                    if (num == 0)
                                    {
                                        text += FF9TextTool.ItemName(battleEnemy.StealableItems[3]);
                                    }
                                    else if (num == 1)
                                    {
                                        text2 += FF9TextTool.ItemName(battleEnemy.StealableItems[3]);
                                    }
                                    else if (num == 2)
                                    {
                                        text3 += FF9TextTool.ItemName(battleEnemy.StealableItems[3]);
                                    }
                                    else if (num == 3)
                                    {
                                        text4 += FF9TextTool.ItemName(battleEnemy.StealableItems[3]);
                                    }
                                    _v.StealItem(battleEnemy, 3);
                                }
                                else if (battleEnemy.StealableItems[2] != RegularItem.NoItem && GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[2], _v.Caster))
                                {
                                    if (num == 0)
                                    {
                                        text += FF9TextTool.ItemName(battleEnemy.StealableItems[2]);
                                    }
                                    else if (num == 1)
                                    {
                                        text2 += FF9TextTool.ItemName(battleEnemy.StealableItems[2]);
                                    }
                                    else if (num == 2)
                                    {
                                        text3 += FF9TextTool.ItemName(battleEnemy.StealableItems[2]);
                                    }
                                    else if (num == 3)
                                    {
                                        text4 += FF9TextTool.ItemName(battleEnemy.StealableItems[2]);
                                    }
                                    _v.StealItem(battleEnemy, 2);
                                }
                                else if (battleEnemy.StealableItems[1] != RegularItem.NoItem && GameRandom.Next8() < StealScript.NewStealableItemRates(battleEnemy.StealableItemRates[1], _v.Caster))
                                {
                                    if (num == 0)
                                    {
                                        text += FF9TextTool.ItemName(battleEnemy.StealableItems[1]);
                                    }
                                    else if (num == 1)
                                    {
                                        text2 += FF9TextTool.ItemName(battleEnemy.StealableItems[1]);
                                    }
                                    else if (num == 2)
                                    {
                                        text3 += FF9TextTool.ItemName(battleEnemy.StealableItems[1]);
                                    }
                                    else if (num == 3)
                                    {
                                        text4 += FF9TextTool.ItemName(battleEnemy.StealableItems[1]);
                                    }
                                    _v.StealItem(battleEnemy, 1);
                                }
                                else if (battleEnemy.StealableItems[0] != RegularItem.NoItem)
                                {
                                    if (num == 0)
                                    {
                                        text += FF9TextTool.ItemName(battleEnemy.StealableItems[0]);
                                    }
                                    else if (num == 1)
                                    {
                                        text2 += FF9TextTool.ItemName(battleEnemy.StealableItems[0]);
                                    }
                                    else if (num == 2)
                                    {
                                        text3 += FF9TextTool.ItemName(battleEnemy.StealableItems[0]);
                                    }
                                    else if (num == 3)
                                    {
                                        text4 += FF9TextTool.ItemName(battleEnemy.StealableItems[0]);
                                    }
                                    _v.StealItem(battleEnemy, 0);
                                }
                            }
                            else if (num == 0)
                            {
                                text += Localization.Get("Miss");
                            }
                            else if (num == 1)
                            {
                                text2 += Localization.Get("Miss");
                            }
                            else if (num == 2)
                            {
                                text3 += Localization.Get("Miss");
                            }
                            else if (num == 3)
                            {
                                text4 += Localization.Get("Miss");
                            }
                            num += 1;
                        }
                        else
                        {
                            if (num == 0)
                            {
                                text += Localization.Get("Miss");
                            }
                            else if (num == 1)
                            {
                                text2 += Localization.Get("Miss");
                            }
                            else if (num == 2)
                            {
                                text3 += Localization.Get("Miss");
                            }
                            else if (num == 3)
                            {
                                text4 += Localization.Get("Miss");
                            }
                            num += 1;
                        }
                    }
                }
                else
                {
                    battleUnit.ChangeRowToDefault();
                }
            }
            if (num == 1)
            {
                UiState.SetBattleFollowFormatMessage(BattleMesages.Stole, new object[]
                {
                    string.Concat(new string[]
                    {
                        text
                    })
                });
                return;
            }
            if (num == 2)
            {
                UiState.SetBattleFollowFormatMessage(BattleMesages.Stole, new object[]
                {
                    string.Concat(new string[]
                    {
                        text,
                        " / ",
                        text2
                    })
                });
                return;
            }
            if (num == 3)
            {
                UiState.SetBattleFollowFormatMessage(BattleMesages.Stole, new object[]
                {
                    string.Concat(new string[]
                    {
                        text,
                        " / ",
                        text2,
                        " / ",
                        text3
                    })
                });
                return;
            }
            if (num == 4)
            {
                UiState.SetBattleFollowFormatMessage(BattleMesages.Stole, new object[]
                {
                    string.Concat(new string[]
                    {
                        text,
                        " / ",
                        text2,
                        " / ",
                        text3,
                        " / ",
                        text4
                    })
                });
                return;
            }
        }

        private static bool HasStealableItems(BattleEnemy enemy)
        {
            bool result = false;
            for (short num = 0; num < 4; num += 1)
            {
                bool flag = enemy.StealableItems[num] != Data.RegularItem.NoItem;
                if (flag)
                {
                    result = true;
                }
            }
            return result;
        }
    }
}
