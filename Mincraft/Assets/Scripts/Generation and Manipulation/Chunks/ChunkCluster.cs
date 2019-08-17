using Core.Math;

namespace Core.Chunking
{
    public class ChunkCluster
    {
        public Int3 Position { get; set; }
        private Chunk[] _chunks;
        private static Int3 dimensionSize = new Int3(16, 16, 16);

        public ChunkCluster(Int3 dimensionSize)
        {
            dimensionSize = ChunkCluster.dimensionSize;
            _chunks = new Chunk[dimensionSize.X * dimensionSize.Y * dimensionSize.Z];
        }

        /// <summary>
        /// Expects a Chunk in local space relative to the ChunkCluster 
        /// </summary>
        /// <param name="chunk"></param>
        public void AddChunk(Chunk chunk)
        {
            Int3 pos = chunk.LocalPosition;
            _chunks[FlattenIdx(pos.X, pos.Y, pos.Z)] = chunk;
        }

        public Chunk GetChunk(int x, int y, int z)
            => _chunks[FlattenIdx(x, y, z)];
        
        public Chunk GetChunk(Int3 pos)
            => GetChunk(pos.X, pos.Y, pos.Z);

        private int FlattenIdx(int x, int y, int z)
            => x + dimensionSize.X * (y + dimensionSize.Z * z);

        public override string ToString()
            => Position.ToString();
    }
}
