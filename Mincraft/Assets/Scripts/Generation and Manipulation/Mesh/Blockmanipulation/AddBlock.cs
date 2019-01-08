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
    
    private ChunkManager chunkManager;
    private MeshModifier modifier;
    private ConcurrentQueue<MeshData> meshDatas;
    
    private void Start()
    {
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
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000.0f))
            {
                Vector3Int centerCube = Vector3Int.FloorToInt(ModifyMesh.CenteredClickPositionOutSide(hit.point, hit.normal));
                Block block = new Block(centerCube)
                {
                    ID = (int) blockUV
                };

                IChunk chunk = chunkManager.AddBlock(block);

                //Synchronous mesh modification and recalculation
                var data = ModifyMesh.Combine(chunk);
                ModifyMesh.RedrawMeshFilter(chunk.CurrentGO, data);

                var bounds = chunk.GetChunkBounds();
                var tuple = chunkManager.IsBoundBlock(bounds, centerCube);
                
                // Asynchronous mesh modification and recalculation 
                if (tuple.Result)
                {
                    for (int i = 0; i < tuple.Directions.Length; i++)
                    {
                        Vector3Int pos = tuple.Directions[i] + chunk.ChunkOffset;
                        IChunk neigbourChunk = ChunkDictionary.GetValue(pos);

                        if (neigbourChunk != null)
                        {
                            modifier.Combine(neigbourChunk);
//                            MeshData nData = ModifyMesh.Combine(neigbourChunk);
                        }
                    }
                }
            }
        }

        #region ThreadingCheck

        if (meshDatas.Count > 0)
        {
            while (meshDatas.TryDequeue(out var data))
            {
                ModifyMesh.RedrawMeshFilter(data.GameObject, data);
            }
        }

        #endregion
    }
}