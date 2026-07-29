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
            public Int32[] AnimIds;

            public SummonData(String modelName, Int32[] animIds)
            {
                ModelName = modelName;
                AnimIds = animIds;
            }
        }

        private static readonly Dictionary<Int32, SummonData> Summons = new Dictionary<Int32, SummonData>
        {
            { 1, new SummonData("GEO_NPC_F9_ShivaMG", new Int32[] { 30013, 30013, 30013, 30013, 30014, 30014 }) },
            { 2, new SummonData("GEO_NPC_F9_IfritMG", new Int32[] { 30016, 30016, 30016, 30016, 30017, 30017 }) },
            { 3, new SummonData("GEO_NPC_F9_RamuhMG", new Int32[] { 30019, 30019, 30019, 30019, 30020, 30020 }) },
            { 4, new SummonData("GEO_NPC_F9_LeviathanMG", new Int32[] { 30022, 30022, 30022, 30022, 30023, 30023 }) },
            { 5, new SummonData("GEO_NPC_F9_AsuraMG", new Int32[] { 30025, 30025, 30025, 30025, 30028, 30028 }) }
        };

        private readonly BattleCalculator _v;

        public MGSummonScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
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

                Int32 key = 2;
                if (!Summons.TryGetValue(key, out SummonData summon))
                    summon = Summons[1];


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

                geo.geoScaleUpdate(_v.Target.Data, true);
                _v.Target.Data.gameObject.SetActive(true);
            }
        }
    }
}
