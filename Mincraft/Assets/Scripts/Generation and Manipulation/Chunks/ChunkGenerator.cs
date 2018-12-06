using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour
{
    [Header("Perlin Noise")]
    [SerializeField] private float smoothness = 0.03f;
    [SerializeField] private float heightMult = 5f;
    
    [Header("Instantiation")]
    [SerializeField] private GameObject blockPrefab = null;
    [SerializeField] private Vector3Int size = default;
    
    private Biom[] bioms;
    private ChunkManager chunkManager;

    private ChunkSpawner spawner;
    
    private void Start()
    {
        chunkManager = ChunkManager.Instance;

        List<Vector3Int> positions = GenerateMap(size, (x, y) =>
        {
            float height = (Mathf.PerlinNoise(x * smoothness, y * smoothness * 2) * heightMult +
                            Mathf.PerlinNoise(x * smoothness, y * smoothness * 2) * heightMult) / 2f;

            return Mathf.CeilToInt(height);
        });


        int bound = size.x * size.z;
        spawner = new ChunkSpawner();
        
        StartCoroutine(StartSpawn(blockPrefab, positions, chunkManager));

//        for (int i = 0; i < bound; i++)
//        {
//            GameObject block = Instantiate(blockPrefab, positions[i], Quaternion.identity);
//            block.name = block.transform.position.ToString();
//            
//            chunkManager.AddBlock(block);
//        }
        
        IEnumerator StartSpawn(GameObject prefab, List<Vector3Int> p, ChunkManager manager)
        {
            int index = 0;
        
            while (index < p.Count)
            {
                GameObject block = Instantiate(prefab, p[index], Quaternion.identity);
                block.name = block.transform.position.ToString();

                manager.AddBlock(block);
                index++;

                return null;
            }

            return null;
        }
    }

    private List<Vector3Int> GenerateMap(Vector3Int size, Func<int, int, int> heightFunc)
    {
        List<Vector3Int> positions = new List<Vector3Int>();

        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z <= size.z; z++)
            {
                positions.Add(new Vector3Int(x, heightFunc(x, z), z));
            }
        }

        return positions;
    }
}

public class ChunkSpawner
{
    public ChunkSpawner()
    {
        
    }
}
