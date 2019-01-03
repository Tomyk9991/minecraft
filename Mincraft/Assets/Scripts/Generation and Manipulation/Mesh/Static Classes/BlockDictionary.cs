using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public static class BlockDictionary
{
    public static Vector3Int NotFoundVectorBasis { get; } = new Vector3Int(0, -500, 0);

    private static Dictionary<Vector3Int, Vector3Int> dictionary = new Dictionary<Vector3Int, Vector3Int>();

    public static void Add(Vector3Int key, Vector3Int value) => dictionary.Add(key, value);
    public static void Remove(Vector3Int key) => dictionary.Remove(key);

    public static Vector3Int GetValue(Vector3Int key)
    {
        Vector3Int value;
        if (dictionary.TryGetValue(key, out value))
        {
            return value;
        }

        return NotFoundVectorBasis;
    }
    public static void Clear() => dictionary.Clear();
}
