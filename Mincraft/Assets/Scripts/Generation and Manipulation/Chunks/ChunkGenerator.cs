using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

public class ChunkGenerator : MonoBehaviour
{
    [Header("Perlin Noise")]
    [SerializeField] private float smoothness = 0.03f;
    [SerializeField] private float heightMult = 5f;
    
    [Header("Instantiation")]
    [SerializeField] private GameObject grassPrefab = null;
    [SerializeField] private GameObject stonePrefab = null;
    [SerializeField, Range(1, 200)] private int instantiationPerFrame = 20;
    [SerializeField] private int splitInFrames = 2000;
    [SerializeField] private Vector3Int size = default;
    
    private Biom[] bioms;
    private ChunkManager chunkManager;
    private BlockPool pool;

    
    private void Start()
    {
        pool = BlockPool.Instance;
        chunkManager = ChunkManager.Instance;
        
        List<Vector3Int> surfacePositions = GenerateHeightMap(size, (x, z) =>
        {
            float height = (Mathf.PerlinNoise(x * smoothness, z * smoothness * 2) * heightMult + 
                            Mathf.PerlinNoise(x * smoothness, z * smoothness * 2) * heightMult) / 2f;
            
            return Mathf.CeilToInt(height);
        });
        
        
        //Surface
        StartCoroutine(InstantiateBlocks(grassPrefab, surfacePositions, chunkManager));
        var gameObjects = SetPositions(surfacePositions);
        ReactivateBlocks(gameObjects, chunkManager);
    }

    private List<GameObject> SetPositions(List<Vector3Int> surfacePositions)
    {
        List<Vector3Int> bottomPositions = GenerateBottomMap(surfacePositions);
        Transform[] transforms = new Transform[bottomPositions.Count];
        for (int i = 0; i < transforms.Length; i++)
            transforms[i] = pool.GameObjectPool.Dequeue().transform;
        
        
        NativeArray<Vector3Int> array = new NativeArray<Vector3Int>(bottomPositions.ToArray(), Allocator.TempJob);
        TransformAccessArray accessArray = new TransformAccessArray(transforms);
        
        EnableJob job = new EnableJob()
        {
            targets = array
        };

        var handle = job.Schedule(accessArray);
        handle.Complete();
        
        array.Dispose();
        accessArray.Dispose();

        return transforms.Select(t => t.gameObject).ToList();
    }

    private void ReactivateBlocks(List<GameObject> gameObjects, ChunkManager manager)
    {
        for (int i = 0; i < gameObjects.Count; i++)
        {
            gameObjects[i].name = gameObjects[i].transform.position.ToString();
            //gameObjects[i].GetComponent<MeshRenderer>().enabled = true;
    
            manager.AddBlock(gameObjects[i]);
        }
    }

    private IEnumerator InstantiateBlocks(GameObject prefab, List<Vector3Int> p, ChunkManager manager, bool subdivide = false)
    {   
        int index = 0;
        while (index < p.Count)
        {
            for (int i = 0; i < instantiationPerFrame; i++)
            {
                if (index == p.Count - 1)
                    break;
                
                
                GameObject block = Instantiate(prefab, p[index], Quaternion.identity);
                block.name = block.transform.position.ToString();
    
                manager.AddBlock(block);
                index++;
            }

            yield return null;
        }
    }

    private List<Vector3Int> GenerateBottomMap(List<Vector3Int> surfacePositions)
    {
        List<Vector3Int> list = new List<Vector3Int>();
        
        for (int i = 0; i < surfacePositions.Count; i++)
        {
            int heightDelta = surfacePositions[i].y - (-size.y);

            for (int j = 1; j < heightDelta; j++)
            {
                list.Add(new Vector3Int(surfacePositions[i].x, surfacePositions[i].y - j, surfacePositions[i].z));
            }
        }

        return list;
    }
    
    private List<Vector3Int> GenerateHeightMap(Vector3Int size, Func<int, int, int> heightFunc)
    {
        bool firstHeightSet = false;
        int firstHeight = int.MaxValue;
        
        List<Vector3Int> positions = new List<Vector3Int>();

        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z <= size.z; z++)
            {
                int y = heightFunc(x, z);

                if (!firstHeightSet)
                {
                    firstHeightSet = true;
                    firstHeight = y;
                }
                
                positions.Add(new Vector3Int(x, y - firstHeight, z));
            }
        }
        
        
        return positions;
    }
}

public struct EnableJob : IJobParallelForTransform
{
    public NativeArray<Vector3Int> targets;
    
    public void Execute(int index, TransformAccess transform)
    {
        transform.position = targets[index];
    }
}
