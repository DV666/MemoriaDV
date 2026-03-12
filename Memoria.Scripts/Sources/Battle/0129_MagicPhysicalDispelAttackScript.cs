using FF9;
using Memoria;
using Memoria.Data;
using Memoria.Prime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using UnityEngine;

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class ModuleInitializerAttribute : Attribute { }
}

namespace Memoria.Scripts.Battle
{
    public static class ModInitializer
    {
        [ModuleInitializer]
        public static void RunOnAssemblyLoad()
        {
            try
            {
                GameObject watcherObj = new GameObject("SaveLoadWatcher");
                GameObject.DontDestroyOnLoad(watcherObj);
                watcherObj.AddComponent<TonModWatcher>();

            }
            catch (Exception)
            {
            }
        }
    }

    public class TonModWatcher : MonoBehaviour
    {
        private bool _wasInLoadMenu = false;
        private bool _wasInTitleScreen = true;

        void Update()
        {
            try
            {
                var ui = PersistenSingleton<UIManager>.Instance;

                if (ui == null || ui.SaveLoadScene == null || ui.TitleScene == null)
                    return;

                bool isInLoadMenu = ui.SaveLoadScene.isActiveAndEnabled && ui.SaveLoadScene.Type == SaveLoadUI.SerializeType.Load;
                bool isInTitleScreen = ui.TitleScene.isActiveAndEnabled;

                if (_wasInLoadMenu && !isInLoadMenu)
                {
                    if (IsPlayerReady()) OnSaveLoaded("Moogle");
                }

                if (_wasInTitleScreen && !isInTitleScreen)
                {
                    if (IsPlayerReady()) OnSaveLoaded("Écran Titre");
                }

                _wasInLoadMenu = isInLoadMenu;
                _wasInTitleScreen = isInTitleScreen;
            }
            catch (Exception)
            {
            }
        }

        private bool IsPlayerReady()
        {
            return FF9StateSystem.Common != null &&
                   FF9StateSystem.Common.FF9 != null &&
                   FF9StateSystem.Common.FF9.party != null;
        }

        private void OnSaveLoaded(string source)
        {
            //EncryptAllDataFiles();
            //VerifyAndRestoreActionsCSV();
            if (FF9StateSystem.EventState.gEventGlobal[1403] >= 4 && FF9StateSystem.EventState.gEventGlobal[1403] <= 6 && false)
            {
                ForceCheatValue("SpeedTimer", true);
                ForceCheatValue("BattleAssistance", false);
                ForceCheatValue("Attack9999", false);
                ForceCheatValue("NoRandomEncounter", false);
                ForceCheatValue("MasterSkill", false);
                ForceCheatValue("LvMax", false);
                ForceCheatValue("GilMax", false);

                try
                {
                    var assembly = System.Reflection.Assembly.GetExecutingAssembly();

                    string resourceName = assembly.GetManifestResourceNames().FirstOrDefault(str => str.EndsWith("AbilityFeaturesTSBackup.enc"));

                    if (resourceName != null)
                    {
                        byte[] key = System.Text.Encoding.UTF8.GetBytes("TranceSeekSecretKey1234567890123");
                        byte[] iv = System.Text.Encoding.UTF8.GetBytes("TranceSeekIV5678");

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

                        Dictionary<Memoria.Data.SupportAbility, Memoria.Data.SupportingAbilityFeature> result = new Dictionary<Memoria.Data.SupportAbility, Memoria.Data.SupportingAbilityFeature>();

                        ff9abil.LoadAbilityFeatureFile(ref result, decryptedText, "Encrypted_AbilityFeaturesTSBackup");

                        ff9abil._FF9Abil_SaFeature = result;

                    }
                }
                catch (Exception)
                {
                }

                try
                {
                    Type scriptsLoaderType = typeof(Memoria.Scripts.ScriptsLoader);

                    FieldInfo sResultField = scriptsLoaderType.GetField("s_result", BindingFlags.NonPublic | BindingFlags.Static);

                    if (sResultField != null)
                    {
                        IEnumerable resultsList = sResultField.GetValue(null) as IEnumerable;

                        if (resultsList != null)
                        {
                            foreach (object res in resultsList)
                            {
                                FieldInfo dllPathField = res.GetType().GetField("DLLPath", BindingFlags.Public | BindingFlags.Instance);
                                string dllPath = dllPathField?.GetValue(res) as string;

                                if (!string.IsNullOrEmpty(dllPath) &&
                                    !dllPath.Contains("TranceSeek") &&
                                    !dllPath.EndsWith("Memoria.Scripts.dll"))
                                {
                                    FieldInfo overloadsField = res.GetType().GetField("OverloadableMethodScripts", BindingFlags.Public | BindingFlags.Instance);

                                    if (overloadsField != null)
                                    {
                                        object overloadsDict = overloadsField.GetValue(res);
                                        MethodInfo clearMethod = overloadsDict.GetType().GetMethod("Clear");
                                        clearMethod?.Invoke(overloadsDict, null);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        private void ForceCheatValue(string cheatName, bool newValue)
        {
            try
            {
                Type configType = typeof(Configuration);
                object instance = null;

                var instanceProp = configType.GetProperty("Instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (instanceProp != null) instance = instanceProp.GetValue(null, null);
                else
                {
                    var instanceField = configType.GetField("Instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    if (instanceField != null) instance = instanceField.GetValue(null);
                }

                if (instance == null) return;

                var cheatsField = configType.GetField("_cheats", BindingFlags.NonPublic | BindingFlags.Instance);
                if (cheatsField == null) return;

                object cheatsSection = cheatsField.GetValue(instance);
                if (cheatsSection == null) return;

                var specificCheatField = cheatsSection.GetType().GetField(cheatName, BindingFlags.Public | BindingFlags.Instance);
                if (specificCheatField != null)
                {
                    object iniValueObj = specificCheatField.GetValue(cheatsSection);
                    if (iniValueObj != null)
                    {
                        var valueField = iniValueObj.GetType().GetField("Value", BindingFlags.Public | BindingFlags.Instance);
                        if (valueField != null)
                        {
                            valueField.SetValue(iniValueObj, newValue);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void VerifyAndRestoreActionsCSV()
        {
            try
            {
                string externalPath = "TranceSeek/StreamingAssets/Data/Battle/Actions.csv";

                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                string resourceName = assembly.GetManifestResourceNames().FirstOrDefault(str => str.EndsWith("Actions.enc"));

                if (resourceName == null)
                {
                    Memoria.Prime.Log.Warning("[Trance Seek Anti-Cheat] Fichier Actions.enc introuvable dans le DLL !");
                    return;
                }

                byte[] key = System.Text.Encoding.UTF8.GetBytes("TranceSeekSecretKey1234567890123");
                byte[] iv = System.Text.Encoding.UTF8.GetBytes("TranceSeekIV5678");

                byte[] decryptedInternalData;

                using (System.IO.Stream internalStream = assembly.GetManifestResourceStream(resourceName))
                using (System.Security.Cryptography.Aes aes = System.Security.Cryptography.Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    using (System.Security.Cryptography.ICryptoTransform decryptor = aes.CreateDecryptor())
                    using (System.Security.Cryptography.CryptoStream cs = new System.Security.Cryptography.CryptoStream(internalStream, decryptor, System.Security.Cryptography.CryptoStreamMode.Read))
                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                    {
                        byte[] buffer = new byte[81920];
                        int bytesRead;
                        while ((bytesRead = cs.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ms.Write(buffer, 0, bytesRead);
                        }
                        decryptedInternalData = ms.ToArray();
                    }
                }

                string internalHash;
                using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
                {
                    internalHash = BitConverter.ToString(md5.ComputeHash(decryptedInternalData));
                }

                string externalHash = "";
                if (System.IO.File.Exists(externalPath))
                {
                    using (var externalStream = System.IO.File.OpenRead(externalPath))
                    using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
                    {
                        externalHash = BitConverter.ToString(md5.ComputeHash(externalStream));
                    }
                }

                if (internalHash != externalHash)
                {
                    Memoria.Prime.Log.Message("[Trance Seek Anti-Cheat] Fichier Actions.csv altéré détecté ! Chargement des données pures en mémoire...");

                    string backupPath = externalPath + ".cheat_backup";

                    try
                    {
                        if (System.IO.File.Exists(externalPath))
                        {
                            if (System.IO.File.Exists(backupPath)) System.IO.File.Delete(backupPath);
                            System.IO.File.Move(externalPath, backupPath);
                        }

                        System.IO.File.WriteAllBytes(externalPath, decryptedInternalData);

                        Type dbType = typeof(FF9BattleDB);
                        System.Reflection.MethodInfo loadActionsMethod = dbType.GetMethod("LoadActions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

                        if (loadActionsMethod != null)
                        {
                            object newActionsDictObj = loadActionsMethod.Invoke(null, null);
                            System.Collections.IDictionary newActionsDict = newActionsDictObj as System.Collections.IDictionary;
                            System.Reflection.FieldInfo charActionsField = dbType.GetField("CharacterActions", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                            if (charActionsField != null && newActionsDict != null)
                            {
                                System.Collections.IDictionary currentActionsDict = charActionsField.GetValue(null) as System.Collections.IDictionary;

                                if (currentActionsDict != null)
                                {
                                    currentActionsDict.Clear();
                                    foreach (System.Collections.DictionaryEntry kvp in newActionsDict)
                                    {
                                        currentActionsDict.Add(kvp.Key, kvp.Value);
                                    }

                                    Memoria.Prime.Log.Message("[Trance Seek Anti-Cheat] Base de données d'actions purgée et remplie avec succès !");
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (System.IO.File.Exists(externalPath))
                            System.IO.File.Delete(externalPath);

                        if (System.IO.File.Exists(backupPath))
                            System.IO.File.Move(backupPath, externalPath);
                    }
                }
                else
                {
                    Memoria.Prime.Log.Message("[Trance Seek Anti-Cheat] Intégrité de Actions.csv validée (Hash OK).");
                }
            }
            catch (Exception ex)
            {
                Memoria.Prime.Log.Error(ex, "[Trance Seek Anti-Cheat] Erreur critique lors de la vérification de Actions.csv.");
            }
        }

        public static void EncryptAllDataFiles()
        {
            try
            {
                string outputFolder = @"TranceSeek/StreamingAssets/Data_Encrypted";

                if (!System.IO.Directory.Exists(outputFolder))
                {
                    System.IO.Directory.CreateDirectory(outputFolder);
                }

                // CORRECTION : La clé fait maintenant EXACTEMENT 32 caractères !
                byte[] key = System.Text.Encoding.UTF8.GetBytes("TranceSeekSecretKey1234567890123");
                // Le vecteur fait toujours EXACTEMENT 16 caractères
                byte[] iv = System.Text.Encoding.UTF8.GetBytes("TranceSeekIV5678");

                // On attrape notre propre DLL
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();

                // On récupère la liste de tous les fichiers incorporés dedans
                string[] resourceNames = assembly.GetManifestResourceNames();

                int count = 0;
                foreach (string resource in resourceNames)
                {
                    // On ne cible que les fichiers textes et CSV que tu as mis en Ressource Incorporée
                    if (resource.EndsWith(".txt") || resource.EndsWith(".csv"))
                    {
                        // Astuce : Les ressources ont un nom complet comme "Memoria.Scripts.TonMod.Data.Actions.csv"
                        // On découpe ce nom pour récupérer juste "Actions"
                        string[] parts = resource.Split('.');
                        string fileName = parts[parts.Length - 2];

                        string outputPath = System.IO.Path.Combine(outputFolder, fileName + ".enc");

                        using (System.Security.Cryptography.Aes aes = System.Security.Cryptography.Aes.Create())
                        {
                            aes.Key = key;
                            aes.IV = iv;

                            // On lit le fichier directement depuis la mémoire du DLL
                            using (System.IO.Stream internalStream = assembly.GetManifestResourceStream(resource))
                            using (System.IO.FileStream fsOut = new System.IO.FileStream(outputPath, System.IO.FileMode.Create))
                            using (System.Security.Cryptography.ICryptoTransform encryptor = aes.CreateEncryptor())
                            using (System.Security.Cryptography.CryptoStream cs = new System.Security.Cryptography.CryptoStream(fsOut, encryptor, System.Security.Cryptography.CryptoStreamMode.Write))
                            {
                                // On utilise la boucle manuelle sécurisée pour éviter le bug du CopyTo
                                byte[] buffer = new byte[81920];
                                int bytesRead;
                                while ((bytesRead = internalStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    cs.Write(buffer, 0, bytesRead);
                                }
                            }
                        }
                        Memoria.Prime.Log.Message($"[Trance Seek Encryptor] Fichier incorporé chiffré : {fileName} -> .enc");
                        count++;
                    }
                }

                Memoria.Prime.Log.Message($"[Trance Seek Encryptor] Terminé ! {count} fichiers ont été extraits et chiffrés.");
            }
            catch (Exception ex)
            {
                Memoria.Prime.Log.Error(ex, "[Trance Seek Encryptor] Erreur lors du chiffrement des fichiers.");
            }
        }
    }
}
