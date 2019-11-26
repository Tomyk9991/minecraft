using System;
using Core.Builder;
using Core.Math;

namespace Core.Chunking.Threading
{
    public class ChunkJob
    {
        public bool Completed { get; set; }
        public Chunk Chunk { get; set; }
        public ChunkColumn Column { get; private set; }
        public MeshData MeshData { get; set; }
        public MeshData ColliderData { get; set; }

        public bool HasBlocks { get; set; }
        public bool RedrawTwice { get; set; }


        public Chunk CreateChunk(Int3 globalPos)
        {
            Chunk chunk = new Chunk
            {
                GlobalPosition = globalPos
            };

            this.HasBlocks = false;
            this.Chunk = chunk;

            return chunk;
        }

        /// <summary>
        /// Creates a new Chunkjob with an existing chunk, so the information is not getting lost
        /// </summary>
        /// <param name="chunkPos">Expects a local space coordinate </param>
        /// <returns></returns>
        public void CreateChunkFromExisting(Chunk chunk, ChunkColumn column)
        {
            this.Chunk = chunk ?? throw new Exception("Chunk is null");
            this.Column = column;
            this.HasBlocks = true;
        }
    }
}
