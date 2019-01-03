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
    
    [SerializeField] private GameObject[] blocks = null;
    private GameObject newBlock;
    private ChunkManager chunkManager;
    
    private void Start()
    {
        chunkManager = ChunkManager.Instance;
        SetBlock(0);
    }

    private void SetBlock(int index) => newBlock = blocks[index];

    private void Update()
    {
        if (Input.GetMouseButtonDown(mouseButtonIndex))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000.0f))
            {
                Vector3 centeredCubePosition = ModifyMesh.CenteredClickPositionOutSide(hit.point, hit.normal);
                Block block = new Block(Vector3Int.FloorToInt(centeredCubePosition));
                
                (IChunk chunk, GameObject parent) = chunkManager.AddBlock(block);

                var data = ModifyMesh.Combine(chunk);
                
                var refMesh = parent.GetComponent<MeshFilter>();
        
                refMesh.mesh = new Mesh()
                {
                    indexFormat = UnityEngine.Rendering.IndexFormat.UInt32,
                    vertices = data.Vertices.ToArray(),
                    triangles = data.Triangles.ToArray()
                };
        
                refMesh.mesh.RecalculateNormals();
                
                Destroy(parent.GetComponent<MeshCollider>());
                parent.AddComponent<MeshCollider>();
            }
        }
    }
}