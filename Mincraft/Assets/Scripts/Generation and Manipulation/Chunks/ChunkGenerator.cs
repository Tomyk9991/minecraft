using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ChunkGenerator : SingletonBehaviour<ChunkGenerator>, ICreateChunk
{
    public static int GetMaxSize => (int) Instance.chunkSize;
    public ChunkGameObjectPool GoPool { get; set; }

    [Header("Chunksettings")]
    [SerializeField] private uint chunkSize = 0;
    
    
    [Header("Perlin Noise")]
    [SerializeField] private float smoothness = 0.03f;
    [SerializeField] private float heightMult = 5f;
    [SerializeField] private int seed = -1;
    
    [Header("Instantiation")]
    [SerializeField] private BlockUV surface = default;
    [SerializeField] private BlockUV bottom = default;
    [SerializeField] private Int3 size = default;
    
    [Header("Drawing Chunk")]
    public bool drawChunk = true;
    
    private List<IChunk> chunks;

    private ConcurrentQueue<MeshData> meshDatas;
    private MeshModifier modifier;

    private Int3 latestPlayerPosition;
    private Int3 originPoint;

    private Task<List<IChunk>> generatingTask;
    private int drawCounter = 0;
    private bool doneMeshing = false;

    private void OnValidate() => PlayerPrefs.SetInt(nameof(chunkSize), (int)chunkSize);

    private void Start()
    {
        if (seed == -1)
            seed = UnityEngine.Random.Range(0,10000);
            
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
        
        generatingTask = GenerateChunks();
        generatingTask.Wait();
        chunks = generatingTask.Result;
        
        watch.Stop();
        Debug.Log($"{watch.ElapsedMilliseconds} ms for generating {chunks.Count} Chunks");
        
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
            for (int y = 0; y > -size.Y; y -= (int) chunkSize)
            {
                for (int x = -size.X / 2 + latestPlayerPosition.X; x < size.X / 2 + latestPlayerPosition.X; x += (int) chunkSize)
                {
                    for (int z = -size.Z / 2 + latestPlayerPosition.Z; z < size.Z / 2 + latestPlayerPosition.Z; z += (int) chunkSize)
                    {
                        IChunk chunk = GenerateChunk(new Int3(x, y, z));
                        list.Add(chunk);
                        GenerateBlocks(chunk);
                    }
                }
            }

            return list;
        });
    }

    public void GenerateBlocks(IChunk chunk)
    {
        float divisior = chunkSize;
        for (int x = 0; x < chunkSize; x++)
        {
            float noiseX = (float) x / divisior;
            for (int y = 0; y < chunkSize; y++)
            {
                float noiseY = (float) y / divisior;
                for (int z = 0; z < chunkSize; z++)
                {
                    float noiseZ = (float) z / divisior;
                    float result = SimplexNoise.Generate(noiseX + chunk.Position.X - chunkSize, noiseY + chunk.Position.Y + chunkSize, noiseZ + chunk.Position.Z + chunkSize);

                    result += (10f - (float) y) / 10;

                    if (result > 0.2f)
                    {
                        Block b = new Block(new Int3(x + chunk.Position.X, y + chunk.Position.Y, z + chunk.Position.Z));
                        b.SetID((int) BlockUV.Stone);
                        chunk.AddBlock(b);
                    }
                }
            }
        }
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
}