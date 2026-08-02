using FF9;
using Memoria.Data;
using Memoria.Prime;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Memoria.Scripts.TranceSeek
{
    /// <summary>
    /// Regen, Shell, Protect, Haste, Reflect, Float, Carbuncle, Mighty Guard, Vanish, Auto-Life, Reis’s Wind, Luna, Aura, Defend
    /// </summary>
    [BattleScript(Id)]
    public sealed class MGSummonScript : IBattleScript
    {
        public const Int32 Id = 0165;

        private class SummonData
        {
            public String ModelName;
            public String BTLName;
            public Int32[] AnimIds;

            public SummonData(String modelName, String btlName, Int32[] animIds)
            {
                ModelName = modelName;
                BTLName = btlName;
                AnimIds = animIds;
            }
        }

        private static readonly Dictionary<Int32, SummonData> Summons = new Dictionary<Int32, SummonData>
        {
            { 1, new SummonData("GEO_MON_F9_ShivaMG", "Shiva", new Int32[] { 30013, 30013, 30013, 30013, 30014, 30014 }) },
            { 2, new SummonData("GEO_MON_F9_IfritMG", "Ifrit", new Int32[] { 30016, 30016, 30016, 30016, 30017, 30017 }) },
            { 3, new SummonData("GEO_MON_F9_RamuhMG", "Ramuh", new Int32[] { 30019, 30019, 30019, 30019, 30020, 30020 }) },
            { 4, new SummonData("GEO_MON_F9_LeviathanMG", "Leviathan", new Int32[] { 30022, 30022, 30022, 30022, 30023, 30023 }) },
            { 5, new SummonData("GEO_MON_F9_AsuraMG", "Asura", new Int32[] { 30025, 30025, 30025, 30025, 30028, 30028 }) },
            { 6, new SummonData("GEO_MON_F9_BahamutMG", "Bahamat", new Int32[] { 30038, 30038, 30038, 30038, 30039, 30039 }) }
        };

        private readonly BattleCalculator _v;

        public MGSummonScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Command.Power == 255 && _v.Command.HitRate == 255)
            {
                btl_util.SetEnemyFadeToPacket(_v.Target, 32);
                return;
            }   
            if (_v.Command.Data.info.effect_counter == 1)
            {
                BattleUnit btl = btl_scrp.FindBattleUnit(32);
                btl.Data.bi.target = 1;
            }
            else
            {
                _v.Target.Data.SetDisappear(false, 3);
                btl_mot.ShowMesh(_v.Target.Data, _v.Target.Data.mesh_banish, true);
                _v.Target.Data.bi.shadow = 1;
                if (_v.Target.Data.getShadow() != null)
                    _v.Target.Data.getShadow().SetActive(true);

                Vector3 currentPos = _v.Target.Data.pos;
                Quaternion currentRot = _v.Target.Data.rot;

                if (_v.Target.Data.gameObject != null)
                    _v.Target.Data.gameObject.SetActive(false);

                Int32 Key = FF9StateSystem.EventState.gEventGlobal[1305];
                if (Summons.TryGetValue(Key, out SummonData summon))
                {
                    GameObject newModel = ModelFactory.CreateModel(summon.ModelName, true);
                    newModel.transform.localPosition = currentPos;
                    newModel.transform.localRotation = currentRot;
                    _v.Target.Data.gameObject = newModel;
                    _v.Target.Data.animation = newModel.GetComponent<Animation>();

                    String[] newMot = new String[Mathf.Max(6, _v.Target.Data.mot.Length)];
                    for (Int16 i = 0; i < 6; i++)
                    {
                        String animName = FF9BattleDB.Animation[summon.AnimIds[i]];
                        newMot[i] = animName;
                        btl_util.getEnemyTypePtr(_v.Target.Data).mot[i] = animName;
                    }
                    _v.Target.Data.mot = newMot;

                    for (Int16 j = 0; j < 6; j++)
                    {
                        String currentAnim = _v.Target.Data.mot[j];
                        if (!String.IsNullOrEmpty(currentAnim))
                            AnimationFactory.AddAnimWithAnimatioName(_v.Target.Data.gameObject, currentAnim);
                    }

                    _v.Target.Data.bi.stop_anim = 0;
                    String animToPlay = _v.Target.Data.mot[_v.Target.Data.bi.def_idle];
                    btl_mot.setMotion(_v.Target.Data, (Byte)_v.Target.Data.bi.def_idle);
                    _v.Target.Data.evt.animFrame = 0;

                    ENEMY_TYPE et = FF9StateSystem.Battle.FF9Battle.enemy[_v.Target.Data.bi.slot_no].et;
                    et.name = summon.BTLName;

                    geo.geoScaleUpdate(_v.Target.Data, true);
                    _v.Target.Data.gameObject.SetActive(true);
                    UIManager.Battle.RefreshNameTarget();
                }
                else
                {
                    _v.Context.Flags |= BattleCalcFlags.Miss;
                    return;
                }
            }
        }
    }
}
