namespace Core.Chunks.Threading.Jobs
{
    public interface IJobCollectionItem : IJob
    {
        bool Finished { get; set; }
    }
            
    public interface IJobCollection<T> : IJob<T>, IJobCollectionItem
    {
        IJobCollectionItem[] OtherJobs { get; set; }
    }
}
