using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

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
