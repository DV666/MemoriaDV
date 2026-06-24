using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Memoria.Prime;
using Memoria.Data; // Nécessaire pour RegularItem

namespace Memoria.Scripts.TranceSeek
{
    public class TranceSeekHackShop : MonoBehaviour
    {
        private ShopUI _lastSortedShop = null;

        private void Update()
        {
            ShopUI shop = UnityEngine.Object.FindObjectOfType<ShopUI>();

            if (shop != null && shop.isActiveAndEnabled)
            {
                FieldInfo typeField = typeof(ShopUI).GetField("type", BindingFlags.NonPublic | BindingFlags.Instance);
                if (typeField != null)
                {
                    ShopUI.ShopType shopType = (ShopUI.ShopType)typeField.GetValue(shop);

                    if (shopType == ShopUI.ShopType.Synthesis && _lastSortedShop != shop)
                    {
                        _lastSortedShop = shop;
                        SortAndRefreshSynthesisShop(shop);
                    }
                }
            }
            else
            {
                _lastSortedShop = null;
            }
        }

        private void SortAndRefreshSynthesisShop(ShopUI shop)
        {
            try
            {
                FieldInfo mixListField = typeof(ShopUI).GetField("mixItemList", BindingFlags.NonPublic | BindingFlags.Instance);
                if (mixListField == null) return;

                System.Collections.IList mixItemList = (System.Collections.IList)mixListField.GetValue(shop);
                if (mixItemList == null || mixItemList.Count == 0) return;

                List<object> rawList = new List<object>();
                foreach (object item in mixItemList)
                {
                    rawList.Add(item);
                }

                List<object> sortedList = rawList.OrderBy(mixData =>
                {
                    if (mixData == null) return 0;

                    Type type = mixData.GetType();
                    RegularItem resultItem = RegularItem.NoItem;

                    PropertyInfo prop = type.GetProperty("Result", BindingFlags.Public | BindingFlags.Instance);
                    if (prop != null)
                    {
                        resultItem = (RegularItem)prop.GetValue(mixData, null);
                    }
                    else
                    {
                        FieldInfo field = type.GetField("Result", BindingFlags.Public | BindingFlags.Instance);
                        if (field != null)
                            resultItem = (RegularItem)field.GetValue(mixData);
                    }

                    if (ff9item._FF9Item_Data != null)
                    {
                        var itemData = ff9item._FF9Item_Data[resultItem];
                        if (itemData != null)
                        {
                            return itemData.shape;
                        }
                    }
                    return 0;
                }).ToList();

                mixItemList.Clear();
                foreach (object sortedItem in sortedList)
                {
                    mixItemList.Add(sortedItem);
                }

                MethodInfo setShopTypeMethod = typeof(ShopUI).GetMethod("SetShopType", BindingFlags.NonPublic | BindingFlags.Instance);
                if (setShopTypeMethod != null)
                {
                    setShopTypeMethod.Invoke(shop, new object[] { ShopUI.ShopType.Synthesis });
                    Log.Message($"[TranceSeekShop] The synth shop n°{shop.Id} has been sorted (by shape).");
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[TranceSeekShop] Error when sorting the synth shop n°{shop.Id} : {ex.Message}");
            }
        }
    }
}
