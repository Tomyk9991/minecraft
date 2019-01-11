using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class ChunkManager : SingletonBehaviour<ChunkManager>
{
    public static int GetMaxSize => Instance.maxSize;
    
    [SerializeField] private int maxSize = 0;
    
    private ChunkGameObjectPool chunkGOPool;
    private ConcurrentQueue<MeshData> meshDatas;
    private MeshModifier modifier;

    private void Start()
    {
        chunkGOPool = ChunkGameObjectPool.Instance;
        meshDatas = new ConcurrentQueue<MeshData>();
        modifier = new MeshModifier();
        modifier.MeshAvailable += (s, data) => meshDatas.Enqueue(data);
    }
    
    public (Vector3Int[] Directions, bool Result) IsBoundBlock((Vector3Int lowerBound, Vector3Int higherBound) tuple, Vector3Int pos)
    {
        List<Vector3Int> directions = new List<Vector3Int>();
        bool result = false;
        
        
        if (pos.x == tuple.lowerBound.x || pos.x - 1 == tuple.lowerBound.x)
        {
            directions.Add(new Vector3Int(-maxSize, 0, 0));
            result = true;
        }

        if (pos.y == tuple.lowerBound.y || pos.y - 1 == tuple.lowerBound.y)
        {
            directions.Add(new Vector3Int(0, -maxSize, 0));
            result = true;
        }

        if (pos.z == tuple.lowerBound.z || pos.z - 1 == tuple.lowerBound.z)
        {
            directions.Add(new Vector3Int(0, 0, -maxSize));
            result = true;
        }
        
        if (pos.x == tuple.higherBound.x || pos.x + 1 == tuple.higherBound.x)
        {
            directions.Add(new Vector3Int(maxSize, 0, 0));
            result = true;
        }

        if (pos.y == tuple.higherBound.y || pos.y + 1 == tuple.higherBound.y)
        {
            directions.Add(new Vector3Int(0, maxSize, 0));
            result = true;
        }
        
        if (pos.z == tuple.higherBound.z || pos.z + 1 == tuple.higherBound.z)
        {
            directions.Add(new Vector3Int(0, 0, maxSize));
            result = true;
        }


        return (directions.ToArray(), result);
    }
}