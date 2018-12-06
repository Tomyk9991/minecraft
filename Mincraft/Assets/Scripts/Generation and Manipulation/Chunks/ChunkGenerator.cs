using System;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour
{
    [Header("Perlin Noise")]
    [SerializeField] private float smoothness = 0.03f;
    [SerializeField] private float heightMult = 5f;
    
    [Header("Instantiation")]
    [SerializeField] private GameObject blockPrefab = null;
    [SerializeField] private Vector2Int size = default;
    
    private Biom[] bioms;
    private ChunkManager chunkManager;
    
    private void Start()
    {
        chunkManager = ChunkManager.Instance;

        List<Vector3Int> positions = GenerateMap(size, (x, y) =>
        {
            float height = (Mathf.PerlinNoise(x * smoothness, y * smoothness * 2) * heightMult +
                            Mathf.PerlinNoise(x * smoothness, y * smoothness * 2) * heightMult) / 2f;

            return Mathf.CeilToInt(height);
        });

        //Hier kann man möglicherweise optimieren, indem man erst alle instanziert und dann Chunk für Chunk kombiniert
        for (int i = 0; i < positions.Count; i++)
        {
            GameObject block = Instantiate(blockPrefab, positions[i], Quaternion.identity);
            block.name = block.transform.position.ToString();
            
            chunkManager.AddBlock(block);
        }
    }

    private List<Vector3Int> GenerateMap(Vector2Int size, Func<int, int, int> heightFunc)
    {
        List<Vector3Int> positions = new List<Vector3Int>();

        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z <= size.y; z++)
            {
                positions.Add(new Vector3Int(x, heightFunc(x, z), z));
            }
        }

        return positions;
    }
}
