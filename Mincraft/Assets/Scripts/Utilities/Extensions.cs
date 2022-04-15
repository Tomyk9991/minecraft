using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Core.Builder;
using UnityEngine;

using Core.Math;
using Random = System.Random;

namespace Extensions
{
    public static class Extensions
    {
        public static Int3 ToInt3(this Vector3 pos)
            => Int3.ToInt3(pos);

        public static int AddPoint(this LineRenderer renderer, Vector3 position)
        {
            int count = renderer.positionCount;
            renderer.positionCount += 1;

            renderer.SetPosition(count, position);

            return count;
        }

        public static void InsertPoint(this LineRenderer renderer, int index, Vector3 position)
        {
            Vector3[] currentPositions = new Vector3[renderer.positionCount];
            renderer.GetPositions(currentPositions);
            
            Vector3[] newPositions = new Vector3[currentPositions.Length + 1];

            for (int i = 0; i < newPositions.Length; i++)
            {
                if (i < index)
                {
                    newPositions[i] = currentPositions[i];
                }
                else if (i == index)
                {
                    newPositions[i] = position;
                }
                else
                {
                    newPositions[i] = currentPositions[i - 1];
                }
            }

            renderer.positionCount += 1;
            renderer.SetPositions(newPositions);
        }

        public static T[] Shuffle<T>(this T[] arr)
        {
            Random random = new Random();

            return arr.OrderBy(x => random.Next()).ToArray();
        }
        
        public static void Clear<T>(this ConcurrentQueue<T> queue)
        {
            T item;
            while (queue.TryDequeue(out item))
            {
            }
        }

        public static void AddSorted<T>(this List<T> @this, T item) where T : IComparable<T>
        {
            if (@this.Count == 0)
            {
                @this.Add(item);
                return;
            }
            if (@this[@this.Count-1].CompareTo(item) <= 0)
            {
                @this.Add(item);
                return;
            }
            if (@this[0].CompareTo(item) >= 0)
            {
                @this.Insert(0, item);
                return;
            }
            int index = @this.BinarySearch(item);
            if (index < 0) 
                index = ~index;
            
            @this.Insert(index, item);
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
