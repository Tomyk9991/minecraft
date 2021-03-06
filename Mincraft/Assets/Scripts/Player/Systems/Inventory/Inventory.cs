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
            if (PlayerSavingManager.LoadInventory(out ItemData[] itemData))
            {
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
            if (PlayerSavingManager.LoadQuickBar(out ItemData[] quickBarItemData))
            {
                if (quickBarItemData != null && quickBarItemData.Length != 0)
                    QuickBar = new QuickBar(this, quickBarItemData);
                else
                    QuickBar = new QuickBar();
            }
            else
                QuickBar = new QuickBar();
        }
        
        private void OnApplicationQuit()
        {
            PlayerSavingManager.SaveInventory(Items.ToArray());
            PlayerSavingManager.SaveQuickbar(QuickBar.items);
        }
    }
}