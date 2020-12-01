using System;
using Core.Saving;
using Utilities;

namespace Core.Player.Systems.Inventory
{
    public class InventoryItemChangedEventArgs : EventArgs
    {
        public Array2D<ItemData> Items { get; }
        public ItemData Item { get; }

        public InventoryItemChangedEventArgs(Array2D<ItemData> items, ItemData item)
        {
            this.Items = items;
            this.Item = item;
        }
    }
}