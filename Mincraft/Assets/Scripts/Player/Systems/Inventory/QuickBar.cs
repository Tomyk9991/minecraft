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
        private int size;

        public ItemData[] Items => items;

        public QuickBar(int size = 10)
        {
            this.items = new ItemData[size];
            this.size = 10;
        }

        public QuickBar(Inventory inventory, ItemData[] data)
        {
            this.items = data;
            this.size = data.Length;
        }

        public ItemData this[int index]
        {
            get => items[index];
            set => items[index] = value;
        }
    }
}