using System;
using System.Collections.Generic;
using FF9;
using Memoria.Data;
using Memoria.Prime;
using static SiliconStudio.Social.ResponseData;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Special
    /// </summary>
    [BattleScript(Id)]
    public sealed class MixStoneScript : IBattleScript
    {
        public const Int32 Id = 0202;

        private readonly BattleCalculator _v;

        public MixStoneScript(BattleCalculator v)
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

            int HPDamage = 0;
            int MPDamage = 0;
            _v.Command.Power = _v.Command.Item.Power;
            switch (_v.Command.ItemId)
            {
                case (RegularItem)2162: // Bombe
                case (RegularItem)2163: // Super Bombe
                case (RegularItem)2164: // Giga Bombe
                case (RegularItem)2167: // Méga Bombe
                case (RegularItem)2168: // Bombe à Neutron
                case (RegularItem)2171: // Bombe toxique
                case (RegularItem)2172: // Bombe silence
                case (RegularItem)2173: // Bombe aveuglante
                case (RegularItem)2174: // Bombe pétrifiante
                case (RegularItem)2175: // Bombe troublante
                case (RegularItem)2176: // Bombe infectée
                case (RegularItem)2177: // Bombe Virus
                case (RegularItem)2178: // Bombe chaotique
                case (RegularItem)2179: // Bombe Anima
                {
                    _v.NormalMagicParams();
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
                    _v.Command.HitRate = _v.Caster.Will;
                    _v.Command.AbilityStatus = _v.Command.ItemStatus;
                    TranceSeekAPI.TryAlterMagicStatuses(_v);
                    return;
                }
                case (RegularItem)2169: // Ultra Bombe G
                case (RegularItem)2170: // Mega Bombe G
                {
                    if (_v.Target.IsUnderAnyStatus(BattleStatus.EasyKill))
                    {
                        _v.Context.Flags = BattleCalcFlags.Miss;
                        return;
                    }
                    _v.Target.CurrentHp = 1;
                    _v.Target.CurrentMp = 1;
                    break;
                }
                case (RegularItem)2180: // Bombe toxique MK.T
                case (RegularItem)2181: // Bombe silence MK.T
                case (RegularItem)2182: // Bombe aveuglante MK.T
                case (RegularItem)2183: // Bombe pétrifiante MK.T
                case (RegularItem)2184: // Bombe troublante MK.T
                case (RegularItem)2185: // Bombe infectée MK.T
                case (RegularItem)2186: // Bombe Virus MK.T
                case (RegularItem)2187: // Bombe chaotique MK.T
                case (RegularItem)2188: // Bombe Anima MK.T
                case (RegularItem)2279: // Roc de foudre
                case (RegularItem)2280: // Globe de foudre
                case (RegularItem)2281: // Cristal de foudre
                case (RegularItem)2284: // Magikoroc de foudre
                case (RegularItem)2285: // Megakoroc de foudre
                {
                    _v.NormalMagicParams();
                    _v.Command.Element = EffectElement.Thunder;
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
                    if ((_v.Target.WeakElement & EffectElement.Thunder) != 0)
                        _v.Target.TryAlterStatuses(_v.Command.ItemStatus, false, _v.Caster);
                    return;
                }
                case (RegularItem)2189: // Bombe toxique MK.F
                case (RegularItem)2190: // Bombe silence MK.F
                case (RegularItem)2191: // Bombe aveuglante MK.F
                case (RegularItem)2192: // Bombe pétrifiante MK.F
                case (RegularItem)2193: // Bombe troublante MK.F
                case (RegularItem)2194: // Bombe infectée MK.F
                case (RegularItem)2195: // Bombe Virus MK.F
                case (RegularItem)2196: // Bombe chaotique MK.F
                case (RegularItem)2197: // Bombe Anima MK.F
                case (RegularItem)2286: // Roc de feu
                case (RegularItem)2287: // Globe de feu
                case (RegularItem)2288: // Cristal de feu
                case (RegularItem)2291: // Magikoroc de feu
                case (RegularItem)2292: // Megakoroc de feu
                {
                    _v.NormalMagicParams();
                    _v.Command.Element = EffectElement.Fire;
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
                    if ((_v.Target.WeakElement & EffectElement.Fire) != 0)
                        _v.Target.TryAlterStatuses(_v.Command.ItemStatus, false, _v.Caster);
                    return;
                }
                case (RegularItem)2198: // Bombe toxique MK.I
                case (RegularItem)2199: // Bombe silence MK.I
                case (RegularItem)2200: // Bombe aveuglante MK.I
                case (RegularItem)2201: // Bombe pétrifiante MK.I
                case (RegularItem)2202: // Bombe troublante MK.I
                case (RegularItem)2203: // Bombe infectée MK.I
                case (RegularItem)2204: // Bombe Virus MK.I
                case (RegularItem)2205: // Bombe chaotique MK.I
                case (RegularItem)2206: // Bombe Anima MK.I
                case (RegularItem)2293: // Roc de glace
                case (RegularItem)2294: // Globe de glace
                case (RegularItem)2295: // Cristal de glace
                case (RegularItem)2298: // Magikoroc de glace
                case (RegularItem)2299: // Megakoroc de glace
                {
                    _v.NormalMagicParams();
                    _v.Command.Element = EffectElement.Cold;
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
                    if ((_v.Target.WeakElement & EffectElement.Cold) != 0)
                        _v.Target.TryAlterStatuses(_v.Command.ItemStatus, false, _v.Caster);
                    return;
                }
                case (RegularItem)2207: // Bombe toxique MK.G
                case (RegularItem)2208: // Bombe silence MK.G
                case (RegularItem)2209: // Bombe aveuglante MK.G
                case (RegularItem)2210: // Bombe pétrifiante MK.G
                case (RegularItem)2211: // Bombe troublante MK.G
                case (RegularItem)2212: // Bombe infectée MK.G
                case (RegularItem)2213: // Bombe Virus MK.G
                case (RegularItem)2214: // Bombe chaotique MK.G
                case (RegularItem)2215: // Bombe Anima MK.G
                {
                    _v.NormalMagicParams();
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
                    if ((btl_util.getEnemyTypePtr(_v.Target.Data).category & (Int16)EnemyCategory.Stone) != 0) // TODO : Need to create an Gravity element
                        _v.Target.TryAlterStatuses(_v.Command.ItemStatus, false, _v.Caster);
                    return;
                }            
                case (RegularItem)2216: // Bombe toxique MK.E
                case (RegularItem)2217: // Bombe silence MK.E
                case (RegularItem)2218: // Bombe aveuglante MK.E
                case (RegularItem)2219: // Bombe pétrifiante MK.E
                case (RegularItem)2220: // Bombe troublante MK.E
                case (RegularItem)2221: // Bombe infectée MK.E
                case (RegularItem)2222: // Bombe Virus MK.E
                case (RegularItem)2223: // Bombe chaotique MK.E
                case (RegularItem)2224: // Bombe Anima MK.E
                case (RegularItem)2307: // Roc d'eau
                case (RegularItem)2308: // Globe d'eau
                case (RegularItem)2309: // Cristal d'eau
                case (RegularItem)2312: // Magikoroc d'eau
                case (RegularItem)2313: // Megakoroc d'eau
                {
                    _v.NormalMagicParams();
                    _v.Command.Element = EffectElement.Aqua;
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
                    if ((_v.Target.WeakElement & EffectElement.Aqua) != 0)
                        _v.Target.TryAlterStatuses(_v.Command.ItemStatus, false, _v.Caster);
                    return;
                }
                case (RegularItem)2225: // Bombe toxique MK.A
                case (RegularItem)2226: // Bombe silence MK.A
                case (RegularItem)2227: // Bombe aveuglante MK.A
                case (RegularItem)2228: // Bombe pétrifiante MK.A
                case (RegularItem)2229: // Bombe troublante MK.A
                case (RegularItem)2230: // Bombe infectée MK.A
                case (RegularItem)2231: // Bombe Virus MK.A
                case (RegularItem)2232: // Bombe chaotique MK.A
                case (RegularItem)2233: // Bombe Anima MK.A
                {
                    _v.NormalMagicParams();
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
                    if (_v.Target.WeakElement == 0)
                        _v.Target.TryAlterStatuses(_v.Command.ItemStatus, false, _v.Caster);
                    return;
                }
                case (RegularItem)2234: // Bombe toxique MK.Q
                case (RegularItem)2235: // Bombe silence MK.Q
                case (RegularItem)2236: // Bombe aveuglante MK.Q
                case (RegularItem)2237: // Bombe pétrifiante MK.Q
                case (RegularItem)2238: // Bombe troublante MK.Q
                case (RegularItem)2239: // Bombe infectée MK.Q
                case (RegularItem)2240: // Bombe Virus MK.Q
                case (RegularItem)2241: // Bombe chaotique MK.Q
                case (RegularItem)2242: // Bombe Anima MK.Q
                case (RegularItem)2314: // Roc de terre
                case (RegularItem)2315: // Globe de terre
                case (RegularItem)2316: // Cristal de terre
                case (RegularItem)2319: // Magikoroc de terre
                case (RegularItem)2320: // Megakoroc de terre
                {
                    _v.NormalMagicParams();
                    _v.Command.Element = EffectElement.Earth;
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
                    if ((_v.Target.WeakElement & EffectElement.Earth) != 0)
                        _v.Target.TryAlterStatuses(_v.Command.ItemStatus, false, _v.Caster);
                    return;
                }
                case (RegularItem)2243: // Bombe toxique MK.R
                case (RegularItem)2244: // Bombe silence MK.R
                case (RegularItem)2245: // Bombe aveuglante MK.R
                case (RegularItem)2246: // Bombe pétrifiante MK.R
                case (RegularItem)2247: // Bombe troublante MK.R
                case (RegularItem)2248: // Bombe infectée MK.R
                case (RegularItem)2249: // Bombe Virus MK.R
                case (RegularItem)2250: // Bombe chaotique MK.R
                case (RegularItem)2251: // Bombe Anima MK.R
                {
                    _v.NormalMagicParams();
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
                    if (_v.Target.IsUnderStatus(BattleStatus.Reflect))
                        _v.Target.TryAlterStatuses(_v.Command.ItemStatus, false, _v.Caster);
                    return;
                }
                case (RegularItem)2252: // Bombe toxique MK.W
                case (RegularItem)2253: // Bombe silence MK.W
                case (RegularItem)2254: // Bombe aveuglante MK.W
                case (RegularItem)2255: // Bombe pétrifiante MK.W
                case (RegularItem)2256: // Bombe troublante MK.W
                case (RegularItem)2257: // Bombe infectée MK.W
                case (RegularItem)2258: // Bombe Virus MK.W
                case (RegularItem)2259: // Bombe chaotique MK.W
                case (RegularItem)2260: // Bombe Anima MK.W
                case (RegularItem)2321: // Roc de vent
                case (RegularItem)2322: // Globe de vent
                case (RegularItem)2323: // Cristal de vent
                case (RegularItem)2326: // Magikoroc de vent
                case (RegularItem)2327: // Megakoroc de vent
                {
                    _v.NormalMagicParams();
                    _v.Command.Element = EffectElement.Wind;
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
                    if ((_v.Target.WeakElement & EffectElement.Wind) != 0)
                        _v.Target.TryAlterStatuses(_v.Command.ItemStatus, false, _v.Caster);
                    return;
                }
                case (RegularItem)2261: // Bombe toxique MK.D
                case (RegularItem)2262: // Bombe silence MK.D
                case (RegularItem)2263: // Bombe aveuglante MK.D
                case (RegularItem)2264: // Bombe pétrifiante MK.D
                case (RegularItem)2265: // Bombe troublante MK.D
                case (RegularItem)2266: // Bombe infectée MK.D
                case (RegularItem)2267: // Bombe Virus MK.D
                case (RegularItem)2268: // Bombe chaotique MK.D
                case (RegularItem)2269: // Bombe Anima MK.D
                case (RegularItem)2328: // Roc de ténèbres
                case (RegularItem)2329: // Globe de ténèbres
                case (RegularItem)2330: // Cristal de ténèbres
                case (RegularItem)2333: // Magikoroc de ténèbres
                case (RegularItem)2334: // Megakoroc de ténèbres
                {
                    _v.NormalMagicParams();
                    _v.Command.Element = EffectElement.Darkness;
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
                    if ((_v.Target.WeakElement & EffectElement.Darkness) != 0)
                        _v.Target.TryAlterStatuses(_v.Command.ItemStatus, false, _v.Caster);
                    return;
                }
                case (RegularItem)2270: // Bombe toxique MK.L
                case (RegularItem)2271: // Bombe silence MK.L
                case (RegularItem)2272: // Bombe aveuglante MK.L
                case (RegularItem)2273: // Bombe pétrifiante MK.L
                case (RegularItem)2274: // Bombe troublante MK.L
                case (RegularItem)2275: // Bombe infectée MK.L
                case (RegularItem)2276: // Bombe Virus MK.L
                case (RegularItem)2277: // Bombe chaotique MK.L
                case (RegularItem)2278: // Bombe Anima MK.L
                case (RegularItem)2335: // Roc de lumière
                case (RegularItem)2336: // Globe de lumière
                case (RegularItem)2337: // Cristal de lumière
                case (RegularItem)2340: // Magikoroc de lumière
                case (RegularItem)2341: // Megakoroc de lumière
                {
                    _v.NormalMagicParams();
                    _v.Command.Element = EffectElement.Holy;
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
                    if ((_v.Target.WeakElement & EffectElement.Holy) != 0)
                        _v.Target.TryAlterStatuses(_v.Command.ItemStatus, false, _v.Caster);
                    return;
                }
                case (RegularItem)2466: // Bombe toxique MK.W
                case (RegularItem)2467: // Bombe silence MK.W
                case (RegularItem)2468: // Bombe aveuglante MK.W
                case (RegularItem)2469: // Bombe pétrifiante MK.W
                case (RegularItem)2470: // Bombe troublante MK.W
                case (RegularItem)2471: // Bombe infectée MK.W
                case (RegularItem)2472: // Bombe Virus MK.W
                case (RegularItem)2473: // Bombe chaotique MK.W
                case (RegularItem)2474: // Bombe Anima MK.W
                {
                    _v.NormalMagicParams();
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

                    int statusrate = 0;

                    BattleStatusId[] statuslist = { BattleStatusId.Protect, BattleStatusId.Shell, BattleStatusId.Regen, BattleStatusId.AutoLife, BattleStatusId.Trance,
                    BattleStatusId.Reflect, BattleStatusId.Haste, BattleStatusId.Vanish, BattleStatusId.Float, TranceSeekStatusId.ArmorUp,
                    TranceSeekStatusId.MagicUp, TranceSeekStatusId.MentalUp, TranceSeekStatusId.PowerUp};

                    for (Int32 i = 0; i < statuslist.Length; i++)
                    {
                        if ((statuslist[i].ToBattleStatus() & _v.Target.CurrentStatus) == 0)
                        {
                            statusrate += 25;
                        }
                    }
                    if ((GameRandom.Next8() % 100) < statusrate)
                        _v.Target.TryAlterStatuses(_v.Command.ItemStatus, false, _v.Caster);
                        
                    return;
                }
                case (RegularItem)2487: // Big Bang
                case (RegularItem)2489: // Super Nova
                {
                    uint CasterMaxDamageLimit = _v.Caster.MaxDamageLimit;
                    _v.Caster.MaxDamageLimit = 99999;
                    _v.NormalMagicParams();
                    TranceSeekAPI.CasterPenaltyMini(_v);
                    TranceSeekAPI.EnemyTranceBonusAttack(_v);
                    TranceSeekAPI.PenaltyShellAttack(_v);
                    TranceSeekAPI.PenaltyCommandDividedAttack(_v);
                    TranceSeekAPI.BonusElement(_v);
                    if (TranceSeekAPI.CanAttackMagic(_v))
                    {
                        _v.CalcHpDamage();
                    }
                    _v.Caster.AddDelayedModifier(
                        caster => caster.CurrentAtb >= caster.MaximumAtb,
                        caster =>
                        {
                            _v.Caster.MaxDamageLimit = CasterMaxDamageLimit;
                        }
                    );
                    return;
                }
                case (RegularItem)2488: // Bombe à Proton
                {
                    uint CasterMaxDamageLimit = _v.Caster.MaxDamageLimit;
                    _v.Caster.MaxDamageLimit = 99999;
                    _v.NormalMagicParams();
                    if (TranceSeekAPI.CanAttackMagic(_v))
                    {
                        HPDamage = 19998;
                    }
                    _v.Caster.AddDelayedModifier(
                        caster => caster.CurrentAtb >= caster.MaximumAtb,
                        caster =>
                        {
                            _v.Caster.MaxDamageLimit = CasterMaxDamageLimit;
                        }
                    );
                    break;
                }
            }

            if (HPDamage > 0)
                _v.Target.Flags |= CalcFlag.HpAlteration;

            if (MPDamage > 0)
                _v.Target.Flags |= CalcFlag.MpAlteration;

            if ((_v.Target.Flags & CalcFlag.HpAlteration) != 0)
                _v.Target.HpDamage = HPDamage;

            if ((_v.Target.Flags & CalcFlag.MpAlteration) != 0)
                _v.Target.MpDamage = MPDamage;

            foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(_v.Caster))
                saFeature.TriggerOnAbility(_v, "CalcDamage", false);
            foreach (SupportingAbilityFeature saFeature in ff9abil.GetEnabledSA(_v.Target))
                saFeature.TriggerOnAbility(_v, "CalcDamage", true);
        }

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
