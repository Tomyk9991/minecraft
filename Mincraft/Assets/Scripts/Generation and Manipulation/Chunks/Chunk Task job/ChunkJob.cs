using Core.Builder;
using Core.Math;

namespace Core.Chunking.Threading
{
    public class ChunkJob
    {
        public bool Completed { get; set; }
        public Chunk Chunk { get; set; }
        public MeshData MeshData { get; set; }
        public MeshData ColliderData { get; set; }

        public bool HasBlocks { get; set; }

        /// <summary>
        /// Creates internally a new Chunk with an empty block array
        /// </summary>
        /// <param name="chunkPos">Expects a local space coordinate </param>
        /// <returns></returns>
        public Chunk CreateChunk(Int3 chunkPos, Int3 chunkClusterPosition)
        {
            Chunk chunk = new Chunk
            {
                LocalPosition = chunkPos,
                GlobalPosition = chunkClusterPosition + (chunkPos * 16)
            };
            
            this.HasBlocks = false;
            this.Chunk = chunk;

            return this.Chunk;
        }
    }
}
