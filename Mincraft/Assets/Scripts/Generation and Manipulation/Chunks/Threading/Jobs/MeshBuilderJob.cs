using Core.Builder;

namespace Core.Chunks.Threading.Jobs
{
    public struct MeshBuilderJob : IJobCollection<MeshJob>
    {
        public MeshJob Target { get; set; }
        public bool Finished { get; set; }
        public IJobCollectionItem[] OtherJobs { get; set; }

        public MeshBuilderJob(MeshJob item)
        {
            this.OtherJobs = null;
            this.Finished = false;
            this.Target = item;
        }

        public void Execute()
        {
            Target.MeshData = MeshBuilder.Combine(Target.Chunk);
        }
    }
}