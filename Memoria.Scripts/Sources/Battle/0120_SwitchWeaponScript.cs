using FF9;
using Memoria.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SiliconStudio.Social.ResponseData;

namespace Memoria.Scripts.TranceSeek
{
    /// <summary>
    /// Special
    /// </summary>
    [BattleScript(Id)]
    public sealed class SwitchWeaponScript : IBattleScript
    {
        public const Int32 Id = 0120;

        private readonly BattleCalculator _v;

        public SwitchWeaponScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Caster.PlayerIndex == CharacterId.Zidane)
            {
                var Zidane_TSVar = _v.CasterState().Zidane;

                if (!Dagger_Sword_DB.TryGetValue(_v.Caster.Weapon, out RegularItem nextWeaponItem))
                {
                    _v.Context.Flags |= BattleCalcFlags.Miss;
                    return;
                }

                if (Zidane_TSVar.OriginalModel == null || Zidane_TSVar.AltModel == null ||
                    Zidane_TSVar.OriginalTranceModel == null || Zidane_TSVar.AltTranceModel == null)
                {
                    InitZidaneModel(_v.Caster);
                }

                string ModelZidane = null;
                Vector3 ModelStatusScaleOld = _v.Caster.ModelStatusScale;
                _v.Caster.ModelStatusScale += new Vector3(0.1f, 0.1f, 0.1f); // To force reset stack status.

                if (btl_util.getSerialNumber(_v.Caster.Data) == CharacterSerialNumber.ZIDANE_DAGGER)
                    ModelZidane = "GEO_MAIN_B0_001"; // Model Zidane_Sword
                else
                    ModelZidane = "GEO_MAIN_B0_000"; // Model Zidane_Dagger

                Boolean SwitchToAlt = true;
                if (_v.Caster.Data.gameObject == Zidane_TSVar.AltModel || _v.Caster.Data.gameObject == Zidane_TSVar.AltTranceModel)
                    SwitchToAlt = false;

                Vector3 position = _v.Caster.Data.gameObject.transform.localPosition;
                _v.Caster.Data.gameObject.SetActive(false);
                _v.Caster.Data.weapon_geo.SetActive(false);

                CharacterBattleParameter btlParam = btl_mot.BattleParameterList[ModelZidane == "GEO_MAIN_B0_001" ? CharacterSerialNumber.ZIDANE_SWORD : CharacterSerialNumber.ZIDANE_DAGGER];
                FF9StateSystem.Common.FF9.player[CharacterId.Zidane].info.serial_no = ModelZidane == "GEO_MAIN_B0_001" ? CharacterSerialNumber.ZIDANE_SWORD : CharacterSerialNumber.ZIDANE_DAGGER;

                if (_v.Caster.IsUnderAnyStatus(BattleStatus.Trance))
                {
                    _v.Caster.Data.gameObject = SwitchToAlt ? Zidane_TSVar.AltTranceModel : Zidane_TSVar.OriginalTranceModel;
                    _v.Caster.Data.originalGo = SwitchToAlt ? Zidane_TSVar.AltModel : Zidane_TSVar.OriginalModel;
                    RefreshTranceModel(_v.Caster.Data);
                    _v.Caster.Data.originalGo.SetActive(false);
                }
                else
                {
                    _v.Caster.Data.gameObject = SwitchToAlt ? Zidane_TSVar.AltModel : Zidane_TSVar.OriginalModel;
                }

                FF9StateSystem.Common.FF9.player[(CharacterId)_v.Caster.Data.bi.slot_no].equip[0] = nextWeaponItem;
                _v.Caster.Data.weapon = ff9item.GetItemWeapon(nextWeaponItem);
                btl_eqp.InitWeapon(FF9StateSystem.Common.FF9.player[CharacterId.Zidane], _v.Caster.Data);

                for (Int32 i = 0; i < 34; i++)
                    _v.Caster.Data.mot[i] = btlParam.AnimationId[i];

                _v.Caster.Data.gameObject.transform.localPosition = position;
                _v.Caster.Data.dms_geo_id = btl_init.GetModelID(btl_util.getSerialNumber(_v.Caster.Data));

                _v.Caster.Data.gameObject.SetActive(true);
                _v.Caster.Data.weapon_geo.SetActive(true);

                btl_mot.setMotion(_v.Caster.Data, BattlePlayerCharacter.PlayerMotionIndex.MP_WIN); //MP_MAGIC
                _v.Caster.Data.evt.animFrame = 0;
                geo.geoScaleUpdate(_v.Caster.Data, true);

                Zidane_TSVar.DaggerAttack = 0;
                Zidane_TSVar.Flexible = 0;

                _v.Caster.AddDelayedModifier(
                    caster => caster.CurrentAtb >= caster.MaximumAtb,
                    caster =>
                    {
                        if (!caster.IsUnderAnyStatus(BattleStatusConst.StopAtb) && caster.CurrentAtb < (4 * caster.MaximumAtb / 5))
                            caster.CurrentAtb += (Int16)(4 * caster.MaximumAtb / 5);
                        caster.RemoveStatus(BattleStatus.Vanish);
                        caster.ModelStatusScale = ModelStatusScaleOld;
                    }
                );
            }
        }

        public static void InitZidaneModel(BattleUnit unit)
        {
            var Zidane_TSVar = unit.State().Zidane;

            if (!Dagger_Sword_DB.ContainsKey(unit.Weapon))
            {
                Zidane_TSVar.OriginalModel = null;
                Zidane_TSVar.OriginalTranceModel = null;
                Zidane_TSVar.AltModel = null;
                Zidane_TSVar.AltTranceModel = null;
                return;
            }

            string ModelZidane;
            string ModelTranceZidane;
            Vector3 ZidanePosition = unit.Data.gameObject.transform.localPosition;

            if (btl_util.getSerialNumber(unit.Data) == CharacterSerialNumber.ZIDANE_DAGGER)
            {
                ModelZidane = "GEO_MAIN_B0_001"; // Model Zidane_Sword
                ModelTranceZidane = "GEO_MAIN_B0_023";
            }
            else
            {
                ModelZidane = "GEO_MAIN_B0_000"; // Model Zidane_Dagger
                ModelTranceZidane = "GEO_MAIN_B0_022";
            }

            Zidane_TSVar.OriginalModel = unit.Data.gameObject;
            Zidane_TSVar.OriginalTranceModel = unit.Data.tranceGo;

            Zidane_TSVar.AltModel = ModelFactory.CreateModel(ModelZidane, true);
            Zidane_TSVar.AltTranceModel = ModelFactory.CreateModel(ModelTranceZidane, true);

            Zidane_TSVar.AltModel.transform.localPosition = ZidanePosition;
            Zidane_TSVar.AltTranceModel.transform.localPosition = ZidanePosition;

            Zidane_TSVar.AltModel.SetActive(false);
            Zidane_TSVar.AltTranceModel.SetActive(false);
        }

        public static void RefreshTranceModel(BTL_DATA btl)
        {
            CharacterSerialNumber serialNo = btl_util.getSerialNumber(btl);
            btl.battleModelIsRendering = true;
            GeoTexAnim.geoTexAnimPlay(btl.tranceTexanimptr, 2);
            btl.meshCount = 0;
            foreach (System.Object obj in btl.gameObject.transform)
            {
                Transform transform = (Transform)obj;
                if (transform.name.Contains("mesh"))
                    btl.meshCount++;
            }
            btl.meshIsRendering = new Boolean[btl.meshCount];
            for (Int32 i = 0; i < btl.meshCount; i++)
                btl.meshIsRendering[i] = true;
            btl_util.GeoSetABR(btl.gameObject, "PSX/BattleMap_StatusEffect");
            BattlePlayerCharacter.InitAnimation(btl);
            AnimationFactory.AddAnimToGameObject(btl.gameObject, btl_mot.BattleParameterList[serialNo].ModelId, true);
        }

        public static readonly Dictionary<RegularItem, RegularItem> Dagger_Sword_DB = new Dictionary<RegularItem, RegularItem>
        {
            // Gladius
            { RegularItem.Gladius, TranceSeekRegularItem.GladiusSword },
            { TranceSeekRegularItem.GladiusSword, RegularItem.Gladius },
            
            // Zorlin Shape
            { RegularItem.ZorlinShape, TranceSeekRegularItem.ZorlinSword },
            { TranceSeekRegularItem.ZorlinSword, RegularItem.ZorlinShape },
            
            // Orichalcon
            { RegularItem.Orichalcon, TranceSeekRegularItem.OrichalconSword },
            { TranceSeekRegularItem.OrichalconSword, RegularItem.Orichalcon },

            // Butterfly
            { TranceSeekRegularItem.ButterflyDagger, RegularItem.ButterflySword },
            { RegularItem.ButterflySword, TranceSeekRegularItem.ButterflyDagger },

            // The Ogre
            { TranceSeekRegularItem.TheOgreDagger, RegularItem.TheOgre },
            { RegularItem.TheOgre, TranceSeekRegularItem.TheOgreDagger },

            // Exploda
            { TranceSeekRegularItem.ExplodaDagger, RegularItem.Exploda },
            { RegularItem.Exploda, TranceSeekRegularItem.ExplodaDagger },

            // Rune Tooth
            { TranceSeekRegularItem.RuneToothDagger, RegularItem.RuneTooth },
            { RegularItem.RuneTooth, TranceSeekRegularItem.RuneToothDagger },

            // Angel Bless
            { TranceSeekRegularItem.AngelBlessDagger, RegularItem.AngelBless },
            { RegularItem.AngelBless, TranceSeekRegularItem.AngelBlessDagger },

            // Sargatanas
            { TranceSeekRegularItem.SargatanasDagger, RegularItem.Sargatanas },
            { RegularItem.Sargatanas, TranceSeekRegularItem.SargatanasDagger },

            // Masamune
            { TranceSeekRegularItem.MasamuneDagger, RegularItem.Masamune },
            { RegularItem.Masamune, TranceSeekRegularItem.MasamuneDagger },

            // The Tower
            { TranceSeekRegularItem.TheTowerDagger, RegularItem.TheTower },
            { RegularItem.TheTower, TranceSeekRegularItem.TheTowerDagger },

            // Ultima Weapon
            { TranceSeekRegularItem.UltimaWeaponDagger, RegularItem.UltimaWeapon },
            { RegularItem.UltimaWeapon, TranceSeekRegularItem.UltimaWeaponDagger }
        };
    }
}
