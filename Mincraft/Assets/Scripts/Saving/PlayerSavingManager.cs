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
            int quickbarIndex = ((PlayerSavingContext) context).SelectedQuickbarIndex;
            
            string inventoryPath = Path.Combine(GameManager.CurrentWorldPath, "Inventory.json");

            ItemData[] itemsToSave = items.Where(i => i != null && i.Amount != 0 && i.ItemID != 0).ToArray();
            Wrapper<ItemData, int> data = new Wrapper<ItemData, int> {items = itemsToSave, additionalData = quickbarIndex };
            File.WriteAllBytes(inventoryPath, Encoding.UTF8.GetBytes(JsonUtility.ToJson(data, true)));
        }

        public override bool Load(FileIdentifier fileIdentifier, out OutputContext outputContext)
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
                        Wrapper<ItemData, int> wrapper = JsonUtility.FromJson<Wrapper<ItemData, int>>(json);
                        outputContext = wrapper;

                        ((Wrapper<ItemData, int>) outputContext).items = ((Wrapper<ItemData, int>) outputContext).items.Where(t => t.Amount != 0 && t.ItemID != 0).ToArray();
                        ((Wrapper<ItemData, int>) outputContext).additionalData = ((Wrapper<ItemData, int>) outputContext).additionalData;
                        return true;
                    }
                    catch (Exception)
                    {
                        Debug.Log("Formatting went wrong");
                    }
                }
            }

            outputContext = null;
            return false;
        }

        [Serializable]
        public class Wrapper<T, K> : OutputContext
        {
            public T[] items;
            public K additionalData;
        }
    }

    public struct InventoryFileIdentifier : FileIdentifier
    {
        
    }
    
    public class PlayerSavingContext : SavingContext
    {
        public ItemData[] Items { get; set; }
        public int SelectedQuickbarIndex { get; set; }

        public PlayerSavingContext(ItemData[] items, int selectedQuickbarIndex)
        {
            this.Items = items;
            this.SelectedQuickbarIndex = selectedQuickbarIndex;
        }
    }
}