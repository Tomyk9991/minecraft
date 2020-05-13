using Core.Builder;

namespace Core.Chunks.Threading.Jobs
{
    public struct ReduceColliderJob : IJobCollection<MeshJob>
    {
        public MeshJob Target { get; set; }
        public bool Finished { get; set; }
        public IJobCollectionItem[] OtherJobs { get; set; }

        public ReduceColliderJob(MeshJob job)
        {
            this.OtherJobs = null;
            this.Finished = false;
            this.Target = job;
        }

        public void Execute() => Target.ColliderData = GreedyMesh.ReduceMesh(Target.Chunk);
    }
}
