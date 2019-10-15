﻿using System;
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

        public int Counter = 0;

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

            return chunk;
        }

        public Chunk CreateChunk(Int3 globalPos, Int3 localPos, ChunkColumn column)
        {
            Chunk chunk = new Chunk
            {
                GlobalPosition = globalPos,
                LocalPosition = localPos
            };

            this.Column = column;

            this.HasBlocks = false;
            this.Chunk = chunk;

            return chunk;
        }

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
        public void CreateChunkFromExisting(Chunk chunk)
        {
            this.Chunk = chunk ?? throw new Exception("Chunk is null");
            this.HasBlocks = true;
        }
    }
}
