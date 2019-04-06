using System;
using System.Collections.Concurrent;
using UnityEngine;

public class ChunkGameObjectPool : SingletonBehaviour<ChunkGameObjectPool>
{
    [Header("Chunk GameObject instantiation settings")]
    [SerializeField] private GameObject chunkPrefab = null;
    [Range(1, 10000)]
    [SerializeField] private int chunksToInstantiate = 210;
    [SerializeField, ShowOnly] private int currentlyUsedObjs;

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
        {
            currentlyUsedObjs++;
            return go;
        }

        throw new Exception("Not enough pool objects. TODO: Instantiate new GameObjects, if a bool is checked");
    }

    private void InstantiateBlock()
    {
        GameObject g = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity, transform);
        SetGameObjectInactive(g);
    }

    public void SetGameObjectInactive(GameObject go)
    {
        go.name = unusedName;
        go.SetActive(false);
        gameObjectChunks.Enqueue(go);

    }
}



