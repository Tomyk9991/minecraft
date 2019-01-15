using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor.U2D;
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
    [SerializeField] private Transform playerPosition;
    [SerializeField] private BlockUV surface = default;
    [SerializeField] private BlockUV bottom = default;
    [SerializeField] private Int3 size = default;
    [SerializeField] private int loadingChunkDistance = 10;
    [SerializeField] private float currentDistanceDebug = 0;
    
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

    private int drawCounter2 = 0;
    private bool doneMeshing2 = false;

    private void OnValidate() => PlayerPrefs.SetInt(nameof(chunkSize), (int)chunkSize);

    private void Start()
    {
        if (seed == -1)
            seed = UnityEngine.Random.Range(0,10000);
            
        latestPlayerPosition = Int3.ToInt3(playerPosition.position);
        originPoint = latestPlayerPosition;
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
        
        if (!doneMeshing2 && meshDatas.Count != 0)
        {
            int counter = 0;
            while (counter < 30 && meshDatas.TryDequeue(out var data))
            {
                counter++;
                modifier.RedrawMeshFilter(data.GameObject, data);

                if (chunks.Count == ++drawCounter2)
                {
                    doneMeshing2 = true;
                }
            }
        }

        latestPlayerPosition = Int3.ToInt3(playerPosition.position);

        if (MovedFromOrigin(latestPlayerPosition))
        {
            //Removing the not needed Chunks, or recalculate all?
            //Atm recalculate all
            //Recalculate only this meshes, which are not loaded yet.
            //the the Task begin
            generatingTask = GenerateChunks();
            //Set doneMeshing property back to false, cause' there is something new to render
            doneMeshing2 = false;
            drawCounter2 = 0;
//            doneMeshing = false;
//            drawCounter = 0;
        }
        
        
        if (generatingTask.IsCompleted && !doneMeshing2)
        {
            //TODO: Make this process multithreaded?!?!? At least "CalculateNeighbours"
            chunks = generatingTask.Result;
            System.Diagnostics.Stopwatch wa = System.Diagnostics.Stopwatch.StartNew();
            
            for (int i = 0; i < chunks.Count; i++)
            {
                chunks[i].CalculateNeigbours();
                chunks[i].CurrentGO.SetActive(true);
                chunks[i].CurrentGO.name = chunks[i].Position.ToString();
                modifier.Combine(chunks[i]);
            }
            
            wa.Stop();
            Debug.Log(wa.ElapsedMilliseconds +"ms für chunks[i]");
        }
    }

    private bool MovedFromOrigin(Int3 playerPos)
    {
        //TODO, ändere das noch zu einer Manhattan-Metrik
        currentDistanceDebug = Vector3.Distance(playerPos.ToVector3(), originPoint.ToVector3());
        if (currentDistanceDebug > loadingChunkDistance)
        {
            originPoint = playerPos;
            return true;
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.DrawSphere(originPoint.ToVector3(), 2f);
        }
    }

    private Task<List<IChunk>> GenerateChunks()
    {
        // Instead of calculating Chunks at the same Position, recalculate Chunks in the player's position proximity
        var list = new List<IChunk>();
        
        
        // -size.x / 2 < x < size.x / 2
        // Bei y ist das auch notwendig, allerdings nimmt man da nicht die Hälte
        // -size.z / 2 < z < size.z / 2

        return Task.Run(() =>
        {
            for (int y = 0; y > -size.Y; y -= (int)chunkSize)
            {
                for (int x = -size.X / 2 + latestPlayerPosition.X; x < size.X / 2 + latestPlayerPosition.X; x += (int)chunkSize)
                {
                    for (int z = -size.Z / 2 + latestPlayerPosition.Z; z < size.Z / 2 + latestPlayerPosition.Z; z += (int)chunkSize)
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

    public IChunk GenerateChunk(Int3 pos)
    {
        IChunk chunk = new Chunk();
        chunk.Position = pos;
        chunk.CurrentGO = GoPool.GetNextUnusedChunk();
        ChunkDictionary.Add(chunk.Position, chunk);
        ChunkGameObjectDictionary.Add(chunk.CurrentGO, chunk);

        return chunk;
    }
    
    private void GenerateBlocks(IChunk chunk)
    {
        List<Block> surfacePositions = GenerateHeightMap((int)chunkSize, chunk.Position, (x, z) =>
        {
            float height = (Mathf.PerlinNoise(seed + x * smoothness, seed + z * smoothness * 2) * heightMult +
                            Mathf.PerlinNoise(seed + x * smoothness, seed + z * smoothness * 2) * heightMult) / 2f;
    
            return Mathf.CeilToInt(height);
        });

        List<Block> bottom = GenerateBottomMap(surfacePositions);

        foreach (Block b in surfacePositions.Concat(bottom))
        {
            b.Position += chunk.Position;
            chunk.AddBlock(b);
        }
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
    

}