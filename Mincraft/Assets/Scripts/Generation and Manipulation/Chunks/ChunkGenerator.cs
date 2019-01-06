using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Amib.Threading;
using Unity.Collections;
using Unity.Jobs;
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
    private IWorkItemResult[] wir;
    private List<IChunk> chunks;


    private void Start() //Multithreaded
    {
        chunkManager = ChunkManager.Instance;
        SmartThreadPool pool = new SmartThreadPool();
        List<Block> blocks = GetBlocks();
        IWorkItemResult<List<IChunk>> chunkResults = pool.QueueWorkItem(DoWork, blocks);


        List<IChunk> DoWork(List<Block> block)
        {
            return GetChunks(block);
        }

        chunks = chunkResults.Result;

        Invoke(nameof(test), 1f);
    }

    //Smart Threading 
    private void test()
    {
        Debug.Log("jetzt multithreading");
        SmartThreadPool smartThreadPool = new SmartThreadPool();
        System.Diagnostics.Stopwatch wa = System.Diagnostics.Stopwatch.StartNew();
        wir = new IWorkItemResult[chunks.Count];
        
        for (int i = 0; i < chunks.Count; i++)
        {
            wir[i] = smartThreadPool.QueueWorkItem(new WorkItemCallback(DoSomeWork), chunks[i]);
        }

        object DoSomeWork(object chunk)
        {
            return ModifyMesh.Combine((IChunk) chunk);
        }

        SmartThreadPool.WaitAll(wir);
        smartThreadPool.Shutdown();

//        for (int i = 0; i < chunks.Count; i++)
//        {
//            ModifyMesh.RedrawMeshFilter(chunks[i].CurrentGO, (MeshData) wir[i].Result);
//        }
        
        wa.Stop();
        Debug.Log(wa.ElapsedMilliseconds + "ms in total");
    }

        //Single Threaded
//    private void test()
//    {
//        Debug.Log("jetzt singlethreaded");
//        System.Diagnostics.Stopwatch wa = System.Diagnostics.Stopwatch.StartNew();
//        
//        for (int i = 0; i < chunkParents.Count; i++)
//        {
//            ModifyMesh.Combine(chunkParents[i]);
//        }
//        
//        wa.Stop();
//        Debug.Log(wa.ElapsedMilliseconds + "ms in total");
//    }


    private List<IChunk> GetChunks(List<Block> blocks)
    {
        
        // Alles sehr langsam
        HashSet<IChunk> parents = new HashSet<IChunk>();

        System.Diagnostics.Stopwatch wa = System.Diagnostics.Stopwatch.StartNew();
        
        for (int i = 0; i < blocks.Count; i++)
        {
            IChunk chunk = chunkManager.AddBlock(blocks[i]);
            parents.Add(chunk);
        }

        for (int i = 0; i < blocks.Count; i++)
        {
            Block b = blocks[i];
            b.ID = blocks[i].GetNeigbourAt(2) == false ? (int) surface : (int) bottom;
            blocks[i] = b;
        }
        
        wa.Stop();
        Debug.Log(wa.ElapsedMilliseconds + "ms in GetChunksAndParentTuples");


        return parents.ToList();
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
