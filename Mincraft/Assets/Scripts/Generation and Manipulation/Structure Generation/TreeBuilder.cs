using System.Collections.Concurrent;
using System.Collections.Generic;
using Core.Builder;
using Core.Builder.Generation;
using Core.Chunking;
using Core.Math;
using UnityEngine;
using static Core.Math.MathHelper;

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
            Block block = new Block();
            block.SetID((int) biom.treeTrunkBlock);
            block.Position = origin;


            //Trunk
            for (int j = 1; j < 5; j++)
            {
                block.Position.Y = origin.Y + j;
                AddBlockToChunk(callingChunk, block);
            }

            Int3 leafOrigin = block.Position;
            //Leaves
            block.SetID((int) biom.treeLeafBlock);

            for (int y = -1; y < 3; y++)
            {
                block.Position.Y = leafOrigin.Y + y;
                for (int z = -2; z < 3; z++)
                {
                    block.Position.Z = leafOrigin.Z + z;
                    for (int x = -2; x < 3; x++)
                    {
                        block.Position.X = leafOrigin.X + x;
                        AddBlockToChunk(callingChunk, block);
                    }
                }
            }
        }

        private static void AddBlockToChunk(Chunk callingChunk, Block block)
        {
            if (InLocalSpace(block.Position))
            {
                callingChunk.AddBlock(block);
            }
            else
            {
                Int3 neighbouringChunkDirection = new Int3(
                    Mathf.FloorToInt(block.Position.X / 16.0f),
                    Mathf.FloorToInt(block.Position.Y / 16.0f),
                    Mathf.FloorToInt(block.Position.Z / 16.0f));

                block.Position.X -= 16 * neighbouringChunkDirection.X;
                block.Position.Y -= 16 * neighbouringChunkDirection.Y;
                block.Position.Z -= 16 * neighbouringChunkDirection.Z;

                if (ChunkBuffer.InLocalSpace(callingChunk.LocalPosition + neighbouringChunkDirection))
                {
                    Chunk c = callingChunk.ChunkNeighbour(neighbouringChunkDirection);
                    c.AddBlock(block);
                    c.ChunkState = ChunkState.Dirty;
                    c.ChunkColumn.Dirty = true;
                }
            }
        }
    }
}