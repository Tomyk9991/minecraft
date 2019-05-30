using UnityEngine;

public class ChunkUpdater : MonoBehaviour
{
    public ChunkGameObjectPool GoPool { get; set; }

    [Header("References")]
    [SerializeField] private ChunkSettings generator = null;
    [SerializeField] private GameObject player = null;

    [Header("Chunk-redraw condition")]
    [SerializeField] private float maxDistance = 10;
    [Tooltip("Float, der bestimmt, nach wie viel Sekunden die Entfernung berechnet werden soll")]
    [SerializeField] private int amountDrawChunksPerFrame = 5;
    [Header("Debug")]
    [SerializeField] private bool drawLatestPlayerPosition = false;

    private Int3 drawDistance;
    private int chunkSize;
    private int maxYHeight;

    private Int3 latestPlayerPosition;

    private float currentDistance = 0;

    private ChunkJobManager chunkJobManager;

    private MeshModifier modifier;

    //private void Start()
    //{
    //    modifier = new MeshModifier();
    //    chunkJobManager = new ChunkJobManager();
    //    chunkJobManager.Start();

    //    latestPlayerPosition = player.transform.position.ToInt3();
    //    drawDistance = generator.drawDistance;

    //    chunkSize = ChunkSettings.GetMaxSize;
    //    maxYHeight = ChunkSettings.MaxYHeight;

    //    GoPool = ChunkGameObjectPool.Instance;

    //    RecalculateChunks();
    //}

    //private void Update()
    //{
    //    currentDistance = Vector3.Distance(player.transform.position, latestPlayerPosition.ToVector3());

    //    if (currentDistance > maxDistance)
    //    {
    //        //Divide and conquer
    //        RecalculateChunks();

    //        currentDistance = 0;
    //        latestPlayerPosition = player.transform.position.ToInt3();
    //    }

    //    for (int i = 0; i < chunkJobManager.FinishedJobsCount && i < amountDrawChunksPerFrame; i++)
    //    {
    //        ChunkJob task = chunkJobManager.DequeueFinishedJobs();

    //        if (task != null && task.Completed)
    //        {
    //            RenderCall(task);
    //        }
    //    }
    //}

    private void RenderCall(ChunkJob t)
    {
        //Ab hier wieder synchron auf Mainthread
        var newChunk = t.Chunk;

        if (newChunk.CurrentGO == null)
            newChunk.CurrentGO = GoPool.GetNextUnusedChunk();

        newChunk.CurrentGO.SetActive(true);
        newChunk.CurrentGO.transform.position = newChunk.Position.ToVector3();

        modifier.SetMesh(newChunk.CurrentGO, t.MeshData, t.ColliderData);
    }


    private void RecalculateChunks()
    {
        // 1) Iteriere durch alle Chunkpositionen in der Umgebung des Spielers
        // 2) Prüfe, ob die Chunkpositionen bereits gezeichnet sind
        // 2.a.1) Erstelle neuen Chunk
        // 2.a.2) Wenn ein Chunk noch nicht gezeichnet wurde, zeichne diesen Chunk
        // 2.a.3) Füge den neu gezeichneten Chunk zu den aktuell gezeichneten Chunks hinzu
        // 2.b.1) Wenn ein Chunk bereits gezeichnet wurde, aber nicht mehr im Umgebung, entferne den Chunk
        // 2.b.2) Entferne alle Referenzen zu diesem Chunk und gib das GameObject wieder frei
        // 3) Berechne Chunks neu. (Nicht alle, nur die Am Rand und die neu hinzugefügten


        // 1)


        int xStart = MathHelper.ClosestMultiple(latestPlayerPosition.X, chunkSize);
        int yStart = MathHelper.ClosestMultiple(0, chunkSize); // Fix height
        int zStart = MathHelper.ClosestMultiple(latestPlayerPosition.Z, chunkSize);


        for (int x = xStart - drawDistance.X; x < xStart + drawDistance.X; x += chunkSize)
        {
            //Vielleicht yStart?
            for (int y = maxYHeight; y > -drawDistance.Y; y -= chunkSize) // Minus to calculate chunks downwards, not upwards
            {
                for (int z = zStart - drawDistance.Z; z < zStart + drawDistance.Z; z += chunkSize)
                {
                    // 2)
                    Int3 chunkPos = new Int3(x, y, z);

                    if (!HashSetPositionChecker.Contains(chunkPos)) //Wenn man innerhalb der neuen Position einen Chunk braucht
                    {
                        //Wird in ChunkJob zum Hash hinzugefügt
                        ChunkJob job = new ChunkJob(chunkPos);
                        chunkJobManager.Add(job);
                    }
                }
            }
        }


        //Kann eigentlich asynchron laufen
        var list = ChunkDictionary.GetActiveChunks();

        for (int i = 0; i < list.Count; i++)
        {
            var currentChunk = list[i];
            if (Mathf.Abs(currentChunk.Position.X - xStart) > drawDistance.X + chunkSize ||
                Mathf.Abs(currentChunk.Position.Y - yStart) > drawDistance.Y + chunkSize ||
                Mathf.Abs(currentChunk.Position.Z - zStart) > drawDistance.Z + chunkSize)
            {
                currentChunk.ReleaseGameObject();
                ChunkDictionary.Remove(currentChunk.Position);
                HashSetPositionChecker.Remove(currentChunk.Position);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && drawLatestPlayerPosition)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(latestPlayerPosition.ToVector3(), 2f);
        }
    }

    private void OnDestroy()
    {
        chunkJobManager?.Dispose();
    }
}