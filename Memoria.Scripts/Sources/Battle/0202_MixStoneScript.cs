using System;
using System.Collections.Generic;
using FF9;
using Memoria.Data;
using Memoria.Prime;
using static SiliconStudio.Social.ResponseData;

namespace Memoria.Scripts.TranceSeek
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
                case TranceSeekRegularItem.Bomb: // Bombe
                case TranceSeekRegularItem.SuperBomb: // Super Bombe
                case TranceSeekRegularItem.GigaBomb: // Giga Bombe
                case TranceSeekRegularItem.MegaBomb: // Méga Bombe
                case TranceSeekRegularItem.NeutronBomb: // Bombe à Neutron
                case TranceSeekRegularItem.ToxicBomb: // Bombe toxique
                case TranceSeekRegularItem.SilenceBomb: // Bombe silence
                case TranceSeekRegularItem.FlashyBomb: // Bombe aveuglante
                case TranceSeekRegularItem.PetrifyingBomb: // Bombe pétrifiante
                case TranceSeekRegularItem.TroubleBomb: // Bombe troublante
                case TranceSeekRegularItem.InfectedBomb: // Bombe infectée
                case TranceSeekRegularItem.VirusBomb: // Bombe Virus
                case TranceSeekRegularItem.ChaoticBomb: // Bombe chaotique
                case TranceSeekRegularItem.AnimaBomb: // Bombe Anima
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
                case TranceSeekRegularItem.UltraBombG: // Ultra Bombe G
                case TranceSeekRegularItem.MegaBombG: // Mega Bombe G
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
                case TranceSeekRegularItem.ToxicBombMKT: // Bombe toxique MK.T
                case TranceSeekRegularItem.SilenceBombMKT: // Bombe silence MK.T
                case TranceSeekRegularItem.FlashyBombMKT: // Bombe aveuglante MK.T
                case TranceSeekRegularItem.PetrifyingBombMKT: // Bombe pétrifiante MK.T
                case TranceSeekRegularItem.TroubleBombMKT: // Bombe troublante MK.T
                case TranceSeekRegularItem.InfectedBombMKT: // Bombe infectée MK.T
                case TranceSeekRegularItem.VirusBombMKT: // Bombe Virus MK.T
                case TranceSeekRegularItem.ChaoticBombMKT: // Bombe chaotique MK.T
                case TranceSeekRegularItem.AnimaBombMKT: // Bombe Anima MK.T
                case TranceSeekRegularItem.LightningStone: // Roc de foudre
                case TranceSeekRegularItem.LightningOrb: // Globe de foudre
                case TranceSeekRegularItem.LightningCrystal: // Cristal de foudre
                case TranceSeekRegularItem.LightningGem: // Magikoroc de foudre
                case TranceSeekRegularItem.LightningMegagem: // Megakoroc de foudre
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
                case TranceSeekRegularItem.ToxicBombMKF: // Bombe toxique MK.F
                case TranceSeekRegularItem.SilenceBombMKF: // Bombe silence MK.F
                case TranceSeekRegularItem.FlashyBombMKF: // Bombe aveuglante MK.F
                case TranceSeekRegularItem.PetrifyingBombMKF: // Bombe pétrifiante MK.F
                case TranceSeekRegularItem.TroubleBombMKF: // Bombe troublante MK.F
                case TranceSeekRegularItem.InfectedBombMKF: // Bombe infectée MK.F
                case TranceSeekRegularItem.VirusBombMKF: // Bombe Virus MK.F
                case TranceSeekRegularItem.ChaoticBombMKF: // Bombe chaotique MK.F
                case TranceSeekRegularItem.AnimaBombMKF: // Bombe Anima MK.F
                case TranceSeekRegularItem.FireStone: // Roc de feu
                case TranceSeekRegularItem.FireOrb: // Globe de feu
                case TranceSeekRegularItem.FireCrystal: // Cristal de feu
                case TranceSeekRegularItem.FireGem: // Magikoroc de feu
                case TranceSeekRegularItem.FireMegagem: // Megakoroc de feu
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
                case TranceSeekRegularItem.ToxicBombMKI: // Bombe toxique MK.I
                case TranceSeekRegularItem.SilenceBombMKI: // Bombe silence MK.I
                case TranceSeekRegularItem.FlashyBombMKI: // Bombe aveuglante MK.I
                case TranceSeekRegularItem.PetrifyingBombMKI: // Bombe pétrifiante MK.I
                case TranceSeekRegularItem.TroubleBombMKI: // Bombe troublante MK.I
                case TranceSeekRegularItem.InfectedBombMKI: // Bombe infectée MK.I
                case TranceSeekRegularItem.VirusBombMKI: // Bombe Virus MK.I
                case TranceSeekRegularItem.ChaoticBombMKI: // Bombe chaotique MK.I
                case TranceSeekRegularItem.AnimaBombMKI: // Bombe Anima MK.I
                case TranceSeekRegularItem.IceStone: // Roc de glace
                case TranceSeekRegularItem.IceOrb: // Globe de glace
                case TranceSeekRegularItem.IceCrystal: // Cristal de glace
                case TranceSeekRegularItem.IceGem: // Magikoroc de glace
                case TranceSeekRegularItem.IceMegagem: // Megakoroc de glace
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
                case TranceSeekRegularItem.ToxicBombMKG: // Bombe toxique MK.G
                case TranceSeekRegularItem.SilenceBombMKG: // Bombe silence MK.G
                case TranceSeekRegularItem.FlashyBombMKG: // Bombe aveuglante MK.G
                case TranceSeekRegularItem.PetrifyingBombMKG: // Bombe pétrifiante MK.G
                case TranceSeekRegularItem.TroubleBombMKG: // Bombe troublante MK.G
                case TranceSeekRegularItem.InfectedBombMKG: // Bombe infectée MK.G
                case TranceSeekRegularItem.VirusBombMKG: // Bombe Virus MK.G
                case TranceSeekRegularItem.ChaoticBombMKG: // Bombe chaotique MK.G
                case TranceSeekRegularItem.AnimaBombMKG: // Bombe Anima MK.G
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
                case TranceSeekRegularItem.ToxicBombMKE: // Bombe toxique MK.E
                case TranceSeekRegularItem.SilenceBombMKE: // Bombe silence MK.E
                case TranceSeekRegularItem.FlashyBombMKE: // Bombe aveuglante MK.E
                case TranceSeekRegularItem.PetrifyingBombMKE: // Bombe pétrifiante MK.E
                case TranceSeekRegularItem.TroubleBombMKE: // Bombe troublante MK.E
                case TranceSeekRegularItem.InfectedBombMKE: // Bombe infectée MK.E
                case TranceSeekRegularItem.VirusBombMKE: // Bombe Virus MK.E
                case TranceSeekRegularItem.ChaoticBombMKE: // Bombe chaotique MK.E
                case TranceSeekRegularItem.AnimaBombMKE: // Bombe Anima MK.E
                case TranceSeekRegularItem.WaterStone: // Roc d'eau
                case TranceSeekRegularItem.WaterOrb: // Globe d'eau
                case TranceSeekRegularItem.WaterCrystal: // Cristal d'eau
                case TranceSeekRegularItem.WaterGem: // Magikoroc d'eau
                case TranceSeekRegularItem.WaterMegagem: // Megakoroc d'eau
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
                case TranceSeekRegularItem.ToxicBombMKA: // Bombe toxique MK.A
                case TranceSeekRegularItem.SilenceBombMKA: // Bombe silence MK.A
                case TranceSeekRegularItem.FlashyBombMKA: // Bombe aveuglante MK.A
                case TranceSeekRegularItem.PetrifyingBombMKA: // Bombe pétrifiante MK.A
                case TranceSeekRegularItem.TroubleBombMKA: // Bombe troublante MK.A
                case TranceSeekRegularItem.InfectedBombMKA: // Bombe infectée MK.A
                case TranceSeekRegularItem.VirusBombMKA: // Bombe Virus MK.A
                case TranceSeekRegularItem.ChaoticBombMKA: // Bombe chaotique MK.A
                case TranceSeekRegularItem.AnimaBombMKA: // Bombe Anima MK.A
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
                case TranceSeekRegularItem.ToxicBombMKQ: // Bombe toxique MK.Q
                case TranceSeekRegularItem.SilenceBombMKQ: // Bombe silence MK.Q
                case TranceSeekRegularItem.FlashyBombMKQ: // Bombe aveuglante MK.Q
                case TranceSeekRegularItem.PetrifyingBombMKQ: // Bombe pétrifiante MK.Q
                case TranceSeekRegularItem.TroubleBombMKQ: // Bombe troublante MK.Q
                case TranceSeekRegularItem.InfectedBombMKQ: // Bombe infectée MK.Q
                case TranceSeekRegularItem.VirusBombMKQ: // Bombe Virus MK.Q
                case TranceSeekRegularItem.ChaoticBombMKQ: // Bombe chaotique MK.Q
                case TranceSeekRegularItem.AnimaBombMKQ: // Bombe Anima MK.Q
                case TranceSeekRegularItem.EarthStone: // Roc de terre
                case TranceSeekRegularItem.EarthOrb: // Globe de terre
                case TranceSeekRegularItem.EarthCrystal: // Cristal de terre
                case TranceSeekRegularItem.EarthGem: // Magikoroc de terre
                case TranceSeekRegularItem.EarthMegagem: // Megakoroc de terre
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
                case TranceSeekRegularItem.ToxicBombMKR: // Bombe toxique MK.R
                case TranceSeekRegularItem.SilenceBombMKR: // Bombe silence MK.R
                case TranceSeekRegularItem.FlashyBombMKR: // Bombe aveuglante MK.R
                case TranceSeekRegularItem.PetrifyingBombMKR: // Bombe pétrifiante MK.R
                case TranceSeekRegularItem.TroubleBombMKR: // Bombe troublante MK.R
                case TranceSeekRegularItem.InfectedBombMKR: // Bombe infectée MK.R
                case TranceSeekRegularItem.VirusBombMKR: // Bombe Virus MK.R
                case TranceSeekRegularItem.ChaoticBombMKR: // Bombe chaotique MK.R
                case TranceSeekRegularItem.AnimaBombMKR: // Bombe Anima MK.R
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
                case TranceSeekRegularItem.ToxicBombMKW: // Bombe toxique MK.W
                case TranceSeekRegularItem.SilenceBombMKW: // Bombe silence MK.W
                case TranceSeekRegularItem.FlashyBombMKW: // Bombe aveuglante MK.W
                case TranceSeekRegularItem.PetrifyingBombMKW: // Bombe pétrifiante MK.W
                case TranceSeekRegularItem.TroubleBombMKW: // Bombe troublante MK.W
                case TranceSeekRegularItem.InfectedBombMKW: // Bombe infectée MK.W
                case TranceSeekRegularItem.VirusBombMKW: // Bombe Virus MK.W
                case TranceSeekRegularItem.ChaoticBombMKW: // Bombe chaotique MK.W
                case TranceSeekRegularItem.AnimaBombMKW: // Bombe Anima MK.W
                case TranceSeekRegularItem.WindStone: // Roc de vent
                case TranceSeekRegularItem.WindOrb: // Globe de vent
                case TranceSeekRegularItem.WindCrystal: // Cristal de vent
                case TranceSeekRegularItem.WindGem: // Magikoroc de vent
                case TranceSeekRegularItem.WindMegagem: // Megakoroc de vent
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
                case TranceSeekRegularItem.ToxicBombMKD: // Bombe toxique MK.D
                case TranceSeekRegularItem.SilenceBombMKD: // Bombe silence MK.D
                case TranceSeekRegularItem.FlashyBombMKD: // Bombe aveuglante MK.D
                case TranceSeekRegularItem.PetrifyingBombMKD: // Bombe pétrifiante MK.D
                case TranceSeekRegularItem.TroubleBombMKD: // Bombe troublante MK.D
                case TranceSeekRegularItem.InfectedBombMKD: // Bombe infectée MK.D
                case TranceSeekRegularItem.VirusBombMKD: // Bombe Virus MK.D
                case TranceSeekRegularItem.ChaoticBombMKD: // Bombe chaotique MK.D
                case TranceSeekRegularItem.AnimaBombMKD: // Bombe Anima MK.D
                case TranceSeekRegularItem.DarkStone: // Roc de ténèbres
                case TranceSeekRegularItem.DarkOrb: // Globe de ténèbres
                case TranceSeekRegularItem.DarkCrystal: // Cristal de ténèbres
                case TranceSeekRegularItem.DarkGem: // Magikoroc de ténèbres
                case TranceSeekRegularItem.DarkMegagem: // Megakoroc de ténèbres
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
                case TranceSeekRegularItem.ToxicBombMKL: // Bombe toxique MK.L
                case TranceSeekRegularItem.SilenceBombMKL: // Bombe silence MK.L
                case TranceSeekRegularItem.FlashyBombMKL: // Bombe aveuglante MK.L
                case TranceSeekRegularItem.PetrifyingBombMKL: // Bombe pétrifiante MK.L
                case TranceSeekRegularItem.TroubleBombMKL: // Bombe troublante MK.L
                case TranceSeekRegularItem.InfectedBombMKL: // Bombe infectée MK.L
                case TranceSeekRegularItem.VirusBombMKL: // Bombe Virus MK.L
                case TranceSeekRegularItem.ChaoticBombMKL: // Bombe chaotique MK.L
                case TranceSeekRegularItem.AnimaBombMKL: // Bombe Anima MK.L
                case TranceSeekRegularItem.HolyStone: // Roc de lumière
                case TranceSeekRegularItem.HolyOrb: // Globe de lumière
                case TranceSeekRegularItem.HolyCrystal: // Cristal de lumière
                case TranceSeekRegularItem.HolyGem: // Magikoroc de lumière
                case TranceSeekRegularItem.HolyMegagem: // Megakoroc de lumière
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
                case TranceSeekRegularItem.ToxicBombMKW_2: // Bombe toxique MK.W
                case TranceSeekRegularItem.SilenceBombMKW_2: // Bombe silence MK.W
                case TranceSeekRegularItem.FlashyBombMKW_2: // Bombe aveuglante MK.W
                case TranceSeekRegularItem.PetrifyingBombMKW_2: // Bombe pétrifiante MK.W
                case TranceSeekRegularItem.TroubleBombMKW_2: // Bombe troublante MK.W
                case TranceSeekRegularItem.InfectedBombMKW_2: // Bombe infectée MK.W
                case TranceSeekRegularItem.VirusBombMKW_2: // Bombe Virus MK.W
                case TranceSeekRegularItem.ChaoticBombMKW_2: // Bombe chaotique MK.W
                case TranceSeekRegularItem.AnimaBombMKW_2: // Bombe Anima MK.W
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
                case TranceSeekRegularItem.BigBang: // Big Bang
                case TranceSeekRegularItem.SuperNova: // Super Nova
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
                case TranceSeekRegularItem.ProtonBomb: // Bombe à Proton
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

