using System;

namespace Core.Chunks.Threading
{
    public class ChunkJobManager : AutoThreadCollection, IDisposable
    {
        public static ChunkJobManager ChunkJobManagerUpdaterInstance { get; private set; }
        
        public ChunkJobManager(int amountThreads, bool chunkUpdaterInstance = false) : base(amountThreads)
        {
            if (chunkUpdaterInstance)
                ChunkJobManagerUpdaterInstance = this;
        }

        public void Dispose()
        {
            base.Stop();
        }
    }
}