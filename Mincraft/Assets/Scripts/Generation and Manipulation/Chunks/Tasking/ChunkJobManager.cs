using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using UnityEngine;

using Core.Builder;
using Core.Managers;
using Core.Saving;
using System.Threading.Tasks;

namespace Core.Chunking.Threading
{
    public class ChunkJobManager : IDisposable
    {
        public static ChunkJobManager ChunkJobManagerUpdaterInstance { get; private set; }
        private ConcurrentQueue<ChunkJob> jobs;
        public ConcurrentQueue<ChunkJob> FinishedJobs { get; private set; }

        public bool Running { get; set; }

        private Thread[] threads;
        private ContextIO<Chunk> chunkLoader;
        private GreedyMesh greedy;

        public int JobsCount => jobs.Count;
        public int FinishedJobsCount => FinishedJobs.Count;

        public ChunkJobManager(bool chunkUpdaterInstance = false)
        {
            if (chunkUpdaterInstance)
                ChunkJobManagerUpdaterInstance = this;

            jobs = new ConcurrentQueue<ChunkJob>();
            greedy = new GreedyMesh();



            FinishedJobs = new ConcurrentQueue<ChunkJob>();
            chunkLoader = new ContextIO<Chunk>(ContextIO.DefaultPath + "/" + GameManager.CurrentWorldName + "/");
            GameManager.AbsolutePath = ContextIO.DefaultPath + "/" + GameManager.CurrentWorldName;

            //What if you got only two cores?
            //threads = SystemInfo.processorCount - 2 <= 0
            //    ? new Thread[1]
            //    : new Thread[SystemInfo.processorCount - 2];
            threads = new Thread[1];

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
                    Thread.Sleep(10); //Needed, because CPU is overloaded in other case
                    continue;
                }

                if (JobsCount > 0)
                {
                    if (!jobs.TryDequeue(out var job)) continue;

                    if (!job.HasBlocks) // Chunk gets build new
                    {
                        job.Chunk.CalculateNeighbours();

                        string path = chunkLoader.Path + job.Chunk.GlobalPosition.ToString() + chunkLoader.FileEnding<Chunk>();

                        if (File.Exists(path))
                        {
                            job.Chunk.LoadChunk(chunkLoader.LoadContext(path));
                        }
                        else
                        {
                            job.HasBlocks = true;
                            job.Chunk.CalculateLight();
                        }
                    }
                    else
                    {
                        job.Chunk.CalculateNeighbours();
                        job.Chunk.CalculateLight();
                    }

                    Task<MeshData> meshData = Task.Run(() => MeshBuilder.Combine(job.Chunk));
                    Task<MeshData> colliderData = Task.Run(() => greedy.ReduceMesh(job.Chunk));

                    Task.WaitAll(meshData, colliderData);


                    job.MeshData = meshData.Result;
                    job.ColliderData = colliderData.Result;

                    job.Chunk.ChunkState = ChunkState.Generated;


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

        public void AddJob(ChunkJob job)
        {
            jobs.Enqueue(job);
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
}
