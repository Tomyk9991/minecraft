using System;
using Core.Saving;
using Utilities;

namespace Core.Player.Systems.Inventory
{
    public class InventoryRedrawEventArgs : EventArgs
    {
        public Array2D<ItemData> Items { get; private set; }

        public InventoryRedrawEventArgs(Array2D<ItemData> items)
        {
            this.Items = items;
        }
    }
}