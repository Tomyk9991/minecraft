using System.Collections.Generic;
using Core.Builder;
using Core.Math;

namespace Core.StructureGeneration
{
    public struct TreeBuilder : IStructureBuilder
    {
        public Block[] treeLocals;

        public TreeBuilder(int globalX, int globalY)
        {
            treeLocals = new[]
            {
                new Block(new Int3(15, 15, 15))
                {
                    ID = (int) BlockUV.Wood
                },
                new Block(new Int3(14, 15, 15))
                {
                    ID = (int) BlockUV.Wood
                }
            };
        }

        public IEnumerable<Block> NextBlock() => treeLocals;
    }
}