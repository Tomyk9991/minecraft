using System.Collections.Generic;
using UnityEngine;

public class ChunkDictionary
{
    private static Dictionary<Vector3Int, IChunk> dictionary = new Dictionary<Vector3Int, IChunk>();

    public static void Add(Vector3Int key, IChunk value) => dictionary.Add(key, value);
    public static void Remove(Vector3Int key) => dictionary.Remove(key);

    public static IChunk GetValue(Vector3Int key)
    {
        IChunk value;
        if (dictionary.TryGetValue(key, out value))
        {
            return value;
        }

        return null;
    }
    public static void Clear() => dictionary.Clear();
}

public class ChunkGameObjectDictionary
{
    private static Dictionary<IChunk, GameObject> dictionary = new Dictionary<IChunk, GameObject>();

    public static void Add(IChunk key, GameObject value) => dictionary.Add(key, value);
    public static void Remove(IChunk key) => dictionary.Remove(key);

    public static GameObject GetValue(IChunk key)
    {
        GameObject value;
        if (dictionary.TryGetValue(key, out value))
        {
            return value;
        }

        return null;
    }
    public static void Clear() => dictionary.Clear();
}
