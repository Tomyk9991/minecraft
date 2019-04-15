using System;
using System.Collections.Concurrent;
using UnityEngine;

public class ChunkGameObjectPool : SingletonBehaviour<ChunkGameObjectPool>
{
    [Header("Chunk GameObject instantiation settings")]
    [SerializeField] private GameObject chunkPrefab = null;
    //[Range(1, 10000)]
    //[SerializeField] private int chunksToInstantiate = 210;
    [SerializeField, ShowOnly] private int currentlyUsedObjs;

    private const string unusedName = "Unused chunk";

    private ConcurrentQueue<GameObject> gameObjectChunks;

    private ConcurrentQueue<GameObject> objectsToRelease;
    
    //TODO make gameobject pool
    private void Start()
    {
        objectsToRelease = new ConcurrentQueue<GameObject>();
        gameObjectChunks = new ConcurrentQueue<GameObject>();
        //for (int i = 0; i < chunksToInstantiate; i++)
        //{
        //    InstantiateBlock();
        //}
    }

    private void Update()
    {
        //while (!objectsToRelease.IsEmpty && objectsToRelease.TryDequeue(out GameObject go))
        //{
        //    go.name = unusedName;

        //    go.GetComponent<MeshFilter>().mesh = null;
        //    go.GetComponent<MeshCollider>().sharedMesh = null;
        //    go.SetActive(false);

        //    gameObjectChunks.Enqueue(go);
        //}
    }


    /// <summary>
    /// Gets the next unused GameObject in Pool. (Can be used in different threads)
    /// </summary>
    /// <returns></returns>
    public GameObject GetNextUnusedChunk()
    {
        //GameObject go;
        //if (gameObjectChunks.TryDequeue(out go) && go != null)
        //{
        //    currentlyUsedObjs++;
        //    return go;
        //}

        return Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity, transform);

        throw new Exception("Not enough pool objects. TODO: Instantiate new GameObjects, if a bool is checked");
    }

    private void InstantiateBlock()
    {
        GameObject g = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity, transform);
        SetInitialGameObjectUnsed(g);
    }

    public void SetGameObjectToUnsed(GameObject go)
    {
        objectsToRelease.Enqueue(go);
    }

    private void SetInitialGameObjectUnsed(GameObject go)
    {
        go.name = unusedName;
        go.SetActive(false);
        gameObjectChunks.Enqueue(go);
    }
}



