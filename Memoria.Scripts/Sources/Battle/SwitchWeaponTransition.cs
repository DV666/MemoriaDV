using System;
using FF9;
using Memoria;
using Memoria.Data;
using Memoria.Prime;
using UnityEngine;

namespace Memoria.Scripts.TranceSeek
{
    internal static class SwitchWeaponTransition
    {
        internal static void SetFinalSerial(PLAYER_INFO playerInfo, CharacterSerialNumber finalFamily)
        {
            if (playerInfo.serial_no == CharacterSerialNumber.ZIDANE_DAGGER ||
                playerInfo.serial_no == CharacterSerialNumber.ZIDANE_SWORD)
                playerInfo.serial_no = finalFamily;
        }

        internal static void RefreshTranceModel(BTL_DATA btl, CharacterBattleParameter parameter)
        {
            btl.battleModelIsRendering = true;
            GeoTexAnim.geoTexAnimPlay(btl.tranceTexanimptr, 2);
            btl.meshCount = 0;
            foreach (Transform child in btl.gameObject.transform)
                if (child.name.Contains("mesh"))
                    btl.meshCount++;

            btl.meshIsRendering = new Boolean[btl.meshCount];
            for (Int32 i = 0; i < btl.meshCount; i++)
                btl.meshIsRendering[i] = true;

            btl_util.GeoSetABR(btl.gameObject, "PSX/BattleMap_StatusEffect");
            BattlePlayerCharacter.InitAnimation(btl);
            AnimationFactory.AddAnimToGameObject(btl.gameObject, parameter.ModelId, true);
        }

        internal static void RebuildWeapon(PLAYER player, BTL_DATA btl, CharacterBattleParameter parameter)
        {
            Boolean isTrance = btl_stat.CheckStatus(btl, BattleStatus.Trance);
            Boolean useTrancePlacement = isTrance && parameter.TranceParameters;
            Int32 oldBone = btl.weapon_bone;
            Int32 finalBone = btl.weapon.ModelId == UInt16.MaxValue
                ? -1
                : useTrancePlacement ? parameter.TranceWeaponBone : parameter.WeaponBone;

            // InitWeapon reads the final model ID and primary bone.
            btl.dms_geo_id = btl_init.GetModelID(player.info.serial_no, isTrance);
            btl.weapon_bone = finalBone;
            btl_eqp.InitWeapon(player, btl);

            BTL_DATA.WEAPON_MODEL primaryWeapon = btl.weaponModels[0];
            primaryWeapon.bone = finalBone;
            primaryWeapon.scale = (useTrancePlacement ? parameter.TranceWeaponSize : parameter.WeaponSize).ToVector3(true);
            primaryWeapon.offset_pos = (useTrancePlacement ? parameter.TranceWeaponOffsetPos : parameter.WeaponOffsetPos).ToVector3(false);
            primaryWeapon.offset_rot = parameter.GetWeaponRotationFixed(btl.weapon.ModelId, useTrancePlacement);

            if (oldBone != finalBone && primaryWeapon.geo != null && finalBone >= 0)
                geo.geoAttach(primaryWeapon.geo, btl.gameObject, finalBone);

            for (Int32 i = 1; i < btl.weaponModels.Count; i++)
            {
                BTL_DATA.WEAPON_MODEL secondWeapon = btl.weaponModels[i];
                secondWeapon.scale = Vector3.one;
                secondWeapon.offset_pos = Vector3.zero;
                secondWeapon.offset_rot = Vector3.zero;
            }
        }
    }
}
