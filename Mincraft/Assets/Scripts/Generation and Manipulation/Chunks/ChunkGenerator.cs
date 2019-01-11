using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.iOS;
using UnityEngine.Networking;

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
    private List<IChunk> chunks;

    private ConcurrentQueue<MeshData> meshDatas;
    private MeshModifier modifier;
    private ChunkGameObjectPool goPool;
    private int chunkSize = 0;


    private int drawCounter = 0;
    private bool doneMeshing = false;

    private void Start() //Multithreaded
    {
        chunkSize = ChunkManager.GetMaxSize;
        chunkManager = ChunkManager.Instance;
        goPool = ChunkGameObjectPool.Instance;
        
        modifier = new MeshModifier();
        meshDatas = new ConcurrentQueue<MeshData>();

        modifier.MeshAvailable += (s, data) =>
        {
            meshDatas.Enqueue(data);
        };

        if (size.x % chunkSize != 0 || size.y % chunkSize != 0 || size.z % chunkSize != 0)
        {
            throw new Exception("Diggah, WeltSize nicht teilbar durch ChunkSize");
        }

        var task = GenerateChunks();
        task.Wait();
        chunks = task.Result;
        
        for (int i = 0; i < chunks.Count; i++)
        {
            chunks[i].CalculateNeigbours();
            chunks[i].CurrentGO.name = chunks[i].Position.ToString();
            modifier.Combine(chunks[i]);
        }
    }

    private void Update()
    {
        if (!doneMeshing && meshDatas.Count != 0)
        {
            while (meshDatas.TryDequeue(out var data))
            {
                modifier.RedrawMeshFilter(data.GameObject, data);

                if (chunks.Count == ++drawCounter)
                {
                    doneMeshing = true;
                }
            }
        }
    }

    private Task<List<IChunk>> GenerateChunks()
    {
        var list = new List<IChunk>();
        return Task.Run(() =>
        {
            int counter = 0;
            for (int y = 0; y > -size.y; y -= chunkSize)
            {
                for (int x = 0; x < size.x; x += chunkSize)
                {
                    for (int z = 0; z < size.z; z += chunkSize)
                    {
                        if (counter < (size.x / chunkSize) * (size.z / chunkSize))
                        {
                            IChunk chunk = new Chunk();
                            chunk.Position = new Vector3Int(x, y, z);
                            chunk.CurrentGO = goPool.GetNextUnusedChunk();
                            GenerateBlocks(chunk);
                            list.Add(chunk);
                            ChunkDictionary.Add(chunk.Position, chunk);
                            ChunkGameObjectDictionary.Add(chunk.CurrentGO, chunk);
                        }
                        else
                        {
                            IChunk chunk = new Chunk();
                            chunk.Position = new Vector3Int(x, y, z);
                            chunk.CurrentGO = goPool.GetNextUnusedChunk();
                            GenerateBox(chunk);
                            list.Add(chunk);
                            ChunkDictionary.Add(chunk.Position, chunk);
                            ChunkGameObjectDictionary.Add(chunk.CurrentGO, chunk);
                        }

                        counter++;
                    }
                }
            }

            return list;
        });
    }

    private List<Block> GenerateBottomMap(List<Block> surfacePositions)
    {
        List<Block> list = new List<Block>();
        
        for (int i = 0; i < surfacePositions.Count; i++)
        {
            for (int j = surfacePositions[i].Position.y - 1; j >= 0; j--)
            {
                list.Add(new Block(new Vector3Int(surfacePositions[i].Position.x, j, surfacePositions[i].Position.z))
                {
                    ID = (int) bottom
                });
            }
        }

        return list;
    }
    
    private List<Block> GenerateHeightMap(int size, Vector3Int offset, Func<int, int, int> heightFunc)
    {
        List<Block> blocks = new List<Block>();

        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                int y = heightFunc(x + offset.x, z + offset.z);

                y = Mathf.Clamp(y, 0, chunkSize - 1);
                
                blocks.Add(new Block(new Vector3Int(x, y, z))
                {
                    ID = (int) surface
                });
            }
        }
        
        return blocks;
    }

    private void GenerateBlocks(IChunk chunk)
    {
        List<Block> surfacePositions = GenerateHeightMap(chunkSize, chunk.Position, (x, z) =>
        {
            float height = (Mathf.PerlinNoise(x * smoothness, z * smoothness * 2) * heightMult +
                            Mathf.PerlinNoise(x * smoothness, z * smoothness * 2) * heightMult) / 2f;
    
            return Mathf.CeilToInt(height);
        });

        List<Block> bottom = GenerateBottomMap(surfacePositions);

        foreach (Block b in surfacePositions.Concat(bottom))
        {
            b.Position += chunk.Position;
            chunk.AddBlock(b);
        }
    }

    private void GenerateBox(IChunk chunk)
    {
        List<Block> blocks = new List<Block>();
        
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    blocks.Add(new Block(new Vector3Int(x, y, z))
                    {
                        ID = (int) bottom
                    });
                }
            }
        }
        foreach (Block b in blocks)
        {
            b.Position += chunk.Position;
            chunk.AddBlock(b);
        }
    }
}