using System;
using Core.Math;
using Core.Saving;
using Core.UI.Ingame;
using UnityEngine;

namespace Core.Player.Systems.Inventory
{
    public class QuickBar
    {
        public ItemData[] items;
        private Inventory inventory;
        private InventoryUI inventoryUI;
        private int size;

        public ItemData[] Items => items;

        public event Action<QuickbarRedrawEventArgs> OnRequestRedraw;
        public event Action<QuickbarItemChangedEventArgs> OnItemAmountChanged;
        public event Action<QuickbarItemChangedEventArgs> OnItemDropped;
        public event Action<QuickbarItemSwappedEventArgs> OnSwapItems;
        public event Action<QuickbarItemMovedEventArgs> OnItemMoved;

        public QuickBar(Inventory inventory, int size = 10)
        {
            this.items = new ItemData[size];
            this.inventory = inventory;
            this.size = 10;
            this.inventoryUI = InventoryUI.Instance;
            //No need to request a redraw, since items are zero
        }

        public QuickBar(Inventory inventory, ItemData[] data)
        {
            this.items = data;
            this.inventory = inventory;
            this.size = data.Length;
            this.inventoryUI = InventoryUI.Instance;

            OnRequestRedraw?.Invoke(new QuickbarRedrawEventArgs(items));
        }

        public ItemData this[int index]
        {
            get => items[index];
            set => items[index] = value;
        }

        public void Swap(Int2 oldMultiIndex, int newIndex)
        {
            if (inventoryUI.Logs)
                Debug.Log("Swapping: old: (" + oldMultiIndex.X + " | " + oldMultiIndex.X + ") new: (" + newIndex + ")");

            if (!ValidPosition(newIndex))
            {
                Debug.Log("Invalid position");
                return;
            }

            ItemData oldItem = inventory[oldMultiIndex.X, oldMultiIndex.Y];
            ItemData newItem = items[newIndex];

            OnSwapItems.Invoke(new QuickbarItemSwappedEventArgs(items, oldItem, newItem));

            //Swap
            ItemData temp = oldItem;

            inventory[oldMultiIndex.X, oldMultiIndex.X] = items[newIndex];
            inventory[oldMultiIndex.X, oldMultiIndex.Y].SetXY(oldMultiIndex.X, oldMultiIndex.Y);
            inventory[oldMultiIndex.X, oldMultiIndex.Y].SetIndex(-1);

            items[newIndex] = temp;
            items[newIndex].SetXY(-1, -1);
            items[newIndex].SetIndex(newIndex);
        }

        public void Swap(int oldIndex, int newIndex)
        {
            if (inventoryUI.Logs)
                Debug.Log("Swapping: old: (" + oldIndex + ") new: (" + newIndex + ")");

            if (!ValidPosition(newIndex))
            {
                Debug.Log("Invalid position");
                return;
            }

            if (SamePosition(oldIndex, newIndex))
            {
                Debug.Log("Same position");
                return;
            }

            ItemData oldItem = items[oldIndex];
            ItemData newItem = items[newIndex];

            OnSwapItems.Invoke(new QuickbarItemSwappedEventArgs(items, oldItem, newItem));

            //Swap
            ItemData temp = oldItem;

            items[oldIndex] = items[newIndex];
            items[oldIndex].SetIndex(oldIndex);

            items[newIndex] = temp;
            items[newIndex].SetIndex(newIndex);
        }

        public void MoveItem(Int2 oldIndex, int newIndex)
        {
            if (inventoryUI.Logs)
                Debug.Log("Moving: old: (" + oldIndex.X + " | " + oldIndex.Y + ") new: (" + newIndex + ")");

            if (!ValidPosition(newIndex))
            {
                Debug.Log("Invalid position");
                return;
            }

            OnItemMoved?.Invoke(new QuickbarItemMovedEventArgs(items, inventory[oldIndex.X, oldIndex.Y], newIndex));
            items[newIndex] = new ItemData(inventory[oldIndex.X, oldIndex.Y].ItemID, -1, -1, newIndex,
                inventory[oldIndex.X, oldIndex.Y].Amount, inventory[oldIndex.X, oldIndex.Y].CurrentGameObject);

            inventory[oldIndex.X, oldIndex.Y] = null;
        }

        public void MoveItem(int oldIndex, int newIndex)
        {
        }

        public void Drop(int index)
        {
        }

        public int IndexFromGameObject(GameObject go)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] != null && items[i].CurrentGameObject == go)
                {
                    return i;
                }
            }

            return -1;
        }

        private bool ValidPosition(int index) => index >= 0 && index < size;
        private bool SamePosition(int a, int b) => a == b;

        public void RaiseOnRequestRedraw()
        {
            OnRequestRedraw?.Invoke(new QuickbarRedrawEventArgs(items));
        }
    }

    public class QuickbarItemChangedEventArgs : EventArgs
    {
    }
}