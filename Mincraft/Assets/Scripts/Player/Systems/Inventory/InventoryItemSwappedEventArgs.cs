using System;
using Core.Saving;
using Utilities;

namespace Core.Player.Systems.Inventory
{
    public class InventoryItemSwappedEventArgs : EventArgs
    {
        public Array2D<ItemData> Items { get; }
        public ItemData OldItem { get; }
        public ItemData NewItem { get; }

        public InventoryItemSwappedEventArgs(Array2D<ItemData> items, ItemData oldItem, ItemData newItem)
        {
            this.Items = items;
            this.OldItem = oldItem;
            this.NewItem = newItem;
        }
    }
}