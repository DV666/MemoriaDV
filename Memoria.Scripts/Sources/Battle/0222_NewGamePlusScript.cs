using FF9;
using Memoria.Assets;
using Memoria.Data;
using Memoria.Prime;
using System;
using System.Collections.Generic;
using System.IO;

namespace Memoria.Scripts.Battle
{
    [BattleScript(Id)]
    public sealed class NewGamePlusScript : IBattleScript
    {
        public const Int32 Id = 0222;

        private readonly BattleCalculator _v;
        private static Dictionary<CharacterId, CharacterParameter> NGP_CharacterParameterList;

        public NewGamePlusScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            NGP_CharacterParameterList = NGP_LoadCharacterParameters();

            if (!FF9StateSystem.EventState.gScriptDictionary.TryGetValue(3011, out Dictionary<Int32, Int32> dictreset))
            {
                _v.Context.Flags = BattleCalcFlags.Miss;
                return;
            }

            Boolean ResetLevel = (dictreset[0] == 1); // Reset Level
            Boolean ResetAASA = (dictreset[1] == 1); // Reset AA/SA
            Boolean ResetItems = (dictreset[2] == 1); // Reset items
            Boolean ResetKeyItems = (dictreset[3] == 1); // Reset key items
            if (ResetLevel || ResetAASA)
            {
                foreach (CharacterParameter param in NGP_CharacterParameterList.Values)
                    FF9Play_Reset(param.Id, ResetLevel, ResetAASA, ResetItems);

                if (ResetLevel)
                {
                    foreach (BattleUnit player in BattleState.EnumerateUnits())
                        if (player.IsPlayer)
                        {
                            PLAYER play = FF9StateSystem.Common.FF9.player[player.PlayerIndex];
                            player.CurrentHp = play.cur.hp;
                            player.MaximumHp = play.max.hp;
                            player.CurrentMp = play.cur.mp;
                            player.MaximumMp = play.max.mp;
                        }
                }
            }

            if (ResetItems) // Reset items
            {
                foreach (CharacterParameter param in NGP_CharacterParameterList.Values)
                {
                    PLAYER play = FF9StateSystem.Common.FF9.player[param.Id];
                    play.equip.Weapon = RegularItem.NoItem;
                    play.equip.Head = RegularItem.NoItem;
                    play.equip.Wrist = RegularItem.NoItem;
                    play.equip.Armor = RegularItem.NoItem;
                    play.equip.Accessory = RegularItem.NoItem;
                }
                ff9item.FF9Item_InitNormal();
                LoadInitialItems();
            }

            FF9_ResetKeyItems(ResetKeyItems);

            if (dictreset[4] == 1) // Reset card
            {
                QuadMistDatabase.MiniGame_AwayAllCard();
                QuadMistDatabase.WinCount = 0;
                QuadMistDatabase.LoseCount = 0;
                QuadMistDatabase.DrawCount = 0;
                QuadMistDatabase.SaveData();
            }

            if (dictreset[5] == 1) // Reset gils
            {
                GameState.Gil = 500; // Gils from the beginning.
            }

            if (dictreset[6] == 1) // Reset time
            {
                FF9StateSystem.Settings.time = 0.0;
            }

            FF9StateSystem.Common.FF9.ff9ResetStateGlobal();
            FF9StateSystem.EventState.gStepCount = 0;
            FF9StateSystem.EventState.gEventGlobal = new Byte[2048];
            FF9StateSystem.EventState.gAbilityUsage.Clear();
            FF9StateSystem.EventState.gScriptVector.Clear();
            FF9StateSystem.EventState.gScriptDictionary.Clear();
        }

        private static Dictionary<CharacterId, CharacterParameter> NGP_LoadCharacterParameters()
        {
            try
            {
                String inputPath = DataResources.Characters.PureDirectory + DataResources.Characters.CharacterParametersFile;
                Dictionary<CharacterId, CharacterParameter> result = new Dictionary<CharacterId, CharacterParameter>();
                foreach (CharacterParameter[] characters in AssetManager.EnumerateCsvFromLowToHigh<CharacterParameter>(inputPath))
                    foreach (CharacterParameter character in characters)
                        result[character.Id] = character;
                if (result.Count == 0)
                    throw new FileNotFoundException($"Cannot load character parameters because a file does not exist: [{DataResources.Characters.Directory + DataResources.Characters.CharacterParametersFile}].", DataResources.Characters.Directory + DataResources.Characters.CharacterParametersFile);
                for (Int32 i = 0; i < 12; i++)
                    if (!result.ContainsKey((CharacterId)i))
                        throw new NotSupportedException($"You must define at least 12 character parameters, with IDs between 0 and 11.");
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[ff9play] Load character parameters failed.");
                UIManager.Input.ConfirmQuit();
                return null;
            }
        }

        private static void LoadInitialItems()
        {
            try
            {
                String inputPath = DataResources.Items.PureDirectory + DataResources.Items.InitialItemsFile;
                FF9ITEM[] items = AssetManager.GetCsvWithHighestPriority<FF9ITEM>(inputPath);
                if (items == null)
                    return;
                foreach (FF9ITEM item in items)
                    ff9item.FF9Item_Add(item.id, item.count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[ff9item] Load initial items failed.");
            }
        }

        public static void FF9Play_Reset(CharacterId slotId, Boolean ResetStat, Boolean ResetPa, Boolean ResetStuff)
        {
            PLAYER play = FF9StateSystem.Common.FF9.player[slotId];
            CharacterParameter parameter = NGP_CharacterParameterList[slotId];

            if (ResetPa)
                play.pa = new Int32[ff9abil._FF9Abil_PaData.TryGetValue(play.info.menu_type, out CharacterAbility[] abilList) ? abilList.Length : 0];

            if (ResetStuff)
                ff9play.FF9Play_SetDefEquips(play.equip, parameter.DefaultEquipmentSet, true);

            if (ResetStat)
            {
                play.status = 0;
                play.permanent_status = 0;
                play.category = parameter.DefaultCategory;
                play.bonus = new FF9LEVEL_BONUS();
                play.info.serial_no = parameter.GetSerialNumber();
                ff9play.FF9Play_Build(play, 1, true, false);
                play.cur.hp = play.max.hp;
                play.cur.mp = play.max.mp;
                play.cur.capa = play.max.capa;
                ff9play.FF9Play_Update(play);
            }
        }

        public static void FF9_ResetKeyItems(Boolean ResetAll)
        {
            // 26 = King of Jump Rope ; 27 = Athlete Queen ; 67 = Rank S Medal ; 69 = Strategy Guide
            // 39 = Griffin’s Heart ; 40 = Doga’s Artifact ; 41 = Une’s Mirror ; 42 = Rat Tail ; 43 = Magical Fingertip
            HashSet<int> itemblacklist = new HashSet<int> { 26, 27, 39, 40, 41, 42, 43, 67, 69 };
            HashSet<int> rate_item_obtained = new HashSet<int>();

            foreach (int key_o in FF9StateSystem.Common.FF9.rare_item_obtained)
                rate_item_obtained.Add(key_o);

            foreach (int keyitem_obtained in rate_item_obtained)
            {
                if (!itemblacklist.Contains(keyitem_obtained) || ResetAll)
                    ff9item.FF9Item_RemoveImportant(keyitem_obtained);
            }
        }
    }
}
