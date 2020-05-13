namespace Core.Chunks.Threading.Jobs
{
    // public struct GenerateNoiseJob : IJob<Chunk>, IJobCollectionItem
    public struct GenerateNoiseJob : IJobCollection<MeshJob>
    {
        public bool Finished { get; set; }
        
        public IJobCollectionItem[] OtherJobs { get; set; }
        public MeshJob Target { get; set; }
        
        public GenerateNoiseJob(MeshJob c)
        {
            this.Finished = false;
            this.OtherJobs = null;
            this.Target = c;
        }

        public void Execute() => Target.Chunk.GenerateBlocks();
    }
}
