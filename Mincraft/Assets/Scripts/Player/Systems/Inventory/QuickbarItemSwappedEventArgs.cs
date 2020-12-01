using System;
using Core.Saving;

namespace Core.Player.Systems.Inventory
{
    public class QuickbarItemSwappedEventArgs : EventArgs
    {
        public ItemData[] Items { get; private set; }
        public ItemData OldItem { get; private set; }
        public ItemData NewItem { get; private set; }

        public QuickbarItemSwappedEventArgs(ItemData[] items, ItemData oldItem, ItemData newItem)
        {
            Items = items;
            OldItem = oldItem;
            NewItem = newItem;
        }
    }
}