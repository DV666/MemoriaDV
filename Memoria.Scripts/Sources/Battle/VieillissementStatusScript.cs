using System;
using System.Collections.Generic;
using System.IO;
using FF9;
using Memoria.Data;
using Memoria.Scripts.Battle;
using UnityEngine;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.CustomStatus16)] // Vieillissement
    public class VieillissementStatusScript : StatusScriptBase
    {
        public Int32 BasicLevel;
        public Int32 BasicStrength;
        public Int32 BasicMagic;
        public Int32 BasicDexterity;
        public Int32 BasicWill;
        public Int32 BasicPhysicalDefence;
        public Int32 BasicPhysicalEvade;
        public Int32 BasicMagicDefence;
        public Int32 BasicMagicEvade;
        public Boolean Init;
        public string[] VanillaTextures = new string[10];
        public SPSEffect spsmonster;

        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            if (target.IsUnderAnyStatus(BattleStatus.EasyKill) || target.IsUnderAnyStatus(BattleStatus.Trance) || target.IsUnderAnyStatus(BattleStatus.CustomStatus16))
                return btl_stat.ALTER_INVALID;

            base.Apply(target, inflicter, parameters);
            if (!Init)
            {
                BasicLevel = Target.Level;
                BasicStrength = Target.Strength;
                BasicMagic = Target.Magic;
                BasicDexterity = Target.Dexterity;
                BasicWill = Target.Will;
                BasicPhysicalDefence = Target.PhysicalDefence;
                BasicPhysicalEvade = Target.PhysicalEvade;
                BasicMagicDefence = Target.MagicDefence;
                BasicMagicEvade = Target.MagicEvade;
                Init = true;
            }    

            BasicStrength = Target.Strength;
            target.Level = (byte)Math.Max(1, BasicLevel - (BasicLevel * 9) / 10);
            target.Strength = (byte)Math.Max(1, BasicStrength - (BasicStrength * 9) / 10);
            target.Magic = (byte)Math.Max(1, BasicMagic - (BasicMagic * 9) / 10);
            target.Dexterity = (byte)Math.Max(1, BasicDexterity - (BasicDexterity * 9) / 10);
            target.Will = (byte)Math.Max(1, BasicWill - (BasicWill * 9) / 10);
            target.PhysicalDefence = (byte)Math.Max(1, BasicPhysicalDefence - (BasicPhysicalDefence * 9) / 10);
            target.PhysicalEvade = (byte)Math.Max(1, BasicPhysicalEvade - (BasicPhysicalEvade * 9) / 10);
            target.MagicDefence = (byte)Math.Max(1, BasicMagicDefence - (BasicMagicDefence * 9) / 10);
            target.MagicEvade = (byte)Math.Max(1, BasicMagicEvade - (BasicMagicEvade * 9) / 10);
            TranceSeekAPI.SA_StatusApply(inflicter, false);
            ChangePlayerTexture(target, true);
            return btl_stat.ALTER_SUCCESS;            
        }

        public override Boolean Remove()
        {
            Target.Level = (byte)BasicLevel;
            Target.Strength = (byte)BasicStrength;
            Target.Magic = (byte)BasicMagic;
            Target.Dexterity = (byte)BasicDexterity;
            Target.Will = (byte)BasicWill;
            Target.PhysicalDefence = (byte)BasicPhysicalDefence;
            Target.PhysicalEvade = (byte)BasicPhysicalEvade;
            Target.MagicDefence = (byte)BasicMagicDefence;
            Target.MagicEvade = (byte)BasicMagicEvade;
            ChangePlayerTexture(Target, false);
            Init = false;
            return true;
        }

        private void ChangePlayerTexture(BattleUnit target, Boolean OldStatus)
        {
            if (target.IsUnderAnyStatus(BattleStatus.Trance))
                return;

            if (!target.IsPlayer)
            {
                if (OldStatus)
                {
                    //btl_stat.AddCustomGlowEffect(target, 0, 80, new int[]{ 250, 250, 250 }, 0);
                }
                else
                {
                    //btl_stat.ClearAllCustomGlowEffect(target);
                }
            }
            else
            {
                CharacterBattleParameter btlParam = btl_mot.BattleParameterList[target.Player.info.serial_no];
                String TexturePath;
                if (OldStatus)
                {
                    TexturePath = "CustomTextures/Players/OldStatus/" + Path.GetDirectoryName(ModelFactory.GetRenameModelPath(ModelFactory.CheckUpscale(btlParam.ModelId))) + "/%.png";
                    int VanillaTexturesID = 0;
                    foreach (Renderer renderer in target.Data.originalGo.GetComponentsInChildren<Renderer>())
                    {
                        VanillaTextures[VanillaTexturesID] = Path.GetDirectoryName(ModelFactory.GetRenameModelPath(ModelFactory.CheckUpscale(btlParam.ModelId))) + "/" + renderer.material.mainTexture.name.Replace(" (Instance)_RT", "");
                        VanillaTexturesID++;
                        String externalPath = AssetManager.SearchAssetOnDisc(TexturePath.Replace("%", renderer.material.mainTexture.name).Replace(" (Instance)_RT", ""), true, false);
                        if (!String.IsNullOrEmpty(externalPath))
                        {
                            Texture texture = AssetManager.LoadFromDisc<Texture2D>(externalPath, "");
                            texture.name = renderer.material.mainTexture.name;
                            renderer.material.mainTexture = texture;
                        }
                    }
                    SkinnedMeshRenderer[] componentsInChildren = target.Data.originalGo.GetComponentsInChildren<SkinnedMeshRenderer>();
                    MeshRenderer[] componentsInChildren2 = target.Data.originalGo.GetComponentsInChildren<MeshRenderer>();
                    NormalSolver.SmoothCharacterMesh(componentsInChildren);
                    NormalSolver.SmoothCharacterMesh(componentsInChildren2);
                }
                else
                {
                    TexturePath = ModelFactory.GetRenameModelPath(ModelFactory.CheckUpscale(btlParam.ModelId));
                    int IDTexture = 0;
                    foreach (Renderer renderer in target.Data.originalGo.GetComponentsInChildren<Renderer>())
                    {
                        Texture texture = AssetManager.Load<Texture>(VanillaTextures[IDTexture], true);
                        IDTexture++;
                        if (texture != null)
                        {
                            renderer.material.SetTexture("_MainTex", texture);
                            ModelFactory.SetMatFilter(renderer.material, Configuration.Graphics.ElementsSmoothTexture);
                        }
                    }
                }
            }
        }
    }
}
