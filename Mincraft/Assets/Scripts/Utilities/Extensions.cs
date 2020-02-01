using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

using Core.Math;

namespace Extensions
{
    public static class Extensions
    {
        public static Int3 ToInt3(this Vector3 pos)
            => Int3.ToInt3(pos);
        
        public static void Clear<T>(this ConcurrentQueue<T> queue)
        {
            T item;
            while (queue.TryDequeue(out item))
            {
            }
        }

        public static T RemoveAndGet<T>(this IList<T> list, int index)
        {
            T result = list[index];
            list.RemoveAt(index);
            return result;
        }
    }
}
