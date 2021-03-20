using System.Collections.Generic;
using Core.Builder;
using Core.Builder.Generation;
using Core.Chunks;
using Core.Math;
using UnityEngine;

namespace Core.StructureGeneration
{
    public abstract class StructureBuilder
    {
        public Queue<(Int3 Origin, Biom Biom)> StructureOrigin { get; set; }

        protected StructureBuilder()
        {
            this.StructureOrigin = new Queue<(Int3 Origin, Biom Biom)>();
        }
        
        public abstract void Build(Biom biom, Chunk callingChunk, in Int3 origin);

        protected bool InChunkSpacePlusOne(Int3 pos)
            => pos.X >= -1 && pos.X <= 16 &&
               pos.Y >= -1 && pos.Y <= 16 &&
               pos.Z >= -1 && pos.Z <= 16;

        protected void AddBlockToChunk(Chunk callingChunk, Block block, Int3 pos)
        {
            //Don't forget about the fact, that the ExtendedArray and the neighbouring Chunk are doing redundant block
            //operations. So you have to add blocks twice. At the calling chunk in the not visible bounds and the
            //neighbouring chunk at the visible bounds
            if (InChunkSpacePlusOne(pos))
            {
                if (callingChunk.Blocks[pos.X, pos.Y, pos.Z].ID == BlockUV.Air)
                {
                    callingChunk.AddBlock(block, pos);
                }
            }

            Int3 neighbouringChunkDirection = new Int3(
                Mathf.FloorToInt(pos.X / 16.0f),
                Mathf.FloorToInt(pos.Y / 16.0f),
                Mathf.FloorToInt(pos.Z / 16.0f)
            );

            pos.X -= 16 * neighbouringChunkDirection.X;
            pos.Y -= 16 * neighbouringChunkDirection.Y;
            pos.Z -= 16 * neighbouringChunkDirection.Z;

            if (ChunkBuffer.InLocalSpace(callingChunk.LocalPosition + neighbouringChunkDirection))
            {
                if (neighbouringChunkDirection != Int3.Zero)
                {
                    Chunk c1 = callingChunk.ChunkNeighbour(neighbouringChunkDirection);
                    if (c1?.Blocks[pos.X, pos.Y, pos.Z].ID == BlockUV.Air)
                    {
                        c1.AddBlock(block, pos);
                    }
                }
            }
        }
    }
}