using System;
using System.Collections.Concurrent;
using UnityEngine;

public class AddBlock : MonoBehaviour, IMouseUsable
{
    public float RaycastHitable
    {
        get => raycastHitable;
        set => raycastHitable = value;
    }

    public int MouseButtonIndex
    {
        get => mouseButtonIndex;
        set => mouseButtonIndex = value;
    }
    
    [SerializeField] private int mouseButtonIndex = 1;
    [SerializeField] private float raycastHitable = 1000f;
    [SerializeField] private BlockUV blockUV = BlockUV.Dirt;
    
    private (Vector3Int lowerBound, Vector3Int higherBound) tuple;
    
    private Camera cameraRef;
    
    private ChunkManager chunkManager;
    private MeshModifier modifier;
    private ConcurrentQueue<MeshData> meshDatas;
    
    private void Start()
    {
        cameraRef = Camera.main;
        chunkManager = ChunkManager.Instance;
        modifier = new MeshModifier();
        meshDatas = new ConcurrentQueue<MeshData>();
        modifier.MeshAvailable += (s, data) => meshDatas.Enqueue(data);
    }

    public void SetBlock(BlockUV uv) => blockUV = uv;

    private void Update()
    {
        if (Input.GetMouseButtonDown(mouseButtonIndex))
        {
            Ray ray = cameraRef.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000.0f))
            {
                Vector3Int centerCube = Vector3Int.FloorToInt(ModifyMesh.CenteredClickPositionOutSide(hit.point, hit.normal));
                Block block = new Block(centerCube)
                {
                    ID = (int) blockUV
                };

                IChunk temp = ChunkGameObjectDictionary.GetValue(hit.transform.gameObject);
                IChunk chunk = temp.TryAddBlock(block, hit.normal);

                if (chunk != temp)
                {
                    chunk.TryAddBlock(block, hit.normal);
                    var data1 = ModifyMesh.Combine(chunk);
                    modifier.RedrawMeshFilter(chunk.CurrentGO, data1);
                }

                if (chunk == null)
                    throw new Exception("Kein Nachbar. Erstelle neuen Chunk genau dort. Also hier.");

                //Synchronous mesh modification and recalculation
                var data = ModifyMesh.Combine(temp);
                modifier.RedrawMeshFilter(temp.CurrentGO, data);

                var bounds = temp.GetChunkBounds();
                this.tuple = bounds;
                var tuple = chunkManager.IsBoundBlock(bounds, centerCube);
                
                // Asynchronous mesh modification and recalculation 
                if (tuple.Result)
                {
                    for (int i = 0; i < tuple.Directions.Length; i++)
                    {
                        Vector3Int pos = tuple.Directions[i] + temp.Position;
                        IChunk neigbourChunk = ChunkDictionary.GetValue(pos);

                        if (neigbourChunk != null)
                        {
                            modifier.Combine(neigbourChunk);
                        }
                    }
                }
            }
        }

        if (meshDatas.Count > 0)
        {
            while (meshDatas.TryDequeue(out var data))
            {
                modifier.RedrawMeshFilter(data.GameObject, data);
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        Vector3 pos = (Vector3) (tuple.lowerBound + (tuple.higherBound - tuple.lowerBound)) * .5f;
        Vector3 size = tuple.higherBound - tuple.lowerBound;

        Gizmos.DrawWireCube(pos, size);
    }
}