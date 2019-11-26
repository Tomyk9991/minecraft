using System.Collections.Concurrent;
using UnityEngine;
using System.Threading;
using System;

namespace Core.Chunking.Threading
{
    public class NoiseJobManager : IDisposable
    {
        public static NoiseJobManager NoiseJobManagerUpdaterInstance { get; private set; }
        public bool Running { get; set; }

        private Thread[] threads;

        private ConcurrentQueue<NoiseJob> jobs;

        public NoiseJobManager(bool noiseUpdaterInstance = false)
        {
            if (noiseUpdaterInstance)
                NoiseJobManagerUpdaterInstance = this;

            threads = new Thread[2];
            threads = SystemInfo.processorCount - 2 <= 0
                ? new Thread[1]
                : new Thread[(SystemInfo.processorCount - 2) / 3];

            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(Calculate)
                {
                    IsBackground = true
                };
            }

            jobs = new ConcurrentQueue<NoiseJob>();
        }

        public void Start()
        {
            Running = true;

            for (int i = 0; i < threads.Length; i++)
                threads[i].Start();
        }

        public void Stop() => Running = false;

        public void AddJob(NoiseJob job, bool a = false)
        {
            jobs.Enqueue(job);
        }

        private void Calculate()
        {
            while (Running)
            {
                if (jobs.Count == 0)
                {
                    //TODO wieder auf 10ms stellen
                    System.Threading.Thread.Sleep(100); //Needed, because CPU is overloaded in other case
                    continue;
                }

                if (jobs.TryDequeue(out var job))
                {
                    for (int i = 0; i < job.Column.chunks.Length; i++)
                    {
                        job.Column.chunks[i].GenerateBlocks();
                    }

                    job.Column.State = DrawingState.NoiseReady;
                    Thread.Sleep(10);
                }
            }
        }

        public void Dispose()
        {
            Running = false;
        }
    }
}
