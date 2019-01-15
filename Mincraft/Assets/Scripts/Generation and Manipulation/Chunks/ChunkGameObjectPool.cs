using System;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.Rendering;

public class ChunkGameObjectPool : SingletonBehaviour<ChunkGameObjectPool>
{
    [Header("Chunk GameObject instantiation settings")]
    [SerializeField] private GameObject chunkPrefab = null;
    [Range(1, 10000)]
    [SerializeField] private int chunksToInstantiate = 210;

    private const string unusedName = "Unused chunk";

    private ConcurrentQueue<GameObject> gameObjectChunks;
    
    private void Start()
    {
        gameObjectChunks = new ConcurrentQueue<GameObject>();
        for (int i = 0; i < chunksToInstantiate; i++)
        {
            InstantiateBlock();
        }
    }


    public GameObject GetNextUnusedChunk()
    {
        if (gameObjectChunks.TryDequeue(out var go))
            return go;

        throw new Exception("Not enough pool objects");
    }

    private void InstantiateBlock()
    {
        GameObject g = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity, transform);
        g.name = unusedName;
        g.SetActive(false);
        gameObjectChunks.Enqueue(g);
    }

    public void SetGameObject(GameObject go)
    {
        go.name = unusedName;
        go.SetActive(false);
        gameObjectChunks.Enqueue(go);
    }
}



