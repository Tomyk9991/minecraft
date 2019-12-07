using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core;
using Core.Chunking;
using Core.Chunking.Threading;
using Core.Math;
using Core.Player;
using Core.Saving;
using Extensions;
using UnityEngine;
using Utilities;

public class ChunkUpdater : SingletonBehaviour<ChunkUpdater>
{
    [SerializeField] private bool moveWithPlayer = true;
    [SerializeField] private int drawDistanceInChunks = 12;

    private int chunkSize;

    private int minHeight;
    private int maxHeight;
    private int dimension;

    private ChunkJobManager chunkJobManager;
    private NoiseJobManager noiseJobManager;

    private SavingJob savingJob;
    private bool isChecking = false;
    
    private Queue<Direction> shiftDirections = new Queue<Direction>();
    private Timer timer;

    private void Start()
    {
        int xPlayerPos = PlayerMovementTracker.Instance.xPlayerPos;
        int zPlayerPos = PlayerMovementTracker.Instance.zPlayerPos;

        PlayerMovementTracker.OnDirectionModified += DirectionModified;

        var minMaxYHeight = WorldSettings.MinMaxYHeight;
        minHeight = minMaxYHeight.X;
        maxHeight = minMaxYHeight.Y;

        chunkJobManager = new ChunkJobManager(true);
        chunkJobManager.Start();
        noiseJobManager = new NoiseJobManager(true);
        noiseJobManager.Start();

        chunkSize = 0x10;
        ChunkBuffer.Init(chunkSize, minHeight, maxHeight, drawDistanceInChunks);

        dimension = ChunkBuffer.dimension;
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
                        GlobalPosition = new Int3(x, y, z)
                    };

                    column[localy] = chunk;
                }

                NoiseJob noiseJob = new NoiseJob()
                {
                    Column = column
                };

                noiseJobManager.AddJob(noiseJob);
                column.State = DrawingState.InNoiseQueue;

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
            if (shiftDirections.Count > 0 && chunkJobManager.JobsCount == 0)
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
            for (int x = 0; x < dimension; x++)
            {
                for (int y = 0; y < dimension; y++)
                {
                    ChunkColumn column = ChunkBuffer.GetChunkColumn(x, y);
                    if (column.DesiredForVisualization && column.State == DrawingState.NoiseReady)
                    {
                        ChunkColumn[] neighbours = column.Neighbours();

                        if (neighbours.All(c => c.State == DrawingState.NoiseReady || c.State == DrawingState.Drawn))
                        {
                            column.State = DrawingState.Drawn;
                            for (int h = minHeight, localy = 0; h < maxHeight; h += chunkSize, localy++)
                            {
                                Chunk chunk = column[localy];
                                ChunkJob job = new ChunkJob();
                                job.CreateChunkFromExisting(chunk, column);
                                chunkJobManager.AddJob(job);
                            }
                        }
                    }
                }
            }

        }); 
        isChecking = false;
    }

    private void OnDestroy()
    {
        chunkJobManager.Dispose();
        noiseJobManager.Dispose();
    }
}