using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core;
using Core.Chunking;
using Core.Chunking.Threading;
using Core.Math;
using Core.Player;
using Core.Saving;
using Extensions;
using UnityEngine;
using UnityInspector.PropertyAttributes;
using Timer = Utilities.Timer;

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


    private Thread worker;
    private bool workerWorking = false;

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
        SetupWorker();
    }

    private void SetupWorker()
    {
        worker = new Thread(() =>
        {
            while (workerWorking)
            {
                for (int x = 1; x < dimension - 1; x++)
                {
                    for (int z = 1; z < dimension - 1; z++)
                    {
                        ChunkColumn column = ChunkBuffer.GetChunkColumn(x, z);

                        if (column.DesiredForVisualization && (column.State == DrawingState.NoiseReady))
                        {
                            ChunkColumn[] neighbours = column.ChunkColumnNeighbours();

                            if (neighbours.All(c => (c.State & drawingStateMask) != 0))
                            {
                                if (column.State == DrawingState.NoiseReady)
                                {
                                    for (int h = minHeight, localy = 0; h < maxHeight; h += chunkSize, localy++)
                                    {
                                        Chunk chunk = column[localy];
                                        _jobManager.CreateChunkFromExistingAndAddJob(chunk);
                                    }

                                    column.State = DrawingState.InDrawingQueue;
                                }
                            }
                        }

                        if (column.Dirty && column.State == DrawingState.Drawn)
                        {
                            for (int h = minHeight, localy = 0; h < maxHeight; h += chunkSize, localy++)
                            {
                                Chunk chunk = column[localy];
                                if (chunk.ChunkState == ChunkState.Dirty)
                                {
                                    _jobManager.CreateChunkFromExistingAndAddJob(chunk);
                                }
                            }

                            column.Dirty = false;
                            column.State = DrawingState.InDrawingQueue;
                        }
                    }
                }
            }
        });

        workerWorking = true;
        worker.Start();
    }

    private void SetupChunkBuffer(in int xPlayerPos, in int zPlayerPos)
    {
        for (int x = xPlayerPos - (drawDistanceInChunks * chunkSize) - chunkSize, localx = 0;
            x <= xPlayerPos + (drawDistanceInChunks * chunkSize) + chunkSize;
            x += chunkSize, localx++)
        {
            for (int z = zPlayerPos - (drawDistanceInChunks * chunkSize) - chunkSize, localz = 0;
                z <= zPlayerPos + (drawDistanceInChunks * chunkSize) + chunkSize;
                z += chunkSize, localz++)
            {
                //Create chunkColumn
                ChunkColumn column = new ChunkColumn(new Int2(x, z), new Int2(localx, localz), minHeight, maxHeight);
                ChunkBuffer.SetChunkColumnNonThreadSafe(localx, localz, column);

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

                _jobManager.Add(new NoiseJob
                {
                    Column = column
                });

                column.DesiredForVisualization = localx != 0 && localx != dimension - 1 &&
                                                 localz != 0 && localz != dimension - 1;
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
        // if (!isChecking)
        //     isChecking = true;
        
        
        // for (int x = 1; x < dimension - 1; x++)
        // {
        //     for (int z = 1; z < dimension - 1; z++)
        //     {
        //         ChunkColumn column = ChunkBuffer.GetChunkColumn(x, z);
        //
        //         if (column.DesiredForVisualization && (column.State == DrawingState.NoiseReady))
        //         {
        //             ChunkColumn[] neighbours = column.ChunkColumnNeighbours();
        //
        //             if (neighbours.All(c => (c.State & drawingStateMask) != 0))
        //             {
        //                 if (column.State == DrawingState.NoiseReady)
        //                 {
        //                     for (int h = minHeight, localy = 0; h < maxHeight; h += chunkSize, localy++)
        //                     {
        //                         Chunk chunk = column[localy];
        //                         _jobManager.CreateChunkFromExistingAndAddJob(chunk);
        //                     }
        //
        //                     column.State = DrawingState.InDrawingQueue;
        //                 }
        //             }
        //         }
        //
        //         if (column.Dirty && column.State == DrawingState.Drawn)
        //         {
        //             for (int h = minHeight, localy = 0; h < maxHeight; h += chunkSize, localy++)
        //             {
        //                 Chunk chunk = column[localy];
        //                 if (chunk.ChunkState == ChunkState.Dirty)
        //                 {
        //                     _jobManager.CreateChunkFromExistingAndAddJob(chunk);
        //                 }
        //             }
        //
        //             column.Dirty = false;
        //             column.State = DrawingState.InDrawingQueue;
        //         }
        //     }
        // }

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
        workerWorking = false;
        worker?.Join();

        _jobManager?.Dispose();
    }
}