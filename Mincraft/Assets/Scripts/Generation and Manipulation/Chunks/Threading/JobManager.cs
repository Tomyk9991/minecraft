using System;
using Core.Managers;

namespace Core.Chunks.Threading
{
    public class JobManager : AutoThreadCollection, IDisposable
    {
        public static JobManager JobManagerUpdaterInstance { get; private set; }

        public JobManager(int amountThreads, bool chunkUpdaterInstance = false) : base(amountThreads)
        {
            if (chunkUpdaterInstance)
                JobManagerUpdaterInstance = this;
        }

        public void Dispose()
        {
            base.Stop();
        }
    }
}