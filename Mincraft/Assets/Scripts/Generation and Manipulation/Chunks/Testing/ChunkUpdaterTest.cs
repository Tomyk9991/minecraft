using Core.Chunking;
using Core.Chunking.Threading;
using Core.Math;
using Core.Saving;
using Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class ChunkUpdaterTest : SingletonBehaviour<ChunkUpdaterTest>
{
    public Int3 LatestPlayerPosition => latestPlayerPosition;
    [SerializeField] private bool moveWithPlayer = true;
    [SerializeField] private int drawDistanceInChunks = 12;
    [Header("References")]
    [SerializeField] private GameObject player = null;

    private int chunkSize;

    private int minHeight;
    private int maxHeight;

    public Int3 latestPlayerPosition;
    public int xPlayerPos = 0;
    public int zPlayerPos = 0;

    private ChunkJobManager chunkJobManager;
    private NoiseJobManager noiseJobManager;

    private SavingJob savingJob;
    private bool isRecalculating = false;
    private bool isChecking = false;
    private int yHeight = 0;

    int distanceNorm = 0;

    private void Start()
    {
        var minMaxYHeight = ChunkSettings.MinMaxYHeight;
        minHeight = minMaxYHeight.X;
        maxHeight = minMaxYHeight.Y;

        chunkJobManager = new ChunkJobManager(true);
        chunkJobManager.Start();
        noiseJobManager = new NoiseJobManager(true);
        noiseJobManager.Start();

        latestPlayerPosition = player.transform.position.ToInt3();

        chunkSize = ChunkSettings.ChunkSize;
        ChunkBuffer.Init(chunkSize, minHeight, maxHeight, drawDistanceInChunks);

        yHeight = ChunkBuffer.YBound;
        distanceNorm = 2 * drawDistanceInChunks + 1;

        xPlayerPos = MathHelper.ClosestMultiple(latestPlayerPosition.X, chunkSize);
        zPlayerPos = MathHelper.ClosestMultiple(latestPlayerPosition.Z, chunkSize);


        for (int x = (xPlayerPos - (drawDistanceInChunks * chunkSize) - chunkSize), localx = 0; x <= xPlayerPos + (drawDistanceInChunks * chunkSize) + chunkSize; x += chunkSize, localx++)
        {
            for (int z = zPlayerPos - (drawDistanceInChunks * chunkSize) - chunkSize, localz = 0; z <= zPlayerPos + (drawDistanceInChunks * chunkSize) + chunkSize; z += chunkSize, localz++)
            {
                ChunkColumn c = new ChunkColumn(new Int2(x, z), new Int2(localx, localz), minHeight, maxHeight);
                ChunkBuffer.SetColumn(localx, localz, c);
            }
        }


        StartTaskedProcess(xPlayerPos, zPlayerPos);
    }

    private void StartTaskedProcess(int xPlayerPos, int zPlayerPos)
    {
        if (!isRecalculating)
        {
            Task.Run(() =>
            {
                isRecalculating = true;
                RecalculateChunks(xPlayerPos, zPlayerPos);
                isRecalculating = false;
            });
        }
    }

    private void Update()
    {
        if (!isChecking)
        {
            Task.Run(() =>
            {
                isChecking = true;
                for (int x = (xPlayerPos - (drawDistanceInChunks * chunkSize) - chunkSize), localx = 0; x <= xPlayerPos + (drawDistanceInChunks * chunkSize) + chunkSize; x += chunkSize, localx++)
                {
                    for (int z = zPlayerPos - (drawDistanceInChunks * chunkSize) - chunkSize, localz = 0; z <= zPlayerPos + (drawDistanceInChunks * chunkSize) + chunkSize; z += chunkSize, localz++)
                    {
                        ChunkColumn column = ChunkBuffer.GetColumn(localx, localz);
                        if (localx > 0 && localx <= distanceNorm && localz > 0 && localz <= distanceNorm && column.State == DrawingState.NoiseReady)
                        {
                            ChunkColumn[] neighbour = column.Neighbours();
                            if (neighbour.All(c => c.State == DrawingState.NoiseReady || c.State == DrawingState.Drawn))
                            {
                                column.State = DrawingState.Drawn;
                                for (int y = minHeight, localy = 0; y < maxHeight; y += chunkSize, localy++)
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
                isChecking = false;
            });
        }
    }

    private void OnDestroy()
    {
        chunkJobManager.Dispose();
        noiseJobManager.Dispose();
    }

    private void RecalculateChunks(int xPlayerPos, int zPlayerPos)
    {
        for (int x = (xPlayerPos - (drawDistanceInChunks * chunkSize) - chunkSize), localx = 0; x <= xPlayerPos + (drawDistanceInChunks * chunkSize) + chunkSize; x += chunkSize, localx++)
        {
            for (int z = zPlayerPos - (drawDistanceInChunks * chunkSize) - chunkSize, localz = 0; z <= zPlayerPos + (drawDistanceInChunks * chunkSize) + chunkSize; z += chunkSize, localz++)
            {
                ChunkColumn column = ChunkBuffer.GetColumn(localx, localz);
                if (column.State == DrawingState.None)
                {
                    Chunk[] chunks = new Chunk[yHeight];

                    for (int y = minHeight, localy = 0; y < maxHeight; y += chunkSize, localy++)
                    {
                        Chunk chunk = new Chunk()
                        {
                            LocalPosition = new Int3(localx, localy, localz),
                            GlobalPosition = new Int3(x, y, z)
                        };

                        column[localy] = chunk;
                        chunks[localy] = chunk;
                    }
                    NoiseJob noiseJob = new NoiseJob
                    {
                        Column = column
                    };

                    noiseJobManager.AddJob(noiseJob);

                    column.State = DrawingState.InNoiseQueue;
                }
            }
        }
    }

    private List<Vector3> chunksNoise = new List<Vector3>();
    private List<Vector3> chunksRender = new List<Vector3>();

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < chunksNoise.Count; i++)
            {
                Gizmos.DrawWireCube(chunksNoise[i] + Vector3.one * 8, Vector3.one * 16);
            }

            Gizmos.color = Color.white;
            for (int i = 0; i < chunksRender.Count; i++)
            {
                Gizmos.DrawWireCube(chunksRender[i] + Vector3.one * 8, Vector3.one * 16);
            }
        }
    }
}