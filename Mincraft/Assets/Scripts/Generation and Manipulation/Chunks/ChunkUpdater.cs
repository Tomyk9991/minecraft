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
        chunkJobManager = new ChunkJobManager(true);
        chunkJobManager.Start();

        latestPlayerPosition = player.transform.position.ToInt3();
        drawDistance = ChunkSettings.DrawDistance;

        chunkSize = ChunkSettings.ChunkSize;

        GoPool = ChunkGameObjectPool.Instance;

        RecalculateChunks();
    }

    private void Update()
    {
        //RecalculateChunks();

        //latestPlayerPosition = player.transform.position.ToInt3();

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


    private void RecalculateChunks()
    {
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
                    //Chunk c = ChunkDictionary.GetValue(chunkPos);

                    //if (c == null)
                    if(!HashSetPositionChecker.Contains(chunkPos))
                    {
                        ChunkJob job = new ChunkJob();
                        Chunk createdChunk = job.CreateChunk(chunkPos); //Reference to created Chunk

                        HashSetPositionChecker.Add(chunkPos);
                        ChunkDictionary.Add(chunkPos, createdChunk);

                        chunkJobManager.Add(job);
                    }

                    //if (!HashSetPositionChecker.Contains(chunkPos)) //Wenn man innerhalb der neuen Position einen Chunk braucht
                    //{
                    //    //Wird in ChunkJob zum Hash hinzugefügt
                    //    ChunkJob job = new ChunkJob(chunkPos);
                    //    chunkJobManager.Add(job);
                    //}
                }
            }
        }


        //TODO: Kann eigentlich asynchron laufen
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

    private void OnDestroy()
    {
        chunkJobManager?.Dispose();
    }
}