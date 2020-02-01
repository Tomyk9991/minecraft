using Core.Builder;
using Core.Chunking;
using Core.Chunking.Threading;
using Extensions;
using UnityEngine;

public class ChunkDrawer : SingletonBehaviour<ChunkDrawer>
{
    public ChunkGameObjectPool GoPool { get; set; }
    [SerializeField] private int drawsPerUpdate = 2;
    
    private MeshJobManager _meshJobManager;
    private MeshModifier modifier;

    private void Start()
    {
        GoPool = ChunkGameObjectPool.Instance;
        _meshJobManager = MeshJobManager.MeshJobManagerUpdaterInstance;

        modifier = new MeshModifier();
    }

    private void Update()
    {
        for (int i = 0; i < _meshJobManager.FinishedJobsCount && i < drawsPerUpdate; i++)
        {
            MeshJob task = _meshJobManager.DequeueFinishedJob();

            if(task != null && task.Completed && task.MeshData.Vertices.Count != 0)
            {
                RenderCall(task);
            }
        }
    }

    private void RenderCall(MeshJob t)
    {
        var drawingChunk = t.Chunk;

        if (drawingChunk.CurrentGO == null)
        {
            drawingChunk.ChunkState = ChunkState.Drawn;
            drawingChunk.CurrentGO = GoPool.GetNextUnusedChunk();
            drawingChunk.CurrentGO.SetActive(true);
            drawingChunk.CurrentGO.transform.position = drawingChunk.GlobalPosition.ToVector3();
            drawingChunk.CurrentGO.name = drawingChunk.GlobalPosition.ToString();

            modifier.SetMesh(drawingChunk.CurrentGO, t.MeshData, t.ColliderData);
        }
        else
        {
            //These chunks are already drawn atm. and need a refresh
            drawingChunk.ChunkState = ChunkState.Drawn;
            modifier.SetMesh(drawingChunk.CurrentGO, t.MeshData, t.ColliderData);
        }
    }
}
