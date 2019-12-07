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
            int height = (int)MathHelper.Map(Mathf.PerlinNoise(x * 0.9f, z * 0.9f), 0f, 1f, minMaxHeight.X + 1, minMaxHeight.Y);
            int volume = (int)MathHelper.Map(Mathf.PerlinNoise(x * 0.9f, z * 0.9f), 0f, 1f, minMaxVolume.X + 1, minMaxVolume.Y);

            Block block = new Block()
            {
                ID = (int)biom.treeTrunkBlock
            };

    //        private static Int3[] directions =
    //        {
    //            Int3.Forward, // 0
    //            Int3.Back, // 1
    //            Int3.Up, // 2
    //            Int3.Down, // 3
    //            Int3.Left, // 4
    //            Int3.Right // 5
    //        };

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

//            block.ID = (int) biom.treeLeafBlock;
//            for (int sx = x - 3; sx <= x + 3; sx++) // tree top
//            {
//                for (int sy = y + height; sy < y + height + 1; sy++)
//                {
//                    for (int sz = z - 3; sz <= z + 3; sz++)
//                    {
//                        (Chunk c, int newX, int newY, int newZ) = ChunkForBlock(chunk, sx, sy, sz);
//                        if (newY < 0 || newY > 15 || newZ < 0 || newZ > 15)
//                        {
//                            continue;
//                        }
//
//                        try
//                        {
//                            block.Position = new Int3(newX, newY, newZ);
//                        }
//                        catch (Exception e)
//                        {
//                            Debug.Log(newX + " " + newY + " " + newZ);
//                            throw;
//                        }
//
//                        try
//                        {
//
//                            c.AddBlock(block);
//                        }
//                        catch (Exception e)
//                        {
//                            Debug.Log(newX + " " + newY + " " + newZ);                       
//                        }
//                    }
//                }
//            }
        }

        private (Chunk c, int dx, int dy, int dz) ChunkForBlock(Chunk root, int x, int y, int z)
        {
            if (x < 0)
                return (root.CalculateNeighbour(4), x + 16, y, z);
            if (x > 15)
                return (root.CalculateNeighbour(5), x - 16, y, z);
//            if (z < 0)
//                return (root.CalculateNeighbour(1), x, y, z + 16);
//            if (z > 15)
//                return (root.CalculateNeighbour(0), x, y, z - 16);
//            if (y < 0)
//                return (root.CalculateNeighbour(3), x, y + 16, z);
//            if (y > 15)
//                return (root.CalculateNeighbour(2), x, y - 16, z);

            return (root, x, y, z);
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
