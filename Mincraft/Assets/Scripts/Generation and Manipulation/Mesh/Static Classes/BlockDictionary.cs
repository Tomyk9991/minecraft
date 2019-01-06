using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public static class BlockDictionary
{
    public static Vector3Int NotFoundVector3Int { get; } = new Vector3Int(0, -500, 0);
//    private static HashSet<Vector3Int> dictionary = new HashSet<Vector3Int>();

//    private static Vector3Int[,,] array = new Vector3Int[48, 48, 48];
//    public static 

//    public static Vector3Int NotFoundVectorBasis { get; } = new Vector3Int(0, -500, 0);

    private static Dictionary<Vector3Int, Vector3Int> dictionary = new Dictionary<Vector3Int, Vector3Int>();

    public static void Add(Vector3Int key, Vector3Int value) => dictionary.Add(key, key);
    public static void Remove(Vector3Int key) => dictionary.Remove(key);

    public static (bool result, Vector3Int vector) GetValue(Vector3Int key)
    {
        bool gotValue = dictionary.TryGetValue(key, out Vector3Int value);
        if (gotValue)
        {
            return (true, value);
        }
        else
        {
            return (false, NotFoundVector3Int);
        }
    }
    public static void Clear() => dictionary.Clear();
}
