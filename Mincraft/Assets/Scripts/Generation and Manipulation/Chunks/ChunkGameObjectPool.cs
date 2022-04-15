using System;
using System.Collections.Generic;
using Attributes;
using Core.Builder;
using UnityEngine;
using Extensions;
using Utilities;


namespace Core.Chunks
{
    public class ChunkGameObjectPool : SingletonBehaviour<ChunkGameObjectPool>
    {
        [Header("Chunk Pool settings")]
        [Range(1, 10000)]
        [SerializeField] private int chunksToInstantiate = 210;
        [SerializeField] private GameObject chunkPrefab = null;
        [SerializeField, ShowOnly] private int gameobjectCount;
        
        private Pool<GameObject> chunkPool = null;
        private Queue<GameObject> objectsToRelease; // Support for multi threaded calls. That's why they are two queues
        
        
        private void Start()
        {
            objectsToRelease = new Queue<GameObject>();
            chunkPool = new Pool<GameObject>(chunksToInstantiate, InstantiateChunkGameObject);
        }

        private void Update()
        {
            while (objectsToRelease.Count > 0)
            {
                GameObject go = objectsToRelease.Dequeue();

                if (go != null)
                    chunkPool.Add(go);
            }
        }


        /// <summary>
        /// Gets the next unused GameObject in Pool
        /// </summary>
        /// <returns></returns>
        public GameObject GetNextUnusedChunk()
        {
            GameObject g = chunkPool.GetNext();
            gameobjectCount = transform.childCount;
            return g;
        }
        
        private GameObject InstantiateChunkGameObject()
        {
            GameObject g = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity, transform);
            //TODO isStatic might be an option
            //g.isStatic = true;
            g.name = "Chunk";

            var m1 = new Mesh();
            var m2 = new Mesh();
            g.GetComponent<MeshFilter>().sharedMesh = m1;
            g.GetComponent<MeshCollider>().sharedMesh = m2;
            
            gameobjectCount = transform.childCount;
            return g;
        }
        
        public void SetGameObjectToUnused(GameObject go)
        {
            chunkPool.Add(go);
            gameobjectCount = transform.childCount;
        }
    }
}
