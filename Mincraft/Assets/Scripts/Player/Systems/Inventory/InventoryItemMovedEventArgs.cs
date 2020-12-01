using System;
using Core.Saving;
using Utilities;

namespace Core.Player.Systems.Inventory
{
    public class InventoryItemMovedEventArgs : EventArgs
    {
        public Array2D<ItemData> Items { get; }
        public ItemData Item { get; }
        
        public int X { get; }
        public int Y { get; }

        public InventoryItemMovedEventArgs(Array2D<ItemData> items, ItemData item, int newX, int newY)
        {
            this.Items = items;
            this.Item = item;
            this.X = newX;
            this.Y = newY;
        }
    }
}