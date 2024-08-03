using System;
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
                switch (_v.Caster.Weapon)
                {
                    case RegularItem.Dagger:
                    case RegularItem.MageMasher:
                    case RegularItem.MythrilDagger:
                        _v.Context.Flags |= BattleCalcFlags.Miss;
                        return;
                    default:
                        break;
                }

                string ModelZidane = null;
                string ModelTranceZidane = null;
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
                RegularItem weaponchoose = (btl_util.getWeaponNumber(_v.Caster.Data) > (RegularItem)1000) ? (btl_util.getWeaponNumber(_v.Caster.Data) - 1000) : (btl_util.getWeaponNumber(_v.Caster.Data) + 1000);
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
                String modelName = _v.Caster.Data.weapon.ModelName;
                SFX.InitBattleParty();
                _v.Caster.Data.weapon_geo = ModelFactory.CreateModel("BattleMap/BattleModel/battle_weapon/" + modelName + "/" + modelName, true);
                MeshRenderer[] componentsInChildren = _v.Caster.Data.weapon_geo.GetComponentsInChildren<MeshRenderer>();
                _v.Caster.Data.weaponMeshCount = componentsInChildren.Length;
                _v.Caster.Data.weaponRenderer = new Renderer[_v.Caster.Data.weaponMeshCount];
                for (Int32 i = 0; i < _v.Caster.Data.weaponMeshCount; i++)
                {
                    _v.Caster.Data.weaponRenderer[i] = componentsInChildren[i].GetComponent<Renderer>();
                    if (_v.Caster.Data.weapon.CustomTexture.Length > 0)
                    {
                        if (!String.IsNullOrEmpty(_v.Caster.Data.weapon.CustomTexture[i]))
                        {
                            _v.Caster.Data.weaponRenderer[i].material.mainTexture = AssetManager.Load<Texture2D>(_v.Caster.Data.weapon.CustomTexture[i], false);
                        }
                    }
                }
                btl_util.SetBBGColor(_v.Caster.Data.weapon_geo);
                geo.geoAttach(_v.Caster.Data.weapon_geo, _v.Caster.Data.gameObject, 13);
                for (Int32 i = 0; i < 34; i++)
                    _v.Caster.Data.mot[i] = btlParam.AnimationId[i];
                _v.Caster.Data.gameObject.SetActive(true);
                _v.Caster.Data.weapon_geo.SetActive(true);
                btl_mot.setMotion(_v.Caster.Data, BattlePlayerCharacter.PlayerMotionIndex.MP_MAGIC);
                _v.Caster.Data.evt.animFrame = 0;
                geo.geoScaleUpdate(_v.Caster.Data, true);
                _v.Caster.AddDelayedModifier(
                    caster => caster.CurrentAtb >= caster.MaximumAtb,
                    caster =>
                    {
                        if (!caster.IsUnderAnyStatus(BattleStatusConst.StopAtb) && caster.CurrentAtb < (4 * caster.MaximumAtb / 5))
                            caster.CurrentAtb += (Int16)(4 * caster.MaximumAtb / 5);
                        caster.RemoveStatus(BattleStatus.Vanish);
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
    }
}
