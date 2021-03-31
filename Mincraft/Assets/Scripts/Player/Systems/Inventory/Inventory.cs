using System;
using System.Collections.Generic;
using Core.Builder;
using Core.Saving;
using Core.UI.Ingame;
using Extensions;
using UnityEngine;

namespace Core.Player.Systems.Inventory
{
    public class Inventory : SingletonBehaviour<Inventory>
    {
        [Header("References")]
        [SerializeField] private InventoryUI inventoryUI = null;
        
        public event Action<int> OnCriticalSizeExceeded;
        
        [Header("Inventory settings")]
        [SerializeField] private int maxStackSize = 128;
        
        public List<ItemData> Items { get; private set; }
        public QuickBar QuickBar { get; private set; }
        
        private void Start()
        {
            Items = new List<ItemData>();
            
            LoadInventory();
            LoadQuickbar();
        }

        private void LoadInventory()
        {
            if (ResourceIO.LoadCached<Inventory>(new InventoryFileIdentifier(), out OutputContext context))
            {
                ItemData[] itemData = ((InventorySavingManager.InventoryLoadingContext<ItemData, int>) context).items;
                if (itemData != null && itemData.Length != 0)
                {
                    foreach (var data in itemData)
                    {
                        Items.Add(data);
                        int newLen = Items.Count;
                        
                        OnCriticalSizeExceeded?.Invoke(newLen >= 153 ? 1 : 0);
                    }
                }
            }
        }

        public void AddBlockToInventory(Block block, int amount)
        {
            if (TryFindItem(block, out ItemData data))
            {
                data.Amount += amount;
                inventoryUI.ItemAmountChanged(data);
            }
            else
            {
                var newItem = new ItemData((int) block.ID, -1, amount, null);
                Items.Add(newItem);
                inventoryUI.ItemCreated(newItem);
            }
        }

        private bool TryFindItem(Block block, out ItemData data)
        {
            bool found = false;
            data = null;

            foreach (var item in Items)
            {
                if (item.ItemID == (int) block.ID)
                {
                    found = true;
                    data = item;
                }
            }

            return found;
        }

        private void LoadQuickbar()
        {
            this.QuickBar = new QuickBar();
            
            foreach (var itemData in Items)
            {
                int quickbarIndex = itemData.QuickbarIndex;
                
                if (quickbarIndex != -1)
                    this.QuickBar[quickbarIndex] = itemData;
            }
        }
        
        public void OnApplicationQuit()
        {
            ResourceIO.Save<Inventory>(new InventorySavingContext(Items.ToArray(), QuickBarSelectionUI.Instance.SelectedIndex));
        }
    }
}