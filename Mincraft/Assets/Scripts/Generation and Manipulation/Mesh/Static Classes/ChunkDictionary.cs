using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class ChunkDictionary
{
    private static ConcurrentDictionary<Int3, Chunk> dictionary = new ConcurrentDictionary<Int3, Chunk>();

    public static void Add(Int3 key, Chunk value)
    {
        if (!dictionary.TryAdd(key, value))
        {
            throw new Exception($"Added an item {key}, that already exists");
        }

        Count++;
    }

    public static int Count { get; private set; }

    public static void Remove(Int3 key)
    {
        if (!dictionary.TryRemove(key, out var value))
            throw new Exception($"Removing an item {key}, that does not exist");
        Count--;
    }

    public static Chunk GetValue(Int3 key) => dictionary.TryGetValue(key, out Chunk value) ? value : null;

    public static void Clear()
    {
        dictionary.Clear();
        Count = 0;
    }
    
    public static List<Chunk> GetActiveChunks()
    {        
        var temp = new List<Chunk>();
        
        foreach (var chunk in dictionary.Values)
        {
            temp.Add(chunk);
        }

        return temp;
    }
}

public static class HashSetPositionChecker
{
    private static HashSet<Int3> hashSet = new HashSet<Int3>();

    public static int Count { get; private set; }

    public static void Add(Int3 item)
    {
        if(!hashSet.Add(item))
            throw new Exception($"Added an item {item} to the Hashset, that already exists.");

        Count++;
    }
    public static bool Contains(Int3 item) => hashSet.Contains(item);

    public static void Remove(Int3 item)
    {
        hashSet.Remove(item);
        Count--;
    }
}
