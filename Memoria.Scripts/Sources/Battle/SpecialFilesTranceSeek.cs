using Assets.Scripts.Common;
using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Data;
using Memoria.Prime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Memoria.Scripts.Battle
{
    public class SpecialFilesTranceSeek
    {
        private const String StuffListedPath = "TranceSeek/StuffListed.txt";
        private const String DebugFilePath = "TranceSeek/DebugBattle.txt";
        private const String DebugAAMonstersPath = "TranceSeek/DebugAAMonsters.txt";

        public static int DebugFilesCooldown = 100;

        public static Boolean DebugBattle = true;
        public static void WriteStuffInFile()
        {
            if (!File.Exists(StuffListedPath))
                File.WriteAllText(StuffListedPath, "");

            String data = "";

            var SATranceSeek = new Dictionary<int, string>();
            var RegularItemTranceSeek = new Dictionary<int, string>();

            var SAfields = typeof(TranceSeekSupportAbility)
                .GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (var SAfield in SAfields)
            {
                int value = (int)(SupportAbility)SAfield.GetValue(null);
                SATranceSeek[value] = SAfield.Name;
            }

            var Itemfields = typeof(TranceSeekRegularItem)
             .GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (var itemfield in Itemfields)
            {
                int value = (int)(RegularItem)itemfield.GetValue(null);
                RegularItemTranceSeek[value] = itemfield.Name;
            }

            foreach (BattleUnit PlayerUnit in BattleState.EnumerateUnits())
            {
                if (!PlayerUnit.IsPlayer)
                    continue;

                data += $"################  {FF9TextTool.CharacterDefaultName(PlayerUnit.PlayerIndex)}  ################";

                data += $"\n⚔️ Stuff";
                if (RegularItemTranceSeek.TryGetValue((int)PlayerUnit.Weapon, out string WeaponName))
                    data += "\n └→ 🗡️ Weapon = " + WeaponName;
                else
                    data += "\n └→ 🗡️ Weapon = " + PlayerUnit.Weapon;

                if (RegularItemTranceSeek.TryGetValue((int)PlayerUnit.Head, out string HeadName))
                    data += "\n └→ 🎩 Head = " + HeadName;
                else
                    data += "\n └→ 🎩 Head = " + PlayerUnit.Head;

                if (RegularItemTranceSeek.TryGetValue((int)PlayerUnit.Wrist, out string WristName))
                    data += "\n └→ 🔗 Wrist = " + WristName;
                else
                    data += "\n └→ 🔗 Wrist = " + PlayerUnit.Wrist;

                if (RegularItemTranceSeek.TryGetValue((int)PlayerUnit.Armor, out string ArmorName))
                    data += "\n └→ 🛡️ Armor = " + ArmorName;
                else
                    data += "\n └→ 🛡️ Armor = " + PlayerUnit.Armor;

                if (RegularItemTranceSeek.TryGetValue((int)PlayerUnit.Accessory, out string AccessoryName))
                    data += "\n └→ 💍 Accessory = " + AccessoryName;
                else
                    data += "\n └→ 💍 Accessory = " + PlayerUnit.Accessory;

                data += $"\n\n📊 Stats";
                data += "\n └→ ❤️ HP = " + PlayerUnit.CurrentHp + "/" + PlayerUnit.MaximumHp;
                data += "\n └→ 🔷 MP = " + PlayerUnit.CurrentMp + "/" + PlayerUnit.MaximumMp;
                data += "\n └→ 🏅 Level = " + PlayerUnit.Level;
                data += "\n └→ 🏹 Dexterity = " + PlayerUnit.Dexterity;
                data += "\n └→ 💪 Strength = " + PlayerUnit.Strength;
                data += "\n └→ ✨ Magic = " + PlayerUnit.Magic;
                data += "\n └→ 🧘 Will = " + PlayerUnit.Will;
                data += "\n └→ 🛡️ PhysicalDefence = " + PlayerUnit.PhysicalDefence;
                data += "\n └→ 🌀 PhysicalEvade = " + PlayerUnit.PhysicalEvade;
                data += "\n └→ 🧙 MagicDefence = " + PlayerUnit.MagicDefence;
                data += "\n └→ 💫 MagicEvade = " + PlayerUnit.MagicEvade;

                if (PlayerUnit.Data.saExtended.Count > 0)
                {
                    List<String> PlayerSAName = new List<String>();
                    foreach (SupportAbility saequipped in PlayerUnit.Data.saExtended)
                        if (SATranceSeek.TryGetValue((int)saequipped, out string abilityName))
                            PlayerSAName.Add(abilityName);
                        else
                            PlayerSAName.Add(saequipped.ToString());

                    PlayerSAName.Sort();

                    data += $"\n\n💎 SA equipped";
                    for (Int32 i = 0; i < PlayerSAName.Count; i++)
                        data += "\n └→ " + PlayerSAName[i];
                }

                data += "\n\n";
            }

            File.WriteAllText(StuffListedPath, data);
        }
        public static void WriteDebugBattleFile()
        {
            if (!File.Exists(DebugFilePath))
                File.WriteAllText(DebugFilePath, "");

            String data = "";

            data += "EDIT ? : No";
            data += "\nRefresh ? : No";

            TSDifficulty currentDifficulty = (TSDifficulty)FF9StateSystem.EventState.gEventGlobal[1403];
            data += "\n💀 TranceSeek Difficulty : " + currentDifficulty;
            data += "\n\n";

            foreach (BattleUnit unit in BattleState.EnumerateUnits())
            {
                if (unit.IsPlayer)
                    data += $"################  ⭐ {FF9TextTool.CharacterDefaultName(unit.PlayerIndex)} ⭐  ################";
                else
                    data += $"################  👾 {RemoveTags(unit.Name)} 👾  ################";

                data += $"\n\n📊 Stats";
                data += "\n └→ ID = " + unit.Id;

                if (!unit.IsPlayer)
                {
                    EnemyCategory cat = (EnemyCategory)btl_util.getEnemyTypePtr(unit.Data).category;
                    data += "\n └→ 🐾 Category = " + cat;
                }

                data += "\n └→ ❤️ HP = " + unit.CurrentHp + "/" + unit.MaximumHp;
                data += "\n └→ 🔷 MP = " + unit.CurrentMp + "/" + unit.MaximumMp;
                data += "\n └→ 🏅 Level = " + unit.Level;
                data += "\n └→ 🏹 Dexterity = " + unit.Dexterity;
                data += "\n └→ 💪 Strength = " + unit.Strength;
                data += "\n └→ ✨ Magic = " + unit.Magic;
                data += "\n └→ 🧘 Will = " + unit.Will;
                data += "\n └→ 🛡️ PhysicalDefence = " + unit.PhysicalDefence;
                data += "\n └→ 🌀 PhysicalEvade = " + unit.PhysicalEvade;
                data += "\n └→ 🧙 MagicDefence = " + unit.MagicDefence;
                data += "\n └→ 💫 MagicEvade = " + unit.MagicEvade;

                data += "\n\n🌊 Elements";
                data += "\n └→ ⚠️ Weak Element = " + unit.WeakElement;
                data += "\n └→ 🌗 Half Element = " + unit.HalfElement;
                data += "\n └→ 🧱 Guard Element = " + unit.GuardElement;
                data += "\n └→ 💖 Absorb Element = " + unit.AbsorbElement;

                data += "\n\n🧪 Status";
                data += "\n └→ 🎭 Current Status = " + unit.CurrentStatus;
                data += "\n └→ 💎 Auto Status = " + unit.PermanentStatus;
                data += "\n └→ 🧿 Resist Status = " + unit.ResistStatus;
                data += "\n\n";
            }

            File.WriteAllText(DebugFilePath, data);
        }

        public static void ReadDebugBattleFile()
        {
            if (!File.Exists(DebugFilePath))
                return;

            string fullText = File.ReadAllText(DebugFilePath);


            if (Regex.IsMatch(fullText, @"Refresh \? :.*Yes", RegexOptions.IgnoreCase))
                WriteDebugBattleFile();

            if (!Regex.IsMatch(fullText, @"EDIT \? :.*Yes", RegexOptions.IgnoreCase))
                return;

            Match matchDiff = Regex.Match(fullText, @"💀 TranceSeek Difficulty : (.*)");
            if (matchDiff.Success)
            {
                string diffVal = matchDiff.Groups[1].Value.Trim();
                try
                {
                    TSDifficulty newDiff = (TSDifficulty)Enum.Parse(typeof(TSDifficulty), diffVal, true);
                    FF9StateSystem.EventState.gEventGlobal[1403] = (byte)newDiff;
                    if (FF9StateSystem.EventState.gEventGlobal[1403] >= 4 && FF9StateSystem.EventState.gEventGlobal[1403] <= 6) // Activate Hardcore IA
                        FF9StateSystem.EventState.gEventGlobal[1407] = 1;
                    else
                        FF9StateSystem.EventState.gEventGlobal[1407] = 0;
                }
                catch { }
            }

            string[] unitBlocks = fullText.Split(new string[] { "################" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string block in unitBlocks)
            {
                Match matchID = Regex.Match(block, @"ID = (\d+)");
                if (!matchID.Success) continue;

                int unitId = int.Parse(matchID.Groups[1].Value);
                BattleUnit unit = BattleState.EnumerateUnits().FirstOrDefault(u => u.Id == unitId);

                if (unit == null) continue;

                Match matchHP = Regex.Match(block, @"❤️ HP = (\d+)/(\d+)");
                if (matchHP.Success)
                {
                    uint newCur = uint.Parse(matchHP.Groups[1].Value);
                    uint newMax = uint.Parse(matchHP.Groups[2].Value);
                    if (unit.MaximumHp != newMax) unit.MaximumHp = newMax;
                    if (unit.CurrentHp != newCur) unit.CurrentHp = newCur;
                }

                Match matchMP = Regex.Match(block, @"🔷 MP = (\d+)/(\d+)");
                if (matchMP.Success)
                {
                    uint newCur = uint.Parse(matchMP.Groups[1].Value);
                    uint newMax = uint.Parse(matchMP.Groups[2].Value);
                    if (unit.MaximumMp != newMax) unit.MaximumMp = newMax;
                    if (unit.CurrentMp != newCur) unit.CurrentMp = newCur;
                }

                ApplyStat(block, @"🏅 Level = (\d+)", v => unit.Level = (byte)v);
                ApplyStat(block, @"🏹 Dexterity = (\d+)", v => unit.Dexterity = (byte)v);
                ApplyStat(block, @"💪 Strength = (\d+)", v => unit.Strength = (byte)v);
                ApplyStat(block, @"✨ Magic = (\d+)", v => unit.Magic = (byte)v);
                ApplyStat(block, @"🧘 Will = (\d+)", v => unit.Will = (byte)v);
                ApplyStat(block, @"🛡️ PhysicalDefence = (\d+)", v => unit.PhysicalDefence = (byte)v);
                ApplyStat(block, @"🌀 PhysicalEvade = (\d+)", v => unit.PhysicalEvade = (byte)v);
                ApplyStat(block, @"🧙 MagicDefence = (\d+)", v => unit.MagicDefence = (byte)v);
                ApplyStat(block, @"💫 MagicEvade = (\d+)", v => unit.MagicEvade = (byte)v);

                if (!unit.IsPlayer)
                {
                    Match matchCat = Regex.Match(block, @"🐾 Category = (.*)");
                    if (matchCat.Success)
                    {
                        try
                        {
                            EnemyCategory newCat = (EnemyCategory)Enum.Parse(typeof(EnemyCategory), matchCat.Groups[1].Value.Trim());
                            btl_util.getEnemyTypePtr(unit.Data).category = (byte)newCat;
                        }
                        catch { }
                    }
                }

                UpdateElementLogic(block, @"⚠️ Weak Element = (.*)", unit.WeakElement,
                    (v) => unit.WeakElement |= v, (v) => unit.WeakElement &= ~v);

                UpdateElementLogic(block, @"🌗 Half Element = (.*)", unit.HalfElement,
                    (v) => unit.HalfElement |= v, (v) => unit.HalfElement &= ~v);

                UpdateElementLogic(block, @"🧱 Guard Element = (.*)", unit.GuardElement,
                    (v) => unit.GuardElement |= v, (v) => unit.GuardElement &= ~v);

                UpdateElementLogic(block, @"💖 Absorb Element = (.*)", unit.AbsorbElement,
                    (v) => unit.AbsorbElement |= v, (v) => unit.AbsorbElement &= ~v);

                UpdateStatusLogic(block, @"🎭 Current Status = (.*)", unit.CurrentStatus,
                    (s) => btl_stat.AlterStatuses(unit, s, unit),
                    (s) => btl_stat.RemoveStatuses(unit, s));

                UpdateStatusLogic(block, @"💎 Auto Status = (.*)", unit.PermanentStatus,
                    (s) => btl_stat.MakeStatusesPermanent(unit, s, true),
                    (s) => btl_stat.MakeStatusesPermanent(unit, s, false));

                UpdateStatusLogic(block, @"🧿 Resist Status = (.*)", unit.ResistStatus,
                    (s) => unit.Data.stat.invalid |= s,
                    (s) => unit.Data.stat.invalid &= ~s);
            }

            WriteDebugBattleFile();
        }
        public static void WriteDebugMonsterAttacks()
        {
            if (!File.Exists(DebugAAMonstersPath))
                File.WriteAllText(DebugAAMonstersPath, "");

            String data = "EDIT ? : No";
            data += "\nRefresh ? : No\n\n";

            List<AA_DATA> attackList = FF9StateSystem.Battle.FF9Battle.enemy_attack;

            for (int i = 0; i < attackList.Count; i++)
            {
                AA_DATA attack = attackList[i];
                string attackName = string.IsNullOrEmpty(attack.Name) ? "Unknown" : RemoveTags(attack.Name);

                data += $"################  ⚔️ [ID:{i}] {attackName}  ################";

                data += "\n └→ 💥 Power = " + attack.Ref.Power;
                data += "\n └→ 🎯 Hit Rate = " + attack.Ref.Rate;
                data += "\n └→ 🌊 Elements = " + (EffectElement)attack.Ref.Elements;
                data += "\n └→ 🧪 Add Status Set = " + attack.AddStatusNo;
                data += "\n └→ 💧 MP Cost = " + attack.MP;
                data += "\n └→ 🏷️ Category = " + attack.Category;              
                data += "\n └→ 📜 Script ID = " + attack.Ref.ScriptId;
                data += "\n\n";
            }

            File.WriteAllText(DebugAAMonstersPath, data);
        }

        public static void ReadDebugMonsterAttacks()
        {
            if (!File.Exists(DebugAAMonstersPath))
                return;

            string fullText = File.ReadAllText(DebugAAMonstersPath);

            if (Regex.IsMatch(fullText, @"Refresh \? :.*Yes", RegexOptions.IgnoreCase))
                WriteDebugMonsterAttacks();

            if (!Regex.IsMatch(fullText, @"EDIT \? :.*Yes", RegexOptions.IgnoreCase))
                return;

            Log.Message("[TranceSeek] Start reading Monster Attacks debug file...");

            List<AA_DATA> attackList = FF9StateSystem.Battle.FF9Battle.enemy_attack;

            MatchCollection idMatches = Regex.Matches(fullText, @"\[ID:(\d+)\]");

            for (int i = 0; i < idMatches.Count; i++)
            {
                Match currentMatch = idMatches[i];
                int unitIndex = int.Parse(currentMatch.Groups[1].Value);

                if (unitIndex < 0 || unitIndex >= attackList.Count) continue;

                int startParams = currentMatch.Index;
                int endParams = (i < idMatches.Count - 1) ? idMatches[i + 1].Index : fullText.Length;

                string block = fullText.Substring(startParams, endParams - startParams);

                AA_DATA attack = attackList[unitIndex];

                ApplyStatDebug(block, @"Power\s*=\s*(\d+)", v => {
                    if (attack.Ref.Power != v) Log.Message($"[TranceSeek] Attack {unitIndex}: Power changed {attack.Ref.Power} -> {v}");
                    attack.Ref.Power = v;
                });

                ApplyStatDebug(block, @"Hit Rate\s*=\s*(\d+)", v => attack.Ref.Rate = v);
                ApplyStatDebug(block, @"Script ID\s*=\s*(\d+)", v => attack.Ref.ScriptId = v);
                ApplyStatDebug(block, @"MP Cost\s*=\s*(\d+)", v => attack.MP = v);
                ApplyStatDebug(block, @"Category\s*=\s*(\d+)", v => attack.Category = (byte)v);

                Match matchStatus = Regex.Match(block, @"Add Status Set\s*=\s*(.*)");
                if (matchStatus.Success)
                {
                    try
                    {
                        attack.AddStatusNo = (StatusSetId)Enum.Parse(typeof(StatusSetId), matchStatus.Groups[1].Value.Trim());
                    }
                    catch { }
                }

                Match matchElem = Regex.Match(block, @"Elements\s*=\s*(.*)");
                if (matchElem.Success)
                {
                    try
                    {
                        string val = matchElem.Groups[1].Value.Trim();
                        if (val.Equals("None", StringComparison.OrdinalIgnoreCase))
                            attack.Ref.Elements = 0;
                        else
                        {
                            EffectElement elems = (EffectElement)Enum.Parse(typeof(EffectElement), val);
                            attack.Ref.Elements = (byte)elems;
                        }
                    }
                    catch { }
                }
            }

            Log.Message("[TranceSeek] Reading done. Rewriting file...");
            WriteDebugMonsterAttacks();
        }

        private static void ApplyStatDebug(string block, string pattern, Action<int> applyAction)
        {
            Match m = Regex.Match(block, pattern, RegexOptions.IgnoreCase);
            if (m.Success && int.TryParse(m.Groups[1].Value, out int val))
            {
                applyAction(val);
            }
        }
        private static void ApplyStat(string block, string pattern, Action<int> applyAction)
        {
            Match m = Regex.Match(block, pattern);
            if (m.Success && int.TryParse(m.Groups[1].Value, out int val))
                applyAction(val);
        }

        private static void UpdateStatusLogic(string block, string pattern, BattleStatus currentStatus, Action<BattleStatus> onAdd, Action<BattleStatus> onRemove)
        {
            Match m = Regex.Match(block, pattern);
            if (!m.Success) return;

            string statusString = m.Groups[1].Value;
            BattleStatus targetStatus = 0;

            if (!string.IsNullOrEmpty(statusString) && statusString.Trim().Length > 0 && statusString.Trim() != "0")
            {
                try { targetStatus = (BattleStatus)Enum.Parse(typeof(BattleStatus), statusString.Trim()); }
                catch { return; }
            }

            BattleStatus statusToAdd = targetStatus & ~currentStatus;
            BattleStatus statusToRemove = currentStatus & ~targetStatus;

            if (statusToAdd != 0) onAdd(statusToAdd);
            if (statusToRemove != 0) onRemove(statusToRemove);
        }

        private static void UpdateElementLogic(string block, string pattern, EffectElement currentElement, Action<EffectElement> onAdd, Action<EffectElement> onRemove)
        {
            Match m = Regex.Match(block, pattern);
            if (!m.Success) return;

            string elemString = m.Groups[1].Value;
            EffectElement targetElement = 0;

            if (!string.IsNullOrEmpty(elemString) && elemString.Trim().Length > 0 && elemString.Trim() != "0")
            {
                try { targetElement = (EffectElement)Enum.Parse(typeof(EffectElement), elemString.Trim()); }
                catch { return; }
            }

            EffectElement elemToAdd = targetElement & ~currentElement;
            EffectElement elemToRemove = currentElement & ~targetElement;

            if (elemToAdd != 0) onAdd(elemToAdd);
            if (elemToRemove != 0) onRemove(elemToRemove);
        }

        public static IEnumerator ReloadDebugFiles() // The Unit Delayer here don't work in some case, like against PlantBrain if a put the code on the first character... ?
        {
            while (SceneDirector.IsBattleScene())
            {
                float start = Time.realtimeSinceStartup;

                while (Time.realtimeSinceStartup < start + 1.0f)
                {
                    yield return null;
                }

                try
                {
                    ReadDebugBattleFile();
                    ReadDebugMonsterAttacks();
                }
                catch (Exception)
                {
                    Log.Message("[TranceSeek] Error while reading DEBUG files :(");
                }
            }
        }

        public static String RemoveTags(string s)
        {
            return Regex.Replace(s, @"\[[^]]*\]", "");
        }

        public enum TSDifficulty : ulong
        {
            Zidane,
            Vivi,
            Eiko,
            Kuja,
            Necron,
            Beatrix,
            Ozma,
            Garland
        }
    }
}
