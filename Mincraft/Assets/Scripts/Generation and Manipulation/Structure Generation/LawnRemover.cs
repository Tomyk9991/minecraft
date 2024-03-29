﻿using System.Collections.Generic;
using Core.Builder;
using Core.Builder.Generation;
using Core.Chunks;
using Core.Math;
using UnityEngine;

namespace Core.StructureGeneration
{
    public class LawnRemover : StructureBuilder
    {
        public LawnRemover()
        {
            StructureOrigin = new Queue<(Int3 Origin, Biom Biom)>();
        }
        
        public override void Build(Biom biom, Chunk callingChunk, in Int3 origin)
        {
            Block block = new Block();
            block.SetID(BlockUV.Air);
            
            Int3 checkingPos = origin;
            checkingPos.Y -= 1;

            if (InChunkSpacePlusOne(checkingPos))
            {
                if (callingChunk.Blocks[checkingPos.X, checkingPos.Y, checkingPos.Z].ID == BlockUV.Air ||
                    callingChunk.Blocks[checkingPos.X, checkingPos.Y, checkingPos.Z].ID == BlockUV.None)
                {
                    callingChunk.AddBlock(block, origin);
                }
            }
            
            Int3 neighbouringChunkDirection = new Int3(
                Mathf.FloorToInt(checkingPos.X / 16.0f),
                Mathf.FloorToInt(checkingPos.Y / 16.0f),
                Mathf.FloorToInt(checkingPos.Z / 16.0f)
            );

            checkingPos.X -= 16 * neighbouringChunkDirection.X;
            checkingPos.Y -= 16 * neighbouringChunkDirection.Y;
            checkingPos.Z -= 16 * neighbouringChunkDirection.Z;

            if (ChunkBuffer.InLocalSpace(callingChunk.LocalPosition + neighbouringChunkDirection))
            {
                if (neighbouringChunkDirection != Int3.Zero)
                {
                    Chunk c1 = callingChunk.ChunkNeighbour(neighbouringChunkDirection);
                    if (c1?.Blocks[checkingPos.X, checkingPos.Y, checkingPos.Z].ID == BlockUV.Air)
                    {
                        c1.AddBlock(block, checkingPos);
                    }
                }
            }
        }
    }
}
