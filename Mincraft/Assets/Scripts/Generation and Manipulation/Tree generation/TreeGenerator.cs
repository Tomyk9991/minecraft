using System;
using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;

public class TreeGenerator : MonoBehaviour
{
    public ChunkGameObjectPool GoPool { get; set; }
    
    private Int3 drawDistance;
    private int chunkSize = 16;
    
    private MeshModifier modifier;
    private ChunkJobManager chunkJobManager;
    
    private void Start()
    {
        drawDistance = ChunkSettings.DrawDistance;
        chunkSize = ChunkSettings.ChunkSize;
        GoPool = ChunkGameObjectPool.Instance;
        
        modifier = new MeshModifier();
        chunkJobManager = new ChunkJobManager();
        chunkJobManager.Start();

        RecalculateChunks();
    }

    private void Update()
    {
        for (int i = 0; i < chunkJobManager.FinishedJobsCount; i++)
        {
            ChunkJob task = chunkJobManager.DequeueFinishedJobs();

            if (task != null && task.Completed)
            {
                RenderCall(task);
            }
        }
    }

    private void RecalculateChunks()
    {
        for (int x = -(drawDistance.X / 2); x < drawDistance.X / 2; x += chunkSize)
        {
            //Vielleicht yStart?
            //for (int y = yStart; y > -drawDistance.Y; y -= chunkSize) // Minus to calculate chunks downwards, not upwards
            for(int y = +drawDistance.Y / 2; y >= -drawDistance.Y / 2; y -= chunkSize)
            {
                for (int z = -(drawDistance.Z / 2); z < drawDistance.Z / 2; z += chunkSize)
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

    private void OnDestroy()
    {
        chunkJobManager?.Dispose();
    }
}
