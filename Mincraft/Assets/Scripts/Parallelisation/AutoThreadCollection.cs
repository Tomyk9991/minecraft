using System;
using System.Collections.Generic;
using System.Threading;

namespace Core.Performance.Parallelisation
{
    public abstract class AutoThreadCollection<T>
    {
        protected List<T> FinishedJobs { get; private set; }

        protected Queue<T> jobs;
        protected Queue<T> highPriorityJobs;
        private Thread[] threads;
        private bool running = false;
        private object lockObject;
        private SemaphoreSlim semaphore;

        private bool singleThreaded = false;


        protected AutoThreadCollection(int amountThreads)
        {
            if (amountThreads < 0)
                throw new ArgumentOutOfRangeException("amountThreads has to be positive or zero");
            if (amountThreads == 0) singleThreaded = true;
            else
            {
                this.jobs = new Queue<T>();
                this.highPriorityJobs = new Queue<T>();
                //this.FinishedJobs = new List<T>();
//                this.FinishedMeshJobs = new List<MeshJob>();
//                this.FinishedNoiseJobs = new List<NoiseJob>();
                this.FinishedJobs = new List<T>();
                semaphore = new SemaphoreSlim(0);
                this.lockObject = new object();
                
                
                threads = new Thread[amountThreads];
                for (int i = 0; i < amountThreads; i++)
                {
                    threads[i] = new Thread(Run)
                    {
                        IsBackground = true
                    };
                }
            }
        }

        public abstract void JobExecute(T job);

        private void Run()
        {
            while (running)
            {
                T job;
                semaphore.Wait();

                bool highPriority = false;
                lock (lockObject)
                {
                    if (highPriorityJobs.Count > 0)
                    {
                        job = highPriorityJobs.Dequeue();
                        highPriority = true;
                    }
                    else
                    {
                        job = jobs.Dequeue();
                    }
                }
                
                JobExecute(job);
                lock (FinishedJobs) //Sorgt dafür, dass die Jobs mit hoher Priorität auch als erstes aus der Liste genommen werden
                {
                    if (highPriority)
                        FinishedJobs.Insert(0, job);
                    else
                        FinishedJobs.Add(job);
                }
            }
        }

        public virtual void AddJob(T job, JobPriority priority = JobPriority.Normal)
        {
            if (!singleThreaded)
            {
                lock (lockObject)
                {
                    if (priority == JobPriority.High)
                        this.highPriorityJobs.Enqueue(job);
                    else
                        this.jobs.Enqueue(job);
                }

                semaphore.Release();
            }
            else
            {
                JobExecute(job);
            }
        }
        

        public void Start()
        {
            if (!singleThreaded)
            {
                this.running = true;
                for (int i = 0; i < threads.Length; i++)
                {
                    threads[i].Start();
                }
            }
        }

        public void Stop()
        {
            if (!singleThreaded)
            {
                this.running = false;
                for (int i = 0; i < threads.Length; i++)
                {
                    //threads[i].Join(30);
                    threads[i].Abort();
                }
            }
        }
    }

    public enum JobPriority
    {
        Normal,
        High,
    }
}