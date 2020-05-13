using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.Chunks.Threading.Jobs;

namespace Core.Chunks.Threading
{
    /// <summary>
    /// Represents a chunk of threads desired to to jobs concurrently
    /// </summary>
    public abstract class AutoThreadCollection
    {
        public int FinishedJobsCount => finishedJobs.Count;
        public int NoiseJobsCount => chunkJobs.Count;
        public int MeshJobsCount => meshJobs.Count;

        private bool running = false;

        private ConcurrentQueue<MeshJob> finishedJobs;
        
        private ConcurrentQueue<IJobCollection<MeshJob>> meshJobs;
        private ConcurrentQueue<JobCollectionItemContainer> chunkJobs;

        private SemaphoreSlim mutex;

        private Thread[] threads;

        protected AutoThreadCollection(int amountThreads)
        {
            finishedJobs = new ConcurrentQueue<MeshJob>();
            mutex = new SemaphoreSlim(0);
            
            meshJobs = new ConcurrentQueue<IJobCollection<MeshJob>>();
            chunkJobs = new ConcurrentQueue<JobCollectionItemContainer>();

            threads = new Thread[amountThreads];

            for (int i = 0; i < amountThreads; i++)
            {
                threads[i] = new Thread(Run) {IsBackground = true};
                // threads[i] = i % 2 == 0 
                //     ? new Thread(RunMesh) { IsBackground = true } 
                //     : new Thread(RunJobContainer) { IsBackground = true };
            }
        }

        public void Start()
        {
            running = true;
            foreach (Thread t in threads)
                t.Start();
        }

        public void Run()
        {
            while (running)
            {
                mutex.Wait();
                if (chunkJobs.TryDequeue(out JobCollectionItemContainer job))
                {
                    // In diesem Fall ist es der NoiseJob, der ausgeführt und verarbeitet wird
                    IJobCollection<MeshJob>[] seq = job.SequentialCollection;
                    for (int i = 0; i < seq.Length; i++)
                    {
                        seq[i].Execute();
                        seq[i].Finished = true;
                    }

                    //Für alle Jobs, die als parallel aktiviert wurden, werden diese einzeln in die Queue für Meshjobs
                    //hinzugefügt
                    IJobCollection<MeshJob>[] par = job.ParallelizedCollection;
                    for (int i = 0; i < par.Length; i++)
                    {
                        par[i].OtherJobs = par;
                        ScheduleMeshJob(par[i]);
                    }
                }
                
                if (meshJobs.TryDequeue(out IJobCollection<MeshJob> meshJob))
                {
                    if (!meshJob.Finished)
                    {
                        meshJob.Execute();
                        meshJob.Finished = true;
                    }
                
                    if (meshJob.OtherJobs.All((IJobCollectionItem other) => other.Finished))
                    {
                        finishedJobs.Enqueue(meshJob.Target);
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

        public void Add(ChunkJob item)
        {
            int len = item.Column.chunks.Length;
            for (int i = 0; i < len; i++)
            {
                MeshJob job = new MeshJob(item.Column.chunks[i]);

                IJobCollection<MeshJob> job1 = new GenerateNoiseJob(job);

                IJobCollection<MeshJob> job2 = new MeshBuilderJob(job);
                IJobCollection<MeshJob> job3 = new ReduceColliderJob(job);

                var container = new JobCollectionItemContainer(1, 2, JobCollectionItemContainer.Order.RunSequentialFirst);
                container.RunSequentially(job1);
                container.RunParallelized(job2, job3);

                ScheduleJob(container);
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

        private void ScheduleJob(JobCollectionItemContainer job)
        {
            chunkJobs.Enqueue(job);
            mutex.Release();
        }

        private void ScheduleMeshJob(IJobCollection<MeshJob> job)
        {
            meshJobs.Enqueue(job);
            mutex.Release();
        }
    }
}