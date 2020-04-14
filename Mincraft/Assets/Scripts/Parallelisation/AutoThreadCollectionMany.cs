using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Core.Builder;
using Core.Chunking;
using Core.Chunking.Threading;

/// <summary>
/// Represents a chunk of threads desired to to jobs concurrently
/// </summary>
public abstract class AutoThreadCollectionMany
{
    public int FinishedJobsCount => finishedJobs.Count;
    public int NoiseJobsCount => noiseJobs.Count;
    public int MeshJobsCount => meshJobs.Count;

    private bool running = false;
    
    private ConcurrentQueue<MeshJob> finishedJobs;
    private ConcurrentQueue<IJob> meshJobs;
    private ConcurrentQueue<IJobManyDependencies> noiseJobs;
    
    private SemaphoreSlim semaphore;

    private Thread[] threads;

    protected AutoThreadCollectionMany(int amountThreads)
    {
        finishedJobs = new ConcurrentQueue<MeshJob>();
        semaphore = new SemaphoreSlim(0);
        meshJobs = new ConcurrentQueue<IJob>();
        noiseJobs = new ConcurrentQueue<IJobManyDependencies>();

        threads = new Thread[amountThreads];

        for (int i = 0; i < amountThreads; i++)
        {
            threads[i] = new Thread(Run)
            {
                IsBackground = true
            };
        }
    }

    public void Run()
    {
        while (running)
        {
            semaphore.Wait();
            if (meshJobs.TryDequeue(out IJob meshjob))
            {
                if (!meshjob.Finished) // Ist dieser Job nicht erledigt, wird dieser nun gemacht
                {
                    meshjob.Execute();
                }
                
                if (meshjob.NeedFinishToo.Finished) // Wenn der Dependencyjob fertig ist
                {
                    meshjob.Target.Chunk.ChunkState = ChunkState.Generated;
                    finishedJobs.Enqueue(meshjob.Target);
                    //continue;
                }
            }

            if (noiseJobs.TryDequeue(out IJobManyDependencies noiseJob))
            {
                if (!noiseJob.Finished)
                {
                    noiseJob.Execute();
                }
                
                if (noiseJob.NeedFinishToo.All(n => n.Finished)) // Wenn alle dependencies fertig sind
                {
                    noiseJob.Target.ChunkColumn.State = DrawingState.NoiseReady;
                }
            }
        }
    }

    public MeshJob DequeueFinishedJob()
    {
        if (finishedJobs.TryDequeue(out MeshJob result))
        {
            return result;
        }

        throw new Exception("Dequeuing but Queue is empty");
    }

    public void Add(in MeshJob item)
    {
        item.Chunk.GenerateStructures();
        IJob job1 = new MeshBuilderJob()
        {
            Target = item,
            Finished = false,
        };
        
        IJob job2 = new ReduceColliderJob()
        {
            Target = item,
            Finished = false,
        };

        job1.NeedFinishToo = job2;
        job2.NeedFinishToo = job1;
        

        ScheduleMeshJob(job1);
        ScheduleMeshJob(job2);
    }
    
    public void Add(in NoiseJob item)
    {
        var jobs = new IJobManyDependencies[item.Column.chunks.Length];
        int len = item.Column.chunks.Length;
        
        for (int i = 0; i < len; i++)
        {
            jobs[i] = new GenerateNoiseJob()
            {
                Finished = false,
                Target = item.Column.chunks[i]
            };
        }

        for (int i = 0; i < len; i++)
        {
            jobs[i].NeedFinishToo = jobs;
        }

        for (int i = 0; i < len; i++)
        {
            ScheduleNoiseJob(jobs[i]);
        }
    }

    public void Start()
    {
        running = true;
        for (int i = 0; i < threads.Length; i++)
        {
            threads[i].Start();
        }
    }

    public void Stop()
    {
        running = false;
        for (int i = 0; i < threads.Length; i++)
        {
            threads[i].Abort();
        }
    }

    private void ScheduleMeshJob(in IJob job)
    {
        meshJobs.Enqueue(job);
        semaphore.Release();
    }

    private void ScheduleNoiseJob(in IJobManyDependencies job)
    {
        noiseJobs.Enqueue(job);
        semaphore.Release();
    }
    
    public struct ReduceColliderJob : IJob
    {
        public bool Finished { get; set; }
        public IJob NeedFinishToo { get; set; }
        public MeshJob Target { get; set; }
        
        public void Execute()
        {
            Target.ColliderData = GreedyMesh.ReduceMesh(Target.Chunk);
            Finished = true;
        }
    }
    
    public struct MeshBuilderJob : IJob
    {
        public MeshJob Target { get; set; }
        public bool Finished { get; set; }
        public IJob NeedFinishToo { get; set; }
        
        public void Execute()
        {
            Target.MeshData = MeshBuilder.Combine(Target.Chunk);
            Finished = true;
        }
    }
    
    
    public struct GenerateNoiseJob : IJobManyDependencies
    {
        public IJobManyDependencies[] NeedFinishToo { get; set; }
        public bool Finished { get; set; }
        public Chunk Target { get; set; }
        
        public void Execute()
        {
            Target.GenerateBlocks();
            Finished = true;
        }
    }
    
    public interface IJob
    {
        IJob NeedFinishToo { get; set; }
        bool Finished { get; set; }
        MeshJob Target { get; set; }
        void Execute();
    }
    

    public interface IJobManyDependencies
    {
        IJobManyDependencies[] NeedFinishToo { get; set; }
        bool Finished { get; set; }
        Chunk Target { get; set; }
        void Execute();
    }
}