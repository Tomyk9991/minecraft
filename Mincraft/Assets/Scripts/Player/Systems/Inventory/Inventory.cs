using System;
using Core.Builder;
using Core.Math;
using Core.Saving;
using Core.UI.Ingame;
using Extensions;
using UnityEngine;
using Utilities;

namespace Core.Player.Systems.Inventory
{
    public class Inventory : SingletonBehaviour<Inventory>
    {
        public int Width => width;
        public int Height => height;
        
        public event Action<InventoryRedrawEventArgs> OnRequestRedraw;
        public event Action<InventoryItemChangedEventArgs> OnItemAmountChanged;
        public event Action<InventoryItemChangedEventArgs> OnNewItem;
        public event Action<InventoryItemChangedEventArgs> OnItemDropped;
        public event Action<InventoryItemSwappedEventArgs> OnSwapItems;
        public event Action<InventoryItemMovedEventArgs> OnItemMoved;

        [SerializeField] private int width = 7;
        [SerializeField] private int height = 8;
        [SerializeField] private int maxStackSize = 128;

        private Array2D<ItemData> items;
        private InventoryUI inventoryUI;
        
        public QuickBar QuickBar { get; private set; }

        private void Start()
        {
            inventoryUI = InventoryUI.Instance;
            items = new Array2D<ItemData>(width, height);
            
            if (PlayerSavingManager.LoadInventory(out ItemData[] itemData))
            {
                if (itemData != null && itemData.Length != 0)
                {
                    foreach (var data in itemData)
                    {
                        items[data.x, data.y] = data;
                    }
                }
            }

            OnRequestRedraw?.Invoke(new InventoryRedrawEventArgs(items));
        }
        
        public void InitQuickbar()
        {
            if (PlayerSavingManager.LoadQuickBar(out ItemData[] quickBarItemData))
            {
                if (quickBarItemData != null && quickBarItemData.Length != 0)
                    QuickBar = new QuickBar(this, quickBarItemData);
                else
                    QuickBar = new QuickBar(this);
            }
            else
                QuickBar = new QuickBar(this);
        }

        private void OnApplicationQuit()
        {
            PlayerSavingManager.SaveInventory(items.Data);
            PlayerSavingManager.SaveQuickbar(QuickBar.items);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                InventoryUI.Instance.OnInventoryRequestRedraw(new InventoryRedrawEventArgs(items));
            }
        }
        
        public void AddBlockToInventory(Block block)
        {
            AddToInventory((int) block.ID, 1);
        }

        public void Swap(int oldX, int oldY, int newX, int newY)
        {
            if (inventoryUI.Logs)
                Debug.Log("Swapping: old: (" + oldX + " | " + oldY + ") new: (" + newX + " | " + newY + ")");

            if (!ValidPosition(oldX, oldY))
            {
                Debug.Log("Invalid position");
                return;
            }

            if (SamePosition(oldX, oldY, newX, newY))
            {
                Debug.Log("Same position");
                return;
            }

            ItemData oldItem = items[oldX, oldY];
            ItemData newItem = items[newX, newY];

            OnSwapItems.Invoke(new InventoryItemSwappedEventArgs(items, oldItem, newItem));

            // Swap
            ItemData temp = oldItem;
            
            items[oldX, oldY] = items[newX, newY];
            items[oldX, oldY].SetXY(oldX, oldY);
            
            items[newX, newY] = temp;
            items[newX, newY].SetXY(newX, newY);
        }

        private bool SamePosition(int oldX, int oldY, int newX, int newY) => oldX == newX && oldY == newY;
        
        private bool ValidPosition(int oldX, int oldY) => oldX != -1 && oldY != -1;

        public void MoveItem(int oldX, int oldY, int newX, int newY)
        {
            if (inventoryUI.Logs)
                Debug.Log("Moving: old: (" + oldX + " | " + oldY + ") new: (" + newX + " | " + newY + ")");

            if (!ValidPosition(oldX, oldY))
            {
                Debug.Log("Invalid position");
                return;
            }
            
            OnItemMoved?.Invoke(new InventoryItemMovedEventArgs(items, items[oldX, oldY], newX, newY));

            items[newX, newY] = new ItemData(items[oldX, oldY].ItemID, newX, newY, -1,
                items[oldX, oldY].Amount, items[oldX, oldY].CurrentGameObject);

            items[oldX, oldY] = null;
        }

        public void Drop(int x, int y)
        {
            if (inventoryUI.Logs)
                Debug.Log("Dropping: (" + x + " " + y + ")");

            OnItemDropped?.Invoke(new InventoryItemChangedEventArgs(items, items[x, y]));

            Destroy(items[x, y].CurrentGameObject);
            items[x, y] = null;
        }

        private void AddToInventory(int itemID, int amount)
        {
            int firstEmptyX = -1;
            int firstEmptyY = -1;

            bool foundEmptySlot = false;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int currentID = 0;

                    if (items[x, y] != null)
                        currentID = items[x, y].ItemID;
                    
                    if (!foundEmptySlot && items[x, y] == null)
                    {
                        firstEmptyX = x;
                        firstEmptyY = y;
                        foundEmptySlot = true;
                    } //Keep track of the first empty slot 

                    if (currentID == itemID)
                    {
                        int currentAmount = items[x, y].Amount;
                        int amountDelta = maxStackSize - (currentAmount + amount);

                        if (amountDelta >= 0) //Enough space in this slot
                        {
                            ItemData item = new ItemData(currentID, x, y, -1, currentAmount + amount, items[x, y].CurrentGameObject);
                            items[x, y] = item;

                            OnItemAmountChanged?.Invoke(new InventoryItemChangedEventArgs(items, item));
                            return;
                        }
                        else // Current slot is full now. Creating new slot + modifying existing one
                        {
                        }
                    }
                }
            }

            if (!foundEmptySlot) return; //Inventory is full

            
            //Creating new item
            ItemData itm = new ItemData(itemID, firstEmptyX, firstEmptyY, -1, amount, null);
            items[firstEmptyX, firstEmptyY] = itm;
            OnNewItem?.Invoke(new InventoryItemChangedEventArgs(items, itm));
        }
        
        public Int2 IndexFromGameObject(GameObject go)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] != null && items[i].CurrentGameObject == go)
                {
                    return new Int2(items[i].x, items[i].y);
                }
            }

            return new Int2(-1, -1);
        }

        public ItemData this[int x, int y]
        {
            get => items[x, y];
            set => items[x, y] = value;
        }
    }
}