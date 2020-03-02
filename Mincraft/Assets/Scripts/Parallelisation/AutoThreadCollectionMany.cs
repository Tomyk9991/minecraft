using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Core.Chunking.Threading;
using UnityEngine;


/// <summary>
/// Represents a chunk of threads desired to to jobs concurrently
/// </summary>
/// <typeparam name="TJobResult">Where the result is used</typeparam>
/// <typeparam name="KJob">Where the result is not in use</typeparam>
public abstract class AutoThreadCollectionMany<TJobResult, KJob>
{
    public int FinishedJobsCount => finishedJobs.Count;
    public int MeshJobsCount => meshJobs.Count;
    public int NoiseJobsCount => noiseJobs.Count;
    
    private Thread[] _threads;
    private bool running = false;
    private bool singleThreaded = false;

    private ConcurrentQueue<TJobResult> meshJobs;
    private ConcurrentQueue<KJob> noiseJobs;

    private ConcurrentQueue<TJobResult> finishedJobs;
    private SemaphoreSlim semaphore;
    
    public AutoThreadCollectionMany(int amountThreads)
    {
        finishedJobs = new ConcurrentQueue<TJobResult>();
        
        if (amountThreads < 0)
            throw new ArgumentOutOfRangeException("amountThreads has to be positive or zero");
        if (amountThreads == 0) singleThreaded = true;
        else
        {
            meshJobs = new ConcurrentQueue<TJobResult>();
            noiseJobs = new ConcurrentQueue<KJob>();
            
            _threads = new Thread[amountThreads];
            semaphore = new SemaphoreSlim(0);

            for (int i = 0; i < amountThreads; i++)
            {
                _threads[i] = new Thread(Run)
                {
                    IsBackground = true
                };
            }
        }
    }

    public abstract void ExecuteMeshJob(TJobResult job);
    public abstract void ExecuteNoiseJob(KJob job);

    public void Run()
    {
        while (running)
        {
            semaphore.Wait();
            
            if (meshJobs.TryDequeue(out TJobResult tjobresult))
            {
                ExecuteMeshJob(tjobresult);
                finishedJobs.Enqueue(tjobresult);
                continue;
            }
            
            if (noiseJobs.TryDequeue(out KJob kjob))
            {
                ExecuteNoiseJob(kjob);
            }
        }
    }

    public TJobResult DequeueFinishedJob()
    {
        if (finishedJobs.TryDequeue(out TJobResult result))
        {
            return result;
        }
        
        throw new Exception("Dequeuing but Queue is empty");
    }

    public void Add(TJobResult job)
    {
        this.meshJobs.Enqueue(job);
        semaphore.Release();
    }

    public void Add(KJob job)
    {
        this.noiseJobs.Enqueue(job);
        semaphore.Release();
    }

    public void Start()
    {
        if (!singleThreaded)
        {
            running = true;
            for (int i = 0; i < _threads.Length; i++)
            {
                _threads[i].Start();
            }
        }
    }

    public void Stop()
    {
        if (!singleThreaded)
        {
            this.running = false;
            for (int i = 0; i < _threads.Length; i++)
            {
                _threads[i].Abort();
            }
        }
    }
}
