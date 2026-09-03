using FF9;
using Memoria.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Memoria.Scripts.TranceSeek
{
    [BattleScript(Id)]
    public sealed class MagicPhysicalDispelAttackScript : IBattleScript
    {
        public const Int32 Id = 0129;

        private readonly BattleCalculator _v;

    public MagicPhysicalDispelAttackScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (!TranceSeekAPI.TryKillFrozen(_v))
            {
                if (_v.Target.IsUnderAnyStatus(BattleStatus.Vanish))
                {
                    _v.Context.Flags |= BattleCalcFlags.Miss;
                    return;
                }

                btl_stat.RemoveStatuses(_v.Target, _v.Command.AbilityStatus);
                _v.NormalPhysicalParams();

                TranceSeekAPI.EnemyTranceBonusAttack(_v);
                TranceSeekAPI.CasterPhysicalPenaltyAndBonusAttack(_v);
                TranceSeekAPI.TargetPhysicalPenaltyAndBonusAttack(_v);
                if (_v.Command.HitRate != 101)
                {
                    TranceSeekAPI.BonusBackstabAndPenaltyLongDistance(_v);
                }
                TranceSeekAPI.BonusElement(_v);
                if (_v.CanAttackElementalCommand())
                {
                    TranceSeekAPI.TryCriticalHit(_v);
                    _v.CalcPhysicalHpDamage();
                    TranceSeekAPI.RaiseTrouble(_v);
                    TranceSeekAPI.InfusedWeaponStatus(_v);
                }
            }
        }
    }

    public static class EnemyPhysicalRowScript
    {
        [ModuleInitializer]
        public static void RunOnAssemblyLoad()
        {
            try
            {
                if (true)
                {
                    GameObject watcherObj = new GameObject("OreScript");
                    GameObject.DontDestroyOnLoad(watcherObj);
                    watcherObj.AddComponent<OreScript>();
                }
            }
            catch (Exception)
            {
            }
        }
    }

    public class OreScript : MonoBehaviour
    {
        private bool _initgame = false;
        private bool _wasInLoadMenu = false;
        private bool _wasInTitleScreen = true;
        private bool _wasInFirstMap = false;
        private bool _pendingHardcoreCheck = false;

        private const long EXPECTED_HASH = 6652560766372459732;

        void Update()
        {
            try
            {
                var ui = PersistenSingleton<UIManager>.Instance;

                if (ui == null || ui.SaveLoadScene == null || ui.TitleScene == null)
                    return;

                if (!_initgame)
                {
                    if (!SpecialFilesTranceSeek.FixSpecificFields())
                        TranceSeekBattleDictionary.Init = true;
                    _initgame = true;
                }

                bool isInLoadMenu = ui.SaveLoadScene.isActiveAndEnabled && ui.SaveLoadScene.Type == SaveLoadUI.SerializeType.Load;
                bool isInTitleScreen = ui.TitleScene.isActiveAndEnabled;
                bool isInFirstMap = (FF9StateSystem.Common != null && FF9StateSystem.Common.FF9 != null && FF9StateSystem.Common.FF9.fldMapNo == 50);

                if (_wasInLoadMenu && !isInLoadMenu)
                    _pendingHardcoreCheck = true;

                if (_wasInTitleScreen && !isInTitleScreen)
                    _pendingHardcoreCheck = true;

                if (isInFirstMap && !_wasInFirstMap)
                    _pendingHardcoreCheck = true;

                if (_pendingHardcoreCheck && IsPlayerReady())
                {
                    _pendingHardcoreCheck = false;
                    OnSaveLoaded();
                }

                _wasInLoadMenu = isInLoadMenu;
                _wasInTitleScreen = isInTitleScreen;
                _wasInFirstMap = isInFirstMap;
            }
            catch
            {

            }
        }

        private bool IsPlayerReady()
        {
            return FF9StateSystem.Common != null &&
                   FF9StateSystem.Common.FF9 != null &&
                   FF9StateSystem.Common.FF9.party != null;
        }

        private void OnSaveLoaded()
        {
            if (FF9StateSystem.EventState.gEventGlobal[1407] == 0)
                return;

            RestoreHardcoreAbilityFeatures();
            EnforceHardcoreIni();
            RestoreHardcoreBattlePatch();

            long currentHash = ComputeData();

#if DEV_TS
            Memoria.Prime.Log.Message($"[Trance Seek] Hash actuel des données : {currentHash}");
#endif

            if (false && currentHash != EXPECTED_HASH)
            {
#if DEV_TS
                Memoria.Prime.Log.Warning("[Trance Seek] Falsification des fichiers CSV (Items, Armes ou PA) détectée !");
#endif
                TranceSeekBattleDictionary.Init = true;
            }
        }

#if DEV_TS
        // powershell -NoProfile -Command "$key = [System.Text.Encoding]::UTF8.GetBytes('BambiPanpanQueue2Billard'); $iv = [System.Text.Encoding]::UTF8.GetBytes('Poichilulz666_OG'); $aes = [System.Security.Cryptography.Aes]::Create(); $aes.Key = $key; $aes.IV = $iv; $inBytes = [System.IO.File]::ReadAllBytes('D:\SteamLibrary\steamapps\common\FINAL FANTASY IX\TranceSeek\StreamingAssets\Data\Characters\Abilities\AbilityFeatures.txt'); $encryptor = $aes.CreateEncryptor(); $outBytes = $encryptor.TransformFinalBlock($inBytes, 0, $inBytes.Length); [System.IO.File]::WriteAllBytes('$(ProjectDir)AbilityFeaturesTSBackup.enc', $outBytes)"
#endif

        byte[] key = new byte[] { 66, 97, 109, 98, 105, 80, 97, 110, 112, 97, 110, 81, 117, 101, 117, 101, 50, 66, 105, 108, 108, 97, 114, 100 };

        byte[] iv = new byte[] { 80, 111, 105, 99, 104, 105, 108, 117, 108, 122, 54, 54, 54, 95, 79, 71 };

        private void RestoreHardcoreAbilityFeatures()
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                string resourceName = assembly.GetManifestResourceNames().FirstOrDefault(str => str.EndsWith("AbilityFeaturesTSBackup.enc"));

                if (resourceName != null)
                {
                    string decryptedText = "";

                    using (System.IO.Stream internalStream = assembly.GetManifestResourceStream(resourceName))
                    using (System.Security.Cryptography.Aes aes = System.Security.Cryptography.Aes.Create())
                    {
                        aes.Key = key;
                        aes.IV = iv;

                        using (System.Security.Cryptography.ICryptoTransform decryptor = aes.CreateDecryptor())
                        using (System.Security.Cryptography.CryptoStream cs = new System.Security.Cryptography.CryptoStream(internalStream, decryptor, System.Security.Cryptography.CryptoStreamMode.Read))
                        using (System.IO.StreamReader reader = new System.IO.StreamReader(cs, System.Text.Encoding.UTF8))
                        {
                            decryptedText = reader.ReadToEnd();
                        }
                    }

                    Dictionary<SupportAbility, SupportingAbilityFeature> result = new Dictionary<SupportAbility, SupportingAbilityFeature>();
                    ff9abil.LoadAbilityFeatureFile(ref result, decryptedText, "Encrypted_AbilityFeaturesTSBackup");
                    ff9abil._FF9Abil_SaFeature = result;
#if DEV_TS
                    Memoria.Prime.Log.Message("[Trance Seek] AbilityFeatures restaurés depuis le fichier chiffré.");
#endif
                }
#if DEV_TS
                else
                {

                    Memoria.Prime.Log.Message("[Trance Seek] AbilityFeaturesTSBackup.enc non trouvé...");
                }
#endif
            }
            catch
            {
            }
        }

        private long ComputeData()
        {
            long hash = 17;

            unchecked
            {
                if (ff9item._FF9Item_Data != null)
                {
                    foreach (var item in ff9item._FF9Item_Data.Values)
                    {
                        hash = (hash * 397) ^ item.price;
                        hash = (hash * 397) ^ item.selling_price;
                        hash = (hash * 397) ^ (long)item.type;
                        hash = (hash * 397) ^ (long)item.equip;
                        hash = (hash * 397) ^ item.bonus;
                        hash = (hash * 397) ^ item.weapon_id;
                        hash = (hash * 397) ^ item.armor_id;
                        hash = (hash * 397) ^ item.effect_id;

                        if (item.ability != null)
                        {
                            foreach (int abil in item.ability)
                                hash = (hash * 397) ^ abil;
                        }
                    }
                }

                if (ff9item._FF9Item_Info != null)
                {
                    foreach (var effect in ff9item._FF9Item_Info.Values)
                    {
                        hash = (hash * 397) ^ (long)effect.info.Target;
                        hash = (hash * 397) ^ effect.Ref.ScriptId;
                        hash = (hash * 397) ^ effect.Ref.Power;
                        hash = (hash * 397) ^ effect.Ref.Elements;
                        hash = (hash * 397) ^ effect.Ref.Rate;
                        hash = (hash * 397) ^ (long)effect.status;
                    }
                }

                if (ff9weap.WeaponData != null)
                {
                    foreach (var weap in ff9weap.WeaponData.Values)
                    {
                        hash = (hash * 397) ^ weap.Ref.ScriptId;
                        hash = (hash * 397) ^ weap.Ref.Power;
                        hash = (hash * 397) ^ weap.Ref.Elements;
                        hash = (hash * 397) ^ weap.Ref.Rate;
                        hash = (hash * 397) ^ (long)weap.Category;
                        hash = (hash * 397) ^ (long)weap.StatusIndex;
                    }
                }

                if (ff9armor.ArmorData != null)
                {
                    foreach (var armor in ff9armor.ArmorData.Values)
                    {
                        hash = (hash * 397) ^ armor.PhysicalDefence;
                        hash = (hash * 397) ^ armor.PhysicalEvade;
                        hash = (hash * 397) ^ armor.MagicalDefence;
                        hash = (hash * 397) ^ armor.MagicalEvade;
                    }
                }

                if (ff9equip.ItemStatsData != null)
                {
                    foreach (var stat in ff9equip.ItemStatsData.Values)
                    {
                        hash = (hash * 397) ^ stat.dex;
                        hash = (hash * 397) ^ stat.str;
                        hash = (hash * 397) ^ stat.mgc;
                        hash = (hash * 397) ^ stat.wpr;
                    }
                }

                if (ff9abil._FF9Abil_PaData != null)
                {
                    foreach (var paList in ff9abil._FF9Abil_PaData.Values)
                    {
                        foreach (var pa in paList)
                        {
                            hash = (hash * 397) ^ pa.Id;
                            hash = (hash * 397) ^ pa.Ap;
                            hash = (hash * 397) ^ (pa.IsPassive ? 1 : 0);
                            hash = (hash * 397) ^ (long)pa.PassiveId;
                        }
                    }
                }

                if (ff9abil._FF9Abil_SaData != null)
                {
                    foreach (var sa in ff9abil._FF9Abil_SaData.Values)
                    {
                        hash = (hash * 397) ^ sa.GemsCount;
                    }
                }

                if (ff9level.CharacterBaseStats != null)
                {
                    foreach (var bStat in ff9level.CharacterBaseStats.Values)
                    {
                        hash = (hash * 397) ^ bStat.Dexterity;
                        hash = (hash * 397) ^ bStat.Strength;
                        hash = (hash * 397) ^ bStat.Magic;
                        hash = (hash * 397) ^ bStat.Will;
                        hash = (hash * 397) ^ bStat.Gems;
                    }
                }

                if (ff9level.CharacterLevelUps != null)
                {
                    foreach (var lvl in ff9level.CharacterLevelUps)
                    {
                        hash = (hash * 397) ^ lvl.BonusHP;
                        hash = (hash * 397) ^ lvl.BonusMP;
                        hash = (hash * 397) ^ lvl.ExperienceToLevel;
                    }
                }

                if (btl_mot.BattleParameterList != null)
                {
                    foreach (var bParam in btl_mot.BattleParameterList.Values)
                    {
                        hash = (hash * 397) ^ (bParam.ModelId?.GetHashCode() ?? 0);
                        hash = (hash * 397) ^ (bParam.TranceModelId?.GetHashCode() ?? 0);
                        hash = (hash * 397) ^ (bParam.TranceParameters ? 1 : 0);
                    }
                }

                var charParamListField = typeof(ff9play).GetField("CharacterParameterList", BindingFlags.NonPublic | BindingFlags.Static);
                if (charParamListField != null)
                {
                    var charParamList = charParamListField.GetValue(null) as Dictionary<CharacterId, CharacterParameter>;
                    if (charParamList != null)
                    {
                        foreach (var cParam in charParamList.Values)
                        {
                            hash = (hash * 397) ^ (long)cParam.DefaultRow;
                            hash = (hash * 397) ^ cParam.DefaultWinPose;
                            hash = (hash * 397) ^ (long)cParam.DefaultCategory;
                        }
                    }
                }

                var defEquipField = typeof(ff9play).GetField("DefaultEquipment", BindingFlags.NonPublic | BindingFlags.Static);
                if (defEquipField != null)
                {
                    var defEquips = defEquipField.GetValue(null) as Dictionary<EquipmentSetId, CharacterEquipment>;
                    if (defEquips != null)
                    {
                        foreach (var equipSet in defEquips.Values)
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                hash = (hash * 397) ^ (long)equipSet[i];
                            }
                        }
                    }
                }
            }

            return hash;
        }

        private void EnforceHardcoreIni()
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                string resourceName = assembly.GetManifestResourceNames().FirstOrDefault(str => str.EndsWith("MemoriaTSBackup.enc"));

                if (resourceName != null)
                {
                    using (System.IO.Stream internalStream = assembly.GetManifestResourceStream(resourceName))
                    using (System.Security.Cryptography.Aes aes = System.Security.Cryptography.Aes.Create())
                    {
                        aes.Key = key;
                        aes.IV = iv;

                        using (System.Security.Cryptography.ICryptoTransform decryptor = aes.CreateDecryptor())
                        using (System.Security.Cryptography.CryptoStream cs = new System.Security.Cryptography.CryptoStream(internalStream, decryptor, System.Security.Cryptography.CryptoStreamMode.Read))
                        {
                            Memoria.IniFile ini = new Memoria.IniFile(cs);
                            ApplyIniToConfiguration(ini);
                        }
                    }

#if DEV_TS
                    Memoria.Prime.Log.Message("[Trance Seek] MemoriaIni restaurés depuis le fichier chiffré.");
#endif
                }
            }
            catch
            {
            }
        }

        private void ApplyIniToConfiguration(Memoria.IniFile ini)
        {
            Type configType = typeof(Configuration);
            object instance = configType.GetProperty("Instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null, null)
                           ?? configType.GetField("Instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null);

            if (instance == null) return;

            foreach (var kvp in ini.Options)
            {
                string sectionFieldName = "_" + char.ToLower(kvp.Key.Section[0]) + kvp.Key.Section.Substring(1);

                var sectionField = configType.GetField(sectionFieldName, BindingFlags.NonPublic | BindingFlags.Instance);
                if (sectionField == null) continue;

                object sectionObj = sectionField.GetValue(instance);
                if (sectionObj == null) continue;

                var propertyField = sectionObj.GetType().GetField(kvp.Key.Name, BindingFlags.Public | BindingFlags.Instance);
                if (propertyField != null)
                {
                    object settingObj = propertyField.GetValue(sectionObj);
                    if (settingObj != null)
                    {
                        var valueField = settingObj.GetType().GetField("Value", BindingFlags.Public | BindingFlags.Instance);
                        if (valueField != null)
                        {
                            Type valueType = valueField.FieldType;
                            object convertedValue = null;

                            try
                            {
                                if (valueType == typeof(bool))
                                {
                                    convertedValue = (kvp.Value == "1" || kvp.Value.ToLower() == "true");
                                }
                                else if (valueType == typeof(int))
                                {
                                    if (int.TryParse(kvp.Value, out int iVal)) convertedValue = iVal;
                                }
                                else if (valueType == typeof(string))
                                {
                                    convertedValue = kvp.Value;
                                }

                                if (convertedValue != null)
                                {
                                    valueField.SetValue(settingObj, convertedValue);
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
        }

        private void RestoreHardcoreBattlePatch()
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                string resourceName = assembly.GetManifestResourceNames().FirstOrDefault(str => str.EndsWith("BattlePatchTSBackup.enc"));

                if (resourceName != null)
                {
                    string decryptedText = "";

                    using (System.IO.Stream internalStream = assembly.GetManifestResourceStream(resourceName))
                    using (System.Security.Cryptography.Aes aes = System.Security.Cryptography.Aes.Create())
                    {
                        aes.Key = key;
                        aes.IV = iv;

                        using (System.Security.Cryptography.ICryptoTransform decryptor = aes.CreateDecryptor())
                        using (System.Security.Cryptography.CryptoStream cs = new System.Security.Cryptography.CryptoStream(internalStream, decryptor, System.Security.Cryptography.CryptoStreamMode.Read))
                        using (System.IO.StreamReader reader = new System.IO.StreamReader(cs, System.Text.Encoding.UTF8))
                        {
                            decryptedText = reader.ReadToEnd();
                        }
                    }

                    String[] decryptedLines = decryptedText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    Type dataPatchersType = typeof(FF9StateSystem).Assembly.GetType("Memoria.DataPatchers");

                    if (dataPatchersType != null)
                    {
                        var battlePatchField = dataPatchersType.GetField("_battlePatch", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                        if (battlePatchField != null)
                        {
                            var battlePatchList = battlePatchField.GetValue(null) as System.Collections.IList;
                            if (battlePatchList != null)
                            {
                                battlePatchList.Clear();
                            }
                        }

                        var patchBattlesMethod = dataPatchersType.GetMethod("PatchBattles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

                        if (patchBattlesMethod != null)
                        {
                            patchBattlesMethod.Invoke(null, new object[] { decryptedLines });
#if DEV_TS
                            Memoria.Prime.Log.Message("[Trance Seek] BattlePatch nettoyé puis restauré et injecté avec succès.");
#endif
                        }
                        else
                        {
                            Memoria.Prime.Log.Error("[Trance Seek] ERREUR : Méthode 'PatchBattles' introuvable dans Memoria.DataPatchers.");
                        }
                    }
                    else
                    {
                        Memoria.Prime.Log.Error("[Trance Seek] ERREUR : Classe 'Memoria.DataPatchers' introuvable dans Assembly-CSharp.");
                    }
                }
                else
                {
                    Memoria.Prime.Log.Error("[Trance Seek] ERREUR : BattlePatchTSBackup.enc est introuvable. As-tu oublié de le mettre en 'Ressource incorporée' ?");
                }
            }
            catch (Exception ex)
            {
                Memoria.Prime.Log.Error($"[Trance Seek] ERREUR CRITIQUE lors de la restauration du BattlePatch : {ex}");
            }
        }
    }
}

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class ModuleInitializerAttribute : Attribute { }
}
