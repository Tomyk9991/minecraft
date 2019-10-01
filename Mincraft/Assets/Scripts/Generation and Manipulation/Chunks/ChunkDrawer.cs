using Core.Builder;
using Core.Chunking.Threading;
using Core.Math;
using UnityEngine;

namespace Core.Chunking
{
    public class ChunkDrawer : MonoBehaviour
    {
        public ChunkGameObjectPool GoPool { get; set; }
        [SerializeField] private int amountDrawChunksPerFrame = 5;

        private Int3 drawDistance;
        private int chunkSize;
        private Int3 latestPlayerPosition;


        private MeshModifier modifier;
        private ChunkJobManager chunkJobManager;
        private ChunkUpdater updater;

        private void Start()
        {
            GoPool = ChunkGameObjectPool.Instance;

            drawDistance = ChunkSettings.DrawDistance;
            chunkSize = ChunkSettings.ChunkSize;

            chunkJobManager = ChunkJobManager.ChunkJobManagerUpdaterInstance;
            updater = ChunkUpdater.Instance;

            modifier = new MeshModifier();
        }

        private void Update()
        {
            latestPlayerPosition = updater.LatestPlayerPosition;
            int xPlayerPos = MathHelper.ClosestMultiple(latestPlayerPosition.X, chunkSize);
            int zPlayerPos = MathHelper.ClosestMultiple(latestPlayerPosition.Z, chunkSize);


            for (int i = 0; i < chunkJobManager.FinishedJobsCount && i < amountDrawChunksPerFrame; i++)
            {
                ChunkJob task = chunkJobManager.DequeueFinishedJobs();

                if (task != null && task.Completed && InsideDrawDistance(task.Chunk.GlobalPosition, xPlayerPos, zPlayerPos) && task.MeshData.Vertices.Count != 0)
                {
                    RenderCall(task);
                }
            }
        }

        private void RenderCall(ChunkJob t)
        {
            //Ab hier wieder synchron auf Mainthread
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
                modifier.SetMesh(drawingChunk.CurrentGO, t.MeshData, t.ColliderData);
                drawingChunk.ChunkState = ChunkState.Drawn;
            }
        }

        private bool InsideDrawDistance(Int3 position, int xPos, int zPos)
        {
            return xPos - drawDistance.X <= position.X && xPos + drawDistance.X >= position.X &&
                   zPos - drawDistance.Z <= position.Z && zPos + drawDistance.Z >= position.Z;
        }
    }
}
