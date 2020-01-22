using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Core.Performance.Parallelisation
{
    public abstract class AutoThreadCollection<T> where T : class
    {
        public ConcurrentQueue<T> FinishedJobs { get; private set; }

        protected Queue<T> jobs;
        private Thread[] threads;
        private bool running = false;
        private object lockObject;
        private SemaphoreSlim semaphore;


        protected AutoThreadCollection(int amountThreads)
        {
            this.jobs = new Queue<T>();
            this.FinishedJobs = new ConcurrentQueue<T>();
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

        public abstract void JobExecute(T job);

        private void Run()
        {
            while (running)
            {
                T job;
                semaphore.Wait();

                lock (lockObject)
                {
                    job = jobs.Dequeue();
                }

                JobExecute(job);
                FinishedJobs.Enqueue(job);
            }
        }

        public void Add(T job)
        {
            lock (lockObject)
            {
                this.jobs.Enqueue(job);
            }

            semaphore.Release();
        }

        public void Start()
        {
            this.running = true;
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i].Start();
            }
        }

        public void Stop()
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