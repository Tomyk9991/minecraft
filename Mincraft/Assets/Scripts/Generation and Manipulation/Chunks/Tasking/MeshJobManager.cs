using System;
using Core.Builder;
using Core.Performance.Parallelisation;
using Extensions;

namespace Core.Chunking.Threading
{
    public class JobManager : AutoThreadCollectionMany<MeshJob, NoiseJob>, IDisposable
    {
        public static JobManager JobManagerUpdaterInstance { get; private set; }
        private bool _calcLight = false;
        public JobManager(int amountThreads, bool chunkUpdaterInstance = false) : base(amountThreads)
        {
            if (chunkUpdaterInstance)
                JobManagerUpdaterInstance = this;

            _calcLight = WorldSettings.CalculateShadows;
        }

        public override void ExecuteMeshJob(MeshJob job)
        {
            if (_calcLight)
                job.Chunk.CalculateLight();
            
            job.MeshData = MeshBuilder.Combine(job.Chunk);
            job.ColliderData = GreedyMesh.ReduceMesh(job.Chunk);

            job.Chunk.ChunkState = ChunkState.Generated;
        }

        public override void ExecuteNoiseJob(NoiseJob job)
        {
            foreach (var chunk in job.Column.chunks)
            {
                chunk.GenerateBlocks();
            }

            job.Column.State = DrawingState.NoiseReady;
        }
        
        public void CreateChunkFromExistingAndAddJob(Chunk chunk)
        {
            MeshJob job = new MeshJob(chunk);
            this.Add(job);
        }

        public void Dispose()
        {
            base.Stop();
        }
    }
//    public class MeshJobManager : AutoThreadCollection<MeshJob>, IDisposable
//    {
//        public static MeshJobManager MeshJobManagerUpdaterInstance { get; private set; }
//
//        public int FinishedJobsCount
//        {
//            get
//            {
//                lock (FinishedJobs)
//                {
//                    return FinishedJobs.Count;
//                }
//            }
//        }
//        public int JobsCount => jobs.Count;
//        
//        private bool _calculateShadows;
//
//        public MeshJobManager(int amountThreads, bool chunkUpdaterInstance = false) : base(amountThreads)
//        {
//            if (chunkUpdaterInstance)
//                MeshJobManagerUpdaterInstance = this;
//            
//            _calculateShadows = WorldSettings.CalculateShadows;
//        }
//
//        public override void JobExecute(MeshJob job)
//        {
//            if (_calculateShadows)
//                job.Chunk.CalculateLight();
           //            
           //            job.MeshData = MeshBuilder.Combine(job.Chunk);
           //            job.ColliderData = GreedyMesh.ReduceMesh(job.Chunk);
           //
           //            job.Chunk.ChunkState = ChunkState.Generated;
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
//        public void CreateChunkFromExistingAndAddJob(Chunk chunk)
//        {
//            MeshJob job = new MeshJob(chunk);
//            this.AddJob(job);
//        }
//
//        public void Dispose()
//        {
//            base.Stop();
//        }
//    }
}