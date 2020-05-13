using System.Collections.Generic;
using Core.Builder;
using Core.Builder.Generation;
using Core.Chunks;
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
            block.SetID((short) biom.treeTrunkBlock);
            Int3 pos = origin;
            //block.Position = origin;


            //Trunk
            for (int j = 1; j < 5; j++)
            {
                pos.Y = origin.Y + j;
                AddBlockToChunk(callingChunk, block, pos);
            }

            Int3 leafOrigin = pos;
            //Leaves
            block.SetID((short) biom.treeLeafBlock);

            for (int y = -1; y < 3; y++)
            {
                pos.Y = leafOrigin.Y + y;
                for (int z = -2; z < 3; z++)
                {
                    pos.Z = leafOrigin.Z + z;
                    for (int x = -2; x < 3; x++)
                    {
                        pos.X = leafOrigin.X + x;
                        AddBlockToChunk(callingChunk, block, pos);
                    }
                }
            }
        }

        private static void AddBlockToChunk(Chunk callingChunk, Block block, Int3 pos)
        {
            if (InLocalSpace(pos))
            {
                callingChunk.AddBlock(block, pos);
            }
            else
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
                    Chunk c = callingChunk.ChunkNeighbour(neighbouringChunkDirection);
                    c.AddBlock(block, pos);
                    
                    //TODO State
                    //c.ChunkState = ChunkState.Dirty;
                    c.ChunkColumn.Dirty = true;
                }
            }
        }
    }
}