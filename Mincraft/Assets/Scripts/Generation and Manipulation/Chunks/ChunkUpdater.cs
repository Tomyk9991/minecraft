using System.Collections.Generic;
using UnityEditor.VersionControl;
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
    private int maxHeight;

    private Int3 latestPlayerPosition;

    private ChunkJobManager chunkJobManager;
    private MeshModifier modifier;
    private ChunkCleanup cleanup;

    private SavingJob savingJob;

    private bool done = false;
    private int counter = 0;

    private void Start()
    {
        maxHeight = ChunkSettings.MaxYHeight;
        modifier = new MeshModifier();
        chunkJobManager = new ChunkJobManager(true);
        chunkJobManager.Start();

        latestPlayerPosition = player.transform.position.ToInt3();

        drawDistance = ChunkSettings.DrawDistance;
        chunkSize = ChunkSettings.ChunkSize;
        GoPool = ChunkGameObjectPool.Instance;

        cleanup = new ChunkCleanup(drawDistance, chunkSize);

        int xStart = MathHelper.ClosestMultiple(latestPlayerPosition.X, chunkSize);
        int yStart = MathHelper.ClosestMultiple(latestPlayerPosition.Y, chunkSize);
        int zStart = MathHelper.ClosestMultiple(latestPlayerPosition.Z, chunkSize);

        RecalculateChunks(xStart, yStart, zStart);
    }

    private void Update()
    {
        int xStart = MathHelper.ClosestMultiple(latestPlayerPosition.X, chunkSize);
        int yStart = MathHelper.ClosestMultiple(latestPlayerPosition.Y, chunkSize);
        int zStart = MathHelper.ClosestMultiple(latestPlayerPosition.Z, chunkSize);

        RecalculateChunks(xStart, yStart, zStart);
        CleanupChunks(xStart, yStart, zStart);

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
            newChunk.CurrentGO.SetActive(true);
            newChunk.CurrentGO.transform.position = newChunk.Position.ToVector3();
            newChunk.CurrentGO.name = newChunk.Position.ToString();

            modifier.SetMesh(newChunk.CurrentGO, t.MeshData, t.ColliderData);
        }
    }


    private void RecalculateChunks(int xStart, int yStart, int zStart)
    {
        if (yStart >= maxHeight) return;

        System.Threading.Tasks.Task.Run(() =>
        {
            for (int x = xStart - drawDistance.X; x < xStart + drawDistance.X; x += chunkSize)
            {
                for(int y = yStart + drawDistance.Y; y >= yStart - drawDistance.Y; y -= chunkSize)
                {
                    for (int z = zStart - drawDistance.Z; z < zStart + drawDistance.Z; z += chunkSize)
                    {
                        // 2)
                        Int3 chunkPos = new Int3(x, y, z);
                        
                        if(!HashSetPositionChecker.Contains(chunkPos))
                        {
                            ChunkJob job = new ChunkJob();
                            Chunk createdChunk = job.CreateChunk(chunkPos); //Reference to created Chunk

                            HashSetPositionChecker.Add(chunkPos);
                            createdChunk.AddedToHash = true;
                            ChunkDictionary.Add(chunkPos, createdChunk);
                            createdChunk.AddedToDick = true;

                            chunkJobManager.Add(job);
                        }
                    }
                }
            }
        });
    }

    private void CleanupChunks(int xStart, int yStart, int zStart)
    {
        cleanup.CheckChunks(xStart, yStart, zStart);
        cleanup.RemoveChunks();
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