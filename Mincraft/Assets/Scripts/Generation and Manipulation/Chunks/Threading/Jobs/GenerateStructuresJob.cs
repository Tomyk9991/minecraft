using Core.Chunks.Threading;
using Core.Chunks.Threading.Jobs;

public class GenerateStructuresJob : IJobCollection<MeshJob>
{
    public bool Finished { get; set; }
    
    public IJobCollectionItem[] OtherJobs { get; set; }
    public MeshJob Target { get; set; }

    public GenerateStructuresJob(MeshJob job)
    {
        this.Finished = false;
        this.OtherJobs = null;
        this.Target = job;
    }
    
    public void Execute()
    {
        Target.Chunk.GenerateStructures();
    }

}
