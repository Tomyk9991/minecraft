using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    public class Pool<T>
    {
        public int Count => pool.Count;
        
        private Queue<T> pool;
        
        public Pool(int capacity)
        { 
            pool = new Queue<T>(capacity);
        }

        public void Add(T item)
        {
            // pool.Enqueue(item);
            // Debug.Log("new Chunk");
        }


        public T GetNext()
        {
            if (pool.Count <= 0)
                return default;

            return pool.Dequeue();
        }

        public void Clear() => pool.Clear();
    }
}
