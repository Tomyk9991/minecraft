using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public static class ChunkDictionary
{
    private static Dictionary<Vector3Int, Vector3Int> dictionary = new Dictionary<Vector3Int, Vector3Int>();

    public static void Add(Vector3Int key, Vector3Int value) => dictionary.Add(key, value);
    public static void Remove(Vector3Int key) => dictionary.Remove(key);
    public static Vector3Int GetValue(Vector3Int key) => dictionary[key];
    public static void Clear() => dictionary.Clear();
}
