using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
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

    private System.Diagnostics.Stopwatch wa;


    private int drawCounter = 0;
    private bool doneMeshing = false;

    private void Start() //Multithreaded
    {
        chunkManager = ChunkManager.Instance;
        
        modifier = new MeshModifier();
        meshDatas = new ConcurrentQueue<MeshData>();

        modifier.MeshAvailable += (s, data) =>
        {
            meshDatas.Enqueue(data);
        };
        
        //Gets the Chunks with multithreaded Power
        wa = new System.Diagnostics.Stopwatch();
        wa.Start();
        
        var chunkTask = GetChunks(GetBlocks());
        chunkTask.Wait();
        chunks = chunkTask.Result;
        
        wa.Stop();
        
        Debug.Log(wa.ElapsedMilliseconds + "ms in this::GetChunks()");
        Debug.Log("Active Chunks: " + chunks.Count);
        
        wa.Reset();
        wa.Start();
        
        for (int i = 0; i < chunks.Count; i++)
        {
            chunks[i].CurrentGO.name = chunks[i].ChunkOffset.ToString();
            ChunkGameObjectDictionary.Add(chunks[i].CurrentGO, chunks[i]);
            modifier.Combine(chunks[i]);
        }
    }

    private void Update()
    {
        //Vielleicht unabhängig von Update lösen? Ein EventListner im Mainthread vielleicht?
        //Event im ConcurrentQueue für den Mainthread?
        if (!doneMeshing && meshDatas.Count != 0)
        {
            while (meshDatas.TryDequeue(out var data))
            {
                ModifyMesh.RedrawMeshFilter(data.GameObject, data);

                if (chunks.Count == ++drawCounter)
                {
                    doneMeshing = true;
                    modifier.MeshAvailable -= (s, d) => { };
                    modifier = null;

                    wa.Stop();
                    Debug.Log(wa.ElapsedMilliseconds + "ms in ModifyMesh::RedrawMeshFilter() and Combine");
                }
            }
        }
    }

    //Wesentlich schneller, als alte Methode mit checken, ob Chunk schon vorhanden und dann einfügen + zurückgeben
    private Task<List<IChunk>> GetChunks(List<Block> blocks)
    {
        return Task.Run(() =>
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                chunkManager.AddBlock(blocks[i]);
                blocks[i].ID = blocks[i].GetNeigbourAt(2) == false ? (int) surface : (int) bottom;
            }

            return ChunkDictionary.GetChunks();
        });
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
    
    private List<Vector3Int> GenerateHeightMap(Vector3Int size, System.Func<int, int, int> heightFunc)
    {
        List<Vector3Int> positions = new List<Vector3Int>();

        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z <= size.z; z++)
            {
                int y = heightFunc(x, z);
                
                positions.Add(new Vector3Int(x, y , z));
            }
        }
        
        
        return positions;
    }

    private List<Block> GetBlocks()
    {
        List<Vector3Int> surfacePositions = GenerateHeightMap(size, (x, z) =>
        {
            float height = (Mathf.PerlinNoise(x * smoothness, z * smoothness * 2) * heightMult +
                            Mathf.PerlinNoise(x * smoothness, z * smoothness * 2) * heightMult) / 2f;
    
            return Mathf.CeilToInt(height);
        });
    
        List<Vector3Int> bottom = GenerateBottomMap(surfacePositions);
    
        return surfacePositions
            .Concat(bottom)
            .Select(pos => new Block(pos))
            .ToList();
    }
}