using Core.Builder;
using Core.Chunks.Threading;
using Extensions;
using UnityEngine;

namespace Core.Chunks
{
    public class ChunkDrawer : SingletonBehaviour<ChunkDrawer>
    {
        public ChunkGameObjectPool GoPool { get; set; }
        [SerializeField] private int drawsPerFrame = 0;

        private ChunkJobManager _chunkJobManager;

        private int jobsDoneInFrame = 0;

        private void Start()
        {
            GoPool = ChunkGameObjectPool.Instance;
            _chunkJobManager = ChunkJobManager.ChunkJobManagerUpdaterInstance;
        }

        private void Update()
        {
            while (_chunkJobManager.FinishedJobsCount > 0 && jobsDoneInFrame < drawsPerFrame)
            {
                MeshJob task = _chunkJobManager.DequeueFinishedJob();
                if (task?.MeshData.Vertices != null && task.MeshData.Vertices.Count != 0)
                {
                    RenderCall(task);
                    jobsDoneInFrame++;
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
                drawingChunk.CurrentGO.name = drawingChunk.GlobalPosition.ToString();
                drawingChunk.CurrentGO.transform.position = drawingChunk.GlobalPosition.ToVector3();
                drawingChunk.CurrentGO.GetComponent<ChunkReferenceHolder>().Chunk = drawingChunk;
            }

            MeshModifier.SetMesh(drawingChunk.CurrentGO, t.MeshData, t.ColliderData);
        }
    }
}