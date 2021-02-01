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
        
        
        public static void SetLeft(this RectTransform rt, float left)
        {
            rt.offsetMin = new Vector2(left, rt.offsetMin.y);
        }
 
        public static void SetRight(this RectTransform rt, float right)
        {
            rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
        }
 
        public static void SetTop(this RectTransform rt, float top)
        {
            rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
        }
 
        public static void SetBottom(this RectTransform rt, float bottom)
        {
            rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
        }
        
    }
}
