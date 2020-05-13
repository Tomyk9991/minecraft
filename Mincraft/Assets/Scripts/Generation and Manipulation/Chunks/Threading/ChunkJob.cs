namespace Core.Chunks.Threading
{
    public class ChunkJob
    {
        public ChunkColumn Column { get; set; }
        
        public ChunkJob(ChunkColumn column)
        {
            this.Column = column;
        }
    }
}