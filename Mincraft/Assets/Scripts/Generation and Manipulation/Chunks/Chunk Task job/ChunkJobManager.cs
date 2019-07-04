using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using UnityEngine;

public class ChunkJobManager : IDisposable
{
    public ConcurrentQueue<ChunkJob> Jobs { get; private set; }
    public ConcurrentQueue<ChunkJob> FinishedJobs { get; private set; }

    public bool Running { get; set; }

    private Thread[] threads;
    private ContextIO<Chunk> chunkLoader;


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

    public ChunkJobManager()
    {
        chunkSize = ChunkSettings.ChunkSize;
        Jobs = new ConcurrentQueue<ChunkJob>();
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
                Thread.Sleep(50); //Needed, because CPU is overloaded in over case
                continue;
            }
            else if(JobsCount > 0)
            {
                ChunkJob job = null;

                Jobs.TryDequeue(out job);

                if (job != null)
                {
                    if (!job.HasBlocks) // Chunk gets build new
                    {
                        ChunkDictionary.Add(job.Chunk.Position, job.Chunk);
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

                    //TODO füge MeshCollider wieder hinzu
                    GreedyMesh mesh = new GreedyMesh();
                    job.MeshData = ModifyMesh.Combine(job.Chunk);
                    job.ColliderData = mesh.ReduceMesh(job.Chunk);


                    job.Completed = true;
                    FinishedJobs.Enqueue(job);
                }
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
