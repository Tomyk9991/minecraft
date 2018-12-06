using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public static class ChunkDictionary
{
    private static Dictionary<Vector3, Transform> dictionary = new Dictionary<Vector3, Transform>();

    public static void Add(Vector3 key, Transform value)
    {
        dictionary.Add(key, value);
    }

    public static void Remove(Vector3 key) => dictionary.Remove(key);
    public static Transform GetValue(Vector3 key) => dictionary[key];
    public static void Clear() => dictionary.Clear();
}
