using FF9;
using Memoria.Data;
using Memoria.Prime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Memoria.Scripts.TranceSeek
{
    public class TranceSeekHackShop : MonoBehaviour
    {
        private ShopUI _lastSortedShop = null;
        private string _lastActiveGroup = string.Empty;

        private float _updateTimer = 0f;
        private const float UpdateInterval = 0.50f;

        private ShopUI _cachedShopUI = null;
        private ItemUI _cachedItemUI = null;
        private EquipUI _cachedEquipUI = null;

        private void Update()
        {
            _updateTimer += Time.deltaTime;
            if (_updateTimer < UpdateInterval) return;
            _updateTimer = 0f;

            if (_cachedShopUI == null)
            {
                _cachedShopUI = UnityEngine.Object.FindObjectOfType<ShopUI>();
            }

            if (_cachedShopUI != null && _cachedShopUI.isActiveAndEnabled)
            {
                FieldInfo typeField = typeof(ShopUI).GetField("type", BindingFlags.NonPublic | BindingFlags.Instance);
                if (typeField != null)
                {
                    ShopUI.ShopType shopType = (ShopUI.ShopType)typeField.GetValue(_cachedShopUI);

                    if (shopType == ShopUI.ShopType.Synthesis && _lastSortedShop != _cachedShopUI)
                    {
                        _lastSortedShop = _cachedShopUI;
                        SortAndRefreshSynthesisShop(_cachedShopUI);
                    }
                }
            }
            else
            {
                _lastSortedShop = null;
            }

            if (_cachedEquipUI == null)
            {
                _cachedEquipUI = UnityEngine.Object.FindObjectOfType<EquipUI>();
            }

            if (_cachedEquipUI != null && _cachedEquipUI.isActiveAndEnabled)
            {
                ApplyEquipCustomSort(_cachedEquipUI);
            }

            /*if (_cachedItemUI == null)
            {
                _cachedItemUI = UnityEngine.Object.FindObjectOfType<ItemUI>();
            }

            if (_cachedItemUI != null && _cachedItemUI.isActiveAndEnabled)
            {
                string currentGroup = ButtonGroupState.ActiveGroup;

                if (_lastActiveGroup == ItemUI.ArrangeMenuGroupButton && currentGroup == ItemUI.SubMenuGroupButton)
                {
                    FieldInfo arrangeModeField = typeof(ItemUI).GetField("_currentArrangeMode", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (arrangeModeField != null)
                    {
                        int arrangeMode = (int)arrangeModeField.GetValue(_cachedItemUI);

                        if (arrangeMode == 1)
                        {
                            ApplyCustomSort(_cachedItemUI);
                        }
                    }
                }

                _lastActiveGroup = currentGroup;
            }
            else
            {
                _lastActiveGroup = string.Empty;
            }*/
        }

        private void SortAndRefreshSynthesisShop(ShopUI shop)
        {
            try
            {
                FieldInfo mixListField = typeof(ShopUI).GetField("mixItemList", BindingFlags.NonPublic | BindingFlags.Instance);
                if (mixListField == null) return;

                List<FF9MIX_DATA> mixItemList = (List<FF9MIX_DATA>)mixListField.GetValue(shop);

                if (mixItemList == null || mixItemList.Count == 0) return;

                List<FF9MIX_DATA> sortedList = mixItemList.OrderBy(synth =>
                {
                    if (synth != null && ff9item._FF9Item_Data != null)
                    {
                        FF9ITEM_DATA itemData = ff9item._FF9Item_Data[synth.Result];
                        if (itemData != null)
                        {
                            return itemData.shape;
                        }
                    }
                    return 0;
                })
                .ThenBy(synth => synth != null ? synth.Price : 0)
                .ToList();

                mixItemList.Clear();
                mixItemList.AddRange(sortedList);

                MethodInfo setShopTypeMethod = typeof(ShopUI).GetMethod("SetShopType", BindingFlags.NonPublic | BindingFlags.Instance);
                if (setShopTypeMethod != null)
                {
                    setShopTypeMethod.Invoke(shop, new object[] { ShopUI.ShopType.Synthesis });
                    Log.Message($"[TranceSeekShop] The synth shop n°{shop.Id} has been sorted (by shape then synthesis price).");
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[TranceSeekShop] Error when sorting the synth shop n°{shop.Id} : {ex.Message}");
            }
        }

        private static readonly List<int> PriorityEquipShapes = new List<int>
        {
            30, // Dark Matter
            32 // Lapita
        };

        private int GetShapePriority(int shape)
        {
            int index = PriorityEquipShapes.IndexOf(shape);
            return index != -1 ? index : int.MaxValue;
        }

        private int CompareEquipItems(FF9ITEM item1, FF9ITEM item2)
        {
            if (item1.id == item2.id) return 0;
            if (item1.id == RegularItem.NoItem) return 1;
            if (item2.id == RegularItem.NoItem) return -1;

            FF9ITEM_DATA data1 = ff9item._FF9Item_Data[item1.id];
            FF9ITEM_DATA data2 = ff9item._FF9Item_Data[item2.id];

            int prio1 = GetShapePriority(data1.shape);
            int prio2 = GetShapePriority(data2.shape);
            int comp = prio1.CompareTo(prio2);
            if (comp != 0) return comp;

            comp = data1.shape.CompareTo(data2.shape);
            if (comp != 0) return comp;

            if (data1.price != data2.price)
            {
                if (data1.price == 2) return 1;
                if (data2.price == 2) return -1;

                return data1.price.CompareTo(data2.price);
            }

            return item1.id.CompareTo(item2.id);
        }

        private void ApplyEquipCustomSort(EquipUI equipUI)
        {
            try
            {
                FieldInfo partField = typeof(EquipUI).GetField("currentEquipPart", BindingFlags.NonPublic | BindingFlags.Instance);
                if (partField == null) return;

                int currentPart = (int)partField.GetValue(equipUI);
                if (currentPart < 0 || currentPart > 4) return;

                FieldInfo listField = typeof(EquipUI).GetField("itemIdList", BindingFlags.NonPublic | BindingFlags.Instance);
                if (listField == null) return;

                List<List<FF9ITEM>> itemIdList = (List<List<FF9ITEM>>)listField.GetValue(equipUI);
                List<FF9ITEM> currentList = itemIdList[currentPart];

                if (currentList == null || currentList.Count <= 1) return;

                bool needsSorting = false;
                for (int i = 0; i < currentList.Count - 1; i++)
                {
                    if (CompareEquipItems(currentList[i], currentList[i + 1]) > 0)
                    {
                        needsSorting = true;
                        break;
                    }
                }

                if (needsSorting)
                {
                    currentList.Sort(CompareEquipItems);

                    FieldInfo scrollListField = typeof(EquipUI).GetField("equipSelectScrollList", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (scrollListField != null)
                    {
                        RecycleListPopulator scrollList = (RecycleListPopulator)scrollListField.GetValue(equipUI);

                        List<ListDataTypeBase> equipTable = new List<ListDataTypeBase>();
                        foreach (FF9ITEM itemData in currentList)
                        {
                            equipTable.Add(new EquipUI.EquipInventoryListData { ItemData = itemData });
                        }

                        scrollList.SetOriginalData(equipTable);
                        Log.Message($"[TranceSeekEquip] Custom shape/price sort applied to equipment part {currentPart}.");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[TranceSeekEquip] Error during custom equip arrange : {ex.Message}");
            }
        }

        /*private static readonly List<RegularItem> ManualPriorityItems = new List<RegularItem>
        {
            RegularItem.Potion,
            RegularItem.HiPotion,
            TranceSeekRegularItem.UltraPotion,
            RegularItem.Ether,
            TranceSeekRegularItem.HiEther,
            RegularItem.PhoenixDown,
            RegularItem.PhoenixPinion,
            RegularItem.Antidote,
            RegularItem.EyeDrops,
            RegularItem.EchoScreen,
            RegularItem.Soft,
            RegularItem.Annoyntment,
            RegularItem.MagicTag,
            RegularItem.Vaccine,
            RegularItem.Remedy,
            TranceSeekRegularItem.HiRemedy,
            RegularItem.Tent,
            RegularItem.Ore,
            RegularItem.Peridot,
            RegularItem.Topaz,
            RegularItem.Opal,
            RegularItem.Sapphire,
            RegularItem.Ruby,
            RegularItem.Amethyst,
            RegularItem.Aquamarine,
            RegularItem.Garnet,
            RegularItem.LapisLazuli,
            RegularItem.Emerald,
            RegularItem.Diamond,
            RegularItem.Moonstone
        };

        // 2. Méthode utilitaire pour définir les grands groupes de tri
        private int GetCategoryPriority(RegularItem id, ItemType type)
        {
            if (ManualPriorityItems.Contains(id)) return 0; // Priorité absolue
            if ((type & ItemType.Weapon) != 0) return 1;    // Armes
            if ((type & ItemType.Armlet) != 0) return 2;    // Bracelets
            if ((type & ItemType.Helmet) != 0) return 3;    // Casques
            if ((type & ItemType.Armor) != 0) return 4;     // Armures
            if ((type & ItemType.Accessory) != 0) return 5; // Accessoires

            return 6; // Tout le reste (Items normaux, Gemmes, etc.)
        }

        private void ApplyCustomSort(ItemUI itemUI)
        {
            try
            {
                FF9StateSystem.Common.FF9.item.Sort((item1, item2) =>
                {
                    if (item1.id == item2.id) return 0;

                    RegularItem id1 = item1.id;
                    RegularItem id2 = item2.id;
                    FF9ITEM_DATA data1 = ff9item._FF9Item_Data[id1];
                    FF9ITEM_DATA data2 = ff9item._FF9Item_Data[id2];

                    // --- ETAPE 1 : Regroupement par Catégorie Principale ---
                    int cat1 = GetCategoryPriority(id1, data1.type);
                    int cat2 = GetCategoryPriority(id2, data2.type);

                    if (cat1 != cat2)
                        return cat1.CompareTo(cat2); // Place les groupes dans l'ordre (0, puis 1, puis 2...)

                    // --- ETAPE 2 : Tri spécifique au sein d'une même catégorie ---
                    switch (cat1)
                    {
                        case 0: // Liste Manuelle
                            int index1 = ManualPriorityItems.IndexOf(id1);
                            int index2 = ManualPriorityItems.IndexOf(id2);
                            return index1.CompareTo(index2); // Respecte l'ordre exact de ta liste

                        case 1: // Armes : Shape puis Power
                            if (data1.shape != data2.shape)
                                return data1.shape.CompareTo(data2.shape);

                            // On récupère la puissance (safe-check via null conditionnel au cas où)
                            int power1 = ff9item.GetItemWeapon(id1)?.Ref.Power ?? 0;
                            int power2 = ff9item.GetItemWeapon(id2)?.Ref.Power ?? 0;
                            // Tri décroissant pour la puissance (les plus fortes en premier)
                            if (power1 != power2) return power2.CompareTo(power1);
                            break;

                        case 2: // Bracelets : Prix de vente
                        case 3: // Casques : Prix de vente
                        case 4: // Armures : Prix de vente
                            if (data1.selling_price != data2.selling_price)
                                return data2.selling_price.CompareTo(data1.selling_price); // Tri décroissant (plus chers en premier)
                            break;

                        case 5: // Accessoires : Shape puis Prix de vente
                            if (data1.shape != data2.shape)
                                return data1.shape.CompareTo(data2.shape);

                            if (data1.selling_price != data2.selling_price)
                                return data2.selling_price.CompareTo(data1.selling_price); // Tri décroissant
                            break;

                        case 6:
                            int typeComp = data1.type.CompareTo(data2.type);
                            if (typeComp != 0) return typeComp;

                            int countComp = item2.count.CompareTo(item1.count);
                            if (countComp != 0) return countComp;
                            break;
                    }

                    return id1.CompareTo(id2);
                });

                MethodInfo displayItemMethod = typeof(ItemUI).GetMethod("DisplayItem", BindingFlags.NonPublic | BindingFlags.Instance);
                if (displayItemMethod != null)
                {
                    displayItemMethod.Invoke(itemUI, null);
                    Log.Message("[TranceSeekItem] Custom auto-arrange with categories applied.");
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[TranceSeekItem] Error during custom arrange : {ex.Message}");
            }
        }*/
    }
}
