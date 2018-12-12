using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Threading.Tasks;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour
{
    [Header("Perlin Noise")]
    [SerializeField] private float smoothness = 0.03f;
    [SerializeField] private float heightMult = 5f;
    
    [Header("Instantiation")]
    [SerializeField] private GameObject grassPrefab = null;
    [SerializeField] private GameObject stonePrefab = null;
    [SerializeField, Range(1, 200)] private int instantiationPerFrame = 20;
    [SerializeField, Tooltip("In wie vielen Frames sollen alle Blöcke instanziert werden")] 
    private int desiredFrameTarget = 200;
    [SerializeField] private Vector3Int size = default;
    
    private Biom[] bioms;
    private ChunkManager chunkManager;

    
    private void Start()
    {
        chunkManager = ChunkManager.Instance;
        
        List<Vector3Int> surfacePositions = GenerateHeightMap(size, (x, z) =>
        {
            float height = (Mathf.PerlinNoise(x * smoothness, z * smoothness * 2) * heightMult + 
                            Mathf.PerlinNoise(x * smoothness, z * smoothness * 2) * heightMult) / 2f;
            
            return Mathf.CeilToInt(height);
        });

        List<Vector3Int> bottomPositions = GenerateBottomMap(surfacePositions);
        
        
        //Surface
        StartCoroutine(InstantiateBlocks(grassPrefab, surfacePositions, chunkManager));
        //Below
        StartCoroutine(InstantiateBlocks(stonePrefab, bottomPositions, chunkManager, true));
    }

    private IEnumerator InstantiateBlocks(GameObject prefab, List<Vector3Int> p, ChunkManager manager, bool subdivide = false)
    {
        if (subdivide)
        {
            int subListCounter = 0;
            var subLists = p.Split(desiredFrameTarget);

            for (int i = 0; i < subLists.Count; i++)
            {
                for (int j = 0; j < subLists[i].Count; j++)
                {
                    GameObject block = Instantiate(prefab, subLists[i][j], Quaternion.identity);
                    block.name = block.transform.position.ToString();

                    manager.AddBlock(block);
                }

                yield return null;
            }
            
            // breaks the function to stop
            yield break;
        }
        
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
