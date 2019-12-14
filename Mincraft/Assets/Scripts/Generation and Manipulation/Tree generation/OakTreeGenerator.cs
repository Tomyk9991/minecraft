using System;
using UnityEngine;
using Core.Chunking;
using Core.Math;

namespace Core.Builder.Generation
{
    public class OakTreeGenerator : IStructureBuilder
    {
        private Int2 minMaxHeight;
        private Int2 minMaxVolume;

        public OakTreeGenerator(Int2 minMaxHeight, Int2 minMaxVolume)
        {
            this.minMaxHeight = minMaxHeight;
            this.minMaxVolume = minMaxVolume;
        }

        public void Generate(Chunk chunk, Biom biom, int x, int y, int z) // xyz kommt in localSpace an
        {
            int globalX = x + chunk.GlobalPosition.X;
            int globalZ = z + chunk.GlobalPosition.Z;
            float noiseValue = Mathf.PerlinNoise(globalX * 0.9f, globalZ * 0.9f);
            
            int height = (int) MathHelper.Map(noiseValue, 0f, 1f, minMaxHeight.X, minMaxHeight.Y + 1);
            int volume = (int) MathHelper.Map(noiseValue, 0f, 1f, minMaxVolume.X, minMaxVolume.Y + 1);

            Block block = new Block()
            {
                ID = (int) biom.treeTrunkBlock
            };

            for (int i = y; i < y + height; i++) //tree trunk
            {
                if (i > 15)
                {
                    Chunk c = chunk.CalculateNeighbour(2);
                    block.Position = new Int3(x, i - 16, z);

                    c.AddBlock(block);
                }
                else
                {
                    block.Position = new Int3(x, i, z);
                    chunk.AddBlock(block);
                }
            }

            // tree top sΔ is somewhere between (-volume, 16 + volume) 
            block.ID = (int) biom.treeLeafBlock;
            for (int sx = x - volume; sx <= x + volume; sx++)
            {
                for (int sy = y + height; sy < y + height + volume; sy++)
                {
                    for (int sz = z - volume; sz <= z + volume; sz++)
                    {
                        (Chunk c, int newX, int newY, int newZ) = ChunkForBlock(chunk, sx, sy, sz);

                        block.Position = new Int3(newX, newY, newZ);
                        c.AddBlock(block);
                    }
                }
            }
        }

        private (Chunk c, int dx, int dy, int dz) ChunkForBlock(Chunk root, int x, int y, int z)
        {
//            Forward, // 0
//            Back, // 1
//            Up, // 2
//            Down, // 3
//            Left, // 4
//            Right // 5
            Chunk c = root;
            
            int xModified = x;
            int yModified = y;
            int zModified = z;

            bool otherChunk = false;

            if (x < 0)
            {
                c = c.CalculateNeighbour(4);
                xModified += 16;
                otherChunk = true;
            }
            else if (x > 15)
            {
                c = c.CalculateNeighbour(5);
                xModified -= 16;
                otherChunk = true;
            }

            if (z < 0)
            {
                c = c.CalculateNeighbour(1);
                zModified += 16;
                otherChunk = true;
            }
            else if (z > 15)
            {
                c = c.CalculateNeighbour(0);
                zModified -= 16;
                otherChunk = true;
            }

            if (y < 0)
            {
                c = c.CalculateNeighbour(3);
                yModified += 16;
                otherChunk = true;
            }
            else if (y > 15)
            {
                c = c.CalculateNeighbour(2);
                yModified -= 16;
                otherChunk = true;
            }

            if (otherChunk)
            {
                c.ChunkState = ChunkState.Dirty;
                ChunkBuffer.ModifyChunkColumn(new Int2(c.LocalPosition.X, c.LocalPosition.Z), DrawingState.Dirty);
            }
            
            return (c, xModified, yModified, zModified);
        }
    }
}