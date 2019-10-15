using Core.Chunking;
using Core.Chunking.Threading;
using Core.Math;
using Core.Saving;
using Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        NoiseJobManager.Start();


        int xPlayerPos = MathHelper.ClosestMultiple(latestPlayerPosition.X, chunkSize);
        int zPlayerPos = MathHelper.ClosestMultiple(latestPlayerPosition.Z, chunkSize);


        for (int x = (xPlayerPos - (drawDistanceInChunks * chunkSize) - chunkSize), localx = 0; x <= xPlayerPos + (drawDistanceInChunks * chunkSize) + chunkSize; x += chunkSize, localx++)
        {
            for (int z = zPlayerPos - (drawDistanceInChunks * chunkSize) - chunkSize, localz = 0; z <= zPlayerPos + (drawDistanceInChunks * chunkSize) + chunkSize; z += chunkSize, localz++)
            {
                ChunkColumn c = new ChunkColumn(new Int2(x, z), new Int2(localx, localz), minHeight, maxHeight);
                AvailableChunks.SetColumn(localx, localz, c);
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

    private void Update()
    {
        //int xPlayerPos = 0;
        //int zPlayerPos = 0;

        //if (moveWithPlayer)
        //{
        //    xPlayerPos = MathHelper.ClosestMultiple(latestPlayerPosition.X, chunkSize);
        //    zPlayerPos = MathHelper.ClosestMultiple(latestPlayerPosition.Z, chunkSize);
        //}

        //if (!isRecalculating)
        //{
        //    StartTaskedProcess(xPlayerPos, zPlayerPos);
        //}
    }

    private void RecalculateChunks(int xPlayerPos, int zPlayerPos)
    {
        int fancyConst = 2 * drawDistanceInChunks + 1;
        for (int x = (xPlayerPos - (drawDistanceInChunks * chunkSize) - chunkSize), localx = 0; x <= xPlayerPos + (drawDistanceInChunks * chunkSize) + chunkSize; x += chunkSize, localx++)
        {
            for (int z = zPlayerPos - (drawDistanceInChunks * chunkSize) - chunkSize, localz = 0;  z <= zPlayerPos + (drawDistanceInChunks * chunkSize) + chunkSize; z += chunkSize, localz++)
            {
                ChunkColumn column = AvailableChunks.GetColumn(localx, localz);

                if (localx > 0 && localx <= fancyConst && localz > 0 && localz <= fancyConst)
                {
                    //hole alle nachbar-referenzen
                    //checke, ob die i-te nachbar-referenz schon noise-berechnungen gemacht hat
                    // => wenn nicht, i-te nachbar-referenz hinzufügen
                    //checke, ob das eigentliche column schon seine noise-berechnung gemacht hat
                    // => wenn nicht, eigentliches column hinzufügen
                    //wenn beide checks alles gerechnet haben, kann zum zeichnen freigegeben werden

                    ChunkColumn[] neighbours = column.Neighbours();
                    for (int i = 0; i < 4; i++)
                    {
                        ChunkColumn neighbourColumn = neighbours[i];
                        if (!neighbourColumn.NoiseReady)
                        {
                            for (int y = minHeight, localy = 0; y < maxHeight; y += chunkSize, localy++)
                            {
                                Chunk c = new Chunk()
                                {
                                    LocalPosition = new Int3(localx, localy, localz),
                                    GlobalPosition = new Int3(x, y, z)
                                };
                                c.GenerateBlocks();

                                neighbourColumn[localy] = c;
                            }
                            neighbourColumn.NoiseReady = true;
                        }
                    }

                    if (!column.NoiseReady)
                    {
                        for (int y = minHeight, localy = 0; y < maxHeight; y += chunkSize, localy++)
                        {
                            Chunk c = new Chunk()
                            {
                                LocalPosition = new Int3(localx, localy, localz),
                                GlobalPosition = new Int3(x, y, z)
                            };
                            c.GenerateBlocks();
                            column[localy] = c;
                        }
                        column.NoiseReady = true;
                    }

                    for (int y = minHeight, localy = 0; y < maxHeight; y += chunkSize, localy++)
                    {
                        if (localy <= 5)
                        {
                            ChunkJob job = new ChunkJob();
                            job.CreateChunkFromExisting(column[localy]);
                            chunkJobManager.Add(job);
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

            //var forward = AvailableChunks.GetColumnNoise(this.LocalPosition.X + 1, this.LocalPosition.Y + 2);
            //var back = AvailableChunks.GetColumnNoise(this.LocalPosition.X + 1, this.LocalPosition.Y);

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

    public ChunkColumn[] Neighbours()
    {
        return new ChunkColumn[]
        {
            //Left
            AvailableChunks.GetColumn(this.LocalPosition.X - 1, this.LocalPosition.Y),
            //Right
            AvailableChunks.GetColumn(this.LocalPosition.X + 1, this.LocalPosition.Y),
            //Forward
            AvailableChunks.GetColumn(this.LocalPosition.X, this.LocalPosition.Y + 1),
            //Back
            AvailableChunks.GetColumn(this.LocalPosition.X, this.LocalPosition.Y - 1),
        };
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

        int d = ((2 * drawDistanceInChunks) + 1);

        noiseColumns = new ChunkColumn[(2 * drawDistanceInChunks + 3) * (2 * drawDistanceInChunks + 3)]; // Um eins in jede Richtung größer, als renderColumns
    }

    public static Chunk GetChunk(Int3 local)
    {
        if (local.Y >= YBound || local.Y < 0)
            return null;

        lock(mutexNoise)
        {
            return noiseColumns[GetFlattenIndex2DNoise(local.X, local.Z)][local.Y];
        }
    }

    public static ChunkColumn GetColumn(int x, int z)
    {
        ChunkColumn c = null;
        lock(mutexNoise)
        {
            c = noiseColumns[GetFlattenIndex2DNoise(x, z)];
        }

        return c;
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
        lock (mutexNoise)
        {
            noiseColumns[GetFlattenIndex2DNoise(x, y)] = column;
        }
    }

    private static int GetFlattenIndex2DNoise(int x, int y)
        => ((2 * DrawDistanceInChunks + 3)) * x + y;
}

public class NoiseJob
{
    public Chunk Chunk { get; set; }
    public ChunkColumn Column { get; set; }

    public Chunk CreateChunk(Int3 globalPos, Int3 localPos, ChunkColumn column)
    {
        Chunk chunk = new Chunk
        {
            LocalPosition = localPos,
            GlobalPosition = globalPos
        };

        this.Chunk = chunk;
        this.Column = column;


        return chunk;
    }
}

public static class NoiseJobManager
{
    private static bool running;
    private static System.Threading.Thread[] threads = SystemInfo.processorCount - 2 <= 0
                ? new System.Threading.Thread[1]
                : new System.Threading.Thread[SystemInfo.processorCount - 2];

    private static ConcurrentQueue<NoiseJob> jobs = new ConcurrentQueue<NoiseJob>();

    public static void Start()
    {
        running = true;
        for (int i = 0; i < threads.Length; i++)
        {
            threads[i] = new System.Threading.Thread(Calculate)
            {
                IsBackground = true
            };
        }
    }

    public static void Stop() => running = false;

    public static void AddJob(NoiseJob job)
    {
        jobs.Enqueue(job);
    } 

    private static void Calculate()
    {
        while(running)
        {
            if (jobs.Count == 0)
            {
                //TODO wieder auf 10ms stellen
                System.Threading.Thread.Sleep(10); //Needed, because CPU is overloaded in other case
                continue;
            }

            if(jobs.TryDequeue(out var job))
            {
                job.Chunk.GenerateBlocks();

                job.Column.NoiseReady = true;
                job.Column.GeneratingQueue = false;
            }
        }
    }
}