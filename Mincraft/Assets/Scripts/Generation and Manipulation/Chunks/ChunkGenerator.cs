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
    [SerializeField] private Vector3Int size = default;
    
    private Biom[] bioms;
    private ChunkManager chunkManager;
    
    private void Start()
    {
        chunkManager = ChunkManager.Instance;

        List<Vector3Int> positions = GenerateHeightMap(size, (x, y) =>
        {
            float height = (Mathf.PerlinNoise(x * smoothness, y * smoothness * 2) * heightMult +
                            Mathf.PerlinNoise(x * smoothness, y * smoothness * 2) * heightMult) / 2f;

            return Mathf.CeilToInt(height);
        });

        // add for every (x, y, z), y - size.y under the heighest block
        // 

        for (int i = 0; i < positions.Count; i++)
        {
            // How much blocks do I have to add?
            // ich habe eine Höhe von 9 und will runter auf bis -16
            // Füge also (9 + 16) - 1 Blöcke ein
            int temp = positions[i].y - (-size.y) - 1;
            for (int j = 1; j <= temp; j++)
                positions.Add(new Vector3Int(positions[i].x, positions[i].y - j, positions[i].z)); 
        } 

        //Hier kann man möglicherweise optimieren, indem man erst alle instanziert und dann Chunk für Chunk kombiniert
        //Instanziere bisher nur die oberste Schicht.
        int bound = size.x * size.z;
        for (int i = 0; i < bound; i++)
        {
            GameObject block = Instantiate(blockPrefab, positions[i], Quaternion.identity);
            block.name = block.transform.position.ToString();
            
            chunkManager.AddBlock(block);
        }
    }

    private List<Vector3Int> GenerateHeightMap(Vector3Int size, Func<int, int, int> heightFunc)
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
