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
    
    public static List<IChunk> GetChunks()
    {
        var temp = new List<IChunk>();
        
        foreach (var chunk in dictionary.Values)
        {
            temp.Add(chunk);
        }

        return temp;
    }
}

public class ChunkGameObjectDictionary
{
    private static Dictionary<GameObject, IChunk> dictionary = new Dictionary<GameObject, IChunk>();

    public static void Add(GameObject key, IChunk value) => dictionary.Add(key, value);
    public static void Remove(GameObject key) => dictionary.Remove(key);

    public static IChunk GetValue(GameObject key)
    {
        IChunk value;
        if (dictionary.TryGetValue(key, out value))
        {
            return value;
        }

        return null;
    }
    public static void Clear() => dictionary.Clear();

    public static List<IChunk> GetChunks()
    {
        var temp = new List<IChunk>();
        
        foreach (var chunk in dictionary.Values)
        {
            temp.Add(chunk);
        }

        return temp;
    }
}
