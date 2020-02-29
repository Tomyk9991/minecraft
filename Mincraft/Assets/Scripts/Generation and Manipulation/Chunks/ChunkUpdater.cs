using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core;
using Core.Chunking;
using Core.Chunking.Threading;
using Core.Math;
using Core.Performance.Parallelisation;
using Core.Player;
using Core.Saving;
using Extensions;
using UnityEngine;
using UnityInspector.PropertyAttributes;
using Utilities;

public class ChunkUpdater : SingletonBehaviour<ChunkUpdater>
{
    [SerializeField] private bool moveWithPlayer = true;
    [SerializeField] private int drawDistanceInChunks = 12;

    [SerializeField] private bool calculateThreads = false;
    [DrawIfFalse(nameof(calculateThreads)), SerializeField]
    private int meshJobThreadAmount = 0;
    [DrawIfFalse(nameof(calculateThreads)), SerializeField]
    private int noiseJobThreadAmount = 0;

    private int chunkSize;

    private int minHeight;
    private int maxHeight;
    private int dimension;

    private MeshJobManager _meshJobManager;
    private NoiseJobManager noiseJobManager;

    private SavingJob savingJob;
    private bool isChecking = false;
    
    private Queue<Direction> shiftDirections = new Queue<Direction>();
    private Timer timer;
    
    private readonly DrawingState drawingStateMask = DrawingState.NoiseReady | DrawingState.Drawn;
    private readonly DrawingState unneededColumnState = DrawingState.Dirty | DrawingState.None;

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
            meshJobThreadAmount = SystemInfo.processorCount - 2 <= 0 ? 1 :SystemInfo.processorCount / 2 - 5;
            noiseJobThreadAmount = SystemInfo.processorCount - 2 <= 0 ? 1 : (SystemInfo.processorCount - 2) / 3;
        }

        _meshJobManager = new MeshJobManager(meshJobThreadAmount, true);
        _meshJobManager.Start();
        noiseJobManager = new NoiseJobManager(noiseJobThreadAmount, true);
        noiseJobManager.Start();

        chunkSize = 0x10;
        ChunkBuffer.Init(chunkSize, minHeight, maxHeight, drawDistanceInChunks);

        dimension = ChunkBuffer.Dimension;
        timer = new Timer(WorldSettings.WorldTick);

        for (int x = xPlayerPos - (drawDistanceInChunks * chunkSize) - chunkSize, localx = 0; x <= xPlayerPos + (drawDistanceInChunks * chunkSize) + chunkSize; x += chunkSize, localx++)
        {
            for (int z = zPlayerPos - (drawDistanceInChunks * chunkSize) - chunkSize, localz = 0; z <= zPlayerPos + (drawDistanceInChunks * chunkSize) + chunkSize; z += chunkSize, localz++)
            {
                //Create chunkColumn
                ChunkColumn column = new ChunkColumn(new Int2(x, z), new Int2(localx, localz), minHeight, maxHeight);
                ChunkBuffer.SetChunkColumn(localx, localz, column);

                //Insert this created chunkColumn to the NoiseJobs 
                for (int y = minHeight, localy = 0; y < maxHeight; y += chunkSize, localy++)
                {
                    Chunk chunk = new Chunk()
                    {
                        LocalPosition = new Int3(localx, localy, localz),
                        GlobalPosition = new Int3(x, y, z),
                    };

                    column[localy] = chunk;
                }
                NoiseJob noiseJob = new NoiseJob()
                {
                    Column = column
                };

                noiseJobManager.AddJob(noiseJob);

                if (localx == 0 || localx == dimension - 1 || localz == 0 || localz == dimension - 1)
                {
                    column.DesiredForVisualization = false;
                }
                else
                    column.DesiredForVisualization = true;
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
        if (!isChecking)
        {
            CheckDrawReady();
        }
        
        if (timer.TimeElapsed(Time.deltaTime))
        {
            if (shiftDirections.Count > 0 && _meshJobManager.JobsCount == 0)
            {
                ChunkBuffer.Shift(shiftDirections.Dequeue());
            }
        }
    }

    private void CheckDrawReady()
    {
        isChecking = true;

        Task.Run(() =>
        {
            for (int x = 1; x < dimension - 1; x++)
            {
                for (int y = 0; y < dimension - 1; y++)
                {
                    ChunkColumn column = ChunkBuffer.GetChunkColumn(x, y);

                    if (column.DesiredForVisualization && column.State == DrawingState.NoiseReady || 
                        column.DesiredForVisualization && column.State == DrawingState.Dirty)
                    {
                        ChunkColumn[] neighbours = column.ChunkColumnNeighbours();

                        if (neighbours.All(c => (c.State & drawingStateMask) != 0))
                        {
                            {
                                for (int h = minHeight, localy = 0; h < maxHeight; h += chunkSize, localy++)
                                {
                                    Chunk chunk = column[localy];
                                    
                                    MeshJob job = new MeshJob(chunk);
                                    //job.CreateChunkFromExisting(chunk);
                                    _meshJobManager.CreateChunkFromExistingAndAddJob(chunk);
                                }
                                column.State = DrawingState.Drawn;
                            }
//                            if (column.State == DrawingState.Dirty)
//                            {
//                                Debug.Log("Dirty ass bitch");
//                                for (int h = minHeight, localy = 0; h < maxHeight; h += chunkSize, localy++)
//                                {
//                                    Chunk chunk = column[localy];
//                                    if (chunk.ChunkState == ChunkState.Dirty)
//                                    {
//                                        _meshJobManager.CreateChunkFromExistingAndAddJob(chunk);
//                                    }
//                                }
//                                
//                                column.State = DrawingState.Drawn;
//                            }
//                            else
                        }
                    }
                }
            }
        });
        
        isChecking = false;
    }

    private void OnDestroy()
    {
//        _jobManager?.Dispose();
//        _meshJobManager?.Dispose();
//        noiseJobManager?.Dispose();
    }
}