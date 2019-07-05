using System.Collections.Generic;
using UnityEngine;

public class ChunkUpdater : MonoBehaviour
{
    public ChunkGameObjectPool GoPool { get; set; }

    [Header("References")]
    [SerializeField] private GameObject player = null;

    [SerializeField] private int amountDrawChunksPerFrame = 5;
    [Header("Debug")]
    [SerializeField] private bool drawLatestPlayerPosition = false;

    private Int3 drawDistance;
    private int chunkSize;

    private Int3 latestPlayerPosition;

    private ChunkJobManager chunkJobManager;
    private MeshModifier modifier;

    private SavingJob savingJob;

    private void Start()
    {
        modifier = new MeshModifier();
        chunkJobManager = new ChunkJobManager();
        chunkJobManager.Start();

        latestPlayerPosition = player.transform.position.ToInt3();
        drawDistance = ChunkSettings.DrawDistance;

        chunkSize = ChunkSettings.ChunkSize;

        GoPool = ChunkGameObjectPool.Instance;

        RecalculateChunks();
    }

    private void Update()
    {
        RecalculateChunks();

        latestPlayerPosition = player.transform.position.ToInt3();

        for (int i = 0; i < chunkJobManager.FinishedJobsCount && i < amountDrawChunksPerFrame; i++)
        {
            ChunkJob task = chunkJobManager.DequeueFinishedJobs();

            if (task != null && task.Completed)
            {
                RenderCall(task);
            }
        }
    }

    private void RenderCall(ChunkJob t)
    {
        //Ab hier wieder synchron auf Mainthread
        var newChunk = t.Chunk;

        if (newChunk.CurrentGO == null)
        {
            newChunk.CurrentGO = GoPool.GetNextUnusedChunk();
        }

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
        int yStart = MathHelper.ClosestMultiple(latestPlayerPosition.Y, chunkSize);
        int zStart = MathHelper.ClosestMultiple(latestPlayerPosition.Z, chunkSize);



        for (int x = xStart - drawDistance.X / 2; x < xStart + drawDistance.X / 2; x += chunkSize)
        {
            for(int y = yStart + drawDistance.Y / 2; y >= yStart - drawDistance.Y / 2; y -= chunkSize)
            {
                for (int z = zStart - drawDistance.Z / 2; z < zStart + drawDistance.Z / 2; z += chunkSize)
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
            if (Mathf.Abs(currentChunk.Position.X - xStart) > drawDistance.X / 2 + chunkSize ||
                Mathf.Abs(currentChunk.Position.Y - yStart) > drawDistance.Y / 2 + chunkSize * 2 ||
                Mathf.Abs(currentChunk.Position.Z - zStart) > drawDistance.Z / 2 + chunkSize)
            {
                if (currentChunk.CurrentGO != null)
                {
                    currentChunk.ReleaseGameObject();
                    ChunkDictionary.Remove(currentChunk.Position);
                    HashSetPositionChecker.Remove(currentChunk.Position);
                }
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

    TreeGenerator treeGenerator = null;
    Int3[] treeCubes;
    private void OnDrawGizmosSelected() 
    {
        if (treeGenerator == null) 
        {
            treeGenerator = new TreeGenerator(new Int2(10, 15), new Int2(7, 10));
            treeCubes = treeGenerator.Generate(Int3.Zero);
        }
        for (int i = 0; i < treeCubes.Length; i++)
        {
            Gizmos.color = i < treeGenerator.MaxWood ? new Color(139f / 255f, 69f / 255f, 19f / 255f) : Color.green;
            Gizmos.DrawCube(treeCubes[i].ToVector3(), Vector3.one);
        }
    }

    private void OnDestroy()
    {
        chunkJobManager?.Dispose();
    }
}