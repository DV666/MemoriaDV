using System;
using System.Collections.Generic;
using FF9;
using Memoria.Data;
using UnityEngine;

namespace Memoria.Scripts.Battle
{
    [BattleScript(Id)]
    public sealed class SummonMonsterScript : IBattleScript
    {
        public const Int32 Id = 0140;

        private readonly BattleCalculator _v;

        public static Dictionary<BTL_DATA, Int32> SummonStep = new Dictionary<BTL_DATA, Int32>();
        public static Dictionary<BTL_DATA, Vector3> InitPosition = new Dictionary<BTL_DATA, Vector3>();
        public static Dictionary<BTL_DATA, Int32> NumberTargets = new Dictionary<BTL_DATA, Int32>();

        public SummonMonsterScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (!SummonStep.TryGetValue(_v.Caster.Data, out Int32 IDStep)) // Init
                SummonStep[_v.Caster.Data] = 0;
            if (!InitPosition.TryGetValue(_v.Caster.Data, out Vector3 InitPos)) // Init
                InitPosition[_v.Caster.Data] = Vector3.zero;

            if (SummonStep[_v.Caster.Data] == 0) // Change into the monster
            {
                short IDMonster = GeoMonsterWithAA[(Int32)_v.Command.AbilityId];
                String geoName = FF9BattleDB.GEO.GetValue(IDMonster);
                InitPosition[_v.Caster.Data] = _v.Caster.Data.gameObject.transform.localPosition;
                _v.Caster.Data.weapon_geo.SetActive(false);
                _v.Caster.Data.flags |= geo.GEO_FLAGS_CLIP;
                _v.Caster.Data.ChangeModel(ModelFactory.CreateModel(geoName, true, true, Configuration.Graphics.ElementsSmoothTexture), IDMonster);
                _v.Caster.Data.gameObject.transform.localPosition = InitPosition[_v.Caster.Data];
                geoName = geoName.Substring(4);
                if (IDMonster == 244) // Cactuar (need to use idle_alternate)
                {
                    Animation animation = _v.Caster.Data.gameObject.GetComponent<Animation>();
                    if (animation.GetClip("ANH_" + geoName + "_001") == null)
                        AnimationFactory.AddAnimWithAnimatioName(_v.Caster.Data.gameObject, "ANH_" + geoName + "_001");
                    for (Int32 i = 0; i < 34; i++)
                        _v.Caster.Data.mot[i] = "ANH_" + geoName + "_001";
                }
                else
                {
                    Animation animation = _v.Caster.Data.gameObject.GetComponent<Animation>();
                    if (animation.GetClip("ANH_" + geoName + "_000") == null)
                        AnimationFactory.AddAnimWithAnimatioName(_v.Caster.Data.gameObject, "ANH_" + geoName + "_000");
                    for (Int32 i = 0; i < 34; i++)
                        _v.Caster.Data.mot[i] = "ANH_" + geoName + "_000";
                }          
                btl_mot.setMotion(_v.Caster.Data, BattlePlayerCharacter.PlayerMotionIndex.MP_IDLE_NORMAL);
                NumberTargets[_v.Caster.Data] = _v.Command.TargetCount;
                SummonStep[_v.Caster.Data] = 1;
            }
            else if (SummonStep[_v.Caster.Data] == 1) // Script for the damage
            {
                switch (SFX.currentEffectID)
                {
                    case SpecialEffect.Thousand_Needles:
                    {
                        _v.Caster.MaxDamageLimit = 10000;
                        _v.Target.Flags |= CalcFlag.HpAlteration;
                        _v.Target.HpDamage = 10000;                      
                        break;
                    }
                    case SpecialEffect.Thundaga__Single:
                    {
                        _v.Command.Power = 72;
                        _v.Command.Element = EffectElement.Thunder;
                        _v.NormalMagicParams();
                        TranceSeekCustomAPI.CharacterBonusPassive(_v, "MagicAttack");
                        TranceSeekCustomAPI.CasterPenaltyMini(_v);
                        TranceSeekCustomAPI.EnemyTranceBonusAttack(_v);
                        TranceSeekCustomAPI.PenaltyShellAttack(_v);
                        TranceSeekCustomAPI.PenaltyCommandDividedAttack(_v);
                        TranceSeekCustomAPI.BonusElement(_v);
                        if (TranceSeekCustomAPI.CanAttackMagic(_v))
                        {
                            if (_v.Caster.HasSupportAbilityByIndex((SupportAbility)102))
                                TranceSeekCustomAPI.TryCriticalHit(_v);
                            _v.CalcHpDamage();
                            TranceSeekCustomAPI.RaiseTrouble(_v);
                        }
                        _v.TryAlterMagicStatuses();
                        break;
                    }
                    case SpecialEffect.Meteor__Success:
                    {
                        _v.Command.Power = 115;
                        _v.Context.Attack = GameRandom.Next16() % (_v.Caster.Magic + 99);
                        _v.SetCommandPower();
                        _v.Context.DefensePower = 0;
                        TranceSeekCustomAPI.CasterPenaltyMini(_v);
                        TranceSeekCustomAPI.EnemyTranceBonusAttack(_v);
                        TranceSeekCustomAPI.PenaltyShellAttack(_v);
                        TranceSeekCustomAPI.PenaltyCommandDividedAttack(_v);
                        TranceSeekCustomAPI.BonusElement(_v);
                        if (TranceSeekCustomAPI.CanAttackMagic(_v))
                        {
                            _v.CalcHpDamage();
                        }
                        _v.TryAlterMagicStatuses();
                        break;
                    }
                }

                NumberTargets[_v.Caster.Data]--;
                if (NumberTargets[_v.Caster.Data] <= 0)
                    SummonStep[_v.Caster.Data] = 2;
            }
            else if (SummonStep[_v.Caster.Data] == 2) // Turn back to the character
            {
                PLAYER player = _v.Caster.Player;
                BattlePlayerCharacter.PlayerMotionIndex motion = btl_mot.getMotion(_v.Caster);
                if (motion == BattlePlayerCharacter.PlayerMotionIndex.MP_MAX)
                    motion = BattlePlayerCharacter.PlayerMotionIndex.MP_IDLE_NORMAL;
                BattlePlayerCharacter.CreatePlayer(_v.Caster, player);
                btl_mot.SetPlayerDefMotion(_v.Caster, player.info.serial_no, (UInt32)_v.Caster.GetIndex());
                BattlePlayerCharacter.InitAnimation(_v.Caster);
                if (_v.Caster.IsUnderAnyStatus(BattleStatus.Trance))
                    _v.Caster.Data.ChangeModel(_v.Caster.Data.tranceGo, btl_init.GetModelID(player.info.serial_no, true));
                else
                    _v.Caster.Data.ChangeModel(_v.Caster.Data.originalGo, btl_init.GetModelID(player.info.serial_no, false));
                btl_mot.setMotion(_v.Caster, motion);
                _v.Caster.Data.gameObject.transform.localPosition = InitPosition[_v.Caster.Data];
                String modelName = _v.Caster.Data.weapon.ModelName;
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
                geo.geoAttach(_v.Caster.Data.weapon_geo, _v.Caster.Data.gameObject, _v.Caster.Data.weapon_bone);
                _v.Caster.Data.weapon_geo.SetActive(true);
                _v.Caster.Data.flags &= (ushort)~geo.GEO_FLAGS_CLIP;
                _v.Caster.MaxDamageLimit = 9999;
                SummonStep[_v.Caster.Data] = 0;
            }

        }

        public static Dictionary<Int32, short> GeoMonsterWithAA = new Dictionary<Int32, short>()
        {
            { 1171, 328 }, // Agares
            { 1243, 244 }, // Cactuar
            { 1244, 1 }, // Ozma
        };
    }
}

