using System;
using System.IO;
using System.Linq;
using System.Text;
using Core.Managers;
using UnityEngine;

namespace Core.Saving
{
    public class PlayerSavingManager : SavingManager
    {
        public override void Save(SavingContext context)
        {
            ItemData[] items = ((PlayerSavingContext) context).Items;
            string inventoryPath = Path.Combine(GameManager.CurrentWorldPath, "Inventory.json");

            ItemData[] itemsToSave = items.Where(i => i != null && i.Amount != 0 && i.ItemID != 0).ToArray();
            Wrapper<ItemData> data = new Wrapper<ItemData> {items = itemsToSave};
            File.WriteAllBytes(inventoryPath, Encoding.UTF8.GetBytes(JsonUtility.ToJson(data, true)));
        }

        public override bool Load(FileIdentifier fileIdentifier, out OutputContext items)
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
                        items = wrapper;

                        ((Wrapper<ItemData>) items).items = ((Wrapper<ItemData>) items).items.Where(t => t.Amount != 0 && t.ItemID != 0).ToArray();

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
        public class Wrapper<T> : OutputContext
        {
            public T[] items;
        }
    }

    public struct InventoryFileIdentifier : FileIdentifier
    {
        
    }
    
    public class PlayerSavingContext : SavingContext
    {
        public ItemData[] Items { get; set; }

        public PlayerSavingContext(ItemData[] items)
        {
            Items = items;
        }
    }
}