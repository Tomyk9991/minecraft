using Core.Chunking;
using Core.Chunking.Threading;
using Core.Math;
using Core.Saving;
using Extensions;
using System;
using System.Collections.Generic;
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

    private Int3 latestPlayerPosition;

    private ChunkJobManager chunkJobManager;

    private SavingJob savingJob;

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

        chunkSize = ChunkSettings.ChunkSize;
        AvailableChunks.Init(chunkSize, minHeight, maxHeight, drawDistanceInChunks);


        int xPlayerPos = MathHelper.ClosestMultiple(latestPlayerPosition.X, chunkSize);
        int zPlayerPos = MathHelper.ClosestMultiple(latestPlayerPosition.Z, chunkSize);

        int fancyConst = 2 * drawDistanceInChunks + 1;
        for (int x = (xPlayerPos - (drawDistanceInChunks * chunkSize) - chunkSize), noiseLocalx = 0, localx = -1; x <= xPlayerPos + (drawDistanceInChunks * chunkSize) + chunkSize; x += chunkSize, noiseLocalx++, localx++)
        {
            for (int z = zPlayerPos - (drawDistanceInChunks * chunkSize) - chunkSize, noiseLocalz = 0, localz = -1; z <= zPlayerPos + (drawDistanceInChunks * chunkSize) + chunkSize; z += chunkSize, noiseLocalz++, localz++)
            {
                if (localx >= 0 && localz >= 0 && localx < fancyConst && localz < fancyConst)
                {
                    ChunkColumn c = new ChunkColumn(new Int2(x, z), new Int2(localx, localz), minHeight, maxHeight);

                    AvailableChunks.SetColumn(localx, localz, c);
                }

                ChunkColumn column = new ChunkColumn(new Int2(x, z), new Int2(noiseLocalx, noiseLocalz), minHeight, maxHeight);
                AvailableChunks.SetColumnNoise(noiseLocalx, noiseLocalz, column);
            }
        }


        StartTaskedProcess(xPlayerPos, zPlayerPos);
    }

    private void StartTaskedProcess(int xPlayerPos, int zPlayerPos)
    {
        //Task.Run(() =>
        //{
            isRecalculating = true;
            RecalculateChunks(xPlayerPos, zPlayerPos);
            isRecalculating = false;
        //});
    }

    //private void Update()
    //{
    //    int xPlayerPos = 0;
    //    int zPlayerPos = 0;

    //    if(moveWithPlayer)
    //    {
    //        xPlayerPos = MathHelper.ClosestMultiple(latestPlayerPosition.X, chunkSize);
    //        zPlayerPos = MathHelper.ClosestMultiple(latestPlayerPosition.Z, chunkSize);
    //    }

    //    if (!isRecalculating)
    //    {
    //        StartTaskedProcess(xPlayerPos, zPlayerPos);
    //    }
    //}

    private List<Vector3> chunksNoise = new List<Vector3>();
    private List<Vector3> chunksRender = new List<Vector3>();

    private void OnDrawGizmosSelected()
    {
        if(Application.isPlaying)
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

    private void RecalculateChunks(int xPlayerPos, int zPlayerPos)
    {
        int fancyConst = 2 * drawDistanceInChunks + 1;

        for (int x = (xPlayerPos - (drawDistanceInChunks * chunkSize) - chunkSize), noiseLocalx = 0, localx = -1; x <= xPlayerPos + (drawDistanceInChunks * chunkSize) + chunkSize; x += chunkSize, noiseLocalx++, localx++)
        {
            for (int z = zPlayerPos - (drawDistanceInChunks * chunkSize) - chunkSize, noiseLocalz = 0, localz = -1; z <= zPlayerPos + (drawDistanceInChunks * chunkSize) + chunkSize; z += chunkSize, noiseLocalz++, localz++)
            {
                ChunkColumn noiseColumn = AvailableChunks.GetColumnNoise(noiseLocalx, noiseLocalz);

                if (!noiseColumn.GeneratingQueue && !noiseColumn.NoiseReady)
                {
                    if (InDistance(noiseColumn.GlobalPosition, xPlayerPos, zPlayerPos, drawDistanceInChunks + 1))
                    {
                        noiseColumn.GeneratingQueue = true;
                        for (int y = minHeight, localy = 0; y < maxHeight; y += chunkSize, localy++)
                        {
                            chunksNoise.Add(new Vector3(x, y, z));
                            ChunkJob job = new ChunkJob
                            {
                                OnlyNoise = true
                            };
                            Chunk createdChunk = job.CreateChunk(new Int3(x, y, z), new Int3(localx, localy, localz), noiseColumn);

                            createdChunk.ChunkState = ChunkState.Created;

                            noiseColumn.chunks[localy] = createdChunk;

                            chunkJobManager.Add(job);
                        }
                    }
                }


                if (localx >= 0 && localz >= 0 && localx < fancyConst && localz < fancyConst) // drawbuffer
                {
                    ChunkColumn renderingColumn = AvailableChunks.GetColumn(localx, localz);
                    if (InDistance(renderingColumn.GlobalPosition, xPlayerPos, zPlayerPos))
                    {
                        if (renderingColumn.HasAllNoiseNeighbours)
                        {
                            for (int y = minHeight, localy = 0; y < maxHeight; y += chunkSize, localy++)
                            {
                                chunksRender.Add(new Vector3(x, y, z));
                                Chunk chunk = AvailableChunks.GetChunkNoise(new Int3(localx, localy, localz));
                                ChunkJob job = new ChunkJob()
                                {
                                    OnlyNoise = false
                                };

                                job.CreateChunkFromExisting(chunk);
                                chunk.ChunkState = ChunkState.Generated;
                                renderingColumn.chunks[localy] = chunk;

                                chunkJobManager.Add(job);

                                //ChunkJob job = new ChunkJob()
                                //{
                                //    OnlyNoise = false
                                //};

                                //Chunk createdChunk = job.CreateChunk(new Int3(x, y, z), new Int3(localx, localy, localz), renderingColumn);
                                //createdChunk.ChunkState = ChunkState.Created;

                                //renderingColumn.chunks[localy] = createdChunk;

                                //chunkJobManager.Add(job);
                            }
                        }
                    }
                }
            }
        }
    }

    private bool InDistance(Int2 globalPos, int xPlayerPos, int zPlayerPos)
        => xPlayerPos - globalPos.X <= drawDistanceInChunks * chunkSize && zPlayerPos - globalPos.Y <= drawDistanceInChunks * chunkSize;

    private bool InDistance(Int2 globalPos, int xPlayerPos, int zPlayerPos, int distance)
    => xPlayerPos - globalPos.X <= distance * chunkSize && zPlayerPos - globalPos.Y <= distance * chunkSize;

}
public class ChunkColumn
{
    public Int2 GlobalPosition { get; }
    /// <summary>
    /// Discribes the local position, which depends on the global player position 
    /// </summary>
    public Int2 LocalPosition { get; private set; }
    public bool NoiseReady { get; set; }

    public bool HasAllNoiseNeighbours
    {
        get
        {
            return true;
            //var left = AvailableChunks.GetColumnNoise(this.LocalPosition.X, this.LocalPosition.Y + 1);
            //var right = AvailableChunks.GetColumnNoise(this.LocalPosition.X + 2, this.LocalPosition.Y + 1);

            //var up = AvailableChunks.GetColumnNoise(this.LocalPosition.X + 1, this.LocalPosition.Y + 2);
            //var down = AvailableChunks.GetColumnNoise(this.LocalPosition.X + 1, this.LocalPosition.Y);

            //return left.NoiseReady && right.NoiseReady && up.NoiseReady && down.NoiseReady;
        }
    }

    public bool GeneratingQueue { get; set; }

    public Chunk[] chunks;

    public ChunkColumn(Int2 globalPosition, Int2 localPosition, int minYHeight, int maxYHeight)
    {
        this.GlobalPosition = globalPosition;
        this.LocalPosition = localPosition;
        chunks = new Chunk[Math.Abs(minYHeight / 16) + Math.Abs(maxYHeight / 16)];
    }

    public void UpdateLocalPosition(int x, int z)
    {
        this.LocalPosition = new Int2(x, z);
    }

    public Chunk this[int index]
    {
        get { return chunks[index]; }
        set { chunks[index] = value; }
    }
}

public enum DrawState
{
    None = 0,
    InDrawingQueue = 1,
    Drawn = 2,
    NoiseReady = 3
}

public static class AvailableChunks
{
    private static ChunkColumn[] renderColumns;
    private static ChunkColumn[] noiseColumns;

    public static int DrawDistanceInChunks { get; private set; }

    public static int XZBound { get; private set; }
    public static int YBound { get; private set; }

    private static int XZBoundNoise { get; set; }
    private static int YBoundNoise { get; set; }

    private static object mutexNoise = new object();
    private static object mutex = new object();

    public static void Init(int chunkSize, int minHeight, int maxHeight, int drawDistanceInChunks)
    {
        DrawDistanceInChunks = drawDistanceInChunks;

        XZBound = ((2 * DrawDistanceInChunks) + 1);
        XZBoundNoise = (2 * DrawDistanceInChunks) + 2;

        YBound = (Math.Abs(minHeight) + Math.Abs(maxHeight)) / chunkSize;
        YBoundNoise = (Math.Abs(minHeight) + Math.Abs(maxHeight)) / chunkSize;

        int d = ((2 * drawDistanceInChunks) + 1);

        renderColumns = new ChunkColumn[d * d];
        noiseColumns = new ChunkColumn[(2 * drawDistanceInChunks + 3) * (2 * drawDistanceInChunks + 3)]; // Um eins in jede Richtung größer, als renderColumns
    }

    public static Chunk GetChunk(Int3 local)
    {
        if (local.X >= XZBound || local.Z >= XZBound || local.Y >= YBound || local.X < 0 || local.Y < 0 || local.Z < 0)
            return null;
        else
        {
            Chunk c = null;

            lock(mutex)
            {
                c = renderColumns[GetFlattenIndex2D(local.X, local.Z)][local.Y];
            }

            return c;
        }
    }


    public static Chunk GetChunkNoise(Int3 local)
    {
        local = new Int3(local.X + 1, local.Y, local.Z + 1);
        if (local.X >= XZBound || local.Z >= XZBound || local.Y >= YBound || local.X < 0 || local.Y < 0 || local.Z < 0)
            return null;
        else
        {
            Chunk c = null;

            lock(mutexNoise)
            {
                return noiseColumns[GetFlattenIndex2DNoise(local.X, local.Z)][local.Y];
            }

            return c;
        }
    }

    public static ChunkColumn GetColumn(int x, int y)
    {
        ChunkColumn c = null;
        lock(mutex)
        {
            c = renderColumns[GetFlattenIndex2D(x, y)];
        }

        return c;
    }

    public static ChunkColumn GetColumnNoise(int x, int z)
    {
        ChunkColumn c = null;
        lock(mutexNoise)
        {
            c = noiseColumns[GetFlattenIndex2DNoise(x, z)];
        }

        return c;
    }


    public static ChunkColumn[] GetAllColumns()
    {
        lock(mutex)
        {
            return renderColumns;
        }
    }

    public static ChunkColumn[] GetAllColumnsNoise()
    {
        lock(mutexNoise)
        {
            return noiseColumns;
        }
    }



    public static void SetColumn(int x, int y, ChunkColumn column)
    {
        lock(mutex)
        {
            renderColumns[GetFlattenIndex2D(x, y)] = column;
        }
    }

    public static void SetColumnNoise(int x, int y, ChunkColumn column)
    {
        lock (mutexNoise)
        {
            noiseColumns[GetFlattenIndex2DNoise(x, y)] = column;
        }
    }



    private static int GetFlattenIndex2D(int x, int y)
        => ((2 * DrawDistanceInChunks) + 1) * x + y;

    private static int GetFlattenIndex2DNoise(int x, int y)
        => ((2 * DrawDistanceInChunks + 3)) * x + y;
}

