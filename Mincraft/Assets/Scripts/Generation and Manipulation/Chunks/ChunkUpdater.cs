﻿using System.Collections.Generic;
using Attributes;
using Core.Chunks.Threading;
using Core.Managers;
using Core.Math;
using Core.Player;
using Core.Saving;
using Extensions;
using UnityEngine;
using Timer = Utilities.Timer;

namespace Core.Chunks
{
    public class ChunkUpdater : SingletonBehaviour<ChunkUpdater>
    {
        [SerializeField] private bool moveWithPlayer = true;
        [SerializeField, Range(0, 12)] private int drawDistanceInChunks = 12;

        [SerializeField] private bool calculateThreads = false;

        [DrawIfFalse(nameof(calculateThreads)), SerializeField]
        private int amountThreads = 0;

        private int chunkSize;

        private int minHeight;
        private int maxHeight;
        private int dimension;

        private JobManager _jobManager;

        private SavingJob savingJob;

        private Queue<Direction> shiftDirections = new Queue<Direction>();
        private Timer timer;

        private readonly DrawingState drawingStateMask = DrawingState.NoiseReady | DrawingState.Drawn;

        private void Start()
        {
            int xPlayerPos = PlayerMovementTracker.Instance.xPlayerPos;
            int zPlayerPos = PlayerMovementTracker.Instance.zPlayerPos;

            PlayerMovementTracker.OnDirectionModified += DirectionModified;

            var minMaxYHeight = WorldSettings.MinMaxYHeight;
            minHeight = minMaxYHeight.X;
            maxHeight = minMaxYHeight.Y;


            if (calculateThreads)
            {
                amountThreads = SystemInfo.processorCount - 5 <= 0 ? 1 : SystemInfo.processorCount - 5;
            }

            _jobManager = new JobManager(amountThreads, true);
            _jobManager.Start();

            chunkSize = 0x10;
            ChunkBuffer.Init(chunkSize, minHeight, maxHeight, drawDistanceInChunks);

            dimension = ChunkBuffer.Dimension;
            timer = new Timer(WorldSettings.WorldTick);
            
            SetupChunkBuffer(xPlayerPos, zPlayerPos);
        }
        
        private void SetupChunkBuffer(in int xPlayerPos, in int zPlayerPos)
        {
            for (int x = xPlayerPos - (drawDistanceInChunks * chunkSize), localx = 0;
                x <= xPlayerPos + (drawDistanceInChunks * chunkSize);
                x += chunkSize, localx++)
            {
                for (int z = zPlayerPos - (drawDistanceInChunks * chunkSize), localz = 0;
                    z <= zPlayerPos + (drawDistanceInChunks * chunkSize);
                    z += chunkSize, localz++)
                {
                    //Create chunkColumn
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
                    
                    _jobManager.Add(new ChunkJob(column));
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
            if (timer.TimeElapsed(Time.deltaTime))
            {
                if(shiftDirections.Count > 0 && _jobManager.MeshJobsCount == 0)
                {
                    ChunkBuffer.Shift(shiftDirections.Dequeue());
                }
            }
        }

        private void OnDestroy()
        {
            _jobManager?.Dispose();
        }
    }
}
