using System.Collections.Generic;
using UnityEngine;

public class ChunkDictionary
{    
    private static Dictionary<Int3, IChunk> dictionary = new Dictionary<Int3, IChunk>();

    public static void Add(Int3 key, IChunk value) => dictionary.Add(key, value);
    public static void Remove(Int3 key) => dictionary.Remove(key);

    public static IChunk GetValue(Int3 key) => dictionary.TryGetValue(key, out IChunk value) ? value : null;
    public static void Clear() => dictionary.Clear();
    
    public static List<IChunk> GetActiveChunks()
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
