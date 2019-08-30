using System.Collections.Generic;
using System.Threading.Tasks;
using Extensions;
using UnityEngine;

using Core.Builder;
using Core.Chunking.Threading;
using Core.Math;
using Core.Saving;

namespace Core.Chunking
{
    public class ChunkUpdater : SingletonBehaviour<ChunkUpdater>
    {
        public ChunkGameObjectPool GoPool { get; set; }

        [Header("References")]
        [SerializeField] private GameObject player = null;

        [SerializeField] private int amountDrawChunksPerFrame = 5;

        private Int3 drawDistance;
        private int chunkSize;
        
        private int minHeight;
        private int maxHeight;

        private Int3 latestPlayerPosition;
        private Int3 previousPlayerPosition = Int3.Zero; 
        

        private ChunkJobManager chunkJobManager;
        private MeshModifier modifier;
        private ChunkCleanup cleanup;

        public List<Chunk> chunks;

        private SavingJob savingJob;

        //Tasking
        private object _mutexChunks = new object();
        private object _mutexManager = new object();
        private bool isRecalculating = false;
        //Counter, if chunk needs to be recalculated
        private float timeElapsed = 0f;
        //Time in seconds determining the period of recalculating chunks
        private float timeThreshhold = 5f;

        private void Start()
        {
            var minMaxYHeight = ChunkSettings.MinMaxYHeight;
            minHeight = minMaxYHeight.X;
            maxHeight = minMaxYHeight.Y;
            
            modifier = new MeshModifier();
            chunkJobManager = new ChunkJobManager(true);
            chunkJobManager.Start();

            latestPlayerPosition = player.transform.position.ToInt3();

            drawDistance = ChunkSettings.DrawDistance;
            chunkSize = ChunkSettings.ChunkSize;
            GoPool = ChunkGameObjectPool.Instance;

            cleanup = new ChunkCleanup(drawDistance, chunkSize);
            chunks = new List<Chunk>();

            int xPlayerPos = MathHelper.ClosestMultiple(latestPlayerPosition.X, chunkSize);
            int zPlayerPos = MathHelper.ClosestMultiple(latestPlayerPosition.Z, chunkSize);

            StartTaskedProcess(xPlayerPos, zPlayerPos);
        }

        private void Update()
        {
            timeElapsed += Time.deltaTime;
            
            int xPlayerPos = MathHelper.ClosestMultiple(latestPlayerPosition.X, chunkSize);
            int yPlayerPos = MathHelper.ClosestMultiple(latestPlayerPosition.Y, chunkSize);
            int zPlayerPos = MathHelper.ClosestMultiple(latestPlayerPosition.Z, chunkSize);


            Int3 temp = new Int3(xPlayerPos, yPlayerPos, zPlayerPos);

            if ((previousPlayerPosition != temp) && !isRecalculating || timeElapsed > timeThreshhold)
            {
                timeElapsed = 0f;
                previousPlayerPosition = temp;

                StartTaskedProcess(xPlayerPos, zPlayerPos);
            }

            latestPlayerPosition = player.transform.position.ToInt3();

            for (int i = 0; i < chunkJobManager.FinishedJobsCount && i < amountDrawChunksPerFrame; i++)
            {
                ChunkJob task = chunkJobManager.DequeueFinishedJobs();
                if (task != null && task.Completed && InsideDrawDistance(task.Chunk.GlobalPosition, xPlayerPos, zPlayerPos) && task.HasBlocks)
                {
                    RenderCall(task);
                }
            }
        }

        private void StartTaskedProcess(int xPlayerPos, int zPlayerPos)
        {
            Task.Run(() =>
            {
                isRecalculating = true;
                RecalculateChunks(xPlayerPos, zPlayerPos);
                cleanup.CheckChunks(xPlayerPos, zPlayerPos);
                    
                isRecalculating = false;
            });
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
        }

        private bool InsideDrawDistance(Int3 position, int xPos, int zPos)
        {
            return xPos - drawDistance.X <= position.X && xPos + drawDistance.X >= position.X &&
                   zPos - drawDistance.Z <= position.Z && zPos + drawDistance.Z >= position.Z;
        }

        private void RecalculateChunks(int xPlayerPos, int zPlayerPos)
        {
            for (int x = xPlayerPos - drawDistance.X; x < xPlayerPos + drawDistance.X; x += chunkSize)
            {
                for (int z = zPlayerPos - drawDistance.Z; z < zPlayerPos + drawDistance.Z; z += chunkSize)
                {
                    Int2 chunkGlobalPosXZ = new Int2(x, z);
                    
                    if (!HashSetPositionChecker.Contains(chunkGlobalPosXZ))
                    {
                        HashSetPositionChecker.Add(chunkGlobalPosXZ);
                        for (int y = minHeight; y < maxHeight; y += chunkSize)
                        {
                            //(-32, 16, -32)
                            Int3 chunkGlobalPos = new Int3(x, y, z);

                            Int3 chunkClusterPosition = new Int3(MathHelper.MultipleFloor(x, chunkSize * chunkSize),
                                                                 MathHelper.MultipleFloor(y, chunkSize * chunkSize),
                                                                 MathHelper.MultipleFloor(z, chunkSize * chunkSize));
                            
                            //(32, 0, 0) = (32, 0, 0) - (0, 0, 0)
                            Int3 chunkLocalPos = chunkGlobalPos - chunkClusterPosition;
                            //(2, 0, 0)
                            chunkLocalPos /= 16;
                            
                            ChunkJob job = new ChunkJob();
                            Chunk createdChunk = job.CreateChunk(chunkLocalPos, chunkClusterPosition); //Reference to created Chunk

                            lock (_mutexChunks)
                            {
                                chunks.Add(createdChunk);
                            }
                            
                            createdChunk.AddedToHash = true;
                            ChunkCluster cluster = ChunkClusterDictionary.Add(chunkClusterPosition, createdChunk);
                            createdChunk.AddedToDick = true;

                            createdChunk.Cluster = cluster;
                            createdChunk.ChunkState = ChunkState.Created;

                            lock (_mutexManager)
                            {
                                chunkJobManager.Add(job);
                            }
                        }
                    }
                    
                }
            }
        }

        private void OnDestroy()
        {
            chunkJobManager?.Dispose();
        }
    }
}
