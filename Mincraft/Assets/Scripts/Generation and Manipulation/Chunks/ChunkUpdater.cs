using System.Threading.Tasks;
using Extensions;
using UnityEngine;

using Core.Chunking.Threading;
using Core.Math;
using Core.Saving;

namespace Core.Chunking
{
    public class ChunkUpdater : SingletonBehaviour<ChunkUpdater>
    {
        public Int3 LatestPlayerPosition => latestPlayerPosition;
        [SerializeField] private bool moveWithPlayer = true;
        [Header("References")]
        [SerializeField] private GameObject player = null;

        private Int3 drawDistance;
        private int chunkSize;
        
        private int minHeight;
        private int maxHeight;

        private Int3 latestPlayerPosition;


        private ChunkJobManager chunkJobManager;
        private ChunkCleanup cleanup;


        private SavingJob savingJob;

        //Tasking
        private object _mutexManager = new object();
        private bool isRecalculating = false;

        private void Start()
        {
            var minMaxYHeight = ChunkSettings.MinMaxYHeight;
            minHeight = minMaxYHeight.X;
            maxHeight = minMaxYHeight.Y;
            
            chunkJobManager = new ChunkJobManager(true);
            chunkJobManager.Start();

            latestPlayerPosition = player.transform.position.ToInt3();

            drawDistance = ChunkSettings.DrawDistance;
            chunkSize = ChunkSettings.ChunkSize;

            cleanup = new ChunkCleanup(drawDistance, chunkSize);
            
            int xPlayerPos = MathHelper.ClosestMultiple(latestPlayerPosition.X, chunkSize);
            int zPlayerPos = MathHelper.ClosestMultiple(latestPlayerPosition.Z, chunkSize);

            StartTaskedProcess(xPlayerPos, zPlayerPos);
        }

        private void Update()
        {
            if (!moveWithPlayer) return;

            int xPlayerPos = MathHelper.ClosestMultiple(latestPlayerPosition.X, chunkSize);
            int zPlayerPos = MathHelper.ClosestMultiple(latestPlayerPosition.Z, chunkSize);
            
            
            if (!isRecalculating)
                StartTaskedProcess(xPlayerPos, zPlayerPos);

            latestPlayerPosition = player.transform.position.ToInt3();
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
