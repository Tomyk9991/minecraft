namespace Core.Chunking.Threading
{
    public class NoiseJob
    {
        public NoiseJob() { }
        public NoiseJob(ChunkColumn column)
        {
            this.Column = column;
        }

        public ChunkColumn Column { get; set; }
    }
}
