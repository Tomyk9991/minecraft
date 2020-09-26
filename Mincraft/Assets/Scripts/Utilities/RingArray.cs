using System;
using UnityEngine;

namespace Utilities
{
    public class RingArray<T> where T : class
    {
        public int Size { get; private set; }
        private T[] data;
        private int currentIndex;
        private int capacity;

        public RingArray(int capacity)
        {
            this.data = new T[capacity];

            this.capacity = capacity;
            this.currentIndex = 0;
            this.Size = 0;
        }

        public void Add(T item)
        {
            this.data[this.currentIndex++] = item;

            this.Size++;
            if (currentIndex >= capacity - 1)
            {
                currentIndex = 0;
                this.Size = capacity;
            }
        }
    }
}