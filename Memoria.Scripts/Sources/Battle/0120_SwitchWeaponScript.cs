using System;
using System.Collections.Generic;
using System.Linq;
using FF9;
using Memoria.Data;
using UnityEngine;

namespace Memoria.Scripts.Battle
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
                if (!Dagger_Sword_DB.ContainsKey(_v.Caster.Weapon) && !Dagger_Sword_DB.ContainsValue(_v.Caster.Weapon))
                {
                    _v.Context.Flags |= BattleCalcFlags.Miss;
                    return;
                }

                string ModelZidane = null;
                string ModelTranceZidane = null;
                Vector3 ModelStatusScaleOld = _v.Caster.ModelStatusScale;
                _v.Caster.ModelStatusScale += new Vector3(0.1f, 0.1f, 0.1f); // To force reset stack status.
                if (btl_util.getSerialNumber(_v.Caster.Data) == CharacterSerialNumber.ZIDANE_DAGGER)
                {
                    ModelZidane = "GEO_MAIN_B0_001"; // Model Zidane_Sword
                    ModelTranceZidane = "GEO_MAIN_B0_023";
                }
                else
                {
                    ModelZidane = "GEO_MAIN_B0_000"; // Model Zidane_Dagger
                    ModelTranceZidane = "GEO_MAIN_B0_022";
                }

                Vector3 position = _v.Caster.Data.gameObject.transform.localPosition;
                _v.Caster.Data.gameObject.SetActive(false);
                _v.Caster.Data.weapon_geo.SetActive(false);
                CharacterBattleParameter btlParam = btl_mot.BattleParameterList[ModelZidane == "GEO_MAIN_B0_001" ? CharacterSerialNumber.ZIDANE_SWORD : CharacterSerialNumber.ZIDANE_DAGGER];
                FF9StateSystem.Common.FF9.player[CharacterId.Zidane].info.serial_no = ModelZidane == "GEO_MAIN_B0_001" ? CharacterSerialNumber.ZIDANE_SWORD : CharacterSerialNumber.ZIDANE_DAGGER;
                if (_v.Caster.IsUnderAnyStatus(BattleStatus.Trance))
                {
                    _v.Caster.Data.gameObject = ModelFactory.CreateModel(ModelTranceZidane, true);
                    _v.Caster.Data.originalGo = ModelFactory.CreateModel(ModelZidane, true);
                    RefreshTranceModel(_v.Caster.Data);
                    _v.Caster.Data.originalGo.SetActive(false);
                }
                else
                {
                    _v.Caster.Data.gameObject = ModelFactory.CreateModel(ModelZidane, true);
                }
                RegularItem weaponchoose = ff9item._FF9Item_Data[_v.Caster.Weapon].shape == 1 ? Dagger_Sword_DB[_v.Caster.Weapon] : Dagger_Sword_DB.FirstOrDefault(Dagger => Dagger.Value == _v.Caster.Weapon).Key;
                if (weaponchoose < 0)
                {
                    weaponchoose = btl_util.getWeaponNumber(_v.Caster.Data);
                    _v.Context.Flags |= BattleCalcFlags.Miss;
                }
                else
                {
                    FF9StateSystem.Common.FF9.player[(CharacterId)_v.Caster.Data.bi.slot_no].equip[0] = weaponchoose;
                }
                _v.Caster.Data.weapon = ff9item.GetItemWeapon(weaponchoose);
                for (Int32 i = 0; i < 34; i++)
                    _v.Caster.Data.mot[i] = btlParam.AnimationId[i];
                _v.Caster.Data.gameObject.transform.localPosition = position;
                _v.Caster.Data.dms_geo_id = btl_init.GetModelID(btl_util.getSerialNumber(_v.Caster.Data));
                _v.Caster.Data.gameObject.SetActive(true);
                _v.Caster.Data.weapon_geo.SetActive(true);
                btl_eqp.InitWeapon(FF9StateSystem.Common.FF9.player[CharacterId.Zidane], _v.Caster.Data);
                btl_mot.setMotion(_v.Caster.Data, BattlePlayerCharacter.PlayerMotionIndex.MP_WIN); //MP_MAGIC
                _v.Caster.Data.evt.animFrame = 0;
                geo.geoScaleUpdate(_v.Caster.Data, true);
                TranceSeekAPI.ZidanePassive[_v.Caster.Data][4] = 0;
                TranceSeekAPI.ZidanePassive[_v.Caster.Data][9] = 0;

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

        public static Dictionary<RegularItem, RegularItem> Dagger_Sword_DB = new Dictionary<RegularItem, RegularItem>
        {
            { RegularItem.Gladius, TranceSeekRegularItem.GladiusSword },
            { RegularItem.ZorlinShape, TranceSeekRegularItem.ZorlinSword },
            { RegularItem.Orichalcon, TranceSeekRegularItem.OrichalconSword },
            { TranceSeekRegularItem.ButterflyDagger, RegularItem.ButterflySword },
            { TranceSeekRegularItem.TheOgreDagger, RegularItem.TheOgre },
            { TranceSeekRegularItem.ExplodaDagger, RegularItem.Exploda },
            { TranceSeekRegularItem.RuneToothDagger, RegularItem.RuneTooth },
            { TranceSeekRegularItem.AngelBlessDagger, RegularItem.AngelBless },
            { TranceSeekRegularItem.SargatanasDagger, RegularItem.Sargatanas },
            { TranceSeekRegularItem.MasamuneDagger, RegularItem.Masamune },
            { TranceSeekRegularItem.AssassinDagger, TranceSeekRegularItem.AssassinSword },
            { TranceSeekRegularItem.TheTowerDagger, RegularItem.TheTower },
            { TranceSeekRegularItem.UltimaWeaponDagger, RegularItem.UltimaWeapon }
        };
    }
}
