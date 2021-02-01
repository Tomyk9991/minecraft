using System;
using System.Collections.Generic;
using Core.Saving;
using Extensions;
using UnityEngine;

namespace Core.Player.Systems.Inventory
{
    public class Inventory : SingletonBehaviour<Inventory>
    {
        public event Action<int> OnCriticalSizeExceeded;
        public int Width => width;
        public int Height => height;
        
        [SerializeField] private int width = 7;
        [SerializeField] private int height = 8;
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