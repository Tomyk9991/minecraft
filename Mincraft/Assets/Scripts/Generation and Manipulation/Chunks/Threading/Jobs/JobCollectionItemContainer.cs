namespace Core.Chunks.Threading.Jobs
{
    public struct JobCollectionItemContainer
    {
        public IJobCollection<MeshJob>[] SequentialCollection;
        public IJobCollection<MeshJob>[] ParallelizedCollection;
        private Order order;

        private int currentIndex;

        public JobCollectionItemContainer(int sequentialLength, int parallelizedLength, Order order)
        {
            this.SequentialCollection = new IJobCollection<MeshJob>[sequentialLength];
            this.ParallelizedCollection = new IJobCollection<MeshJob>[parallelizedLength];
            this.order = order;
            this.currentIndex = 0;
        }

        public void RunSequentially(params IJobCollection<MeshJob>[] items)
        {
            SequentialCollection = items;
        }

        public void RunParallelized(params IJobCollection<MeshJob>[] items)
        {
            ParallelizedCollection = items;
        }

        public enum Order
        {
            RunSequentialFirst
        }
    }
}