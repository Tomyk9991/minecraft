using System;
using System.Collections;
using System.Collections.Generic;
using Core.Builder;
using UnityEngine;

namespace Utilities
{
    public class Pool<T>
    {
        public int Count => pool.Count;
        
        private Queue<T> pool;
        private Func<T> newInstanceFunc;
        
        public Pool(int capacity, Func<T> func)
        {
            this.newInstanceFunc = func;
            pool = new Queue<T>(capacity);

            for (int i = 0; i < capacity; i++)
            {
                this.Add(newInstanceFunc());
            }
            
        }

        public void Add(T item)
        {
            pool.Enqueue(item);
        }


        public T GetNext()
        {
            if (pool.Count <= 0)
            {
                Debug.Log("New instantiation in pool");
                return newInstanceFunc();
            }

            return pool.Dequeue();
        }

        public void Clear() => pool.Clear();
    }
}
