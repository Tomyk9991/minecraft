using System.Collections.Generic;
using Core.Builder;
using Core.Builder.Generation;
using Core.Chunks;
using Core.Math;
using UnityEngine;

namespace Core.StructureGeneration
{
    public class TreeBuilder : IStructureBuilder
    {
        public Queue<(Int3, Biom)> StructureOrigin { get; set; }

        public TreeBuilder()
        {
            StructureOrigin = new Queue<(Int3, Biom)>();
        }

        public void Build(Biom biom, Chunk callingChunk, in Int3 origin)
        {
            HashSet<Chunk> usedChunks = new HashSet<Chunk>();

            Block block = new Block();
            block.SetID(biom.treeTrunkBlock);
            Int3 pos = origin;

            //Trunk
            for (int j = 1; j < 5; j++)
            {
                pos.Y = origin.Y + j;
                usedChunks.Add(AddBlockToChunk(callingChunk, block, pos));
            }

            Int3 leafOrigin = pos;
            //Leaves
            block.SetID(biom.treeLeafBlock);

            for (int y = -1; y < 3; y++)
            {
                pos.Y = leafOrigin.Y + y;
                for (int z = -2; z < 3; z++)
                {
                    pos.Z = leafOrigin.Z + z;
                    for (int x = -2; x < 3; x++)
                    {
                        pos.X = leafOrigin.X + x;
                        usedChunks.Add(AddBlockToChunk(callingChunk, block, pos));
                    }
                }
            }

            // foreach (Chunk chunk in usedChunks)
            // {
            //     chunk?.FinishUpStructures();
            // }
        }

        private bool InChunkSpacePlusOne(Int3 pos)
            => pos.X >= -1 && pos.X <= 16 &&
               pos.Y >= -1 && pos.Y <= 16 &&
               pos.Z >= -1 && pos.Z <= 16;

        private Chunk AddBlockToChunk(Chunk callingChunk, Block block, Int3 pos)
        {
            //Don't forget about the fact, that the ExtendedArray and the neighbouring Chunk are doing redundant block
            //operations. So you have to add blocks twice. At the calling chunk in the not visible bounds and the
            //neighbouring chunk at the visible bounds
            if (InChunkSpacePlusOne(pos))
            {
                callingChunk.AddBlock(block, pos);
                //callingChunk.AddBlockLater(block, pos);
            }
            
            return _addToOtherChunk();

            Chunk _addToOtherChunk()
            {
                Int3 neighbouringChunkDirection = new Int3(
                    Mathf.FloorToInt(pos.X / 16.0f),
                    Mathf.FloorToInt(pos.Y / 16.0f),
                    Mathf.FloorToInt(pos.Z / 16.0f));
            
                pos.X -= 16 * neighbouringChunkDirection.X;
                pos.Y -= 16 * neighbouringChunkDirection.Y;
                pos.Z -= 16 * neighbouringChunkDirection.Z;
            
                if (ChunkBuffer.InLocalSpace(callingChunk.LocalPosition + neighbouringChunkDirection))
                {
                    if (neighbouringChunkDirection != Int3.Zero)
                    {
                        Chunk c = callingChunk.ChunkNeighbour(neighbouringChunkDirection);
                        c.AddBlock(block, pos);
                        return c;
                    }
                }
            
                return null;
            }
        }
    }
}