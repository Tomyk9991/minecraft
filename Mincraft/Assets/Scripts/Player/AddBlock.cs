using UnityEngine;

public class AddBlock : MonoBehaviour, IMouseUsable, IConsoleToggle
{
    public ChunkGameObjectPool GoPool { get; set; }
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

    private int chunkSize;
    
    private Camera cameraRef;

    private ChunkJobManager chunkJobManager;
    private MeshModifier modifier;

    public bool Enabled
    {
        get => this.enabled;
        set => this.enabled = value;
    }

    private void Start()
    {
        chunkSize = ChunkSettings.GetMaxSize;
        GoPool = ChunkGameObjectPool.Instance;
        cameraRef = Camera.main;

        chunkJobManager = new ChunkJobManager();
        modifier = new MeshModifier();
        chunkJobManager.Start();
    }

    public void SetBlock(BlockUV uv) => blockUV = uv;

    private void Update()
    {
        if (Input.GetMouseButtonDown(mouseButtonIndex))
        {
            Ray ray = cameraRef.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, RaycastHitable))
            {
                Int3 globalCenterCubePosition =
                    Int3.FloorToInt(ModifyMesh.CenteredClickPositionOutSide(hit.point, hit.normal));

                Block block = new Block(globalCenterCubePosition)
                {
                    ID = (int)blockUV
                };

                Chunk chunkOnClicked = ChunkDictionary.GetValue(hit.transform.position.ToInt3());

                Chunk chunk = chunkOnClicked.TryAddBlockFromGlobal(block);

                if (chunk != null)
                {
                    chunkJobManager.Add(new ChunkJob(chunk));
                }
            }
        }

        for (int i = 0; i < chunkJobManager.FinishedJobsCount; i++)
        {
            ChunkJob task = chunkJobManager.DequeueFinishedJobs();

            if (task != null && task.Completed)
            {
                modifier.SetMesh(task.Chunk.CurrentGO, task.MeshData, task.ColliderData);
            }
        }
    }

    private void OnDestroy()
    {
        //chunkJobManager.Dispose();
    }
}