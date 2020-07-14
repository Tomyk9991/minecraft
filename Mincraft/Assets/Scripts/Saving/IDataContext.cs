namespace Core.Saving
{
    public interface IDataContext
    {
        DataContextFinder Finder { get; }
    }
}