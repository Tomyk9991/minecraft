using System;
using System.Collections.Generic;
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

    private Queue<GameObject> objectsToRelease;
    private void Start()
    {
        objectsToRelease = new Queue<GameObject>();
        gameObjectChunks = new ConcurrentQueue<GameObject>();
        for (int i = 0; i < chunksToInstantiate; i++)
        {
            InstantiateBlock();
        }
    }

    private void Update()
    {
        for (int i = 0; i < objectsToRelease.Count && i < 5; i++)
        {
            GameObject go = objectsToRelease.Dequeue();

            if (go != null)
            {
                go.name = unusedName;

                go.GetComponent<MeshFilter>().mesh = null;
                go.GetComponent<MeshCollider>().sharedMesh = null;
                go.SetActive(false);

                gameObjectChunks.Enqueue(go);
            }

        }
    }


    /// <summary>
    /// Gets the next unused GameObject in Pool. (Can be used in different threads)
    /// </summary>
    /// <returns></returns>
    public GameObject GetNextUnusedChunk()
    {
        GameObject go;
        if (gameObjectChunks.TryDequeue(out go))
        {
            currentlyUsedObjs++;
            return go;
        }

        return Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity, transform);
    }

    private void InstantiateBlock()
    {
        GameObject g = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity, transform);
        SetInitialGameObjectUnsed(g);
    }

    public void SetGameObjectToUnsed(GameObject go)
    {
        currentlyUsedObjs--;
        objectsToRelease.Enqueue(go);
    }

    private void SetInitialGameObjectUnsed(GameObject go)
    {
        go.name = unusedName;
        go.SetActive(false);
        gameObjectChunks.Enqueue(go);
    }
}