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
    }
}
