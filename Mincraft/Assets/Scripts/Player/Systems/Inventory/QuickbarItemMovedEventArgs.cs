using System;
using Core.Saving;

namespace Core.Player.Systems.Inventory
{
    public class QuickbarItemMovedEventArgs : EventArgs
    {
        public ItemData[] Items { get; private set; }
        public ItemData OldItem { get; private set; }
        public int Index { get; private set; }

        public QuickbarItemMovedEventArgs(ItemData[] items, ItemData oldItem, int index)
        {
            Items = items;
            OldItem = oldItem;
            Index = index;
        }
    }
}