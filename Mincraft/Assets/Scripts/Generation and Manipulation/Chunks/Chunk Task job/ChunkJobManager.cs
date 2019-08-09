using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;

public class ChunkJobManager : IDisposable
{
    public static ChunkJobManager ChunkJobManagerUpdaterInstance { get; private set; }
    public ConcurrentQueue<ChunkJob> Jobs { get; private set; }
    public ConcurrentQueue<ChunkJob> FinishedJobs { get; private set; }

    public bool Running { get; set; }

    private Thread[] threads;
    private ContextIO<Chunk> chunkLoader;
    private GreedyMesh greedy;


    private static Int3[] directions =
    {
        Int3.Forward, // 0
        Int3.Back, // 1
        Int3.Up, // 2
        Int3.Down, // 3
        Int3.Left, // 4
        Int3.Right // 5
    };


    private int chunkSize;

    public int JobsCount => Jobs.Count;
    public int FinishedJobsCount => FinishedJobs.Count;

    public ChunkJobManager(bool chunkUpdaterInstance = false)
    {
        if (chunkUpdaterInstance)
            ChunkJobManagerUpdaterInstance = this;

        chunkSize = ChunkSettings.ChunkSize;
        Jobs = new ConcurrentQueue<ChunkJob>();
        greedy = new GreedyMesh();



        FinishedJobs = new ConcurrentQueue<ChunkJob>();
        chunkLoader = new ContextIO<Chunk>(ContextIO.DefaultPath + "/" + GameManager.CurrentWorldName + "/");
        GameManager.AbsolutePath = ContextIO.DefaultPath + "/" + GameManager.CurrentWorldName;

        //What if you got only two cores?
        threads = SystemInfo.processorCount - 2 <= 0 
            ? new Thread[1] 
            : new Thread[SystemInfo.processorCount - 2];

        for (int i = 0; i < threads.Length; i++)
        {
            threads[i] = new Thread(Calculate)
            {
                IsBackground = true
            };
        }
    }

    public ChunkJobManager(int test)
    {
        if (true)
            ChunkJobManagerUpdaterInstance = this;

        chunkSize = 16;
        Jobs = new ConcurrentQueue<ChunkJob>();
        greedy = new GreedyMesh(true);

        FinishedJobs = new ConcurrentQueue<ChunkJob>();
        chunkLoader = new ContextIO<Chunk>(ContextIO.DefaultPath + "/" + GameManager.CurrentWorldName + "/");
        GameManager.AbsolutePath = ContextIO.DefaultPath + "/" + GameManager.CurrentWorldName;

        threads = new Thread[SystemInfo.processorCount - 2];

        for (int i = 0; i < threads.Length; i++)
        {
            threads[i] = new Thread(Calculate)
            {
                IsBackground = true
            };
        }
    }

    private void Calculate()
    { 
        while (Running)
        {
            if (JobsCount == 0)
            {
                //TODO wieder auf 10ms stellen
                Thread.Sleep(10); //Needed, because CPU is overloaded in over case
                continue;
            }
            else if(JobsCount > 0)
            {
                Jobs.TryDequeue(out var job);

                if (job == null) continue;

                if (!job.HasBlocks) // Chunk gets build new
                {
                    job.Chunk.CalculateNeigbours();

                    string path = chunkLoader.Path + job.Chunk.Position.ToString() + chunkLoader.FileEnding<Chunk>();

                    if (File.Exists(path))
                    {
                        job.Chunk.LoadChunk(chunkLoader.LoadContext(path));
                    }
                    else
                    {
                        job.Chunk.GenerateBlocks();
                    }
                }
                else
                {
                    job.Chunk.CalculateNeigbours();
                }

                //if "if" not executed, than chunk already existed

                //TODO: Kann parallisiert werden
                job.MeshData = ModifyMesh.Combine(job.Chunk);
                job.ColliderData = greedy.ReduceMesh(job.Chunk);


                job.Completed = true;
                FinishedJobs.Enqueue(job);
            }
        }
    }

    public void Start()
    {
        Running = true;

        for (int i = 0; i < threads.Length; i++)
            threads[i].Start();
    }

    public void Add(ChunkJob job)
    {
        if (Jobs.Any(j => j == job))
            return;

        Jobs.Enqueue(job);
    }

    public ChunkJob DequeueFinishedJobs()
    {
        if (FinishedJobs.TryDequeue(out var result))
        {
            return result;
        }

        return null;
    }

    public void Dispose()
    {
        Running = false;
    }
}
