using System;
using System.IO;
using System.Linq;
using System.Text;
using Core.Managers;
using Core.Player.Systems;
using Core.UI.Ingame;
using UnityEngine;
using Utilities;

namespace Core.Saving
{
    public class PlayerSavingManager : MonoBehaviour
    {
        public static void SaveInventory(ItemData[] items)
        {
            string inventoryPath = Path.Combine(GameManager.CurrentWorldPath, "Inventory.json");

            ItemData[] itemsToSave = items.Where(i => i != null && i.Amount != 0 && i.ItemID != 0).ToArray();
            Wrapper<ItemData> data = new Wrapper<ItemData> {items = itemsToSave};
            File.WriteAllBytes(inventoryPath, Encoding.UTF8.GetBytes(JsonUtility.ToJson(data, true)));
        }
        
        public static void SaveQuickbar(ItemData[] items)
        {
            string quickbarPath = Path.Combine(GameManager.CurrentWorldPath, "Quickbar.json");

            ItemData[] itemsToSave = items.Where(i => i != null && i.Amount != 0 && i.ItemID != 0).ToArray();
            Wrapper<ItemData> data = new Wrapper<ItemData> {items = itemsToSave};
            File.WriteAllBytes(quickbarPath, Encoding.UTF8.GetBytes(JsonUtility.ToJson(data, true)));
        }

        public static bool LoadInventory(out ItemData[] items)
        {
            string inventoryPath = Path.Combine(GameManager.CurrentWorldPath, "Inventory.json");
            if (File.Exists(inventoryPath))
            {
                string json = "";
                try
                {
                    json = File.ReadAllText(inventoryPath);
                }
                catch (Exception)
                {
                    Debug.Log($"Could not load inventory from {inventoryPath}");
                }

                if (json != "")
                {
                    try
                    {
                        Wrapper<ItemData> wrapper = JsonUtility.FromJson<Wrapper<ItemData>>(json);
                        items = wrapper.items;

                        items = items.Where(t => t.Amount != 0 && t.ItemID != 0).ToArray();

                        return true;
                    }
                    catch (Exception)
                    {
                        Debug.Log("Formatting went wrong");
                    }
                }
            }

            items = null;
            return false;
        }

        [Serializable]
        public class Wrapper<T>
        {
            public T[] items;
        }

        public static bool LoadQuickBar(out ItemData[] items)
        {
            string quickBarPath = Path.Combine(GameManager.CurrentWorldPath, "Quickbar.json");
            if (File.Exists(quickBarPath))
            {
                string json = "";
                try
                {
                    json = File.ReadAllText(quickBarPath);
                }
                catch (Exception)
                {
                    Debug.Log($"Could not load quickbar inventory from {quickBarPath}");
                }

                if (json != "")
                {
                    try
                    {
                        Wrapper<ItemData> wrapper = JsonUtility.FromJson<Wrapper<ItemData>>(json);
                        items = wrapper.items;

                        items = items.Where(t => t.Amount != 0 && t.ItemID != 0).ToArray();
                        return true;
                    }
                    catch (Exception)
                    {
                        Debug.Log("Formatting went wrong");
                    }
                }
            }

            items = null;
            return false;
        }
    }
}