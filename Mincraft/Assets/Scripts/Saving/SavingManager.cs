namespace Core.Saving
{
    public abstract class SavingManager
    {
        public abstract void Save(SavingContext context);
        public abstract bool Load(FileIdentifier fileIdentifier, out OutputContext output);
    }

    public interface OutputContext
    {
    }

    public interface SavingContext
    {
    }

    public interface FileIdentifier
    {
    }
}