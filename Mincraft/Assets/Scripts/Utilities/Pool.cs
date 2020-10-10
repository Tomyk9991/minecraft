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
        private Func<T> _newInstanceInstantiateFunc;
        
        public Pool(int capacity, Func<T> instantiateFunc)
        {
            this._newInstanceInstantiateFunc = instantiateFunc;
            pool = new Queue<T>(capacity);

            for (int i = 0; i < capacity; i++)
            {
                this.Add(_newInstanceInstantiateFunc());
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
                return _newInstanceInstantiateFunc();
            }

            return pool.Dequeue();
        }

        public void Clear() => pool.Clear();
    }
}
