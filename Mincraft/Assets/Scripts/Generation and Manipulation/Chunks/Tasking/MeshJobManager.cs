using System;
using System.IO;

using Core.Builder;
using Core.Managers;
using Core.Saving;
using System.Threading.Tasks;
using Core.Performance.Parallelisation;
using Extensions;

namespace Core.Chunking.Threading
{
    public class MeshJobManager : AutoThreadCollection<MeshJob>, IDisposable
    {
        public static MeshJobManager MeshJobManagerUpdaterInstance { get; private set; }

        public int FinishedJobsCount
        {
            get
            {
                lock (FinishedJobs)
                {
                    return FinishedJobs.Count;
                }
            }
        }
        public int JobsCount => jobs.Count;
        
        private ContextIO<Chunk> chunkLoader;
        private GreedyMesh greedy;
        private bool _calculateShadows;

        public MeshJobManager(int amountThreads, bool chunkUpdaterInstance = false) : base(amountThreads)
        {
            if (chunkUpdaterInstance)
                MeshJobManagerUpdaterInstance = this;
            
            greedy = new GreedyMesh();
            _calculateShadows = WorldSettings.CalculateShadows;

            
            chunkLoader = new ContextIO<Chunk>(ContextIO.DefaultPath + "/" + GameManager.CurrentWorldName + "/");
            GameManager.AbsolutePath = ContextIO.DefaultPath + "/" + GameManager.CurrentWorldName;
        }

        public override void JobExecute(MeshJob job)
        {
            if (!job.HasBlocks) // Chunk gets build new
            {
                //job.Chunk.CalculateNeighbour();

                string path = chunkLoader.Path + job.Chunk.GlobalPosition.ToString() + chunkLoader.FileEnding<Chunk>();
                
                if (File.Exists(path))
                {
                    job.Chunk.LoadChunk(chunkLoader.LoadContext(path));
                }
                else
                {
//                    job.Chunk.GenerateAdditionalBlocks();
//                    job.HasBlocks = true;
                    if (_calculateShadows)
                        job.Chunk.CalculateLight();
                }
            }
            else
            {
//                job.Chunk.GenerateAdditionalBlocks();
//                job.Chunk.CalculateNeighbour();
//
//                if (_calculateShadows)
//                    job.Chunk.CalculateLight();
            }

            Task<MeshData>  meshData = Task.Run(() => MeshBuilder.Combine(job.Chunk));
            Task<MeshData>  colliderData = Task.Run(() => greedy.ReduceMesh(job.Chunk));

            Task.WaitAll(meshData, colliderData);


            job.MeshData = meshData.Result;
            job.ColliderData = colliderData.Result;

            job.Chunk.ChunkState = ChunkState.Generated;


            job.Completed = true;
        }

        public MeshJob DequeueFinishedJob()
        {
            lock (FinishedJobs)
            {
                return FinishedJobs.RemoveAndGet(0);
            }
//            if (FinishedJobs.TryDequeue(out var result))
//            {
//                return result;
//            }

            return null;
        }

        public void Dispose()
        {
            base.Stop();
        }
    }
}
