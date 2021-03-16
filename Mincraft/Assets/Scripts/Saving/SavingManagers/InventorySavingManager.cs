using System;
using System.IO;
using System.Linq;
using System.Text;
using Core.Managers;
using UnityEngine;

namespace Core.Saving
{
    public class InventorySavingManager : SavingManager
    {
        public override void Save(SavingContext context)
        {
            ItemData[] items = ((InventorySavingContext) context).Items;
            int quickbarIndex = ((InventorySavingContext) context).SelectedQuickbarIndex;
            
            string inventoryPath = Path.Combine(GameManager.CurrentWorldPath, "Inventory.json");

            ItemData[] itemsToSave = items.Where(i => i != null && i.Amount != 0 && i.ItemID != 0).ToArray();
            InventoryLoadingContext<ItemData, int> data = new InventoryLoadingContext<ItemData, int> {items = itemsToSave, additionalData = quickbarIndex };
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
                        InventoryLoadingContext<ItemData, int> inventoryLoadingContext = JsonUtility.FromJson<InventoryLoadingContext<ItemData, int>>(json);
                        outputContext = inventoryLoadingContext;

                        ((InventoryLoadingContext<ItemData, int>) outputContext).items = ((InventoryLoadingContext<ItemData, int>) outputContext).items.Where(t => t.Amount != 0 && t.ItemID != 0).ToArray();
                        ((InventoryLoadingContext<ItemData, int>) outputContext).additionalData = ((InventoryLoadingContext<ItemData, int>) outputContext).additionalData;
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
        public class InventoryLoadingContext<T, K> : OutputContext
        {
            public T[] items;
            public K additionalData;
        }
    }

    public struct InventoryFileIdentifier : FileIdentifier
    {
        
    }
    
    public class InventorySavingContext : SavingContext
    {
        public ItemData[] Items { get; set; }
        public int SelectedQuickbarIndex { get; set; }

        public InventorySavingContext(ItemData[] items, int selectedQuickbarIndex)
        {
            this.Items = items;
            this.SelectedQuickbarIndex = selectedQuickbarIndex;
        }
    }
}