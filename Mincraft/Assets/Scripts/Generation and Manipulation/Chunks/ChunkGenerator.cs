using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ChunkGenerator : SingletonBehaviour<ChunkGenerator>, ICreateChunk
{
    public static int GetMaxSize => (int) Instance.chunkSize;
    public static SimplexNoiseSettings SimplexNoiseSettings => Instance.simplexNoiseSettings;
    public ChunkGameObjectPool GoPool { get; set; }

    [Header("Chunksettings")]
    [SerializeField] private uint chunkSize = 0;
    [SerializeField] private int seed = -1; //TODO Insert seed in noise

    [Header("Noisesettings")] //Remake this to make is depend on the biom, you're currently at
    [SerializeField] public float smoothness = 40;
    [SerializeField] public float steepness = 2;
    private SimplexNoiseSettings simplexNoiseSettings;

    [Header("Instantiation")]
    //[SerializeField] private BlockUV surface = default;
    //[SerializeField] private BlockUV bottom = default;
    [SerializeField] public Int3 drawDistance = default;

    [Header("Draw chunkbounds with Gizmos")]
    public bool drawChunk;
    
    
    private List<IChunk> currentlyActiveChunks;

    private ConcurrentQueue<MeshData> meshDatasQueue;
    private MeshModifier modifier;

    private Int3 latestPlayerPosition = default;

    private Task<List<IChunk>> generatingTask;
    private int drawCounter = 0;
    private bool doneMeshing = false;

    private void OnValidate() => PlayerPrefs.SetInt(nameof(chunkSize), (int)chunkSize);

    private void Start()
    {
        simplexNoiseSettings = new SimplexNoiseSettings(smoothness, steepness);
        if (seed == -1)
            seed = UnityEngine.Random.Range(0,10000);
            
        GoPool = ChunkGameObjectPool.Instance;
        
        modifier = new MeshModifier();
        meshDatasQueue = new ConcurrentQueue<MeshData>();

        modifier.MeshAvailable += (sender, data) =>
        {
            meshDatasQueue.Enqueue(data);
        };

        if (drawDistance.X % chunkSize != 0 || drawDistance.Y % chunkSize != 0 || drawDistance.Z % chunkSize != 0)
        {
            throw new Exception("Diggah, WeltSize nicht teilbar durch ChunkSize");
        }

        System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
        
        generatingTask = GenerateChunks();
        generatingTask.Wait();
        currentlyActiveChunks = generatingTask.Result;
        
        watch.Stop();
        Debug.Log($"{watch.ElapsedMilliseconds} ms for generating {currentlyActiveChunks.Count} Chunks");
        
        for (int i = 0; i < currentlyActiveChunks.Count; i++)
        {
            currentlyActiveChunks[i].CalculateNeigbours();
            currentlyActiveChunks[i].CurrentGO.SetActive(true);
            currentlyActiveChunks[i].CurrentGO.name = currentlyActiveChunks[i].Position.ToString();
            modifier.Combine(currentlyActiveChunks[i]);
        }
    }

    private void Update()
    {
        DrawExistingChunks();
    }

    private Task<List<IChunk>> GenerateChunks()
    {
        var list = new List<IChunk>();
        return Task.Run(() =>
        {
            int xStart = MathHelper.ClosestMultiple(latestPlayerPosition.X, (int)chunkSize);
            int yStart = MathHelper.ClosestMultiple(0, (int)chunkSize);
            int zStart = MathHelper.ClosestMultiple(latestPlayerPosition.Z, (int)chunkSize);

            for (int x = xStart - drawDistance.X; x < drawDistance.X; x += (int)chunkSize) //Klappt bei XStart - size.X, weil sich der Spieler auf der Position (0, y, z) befindet
            {
                // Minus to calculate chunks downwards, not upwards
                for (int y = yStart; y > -drawDistance.Y; y -= (int)chunkSize)
                {
                    for (int z = zStart - drawDistance.Z; z < drawDistance.Z; z += (int)chunkSize)
                    {
                        IChunk chunk = GenerateChunk(new Int3(x, y, z));
                        list.Add(chunk);
                        chunk.GenerateBlocks();
                    }
                }
            }

            return list;
        });
    }

    public IChunk GenerateChunk(Int3 pos)
    {
        IChunk chunk = new Chunk
        {
            Position = pos,
            CurrentGO = GoPool.GetNextUnusedChunk()
        };

        ChunkDictionary.Add(chunk.Position, chunk);
        ChunkGameObjectDictionary.Add(chunk.CurrentGO, chunk);
        return chunk;
    }

    private void DrawExistingChunks()
    {
        if (!doneMeshing && meshDatasQueue.Count != 0)
        {
            while (meshDatasQueue.TryDequeue(out var data))
            {
                modifier.RedrawMeshFilter(data.GameObject, data);

                if (currentlyActiveChunks.Count == ++drawCounter)
                {
                    doneMeshing = true;
                }
            }
        }
    }
}