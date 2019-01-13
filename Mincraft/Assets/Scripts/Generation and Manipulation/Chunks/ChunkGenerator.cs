using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class ChunkGenerator : SingletonBehaviour<ChunkGenerator>, ICreateChunk
{
    public static int GetMaxSize => (int) Instance.chunkSize;
    public ChunkGameObjectPool GoPool { get; set; }
    
    [SerializeField] private uint chunkSize = 0;
    
    
    [Header("Perlin Noise")]
    [SerializeField] private float smoothness = 0.03f;
    [SerializeField] private float heightMult = 5f;
    
    [Header("Instantiation")]
    [SerializeField] private BlockUV surface = default;
    [SerializeField] private BlockUV bottom = default;
    [SerializeField] private Int3 size = default;
    
    [Header("Drawing Chunk")]
    public bool drawChunk = true;
    
    private List<IChunk> chunks;

    private ConcurrentQueue<MeshData> meshDatas;
    private MeshModifier modifier;


    private int drawCounter = 0;
    private bool doneMeshing = false;

    private void OnValidate() => PlayerPrefs.SetInt(nameof(chunkSize), (int)chunkSize);

    private void Start()
    {
        GoPool = ChunkGameObjectPool.Instance;
        
        modifier = new MeshModifier();
        meshDatas = new ConcurrentQueue<MeshData>();

        modifier.MeshAvailable += (s, data) =>
        {
            meshDatas.Enqueue(data);
        };

        if (size.X % chunkSize != 0 || size.Y % chunkSize != 0 || size.Z % chunkSize != 0)
        {
            throw new Exception("Diggah, WeltSize nicht teilbar durch ChunkSize");
        }

        System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
        var task = GenerateChunks();
        task.Wait();
        chunks = task.Result;
        watch.Stop();
        Debug.Log(watch.ElapsedMilliseconds + "ms for generating Chunks");
        
        for (int i = 0; i < chunks.Count; i++)
        {
            chunks[i].CalculateNeigbours();
            chunks[i].CurrentGO.SetActive(true);
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
            for (int y = 0; y > -size.Y; y -= (int)chunkSize)
            {
                for (int x = 0; x < size.X; x += (int)chunkSize)
                {
                    for (int z = 0; z < size.Z; z += (int)chunkSize)
                    {
                        if (counter < (size.X / chunkSize) * (size.Z / chunkSize))
                        {
                            IChunk chunk = GenerateChunk(new Int3(x, y, z));
                            list.Add(chunk);
                            GenerateBlocks(chunk);
                        }
                        else
                        {
                            IChunk chunk = GenerateChunk(new Int3(x, y, z));
                            list.Add(chunk);
                            GenerateBox(chunk);
                        }

                        counter++;
                    }
                }
            }

            return list;
        });
    }

    public IChunk GenerateChunk(Int3 pos)
    {
        IChunk chunk = new Chunk();
        chunk.Position = pos;
        chunk.CurrentGO = GoPool.GetNextUnusedChunk();
        ChunkDictionary.Add(chunk.Position, chunk);
        ChunkGameObjectDictionary.Add(chunk.CurrentGO, chunk);

        return chunk;
    }
    
    private List<Block> GenerateBottomMap(List<Block> surfacePositions)
    {
        List<Block> list = new List<Block>();
        
        for (int i = 0; i < surfacePositions.Count; i++)
        {
            for (int j = surfacePositions[i].Position.Y - 1; j >= 0; j--)
            {
                list.Add(new Block(new Int3(surfacePositions[i].Position.X, j, surfacePositions[i].Position.Z))
                {
                    ID = (int) bottom
                });
            }
        }

        return list;
    }
    
    private List<Block> GenerateHeightMap(int size, Int3 offset, Func<int, int, int> heightFunc)
    {
        List<Block> blocks = new List<Block>();

        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                int y = heightFunc(x + offset.X, z + offset.Z);

                y = Mathf.Clamp(y, 0, (int)chunkSize - 1);
                
                blocks.Add(new Block(new Int3(x, y, z))
                {
                    ID = (int) surface
                });
            }
        }
        
        return blocks;
    }

    private void GenerateBlocks(IChunk chunk)
    {
        List<Block> surfacePositions = GenerateHeightMap((int)chunkSize, chunk.Position, (x, z) =>
        {
            float height = (Mathf.PerlinNoise(x * smoothness, z * smoothness * 2) * heightMult +
                            Mathf.PerlinNoise(x * smoothness, z * smoothness * 2) * heightMult) / 2f;
    
            return Mathf.CeilToInt(height);
        });

        List<Block> bottom = GenerateBottomMap(surfacePositions);

        foreach (Block b in surfacePositions.Concat(bottom))
        {
            Block temp = new Block(b.Position + chunk.Position)
            {
                ID = b.ID
            };
            chunk.AddBlock(temp);
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
                    blocks.Add(new Block(new Int3(x, y, z))
                    {
                        ID = (int) bottom
                    });
                }
            }
        }
        foreach (Block b in blocks)
        {
            Block temp = new Block(b.Position + chunk.Position)
            {
                ID = b.ID
            };
            chunk.AddBlock(temp);
        }
    }

}