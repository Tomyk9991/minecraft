using System.Collections.Concurrent;
using System.Security.Cryptography;
using Core.Chunks.Threading.Jobs;

namespace Core.Chunks.Threading
{
    public class Pass
    {
        public ConcurrentQueue<JobCollectionItemContainer> Jobs { get; set; }
        public string Name { get; set; }
        public int Count => Jobs.Count;

        public Pass(string name)
        {
            this.Name = name;
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