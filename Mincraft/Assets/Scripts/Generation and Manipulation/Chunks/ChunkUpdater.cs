using System.Collections.Generic;
using Attributes;
using Core.Chunks.Threading;
using Core.Managers;
using Core.Math;
using Core.Player;
using Extensions;
using UnityEngine;
using Timer = Utilities.Timer;

namespace Core.Chunks
{
    public class ChunkUpdater : SingletonBehaviour<ChunkUpdater>
    {
        [SerializeField] private bool moveWithPlayer = true;
        [SerializeField, Range(0, 24)] private int drawDistanceInChunks = 12;

        [SerializeField] private bool calculateThreads = false;

        [DrawIfFalse(nameof(calculateThreads)), SerializeField]
        private int amountThreads = 0;

        private int chunkSize;

        private int minHeight;
        private int maxHeight;

        private ChunkJobManager _chunkJobManager;

        private Queue<Direction> shiftDirections = new Queue<Direction>();
        private Timer timer;

        private void Start()
        {
            chunkSize = 0x10;
            int xPlayerPos = 0;
            int zPlayerPos = 0;

            PlayerMovementTracker.OnDirectionModified += DirectionModified;

            Int2 minMaxYHeight = WorldSettings.MinMaxYHeight;
            minHeight = minMaxYHeight.X;
            maxHeight = minMaxYHeight.Y;


            if (calculateThreads)
                amountThreads = SystemInfo.processorCount - 5 <= 0 ? 1 : SystemInfo.processorCount - 5;

            _chunkJobManager = new ChunkJobManager(amountThreads, true);

            ChunkBuffer.Init(chunkSize, minHeight, maxHeight, drawDistanceInChunks);

            _chunkJobManager.PassBegin();
            SetupChunkBuffer(xPlayerPos, zPlayerPos);
            _chunkJobManager.PassEnd();


            _chunkJobManager.Start();

            timer = new Timer(WorldSettings.WorldTick);
        }

        private void SetupChunkBuffer(in int xPlayerPos, in int zPlayerPos)
        {
            for (int x = xPlayerPos - drawDistanceInChunks * chunkSize, localx = 0;
                x <= xPlayerPos + drawDistanceInChunks * chunkSize;
                x += chunkSize, localx++)
            {
                for (int z = zPlayerPos - drawDistanceInChunks * chunkSize, localz = 0;
                    z <= zPlayerPos + drawDistanceInChunks * chunkSize;
                    z += chunkSize, localz++)
                {
                    ChunkColumn column = new ChunkColumn(new Int2(x, z), new Int2(localx, localz), minHeight, maxHeight);
                    ChunkBuffer.SetChunkColumnNTS(localx, localz, column);
                    
                    //Insert this created chunkColumn to the NoiseJobs 
                    for (int y = minHeight, localy = 0; y < maxHeight; y += chunkSize, localy++)
                    {
                        Chunk chunk = new Chunk()
                        {
                            LocalPosition = new Int3(localx, localy, localz),
                            GlobalPosition = new Int3(x, y, z),
                            ChunkColumn = column
                        };
                    
                        column[localy] = chunk;
                    }
                    
                    _chunkJobManager.Add(new ChunkJob(column));
                }
            }
        }

        private void DirectionModified(Direction direction)
        {
            if (moveWithPlayer)
                shiftDirections.Enqueue(direction);
        }
        
        private void Update()
        {
            if (!timer.TimeElapsed(Time.deltaTime)) return;
            
            if(shiftDirections.Count > 0 && _chunkJobManager.MeshJobsCount == 0 && _chunkJobManager.FinishedJobsCount == 0 && _chunkJobManager.NoiseJobsCount == 0)
                ChunkBuffer.Shift(shiftDirections.Dequeue());
        } 

        private void OnDestroy()
        {
            _chunkJobManager?.Dispose();
            PlayerMovementTracker.OnDirectionModified -= (Direction d) => { };
        }
    }
}
