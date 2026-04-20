using System;
using System.Collections.Generic;
using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Data;
using Memoria.Prime;
using static TitleUI;

namespace Memoria.Scripts.TranceSeek
{
    /// <summary>
    /// Special
    /// </summary>
    [BattleScript(Id)]
    public sealed class MixOresScript : IBattleScript
    {
        public const Int32 Id = 0203;

        private readonly BattleCalculator _v;

        public MixOresScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Command.ItemId == RegularItem.NoItem)
            {
                _v.Context.Flags = BattleCalcFlags.Miss;
                return;
            }

            int HPHeal = 0;
            int MPHeal = 0;
            int PowerGemVanilla = _v.Command.Item.Power * (ff9item.FF9Item_GetCount(_v.Command.ItemId) + 1);
            _v.Command.Power = _v.Command.Item.Power;
            switch (_v.Command.ItemId)
            {
                case TranceSeekRegularItem.MotherG: // Mother G
                case TranceSeekRegularItem.SuperMotherG: // Super Mother G
                {
                    _v.Command.AbilityStatus = _v.Command.ItemStatus;
                    TranceSeekAPI.TryAlterCommandStatuses(_v);
                    break;
                }
                case TranceSeekRegularItem.SuperPeridot: // Super Péridot
                case TranceSeekRegularItem.SuperTopaz: // Super Topaze
                case TranceSeekRegularItem.SuperOpal: // Super Opale
                case TranceSeekRegularItem.SuperAmethyst: // Super Améthyst
                case TranceSeekRegularItem.SuperAquamarine: // Super Beryl
                case TranceSeekRegularItem.SuperGarnet: // Super Grenat
                case TranceSeekRegularItem.SuperSapphire: // Super Saphir
                case TranceSeekRegularItem.SuperRuby: // Super Rubis
                case TranceSeekRegularItem.SuperEmerald: // Super Emeraude
                case TranceSeekRegularItem.SuperLapisLazuli: // Super Lapis Lazuli
                case TranceSeekRegularItem.SuperMoonstone: // Super Lunalithe
                case TranceSeekRegularItem.SuperDiamond: // Super Diamant
                {
                    HPHeal = PowerGemVanilla * 2;
                    break;
                }
                case TranceSeekRegularItem.Peridotopaz: // Péridotopaze
                case TranceSeekRegularItem.Peridotopal: // Péridotopale
                case TranceSeekRegularItem.Peridothyst: // Péridothyste
                case TranceSeekRegularItem.Peridamarine: // Péridoryl
                case TranceSeekRegularItem.Peridenat: // Péridenat
                case TranceSeekRegularItem.Peridapphire: // Péridaphir
                case TranceSeekRegularItem.Periderald: // Périderaude
                case TranceSeekRegularItem.Peridazuli: // Péridazuli
                case TranceSeekRegularItem.Peridoon: // Péridalithe
                case TranceSeekRegularItem.Topal: // Topale
                case TranceSeekRegularItem.Topamethyst: // Topaméthyste
                case TranceSeekRegularItem.Topamarine: // Toperyl
                case TranceSeekRegularItem.Topenat: // Topenat
                case TranceSeekRegularItem.Topapphire: // Topaphir
                case TranceSeekRegularItem.Toperald: // Toperaude
                case TranceSeekRegularItem.Topazuli: // Topazuli
                case TranceSeekRegularItem.Topoon: // Topalithe
                case TranceSeekRegularItem.Opalethyst: // Opalèthyste
                case TranceSeekRegularItem.Opamarine: // Operyl
                case TranceSeekRegularItem.Openat: // Openat
                case TranceSeekRegularItem.Opapphire: // Opaphir
                case TranceSeekRegularItem.Operald: // Operaude
                case TranceSeekRegularItem.Opazuli: // Opazuli
                case TranceSeekRegularItem.Opoon: // Opalithe
                case TranceSeekRegularItem.Aquathyst: // Berhyste
                case TranceSeekRegularItem.Garnethyst: // Grenéthyst
                case TranceSeekRegularItem.Amepphire: // Améphir
                case TranceSeekRegularItem.Amerald: // Améraude
                case TranceSeekRegularItem.Amethazuli: // Améthazuli
                case TranceSeekRegularItem.Amoon: // Lunathyst
                case TranceSeekRegularItem.Garnarine: // Greryl
                case TranceSeekRegularItem.Aquaphir: // Beryphir
                case TranceSeekRegularItem.Aquarald: // Berylaude
                case TranceSeekRegularItem.Aquazuli: // Berylazuli
                case TranceSeekRegularItem.Aquoon: // Berylithe
                case TranceSeekRegularItem.Grenapphire: // Grenaphir
                case TranceSeekRegularItem.Garrald: // Greraude
                case TranceSeekRegularItem.Garnazuli: // Grenazuli
                case TranceSeekRegularItem.Garnoon: // Grenalithe
                case TranceSeekRegularItem.Sappherald: // Saphemaude
                case TranceSeekRegularItem.LapisLapphire: // Lapis Laphir
                case TranceSeekRegularItem.Sapphoon: // Lunaphir
                case TranceSeekRegularItem.Lazerald: // Lazeraude
                case TranceSeekRegularItem.Moonerald: // Luneraude
                case TranceSeekRegularItem.LapisMoonazuli: // Lapis Lunazuli
                case TranceSeekRegularItem.Peridiamond: // Péridiamant
                case TranceSeekRegularItem.Diamaz: // Diamaze
                case TranceSeekRegularItem.Diapal: // Diapale
                case TranceSeekRegularItem.Diamethyst: // Diaméthyst
                case TranceSeekRegularItem.Diamarine: // Diameryl
                case TranceSeekRegularItem.Garnond: // Greniamant
                case TranceSeekRegularItem.Diaphir: // Diaphir
                case TranceSeekRegularItem.Diamubis: // Diamubis
                case TranceSeekRegularItem.Diamerald: // Diameraude
                case TranceSeekRegularItem.Diazuli: // Diazuli
                case TranceSeekRegularItem.Diamoon: // Diamalithe
                {
                    RegularItem[] ingredients = new RegularItem[2];
                    EffectElement element1 = EffectElement.None;
                    EffectElement element2 = EffectElement.None;
                    foreach (MixItems mixCandidate in ff9mixitem.MixItemsData.Values)
                    {
                        if (mixCandidate.Id < 0)
                            continue;
                        if (mixCandidate.Result == _v.Command.ItemId)
                        {
                            for (int i = 0; i < mixCandidate.Ingredients.Count; i++)
                            {
                                ingredients[i] = mixCandidate.Ingredients[i];
                            }
                        }                     
                    }
                    HPHeal = PowerGemVanilla;
                    if (ElementItem.ContainsKey(ingredients[0]))
                        element1 = ElementItem[ingredients[0]];
                    if (ElementItem.ContainsKey(ingredients[1]))
                        element2 = ElementItem[ingredients[1]];

                    if (ingredients[0] == RegularItem.Garnet || ingredients[1] == RegularItem.Garnet)
                    {
                        if ((element1 > 0 && _v.Target.AbsorbElement == element1) || (element2 > 0 && _v.Target.AbsorbElement == element2))
                            HPHeal *= 4;
                    }
                    else
                    {
                        if ((_v.Target.AbsorbElement & element1) != 0)
                            HPHeal *= 2;
                        if ((_v.Target.AbsorbElement & element2) != 0)
                            HPHeal *= 2;
                    }
                    break;
                }
                case TranceSeekRegularItem.RainOfOres: // Pluie de Gemme
                case TranceSeekRegularItem.RainOfPeridots: // Pluie de Péridot
                case TranceSeekRegularItem.RainOfTopazes: // Pluie de Topaze
                case TranceSeekRegularItem.RainOfOpales: // Pluie d'Opale
                case TranceSeekRegularItem.RainOfAmethyst: // Pluie d'Améthyste
                case TranceSeekRegularItem.RainOfAquamarine: // Pluie de Beryl
                case TranceSeekRegularItem.RainOfGarnet: // Pluie de Grenat
                case TranceSeekRegularItem.RainOfSapphire: // Pluie de Saphir
                case TranceSeekRegularItem.RainOfRuby: // Pluie de Rubis
                case TranceSeekRegularItem.RainOfEmerald: // Pluie d'Emeraude
                case TranceSeekRegularItem.RainOfLapisLazuli: // Pluie de Lapis Lazuli
                case TranceSeekRegularItem.RainOfMoonstone: // Pluie de Lunalithe
                case TranceSeekRegularItem.RainOfDiamond: // Pluie de Diamant
                {
                    HPHeal = PowerGemVanilla;
                    break;
                }
                case TranceSeekRegularItem.BrilliantLightningOrb: // Globe brilliant de foudre
                case TranceSeekRegularItem.BrilliantFireOrb: // Globe brilliant de feu
                case TranceSeekRegularItem.BrilliantIceOrb: // Globe brilliant de glace
                case TranceSeekRegularItem.BrilliantGravityOrb: // Globe brilliant de gravité
                case TranceSeekRegularItem.BrilliantWaterOrb: // Globe brilliant d'eau
                case TranceSeekRegularItem.BrilliantShiningOrb: // Globe brilliant flambloyant
                case TranceSeekRegularItem.BrilliantEarthOrb: // Globe brilliant de terre
                case TranceSeekRegularItem.BrilliantWindOrb: // Globe brilliant de vent
                case TranceSeekRegularItem.BrilliantDarkOrb: // Globe brilliant de ténèbres
                case TranceSeekRegularItem.BrilliantLightOrb: // Globe brilliant de lumière
                {
                    _v.Command.Power = _v.Command.Item.Power;
                    _v.NormalMagicParams();
                    if (ElementItem.ContainsKey(_v.Command.ItemId))
                        _v.Command.Element = ElementItem[_v.Command.ItemId];

                    if (_v.Target.IsUnderAnyStatus(BattleStatus.Reflect))
                        _v.Command.Power *= 2;

                    TranceSeekAPI.CasterPenaltyMini(_v);
                    TranceSeekAPI.EnemyTranceBonusAttack(_v);
                    TranceSeekAPI.PenaltyShellAttack(_v);
                    TranceSeekAPI.PenaltyCommandDividedAttack(_v);
                    TranceSeekAPI.BonusElement(_v);
                    if (TranceSeekAPI.CanAttackMagic(_v))
                    {
                        _v.CalcHpDamage();
                        TranceSeekAPI.RaiseTrouble(_v);
                    }
                    return;
                }
                case TranceSeekRegularItem.YellowElixir: // Elixir Jaune
                case TranceSeekRegularItem.RedElixir: // Elixir Rouge
                case TranceSeekRegularItem.CyanElixir: // Elixir Cyan
                case TranceSeekRegularItem.PurpleElixir: // Elixir Violet
                case TranceSeekRegularItem.BlueElixir: // Elixir Bleu
                case TranceSeekRegularItem.CrystallineElixir: // Elixir Cristallin
                case TranceSeekRegularItem.BrownElixir: // Elixir Marron
                case TranceSeekRegularItem.GreenElixir: // Elixir Vert
                case TranceSeekRegularItem.BlackElixir: // Elixir Noir
                case TranceSeekRegularItem.WhiteElixir: // Elixir Blanc
                case TranceSeekRegularItem.YellowMegalixir: // Megalixir Jaune
                case TranceSeekRegularItem.RedMegalixir: // Megalixir Rouge
                case TranceSeekRegularItem.CyanMegalixir: // Megalixir Cyan
                case TranceSeekRegularItem.PurpleMegalixir: // Megalixir Violet
                case TranceSeekRegularItem.BlueMegalixir: // Megalixir Bleu
                case TranceSeekRegularItem.CrystallineMegalixir: // Megalixir Cristallin
                case TranceSeekRegularItem.BrownMegalixir: // Megalixir Marron
                case TranceSeekRegularItem.GreenMegalixir: // Megalixir Vert
                case TranceSeekRegularItem.BlackMegalixir: // Megalixir Noir
                case TranceSeekRegularItem.WhiteMegalixir: // Megalixir Blanc
                {
                    Boolean Message = false;
                    string ItemName = FF9TextTool.ItemName(_v.Command.ItemId);
                    var Target_TSVar = _v.TargetState();

                    if (ElementItem.ContainsKey(_v.Command.ItemId))
                        Target_TSVar.AbsorbElement = (Int32)ElementItem[_v.Command.ItemId];
                    if (_v.Command.ItemId == TranceSeekRegularItem.PurpleElixir || _v.Command.ItemId == TranceSeekRegularItem.PurpleMegalixir)
                        Target_TSVar.AbsorbElement = 256; // Gravity

                    Int32 wait = (short)((400 + (_v.Caster.Will * 3)) * 30);
                    _v.Target.AddDelayedModifier(
                    target => (wait -= target.Data.cur.at_coef * BattleState.ATBTickCount) > 0,
                    target =>
                    {
                        Target_TSVar.AbsorbElement = -1;
                        if (!Message)
                        {
                            Dictionary<String, String> localizedStatusProtect = new Dictionary<String, String>
                            {
                                { "US", $"- {ItemName}" },
                                { "UK", $"- {ItemName}" },
                                { "JP", $"- {ItemName}" },
                                { "ES", $"- {ItemName}" },
                                { "FR", $"- {ItemName}" },
                                { "GR", $"- {ItemName}" },
                                { "IT", $"- {ItemName}" },
                            };
                            btl2d.Btl2dReqSymbolMessage(target.Data, "[38FF1F]", localizedStatusProtect, HUDMessage.MessageStyle.DAMAGE, 5);
                            Message = true;
                        }
                    }
                    );
                    HPHeal = (int)(_v.Target.MaximumHp);
                    MPHeal = (int)(_v.Target.MaximumMp);
                    break;
                }
                case TranceSeekRegularItem.IronStone: // Roc de fer
                {
                    btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.ArmorUp, parameters: "+1");
                    return;
                }
                case TranceSeekRegularItem.TitaniumStone: // Roc de titane
                {
                    btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.ArmorUp, parameters: "+2");
                    return;
                }
                case TranceSeekRegularItem.AdamantiumStone: // Roc d'adamantium
                {
                    btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.ArmorUp, parameters: "+3");
                    return;
                }
                case TranceSeekRegularItem.PurplishStone: // Roc violâtre
                {
                    btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.MentalUp, parameters: "+2");
                    return;
                }
                case TranceSeekRegularItem.SpiritualStone: // Roc spirituel
                {
                    btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.MentalUp, parameters: "+4");
                    return;
                }
                case TranceSeekRegularItem.FabulousStone: // Roc fabuleux
                {
                    btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.ArmorUp, parameters: "+1");
                    btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.MentalUp, parameters: "+1");
                    return;
                }
                case TranceSeekRegularItem.MysticalStone: // Roc mystique
                {
                    btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.ArmorUp, parameters: "+2");
                    btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.MentalUp, parameters: "+2");
                    return;
                }
                case TranceSeekRegularItem.MythrilStone: // Roc en mythril
                case TranceSeekRegularItem.MythrilMegagem: // Megakoroc en mythril
                {
                    btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.ArmorUp, parameters: "+5");
                    btl_stat.AlterStatus(_v.Target, TranceSeekStatusId.MentalUp, parameters: "+5");
                    return;
                }
            }

            if (HPHeal > 0)
                _v.Target.Flags |= CalcFlag.HpDamageOrHeal;

            if (MPHeal > 0)
                _v.Target.Flags |= CalcFlag.MpDamageOrHeal;

            if ((_v.Target.Flags & CalcFlag.HpAlteration) != 0)
                _v.Target.HpDamage = HPHeal;

            if ((_v.Target.Flags & CalcFlag.MpAlteration) != 0)
                _v.Target.MpDamage = MPHeal;

            foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(_v.Caster))
                saFeature.TriggerOnAbility(_v, "CalcDamage", false);
            foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(_v.Target))
                saFeature.TriggerOnAbility(_v, "CalcDamage", true);
        }

        public static Dictionary<RegularItem, EffectElement> ElementItem = new Dictionary<RegularItem, EffectElement>
        {
            { RegularItem.Peridot, EffectElement.Thunder },
            { RegularItem.Topaz, EffectElement.Fire },
            { RegularItem.Opal, EffectElement.Cold },
            { RegularItem.Aquamarine, EffectElement.Aqua },
            { RegularItem.Sapphire, EffectElement.Earth },
            { RegularItem.Garnet, EffectElement.None },
            { RegularItem.Emerald, EffectElement.Wind },
            { RegularItem.LapisLazuli, EffectElement.Darkness },
            { TranceSeekRegularItem.YellowElixir, EffectElement.Thunder },
            { TranceSeekRegularItem.RedElixir, EffectElement.Fire },
            { TranceSeekRegularItem.CyanElixir, EffectElement.Cold },
            { TranceSeekRegularItem.PurpleElixir, EffectElement.None }, // Gravity
            { TranceSeekRegularItem.BlueElixir, EffectElement.Aqua },
            { TranceSeekRegularItem.CrystallineElixir, EffectElement.None },
            { TranceSeekRegularItem.BrownElixir, EffectElement.Earth },
            { TranceSeekRegularItem.GreenElixir, EffectElement.Wind },
            { TranceSeekRegularItem.BlackElixir, EffectElement.Darkness },
            { TranceSeekRegularItem.WhiteElixir, EffectElement.Holy },
            { TranceSeekRegularItem.YellowMegalixir, EffectElement.Thunder },
            { TranceSeekRegularItem.RedMegalixir, EffectElement.Fire },
            { TranceSeekRegularItem.CyanMegalixir, EffectElement.Cold },
            { TranceSeekRegularItem.PurpleMegalixir, EffectElement.None }, // Gravity
            { TranceSeekRegularItem.BlueMegalixir, EffectElement.Aqua },
            { TranceSeekRegularItem.CrystallineMegalixir, EffectElement.None },
            { TranceSeekRegularItem.BrownMegalixir, EffectElement.Earth },
            { TranceSeekRegularItem.GreenMegalixir, EffectElement.Wind },
            { TranceSeekRegularItem.BlackMegalixir, EffectElement.Darkness },
            { TranceSeekRegularItem.WhiteMegalixir, EffectElement.Holy },
            { TranceSeekRegularItem.BrilliantLightningOrb, EffectElement.Thunder },
            { TranceSeekRegularItem.BrilliantFireOrb, EffectElement.Fire },
            { TranceSeekRegularItem.BrilliantIceOrb, EffectElement.Cold },
            { TranceSeekRegularItem.BrilliantWaterOrb, EffectElement.Aqua },
            { TranceSeekRegularItem.BrilliantEarthOrb, EffectElement.Earth },
            { TranceSeekRegularItem.BrilliantWindOrb, EffectElement.Wind },
            { TranceSeekRegularItem.BrilliantDarkOrb, EffectElement.Darkness },
            { TranceSeekRegularItem.BrilliantLightOrb, EffectElement.Holy }
        };

        private Boolean HitRateForZombie()
        {
            if (_v.Target.IsZombie)
            {
                TranceSeekAPI.MagicAccuracy(_v);
                return true;
            }
            return false;
        }
    }
}

