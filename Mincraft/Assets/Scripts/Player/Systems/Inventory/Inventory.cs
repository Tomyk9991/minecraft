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
        public event Action<ItemChangedEventArgs> OnItemAmountChanged;
        public event Action<ItemChangedEventArgs> OnNewItem;
        public event Action<ItemChangedEventArgs> OnItemDeleted;
        public event Action<ItemSwappedEventArgs> OnSwapItems;
        public event Action<ItemMovedEventArgs> OnItemMoved;

        [SerializeField] private int width = 7;
        [SerializeField] private int height = 8;
        [SerializeField] private int maxStackSize = 128;

        private Array2D<ItemData> items;

        private void Start()
        {
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

        private void OnApplicationQuit()
        {
            PlayerSavingManager.SaveInventory(items.Data);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                InventoryUI.Instance.OnRequestRedraw(new InventoryRedrawEventArgs(items));
            }
        }
        
        public void AddBlockToInventory(Block block)
        {
            AddToInventory((int) block.ID, 1);
        }

        public void Swap(int oldX, int oldY, int newX, int newY)
        {
            Debug.Log("Swapping: old: (" + oldX + " | " + oldY + ") new: (" + newX + " | " + newY + ")");
        }

        public void MoveItem(int oldX, int oldY, int newX, int newY)
        {
            Debug.Log("Moving: old: (" + oldX + " | " + oldY + ") new: (" + newX + " | " + newY + ")");

            if (oldX == -1 || oldY == -1)
            {
                Debug.Log("Invalid position");
                return;
            }
            
            OnItemMoved?.Invoke(new ItemMovedEventArgs(items, items[oldX, oldY], newX, newY));

            items[newX, newY] = new ItemData(items[oldX, oldY].ItemID, newX, newY,
                items[oldX, oldY].Amount, items[oldX, oldY].CurrentGameObject);

            items[oldX, oldY] = null;
        }

        public void Drop(int x, int y)
        {
            Debug.Log("Dropping: " + x + " " + y);
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
                            ItemData item = new ItemData(currentID, x, y, currentAmount + amount, items[x, y].CurrentGameObject);
                            items[x, y] = item;

                            OnItemAmountChanged?.Invoke(new ItemChangedEventArgs(items, item));
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
            ItemData itm = new ItemData(itemID, firstEmptyX, firstEmptyY, amount, null);
            items[firstEmptyX, firstEmptyY] = itm;
            OnNewItem?.Invoke(new ItemChangedEventArgs(items, itm));
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

        public ItemData this[int x, int y] => items[x, y];
    }
    
    public class InventoryRedrawEventArgs : EventArgs
    {
        public Array2D<ItemData> Items { get; private set; }

        public InventoryRedrawEventArgs(Array2D<ItemData> items)
        {
            this.Items = items;
        }
    }

    public class ItemSwappedEventArgs : EventArgs
    {
        public Array2D<ItemData> Items { get; private set; }
        public ItemData OldItem { get; private set; }
        public ItemData NewData { get; private set; }

        public ItemSwappedEventArgs(Array2D<ItemData> items, ItemData oldItem, ItemData newData)
        {
            this.Items = items;
            this.OldItem = oldItem;
            this.NewData = newData;
        }
    }

    public class ItemChangedEventArgs : EventArgs
    {
        public Array2D<ItemData> Items { get; private set; }
        public ItemData Item { get; private set; }

        public ItemChangedEventArgs(Array2D<ItemData> items, ItemData item)
        {
            this.Items = items;
            this.Item = item;
        }
    }
    
    public class ItemMovedEventArgs : EventArgs
    {
        public Array2D<ItemData> Items { get; private set; }
        public ItemData Item { get; private set; }
        
        public int X { get; private set; }
        public int Y { get; private set; }

        public ItemMovedEventArgs(Array2D<ItemData> items, ItemData item, int newX, int newY)
        {
            this.Items = items;
            this.Item = item;
            this.X = newX;
            this.Y = newY;
        }
    }
}