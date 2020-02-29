using Core.Performance.Parallelisation;

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
        
//        public override void ExecuteJob()
        public void ExecuteJob()
        {
            foreach (var chunk in this.Column.chunks)
            {
                chunk.GenerateBlocks();
            }
            
            this.Column.State = DrawingState.NoiseReady;
        }
    }
}
