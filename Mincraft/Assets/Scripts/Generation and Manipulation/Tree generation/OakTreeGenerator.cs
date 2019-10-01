using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Core.Chunking;
using Core.Chunking.Threading;
using Core.Math;

namespace Core.Builder.Generation
{
    public class OakTreeGenerator : TreeGenerator
    {
        private Int2 minMaxHeight;
        private Int2 minMaxVolume;

        public OakTreeGenerator(Int2 minMaxHeight, Int2 minMaxVolume)
        {
            this.minMaxHeight = minMaxHeight;
            this.minMaxVolume = minMaxVolume;
        }

        public override List<ChunkJob> Generate(Chunk chunk, int x, int y, int z) // xyz kommt in globalspace an
        {
            base.Generate(chunk, x, y, z);

            int height = (int)MathHelper.Map(Mathf.PerlinNoise(x * 0.9f, z * 0.9f), 0f, 1f, minMaxHeight.X + 1, minMaxHeight.Y);
            int volume = (int)MathHelper.Map(Mathf.PerlinNoise(x * 0.9f, z * 0.9f), 0f, 1f, minMaxVolume.X + 1, minMaxVolume.Y);

            Block block = new Block();

            int localX = x - chunk.GlobalPosition.X;
            int localY = y - chunk.GlobalPosition.Y;
            int localZ = z - chunk.GlobalPosition.Z;

            block.ID = (int) BlockUV.Wood;
            (Chunk c, bool ownChunk) result;

            for (int i = localY; i < localY + height; i++) //tree trunk
            {
                block.Position = new Int3(localX, i, localZ);

                result = base.AddBlock(block, chunk);
                base.HandleChunk(result.c, result.ownChunk);
            }

            //block.ID = (int) BlockUV.Leaf;
            //for (int sx = localX - 3; sx <= localX + 3; sx++) // tree top
            //{
            //    for (int sy = localY + height; sy < localY + height + 1; sy++)
            //    {
            //        for (int sz = localZ - 3; sz <= localZ + 3; sz++)
            //        {
            //            block.Position = new Int3(sx, sy, sz);
            //            result = base.AddBlock(block, chunk);
            //            base.HandleChunk(result.c, result.ownChunk);
            //        }
            //    }
            //}

            return chunkJobs.ToList();
        }

        

        private bool LeafSpawn(int x, int y, int z, int volume) 
        {
            float a = Mathf.Abs(-volume / 2 + x);
            float b = Mathf.Abs(-volume / 2 + y);
            float c = Mathf.Abs(-volume / 2 + z);

            float distance = Mathf.Sqrt(a * a + b * b + c * c);
            return (distance < volume / 2f && (Mathf.PerlinNoise(x * 0.9f, y * 0.9f) * volume) > distance);
        }
    }
}
