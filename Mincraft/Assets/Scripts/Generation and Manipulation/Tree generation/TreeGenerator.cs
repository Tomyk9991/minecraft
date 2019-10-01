using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Core.Chunking;
using Core.Chunking.Threading;
using Core.Math;

namespace Core.Builder.Generation
{
    public abstract class TreeGenerator
    {
        //Achte darauf nicht mehrmals den selben Chunk hinzuzufügen
        public HashSet<ChunkJob> chunkJobs = new HashSet<ChunkJob>();
        
        private const int chunkSize = 16;
        
        /// <summary>
        /// Adds an block to the corresponding Chunk automatically
        /// </summary>
        /// <param name="block">The blockposition has to be in local space</param>
        /// <param name="chunk">The chunk, you're setting the initial tree plant</param>
        public (Chunk chunk, bool ownChunk) AddBlock(Block block, Chunk chunk) // blockpos kommt in local space an
        {
            if (block.Position.AnyAttribute(d => d < 0 || d >= chunkSize, out int value))
            {
                Int3 globalBlockPosition = chunk.GlobalPosition + block.Position;

                Int3 newChunkPos = new Int3(MathHelper.MultipleFloor(globalBlockPosition.X, 16),
                    MathHelper.MultipleFloor(globalBlockPosition.Y, 16),
                    MathHelper.MultipleFloor(globalBlockPosition.Z, 16));

                Chunk c = ChunkClusterDictionary.GetChunkAt(newChunkPos);
                block.Position = globalBlockPosition - newChunkPos;
                c?.AddBlock(block);

                return (null, false);
            }
            else
            {
                chunk.AddBlock(block);
                return (chunk, true);
            }
        }

        protected void HandleChunk(Chunk c, bool ownChunk)
        {
            if (c != null && !ownChunk)
            {
                ChunkJob job = new ChunkJob();
                job.CreateChunkFromExisting(c);
                this.chunkJobs.Add(job);
            }

            if (c == null)
            {
                //Debug.Log("Wird sich drum gekümmert. Bald. Versprochen");
                ChunkJob job = new ChunkJob();
                //job.CreateChunk()
            }
        }

        public virtual List<ChunkJob> Generate(Chunk c, int x, int y, int z)
        {
            chunkJobs.Clear();

            return null;
        }
    }
}
