using Core.Builder;
using Core.Chunking;
using Core.Chunking.Threading;
using Extensions;
using UnityEngine;

public class ChunkDrawer : SingletonBehaviour<ChunkDrawer>
{
    public ChunkGameObjectPool GoPool { get; set; }
    [SerializeField] private int drawsPerFrame = 2;
    [SerializeField] private Material standardMaterial = null;
    
    
    private JobManager _jobManager;
    
    private int jobsDoneInFrame = 0;

    private void Start()
    {
        GoPool = ChunkGameObjectPool.Instance;
        _jobManager = JobManager.JobManagerUpdaterInstance;
    }

    private void Update()
    {
        while (_jobManager.FinishedJobsCount > 0 && jobsDoneInFrame < drawsPerFrame)
        {
            MeshJob task = _jobManager.DequeueFinishedJob();

            if (task.MeshData.Vertices != null && task.MeshData.Vertices.Count != 0)
            {
                jobsDoneInFrame++;
                RenderCall(task);
            }
        }

        jobsDoneInFrame = 0;
    }


    private void RenderCall(MeshJob t)
    {
        var drawingChunk = t.Chunk;
        
        if (drawingChunk.CurrentGO == null)
        {
            drawingChunk.CurrentGO = GoPool.GetNextUnusedChunk();
            drawingChunk.CurrentGO.SetActive(true);
            drawingChunk.CurrentGO.transform.position = drawingChunk.GlobalPosition.ToVector3();
        }
        
        MeshModifier.SetMesh(drawingChunk.CurrentGO, t.MeshData, t.ColliderData);

        drawingChunk.ChunkState = ChunkState.Drawn;
        drawingChunk.ChunkColumn.State = DrawingState.Drawn;
    }

}