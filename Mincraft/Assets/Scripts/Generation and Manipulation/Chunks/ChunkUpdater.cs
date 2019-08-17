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
        [Header("Debug")]
        [SerializeField] private bool drawLatestPlayerPosition = false;

        private Int3 drawDistance;
        private int chunkSize;
        private int maxHeight;

        private Int3 latestPlayerPosition;

        [SerializeField] private Int3 test = Int3.Zero;
        private Int3 testLast = Int3.Zero; 
        

        private ChunkJobManager chunkJobManager;
        private MeshModifier modifier;
        private ChunkCleanup cleanup;

        private SavingJob savingJob;

        //Tasking
        private object _mutexChunks = new object();
        private object _mutexManager = new object();
        private bool isRecalculating = false;
        //Counter, if chunk needs to be recalculated
        private float timeElapsed = 0f;
        //Time in seconds determining the period of recalculating chunks
        private float timeThreshhold = 5f;

        public List<Chunk> chunks = new List<Chunk>();

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

            //TODO: Make Recalculate in a separat thread
            RecalculateChunks(xStart, yStart, zStart);
        }

        private void Update()
        {
            timeElapsed += Time.deltaTime;
            int xStart = MathHelper.ClosestMultiple(latestPlayerPosition.X, chunkSize);
            int yStart = MathHelper.ClosestMultiple(latestPlayerPosition.Y, chunkSize);
            int zStart = MathHelper.ClosestMultiple(latestPlayerPosition.Z, chunkSize);
            

            test.X = xStart;
            test.Y = yStart;
            test.Z = zStart;

            if ((testLast != test) && !isRecalculating || timeElapsed > timeThreshhold)
            {
                timeElapsed = 0f;
                testLast = test;

                Task.Run(() =>
                {
                    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                    isRecalculating = true;
                    cleanup.CheckChunks(xStart, yStart, zStart, chunks);
                    RecalculateChunks(xStart, yStart, zStart);
                    isRecalculating = false;
                    stopwatch.Stop();
                    Debug.Log("Berechnung: " + stopwatch.ElapsedMilliseconds + "ms");
                });
            }

            latestPlayerPosition = player.transform.position.ToInt3();

            for (int i = 0; i < chunkJobManager.FinishedJobsCount && i < amountDrawChunksPerFrame; i++)
            {
                ChunkJob task = chunkJobManager.DequeueFinishedJobs();
                if (task != null && task.Completed && InsideDrawDistance(task.Chunk.GlobalPosition, xStart, yStart, zStart))
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
        }

        private bool InsideDrawDistance(Int3 position, int xPos, int yPos, int zPos)
        {
            return xPos - drawDistance.X <= position.X && xPos + drawDistance.X >= position.X &&
                   yPos - drawDistance.Y <= position.Y && yPos + drawDistance.Y >= position.Y &&
                   zPos - drawDistance.Z <= position.Z && zPos + drawDistance.Z >= position.Z;
        }
        
        private void RecalculateChunks(int xStart, int yStart, int zStart)
        {
            if (yStart >= maxHeight) return;

            for (int x = xStart - drawDistance.X; x < xStart + drawDistance.X; x += chunkSize)
            {
                for(int y = yStart + drawDistance.Y; y >= yStart - drawDistance.Y; y -= chunkSize)
                {
                    for (int z = zStart - drawDistance.Z; z < zStart + drawDistance.Z; z += chunkSize)
                    {
                        // (-32, 16, -32)
                        Int3 chunkGlobalPos = new Int3(x, y, z); //Global chunk-Position

                        
                        if(!HashSetPositionChecker.Contains(chunkGlobalPos))
                        {
                            // (-256, 0, -256)
                            Int3 chunkClusterPosition = new Int3(MathHelper.MultipleFloor(x, chunkSize * chunkSize),
                                MathHelper.MultipleFloor(y, chunkSize * chunkSize),
                                MathHelper.MultipleFloor(z, chunkSize * chunkSize));

                            //  (32, 0, 0) = (32, 0, 0) - (0, 0, 0) 
                            Int3 chunkLocalPos = chunkGlobalPos - chunkClusterPosition;
                            // (2, 0, 0)
                            chunkLocalPos /= 16;
                            
                            ChunkJob job = new ChunkJob();
                            Chunk createdChunk = job.CreateChunk(chunkLocalPos, chunkClusterPosition); //Reference to created Chunk

                            lock (_mutexChunks)
                            {
                                chunks.Add(createdChunk);
                            }
                            
                            HashSetPositionChecker.Add(chunkGlobalPos);
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
}
