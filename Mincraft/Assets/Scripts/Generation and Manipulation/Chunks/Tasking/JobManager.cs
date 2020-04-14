using System;
using Core.Builder;

namespace Core.Chunking.Threading
{
    public class JobManager : AutoThreadCollectionMany, IDisposable
    {
        public static JobManager JobManagerUpdaterInstance { get; private set; }

        private bool _calcLight = false;
        public JobManager(int amountThreads, bool chunkUpdaterInstance = false) : base(amountThreads)
        {
            if (chunkUpdaterInstance)
                JobManagerUpdaterInstance = this;

            _calcLight = WorldSettings.CalculateShadows;
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
}