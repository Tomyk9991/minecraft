using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour
{
    [Header("Perlin Noise")]
    [SerializeField] private float smoothness = 0.03f;
    [SerializeField] private float heightMult = 5f;
    
    [Header("Instantiation")]
    [SerializeField] private BlockUV surface = default;
    [SerializeField] private BlockUV bottom = default;
    [SerializeField] private Vector3Int size = default;
    
    private Biom[] bioms;
    private ChunkManager chunkManager;
    
    private void Start()
    {   
        chunkManager = ChunkManager.Instance;

        (int surfacePositionsCount, List<Block> blocks) = GetBlocks();

        System.Diagnostics.Stopwatch wa = new System.Diagnostics.Stopwatch();
        wa.Start();

        HashSet<(IChunk, GameObject)> parents = new HashSet<(IChunk, GameObject)>();

        for (int i = 0; i < blocks.Count; i++)
        {
            if (i > surfacePositionsCount)
                blocks[i].UVSetter.SetBlockUV(BlockUV.Stone);
            
            parents.Add(chunkManager.AddBlock(blocks[i]));
        }


        List<(IChunk chunk, GameObject parent)> bla = parents.ToList();
        
        for (int i = 0; i < bla.Count; i++)
        {
            MeshData data = ModifyMesh.Combine(bla[i].chunk);
            var refMesh = bla[i].parent.GetComponent<MeshFilter>();

            refMesh.mesh = new Mesh
            {
                indexFormat = UnityEngine.Rendering.IndexFormat.UInt32,
                vertices = data.Vertices.ToArray(),
                triangles = data.Triangles.ToArray(),
                uv = data.UVs.ToArray()
            };
            
            refMesh.mesh.RecalculateNormals();
            bla[i].parent.AddComponent<MeshCollider>();
        }
        
        wa.Stop();
        Debug.Log(wa.ElapsedMilliseconds + "ms");
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
    
    (int surfaceCount, List<Block>) GetBlocks()
    {
        List<Vector3Int> surfacePositions = GenerateHeightMap(size, (x, z) =>
        {
            float height = (Mathf.PerlinNoise(x * smoothness, z * smoothness * 2) * heightMult +
                            Mathf.PerlinNoise(x * smoothness, z * smoothness * 2) * heightMult) / 2f;

            return Mathf.CeilToInt(height);
        });

        List<Vector3Int> bottom = GenerateBottomMap(surfacePositions);

        List<Block> blocks = surfacePositions
            .Concat(bottom)
            .Select(pos => new Block(pos))
            .ToList();

        return (surfacePositions.Count, blocks);
    }
}

