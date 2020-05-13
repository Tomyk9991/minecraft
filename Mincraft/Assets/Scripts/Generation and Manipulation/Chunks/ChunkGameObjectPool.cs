using System.Collections.Generic;
using Attributes;
using UnityEngine;
using Extensions;


namespace Core.Chunks
{
    public class ChunkGameObjectPool : SingletonBehaviour<ChunkGameObjectPool>
    {
        [Header("Chunk GameObject instantiation settings")]
        [SerializeField] private GameObject chunkPrefab = null;
        [Range(1, 10000)]
        [SerializeField] private int chunksToInstantiate = 210;
        [SerializeField, ShowOnly] private int gameobjectCount;
        
        private Queue<GameObject> gameObjectChunks;
        private Queue<GameObject> objectsToRelease;
        
        private void Start()
        {
            objectsToRelease = new Queue<GameObject>();
            gameObjectChunks = new Queue<GameObject>();
            
            for (int i = 0; i < chunksToInstantiate; i++)
            {
                gameObjectChunks.Enqueue(InstantiateChunkGameObject());
            }
        }

        private void Update()
        {
            while (objectsToRelease.Count > 0)
            {
                GameObject go = objectsToRelease.Dequeue();

                if (go != null)
                {
                    go.SetActive(false);
                    gameObjectChunks.Enqueue(go);
                }
            }
        }


        /// <summary>
        /// Gets the next unused GameObject in Pool
        /// </summary>
        /// <returns></returns>
        public GameObject GetNextUnusedChunk()
        {
            if (gameObjectChunks.Count == 0)
            {
                return InstantiateChunkGameObject();
            }

            gameobjectCount = transform.childCount;
            return gameObjectChunks.Dequeue();
        }

        private GameObject InstantiateChunkGameObject()
        {
            GameObject g = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity, transform);
            //TODO isStatic might be an option
            //g.isStatic = true;
            g.name = "Chunk";
            g.SetActive(false);

            var m1 = new Mesh();
            var m2 = new Mesh();
            g.GetComponent<MeshFilter>().sharedMesh = m1;
            g.GetComponent<MeshCollider>().sharedMesh = m2;
            
            gameobjectCount = transform.childCount;
            return g;
        }

        public void SetGameObjectToUnused(GameObject go)
        {
            objectsToRelease.Enqueue(go);
            gameobjectCount = transform.childCount;
        }
    }
}
