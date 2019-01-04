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
    
    private void Start()
    {
        chunkManager = ChunkManager.Instance;
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

                (IChunk chunk, GameObject parent) = chunkManager.AddBlock(block);

                var data = ModifyMesh.Combine(chunk);
                ModifyMesh.RedrawMeshFilter(parent, data);

                var bounds = chunk.GetChunkBounds();
                var tuple = ChunkManager.IsBoundBlock(bounds, centerCube);
                
                if (tuple.Result)
                {
                    for (int i = 0; i < tuple.Directions.Length; i++)
                    {
                        Vector3Int pos = tuple.Directions[i] + chunk.ChunkOffset;
                        IChunk neigbourChunk = ChunkDictionary.GetValue(pos);

                        MeshData nData = ModifyMesh.Combine(neigbourChunk);
                        ModifyMesh.RedrawMeshFilter(neigbourChunk.CurrentGO, nData);
                    }
                }
            }
        }
    }
}