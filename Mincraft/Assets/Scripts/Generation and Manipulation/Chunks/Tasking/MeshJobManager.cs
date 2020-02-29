using System;
using Core.Builder;
using Core.Performance.Parallelisation;
using Extensions;
using Unity.Jobs;

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
        
        private bool _calculateShadows;

        public MeshJobManager(int amountThreads, bool chunkUpdaterInstance = false) : base(amountThreads)
        {
            if (chunkUpdaterInstance)
                MeshJobManagerUpdaterInstance = this;
            
            _calculateShadows = WorldSettings.CalculateShadows;
        }

        public override void JobExecute(MeshJob job)
        {
            if (_calculateShadows)
                job.Chunk.CalculateLight();
            
            job.MeshData = MeshBuilder.Combine(job.Chunk);
            job.ColliderData = GreedyMesh.ReduceMesh(job.Chunk);

            job.Chunk.ChunkState = ChunkState.Generated;
        }

        public MeshJob DequeueFinishedJob()
        {
            lock (FinishedJobs)
            {
                return FinishedJobs.RemoveAndGet(0);
            }
        }
        
        public void CreateChunkFromExistingAndAddJob(Chunk chunk)
        {
            MeshJob job = new MeshJob(chunk);
            this.AddJob(job);
        }

        public void Dispose()
        {
            base.Stop();
        }
//        public static MeshJobManager MeshJobManagerUpdaterInstance { get; private set; }
//
//        public int JobsCount => jobs.Count;
//        public int FinishedJobsCount
//        {
//            get
//            {
//                lock(FinishedJobs)
//                    return this.FinishedJobs.Count;
//            }
//        }
//
//        private bool _calculateShadows;
//
//        public MeshJobManager(int amountThreads, bool chunkUpdaterInstance = false) : base(amountThreads)
//        {
//            if (chunkUpdaterInstance)
//                MeshJobManagerUpdaterInstance = this;
//            _calculateShadows = WorldSettings.CalculateShadows;
//        }
//        
//        public void CreateChunkFromExistingAndAddJob(Chunk chunk)
//        {
//            var job = new MeshJob(chunk);
//            AddJob(job);
//        }
//
//        public MeshJob DequeueFinishedJob()
//        {
//            lock (FinishedJobs)
//            {
//                return FinishedJobs.RemoveAndGet(0);
//            }
//        }
//
//        public void Dispose()
//        {
//            base.Stop();
//        }
//
//        public override void JobExecute(MeshJob job)
//        {
//            if (job.Chunk.GetBlocks().All(block => block.Equals(Block.Empty())))
//            {
//                job.MeshData = new MeshData(new List<Vector3>(0), new List<int>(0), new List<int>(0), new List<Vector2>(0), new List<Color>(0));
//                job.ColliderData = new MeshData(new List<Vector3>(0), new List<int>(0), null, null, null);
//                return;
//            }
//
//            if (WorldSettings.CalculateShadows)
//            {
//                job.Chunk.CalculateLight();
//                job.MeshData = MeshBuilder.Combine(job.Chunk);
//                job.ColliderData = GreedyMesh.ReduceMesh(job.Chunk);
//            }
//            else
//            {
//                //Parallelize den Process
//                job.MeshData = MeshBuilder.Combine(job.Chunk);
//                job.ColliderData = GreedyMesh.ReduceMesh(job.Chunk);
//            }
//            
//            job.Chunk.ChunkState = ChunkState.Generated;
//        }
    }
}
