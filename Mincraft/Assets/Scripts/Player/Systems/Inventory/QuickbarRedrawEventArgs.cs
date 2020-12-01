using System;
using Core.Saving;

namespace Core.Player.Systems.Inventory
{
    public class QuickbarRedrawEventArgs : EventArgs
    {
        public ItemData[] Items { get; private set; }

        public QuickbarRedrawEventArgs(ItemData[] items)
        {
            Items = items;
        }
    }
}