using System.Collections.Concurrent;
using Core.Chunks.Threading.Jobs;

namespace Core.Chunks.Threading
{
    public class Pass
    {
        public ConcurrentQueue<JobCollectionItemContainer> Jobs { get; set; }
        public ChunkJobPriority Priority { get; set; }
        public int Count => Jobs.Count;

        public Pass()
        {
            Jobs = new ConcurrentQueue<JobCollectionItemContainer>();
        }

        public void Add(JobCollectionItemContainer item)
        {
            Jobs.Enqueue(item);
        }

        public bool Dequeue(out JobCollectionItemContainer job)
        {
            bool returnValue = Jobs.TryDequeue(out JobCollectionItemContainer j);
            job = j;
            
            return returnValue;
        }
    }
}