using System.Collections.Generic;
using UnityEngine;

using UnityInspector;
using Extensions;


namespace Core.Chunking
{
    public class ChunkGameObjectPool : SingletonBehaviour<ChunkGameObjectPool>
    {
        [Header("Chunk GameObject instantiation settings")]
        [SerializeField] private GameObject chunkPrefab = null;
        [Range(1, 10000)]
        [SerializeField] private int chunksToInstantiate = 210;
        [SerializeField, ShowOnly] private int gameobjectCount;

        private const string unusedName = "Unused chunk";

        private Queue<GameObject> gameObjectChunks;
        private Queue<GameObject> objectsToRelease;
        
        private void Start()
        {
            objectsToRelease = new Queue<GameObject>();
            gameObjectChunks = new Queue<GameObject>();
            
            for (int i = 0; i < chunksToInstantiate; i++)
            {
                InstantiateBlock();
            }
        }

        private void Update()
        {
            for (int i = 0; i < objectsToRelease.Count; i++)
            {
                GameObject go = objectsToRelease.Dequeue();

                if (go != null)
                {
                    go.name = unusedName;

                    var refMesh = go.GetComponent<MeshFilter>(); 
                    refMesh.mesh.name = unusedName;
//                    refMesh.sharedMesh.name = unusedName;
                    
//                    refMesh.sharedMesh = null;
//                    refMesh.sharedMesh = null;
//                    go.GetComponent<MeshCollider>().sharedMesh = null;
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
            if (gameObjectChunks.Count == 0)
            {
                GameObject g = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity, transform);
                g.name = unusedName;
                g.SetActive(false);
                g.GetComponent<MeshFilter>().mesh = new Mesh();
                return g;
            }
                
            GameObject go = gameObjectChunks.Dequeue();
            gameobjectCount = transform.childCount;
            
            if (go != null)
            {
                return go;
            }

            GameObject g0 = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity, transform);
            g0.name = unusedName;
            g0.SetActive(false);
            g0.GetComponent<MeshFilter>().mesh = new Mesh();
            return g0;
        }

        private void InstantiateBlock()
        {
            GameObject g = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity, transform);
            SetInitialGameObjectUnsed(g);
            gameobjectCount = transform.childCount;
        }

        public void SetGameObjectToUnsed(GameObject go)
        {
            objectsToRelease.Enqueue(go);
            gameobjectCount = transform.childCount;
        }

        private void SetInitialGameObjectUnsed(GameObject go)
        {
            go.name = "FUCK";
            go.SetActive(false);
            go.GetComponent<MeshFilter>().mesh = new Mesh();
            gameObjectChunks.Enqueue(go);
        }
    }
}
