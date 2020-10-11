using System;
using Core.Builder;
using Core.Player.Interaction;
using Core.Saving;
using Extensions;
using UnityEngine;
using Utilities;

namespace Core.Player.Systems.Inventory
{
    public class Inventory : SingletonBehaviour<Inventory>
    {
        public int Width => width;
        public int Height => height;
        public event Action<ItemChangedEventArgs> OnInventoryChanged;

        [SerializeField] private int width = 7;
        [SerializeField] private int height = 8;
        [SerializeField] private int maxStackSize = 128;

        private Array2D<ItemData> items;

        private void Start()
        {
            items = new Array2D<ItemData>(width, height);
            if (PlayerSavingManager.Load(out ItemData[] itemData))
            {
                items.Data = itemData;
            }

            if (OnInventoryChanged == null)
                Debug.Log("ist noch null :sadge:");
            OnInventoryChanged?.Invoke(new ItemChangedEventArgs(items, ItemData.Empty, false, false, true));
        }

        private void OnApplicationQuit()
        {
            PlayerSavingManager.Save(items.Data);
        }
        
        public void AddBlockToInventory(Block block)
        {
            AddToInventory((int) block.ID, 1);
        }
        
        public void MoveItemFromTo(int oldX, int oldY, int newX, int newY)
        {
            items[newX, newY] = items[oldX, oldY];
            items[oldX, oldY] = default;
        }

        public void Remove(int x, int y)
        {
            items[x, y] = default;
        }

        public void Swap(int oldX, int oldY, int newX, int newY)
        {
            ItemData temp = items[newX, newY];
            items[newX, newY] = items[oldX, oldY];
            items[oldX, oldY] = temp;

            items[oldX, oldY].SetXY(newX, newY);
            items[newX, newY].SetXY(oldX, oldY);
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
                    int currentID = items[x, y].ItemID;

                    if (!foundEmptySlot && currentID == (int) BlockUV.Air)
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
                            ItemData item = new ItemData(currentID, x, y, currentAmount + amount);
                            items[x, y] = item;

                            OnInventoryChanged?.Invoke(new ItemChangedEventArgs(items, item, true, false, false));
                            return;
                        }
                        else // Current slot is full now. Creating new slot + modifying existing one
                        {
                        }
                    }
                }
            }

            if (!foundEmptySlot) return; //Inventory is full

            ItemData itm = new ItemData(itemID, firstEmptyX, firstEmptyY, amount);
            items[firstEmptyX, firstEmptyY] = new ItemData(itemID, firstEmptyX, firstEmptyY, amount);
            OnInventoryChanged?.Invoke(new ItemChangedEventArgs(items, itm, false, true, false));
        }

        private void Load()
        {
        }

        public ItemData this[int x, int y] => items[x, y];
    }

    public class ItemChangedEventArgs : EventArgs
    {
        public bool ItemSlotModified { get; private set; }
        public bool ItemSlotRequest { get; private set; }
        public bool RequestRedraw { get; private set; }


        public Array2D<ItemData> Items { get; private set; }
        public ItemData Item { get; private set; }

        public ItemChangedEventArgs(Array2D<ItemData> items, ItemData item, bool itemSlotModified, bool itemSlotRequest,
            bool requestRedraw)
        {
            ItemSlotModified = itemSlotModified;
            ItemSlotRequest = itemSlotRequest;
            RequestRedraw = requestRedraw;
            Items = items;
            Item = item;
        }
    }
}