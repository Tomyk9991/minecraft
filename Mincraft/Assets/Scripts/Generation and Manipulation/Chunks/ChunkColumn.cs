using Core.Math;

namespace Core.Chunking
{
    public class ChunkColumn
    {
        private DrawingState state;
        public Int2 GlobalPosition { get; }
        /// <summary>
        /// Discribes the local position, which depends on the global player position 
        /// </summary>
        public Int2 LocalPosition { get; private set; }
        public DrawingState State
        {
            get
            {
                lock (_drawingstateMutex)
                {
                    return state;
                }
            }
            set
            {
                lock (_drawingstateMutex)
                {
                    this.state = value;
                }
            }
        }

        private object _mutex = new object();
        private object _drawingstateMutex = new object();

        public Chunk[] chunks;

        public ChunkColumn(Int2 globalPosition, Int2 localPosition, int minYHeight, int maxYHeight)
        {
            this.GlobalPosition = globalPosition;
            this.LocalPosition = localPosition;
            chunks = new Chunk[System.Math.Abs(minYHeight / 16) + System.Math.Abs(maxYHeight / 16)];
        }

        public void UpdateLocalPosition(int x, int z)
        {
            this.LocalPosition = new Int2(x, z);
        }

        public ChunkColumn[] Neighbours()
        {
            return new ChunkColumn[]
            {
                //Left
                ChunkBuffer.GetColumn(this.LocalPosition.X - 1, this.LocalPosition.Y),
                //Right
                ChunkBuffer.GetColumn(this.LocalPosition.X + 1, this.LocalPosition.Y),
                //Forward
                ChunkBuffer.GetColumn(this.LocalPosition.X, this.LocalPosition.Y + 1),
                //Back
                ChunkBuffer.GetColumn(this.LocalPosition.X, this.LocalPosition.Y - 1),
            };
        }

        public int ChunksLength
        {
            get
            {
                lock (_mutex)
                {
                    return chunks.Length;
                }
            }
        }

        public Chunk this[int index]
        {
            get
            {
                lock (_mutex)
                {
                    return chunks[index];
                }
            }
            set
            {
                lock (_mutex)
                {
                    chunks[index] = value;
                }
            }
        }
    }

    public enum DrawingState
    {
        None = 0,
        Drawn = 1,
        NoiseReady = 2,
        InNoiseQueue = 3
    }
}